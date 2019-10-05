using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Gsemac {

    public interface IFileHostDownloadClient :
        IDisposable {

        event AsyncCompletedEventHandler DownloadFileCompleted;
        event DownloadProgressChangedEventHandler DownloadProgressChanged;
        event DownloadStringCompletedEventHandler DownloadStringCompleted;
        event OpenReadCompletedEventHandler OpenReadCompleted;

        void DownloadFile(Uri address, string filename);
        void DownloadFileAsync(Uri address, string filename, object userToken);
        string DownloadString(Uri address);
        void DownloadStringAsync(Uri address, object userToken);
        Stream OpenRead(Uri address);
        void OpenReadAsync(Uri address, object userToken);

        Uri GetDirectUri(Uri address);

        string GetFilename(Uri address);

    }

}