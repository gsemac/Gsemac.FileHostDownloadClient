# FileHostDownloadClient
`WebClient`-based download clients for common file hosts targeting .NET Framework 4.0.

## Usage
Usage is very similar to that of `WebClient`:
```c#
IFileHostDownloadClient client = new FileHostDownloadClient();

client.DownloadProgressChanged += (sender, e) => {
  Console.WriteLine("Download progress: " + e.ProgressPercentage);
};

client.DownloadFileCompleted += (sender, e) => {
  Console.WriteLine("Download complete!");
};

Uri address = new Uri("https://drive.google.com/open?id=0BwmD_VLjROrfTHk4NFg2SndKcjQ");
string filename = client.GetFilename(address);

client.DownloadFileAsync(address, filename);
```
`FileHostDownloadClient` acts a generic client that spawns specialized clients when required/available. 

`DropboxDownloadClient` and `GoogleDriveDownloadClient` are registered by default, and additional clients can be registered via `FileHostDownloadClient.RegisterClient`.
## Implementations

### `DropboxDownloadClient`

`DropboxDownloadClient` specializes in downloading files from Dropbox, and accepts both share links and direct download links. 
It is able to follow through Dropbox's "speedbump" page (a verification page that appears when downloading certain file types) automatically.

### `GoogleDriveDownloadClient`

`GoogleDriveDownloadClient` specializes in downloading files from Google Drive, and accepts both share links and direct download links.
It is able to follow through Google Drive's "can't scan this file for viruses" automatically for files that are large enough to trigger it.

Additional implementations can be created by inheriting from `IFileHostDownloadClient` or `FileHostDownloadClientBase` and then registered for use on specific domains via `FileHostDownloadClient.RegisterClient`.
