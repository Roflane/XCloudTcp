using XCloudRepo.Configs;

namespace XCloudRepo.Internals;

public class XBuffer {
    public byte[] EnterBuffer { get; set; } = new byte[100];
    public byte[] UserDataBuffer { get; set; } = new byte[XRegistrationConfig.MaxLoginLength + XRegistrationConfig.MaxLoginLength];
    
    public byte[] RequestBuffer { get; set; } = new byte[50];
    
    public byte[] OldDirBuffer { get; set; } = new byte[260];
    public byte[] NewDirBuffer { get; set; } = new byte[260];
    public byte[] FileNameBuffer { get; set; } = new byte[260];
    public byte[] FileSizeBuffer { get; set; } = new byte[sizeof(long)];
    
    public byte[] DirToCreateBuffer { get; set; } = new byte[260];
    public byte[] DirToDeleteBuffer { get; set; } = new byte[260];
    public byte[] DirToUploadBuffer { get; set; } = new byte[260];
    public byte[] FileToUploadBuffer { get; set; } = [];
    public byte[] FileToDownloadBuffer { get; set; } = new byte[260];
    public byte[] FileToDeleteBuffer { get; set; } = new byte[260];
    public byte[] FileToRenameBuffer { get; set; } = new byte[260];
    public byte[] FileToCopyBuffer { get; set; } = new byte[260];
    
    // public byte[] fileBufferToSend { get; set; } = [];
    
    
}