using Microsoft.Identity.Client;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace W8lessLabs.GraphAPI.Windows
{
    internal class TokenCacheService
    {
        public TokenCacheService(string cacheName)
        {
            _fileLock = new object();
            _cacheFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), 
                cacheName + ".msalcache.bin");

            TokenCache = new TokenCache();
            TokenCache.SetBeforeAccess(BeforeAccessNotification);
            TokenCache.SetAfterAccess(AfterAccessNotification);
        }

        private string _cacheFilePath;
        private readonly object _fileLock;

        public TokenCache TokenCache { get; private set; }

        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (_fileLock)
            {
                args.TokenCache.Deserialize(
                    System.IO.File.Exists(_cacheFilePath) ?
                        ProtectedData.Unprotect(System.IO.File.ReadAllBytes(_cacheFilePath), null, DataProtectionScope.CurrentUser)
                    : null);
            }
        }

        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (args.HasStateChanged)
            {
                lock (_fileLock)
                {
                    System.IO.File.WriteAllBytes(_cacheFilePath,
                        ProtectedData.Protect(args.TokenCache.Serialize(), null, DataProtectionScope.CurrentUser));
                }
            }
        }
    }
}
