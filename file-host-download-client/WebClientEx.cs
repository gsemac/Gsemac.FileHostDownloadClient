using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Gsemac {

    internal class WebClientEx :
        WebClient {

        public string Method { get; set; }
        public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.Deflate | DecompressionMethods.GZip;
        public CookieContainer Cookies { get; set; } = new CookieContainer();
        public bool AllowAutoRedirect { get; set; } = true;

        public HttpStatusCode StatusCode { get; set; }

        public string Get(Uri address) {
            return DownloadString(address);
        }
        public string Get(string address) {
            return Get(new Uri(address));
        }
        public string Head(Uri address) {

            string methodBefore = Method;

            Method = "HEAD";

            string result = DownloadString(address);

            Method = methodBefore; // restore previous value

            return result;

        }
        public string Head(string address) {
            return Head(new Uri(address));
        }

        protected override WebRequest GetWebRequest(Uri address) {

            WebRequest request = base.GetWebRequest(address);

            if (request is HttpWebRequest httpRequest) {

                httpRequest.CookieContainer = Cookies;
                httpRequest.AutomaticDecompression = AutomaticDecompression;
                httpRequest.AllowAutoRedirect = AllowAutoRedirect;

                if (!string.IsNullOrEmpty(Method))
                    httpRequest.Method = Method;

            }

            return request;

        }
        protected override WebResponse GetWebResponse(WebRequest request) {

            WebResponse response = base.GetWebResponse(request);

            _getDataFromWebResponse(response);

            return response;

        }
        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result) {

            WebResponse response = base.GetWebResponse(request, result);

            _getDataFromWebResponse(response);

            return response;

        }

        private void _getDataFromWebResponse(WebResponse response) {

            if (response is HttpWebResponse httpResponse) {

                Cookies.Add(httpResponse.Cookies);

                StatusCode = httpResponse.StatusCode;

            }

        }

    }

}