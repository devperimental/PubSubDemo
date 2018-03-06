using PubSub.Behaviours;
using System;
using System.Threading;

namespace PubSub.Azure
{
    public class AzureQueueSubscriber<T> : IQueueSubscriber<T>
    {
        public void Process(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
