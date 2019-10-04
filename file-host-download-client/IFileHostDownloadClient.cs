using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;

namespace Gsemac {

    public interface IFileHostDownloadClient {

        string Accept { get; set; }
        DecompressionMethods AcceptEncoding { get; set; }
        string AcceptLanguage { get; set; }
        string UserAgent { get; set; }
        CookieContainer Cookies { get; set; }

        event AsyncCompletedEventHandler DownloadFileCompleted;
        event DownloadProgressChangedEventHandler DownloadProgressChanged;

        void DownloadFile(Uri address, string filename);
        void DownloadFile(string address, string filename);

        void DownloadFileAsync(Uri address, string filename);
        void DownloadFileAsync(Uri address, string filename, object userToken);

        Uri GetDirectUri(Uri address);
        string GetDirectUri(string address);

        string GetFilename(Uri address);
        string GetFilename(string address);

    }

}