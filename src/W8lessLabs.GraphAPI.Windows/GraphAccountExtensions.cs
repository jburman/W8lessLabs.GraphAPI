using Microsoft.Identity.Client;

namespace W8lessLabs.GraphAPI.Windows
{
    public static class GraphAccountExtensions
    {
        public static GraphAccount ToGraphAccount(this IAccount account)
        {
            if (account is null)
                return default;
            else
                return new GraphAccount(
                    accountId: account.HomeAccountId?.Identifier,
                    accountName: account.Username,
                    environment: account.Environment,
                    azureADObjectId: account.HomeAccountId?.ObjectId,
                    tenantId: account.HomeAccountId?.TenantId);
        }
    }
}
