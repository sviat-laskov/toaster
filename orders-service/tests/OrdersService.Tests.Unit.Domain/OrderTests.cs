using FluentAssertions;
using NUnit.Framework;
using OrdersService.Domain.Events;
using OrdersService.Domain.Events.Base;
using OrdersService.Domain.Models;

namespace OrdersService.Domain.UnitTests;

public class OrderTests
{
    [Test]
    public void OrderShouldGenerateOrderEventsWhenOrderFlowHappens()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        string orderUserNote = "Don't call me";
        var buyerId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var expectedOrderEvents = new OrderEvent[] { new OrderCreatedEvent(orderId, orderUserNote, buyerId), new OrderSubscriptionAddedEvent(orderId, buyerId), new OrderConfirmedEvent(orderId, adminId) };

        // Act
        var order = new Order(orderId, orderUserNote, buyerId);
        order.Confirm(adminId);

        // Assert
        IReadOnlyCollection<OrderEvent> actualEvents = order.DequeueUncommittedEvents();
        actualEvents.Should().BeEquivalentTo(
            expectedOrderEvents,
            config => config
                .Excluding(orderEvent => orderEvent.Id)
                .Excluding(orderEvent => orderEvent.OccurredAt));
    }
}