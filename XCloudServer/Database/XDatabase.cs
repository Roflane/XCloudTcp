public class XDatabase {
    private readonly string _fileName = "XCloudUsers.bin"; 
    
    public async Task<EUser> UserExists(string userDataFormat) {
        if (!File.Exists(_fileName))
            return EUser.NotExists;

        var lines = await File.ReadAllLinesAsync(_fileName);

        foreach (var line in lines) {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string plainText = Decryptor.XorDecrypt(line);
            
            if (plainText.StartsWith(userDataFormat))
                return EUser.Exists;
        }

        return EUser.NotExists;
    }
    
    public async Task<EDataStatus> AddUser(string login, string password) {
        if (await UserExists(login) == EUser.Exists) 
            return EDataStatus.Invalid;

        string format = $"{login}:{password}";
        string encryptedData = Decryptor.XorEncrypt(format);

        await File.AppendAllTextAsync(_fileName, encryptedData + Environment.NewLine);
        return EDataStatus.Success;
    }
}