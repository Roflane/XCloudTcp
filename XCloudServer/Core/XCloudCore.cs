namespace XCloudRepo.Core;

public class XCloudCore(string login) {
    public string RootDir => $"C:/XCloud/{login}/";
    
    public bool DirectoryExists(string folder) {
        return Directory.Exists(Path.Combine(RootDir, folder));
    }

    public string[] DirectoryViewRoot() {
        string[] dirs = Directory.GetDirectories(RootDir);
        return dirs.Select(dir => dir.Replace($"{RootDir}", "")).ToArray();
    }
    
    public bool DirectoryCreate(string dir) {
        string targetDir = Path.Combine(RootDir, dir);
        
        if (string.IsNullOrEmpty(dir) || 
            Directory.Exists(targetDir)) return false;
        
        Directory.CreateDirectory(targetDir);
        return true;
    }
    
    public bool DirectoryDelete(string dir) {
        string targetDir = Path.Combine(RootDir, dir);
        
        if (string.IsNullOrEmpty(dir)) return false;
        
        Directory.Delete(targetDir);
        return true;
    }
    
    public bool DirectoryRename(string dir, string newDirName) {
        string targetDir = Path.Combine(RootDir, dir);
        
        if (string.IsNullOrEmpty(dir) || 
            !Directory.Exists(targetDir)) return false;

        Directory.Move(targetDir, $"{RootDir}/{newDirName}");
        return true;
    }
    
    public bool FileUpload(string dir, string fileName, byte[] fileBuffer) {
        try {
            string targetDir = Path.Combine(RootDir, dir, fileName);

            string? directory = Path.GetDirectoryName(targetDir);
            if (string.IsNullOrEmpty(directory)) 
                return false;

            File.WriteAllBytes(targetDir, fileBuffer);
            return true;
        }
        catch { return false; }
    }
    
    public async Task<bool> FileDownload(string dir, byte[] fileBuffer) {
        try {
            string targetDir = Path.Combine(RootDir, dir);
            string? directory = Path.GetDirectoryName(targetDir);
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory)) {
                return false;
            }

            await File.WriteAllBytesAsync(targetDir, fileBuffer);
            return true;
        }
        catch { return false; }
    }
    
    public async Task<bool> FileDelete(string dir) {
        string targetDir = Path.Combine(RootDir, dir);
        
        if (string.IsNullOrEmpty(targetDir) || 
            !File.Exists(dir)) return false;

        await Task.Run(() => File.Delete(dir)); 
        return true;
    }

    public async Task<bool> FileRename(string oldPath, string newPath) {
        string targetOldPath = Path.Combine(RootDir, oldPath);
        string targetNewPath = Path.Combine(RootDir, newPath);

        if (string.IsNullOrEmpty(targetOldPath) || 
            !File.Exists(targetOldPath)) return false;

        await Task.Run(() => File.Move(targetOldPath, targetNewPath));
        return true;
    }

    public async Task<bool> FileCopy(string dir) {
        int n = 0;
        string file = Path.Combine(RootDir, dir);

        string directory = Path.GetDirectoryName(file)!;
        if (string.IsNullOrEmpty(directory)) return false;
        
        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);
        string extension = Path.GetExtension(file);

        string newFilePath = file;

        await Task.Run(() => {
            while (File.Exists(newFilePath)) {
                n++;
                string newFileName = $"{fileNameWithoutExt} Copy ({n}){extension}";
                newFilePath = Path.Combine(directory, newFileName);
            } 
        });
        
        File.Copy(file, newFilePath);
        return true;
    }
}