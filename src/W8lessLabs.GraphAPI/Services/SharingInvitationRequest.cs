using System.Linq;

namespace W8lessLabs.GraphAPI
{
    public class SharingInvitationRequest
    {
        public SharingInvitationRequest(DriveRecipient[] recipients,
            string message,
            bool requireSignIn,
            bool sendInvidation,
            PermissionRoleOptions[] roles)
        {
            Recipients = recipients;
            Message = message;
            RequireSignIn = requireSignIn;
            SendInvitation = sendInvidation;
            Roles = roles?.Select(r => r.AsString()).ToArray();
        }

        public DriveRecipient[] Recipients { get; private set; }
        public string Message { get; private set; }
        public bool RequireSignIn { get; private set; }
        public bool SendInvitation { get; private set; }
        public string[] Roles { get; private set; }
    }
}
