namespace W8lessLabs.GraphAPI
{
    public class HttpResponseValue<T>
    {
        public HttpResponseValue(bool success, T value)
            : this(success, value, null)
        {
        }

        public HttpResponseValue(bool success, T value, string errorMessage)
        {
            Success = success;
            Value = value;
            ErrorMessage = errorMessage;
        }

        public bool Success;
        public T Value;
        public string ErrorMessage;
    }
}
