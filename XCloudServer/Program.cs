using XCloudRepo.Server;

namespace XCloudRepo;

static class Program {
    static async Task Main() {
        XCloudServer server = new("127.0.0.1:4773"); 
        Console.Title = $"[{server.Ip}:{server.Port}] XCloud server";
        await server.BeginListening();
    }
}