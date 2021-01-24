using System.Threading.Tasks;

namespace IdentityServer.Abstract
{
    public interface IPublisher
    {
        Task SendAsync<T>(T message);
    }
}