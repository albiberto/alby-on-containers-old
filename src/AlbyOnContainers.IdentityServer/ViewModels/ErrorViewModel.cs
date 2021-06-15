using IdentityServer4.Models;

namespace IdentityServer.ViewModels
{
    public record ErrorViewModel
    {
        public ErrorViewModel() : this("Generic Error")
        {
        }
        
        public ErrorViewModel(string error) : this(new ErrorMessage { Error = error})
        {
        }

        public ErrorViewModel(ErrorMessage? error)
        {
            Error = error;
        }

        public ErrorMessage? Error { get; }
    }
}