using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
                string response = await _http.GetStringAsync(requestUri).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(response))
                    return _json.Deserialize<T>(response);
            }
            return default;
        }

        public async Task<T> PostJsonAsync<T>(string requestUri, string jsonBody)
        {
            if (!string.IsNullOrEmpty(requestUri))
            {
                HttpResponseMessage response = await _http.PostAsync(requestUri, new StringContent(jsonBody, Encoding.UTF8, "application/json")).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                    return _json.Deserialize<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
            return default;
        }

        public async Task<T> PutJsonAsync<T>(string requestUri, string jsonBody)
        {
            if (!string.IsNullOrEmpty(requestUri))
            {
                HttpResponseMessage response = await _http.PutAsync(requestUri, new StringContent(jsonBody)).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                    return _json.Deserialize<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                else
                    Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
            return default;
        }

        public async Task<T> PutBinaryAsync<T>(string requestUri, Stream content)
        {
            if (!string.IsNullOrEmpty(requestUri))
            {
                var httpContent = new StreamContent(content);
                HttpResponseMessage response = await _http.PutAsync(requestUri, httpContent).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                    return _json.Deserialize<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                else
                    Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
            return default;
        }
    }
}
