namespace Oswos.Server
{
    public interface IStreamRouter
    {
        void Route(ISocketConnection connection);
    }
}