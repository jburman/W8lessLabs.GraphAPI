using System;

namespace W8lessLabs.GraphAPI.Windows
{
    public class GraphAccount
    {
        public GraphAccount(
            string accountId, 
            string accountName, 
            string environment,
            string tenantId, 
            string azureADObjectId)
        {
            AccountId = accountId;
            AccountName = accountName;
            Environment = environment;
            TenantId = tenantId;
            AzureADObjectId = azureADObjectId;
        }

        
        public string AccountId { get; private set; }
        public string AccountName { get; private set; }
        public string Environment { get; private set; }
        public string TenantId { get; private set; }
        public string AzureADObjectId { get; private set; }

        public void Deconstruct(
            out string accountId,
            out string accountName,
            out string environment,
            out string tenantId,
            out string azureADObjectId)
        {
            accountId = AccountId;
            accountName = AccountName;
            environment = Environment;
            tenantId = TenantId;
            azureADObjectId = AzureADObjectId;
        }
    }
}
