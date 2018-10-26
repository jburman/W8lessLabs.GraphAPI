using System.Collections.Generic;

namespace W8lessLabs.GraphAPI
{
    public class AuthConfig
    {
        public AuthConfig(string clientId, string[] scopes)
        {
            ClientId = clientId;
            Scopes = new List<string>(scopes);
        }

        public string ClientId { get; private set; }
        public IReadOnlyList<string> Scopes { get; private set; }
    }
}
