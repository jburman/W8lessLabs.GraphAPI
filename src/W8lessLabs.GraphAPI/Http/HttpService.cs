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

        public async Task<HttpResponseValue<T>> GetJsonAsync<T>(string requestUri)
        {
            if (!string.IsNullOrEmpty(requestUri))
            {
                try
                {
                    string response = await _http.GetStringAsync(requestUri).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(response))
                        return new HttpResponseValue<T>(true, _json.Deserialize<T>(response));
                }
                catch (HttpRequestException httpEx)
                {
                    return new HttpResponseValue<T>(false, default, httpEx.Message);
                }
            }
            return default;
        }

        public async Task<HttpResponseValue<string>> DeleteAsync(string requestUri)
        {
            if (!string.IsNullOrEmpty(requestUri))
            {
                try
                {
                    HttpResponseMessage response = await _http.DeleteAsync(requestUri).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                        return new HttpResponseValue<string>(true, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    else
                        return new HttpResponseValue<string>(false, default);
                }
                catch (HttpRequestException httpEx)
                {
                    return new HttpResponseValue<string>(false, default, httpEx.Message);
                }
            }
            return default;
        }

        public async Task<HttpResponseValue<string>> PostJsonAsync(string requestUri, string jsonBody)
        {
            if (!string.IsNullOrEmpty(requestUri))
            {
                try
                {
                    HttpResponseMessage response = await _http.PostAsync(requestUri, new StringContent(jsonBody, Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                        return new HttpResponseValue<string>(
                            true,
                            await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    else
                        return new HttpResponseValue<string>(false, default, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
                catch (HttpRequestException httpEx)
                {
                    return new HttpResponseValue<string>(false, default, httpEx.Message);
                }
            }
            return default;
        }

        public async Task<HttpResponseValue<T>> PostJsonAsync<T>(string requestUri, string jsonBody)
        {
            if (!string.IsNullOrEmpty(requestUri))
            {
                try
                {
                    HttpResponseMessage response = await _http.PostAsync(requestUri, new StringContent(jsonBody, Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                        return new HttpResponseValue<T>(
                            true, 
                            _json.Deserialize<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false)));
                    else
                        return new HttpResponseValue<T>(false, default, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
                catch (HttpRequestException httpEx)
                {
                    return new HttpResponseValue<T>(false, default, httpEx.Message);
                }
            }
            return default;
        }

        public async Task<HttpResponseValue<T>> PutJsonAsync<T>(string requestUri, string jsonBody)
        {
            if (!string.IsNullOrEmpty(requestUri))
            {
                try
                {
                    HttpResponseMessage response = await _http.PutAsync(requestUri, new StringContent(jsonBody)).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                        return new HttpResponseValue<T>(
                            true, 
                            _json.Deserialize<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false)));
                    else
                        return new HttpResponseValue<T>(false, default, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
                catch (HttpRequestException httpEx)
                {
                    return new HttpResponseValue<T>(false, default, httpEx.Message);
                }
            }
            return default;
        }

        public async Task<HttpResponseValue<T>> PutBinaryAsync<T>(string requestUri, Stream content, params (string name, string value)[] contentHeaders)
        {
            if (!string.IsNullOrEmpty(requestUri))
            {
                try
                {
                    var httpContent = new StreamContent(content);
                    if(contentHeaders != null)
                    {
                        foreach ((string name, string value) in contentHeaders)
                            httpContent.Headers.TryAddWithoutValidation(name, value);
                    }

                    HttpResponseMessage response = await _http.PutAsync(requestUri, httpContent).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                        return new HttpResponseValue<T>(
                            true,
                            _json.Deserialize<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false)));
                    else
                        return new HttpResponseValue<T>(false, default, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
                catch (HttpRequestException httpEx)
                {
                    return new HttpResponseValue<T>(false, default, httpEx.Message);
                }
            }
            return default;
        }
    }
}
