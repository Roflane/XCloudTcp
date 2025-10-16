namespace XCloudClient.Enums;

public enum EResponseCode : long {
    FileSizeOk = 0x73000,
    FileSizeOverflow,
    DirExists,
    DirNotExists,
    FileExists,
    FileNotExists,
    FileTransferComplete,
    FileTransferFailed
}