using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace W8lessLabs.GraphAPI
{
    public class HttpService : IHttpService
    {
        private HttpClient _http;
        private IJsonSerializer _json;

        public HttpService(HttpClient client, IJsonSerializer json)
        {
            _http = client ?? throw new ArgumentNullException(nameof(client));
            _json = json ?? throw new ArgumentNullException(nameof(json));
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public HttpServiceHeadersScope WithHeaders(params (string name, string value)[] headers) =>
            new HttpServiceHeadersScope(_http, headers);

        public async Task<T> GetJsonAsync<T>(string requestUri)
        {
            if (!string.IsNullOrEmpty(requestUri))
            {
                string response = await _http.GetStringAsync(requestUri);
                if (!string.IsNullOrEmpty(response))
                    return _json.Deserialize<T>(response);
            }
            return default;
        }
    }
}
