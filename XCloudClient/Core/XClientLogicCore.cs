using System.Net;
using System.Net.Sockets;
using System.Text;
using XCloudClient.Configs;
using XCloudClient.Enums;
using XCloudClient.Internals;
using XCloudClient.ResponseHandler;

namespace XCloudClient.Core;

public class XClientLogicCore(Socket socket, XCloudFunc func, XBuffer xb, XResponseHandler rh) {
    public async Task<bool> ViewRootDirectoryAsync(byte[] buffer) {
        string[] jsonTree = await func.DeserializeRootDir(buffer);
        if (jsonTree.Length == 0) return false;
        
        foreach (var dir in jsonTree) {
            Log.Blue($"{dir.Replace(@"\", "/")}\n");
        }
        Log.Blue("Enter any key to return false.", true);
        return true;
    }

    public async Task<bool> CreateDirectoryAsync(byte[] buffer) {
        Log.Green("Enter remote directory to create: ");
        string remoteDir = Console.ReadLine()!;
        if (string.IsNullOrEmpty(remoteDir)) return false;
        
        await socket.SendAsync(Encoding.UTF8.GetBytes(remoteDir));
        return true;
    }
    
    public async Task<bool> DeleteDirectoryAsync() {
        Log.Green("Enter remote directory to delete: ");
        string remoteDirToDelete = Console.ReadLine()!;
        if (string.IsNullOrEmpty(remoteDirToDelete)) return false;
        
        await socket.SendAsync(Encoding.UTF8.GetBytes(remoteDirToDelete));
        return true;
    }

    public async Task<bool> RenameDirectoryAsync() {
        Log.Green("Enter current directory name: ");
        string oldDirName = Console.ReadLine()!;
        if (string.IsNullOrEmpty(oldDirName)) return false;
                        
        Log.Green("Enter new directory name: ");
        string newDirName = Console.ReadLine()!;
        if (string.IsNullOrEmpty(newDirName)) return false;
                        
        await socket.SendAsync(Encoding.UTF8.GetBytes(oldDirName));
        await socket.SendAsync(Encoding.UTF8.GetBytes(newDirName));
        return true;
    }
    
    public bool UploadFile() {
        Log.Green("Enter remote directory to upload a file: ");
        string cloudDir = Console.ReadLine()!;
        if (string.IsNullOrEmpty(cloudDir)) return false;
        socket.Send(Encoding.UTF8.GetBytes(cloudDir));

        EResponseCode remoteDirStatus = func.ReceiveData(xb.StatusBuffer);
        if (remoteDirStatus == EResponseCode.DirNotExists) {
            Log.Red("Directory doesn't exist.", true);
            return false;
        }

        Log.Green("Enter file path to upload: ");
        string filePath = Console.ReadLine()!;

        try {
            FileInfo fi = new FileInfo(filePath);
            if (!rh.LocalFileExists(fi.Exists, fi.Name, () => {
                    Log.Red("File doesn't exist.", true);
                })) return false;
            
            socket.Send(BitConverter.GetBytes(fi.Length));
            socket.Send(File.ReadAllBytes(filePath));
        }
        catch (Exception ex) {
            Log.Red($"Error: {ex.Message}");
        }
        return true;
    }
    
    public bool UploadFileParallel() {
        Log.Green("Enter remote directory to upload a file: ");
        string cloudDir = Console.ReadLine()!;
        if (string.IsNullOrEmpty(cloudDir)) return false;
        socket.Send(Encoding.UTF8.GetBytes(cloudDir));

        EResponseCode remoteDirStatus = func.ReceiveData(xb.StatusBuffer);
        if (remoteDirStatus == EResponseCode.DirNotExists) {
            Log.Red("Directory doesn't exist.", true);
            return false;
        }

        Log.Green("Enter file path to upload: ");
        string filePath = Console.ReadLine()!.Replace("\"", "");
 
        try {
            FileInfo fi = new FileInfo(filePath);
            if (!rh.LocalFileExists(fi.Exists, fi.Name, () => {
                    Log.Red("File doesn't exist.", true);
                })) return false;
            
            socket.Send(BitConverter.GetBytes(fi.Length));
            Thread th = new Thread(() => {
                socket.Send(File.ReadAllBytes(filePath));
            });
            th.Start();
        }
        catch (Exception ex) {
            Log.Red($"Error: {ex.Message}");
        }
        return true;
    }
    
    
    public bool DownloadFile() {    
        Log.Green("Enter remote directory to download a file: ");
        string remoteFile = Console.ReadLine()!.Trim();
        if (string.IsNullOrEmpty(remoteFile)) return false;
        
        socket.Send(Encoding.UTF8.GetBytes(remoteFile));
        
        EResponseCode remoteFileStatus = func.ReceiveData(xb.StatusBuffer);
        if (remoteFileStatus != EResponseCode.FileExists) {
            Log.Red("File not found on server.", true);
            return false;
        }
        
        Log.Green("Enter your directory to download a file: ");
        string localDir = Console.ReadLine()!.Trim();
        if (string.IsNullOrEmpty(localDir)) return false;
        
        byte[] lenBuffer = new byte[sizeof(int)];
        int bytesReadLen = socket.Receive(lenBuffer);
        if (bytesReadLen != sizeof(int)) {
            Log.Red("Failed to receive filename length.", true);
            return false;
        }
        int fileNameLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenBuffer));
        
        byte[] nameBuffer = new byte[fileNameLength];
        int totalNameReceived = 0;
        while (totalNameReceived < fileNameLength) {
            int received = socket.Receive(nameBuffer, totalNameReceived, fileNameLength - totalNameReceived, SocketFlags.None);
            if (received <= 0) break;
            totalNameReceived += received;
        }

        string remoteFileName = Encoding.UTF8.GetString(nameBuffer, 0, totalNameReceived).TrimEnd('\0');
        
        byte[] sizeBuffer = new byte[sizeof(long)];
        int bytesReadSize = socket.Receive(sizeBuffer);
        if (bytesReadSize != sizeof(long)) {
            Log.Red("Failed to receive file size.", true);
            return false;
        }
        long remoteFileSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt64(sizeBuffer));
        
        string localPath = Path.Combine(localDir, remoteFileName);
        Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);
        
        byte[] chunk = new byte[1024 * 16];
        long totalReceived = 0;
        
        using (FileStream fs = new FileStream(localPath, FileMode.Create, FileAccess.Write)) {
            while (totalReceived < remoteFileSize) {
                int receivedBytes = socket.Receive(chunk, 0, chunk.Length, SocketFlags.None);
                if (receivedBytes <= 0) break;
                fs.Write(chunk, 0, receivedBytes);
                totalReceived += receivedBytes;
            }
        }
        
        Log.Green($"File downloaded successfully ({remoteFileName}, {remoteFileSize} bytes)");
        return true;
    }
    
     public bool DownloadFileParallel() {    
        Log.Green("Enter remote directory to download a file: ");
        string remoteFile = Console.ReadLine()!.Trim();
        if (string.IsNullOrEmpty(remoteFile)) return false;
        
        socket.Send(Encoding.UTF8.GetBytes(remoteFile));
        
        EResponseCode remoteFileStatus = func.ReceiveData(xb.StatusBuffer);
        if (remoteFileStatus != EResponseCode.FileExists) {
            Log.Red("File not found on server.", true);
            return false;
        }
        
        Log.Green("Enter your directory to download a file: ");
        string localDir = Console.ReadLine()!.Trim();
        if (string.IsNullOrEmpty(localDir)) return false;
        
        byte[] lenBuffer = new byte[sizeof(int)];
        int bytesReadLen = socket.Receive(lenBuffer);
        if (bytesReadLen != sizeof(int)) {
            Log.Red("Failed to receive filename length.", true);
            return false;
        }
        int fileNameLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenBuffer));
        
        byte[] nameBuffer = new byte[fileNameLength];
        int totalNameReceived = 0;
        while (totalNameReceived < fileNameLength) {
            int received = socket.Receive(nameBuffer, totalNameReceived, fileNameLength - totalNameReceived, SocketFlags.None);
            if (received <= 0) break;
            totalNameReceived += received;
        }

        string remoteFileName = Encoding.UTF8.GetString(nameBuffer, 0, totalNameReceived).TrimEnd('\0');
        
        byte[] sizeBuffer = new byte[sizeof(long)];
        int bytesReadSize = socket.Receive(sizeBuffer);
        if (bytesReadSize != sizeof(long)) {
            Log.Red("Failed to receive file size.", true);
            return false;
        }
        long remoteFileSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt64(sizeBuffer));
        
        string localPath = Path.Combine(localDir, remoteFileName);
        Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);
        
        byte[] chunk = new byte[1024 * 16];
        long totalReceived = 0;

        Thread thread = new Thread(() => {
            using FileStream fs = new FileStream(localPath, FileMode.Create, FileAccess.Write);
            while (totalReceived < remoteFileSize) {
                int receivedBytes = socket.Receive(chunk, 0, chunk.Length, SocketFlags.None);
                if (receivedBytes <= 0) break;
                fs.Write(chunk, 0, receivedBytes);
                totalReceived += receivedBytes;
            }
        });
        thread.Start();
        return true;
    }
     

    public async Task<bool> DeleteFileAsync() {
        Log.Green("Enter remote directory to delete a file: ");
        string fileToDelete = Console.ReadLine()!;
        if (string.IsNullOrEmpty(fileToDelete)) return false;
                        
        await socket.SendAsync(Encoding.UTF8.GetBytes(fileToDelete));
        EResponseCode remoteFileToDeleteStatus = await func.ReceiveDataAsync(xb.StatusBuffer);
        PLog.FileDelete(remoteFileToDeleteStatus);
        return true;
    }

    public async Task<bool> RenameFileAsync() {
        Log.Green("Enter remote old directory to rename a file name: ");
        string oldFileName = Console.ReadLine()!;
        if (string.IsNullOrEmpty(oldFileName)) return false;
        
        Log.Green("Enter remote new directory to rename a file name: ");
        string newFileName = Console.ReadLine()!;
        if (string.IsNullOrEmpty(newFileName)) return false;
        
        await socket.SendAsync(Encoding.UTF8.GetBytes(oldFileName));
        EResponseCode oldFileNameStatus = await func.ReceiveDataAsync(xb.StatusBuffer);
        await socket.SendAsync(Encoding.UTF8.GetBytes(newFileName));
        PLog.FileRename(oldFileNameStatus);
        return true;
    }

    public async Task<bool> CopyFileAsync() {
        Log.Green("Enter remote old directory to copy a file name: ");
        string fileNameToCopy = Console.ReadLine()!;
        if (string.IsNullOrEmpty(fileNameToCopy)) return false;
        
        await socket.SendAsync(Encoding.UTF8.GetBytes(fileNameToCopy));
        EResponseCode fileNameToCopyStatus = await func.ReceiveDataAsync(xb.StatusBuffer);
        PLog.FileCopy(fileNameToCopyStatus);
        return true;
    }
}