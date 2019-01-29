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


        private async Task _WriteFileStreamAsync(Stream content, int contentLength, int offset)
        {
            var response = await _http.PutBinaryAsync<UploadRangeResponse>(
                SessionUrl,
                content,
                ("Content-Length", contentLength.ToString()),
                ("Content-Range", $"bytes 0-{(contentLength - 1).ToString()}/{contentLength.ToString()}"));
        }

        private async Task _WriteFileChunkAsync(byte[] content, int contentLength, int fileSize, int offset)
        {
            var response = await _http.PutBinaryAsync<UploadRangeResponse>(
                SessionUrl, 
                content,
                ("Content-Length", contentLength.ToString()),
                ("Content-Range", $"bytes {offset}-{((offset + contentLength) - 1).ToString()}/{fileSize.ToString()}"));
        }

        public async Task UploadAsync(Stream content, int fileSize, CancellationToken cancellationToken = default)
        {
            // From the Docs - https://docs.microsoft.com/en-us/graph/api/driveitem-createuploadsession?view=graph-rest-1.0#upload-bytes-to-the-upload-session
            // QUOTE:   Note: If your app splits a file into multiple byte ranges, the size of each byte range MUST be a multiple of 320 KiB (327,680 bytes). 
            //          Using a fragment size that does not divide evenly by 320 KiB will result in errors committing some files.
            const int ChunkSize = 327_680 * 8;

            if(fileSize < ChunkSize)
                await _WriteFileStreamAsync(content, fileSize, 0).ConfigureAwait(false);
            else
            {
                int chunks = (int)Math.Ceiling(fileSize / (double)ChunkSize);
                byte[] buffer = new byte[ChunkSize];
                int bytesRead = 0;
                int totalBytesRead = 0;
                int uploadOffset = 0;
                while(totalBytesRead < fileSize && (bytesRead = await content.ReadAsync(buffer, 0, ChunkSize)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if(bytesRead < ChunkSize)
                    {
                        byte[] lastBuffer = new byte[bytesRead];
                        Array.Copy(buffer, 0, lastBuffer, 0, lastBuffer.Length);
                        buffer = lastBuffer;
                    }
                    // TODO handle fail / retry / back-off
                    await _WriteFileChunkAsync(buffer, bytesRead, fileSize, uploadOffset);
                    uploadOffset += bytesRead;
                }
            }
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
