using System.Security.Cryptography;

namespace AuthApiBackend.Utilities
{

    public static class GenerateCode
    {

        public static string GenerateVerificationCode()
        {
            int code = RandomNumberGenerator.GetInt32(10000000, 99999999);

            return code.ToString();
        }

        public static string GenerateToken()
        {
            byte[] tokenData = new byte[32];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenData);
            }

            return Convert.ToBase64String(tokenData);
        }

        public static string GenerateRetreshToken()
        {
            byte[] tokenData = new byte[32];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenData);
            }

            return Convert.ToBase64String(tokenData)
                           .Replace("+", "-")
                           .Replace("/", "_")
                           .TrimEnd('='); ;
        }

        public static string GenerateAccountNumber(string? lastAccountNumber)
        {
            string takelast2 = DateTime.UtcNow.ToString("yy");

            int sequence = 1;

            if (!string.IsNullOrEmpty(lastAccountNumber))
            {
                string lastSevenDigits = lastAccountNumber.Substring(2);
                if (int.TryParse(lastSevenDigits, out int lastNumber))
                {
                    sequence = lastNumber + 1;
                }
            }

            return $"{takelast2}{sequence:D7}";
        }

        public static string TemporaryPassword(int length = 9)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@<>?+=%";

            char[] password = new char[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[4];

                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(bytes);
                    int index = (int)(BitConverter.ToUInt32(bytes, 0) % (uint)chars.Length);
                    password[i] = chars[index];
                }
            }

            return new string(password);
        }

    }

}