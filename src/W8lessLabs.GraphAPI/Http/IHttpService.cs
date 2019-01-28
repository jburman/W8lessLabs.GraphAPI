using System.IO;
using System.Threading.Tasks;

namespace W8lessLabs.GraphAPI
{
    public interface IHttpService
    {
        HttpServiceHeadersScope WithHeaders(params (string name, string value)[] headers);
        Task<HttpResponseValue<T>> GetJsonAsync<T>(string requestUri);
        Task<HttpResponseValue<string>> DeleteAsync(string requestUri);
        Task<HttpResponseValue<T>> PostJsonAsync<T>(string requestUri, string jsonBody);
        Task<HttpResponseValue<string>> PostJsonAsync(string requestUri, string jsonBody);
        Task<HttpResponseValue<T>> PutJsonAsync<T>(string requestUri, string jsonBody);
        Task<HttpResponseValue<T>> PutBinaryAsync<T>(string requestUri, Stream content, params (string name, string value)[] contentHeaders);
    }
}
