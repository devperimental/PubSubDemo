using System.Threading.Tasks;

namespace PubSub.Behaviours
{
    public interface IQueuePublisher<T>
    {
        Task SendMessage(T message);
    }
}
