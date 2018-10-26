using System;

namespace W8lessLabs.GraphAPI
{
    public class Shared
    {
        public IdentitySet Owner { get; set; }
        public string Scope { get; set; }
        public IdentitySet SharedBy { get; set; }
        public DateTimeOffset SharedDateTime {get;set; }
    }
}
