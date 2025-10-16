using System.Net.Sockets;
using System.Text;
using XCloudClient.Internals;
using XCloudClient.Menu;
using XCloudClient.User;

namespace XCloudClient.Core;

public class XAccountCore(Socket socket, UserData userData, XBuffer xb) {
    public async Task<bool> Run() {
        XMenu.PrintEnterOptions();
                
        Log.Green("\nEnter option: ");
        string option = Console.ReadLine()!;
        if (string.IsNullOrEmpty(option)) return false;
        
        Log.Green("Enter login: ");
        string login = Console.ReadLine()!;
        if (string.IsNullOrEmpty(login)) return false;
        
        Log.Green("Enter password: ");
        string password = Console.ReadLine()!;
        if (string.IsNullOrEmpty(password)) return false;
        
        if (!PLog.AccoundData(login, password)) return false;
        
        await socket.SendAsync(Encoding.UTF8.GetBytes(option));
        await socket.SendAsync(Encoding.UTF8.GetBytes($"{login}:{password}"));
        
        int enterStatusData = await socket.ReceiveAsync(xb.EnterStatusBuffer);
        string enterStatus = Encoding.UTF8.GetString(xb.EnterStatusBuffer, 0, enterStatusData);
        if (enterStatus == "False") return false;
        
        userData.Login = login;
        userData.IsAuthorized = true;
        Log.Green($"Welcome to XCloud, {userData.Login}!\n", true);
        return true;
    }
}