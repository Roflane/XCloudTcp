using System.Net.Sockets;
using System.Text;
using XCloudClient.Configs;
using XCloudClient.Enums;

namespace XCloudClient.ResponseHandler;

public class XResponseHandler(Socket socket) {
    public bool RemoteDir(EResponseCode status, Action log) {
        if (status == EResponseCode.DirNotExists) {
            socket.Send(BitConverter.GetBytes((long)EResponseCode.DirNotExists));
            log.Invoke();
            return false;
        }
        socket.Send(BitConverter.GetBytes((long)EResponseCode.DirNotExists));
        return true;
    }
    
    public bool LocalFileExists(bool status, string name, Action log) {
        if (!status)  {
            socket.Send(Encoding.UTF8.GetBytes(XReservedData.InvalidName));
            log.Invoke();
            return false;
        }
        socket.Send(Encoding.UTF8.GetBytes(name));
        return true;
    }

    public bool FileSize(EResponseCode status, string fileDir, Action log) {
        if (status == EResponseCode.FileSizeOverflow) {
            socket.Send(BitConverter.GetBytes((long)EResponseCode.FileSizeOverflow));
            return false;
        }
        log.Invoke();
        return true;
    }
}