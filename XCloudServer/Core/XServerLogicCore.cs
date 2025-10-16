using System.Net;
using System.Net.Sockets;
using System.Text;
using XCloudRepo.Configs;
using XCloudRepo.Enums;
using XCloudRepo.Internals;
using XCloudRepo.ResponseHandler;
using XCloudServer.Configs;

namespace XCloudRepo.Core;

public class XServerLogicCore(Socket client, XCloudFunc func, XBuffer xb, XResponseHandler rh) {
    public bool ViewRootDirectory(XCloudCore core) {
        bool jsonTreeStatus = func.SerializeRootDir(client, core) > 0;
        if (!jsonTreeStatus) return false;
        
        PLog.DirectoryViewRoot(jsonTreeStatus, 
            client.RemoteEndPoint!.ToString());
        return true;
    }

    public async Task<bool> CreateDirectoryAsync(XCloudCore core) {
        string dirToCreate = await func.ReceiveStringAsync(client, xb.DirToCreateBuffer);
        if (string.IsNullOrEmpty(dirToCreate)) return false;
        
        PLog.DirectoryCreate(core.DirectoryCreate(dirToCreate), 
            client.RemoteEndPoint!.ToString(), dirToCreate);
        return true;
    }
    
    public async Task<bool> DeleteDirectoryAsync(XCloudCore core) {
        string dirDelete = await func.ReceiveStringAsync(client, xb.DirToDeleteBuffer);
        if (string.IsNullOrEmpty(dirDelete)) return false;
        
        PLog.DirectoryDelete(core.DirectoryDelete(dirDelete), 
            client.RemoteEndPoint!.ToString(), dirDelete);
        return true;
    }

    public async Task<bool> RenameDirectoryAsync(XCloudCore core) {
        string oldDir = await func.ReceiveStringAsync(client, xb.OldDirBuffer);
        string newDir = await func.ReceiveStringAsync(client, xb.NewDirBuffer);
        if (string.IsNullOrEmpty(oldDir) || string.IsNullOrEmpty(newDir)) return false;
        
        PLog.DirectoryRename(core.DirectoryRename(oldDir, newDir), client.RemoteEndPoint!.ToString(), oldDir, newDir);
        return true;
    }


    public bool UploadFile(XCloudCore core) {
        string cloudDir = func.ReceiveString(client, xb.DirToUploadBuffer);
        if (!rh.DirectoryExists(core, cloudDir, client,
                () => { Log.Red("Directory doesn't exist."); })) 
            return false;
                            
        string fileName = func.ReceiveString(client, xb.FileNameBuffer);
        if (fileName == XReservedData.InvalidName) return false;
                            
        long fileSize = func.ReceiveLong(client, xb.FileSizeBuffer);
        if (!rh.FileSize(fileSize, client,
                () => { Log.Red("File size overflow."); })) 
            return false;
        
        xb.FileToUploadBuffer = new byte[fileSize];
        client.Receive(xb.FileToUploadBuffer);
        PLog.FileUpload(core.FileUpload(cloudDir, fileName, xb.FileToUploadBuffer), 
            client.RemoteEndPoint!.ToString(), fileName);
        return true;
    }
    
    public bool UploadFileParallel(XCloudCore core) {
        string cloudDir = func.ReceiveString(client, xb.DirToUploadBuffer);
        if (!rh.DirectoryExists(core, cloudDir, client,
                () => { Log.Red("Directory doesn't exist."); })) 
            return false;
                            
        string fileName = func.ReceiveString(client, xb.FileNameBuffer);
        if (fileName == XReservedData.InvalidName) return false;
                            
        long fileSize = func.ReceiveLong(client, xb.FileSizeBuffer);
        if (!rh.FileSize(fileSize, client,
                () => { Log.Red("File size overflow."); })) 
            return false;
        
        xb.FileToUploadBuffer = new byte[fileSize];
        client.Receive(xb.FileToUploadBuffer);

        Thread th = new Thread(() => {
            PLog.FileUpload(core.FileUpload(cloudDir, fileName, xb.FileToUploadBuffer), 
                client.RemoteEndPoint!.ToString(), fileName);    
        }); 
        th.Start();
        return true;
    }

    public bool DownloadFile(XCloudCore core) {
        string remotePath = func.ReceiveString(client, xb.FileToDownloadBuffer);
        string fileToDownload = Path.Combine(core.RootDir, remotePath);
        Log.Red(fileToDownload);
    
        if (!rh.LocalFileExists(fileToDownload, client, () => Log.Red("File doesn't exist."))) {
            return false;
        }

        FileInfo fi = new FileInfo(fileToDownload);
        
        byte[] fileNameBytes = Encoding.UTF8.GetBytes(fi.Name);
        client.Send(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(fileNameBytes.Length)));
        client.Send(fileNameBytes);
        client.Send(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(fi.Length)));

        byte[] buffer = new byte[XCommonConfig.ChunkSize];
        long totalSent = 0;

        using (FileStream fs = new FileStream(fileToDownload, FileMode.Open, FileAccess.Read, FileShare.Read)) {
            int bytesRead;
            while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0) {
                client.Send(buffer, 0, bytesRead, SocketFlags.None);
                totalSent += bytesRead;
            }
        }

        Log.Green($"File download done ({fi.Name}, {fi.Length} bytes)");
        client.Send(BitConverter.GetBytes((long)EResponseCode.FileTransferComplete));
        return true;
    }
    
    public bool DownloadFileParallel(XCloudCore core) {
        string remotePath = func.ReceiveString(client, xb.FileToDownloadBuffer);
        string fileToDownload = Path.Combine(core.RootDir, remotePath);
        Log.Red(fileToDownload);
    
        if (!rh.LocalFileExists(fileToDownload, client, () => Log.Red("File doesn't exist."))) {
            return false;
        }

        FileInfo fi = new FileInfo(fileToDownload);
        
        byte[] fileNameBytes = Encoding.UTF8.GetBytes(fi.Name);
        client.Send(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(fileNameBytes.Length)));
        client.Send(fileNameBytes);
        client.Send(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(fi.Length)));

        byte[] buffer = new byte[XCommonConfig.ChunkSize];
        long totalSent = 0;

        Thread downloadThread = new Thread(() => {
            using (FileStream fs = new FileStream(fileToDownload, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                int bytesRead;
                while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0) {
                    client.Send(buffer, 0, bytesRead, SocketFlags.None);
                    totalSent += bytesRead;
                }
            }
            
            Log.Green($"File download done ({fi.Name}, {fi.Length} bytes)");
            client.Send(BitConverter.GetBytes((long)EResponseCode.FileTransferComplete));
        });
        downloadThread.Start();
        return true;
    }

    
    public async Task<bool> DeleteFileAsync(XCloudCore core) {
        string fileToDelete = $"{core.RootDir}/{await func.ReceiveStringAsync(client, xb.FileToDownloadBuffer)}";
        if (!await rh.LocalFileExistsAsync(fileToDelete, client, () =>  { Log.Red("File doesn't exist."); })) {
            return false;
        }

        if (await core.FileDelete(fileToDelete)) {
            Log.Green("Request 'FileDelete' succeeded");
        } else Log.Green("Request 'FileDelete' unsucceeded");
        return true;
    }

    public async Task<bool> RenameFileAsync(XCloudCore core) {
        string oldFileName = $"{core.RootDir}/{await func.ReceiveStringAsync(client, xb.FileToRenameBuffer)}";
        if (!await rh.LocalFileExistsAsync(oldFileName, client, () =>  { Log.Red("File doesn't exist."); })) 
            return false;
        
        string newFileName = $"{core.RootDir}/{await func.ReceiveStringAsync(client, xb.FileToRenameBuffer)}";
        
        Log.Red(oldFileName);
        Log.Red(newFileName);
                            
        if (await core.FileRename(oldFileName, newFileName)) {
            Log.Green("Request 'FileRename' succeeded.");
        } else Log.Red("Request 'FileRename' unsucceeded.");
        return true;
    }

    public async Task<bool> CopyFileAsync(XCloudCore core) {
        string fileToCopy = $"{core.RootDir}/{await func.ReceiveStringAsync(client, xb.FileToCopyBuffer)}";
        if (!await rh.LocalFileExistsAsync(fileToCopy, client, () =>  { Log.Red("File doesn't exist."); }))
            return false;
                            
        if (await core.FileCopy(fileToCopy)) {
            Log.Green("Request 'FileCopy' succeeded.");
        } else Log.Green("Request 'FileCopy' unsucceeded.");
        return true;
    }
}