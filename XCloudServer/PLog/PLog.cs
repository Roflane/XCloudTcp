public static class PLog {
    public static void Register(bool status, string? ep, string userLogin) {
        if (status) {
            Log.Green($"[{ep}] Client {userLogin} registered successfully");
        }
        else Log.Red($"[{ep}] Client {userLogin} registered unsuccessfully");
    }
    
    public static void Auth(bool status, string? ep, string userLogin) {
        if (status) {
            Log.Green($"[{ep}] Client {userLogin} authorized successfully");
        }
        else Log.Red($"[{ep}] Client authorized unsuccessfully");
    }

    public static void DirectoryViewRoot(bool status, string? ep) {
        if (status) {
            Log.Green($"[{ep}] Request 'DirectoryViewRoot' succeeded.");
        }
        else Log.Red($"[{ep}] Request 'DirectoryViewRoot' unsucceeded.");
    }

    
    public static void DirectoryCreate(bool status, string? ep, string dirName) {
        if (status) {
            Log.Green($"[{ep}] Request 'DirectoryCreate' succeeded: {dirName}.");
        }
        else Log.Red($"[{ep}] Request 'DirectoryCreate' unsucceeded: {dirName}.");
    }
    
    public static void DirectoryDelete(bool status, string? ep, string dirName) {
        if (status) {
            Log.Green($"[{ep}] Request 'DirectoryDelete' succeeded: {dirName}.");
        }
        else Log.Red($"[{ep}] Request 'DirectoryDelete' unsucceeded: {dirName}.");
    }

    public static void DirectoryRename(bool status, string? ep, string oldDirName, string newDirName) {
        if (status) {
            Log.Green($"[{ep}] Request 'DirectoryRename' succeeded: {oldDirName} → {newDirName}.");
        }
        else Log.Red($"[{ep}] Request 'DirectoryRename' unsucceeded: {oldDirName} → {newDirName}.");
    }
    
    public static void FileUpload(bool status, string? ep, string fileName) {
        if (status) {
            Log.Green($"[{ep}] Request 'FileUpload' succeeded: {fileName}.");
        }
        else Log.Red($"[{ep}] Request 'FileUpload' unsucceeded: {fileName}.");
    }
}