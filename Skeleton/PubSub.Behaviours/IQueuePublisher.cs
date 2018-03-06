namespace PubSub.Behaviours
{
    public interface IQueuePublisher<T>
    {
        void SendMessage(T message);
    }
}
