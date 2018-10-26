namespace W8lessLabs.GraphAPI
{
    public class SharingLink
    {
        public Identity Application { get; set; }
        public string Type { get; set; }
        public string Scope { get; set; }
        public string WebHtml { get; set; }
        public string WebUrl { get; set; }
    }
}
