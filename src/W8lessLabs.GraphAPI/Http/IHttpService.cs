using System.Threading.Tasks;

namespace W8lessLabs.GraphAPI
{
    public interface IHttpService
    {
        HttpServiceHeadersScope WithHeaders(params (string name, string value)[] headers);
        Task<T> GetJsonAsync<T>(string requestUri);
    }
}
