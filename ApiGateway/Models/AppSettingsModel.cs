namespace ApiGateway.Models
{
    public class AppSettingsModel
    {
        public string SecretKey { get; set; }
        public string Iss { get; set; }
        public string Aud { get; set; }
    }
}