using System;

namespace Dinucci.Salesforce.Client.Auth
{
    public interface IAuthInfo
    {
        string Id { get; }
        DateTime IssuedAt { get; }
        string Url { get; }
        string Signature { get; }
        string Token { get; }
        string RefreshToken { get; }
    }
}