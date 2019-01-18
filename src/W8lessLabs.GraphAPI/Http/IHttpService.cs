using System.IO;
using System.Threading.Tasks;

namespace W8lessLabs.GraphAPI
{
    public interface IHttpService
    {
        HttpServiceHeadersScope WithHeaders(params (string name, string value)[] headers);
        Task<T> GetJsonAsync<T>(string requestUri);
        Task<T> PostJsonAsync<T>(string requestUri, string jsonBody);
        Task<T> PutJsonAsync<T>(string requestUri, string jsonBody);
        Task<T> PutBinaryAsync<T>(string requestUri, Stream content);
    }
}
