using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;

namespace Gsemac {

    public class WebClientEx :
        WebClient {

        // Public members

        public WebClientEx() {

            Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            AcceptLanguage = "en-US,en;q=0.5";
            UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:64.0) Gecko/20100101 Firefox/64.0";

        }

        public virtual string Accept {
            get {
                return Headers[HttpRequestHeader.Accept];
            }
            set {
                Headers.Set(HttpRequestHeader.Accept, value);
            }
        }
        public virtual string AcceptLanguage {
            get {
                return Headers[HttpRequestHeader.AcceptLanguage];
            }
            set {
                Headers.Set(HttpRequestHeader.AcceptLanguage, value);
            }
        }
        public bool AllowAutoRedirect { get; set; } = true;
        public DecompressionMethods AutomaticDecompression { get; set; } = DecompressionMethods.Deflate | DecompressionMethods.GZip;
        public CookieContainer Cookies { get; set; } = new CookieContainer();
        public string Method { get; set; }
        public string Referer {
            get {
                return Headers[HttpRequestHeader.Referer];
            }
            set {
                Headers.Set(HttpRequestHeader.Referer, value);
            }
        }
        public HttpStatusCode StatusCode { get; set; }
        public virtual string UserAgent {
            get {
                return Headers[HttpRequestHeader.UserAgent];
            }
            set {
                Headers.Set(HttpRequestHeader.UserAgent, value);
            }
        }

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
        public string Post(Uri address, NameValueCollection formData) {
            return Encoding.UTF8.GetString(UploadValues(address, "POST", formData));
        }
        public string Post(string address, NameValueCollection formData) {
            return Post(new Uri(address), formData);
        }

        // Protected members

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

        // Private members

        private void _getDataFromWebResponse(WebResponse response) {

            if (response is HttpWebResponse httpResponse) {

                Cookies.Add(httpResponse.Cookies);

                StatusCode = httpResponse.StatusCode;

            }

        }

    }

}