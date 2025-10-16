using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;
using XCloudClient.Configs;
using XCloudClient.Core;
using XCloudClient.Internals;
using XCloudClient.Menu;
using XCloudClient.ResponseHandler;
using XCloudClient.User;

namespace XCloudClient.Client;

public class XCloudClient : IDisposable {
    private readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private IPEndPoint? _ep;
    
    private readonly UserData _userData = new();
    
    public string Ip => _ep!.Address.ToString();
    public string Port => _ep!.Port.ToString();
    
    public XCloudClient(string ipPort) {
        TryInitializeClient(ipPort);
    }

    private void TryInitializeClient(string ipPort) {
        try {
            _ep = IPEndPoint.Parse(ipPort);
            _socket.Connect(_ep);
        }
        catch (Exception ex) {
            Log.Red(ex.Message, true);
            Environment.Exit(1);
        }
    }
    
    [DoesNotReturn]
    public async Task Run() {
        XResponseHandler rh = new(_socket);
        XCloudFunc func = new(_socket);
        XBuffer xb = new();
        
        XAccountCore accountCore = new XAccountCore(_socket, _userData, xb);
        XClientLogicCore clientLogicCoreImpl = new(_socket, func, xb, rh);
        
        while (true) {
            if (!_userData.IsAuthorized) await accountCore.Run();
            else {
                Console.Clear();
                XMenu.PrintCloudOptions();

                Log.Green("\nEnter option: ");
                string option = Console.ReadLine()!;
                _socket.Send(Encoding.UTF8.GetBytes(option));

                byte[] buffer = new byte[1024 * 10];
                
                switch (option) {
                    case XCloudClientConfig.DirectoryViewRoot:
                        await clientLogicCoreImpl.ViewRootDirectoryAsync(buffer);
                        break;
                    case XCloudClientConfig.DirectoryCreate:
                        await clientLogicCoreImpl.CreateDirectoryAsync(buffer);
                        break;
                    case XCloudClientConfig.DirectoryDelete:
                        await clientLogicCoreImpl.DeleteDirectoryAsync();
                        break;
                    case XCloudClientConfig.DirectoryRename:
                        await clientLogicCoreImpl.RenameDirectoryAsync();
                        break;
                    case XCloudClientConfig.FileUpload:
                        try { clientLogicCoreImpl.UploadFileParallel(); }
                        catch (Exception ex) { Log.Red(ex.Message, true); }
                        break;
                    case XCloudClientConfig.FileDownload:
                        try { clientLogicCoreImpl.DownloadFileParallel(); }
                        catch (Exception ex) { Log.Red(ex.Message, true); }
                        break;
                    case XCloudClientConfig.FileDelete:
                        await clientLogicCoreImpl.DeleteFileAsync();
                        break;
                    case XCloudClientConfig.FileRename:
                        await clientLogicCoreImpl.RenameFileAsync();
                        break;
                    case XCloudClientConfig.FileCopy:
                        await clientLogicCoreImpl.CopyFileAsync();
                        break;
                }
            }
        }
    }

    public void Dispose() {
        _socket.Dispose();
    }
}