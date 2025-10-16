namespace XCloudRepo.Core;

public class XCloudAccountCore {
    private readonly XDatabase _db = new();
    private string _userLogin = string.Empty;
    private string _userPassword = string.Empty;
    private bool _isAuthorized = false;
    public string UserLogin => _userLogin;
    public bool IsAuthorized => _isAuthorized;

    public Task<bool> RegisterUser(string userDataFormat) {
        string[] dataArray = userDataFormat.Split(':');
        if (dataArray.Length != 2) return Task.FromResult(false);
        
        _userLogin = dataArray[0];
        _userPassword = dataArray[1];
        if (XRegValidator.CheckData(_userLogin, _userPassword) != EDataStatus.Success)
            return Task.FromResult(false);


        if (_db.AddUser(_userLogin, _userPassword).Result != EDataStatus.Success) 
            return Task.FromResult(false);

        Directory.CreateDirectory($"Users/{_userLogin}");
        _isAuthorized = true;
        return Task.FromResult(true);
    }

    public Task<bool> AuthUser(string userDataFormat) {
        string[] dataArray = userDataFormat.Split(':');
        if (dataArray.Length != 2) return Task.FromResult(false);

        if (XRegValidator.CheckData(dataArray[0], dataArray[1]) != EDataStatus.Success)
            return Task.FromResult(false);

        if (_db.UserExists(userDataFormat).Result == EUser.NotExists)
            return Task.FromResult(false);
        
        _userLogin = dataArray[0];
        _userPassword = dataArray[1];
        _isAuthorized = true;
        return Task.FromResult(true);
    }
}