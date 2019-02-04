using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using W8lessLabs.GraphAPI.Logging;

namespace W8lessLabs.GraphAPI
{
    public class HttpService : IHttpService
    {
        private HttpClient _http;
        private IJsonSerializer _json;
        private ILogger _logger;

        public HttpService(HttpClient client, IJsonSerializer json, ILoggerProvider loggerProvider = null)
        {
            _http = client ?? throw new ArgumentNullException(nameof(client));
            _json = json ?? throw new ArgumentNullException(nameof(json));
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            if (loggerProvider is null)
                _logger = NullLogger.Instance;
            else
                _logger = loggerProvider.GetLogger();
        }

        private HttpResponseValue<T> _CreateErrorResponse<T>(string requestUri, Exception ex) =>
            new HttpResponseValue<T>(requestUri, false, default, new ErrorMessage() { Message = ex.Message });

        private HttpResponseValue<T> _CreateErrorResponse<T>(string requestUri, string message)
        {
            ErrorMessage errorMessage = null;
            if (message?.StartsWith("{") == true)
            {
                try
                {
                    errorMessage = _json.Deserialize<ErrorResponse>(message)?.Error;
                } catch { }
            }

            if(errorMessage is null)
                errorMessage = new ErrorMessage()
                {
                    Message = message
                };

            return new HttpResponseValue<T>(requestUri, false, default, errorMessage);
        }

        public HttpServiceHeadersScope WithHeaders(params (string name, string value)[] headers) =>
            new HttpServiceHeadersScope(_http, headers, _logger);

        public async Task<HttpResponseValue<T>> GetJsonAsync<T>(string requestUri)
        {
            if (!string.IsNullOrEmpty(requestUri))
            {
                _logger.Trace("GetJsonAsync<{0}> {1}", typeof(T).Name, requestUri);

                try
                {
                    string response = await _http.GetStringAsync(requestUri).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(response))
                        return new HttpResponseValue<T>(requestUri, true, _json.Deserialize<T>(response));
                }
                catch (HttpRequestException httpEx)
                {
                    return _CreateErrorResponse<T>(requestUri, httpEx);
                }
            }
            return new HttpResponseValue<T>(null, false, default);
        }

        public async Task<HttpResponseValue<Stream>> GetStreamAsync(string requestUri, params (string name, string value)[] contentHeaders)
        {
            if (!string.IsNullOrEmpty(requestUri))
            {
                _logger.Trace("GetStreamAsync {0}", requestUri);

                try
                {
                    HttpServiceHeadersScope headerScope = null;

                    try
                    {
                        if(contentHeaders?.Length > 0)
                        {
                            headerScope = WithHeaders(contentHeaders);
                        }

                        var response = await _http.GetAsync(requestUri).ConfigureAwait(false);
                        if ((response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.PartialContent) 
                            && response.Content is StreamContent)
                            return new HttpResponseValue<Stream>(requestUri, true, await response.Content.ReadAsStreamAsync().ConfigureAwait(false));
                        if (response.StatusCode == HttpStatusCode.Redirect)
                        {
                            string location = response.Headers.Location.ToString(); // only follow one redirect here...
                            response = await _http.GetAsync(requestUri).ConfigureAwait(false);
                            if (response.IsSuccessStatusCode)
                                return new HttpResponseValue<Stream>(requestUri, true, await response.Content.ReadAsStreamAsync().ConfigureAwait(false));
                        }
                    }
                    finally
                    {
                        if (headerScope != null)
                            headerScope.Dispose();
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    return _CreateErrorResponse<Stream>(requestUri, httpEx);
                }
            }
            return new HttpResponseValue<Stream>(null, false, default);
        }

        public async Task<HttpResponseValue<string>> DeleteAsync(string requestUri)
        {
            if (!string.IsNullOrEmpty(requestUri))
            {
                _logger.Trace("DeleteAsync<string> {0}", requestUri);
                try
                {
                    HttpResponseMessage response = await _http.DeleteAsync(requestUri).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                        return new HttpResponseValue<string>(requestUri, true, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    else
                        return new HttpResponseValue<string>(requestUri, false, default);
                }
                catch (HttpRequestException httpEx)
                {
                    return _CreateErrorResponse<string>(requestUri, httpEx);
                }
            }
            return new HttpResponseValue<string>(null, false, default);
        }

        public async Task<HttpResponseValue<string>> PostJsonAsync(string requestUri, string jsonBody)
        {
            if (!string.IsNullOrEmpty(requestUri))
            {
                _logger.Trace("PostJsonAsync {0}", requestUri);

                try
                {
                    HttpResponseMessage response = await _http.PostAsync(requestUri, new StringContent(jsonBody, Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                        return new HttpResponseValue<string>(
                            requestUri,
                            true,
                            await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    else
                        return _CreateErrorResponse<string>(requestUri, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
                catch (HttpRequestException httpEx)
                {
                    return _CreateErrorResponse<string>(requestUri, httpEx);
                }
            }
            return new HttpResponseValue<string>(null, false, default);
        }

        public async Task<HttpResponseValue<T>> PostJsonAsync<T>(string requestUri, string jsonBody)
        {
            if (!string.IsNullOrEmpty(requestUri))
            {
                _logger.Trace("PostJsonAsync<{0}> {1}", typeof(T).Name, requestUri);

                try
                {
                    HttpResponseMessage response = await _http.PostAsync(requestUri, new StringContent(jsonBody, Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                        return new HttpResponseValue<T>(
                            requestUri,
                            true, 
                            _json.Deserialize<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false)));
                    else
                        return _CreateErrorResponse<T>(requestUri, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
                catch (HttpRequestException httpEx)
                {
                    return _CreateErrorResponse<T>(requestUri, httpEx);
                }
            }
            return new HttpResponseValue<T>(null, false, default);
        }

        public async Task<HttpResponseValue<T>> PatchJsonAsync<T>(string requestUri, string jsonBody)
        {
            if (!string.IsNullOrEmpty(requestUri))
            {
                _logger.Trace("PatchJsonAsync<{0}> {1}", typeof(T).Name, requestUri);

                try
                {
                    var method = new HttpMethod("PATCH");
                    HttpRequestMessage request = new HttpRequestMessage(method, requestUri)
                    {
                        Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
                    };

                    HttpResponseMessage response = await _http.SendAsync(request).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                        return new HttpResponseValue<T>(
                            requestUri,
                            true,
                            _json.Deserialize<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false)));
                    else
                        return _CreateErrorResponse<T>(requestUri, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
                catch (HttpRequestException httpEx)
                {
                    return _CreateErrorResponse<T>(requestUri, httpEx);
                }
            }
            return new HttpResponseValue<T>(null, false, default);
        }

        public async Task<HttpResponseValue<T>> PutJsonAsync<T>(string requestUri, string jsonBody)
        {
            if (!string.IsNullOrEmpty(requestUri))
            {
                _logger.Trace("PutJsonAsync<{0}> {1}", typeof(T).Name, requestUri);

                try
                {
                    HttpResponseMessage response = await _http.PutAsync(requestUri, new StringContent(jsonBody)).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                        return new HttpResponseValue<T>(
                            requestUri,
                            true, 
                            _json.Deserialize<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false)));
                    else
                        return _CreateErrorResponse<T>(requestUri, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
                catch (HttpRequestException httpEx)
                {
                    return _CreateErrorResponse<T>(requestUri, httpEx);
                }
            }
            return new HttpResponseValue<T>(null, false, default);
        }

        public async Task<HttpResponseValue<T>> PutBinaryAsync<T>(string requestUri, Stream content, params (string name, string value)[] contentHeaders) =>
            await _PutBinaryAsync<T>(requestUri, new StreamContent(content), contentHeaders).ConfigureAwait(false);

        public async Task<HttpResponseValue<T>> PutBinaryAsync<T>(string requestUri, byte[] content, params (string name, string value)[] contentHeaders) =>
            await _PutBinaryAsync<T>(requestUri, new ByteArrayContent(content), contentHeaders).ConfigureAwait(false);

        private async Task<HttpResponseValue<T>> _PutBinaryAsync<T>(string requestUri, HttpContent httpContent, (string name, string value)[] contentHeaders)
        {
            if (!string.IsNullOrEmpty(requestUri))
            {
                _logger.Trace("_PutBinaryAsync<{0}> {1}", typeof(T).Name, requestUri);

                try
                {
                    if (contentHeaders != null)
                    {
                        foreach ((string name, string value) in contentHeaders)
                        {
                            _logger.Trace("Adding Scoped Header: {0} = {1}", name, value);

                            httpContent.Headers.TryAddWithoutValidation(name, value);
                        }
                    }

                    HttpResponseMessage response = await _http.PutAsync(requestUri, httpContent).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                        return new HttpResponseValue<T>(
                            requestUri,
                            true,
                            _json.Deserialize<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false)));
                    else
                        return _CreateErrorResponse<T>(requestUri, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
                catch (HttpRequestException httpEx)
                {
                    return _CreateErrorResponse<T>(requestUri, httpEx);
                }
            }
            return new HttpResponseValue<T>(null, false, default);
        }
    }
}
