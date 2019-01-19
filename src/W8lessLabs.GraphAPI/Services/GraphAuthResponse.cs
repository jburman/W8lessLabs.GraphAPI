using System;

namespace W8lessLabs.GraphAPI
{
    public class GraphAuthResponse
    {
        public GraphAuthResponse(string accessToken, DateTimeOffset tokenExpires, GraphAccount account)
        {
            AccessToken = accessToken;
            TokenExpires = tokenExpires;
            Account = account;
        }

        /// <summary>
        /// Bearer token for API calls
        /// </summary>
        public string AccessToken { get; set; }
        public DateTimeOffset TokenExpires { get; set; }
        public GraphAccount Account { get; set; }

        public void Deconstruct(out string accessToken, out DateTimeOffset tokenExpires, out GraphAccount account)
        {
            accessToken = AccessToken;
            tokenExpires = TokenExpires;
            account = Account;
        }
    }
}
