namespace Dinucci.Salesforce.Client.Auth
{
    public interface IAuthenticator
    {
        IAuthInfo Authenticate();
        IAuthInfo GetAuthInfo();
    }
}