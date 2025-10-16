namespace XCloudClient;

static class Program {
    static async Task Main() {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        Client.XCloudClient client = new("127.0.0.1:4773");
        Console.Title = $"[{client.Ip}:{client.Port}] XCloud Client";
        
        await Task.Run(() => client.Run());
    }
}