using System.Net.Sockets;
using XCloudRepo.Configs;
using XCloudRepo.Core;
using XCloudRepo.Enums;

namespace XCloudRepo.ResponseHandler;

public class XResponseHandler {
    public bool DirectoryExists(XCloudCore core, string dir, Socket client, Action log) {
        if (!core.DirectoryExists(dir)) {
            client.Send(BitConverter.GetBytes((long)EResponseCode.DirNotExists));
            log.Invoke();
            return false;
        }
        client.Send(BitConverter.GetBytes((long)EResponseCode.DirExists));
        return true;
    }

    
    public async Task<bool> DirectoryExistsAsync(XCloudCore core, string dir, Socket client, Action log) {
        if (!core.DirectoryExists(dir))
        {
            log.Invoke();
            await client.SendAsync(BitConverter.GetBytes((long)EResponseCode.DirNotExists));
            return false;
        }

        await client.SendAsync(BitConverter.GetBytes((long)EResponseCode.DirExists));
        return true;
    }

    
    public bool FileSize(long fileSize, Socket client, Action log) {
        if (fileSize > XCloudServerConfig.MaxFileBufferSize) {
            log.Invoke();
            client.Send(BitConverter.GetBytes((long)EResponseCode.FileSizeOverflow));
            return false;
        }
        client.Send(BitConverter.GetBytes((long)EResponseCode.FileSizeOk));
        return true;
    }
    
    public async Task<bool> FileSizeAsync(long fileSize, Socket client, Action log) {
        if (fileSize > XCloudServerConfig.MaxFileBufferSize) {
            log.Invoke();
            await client.SendAsync(BitConverter.GetBytes((long)EResponseCode.FileSizeOverflow));
            return false;
        }
        await client.SendAsync(BitConverter.GetBytes((long)EResponseCode.FileSizeOk));
        return true;
    }

    public bool LocalFileExists(string file, Socket client, Action log) {
        if (!File.Exists(file)) {
            log.Invoke();
            client.Send(BitConverter.GetBytes((long)EResponseCode.FileNotExists));
            return false;
        } 
        client.Send(BitConverter.GetBytes((long)EResponseCode.FileExists));
        return true;
    }
    
    
    public async Task<bool> LocalFileExistsAsync(string file, Socket client, Action log) {
        if (!File.Exists(file)) {
            log.Invoke();
            await client.SendAsync(BitConverter.GetBytes((long)EResponseCode.FileNotExists));
            return false;
        }
        await client.SendAsync(BitConverter.GetBytes((long)EResponseCode.FileExists));
        return true;
    }
    
    // public bool FileName(string fileName, Socket client, Action log) {
    //     if (fileName == XReservedData.InvalidName) {
    //         log.Invoke();
    //         client.Send(BitConverter.GetBytes((long)EResponseCode.FileNotExists));
    //         return false;
    //     }
    //     client.Send(BitConverter.GetBytes((long)EResponseCode.FileExists));
    //     return true;
    // }
}