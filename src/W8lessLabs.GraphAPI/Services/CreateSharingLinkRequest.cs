namespace W8lessLabs.GraphAPI
{
    internal class CreateSharingLinkRequest
    {
        public CreateSharingLinkRequest(SharingLinkTypeOptions type, SharingLinkScopeOptions scope)
        {
            Type = type.AsString();
            Scope = scope.AsString();
        }

        public string Type { get; private set; }
        public string Scope { get; private set; }
    }
}
