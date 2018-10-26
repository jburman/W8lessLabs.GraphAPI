namespace W8lessLabs.GraphAPI
{
    public class SharingInvitation
    {
        public string Email { get; set; }
        public IdentitySet InvitedBy { get; set; }
        public bool SignInRequired { get; set; }
    }
}
