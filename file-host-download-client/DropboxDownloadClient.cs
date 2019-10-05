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

        public override Uri GetDirectUri(Uri address) {

            string directUrl = string.Empty;

            switch (address.Host.ToLower()) {

                case "www.dropbox.com":
                    directUrl = _getDirectDownloadUrlFromDropbox(address);
                    break;

                case "dl.dropboxusercontent.com":
                    directUrl = _getDirectDownloadUrlFromDropboxUserContent(address);
                    break;

                default:
                    throw new ArgumentException("This download client does not handle URIs of this type.");

            }

            if (!string.IsNullOrEmpty(directUrl) && Uri.TryCreate(directUrl, UriKind.Absolute, out Uri result))
                return result;
            else
                throw new Exception("Failed to get direct download URI.");

        }

        // Protected members

        protected override void PrepareClientForDownload(Uri address) {
            Client.Referer = address.AbsoluteUri;
        }

        // Private members

        private string _getDirectDownloadUrlFromDropbox(Uri address) {

            // Make a GET request to the page so we can get cookies to send with the request.
            Client.DownloadString(address);

            // Make a POST request to "/sharing/fetch_user_content_link" to get a direct download link.

            string postAddress = string.Format("{0}/sharing/fetch_user_content_link", address.GetLeftPart(UriPartial.Authority));

            Client.Headers.Set(HttpRequestHeader.Referer, address.AbsoluteUri);
            Client.Headers.Set("x-requested-with", "XMLHttpRequest");

            NameValueCollection formData = new NameValueCollection {
                { "is_xhr", "true" },
                { "url", address.AbsoluteUri }
            };

            string t = Client.Cookies.GetCookies(address).Cast<Cookie>().FirstOrDefault(x => x.Name == "t")?.Value ?? string.Empty;

            if (!string.IsNullOrEmpty(t))
                formData.Add("t", t);

            string directDownloadUrl = Client.Post(postAddress, formData);

            // Remove added headers that are no longer relevant.
            Client.Headers.Remove("x-requested-with");

            // Return the result.
            return directDownloadUrl;

        }
        private string _getDirectDownloadUrlFromDropboxUserContent(Uri address) {

            string directDownloadUrl = address.AbsoluteUri;

            // We might get redirected to the "speedbump" page before being able to download the file for certain file types.
            // If that happens, we'll need to follow the redirect and then extract the download URL.

            Client.Head(address);

            if (Client.StatusCode == HttpStatusCode.Redirect) {

                // Follow the redirect to the "speedbump" page, where we can extract the direct download URL.

                string locationHeader = Client.ResponseHeaders[HttpResponseHeader.Location];

                Client.Headers.Add(HttpRequestHeader.Referer, address.AbsoluteUri);

                string speedbumpResponse = Client.DownloadString(locationHeader);

                Match contentLinkMatch = Regex.Match(speedbumpResponse, @"content_link"": ""([^ ""]+)");

                if (contentLinkMatch.Success)
                    directDownloadUrl = contentLinkMatch.Groups[1].Value;
                else
                    throw new Exception("Failed to determine direct download URL.");

            }

            return directDownloadUrl;

        }

    }

}