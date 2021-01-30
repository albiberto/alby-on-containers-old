using System.Threading;
using System.Threading.Tasks;
using AlbyOnContainers.Messages;

namespace IdentityServer.Abstract
{
    public interface IMessagePublisher
    {
        Task Send(CancellationToken cancellationToken, EmailMessage message);
    }
}