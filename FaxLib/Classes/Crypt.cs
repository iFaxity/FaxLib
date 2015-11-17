using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FaxLib {
    /// <summary>
    /// Defines a key or a blocks size in an AES encryption
    /// </summary>
    public enum KeyBlockSize {
        _128 = 128,
        _160 = 160,
        _192 = 192,
        _224 = 224,
        _256 = 256
    };
    /// <summary>
    /// Defines which hash algorithm to use when hashing
    /// </summary>
    public enum HashType {
        MD5,
        SHA1,
        SHA256,
        SHA512,
        SHA3
    }
    /// <summary>
    /// Defines the bit size of SHA3
    /// </summary>
    public enum SHA3BitSize {
        _224 = 224,
        _256 = 256,
        _384 = 384,
        _512 = 512
    }

    /// <summary>
    /// A class that contains properties for AES cryption.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public class Cryptor {
        #region Properties
        /// <summary>
        /// Key size to use in symmetric algorithm
        /// </summary>
        public int KeySize { get; private set; }
        /// <summary>
        /// Block size to use in symmetric algorithm
        /// </summary>
        public int BlockSize { get; private set; }
        /// <summary>
        /// Initialization vector to use in symmetric algorithm
        /// </summary>
        public byte[] IV { get; private set; }
        /// <summary>
        /// Key to use in symmetric algorithm
        /// </summary>
        public byte[] Key { get; private set; }
        /// <summary>
        /// Ciphermode to use in symmetric algorithm
        /// </summary>
        public CipherMode Mode { get { return CipherMode.CBC; } }
        #endregion

        /// <summary>
        /// Initializes a new symmetric Cryptor class
        /// </summary>
        /// <param name="password">The cryptor password</param>
        /// <param name="salt">The cryptor salt</param>
        /// <param name="keySize">The cryptor key size</param>
        /// <param name="blockSize">The cryptor block size</param>
        /// <param name="encType">The encryption type (only symmetric)</param>
        public Cryptor(string password, string salt, KeyBlockSize keySize = KeyBlockSize._128, KeyBlockSize blockSize = KeyBlockSize._128) {
            // The salt bytes must be at least 8 bytes.
            if(salt.Length < 8)
                throw new Exception("The salt needs to be at least 8 characters long or more.");

            var rfc = new Rfc2898DeriveBytes(password, Encoding.ASCII.GetBytes(salt));
            Key = rfc.GetBytes((int)keySize / 8);
            IV = rfc.GetBytes((int)blockSize / 8);
            KeySize = (int)keySize;
            BlockSize = (int)blockSize;
        }
    }

    /// <summary>
    /// Class for cryptiong an hashing strings. Useuful for passwords
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static class Crypt {
        #region Crypt
        /// <summary>
        /// Encrypts a string in AES
        /// </summary>
        /// <param name="text">Input string</param>
        /// <param name="password">Input password</param>
        /// <param name="salt">Input Salt</param>
        public static string Encrypt(string text, string password, string salt) {
            return Encrypt(text, new Cryptor(password, salt));
        }
        /// <summary>
        /// Encrypts a string
        /// </summary>
        /// <param name="text">Input string</param>
        /// <param name="cryptor">Cryptor</param>
        public static string Encrypt(string text, Cryptor cryptor) {
            if(string.IsNullOrEmpty(text))
                throw new ArgumentException("The parameter is null or empty.","text");
            else if(cryptor == null)
                throw new Exception("Crypor is not defined");
            // Setup encryptor and encrypt the message
            using(var ms = new MemoryStream()) {
                var crypt = new RijndaelManaged() {
                    KeySize = cryptor.KeySize,
                    BlockSize = cryptor.BlockSize,
                    Key = cryptor.Key,
                    IV = cryptor.IV,
                    Mode = cryptor.Mode
                };

                using(var cs = new CryptoStream(ms, crypt.CreateEncryptor(), CryptoStreamMode.Write)) {
                    using(var sw = new StreamWriter(cs))
                        sw.Write(text);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        /// <summary>
        /// Decrypts a string
        /// </summary>
        /// <param name="text">Input string</param>
        /// <param name="password">Input password</param>
        /// <param name="salt">Input Salt</param>
        public static string Decrypt(string text, string password, string salt) {
            return Decrypt(text, new Cryptor(password, salt));
        }
        /// <summary>
        /// Decrypts a string
        /// </summary>
        /// <param name="text">Input string</param>
        /// <param name="cryptor">Cryptor</param>
        public static string Decrypt(string text, Cryptor cryptor) {
            if(string.IsNullOrEmpty(text))
                throw new ArgumentException("The parameter is null or empty.", "text");
            else if(cryptor == null)
                throw new Exception("Crypor is not defined");

            var plainText = "";
            var baseText = Convert.FromBase64String(text);

            using(var ms = new MemoryStream(baseText)) {
                var crypt = new RijndaelManaged();
                crypt.KeySize = cryptor.KeySize;
                crypt.BlockSize = cryptor.BlockSize;
                crypt.Key = cryptor.Key;
                crypt.IV = cryptor.IV;
                crypt.Mode = cryptor.Mode;

                using(var cs = new CryptoStream(ms, crypt.CreateDecryptor(), CryptoStreamMode.Read)) {
                    using(StreamReader sr = new StreamReader(cs))
                        plainText = sr.ReadToEnd();
                }
            }
            return plainText;
        }
        #endregion

        #region File
        /// <summary>
        /// Decrypts the contents of a file
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="password">Password to encrypt with</param>
        /// <param name="salt">Salt to apply to password</param>
        public static string DecryptFile(string path, string password, string salt) {
            return Decrypt(File.ReadAllText(path), password, salt);
        }
        /// <summary>
        /// Decrypts the contents of a file
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="cryptor">Cryptor information</param>
        public static string DecryptFile(string path, Cryptor cryptor) {
            return Decrypt(File.ReadAllText(path), cryptor);
        }
        /// <summary>
        /// Encrypts the contents of a file
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="password">Password to encrypt with</param>
        /// <param name="salt">Salt to apply to password</param>
        public static void EncryptFile(string path, string contents, string password, string salt) {
            File.WriteAllText(path, Encrypt(contents, password, salt));
        }
        /// <summary>
        /// Encrypts the contents of a file
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="cryptor">Cryptor information</param>
        public static void EncryptFile(string path, string contents, Cryptor cryptor) {
            File.WriteAllText(path, Encrypt(contents, cryptor));
        }
        #endregion

        #region Hash
        // Creates a hash
        static byte[] ComputeHash(byte[] bytes, byte[] salt, HashType type, SHA3BitSize bitSize) {
            // Create a hash buffer with a salt
            if(salt != null && salt.Length > 0) {
                byte[] full = new byte[bytes.Length + salt.Length];
                for(int i = 0; i < bytes.Length; i++)
                    full[i] = bytes[i];
                for(int i = 0; i < salt.Length; i++)
                    full[bytes.Length + i] = salt[i];
                bytes = full;
            }
            // Identify the hash algorithm type
            HashAlgorithm crypt;
            switch(type) {
                case HashType.SHA1:
                    crypt = new SHA1Managed();
                    break;
                case HashType.SHA256:
                    crypt = new SHA256Managed();
                    break;
                case HashType.SHA512:
                    crypt = new SHA512Managed();
                    break;
                case HashType.SHA3:
                    crypt = new SHA3.SHA3Managed((int)bitSize);
                    break;
                case HashType.MD5:
                    crypt = new MD5CryptoServiceProvider();
                    break;
                default:
                    throw new ArgumentException("Parameter invalid", "type");
            }
            return crypt.ComputeHash(bytes);
        }

        /// <summary>
        /// Creates a new hash
        /// </summary>
        /// <param name="text">Text to hash</param>
        /// <param name="type">Hash algorithm to use</param>
        public static string Hash(string text, HashType type = HashType.SHA256) {
            return Hash(text, Encoding.UTF8, type);
        }
        /// <summary>
        /// Creates a new salted hash
        /// </summary>
        /// <param name="text">Text to hash</param>
        /// <param name="salt">Salt to hash with</param>
        /// <param name="type">Hash algorithm to use</param>
        public static string Hash(string text, string salt, HashType type = HashType.SHA256) {
            return Hash(text, salt, Encoding.UTF8, type);
        }
        /// <summary>
        /// Creates a new hash
        /// </summary>
        /// <param name="text">Text to hash</param>
        /// <param name="e">Encoding to use</param>
        /// <param name="type">Hash algorithm type</param>
        /// <param name="bitSize">[ONLY IN SHA3] SHA3 algorithm bit size</param>
        public static string Hash(string text, Encoding e = null, HashType type = HashType.SHA256, SHA3BitSize bitSize = SHA3BitSize._256) {
            if(string.IsNullOrEmpty(text))
                throw new ArgumentException("The parameter is null or empty.", "text");
            else if(e == null)
                e = Encoding.Default;

            // Create a hexadecimal hash as a string
            var hash = ComputeHash(e.GetBytes(text), null, type, bitSize);
            var sb = new StringBuilder();
            for(int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("X2"));
            return sb.ToString();
        }
        /// <summary>
        /// Creates a new salted hash
        /// </summary>
        /// <param name="text">Text to hash</param>
        /// <param name="salt">Salt to hash with</param>
        /// <param name="e">Encoding to use</param>
        /// <param name="type">Hash algorithm type</param>
        /// <param name="bitSize">[ONLY IN SHA3] SHA3 algorithm bit size</param>
        public static string Hash(string text, string salt, Encoding e, HashType type = HashType.SHA256, SHA3BitSize bitSize = SHA3BitSize._256) {
            if(string.IsNullOrEmpty(text))
                throw new ArgumentException("The parameter is null or empty.", "text");
            else if(e == null)
                e = Encoding.Default;

            // Create a hexadecimal hash as a string
            var hash = ComputeHash(e.GetBytes(text), e.GetBytes(salt), type, bitSize);
            var sb = new StringBuilder();
            for(int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("X2"));
            return sb.ToString();
        }

        /// <summary>
        /// Creates a new hash
        /// </summary>
        /// <param name="bytes">Byte array to hash</param>
        /// <param name="type">Hash algorithm to use</param>
        /// <param name="bitSize">[ONLY IN SHA3] SHA3 algorithm bit size</param>
        public static byte[] Hash(byte[] bytes, HashType type = HashType.SHA256, SHA3BitSize bitSize = SHA3BitSize._256) {
            return ComputeHash(bytes, null, type, bitSize);
        }
        /// <summary>
        /// Creates a new salted hash
        /// </summary>
        /// <param name="bytes">Text to hash</param>
        /// <param name="salt">Salt to hash with</param>
        /// <param name="type">Hash algorithm to use</param>
        /// <param name="bitSize">[ONLY IN SHA3] SHA3 algorithm bit size</param>
        public static byte[] Hash(byte[] bytes, byte[] salt, HashType type = HashType.SHA256, SHA3BitSize bitSize = SHA3BitSize._256) {
            return ComputeHash(bytes, salt, type, bitSize);
        }

        /// <summary>
        /// Verifies if 2 hashes match
        /// </summary>
        /// <param name="hash1">First hash</param>
        /// <param name="hash2">Second hash</param>
        public static bool VerifyHash(byte[] hash1, byte[] hash2) {
            return hash1.Equals(hash2);
        }
        /// <summary>
        /// Verifies if 2 hashes match
        /// </summary>
        /// <param name="hash1">First hash</param>
        /// <param name="hash2">Second hash</param>
        public static bool VerifyHash(string hash1, string hash2) {
            return hash1.Equals(hash2);
        }

        /// <summary>
        /// Hashes a strong password using PBKDF2
        /// </summary>
        /// <param name="password">Password to hash</param>
        /// <param name="salt">Salt to hash with</param>
        /// <param name="e">Encoding of password</param>
        /// <param name="length">Length of hash</param>
        /// <param name="iterations">Iterations of hash</param>
        public static string HashPassword(string password, string salt, Encoding e, int length = 30, int iterations = 1000) {
            var derive = new Rfc2898DeriveBytes(password, e.GetBytes(salt), iterations);
            var hash = derive.GetBytes(length);
           
            // Create a hexadecimal hash as a string
            var sb = new StringBuilder();
            for(int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("X2"));
            return sb.ToString();
        }
        /// <summary>
        /// Hashes a strong password
        /// </summary>
        /// <param name="password">Password to hash</param>
        /// <param name="salt">Salt to hash with</param>
        /// <param name="length">Length of hash</param>
        /// <param name="iterations">Iterations of hash</param>
        public static byte[] HashPassword(byte[] password, byte[] salt, int length = 30, int iterations = 1000) {
            var derive = new Rfc2898DeriveBytes(password, salt, iterations);
            return derive.GetBytes(length);
        }
        #endregion

        /// <summary>
        /// Creates a secure random byte array
        /// </summary>
        /// <param name="length">Length of array</param>
        public static byte[] RandomBytes(int length) {
            byte[] buffer = new byte[length];
            new RNGCryptoServiceProvider().GetBytes(buffer);
            return buffer;
        }
    }
}
