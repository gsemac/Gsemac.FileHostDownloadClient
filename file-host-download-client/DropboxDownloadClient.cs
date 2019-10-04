using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Gsemac {

    public class DropboxDownloadClient :
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

                client.DownloadFileAsync(directUri, filename, userToken);

            }

        }

        public override Uri GetDirectUri(Uri address) {

            using (WebClientEx client = CreateWebClient() as WebClientEx)
                return _getDirectDownloadUri(client, address);

        }

        // Private members

        private enum AddressType {
            Unsupported,
            Dropbox,
            DropboxUserContent
        }

        private AddressType _getAddressType(string address) {

            Match domainMatch = Regex.Match(address, @"^https?:\/\/(?:www\.)?(dropbox|dl\.dropboxusercontent)\.com");

            if (!domainMatch.Success)
                return AddressType.Unsupported;

            switch (domainMatch.Groups[1].Value.ToLower()) {

                case "dropbox":
                    return AddressType.Dropbox;

                case "dl.dropboxusercontent":
                    return AddressType.DropboxUserContent;

                default:
                    return AddressType.Unsupported;

            }

        }
        private bool _addressIsSupported(string address) {
            return _getAddressType(address) != AddressType.Unsupported;
        }
        private Uri _getDirectDownloadUri(WebClientEx client, Uri address) {

            AddressType type = _getAddressType(address.AbsoluteUri);
            string directUrl = string.Empty;

            switch (type) {

                case AddressType.Dropbox:
                    directUrl = _getDirectDownloadUrlFromDropbox(client, address);
                    break;

                case AddressType.DropboxUserContent:
                    directUrl = _getDirectDownloadUrlFromDropboxUserContent(client, address);
                    break;

                default:
                    throw new ArgumentException("This download client does not handle URIs of this type.");

            }

            if (!string.IsNullOrEmpty(directUrl) && Uri.TryCreate(directUrl, UriKind.Absolute, out Uri result))
                return result;
            else
                throw new Exception("Failed to get direct download URI.");

        }
        private string _getDirectDownloadUrlFromDropbox(WebClientEx client, Uri address) {

            // Make a GET request to the page so we can get cookies to send with the request.

            client.DownloadString(address);

            // Make a POST request to "/sharing/fetch_user_content_link" to get a direct download link.

            string postAddress = string.Format("{0}/sharing/fetch_user_content_link", address.GetLeftPart(UriPartial.Authority));

            client.Headers.Set(HttpRequestHeader.Referer, address.AbsoluteUri);
            client.Headers.Set("x-requested-with", "XMLHttpRequest");

            NameValueCollection formData = new NameValueCollection {
                { "is_xhr", "true" },
                { "url", address.AbsoluteUri }
            };

            string t = client.Cookies.GetCookies(address).Cast<Cookie>().FirstOrDefault(x => x.Name == "t")?.Value ?? string.Empty;

            if (!string.IsNullOrEmpty(t))
                formData.Add("t", t);

            string responseBody = Encoding.UTF8.GetString(client.UploadValues(postAddress, "POST", formData));
            string directDownloadUrl = responseBody;

            // Remove added headers that are no longer relevant.

            client.Headers.Remove("x-requested-with");

            // Return the result.

            return directDownloadUrl;

        }
        private string _getDirectDownloadUrlFromDropboxUserContent(WebClientEx client, Uri address) {

            string directDownloadUrl = address.AbsoluteUri;

            // We might get redirected to the "speedbump" page before being able to download the file for certain file types.
            // If that happens, we'll need to follow the redirect and then extract the download URL.

            client.Head(address);

            if (client.StatusCode == HttpStatusCode.Redirect) {

                // Follow the redirect to the "speedbump" page, where we can extract the direct download URL.

                string locationHeader = client.ResponseHeaders[HttpResponseHeader.Location];

                client.Headers.Add(HttpRequestHeader.Referer, address.AbsoluteUri);

                string speedbumpResponse = client.DownloadString(locationHeader);

                Match contentLinkMatch = Regex.Match(speedbumpResponse, @"content_link"": ""([^ ""]+)");

                if (contentLinkMatch.Success)
                    directDownloadUrl = contentLinkMatch.Groups[1].Value;
                else
                    throw new Exception("Failed to determine direct download URL.");

            }

            return directDownloadUrl;

        }
        private void _prepareClientForDownload(WebClientEx client, Uri address) {

            client.Headers.Set(HttpRequestHeader.Referer, address.AbsoluteUri);

        }

    }

}