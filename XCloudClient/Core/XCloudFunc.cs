using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using XCloudClient.Enums;

namespace XCloudClient.Core;

public class XCloudFunc(Socket socket) {
    public async Task<string[]> DeserializeRootDir(byte[] buffer) {
        int receive = await socket.ReceiveAsync(buffer);
        string json = Encoding.UTF8.GetString(buffer, 0, receive);
        return JsonConvert.DeserializeObject<string[]>(json) ?? [];
    }

    public string ReceiveString(byte[] buffer) {
        return Encoding.UTF8.GetString(buffer, 0, socket.Receive(buffer)).TrimEnd('\0');
    }
    
    public async Task<string> ReceiveStringAsync(byte[] buffer) {
        int receive = await socket.ReceiveAsync(buffer);
        return Encoding.UTF8.GetString(buffer, 0, receive);
    }
    
    
    public EResponseCode ReceiveData(byte[] buffer) {
        socket.Receive(buffer);
        return (EResponseCode)BitConverter.ToInt64(buffer, 0);
    }
    
    public async Task<EResponseCode> ReceiveDataAsync(byte[] buffer) {
        await socket.ReceiveAsync(buffer);
        return (EResponseCode)BitConverter.ToInt64(buffer, 0);
    }
    
    public long ReceiveLong(byte[] buffer) {
        socket.Receive(buffer);
        return BitConverter.ToInt64(buffer, 0);
    }
    
    public async Task<long> ReceiveLongAsync(byte[] buffer) {
        await socket.ReceiveAsync(buffer);
        return BitConverter.ToInt64(buffer, 0);
    }
}