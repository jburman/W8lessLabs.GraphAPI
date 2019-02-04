using System.Collections.Generic;

namespace W8lessLabs.GraphAPI
{
    public class ErrorMessage
    {
        public ErrorMessage()
        {
            Code = string.Empty;
            Message = string.Empty;
        }

        public string Code { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string> InnerError { get; set; }
    }
}
