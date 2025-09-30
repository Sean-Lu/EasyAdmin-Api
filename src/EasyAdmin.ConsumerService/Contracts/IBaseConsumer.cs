namespace EasyAdmin.ConsumerService.Contracts;

public interface IBaseConsumer
{
    bool IsStarted { get; }

    void Start();
    void Stop();
}