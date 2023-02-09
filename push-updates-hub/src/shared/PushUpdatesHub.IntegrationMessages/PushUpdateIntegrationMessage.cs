namespace PushUpdatesHub.IntegrationMessages
{
    public class PushUpdateIntegrationMessage
    {
        /// <summary>
        ///     Push update code.
        /// </summary>
        /// <example>order-placed</example>
        public string Code { get; init; } = null!;

        /// <summary>
        ///     Custom data to make push update more understandable for end-user.
        /// </summary>
        public object? Payload { get; init; } = null!;

        /// <summary>
        ///     Id of user, that initiated push update. May be admin.
        /// </summary>
        public Guid InitiatedByUserId { get; init; }

        /// <summary>
        ///     Timestamp of push update.
        /// </summary>
        public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.Now;

        /// <summary>
        ///     User ids, that should receive push update.
        /// </summary>
        public IEnumerable<Guid> SubscriberIds { get; init; } = Enumerable.Empty<Guid>();
    }
}