using System.Threading;

namespace PubSub.Behaviours
{
    public interface IQueueSubscriber<T>
    {
        void Process(CancellationToken cancellationToken);
    }
}
