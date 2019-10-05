using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Gsemac {

    public abstract class FileHostDownloadClientBase :
        IFileHostDownloadClient {

        // Public members

        public event AsyncCompletedEventHandler DownloadFileCompleted {
            add {
                Client.DownloadFileCompleted += value;
            }
            remove {
                Client.DownloadFileCompleted -= value;
            }
        }
        public event DownloadProgressChangedEventHandler DownloadProgressChanged {
            add {
                Client.DownloadProgressChanged += value;
            }
            remove {
                Client.DownloadProgressChanged -= value;
            }
        }
        public event DownloadStringCompletedEventHandler DownloadStringCompleted {
            add {
                Client.DownloadStringCompleted += value;
            }
            remove {
                Client.DownloadStringCompleted -= value;
            }
        }
        public event OpenReadCompletedEventHandler OpenReadCompleted {
            add {
                Client.OpenReadCompleted += value;
            }
            remove {
                Client.OpenReadCompleted -= value;
            }
        }

        public virtual void DownloadFile(Uri address, string filename) {

            Uri directUri = GetDirectUri(address);

            PrepareClientForDownload(address);

            Client.DownloadFile(directUri, filename);

        }
        public virtual void DownloadFileAsync(Uri address, string filename, object userToken) {

            Uri directUri = GetDirectUri(address);

            PrepareClientForDownload(address);

            if (userToken is null)
                Client.DownloadFileAsync(directUri, filename);
            else
                Client.DownloadFileAsync(directUri, filename, userToken);

        }
        public virtual string DownloadString(Uri address) {

            Uri directUri = GetDirectUri(address);

            PrepareClientForDownload(address);

            return Client.DownloadString(directUri);

        }
        public virtual void DownloadStringAsync(Uri address, object userToken) {

            Uri directUri = GetDirectUri(address);

            PrepareClientForDownload(address);

            if (userToken is null)
                Client.DownloadStringAsync(directUri);
            else
                Client.DownloadStringAsync(directUri, userToken);

        }
        public virtual Stream OpenRead(Uri address) {

            Uri directUri = GetDirectUri(address);

            PrepareClientForDownload(address);

            return Client.OpenRead(directUri);

        }
        public virtual void OpenReadAsync(Uri address, object userToken) {

            Uri directUri = GetDirectUri(address);

            PrepareClientForDownload(address);

            if (userToken is null)
                Client.OpenReadAsync(directUri);
            else
                Client.OpenReadAsync(directUri, userToken);

        }
        public virtual Uri GetDirectUri(Uri address) {
            return address;
        }
        public virtual string GetFilename(Uri address) {

            string filename = System.IO.Path.GetFileName(address.LocalPath);

            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException("Could not deterine filename from the given URI.");

            return filename;

        }

        public void DownloadFile(string address, string filename) {
            DownloadFile(new Uri(address), filename);
        }
        public void DownloadFileAsync(Uri address, string filename) {
            DownloadFileAsync(address, filename, null);
        }
        public string DownloadString(string address) {
            return DownloadString(new Uri(address));
        }
        public void DownloadStringAsync(Uri address) {
            DownloadStringAsync(address, null);
        }
        public virtual void OpenReadAsync(Uri address) {
            OpenReadAsync(address, null);
        }
        public string GetDirectUri(string address) {
            return GetDirectUri(new Uri(address)).AbsoluteUri;
        }
        public string GetFilename(string address) {
            return GetFilename(new Uri(address));
        }

        public void Dispose() {
            Dispose(true);
        }

        // Protected members

        protected WebClientEx Client { get; private set; } = new WebClientEx();

        protected virtual void PrepareClientForDownload(Uri address) { }
        protected virtual void Dispose(bool disposing) {

            if (!_disposed) {

                if (disposing) {

                    if (_dispose_client)
                        Client.Dispose();

                }

                _disposed = true;
            }

        }

        protected static void SetWebClient(FileHostDownloadClientBase downloadClient, WebClientEx webClient, bool diposeWebClient) {

            downloadClient.Client = webClient;
            downloadClient._dispose_client = diposeWebClient;

        }

        // Private members

        private bool _dispose_client = true;
        private bool _disposed = false;

    }

}