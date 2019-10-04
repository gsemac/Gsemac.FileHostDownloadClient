using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gsemac {

    public class GoogleDriveDownloadClient :
        FileHostDownloadClientBase {

        // Public members

        public override void DownloadFile(Uri address, string filename) {

            using (WebClientEx client = CreateWebClient() as WebClientEx) {

                Uri directUri = _getDirectDownloadUri(client, address);

                _prepareClientForDownload(client, address);

                client.DownloadFile(directUri, filename);

            }

        }
        public override void DownloadFileAsync(Uri address, string filename, object userToken) {

            using (WebClientEx client = CreateWebClient() as WebClientEx) {

                Uri directUri = _getDirectDownloadUri(client, address);

                _prepareClientForDownload(client, address);

                client.DownloadFileAsync(directUri, filename);

            }

        }

        public override Uri GetDirectUri(Uri address) {

            using (WebClientEx client = CreateWebClient() as WebClientEx)
                return _getDirectDownloadUri(client, address);

        }

        public override string GetFilename(Uri address) {

            using (WebClientEx client = CreateWebClient() as WebClientEx) {

                Uri directUri = _getDirectDownloadUri(client, address);

                _prepareClientForDownload(client, address);

                using (Stream stream = client.OpenRead(directUri)) {

                    string filename = _getFileNameFromResponseHeaders(client, directUri);

                    stream.Close();

                    return filename;

                }

            }

        }

        // Protected members

        protected override string GetFilenameFromUri(Uri address) {

            // The filename can't be determined until we access the URI, so it will be retrieved later.
            return string.Empty;

        }

        // Private members

        private bool _addressIsSupported(Uri address) {
            return Regex.Match(address.AbsoluteUri, @"^https?:\/\/(?:www\.)?drive\.google\.com").Success;
        }
        private string _getFileIdFromUri(Uri address) {

            Match fileIdMatch = Regex.Match(address.Query, @"\bid=(.+?)(?:&|$)");

            if (!fileIdMatch.Success || string.IsNullOrEmpty(fileIdMatch.Groups[1].Value))
                throw new ArgumentException("Could not extract file ID from URI.");

            return fileIdMatch.Groups[1].Value;

        }
        private Uri _getDirectDownloadUri(WebClientEx client, Uri address, string confirmationCode = "") {

            if (!_addressIsSupported(address))
                throw new ArgumentException("This download client does not handle URIs of this type.");

            // Start by converting the URL to a download URL, which, under normal circumstances, can be downloaded directly.
            // Accessing this URL will redirect to a direct download URL.

            string fileId = _getFileIdFromUri(address);
            string downloadUrl = string.Format("{0}/uc?export=download&id={1}", address.GetLeftPart(UriPartial.Authority), fileId);
            string directDownloadUrl = string.Empty;

            if (!string.IsNullOrEmpty(confirmationCode))
                downloadUrl += string.Format("&confirm={0}", confirmationCode);

            // Issue a request to get the download URL we get redirected to.

            client.Headers.Add(HttpRequestHeader.Referer, address.AbsoluteUri);
            client.AllowAutoRedirect = false;

            string responseBody = client.Get(downloadUrl);

            client.AllowAutoRedirect = true;

            if (client.StatusCode == HttpStatusCode.Redirect) {

                directDownloadUrl = client.ResponseHeaders[HttpResponseHeader.Location];

            }
            else if (string.IsNullOrEmpty(confirmationCode)) {

                // Didn't get redirected? We're probably trying to download a file too large for Google to scan for viruses.
                // Ex: https://drive.google.com/uc?export=download&id=0BwmD_VLjROrfTHk4NFg2SndKcjQ

                // We'll need to get the confirmation code in order to proceed with the download, and then repeat this process.

                Match confirmationCodeMatch = Regex.Match(responseBody, @"\bconfirm=(.+?)&");

                if (confirmationCodeMatch.Success) {

                    confirmationCode = confirmationCodeMatch.Groups[1].Value;

                    return _getDirectDownloadUri(client, new Uri(downloadUrl), confirmationCode);

                }

            }

            if (string.IsNullOrEmpty(directDownloadUrl))
                throw new Exception("Failed to determine direct download URL.");

            // Return the result.

            return new Uri(directDownloadUrl);

        }
        private string _getFileNameFromResponseHeaders(WebClientEx client, Uri address) {

            string contentDispositionHeader = client.ResponseHeaders.Get("content-disposition") ?? "";
            Match filenameMatch = Regex.Match(contentDispositionHeader, @"\bfilename=""([^ ""]+?)""");
            string filename = filenameMatch.Success ? Uri.UnescapeDataString(filenameMatch.Groups[1].Value) : base.GetFilenameFromUri(address);

            return filename;

        }
        private void _prepareClientForDownload(WebClientEx client, Uri address) {

            client.Headers.Set(HttpRequestHeader.Referer, address.AbsoluteUri);

        }

    }

}