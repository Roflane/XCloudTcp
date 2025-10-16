using XCloudRepo.Configs;

public static class XRegValidator {
    public static EDataStatus CheckData(string login, string password) {
        if (login.Length > XRegistrationConfig.MaxLoginLength ||
            password.Length > XRegistrationConfig.MaxPasswordLength) {
            return EDataStatus.DataLengthExceeded;
        }
        return EDataStatus.Success;
    }
}