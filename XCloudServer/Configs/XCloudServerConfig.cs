namespace XCloudRepo.Configs;

public static class XCloudServerConfig {
    public const int MaxFileBufferSize = 1024 * 100 * 1024;
    public const int chunkSize = 1024 * 16;
    
    public const string Register = "Register";
    public const string Auth = "Auth";
    
    public const string DirectoryViewRoot = "dvr";
    public const string DirectoryCreate = "dc";
    public const string DirectoryDelete = "dd";
    public const string DirectoryRename = "dr";
    
    public const string FileDownload = "fd";
    public const string FileUpload = "fu";
    public const string FileDelete = "frm";
    public const string FileRename = "fr";
    public const string FileCopy = "fc";
}