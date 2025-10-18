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
    }
}
