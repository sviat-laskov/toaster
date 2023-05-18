using EasyNetQ;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using PushUpdatesHub.Common.Constants;
using PushUpdatesHub.IntegrationMessages;
using PushUpdatesHub.Tests.Integration.SystemUnderTest;
using PushUpdatesHub.Tests.Integration.SystemUnderTest.Models;

namespace PushUpdatesHub.Tests.Integration
{
    public class WebSocketAndAmqpIntegrationTests
    {
        private PushUpdatesHubApplicationFactory _pushUpdatesHubApplication = null!;

        [OneTimeSetUp]
        public void Setup() => _pushUpdatesHubApplication = new PushUpdatesHubApplicationFactory();

        [OneTimeTearDown]
        public void Teardown() => _pushUpdatesHubApplication.DisposeAsync().GetAwaiter().GetResult();

        [Test]
        public async Task ShouldRetrievePushUpdateViaWebsocketWhenIntegrationMessageIsPublished()
        {
            // Arrange
            TestUser user = TestConstants.User1;
            var pushUpdateRetrievalCompletionSource = new TaskCompletionSource<(string pushUpdateCode, TestPushUpdatePayload payload, Guid initiatedBy, DateTimeOffset occurredAt)>();

            await using HubConnection connection = _pushUpdatesHubApplication.CreatePushUpdatesHubConnection(user);
            connection.On<string, TestPushUpdatePayload, Guid, DateTimeOffset>(
                PushUpdateConstants.ReceiveMethodName,
                (pushUpdateCode, payload, initiatedBy, occurredAt) => pushUpdateRetrievalCompletionSource.SetResult((pushUpdateCode, payload, initiatedBy, occurredAt)));
            await connection.StartAsync();

            // Prepare integration message
            var bus = _pushUpdatesHubApplication.Services.GetRequiredService<IBus>();
            var pushUpdateIntegrationMessage = new PushUpdateIntegrationMessage
            {
                Code = "order-placed",
                InitiatedByUserId = Guid.NewGuid(),
                Payload = new TestPushUpdatePayload("Some very useful info."),
                OccurredAt = DateTimeOffset.Now,
                SubscriberIds = new[] { user.Id }
            };

            // Act
            await bus.PubSub.PublishAsync(pushUpdateIntegrationMessage);

            (string pushUpdateCode, TestPushUpdatePayload payload, Guid initiatedBy, DateTimeOffset occurredAt) pushUpdate = await pushUpdateRetrievalCompletionSource.Task;

            // Assert
            pushUpdate.pushUpdateCode.Should().Be(pushUpdateIntegrationMessage.Code);
            pushUpdate.initiatedBy.Should().Be(pushUpdateIntegrationMessage.InitiatedByUserId);
            pushUpdate.payload.Should().BeEquivalentTo(pushUpdateIntegrationMessage.Payload);
            pushUpdate.occurredAt.Should().Be(pushUpdateIntegrationMessage.OccurredAt);
        }
    }
}