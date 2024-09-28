namespace IdentityServer.Models.Settings
{
    public class AppSettings
    {
        public string WebApiSecret { get; set; }
        public string ReverseProxySecret { get; set; }
        public List<User> Users { get; set; }
    }

    public class User()
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }
}
