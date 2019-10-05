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

        public override Uri GetDirectUri(Uri address) {
            return _getDirectDownloadUri(address);
        }
        public override string GetFilename(Uri address) {

            Uri directUri = _getDirectDownloadUri(address);

            PrepareClientForDownload(address);

            using (Stream stream = Client.OpenRead(directUri)) {

                string filename = _getFileNameFromResponseHeaders(directUri);

                stream.Close();

                return filename;

            }

        }

        // Protected members

        protected override void PrepareClientForDownload(Uri address) {
            Client.Referer = address.AbsoluteUri;
        }

        // Private members

        private string _getFileIdFromUri(Uri address) {

            // Match IDs from URIs of the following forms:
            // drive.google.com/open?id=XXXXX
            // drive.google.com/file/d/XXXXX/view?usp=sharing

            Match fileIdMatch = Regex.Match(address.AbsoluteUri, @"\b(?:id=|\/d\/)(.+?)(?=&|\/|$)");

            if (!fileIdMatch.Success || string.IsNullOrEmpty(fileIdMatch.Groups[1].Value))
                throw new ArgumentException("Could not extract file ID from URI.");

            return fileIdMatch.Groups[1].Value;

        }
        private Uri _getDirectDownloadUri(Uri address, string confirmationCode = "") {

            if (address.Host.ToLower() != "drive.google.com")
                throw new ArgumentException("This download client does not handle URIs of this type.");

            // Start by converting the URL to a download URL, which, under normal circumstances, can be downloaded directly.
            // Accessing this URL will redirect to a direct download URL.

            string fileId = _getFileIdFromUri(address);
            string downloadUrl = string.Format("{0}/uc?export=download&id={1}", address.GetLeftPart(UriPartial.Authority), fileId);
            string directDownloadUrl = string.Empty;

            if (!string.IsNullOrEmpty(confirmationCode))
                downloadUrl += string.Format("&confirm={0}", confirmationCode);

            // Issue a request to get the download URL we get redirected to.

            Client.Headers.Add(HttpRequestHeader.Referer, address.AbsoluteUri);
            Client.AllowAutoRedirect = false;

            string responseBody = Client.Get(downloadUrl);

            Client.AllowAutoRedirect = true;

            if (Client.StatusCode == HttpStatusCode.Redirect) {

                directDownloadUrl = Client.ResponseHeaders[HttpResponseHeader.Location];

            }
            else if (string.IsNullOrEmpty(confirmationCode)) {

                // Didn't get redirected? We're probably trying to download a file too large for Google to scan for viruses.
                // Ex: https://drive.google.com/uc?export=download&id=0BwmD_VLjROrfTHk4NFg2SndKcjQ

                // We'll need to get the confirmation code in order to proceed with the download, and then repeat this process.

                Match confirmationCodeMatch = Regex.Match(responseBody, @"\bconfirm=(.+?)&");

                if (confirmationCodeMatch.Success) {

                    confirmationCode = confirmationCodeMatch.Groups[1].Value;

                    return _getDirectDownloadUri(new Uri(downloadUrl), confirmationCode);

                }

            }

            if (string.IsNullOrEmpty(directDownloadUrl))
                throw new Exception("Failed to determine direct download URL.");

            // Return the result.

            return new Uri(directDownloadUrl);

        }
        private string _getFileNameFromResponseHeaders(Uri address) {

            string contentDispositionHeader = Client.ResponseHeaders.Get("content-disposition") ?? "";
            Match filenameMatch = Regex.Match(contentDispositionHeader, @"\bfilename=""([^ ""]+?)""");
            string filename = filenameMatch.Success ? Uri.UnescapeDataString(filenameMatch.Groups[1].Value) : base.GetFilename(address);

            return filename;

        }

    }

}