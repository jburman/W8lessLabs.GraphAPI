namespace W8lessLabs.GraphAPI
{
    public class Permission
    {
        public string Id { get; set; }
        public IdentitySet GrantedTo { get; set; }
        public SharingInvitation Invitation { get; set; }
        public ItemReference InheritedFrom { get; set; }
        public SharingLink Link { get; set; }
        public string[] Roles { get; set; }
        public string ShareId { get; set; }
    }
}
