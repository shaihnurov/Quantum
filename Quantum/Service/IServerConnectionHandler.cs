using System.Threading.Tasks;

namespace Quantum.Service;

public interface IServerConnectionHandler
{
    public Task ConnectServer();
}