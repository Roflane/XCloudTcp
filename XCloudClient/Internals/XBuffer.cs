using XCloudClient.Enums;

namespace XCloudClient.Internals;

public class XBuffer {
    public byte[] EnterStatusBuffer { get; set; } = new byte[5];
    public byte[] StatusBuffer { get; set; } = new byte[sizeof(EResponseCode)];
    
    public byte[] RemoteFileNameBuffer { get; set; } = new byte[260];
    
    public byte[] RemoteFileBuffer { get; set; } = [];
    
}