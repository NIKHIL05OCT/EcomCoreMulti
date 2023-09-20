namespace EcomAuthentication.AppCodes
{
    public interface IAppConfig
    {
        string GetConnectionString(string connectionName);

        IConfigurationSection GetConfigurationSection(string Key);
    }
}
