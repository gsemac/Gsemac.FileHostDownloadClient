using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;

namespace Gsemac {

    public sealed class FileHostDownloadClient :
        FileHostDownloadClientBase {

        // Public members

        public override void DownloadFile(Uri address, string filename) {

            IFileHostDownloadClient downloadClient = _getClientFromRegistry(address.Host);

            if (downloadClient is null)
                base.DownloadFile(address, filename);
            else
                downloadClient.DownloadFile(address, filename);

        }
        public override void DownloadFileAsync(Uri address, string filename, object userToken) {

            IFileHostDownloadClient downloadClient = _getClientFromRegistry(address.Host);

            if (downloadClient is null)
                base.DownloadFileAsync(address, filename, userToken);
            else
                downloadClient.DownloadFileAsync(address, filename, userToken);

        }

        public override Uri GetDirectUri(Uri address) {

            IFileHostDownloadClient downloadClient = _getClientFromRegistry(address.Host);

            if (downloadClient is null)
                return base.GetDirectUri(address);
            else
                return downloadClient.GetDirectUri(address);

        }

        public override string GetFilename(Uri address) {

            IFileHostDownloadClient downloadClient = _getClientFromRegistry(address.Host);

            if (downloadClient is null)
                return base.GetFilename(address);
            else
                return downloadClient.GetFilename(address);

        }

        public static void RegisterClient<T>(params string[] hostnames) where T : IFileHostDownloadClient, new() {

            foreach (string hostname in hostnames)
                _client_registry.TryAdd(hostname.ToLower(System.Globalization.CultureInfo.InvariantCulture), () => new T());

        }

        // Private members

        private static ConcurrentDictionary<string, Func<IFileHostDownloadClient>> _client_registry = new ConcurrentDictionary<string, Func<IFileHostDownloadClient>>();

        private void _initializeRegistry() {

            if (_client_registry.Count <= 0) {

                RegisterClient<DropboxDownloadClient>("www.dropbox.com", "dl.dropboxusercontent.com");
                RegisterClient<GoogleDriveDownloadClient>("drive.google.com");
            }

        }
        private IFileHostDownloadClient _getClientFromRegistry(string hostname) {

            _initializeRegistry();

            if (_client_registry.TryGetValue(hostname, out Func<IFileHostDownloadClient> value))
                return value();

            return null;

        }

    }

}