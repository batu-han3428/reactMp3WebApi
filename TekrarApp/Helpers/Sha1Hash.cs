using System.Security.Cryptography;
using System.Text;

namespace TekrarApp.Helpers
{
    public class Sha1Hash
    {
        private SHA1 _sha1;
        public Sha1Hash()
        {
            _sha1 = new SHA1CryptoServiceProvider();
        }
        public string Encrypt(string data)
        {
            return Convert.ToBase64String(_sha1.ComputeHash(Encoding.UTF8.GetBytes(data)));
        }
    }
}
