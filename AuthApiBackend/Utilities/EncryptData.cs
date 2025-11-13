using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Security.Cryptography;
using System.Text;

namespace AuthApiBackend.Utilities
{

    public static class EncryptData
    {

        public static string Encrypt(string password)
        {


            using var aes = Aes.Create();
            var key = Environment.GetEnvironmentVariable("AES_KEY")!;

            aes.Key = Convert.FromBase64String(key);

           
            aes.GenerateIV();

            using var ms = new MemoryStream();
            
            ms.Write(aes.IV, 0, aes.IV.Length);

            using (var cryptoStream = new CryptoStream(ms, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write))
            using (var writer = new StreamWriter(cryptoStream, Encoding.UTF8))
            {

                writer.Write(password);

            } 

            return Convert.ToBase64String(ms.ToArray());

        }


        public static string Decrypt(string cipherText)
        {

            byte[] fullCipher = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(Environment.GetEnvironmentVariable("AES_KEY")!);

            int ivLength = aes.BlockSize / 8;
            byte[] iv = new byte[ivLength];
            byte[] cipherBytes = new byte[fullCipher.Length - ivLength];

            Array.Copy(fullCipher, iv, ivLength);
            Array.Copy(fullCipher, ivLength, cipherBytes, 0, cipherBytes.Length);
            aes.IV = iv;

            using var ms = new MemoryStream(cipherBytes);
            using var cryptoStream = new CryptoStream(ms, aes.CreateDecryptor(aes.Key, aes.IV), CryptoStreamMode.Read);
            using var reader = new StreamReader(cryptoStream, Encoding.UTF8);

            return reader.ReadToEnd();

        }

    }

}