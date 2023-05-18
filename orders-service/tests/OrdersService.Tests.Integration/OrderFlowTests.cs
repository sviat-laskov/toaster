using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using EasyNetQ;
using EventStore.Client;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using NUnit.Framework;
using OrdersService.Api.ViewModels;
using OrdersService.Application.PushUpdatePayloads;
using OrdersService.Domain.Events;
using OrdersService.Domain.Events.Base;
using OrdersService.Tests.Integration.SystemUnderTest;
using PushUpdatesHub.IntegrationMessages;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace OrdersService.Tests.Integration;

public class OrderFlowTests
{
    private OrdersServiceApplicationFactory _ordersServiceApplication = null!;

    [OneTimeSetUp]
    public void Setup() => _ordersServiceApplication = new OrdersServiceApplicationFactory();

    [OneTimeTearDown]
    public void Teardown() => _ordersServiceApplication.DisposeAsync().GetAwaiter().GetResult();

    [Test]
    public async Task PushUpdateIntegrationMessageWithOrderConfirmedPushUpdatePayloadShouldBePublishedWhenOrderIsConfirmed()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var pushUpdateRetrievalCompletionSource = new TaskCompletionSource<PushUpdateIntegrationMessage>();

        using HttpClient ordersServiceClient = _ordersServiceApplication.CreateDefaultClient();

        Task<string> user1AccessToken = _ordersServiceApplication.KeycloakTestContainer.GetAccessTokenViaDirectAccessGrant(TestConstants.User1);
        Task<string> admin1AccessToken = _ordersServiceApplication.KeycloakTestContainer.GetAccessTokenViaDirectAccessGrant(TestConstants.Admin1);
        Task pushUpdateIntegrationMessageSubscriptionTask = _ordersServiceApplication.Services.GetRequiredService<IBus>()
            .PubSub.SubscribeAsync<PushUpdateIntegrationMessage>("test", pushUpdateIntegrationMessage => pushUpdateRetrievalCompletionSource.SetResult(pushUpdateIntegrationMessage));

        var orderCreationHttpRequest = new HttpRequestMessage(HttpMethod.Post, "orders")
        {
            Content = JsonContent.Create(new OrderCreationVM
            {
                OrderId = orderId,
                UserNote = "Don't call me, please"
            })
        };
        orderCreationHttpRequest.Headers.Add(HeaderNames.Authorization, $"{JwtBearerDefaults.AuthenticationScheme} {await user1AccessToken}");

        var ordersRetrievalHttpRequest = new HttpRequestMessage(HttpMethod.Get, "orders");
        ordersRetrievalHttpRequest.Headers.Add(HeaderNames.Authorization, $"{JwtBearerDefaults.AuthenticationScheme} {await admin1AccessToken}");

        var orderConfirmationHttpRequest = new HttpRequestMessage(HttpMethod.Post, $"orders/{orderId}");
        orderConfirmationHttpRequest.Headers.Add(HeaderNames.Authorization, $"{JwtBearerDefaults.AuthenticationScheme} {await admin1AccessToken}");

        await pushUpdateIntegrationMessageSubscriptionTask;

        // Act
        var orderCreationHttpResponse = await ordersServiceClient.SendAsync(orderCreationHttpRequest); // Create order.
        orderCreationHttpResponse.EnsureSuccessStatusCode();
        var orderConfirmationHttpResponse = await ordersServiceClient.SendAsync(orderConfirmationHttpRequest); // Confirm order.
        orderConfirmationHttpResponse.EnsureSuccessStatusCode();

        // Assert
        PushUpdateIntegrationMessage pushUpdateIntegrationMessage = await pushUpdateRetrievalCompletionSource.Task;
        pushUpdateIntegrationMessage.SubscriberIds.Should().Contain(TestConstants.User1.Id);
        pushUpdateIntegrationMessage.Code.Should().Be(EventTypeTitles.OrderConfirmed);
        pushUpdateIntegrationMessage.InitiatedByUserId.Should().Be(TestConstants.Admin1.Id);
        var orderConfirmedPushUpdatePayload = pushUpdateIntegrationMessage.Payload.Should().BeOfType<JsonElement>().Which.Deserialize<OrderConfirmedPushUpdatePayload>(new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        orderConfirmedPushUpdatePayload.OrderId.Should().Be(orderId);
    }

    [Test]
    public async Task AllOrderEventsShouldBeSavedToEventStoreAndElasticSearch()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        string userNote = "Don't call me, please";
        var expectedEventTypeTitles = new List<string>
        {
            EventTypeTitles.OrderCreated,
            EventTypeTitles.OrderSubscriptionAdded,
            EventTypeTitles.OrderConfirmed
        };
        var actualEventTypeTitles = new List<string>();
        var orderConfirmationCompletionSource = new TaskCompletionSource();

        using HttpClient ordersServiceClient = _ordersServiceApplication.CreateDefaultClient();

        Task<string> user1AccessToken = _ordersServiceApplication.KeycloakTestContainer.GetAccessTokenViaDirectAccessGrant(TestConstants.User1);
        Task<string> admin1AccessToken = _ordersServiceApplication.KeycloakTestContainer.GetAccessTokenViaDirectAccessGrant(TestConstants.Admin1);

        var eventStorePersistentSubscriptionsClient = _ordersServiceApplication.Services.GetRequiredService<EventStorePersistentSubscriptionsClient>();
        await eventStorePersistentSubscriptionsClient.CreateToAllAsync(
            "test-order-group",
            StreamFilter.Prefix("order"),
            new PersistentSubscriptionSettings(),
            TimeSpan.MaxValue);
        await eventStorePersistentSubscriptionsClient.SubscribeToAllAsync(
            "test-order-group",
            (persistentSubscription, resolvedEvent, _, _) =>
            {
                byte[] eventDataBytes = resolvedEvent.Event.Data.ToArray();
                string eventDataJson = Encoding.UTF8.GetString(eventDataBytes);
                string orderEventTypeTitle = JsonSerializer
                    .Deserialize<JsonElement>(eventDataJson)
                    .GetProperty(nameof(OrderEvent.TypeTitle))
                    .GetString()!;
                actualEventTypeTitles.Add(orderEventTypeTitle);
                if (orderEventTypeTitle == EventTypeTitles.OrderConfirmed) orderConfirmationCompletionSource.SetResult();

                return persistentSubscription.Ack(resolvedEvent);
            });

        var orderCreationHttpRequest = new HttpRequestMessage(HttpMethod.Post, "orders")
        {
            Content = JsonContent.Create(new OrderCreationVM
            {
                OrderId = orderId,
                UserNote = userNote
            })
        };
        orderCreationHttpRequest.Headers.Add(HeaderNames.Authorization, $"{JwtBearerDefaults.AuthenticationScheme} {await user1AccessToken}");

        var ordersRetrievalHttpRequest = new HttpRequestMessage(HttpMethod.Get, "orders");
        ordersRetrievalHttpRequest.Headers.Add(HeaderNames.Authorization, $"{JwtBearerDefaults.AuthenticationScheme} {await user1AccessToken}");

        var orderConfirmationHttpRequest = new HttpRequestMessage(HttpMethod.Post, $"orders/{orderId}");
        orderConfirmationHttpRequest.Headers.Add(HeaderNames.Authorization, $"{JwtBearerDefaults.AuthenticationScheme} {await admin1AccessToken}");

        // Act
        var orderCreationHttpResponse = await ordersServiceClient.SendAsync(orderCreationHttpRequest); // Create order.
        orderCreationHttpResponse.EnsureSuccessStatusCode();
        var orderConfirmationHttpResponse = await ordersServiceClient.SendAsync(orderConfirmationHttpRequest); // Confirm order.
        orderConfirmationHttpResponse.EnsureSuccessStatusCode();

        // Assert
        await orderConfirmationCompletionSource.Task;
        actualEventTypeTitles.Should().BeEquivalentTo(expectedEventTypeTitles);

        await Task.Delay(TimeSpan.FromSeconds(3)); // Dirty hack to let ElasticSearch update document.

        HttpResponseMessage ordersRetrievalHttpResponse = await ordersServiceClient.SendAsync(ordersRetrievalHttpRequest); // Get order from search index.
        var orderVMs = await ordersRetrievalHttpResponse.Content.ReadFromJsonAsync<IEnumerable<OrderVM>>();
        OrderVM? orderVM = orderVMs.Should().ContainSingle().Which;
        orderVM.Id.Should().Be(orderId);
        orderVM.IsUserSubscribed.Should().BeTrue("Customer, created order, is subscribed to it.");
        orderVM.UserNote.Should().Be(userNote);
        orderVM.Version.Should().Be((ulong)expectedEventTypeTitles.Count - 1); // One less, because CreatedEvent makes version 0.
    }
}