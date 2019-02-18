using System;

namespace W8lessLabs.GraphAPI
{
    public class GraphTokenResult
    {
        public static readonly GraphTokenResult Failed = new GraphTokenResult(false);

        internal GraphTokenResult(bool success)
        {
            Success = success;
        }

        public GraphTokenResult(bool success, string accessToken, DateTimeOffset expires)
        {
            Success = success;
            AccessToken = accessToken;
            Expires = expires;
        }

        public bool Success { get; private set; }
        public string AccessToken { get; private set; }
        public DateTimeOffset Expires { get; private set; }

        public void Destructor(out bool success, out string accessToken, out DateTimeOffset expires)
        {
            success = Success;
            accessToken = AccessToken;
            expires = Expires;
        }
    }
}
