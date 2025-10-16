namespace XCloudServer;

static class Program {
    static async Task Main() {
        XCloudTcp.Server.XCloudServer server = new("192.168.31.126:4773"); 
        Console.Title = $"[{server.Ip}:{server.Port}] XCloud server";
        await server.BeginListening();
    }
}