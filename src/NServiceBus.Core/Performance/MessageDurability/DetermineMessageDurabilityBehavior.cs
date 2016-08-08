namespace NServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using DeliveryConstraints;
    using Pipeline;

    class DetermineMessageDurabilityBehavior : Behavior<IOutgoingLogicalMessageContext>
    {
        public DetermineMessageDurabilityBehavior(Func<Type, bool> convention)
        {
            this.convention = convention;
            durabilityCache = new ConcurrentDictionary<Type, bool>();
        }

        public override Task Invoke(IOutgoingLogicalMessageContext context, Func<Task> next)
        {
            if (durabilityCache.GetOrAdd(context.Message.MessageType, t => convention(t)))
            {
                context.Extensions.AddDeliveryConstraint(new NonDurableDelivery());

                context.Headers[Headers.NonDurableMessage] = true.ToString();
            }

            return next();
        }

        Func<Type, bool> convention;

        ConcurrentDictionary<Type, bool> durabilityCache;
    }
}