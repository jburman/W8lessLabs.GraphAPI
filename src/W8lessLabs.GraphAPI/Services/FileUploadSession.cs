using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace W8lessLabs.GraphAPI
{
    public class FileUploadSession : IDisposable
    {
        internal class UploadRangeResponse
        {
            public DateTimeOffset ExpirationDateTime { get; set; }
            public string[] NextExpectedRanges { get; set; }

            public string Id { get; set; }
            public string Name { get; set; }
            public int Size { get; set; }
        }

        private enum UploadStatus
        {
            InProgress,
            Errored,
            Success,
            Cancelled
        }

        private readonly IHttpService _http;
        private readonly IJsonSerializer _json;
        private readonly FileUploadRequest _uploadRequest;
        private UploadStatus _status;

        internal FileUploadSession(
            FileUploadRequest uploadRequest,
            string uploadSessionUrl,
            DateTimeOffset uploadSessionExpires,
            IHttpService http,
            IJsonSerializer json)
        {
            _uploadRequest = uploadRequest ?? throw new ArgumentNullException(nameof(uploadRequest));
            SessionUrl = uploadSessionUrl ?? throw new ArgumentNullException(nameof(uploadSessionUrl));
            SessionExpires = uploadSessionExpires;
            _status = UploadStatus.InProgress;

            _http = http ?? throw new ArgumentNullException(nameof(http));
            _json = json ?? throw new ArgumentNullException(nameof(json));
        }

        public string SessionUrl { get; private set; }
        public DateTimeOffset SessionExpires { get; private set; }

        public async Task UploadAsync(Stream content, CancellationToken cancellationToken = default)
        {
            long length = content.Length;
            // TODO - chunk into ranges
            var response = await _http.PutBinaryAsync<UploadRangeResponse>(SessionUrl, content, 
                ("Content-Length", length.ToString()),
                ("Content-Range", $"bytes 0-{(length - 1).ToString()}/{length.ToString()}"));
        }

        private bool _CanCancel => _status == UploadStatus.InProgress || _status == UploadStatus.Errored;

        public async Task<bool> CancelUploadAsync()
        {
            if(_CanCancel && !string.IsNullOrEmpty(SessionUrl))
            {
                var response = await _http.DeleteAsync(SessionUrl).ConfigureAwait(false);
                _status = UploadStatus.Cancelled;
                return response.Success;
            }
            return false;
        }

        private bool _disposed;
        public void Dispose()
        {
            if(!_disposed)
            {
                _disposed = true;

                if(!string.IsNullOrEmpty(SessionUrl))
                {
                    if (_CanCancel)
                    {
                        CancelUploadAsync().GetAwaiter().GetResult();
                        SessionUrl = string.Empty;
                    }
                }
            }
        }
    }
}
