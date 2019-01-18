namespace W8lessLabs.GraphAPI
{
    public class GraphAccount
    {
        public GraphAccount()
        {
        }

        public GraphAccount(string accountId, 
            string accountName, 
            string identityProvider,
            string azureObjectId,
            string azureTenantId)
        {
            AccountId = accountId;
            AccountName = accountName;
            IdentityProvider = identityProvider;
            AzureObjectId = azureObjectId;
            AzureTenantId = azureTenantId;
        }

        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public string IdentityProvider { get; set; }

        public string AzureObjectId { get; set; }
        public string AzureTenantId { get; set; }
    }
}
