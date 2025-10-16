namespace XCloudClient.Data;

public class XCloudData {
    public string[]? LastDirs { get; set; }

    public void TryPrintLastDirs() {
        if (LastDirs!.Length == 0) return; 
        foreach (var dir in LastDirs!) {
            Log.Green(dir);
        }
    }
}