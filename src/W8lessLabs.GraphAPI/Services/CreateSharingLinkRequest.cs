namespace W8lessLabs.GraphAPI
{
    internal class CreateSharingLinkRequest
    {
        public CreateSharingLinkRequest(SharingLinkTypeOptions type, SharingLinkScopeOptions scope)
        {
            Type = type.ToString().ToLower();
            Scope = scope.ToString().ToLower();
        }

        public string Type { get; set; }
        public string Scope { get; set; }
    }
}
