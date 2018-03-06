using System.Threading;
using System.Threading.Tasks;

namespace PubSub.Behaviours
{
    public interface IQueueSubscriber<T>
    {
        Task Process();
    }
}
