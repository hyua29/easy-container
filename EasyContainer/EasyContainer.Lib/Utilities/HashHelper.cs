namespace EasyContainer.Lib.Utilities
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public static class HashHelper
    {
        public static string ToSHA256(string source)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] sourceBytes = Encoding.UTF8.GetBytes(source);
                byte[] hashBytes = sha256Hash.ComputeHash(sourceBytes);
                string hash = BitConverter.ToString(hashBytes);
                hash = hash.Replace("-", String.Empty);
                return hash;
            }
        }
    }
}