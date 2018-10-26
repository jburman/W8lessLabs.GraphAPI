using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace W8lessLabs.GraphAPI
{
    public class GraphService
    {
        internal const string GraphEndpoint = "https://graph.microsoft.com/v1.0";
        internal const string GraphEndpoint_Me = GraphEndpoint+ "/me";
        internal const string GraphEndpoint_Drives = GraphEndpoint + "/drives";
        internal const string GraphEndpoint_DriveRoot = GraphEndpoint+ "/me/drive/root";
        internal const string GraphEndpoint_DriveItems = GraphEndpoint + "/me/drive/items";
        internal const string GraphEndpoint_DriveRootChildren = GraphEndpoint + "/me/drive/root/children";
        internal const string GraphEndpoint_Delta = GraphEndpoint + "/me/drive/root/delta";

        internal const string SkipTokenParam = "$skipToken=";
        internal const string DeltaTokenParam = "token=";

        private readonly IAuthService _authService;
        private readonly IHttpService _http;

        // cache token for a few minutes
        private string _token;
        private DateTimeOffset _tokenExpires;

        // keep a small LRU queue of drive items
        private LinkedList<LRUCacheEntry<GetDriveItemsResponse>> _itemsCache;
        private const int CacheLimit = 30;
        private const int MaxCacheAgeSeconds = 5 * 3;

        public GraphService(IAuthService authService, IHttpService http)
        {
            _authService = authService;
            _http = http;
            _itemsCache = new LinkedList<LRUCacheEntry<GetDriveItemsResponse>>();
            _token = null;
        }

        private async Task<(bool tokenSuccess, string token)> _TryGetTokenAsync()
        {
            bool tokenSuccess = false;
            string token = null;
            DateTimeOffset tokenExpires = default;
            if(_token == null || tokenExpires == default || DateTimeOffset.Now > (tokenExpires - TimeSpan.FromSeconds(30)))
            {
                (tokenSuccess, token, tokenExpires) = await _authService.TryGetTokenAsync();
                if(tokenSuccess)
                {
                    _token = token;
                    _tokenExpires = tokenExpires;
                }
            }
            else
            {
                token = _token;
                tokenSuccess = true;
            }

            return (tokenSuccess, token);
        }

        private bool _TryGetRequestUrlForFolder(string path, out string requestUrl)
        {
            if (string.IsNullOrEmpty(path) || path == "/")
                requestUrl = GraphEndpoint_DriveRoot;
            else
                requestUrl = GraphEndpoint_DriveRoot + ":" + Uri.EscapeUriString(path);
            return true;
        }

        private bool _TryGetRequestUrlForChildren(string path, out string requestUrl)
        {
            if (string.IsNullOrEmpty(path) || path == "/")
                requestUrl = GraphEndpoint_DriveRootChildren;
            else
                requestUrl = GraphEndpoint_DriveRoot + ":" + Uri.EscapeUriString(path) + ":/children";
            return true;
        }

        private bool _TryGetRequestUrlForPermissions(string path, out string requestUrl)
        {
            if (string.IsNullOrEmpty(path) || path == "/")
                requestUrl = GraphEndpoint_DriveRoot + "/permissions";
            else
                requestUrl = GraphEndpoint_DriveRoot + ":" + Uri.EscapeUriString(path) + ":/permissions";
            return true;
        }

        private bool _TryGetRequestUrlForPermissionsById(string driveItemId, out string requestUrl)
        {
            if (string.IsNullOrEmpty(driveItemId))
            {
                requestUrl = string.Empty;
                return false;
            }
            else
                requestUrl = GraphEndpoint_DriveItems + "/" + Uri.EscapeUriString(driveItemId) + "/permissions";
            return true;
        }

        private bool _TryGetRequestUrlWithPaging(GetDriveItemsRequest request, out string requestUrl)
        {
            if (_TryGetRequestUrlForChildren(request?.Path, out requestUrl))
            {
                int pageSize = Math.Max(request.PageSize, GetDriveItemsRequest.MinimumPageSize);
                requestUrl += "?$top=" + pageSize;

                if (!string.IsNullOrEmpty(request.SkipToken))
                    requestUrl += "&$skipToken=" + WebUtility.UrlEncode(request.SkipToken);

                return true;
            }
            else
                requestUrl = null;
            return false;
        }

        private string _GetSkipToken(string nextLink)
        {
            string skipToken = null;
            if(!string.IsNullOrEmpty(nextLink))
            {
                int index = nextLink.IndexOf(SkipTokenParam, StringComparison.OrdinalIgnoreCase);
                if(index != -1)
                {
                    skipToken = nextLink.Substring(index + SkipTokenParam.Length);
                    index = skipToken.IndexOf('&');
                    if(index != -1) // there's another parameter to trim off...
                        skipToken = skipToken.Substring(0, index);
                }
            }
            return skipToken;
        }

        private HttpServiceHeadersScope _WithAuthHeader(string token) =>
            _http.WithHeaders(("Authorization", "Bearer " + token));

        public async Task<GraphUser> GetMeAsync()
        {
            (bool tokenSuccess, string token) = await _TryGetTokenAsync();
            if (tokenSuccess)
            {
                using(_WithAuthHeader(token))
                    return await _http.GetJsonAsync<GraphUser>(GraphEndpoint_Me);
            }
            else
                return null;
        }

        public async Task<GetDriveItemsResponse> GetDriveItemsAsync(GetDriveItemsRequest request)
        {
            if (request != null)
            {
                if (request.AllowCache && _TryGetFromCache(request.GetCacheKey(), out GetDriveItemsResponse cachedResponse))
                    return cachedResponse;
                else
                {
                    if (_TryGetRequestUrlWithPaging(request, out string requestUrl))
                    {
                        (bool tokenSuccess, string token) = await _TryGetTokenAsync();
                        if (tokenSuccess)
                        {
                            using (_WithAuthHeader(token))
                            {
                                var graphResponse = await _http.GetJsonAsync<GraphResponse<DriveItem[]>>(requestUrl);
                                var response = new GetDriveItemsResponse(graphResponse.Value, _GetSkipToken(graphResponse.NextLink));
                                _AddToCache(request, response);
                                return response;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public async Task<GetDriveItemsDeltaResponse> GetDriveItemsDeltaAsync(string deltaOrNextLink = null)
        {
            string requestUrl = GraphEndpoint_DriveRoot + "/delta";
            if (!string.IsNullOrEmpty(deltaOrNextLink))
                requestUrl = deltaOrNextLink;

            (bool tokenSuccess, string token) = await _TryGetTokenAsync();
            if (tokenSuccess)
            {
                using (_WithAuthHeader(token))
                {
                    var graphResponse = await _http.GetJsonAsync<GraphResponse<DriveItem[]>>(requestUrl);
                    var response = new GetDriveItemsDeltaResponse(graphResponse.Value, graphResponse.NextLink, graphResponse.DeltaLink);
                    return response;
                }
            }
            return null;
        }

        public string GetDeltaLinkFromToken(string deltaToken)
        {
            if (deltaToken is null) throw new ArgumentNullException(nameof(deltaToken));

            return GraphEndpoint_Delta + "?" + DeltaTokenParam + Uri.EscapeDataString(deltaToken);
        }

        public async Task<int> GetChildItemsCountAsync(string path)
        {
            int count = 0;
            if (_TryGetRequestUrlForFolder(path, out string requestUrl))
            {
                requestUrl += "?expand=children(select=id)";

                (bool tokenSuccess, string token) = await _TryGetTokenAsync();
                if (tokenSuccess)
                {
                    using (_WithAuthHeader(token))
                    {
                        var response = await _http.GetJsonAsync<DriveItem>(requestUrl);
                        count = response?.IsFolder == true ? response.Folder.ChildCount : 0;
                    }
                }
            }
            return count;
        }

        public async Task<IEnumerable<Permission>> GetPermissionsAsync(string path)
        {
            if (_TryGetRequestUrlForPermissions(path, out string requestUrl))
            {
                (bool tokenSuccess, string token) = await _TryGetTokenAsync();
                if (tokenSuccess)
                {
                    using (_WithAuthHeader(token))
                    {
                        var response = await _http.GetJsonAsync<GraphResponse<Permission[]>>(requestUrl);
                        return response?.Value;
                    }
                }
            }
            return Array.Empty<Permission>();
        }

        public async Task<IEnumerable<Permission>> GetPermissionsByIdAsync(string driveItemId)
        {
            if (_TryGetRequestUrlForPermissionsById(driveItemId, out string requestUrl))
            {
                (bool tokenSuccess, string token) = await _TryGetTokenAsync();
                if (tokenSuccess)
                {
                    using (_WithAuthHeader(token))
                    {
                        var response = await _http.GetJsonAsync<GraphResponse<Permission[]>>(requestUrl);
                        return response?.Value;
                    }
                }
            }
            return Array.Empty<Permission>();
        }

        private bool _TryGetFromCache(string cacheKey, out GetDriveItemsResponse cachedResponse)
        {
            var entry = _itemsCache.FirstOrDefault(i => i.Key == cacheKey);
            bool found = false;

            if (entry != null)
            {
                if ((DateTime.Now - entry.Created).TotalSeconds > MaxCacheAgeSeconds)
                {
                    _itemsCache.Remove(entry);
                    cachedResponse = default;
                }
                else
                {
                    entry.LastUsed = DateTime.Now;
                    cachedResponse = entry.Value;
                    found = true;
                }
            }
            else
                cachedResponse = default;

            return found;
        }

        private void _AddToCache(GetDriveItemsRequest request, GetDriveItemsResponse response)
        {
            if (!_TryGetFromCache(request.GetCacheKey(), out GetDriveItemsResponse cachedResponse))
            {
                if (_itemsCache.Count > CacheLimit)
                {
                    var prune = _itemsCache.OrderBy(c => c.LastUsed).First();
                    _itemsCache.Remove(prune);
                }
                _itemsCache.AddFirst(new LRUCacheEntry<GetDriveItemsResponse>()
                {
                    Key = request.GetCacheKey(),
                    Value = response,
                    LastUsed = DateTime.Now,
                    Created = DateTime.Now
                });
            }
        }

        private class LRUCacheEntry<T>
        {
            public string Key { get; set; }
            public DateTime LastUsed { get; set; }
            public DateTime Created { get; set; }
            public T Value { get; set; }
        }
    }
}
