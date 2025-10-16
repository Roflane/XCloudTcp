using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace XCloudRepo.Core;

public class XCloudFunc {
    public void SendAccountStatus(Socket client, bool status) {
        client.Send(Encoding.UTF8.GetBytes(status.ToString()));
    }
    
    public int SerializeRootDir(Socket client, XCloudCore core) {
        string json = JsonConvert.SerializeObject(core.DirectoryViewRoot());
        return client.Send(Encoding.UTF8.GetBytes(json));
    }
    
    public string ReceiveString(Socket client, byte[] buffer) {
        return Encoding.UTF8.GetString(buffer, 0, client.Receive(buffer));
    }
    
    public async Task<string> ReceiveStringAsync(Socket client, byte[] buffer) {
        int bytesReceived = await client.ReceiveAsync(buffer, SocketFlags.None);
        return Encoding.UTF8.GetString(buffer, 0, bytesReceived);
    }
    
    public long ReceiveLong(Socket client, byte[] buffer) {
        client.Receive(buffer);
        return BitConverter.ToInt64(buffer, 0);
    }
    
    public async Task<long> ReceiveLongAsync(Socket client, byte[] buffer) {
        await client.ReceiveAsync(buffer);
        return BitConverter.ToInt64(buffer, 0);
    }
}