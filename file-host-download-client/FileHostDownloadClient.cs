using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
        public override string DownloadString(Uri address) {

            IFileHostDownloadClient downloadClient = _getClientFromRegistry(address.Host);

            if (downloadClient is null)
                return base.DownloadString(address);
            else
                return downloadClient.DownloadString(address);

        }
        public override void DownloadStringAsync(Uri address, object userToken) {

            IFileHostDownloadClient downloadClient = _getClientFromRegistry(address.Host);

            if (downloadClient is null)
                base.DownloadStringAsync(address, userToken);
            else
                downloadClient.DownloadStringAsync(address, userToken);

        }
        public override Stream OpenRead(Uri address) {

            IFileHostDownloadClient downloadClient = _getClientFromRegistry(address.Host);

            if (downloadClient is null)
                return base.OpenRead(address);
            else
                return downloadClient.OpenRead(address);

        }
        public override void OpenReadAsync(Uri address, object userToken) {

            IFileHostDownloadClient downloadClient = _getClientFromRegistry(address.Host);

            if (downloadClient is null)
                base.OpenReadAsync(address, userToken);
            else
                downloadClient.OpenReadAsync(address, userToken);

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

        public static void RegisterClient<T>(params string[] hostnames) where T : FileHostDownloadClientBase, new() {

            foreach (string hostname in hostnames)
                _client_registry.TryAdd(hostname.ToLower(System.Globalization.CultureInfo.InvariantCulture), (me) => {

                    T downloadClient = new T();

                    // Sets the download client's web client to this client's web client, so we don't need to copy over event handlers and so on.
                    SetWebClient(downloadClient, me.Client, false);

                    return downloadClient as IFileHostDownloadClient;

                });

        }

        // Private members

        private static ConcurrentDictionary<string, Func<FileHostDownloadClient, IFileHostDownloadClient>> _client_registry = new ConcurrentDictionary<string, Func<FileHostDownloadClient, IFileHostDownloadClient>>();

        private void _initializeRegistry() {

            if (_client_registry.Count <= 0) {

                RegisterClient<DropboxDownloadClient>("www.dropbox.com", "dl.dropboxusercontent.com");
                RegisterClient<GoogleDriveDownloadClient>("drive.google.com");
            }

        }
        private IFileHostDownloadClient _getClientFromRegistry(string hostname) {

            _initializeRegistry();

            if (_client_registry.TryGetValue(hostname, out Func<FileHostDownloadClient, IFileHostDownloadClient> func)) {

                IFileHostDownloadClient client = func(this);

                if (client is null)
                    throw new Exception(string.Format("Failed to construct client registered for host {0}.", hostname));

                if (client != null)
                    return client;

            }

            return null;

        }

    }

}