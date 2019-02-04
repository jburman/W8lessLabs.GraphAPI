using System;

namespace W8lessLabs.GraphAPI
{
    public static class OptionsExtensions
    {
        public static string AsString(this PermissionRoleOptions option)
        {
            switch(option)
            {
                case PermissionRoleOptions.Read:
                    return "read";
                case PermissionRoleOptions.Write:
                    return "write";
                case PermissionRoleOptions.SPMember:
                    return "sp.member";
                case PermissionRoleOptions.SPOwner:
                    return "sp.owner";
                default:
                    throw new ArgumentOutOfRangeException(nameof(option), $"Unknown {option.GetType().Name} type supplied.");
            }
        }

        public static string AsString(this SharingLinkScopeOptions option)
        {
            switch (option)
            {
                case SharingLinkScopeOptions.Anonymous:
                    return "anonymous";
                case SharingLinkScopeOptions.Organization:
                    return "organization";
                default:
                    throw new ArgumentOutOfRangeException(nameof(option), $"Unknown {option.GetType().Name} type supplied.");
            }
        }

        public static string AsString(this SharingLinkTypeOptions option)
        {
            switch (option)
            {
                case SharingLinkTypeOptions.View:
                    return "view";
                case SharingLinkTypeOptions.Edit:
                    return "edit";
                case SharingLinkTypeOptions.Embed:
                    return "embed";
                default:
                    throw new ArgumentOutOfRangeException(nameof(option), $"Unknown {option.GetType().Name} type supplied.");
            }
        }
    }
}
