using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;

namespace Gsemac {

    public abstract class FileHostDownloadClientBase :
        IFileHostDownloadClient {

        // Public members

        public virtual string Accept { get; set; } = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
        public virtual DecompressionMethods AcceptEncoding { get; set; } = DecompressionMethods.Deflate | DecompressionMethods.GZip;
        public virtual string AcceptLanguage { get; set; } = "en-US,en;q=0.5";
        public virtual string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:64.0) Gecko/20100101 Firefox/64.0";
        public virtual CookieContainer Cookies { get; set; } = new CookieContainer();

        public event AsyncCompletedEventHandler DownloadFileCompleted;
        public event DownloadProgressChangedEventHandler DownloadProgressChanged;

        public virtual void DownloadFile(Uri address, string filename) {

            using (WebClient client = CreateWebClient())
                client.DownloadFile(address, filename);

        }
        public void DownloadFile(string address, string filename) {
            DownloadFile(new Uri(address), filename);
        }

        public void DownloadFileAsync(Uri address, string filename) {
            DownloadFileAsync(address, filename, null);
        }
        public virtual void DownloadFileAsync(Uri address, string filename, object userToken) {

            using (WebClient client = CreateWebClient()) {

                if (userToken is null)
                    client.DownloadFileAsync(address, filename);
                else
                    client.DownloadFileAsync(address, filename, userToken);

            }

        }

        public virtual Uri GetDirectUri(Uri address) {
            return address;
        }
        public string GetDirectUri(string address) {
            return GetDirectUri(new Uri(address)).AbsoluteUri;
        }

        public virtual string GetFilename(Uri address) {
            return GetFilenameFromUri(address);
        }
        public string GetFilename(string address) {
            return GetFilename(new Uri(address));
        }

        // Protected members

        protected WebClient CreateWebClient() {

            WebClientEx client = new WebClientEx {
                Cookies = Cookies,
                AutomaticDecompression = AcceptEncoding
            };

            client.Headers[HttpRequestHeader.Accept] = Accept;
            client.Headers[HttpRequestHeader.AcceptLanguage] = AcceptLanguage;
            client.Headers[HttpRequestHeader.UserAgent] = UserAgent;

            if (DownloadFileCompleted != null)
                client.DownloadFileCompleted += DownloadFileCompleted;

            if (DownloadProgressChanged != null)
                client.DownloadProgressChanged += DownloadProgressChanged;

            return client;

        }
        protected string GetFilenameFromUri(Uri address) {

            string filename = System.IO.Path.GetFileName(address.LocalPath);

            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException("Could not deterine filename from the given URI.");

            return filename;

        }

    }

}