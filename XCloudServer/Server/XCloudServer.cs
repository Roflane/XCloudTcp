using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using XCloudRepo.Configs;
using XCloudRepo.Core;
using XCloudRepo.Internals;
using XCloudRepo.ResponseHandler;

namespace XCloudRepo.Server;

public class XCloudServer : IDisposable {
    private readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private IPEndPoint _ep = null!;
    
    public string Ip => _ep.Address.ToString();
    public string Port => _ep.Port.ToString();
    
    public XCloudServer(string ipPort) {
        TryInitializeServer(ipPort);
        PrintInfo();
    }

    private void TryInitializeServer(string ipPort) {
        try {
            _ep = IPEndPoint.Parse(ipPort);
            _socket.Bind(_ep);
            _socket.Listen();
        }
        catch (Exception ex) {
            Log.Red(ex.Message, true);
            Environment.Exit(1);
        }
    }
    
    private void PrintInfo() {
        Log.Red($"""
                 _____________________________________
                 Address: {_ep.Address}
                 Port: {_ep.Port}
                 Address Family: {_ep.AddressFamily}
                 _____________________________________
                 """);
    } 
    
    public async Task BeginListening() {
        var listener = Task.Run(async () => {
            Log.Blue("Waiting for client...");
        
            while (true) {
                try {
                    Socket client = await _socket.AcceptAsync();
                    
                    string clientLog = $"[{client.RemoteEndPoint}] Connected at {DateTime.Now}";
                    Log.Green($"\n{clientLog}");

                    await Task.Run(() => HandleClientAsync(client)); 
                }
                catch (Exception ex) {
                    Log.Red($"Accept error: {ex.Message}");
                    break;
                }
            }
        });
        await listener;
    }
    
    private async Task HandleClientAsync(Socket client) {
        XCloudAccountCore accountManager = new();
        XResponseHandler rh = new();
        XCloudFunc func = new();
        XBuffer xb = new();
        
        XServerLogicCore serverCoreLogicImpl = new(client, func, xb, rh);
        try {
            while (client.Connected) {
                if (!accountManager.IsAuthorized) {
                    string enterRequest = Encoding.UTF8.GetString(xb.EnterBuffer, 0, client.Receive(xb.EnterBuffer));
                    string userDataFormat = Encoding.UTF8.GetString(xb.UserDataBuffer, 0, client.Receive(xb.UserDataBuffer));

                    switch (enterRequest) {
                        case XCloudServerConfig.Register:
                            bool registerStatus = accountManager.RegisterUser(userDataFormat).Result;
                            PLog.Register(registerStatus, client.RemoteEndPoint!.ToString(), accountManager.UserLogin);
                            break;
                        case XCloudServerConfig.Auth:
                            bool authStatus = accountManager.AuthUser(userDataFormat).Result;
                            PLog.Auth(authStatus, client.RemoteEndPoint!.ToString(), accountManager.UserLogin);
                            break;
                    }
                    func.SendAccountStatus(client, accountManager.IsAuthorized);
                }
                else {
                    XCloudCore core = new(accountManager.UserLogin);
                    
                    string request = Encoding.UTF8.GetString(xb.RequestBuffer, 0, client.Receive(xb.RequestBuffer));
                    switch (request) {
                        case XCloudServerConfig.DirectoryViewRoot:
                            serverCoreLogicImpl.ViewRootDirectory(core);
                            break;
                        case XCloudServerConfig.DirectoryCreate:
                            await serverCoreLogicImpl.CreateDirectoryAsync(core);
                            break;
                        case XCloudServerConfig.DirectoryDelete:
                            await serverCoreLogicImpl.DeleteDirectoryAsync(core);
                            break;
                        case XCloudServerConfig.DirectoryRename:
                            await serverCoreLogicImpl.RenameDirectoryAsync(core);
                            break;
                        case XCloudServerConfig.FileUpload:
                            serverCoreLogicImpl.UploadFileParallel(core);
                            break;
                        case XCloudServerConfig.FileDownload:
                            try { serverCoreLogicImpl.DownloadFileParallel(core); }
                            catch (Exception ex) { Log.Red(ex.Message); }
                            break;
                        case XCloudServerConfig.FileDelete:
                            await serverCoreLogicImpl.DeleteFileAsync(core);
                            break;
                        case XCloudServerConfig.FileRename:
                            await serverCoreLogicImpl.RenameFileAsync(core);
                            break;
                        case XCloudServerConfig.FileCopy:
                            await serverCoreLogicImpl.CopyFileAsync(core);
                            break;
                    }
                }
            }
        }
        catch (Exception ex) {
            Log.Red(ex.Message);
        }
        finally {
            string clientLog = $"Client {client.RemoteEndPoint} disconnected at {DateTime.Now}";
            Log.Red($"[{DateTime.Now}] {clientLog}\n");
            client.Close();
        }
    }

    public void Dispose() {
        _socket.Close();
        _socket.Dispose();
    }
}