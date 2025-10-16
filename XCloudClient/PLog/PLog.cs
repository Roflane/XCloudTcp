using XCloudClient.Configs;
using XCloudClient.Enums;


public static class PLog {
    private static readonly string _await = "\nPress any key to continue";
    
    public static bool AccoundData(string login, string password) {
        if (login.Length > XRegistrationConfig.MaxDataLength) {
            Log.Red($"Exceeded maximum length of {XRegistrationConfig.MaxDataLength} for login.");
            return false;
        }
        if (password.Length > XRegistrationConfig.MaxDataLength + 1) {
            Log.Red($"Exceeded maximum length of {XRegistrationConfig.MaxDataLength} for password.");
            return false;
        }
        return true;
    }
    
    public static void FileSize(EResponseCode status) {
        switch (status) {
            case EResponseCode.FileSizeOk:
                Log.Green($"File upload request sent.{_await}", true);
                break;
            case EResponseCode.FileSizeOverflow:
                Log.Red($"Response: file size overflow.{_await}", true);
                break;
        }
    }
    
    public static void FileDelete(EResponseCode status) {
        switch (status) {
            case EResponseCode.FileExists:
                Log.Green($"File delete successfully.{_await}", true);
                break;
            case EResponseCode.FileNotExists:
                Log.Red($"File delete unsuccessfully.{_await}", true);
                break;
        }
    }
    
    public static void FileDownload(EResponseCode status, long totalReceived, long remoteFileSize) {
        if (status == EResponseCode.FileTransferComplete && totalReceived == remoteFileSize) {
            Log.Green($"File download successfully.{_await}", true);
        }
        else Log.Red(
                $"File download incomplete or failed. (Received {totalReceived}/{remoteFileSize} bytes, Code: {status}){_await}",
                true
                );
    }

    public static void FileRename(EResponseCode status) {
        if (status == EResponseCode.FileExists) {
            Log.Green($"File renamed successfully.{_await}", true);
        }
        else Log.Red($"File renamed unsuccessfully.{_await}", true);
    }
    
    public static void FileCopy(EResponseCode status) {
        if (status == EResponseCode.FileExists) {
            Log.Green($"File copied successfully.{_await}", true);
        }
        else Log.Red($"File copied unsuccessfully.{_await}", true);
    }
}