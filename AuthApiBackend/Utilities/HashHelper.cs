using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace AuthApiBackend.Utilities
{

    public static class HashHelper
    {
        
        public static string HashId(string id)
        {

            byte[] keybytes = Convert.FromBase64String(Environment.GetEnvironmentVariable("JWT_KEY")!);

            byte[] databytes = Encoding.UTF8.GetBytes(id);

            using (var hmac = new HMACSHA256(keybytes))
            {
                var hashedDataBytes = hmac.ComputeHash(databytes);

                return BitConverter.ToString(hashedDataBytes).Replace("-", "").ToLowerInvariant();
            }

        }

        public static string HashPassword(string password)
        {

            return new PasswordHasher<string>().HashPassword(Environment.GetEnvironmentVariable("SECRET_KEY")!
                                                             , password);

        }

        public static PasswordVerificationResult VerifyHashPassword(string hashedPassword, string password)
        {

            return new PasswordHasher<string>().VerifyHashedPassword(Environment.GetEnvironmentVariable("SECRET_KEY")!
                                                                     , hashedPassword, password);

        }

    }

}
