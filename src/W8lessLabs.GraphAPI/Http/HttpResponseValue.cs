namespace W8lessLabs.GraphAPI
{
    public class HttpResponseValue<T>
    {
        public HttpResponseValue(string requestUri, bool success, T value)
            : this(requestUri, success, value, null)
        {
        }

        public HttpResponseValue(string requestUri, bool success, T value, string errorMessage)
        {
            RequestUri = requestUri;
            Success = success;
            Value = value;
            ErrorMessage = errorMessage;
        }

        public string RequestUri;
        public bool Success;
        public T Value;
        public string ErrorMessage;
    }
}
