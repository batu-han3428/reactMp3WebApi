namespace TekrarApp.Model
{
    public class Token
    {
        public string AccessToken { get; set; }
        public DateTime Expiration { get; set; }
        public string RefreshToken { get; set; }
        public string ConfirmToken { get; set; }
    }
}
