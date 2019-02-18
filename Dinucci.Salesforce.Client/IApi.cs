using Dinucci.Salesforce.Client.Auth;

namespace Dinucci.Salesforce.Client
{
    public interface IApi
    {
        IAuthenticator Authenticator { get; }
    }
}