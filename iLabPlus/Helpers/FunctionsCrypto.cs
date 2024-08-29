using System.IO;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;


namespace iLabPlus.Models.Clases
{
    [Authorize]
    public class FunctionsCrypto
    {

        // ******************************************************************************************************************
        // CIFRADO SIMETRICO (EN AES) : PERMITE ENCRIPTAR Y DESENCRIPTAR
        // ******************************************************************************************************************

        // 256 bits
        private static readonly byte[] Key = {
            0x4E, 0x62, 0x3A, 0x92, 0x68, 0x7F, 0x0C, 0xE5,
            0x1F, 0x86, 0xC4, 0x29, 0x7D, 0x35, 0x9A, 0x20,
            0x8B, 0x5D, 0xA3, 0xF1, 0x14, 0x02, 0x8E, 0xA7,
            0xC9, 0x6F, 0xDB, 0x53, 0x40, 0x1C, 0x75, 0xEE
        };

        // 128 bits
        private static readonly byte[] IV = {
            0xC3, 0xA5, 0xE9, 0x92, 0x3F, 0x8B, 0x74, 0x6D,
            0x21, 0x07, 0x9C, 0x6A, 0xE0, 0x5F, 0xBA, 0xD7
        };


        public static byte[] EncryptAES(string plainText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return msEncrypt.ToArray();
                    }
                }
            }
        }


        public static string DecryptAES(byte[] cipherText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }




        // ******************************************************************************************************************
        // BCrypt : hash irreversible + Salting
        // ******************************************************************************************************************

        private const int WorkFactor = 12; // Ajusta este valor según tus necesidades de seguridad

        public static string HashPassword(string password)
        {
            string salt = BCrypt.Net.BCrypt.GenerateSalt(WorkFactor);
            return BCrypt.Net.BCrypt.HashPassword(password, salt);
        }


        public static Func<string, string, bool> VerifyPasswordImplementation { get; set; }
           = BCrypt.Net.BCrypt.Verify;

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return VerifyPasswordImplementation(password, hashedPassword);
        }

        //public static bool VerifyPassword(string password, string hashedPassword)
        //{
        //    return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        //}
 

    }
}
