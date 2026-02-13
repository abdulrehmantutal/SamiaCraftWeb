using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace AlHilal.Models
{
    public class RSA
    {
        public static void GenerateKeys(int Keysize, out string r_publickey, out string r_privateKey)
        {
            r_publickey = string.Empty;
            r_privateKey = string.Empty;

            if (Keysize % 2 != 0 || Keysize < 512)
                throw new Exception("Key should be multiple of two and greater than 512.");

            if (Keysize > 4096)
                throw new Exception("Key size less than 4097.");


            var keyGenrator = new RSA();

            var rsaKeysTypes = new RSAKeysTypes();
            using (var provider = new RSACryptoServiceProvider(Keysize))
            {
                var publicKey = provider.ToXmlString(false);
                var privateKey = provider.ToXmlString(true);
                var publicKeyWithSize = IncludeKeyInEncryptionString(publicKey, Keysize);
                var privateKeyWithSize = IncludeKeyInEncryptionString(privateKey, Keysize);
                rsaKeysTypes.PublicKey = publicKeyWithSize;
                rsaKeysTypes.PrivateKey = privateKeyWithSize;
            }

            r_publickey = rsaKeysTypes.PublicKey;
            r_privateKey = rsaKeysTypes.PrivateKey;


            //var keys = keyGenrator.GenerateKeys(Keysize);


            ////lets take a new CSP with a new 2048 bit rsa key pair
            //var csp = new RSACryptoServiceProvider(Keysize);

            ////how to get the private key
            //var privKey = csp.ExportParameters(true);

            ////and the public key ...
            //var pubKey = csp.ExportParameters(false);

            ////converting the public key into a string representation
            //string pubKeyString;
            //{
            //    //we need some buffer
            //    var sw = new System.IO.StringWriter();
            //    //we need a serializer
            //    var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            //    //serialize the key into the stream
            //    xs.Serialize(sw, pubKey);
            //    //get the string from the stream
            //    pubKeyString = sw.ToString();
            //}

            ////converting the public key into a string representation
            //string privKeyString;
            //{
            //    //we need some buffer
            //    var sw = new System.IO.StringWriter();
            //    //we need a serializer
            //    var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            //    //serialize the key into the stream
            //    xs.Serialize(sw, privKey);
            //    //get the string from the stream
            //    privKeyString = sw.ToString();
            //}

            //r_publickey = pubKeyString;
            //r_privateKey = privKeyString;

            ////converting it back (public key)
            //{
            //    //get a stream from the string
            //    var sr = new System.IO.StringReader(pubKeyString);
            //    //we need a deserializer
            //    var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            //    //get the object back from the stream
            //    pubKey = (RSAParameters)xs.Deserialize(sr);
            //}

            ////converting it back (private key)
            //{
            //    //get a stream from the string
            //    var sr = new System.IO.StringReader(privKeyString);
            //    //we need a deserializer
            //    var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            //    //get the object back from the stream
            //    privKey = (RSAParameters)xs.Deserialize(sr);
            //}


        }



        public string EnCrypt(string key, string text, int keySize)
        {

            //int keySize = 2048;
            string publicKeyXml = "";
            //  GetKeyFromEncryptionString(key, out keySize, out publicKeyXml);

            var encrypted = Encrypt(Encoding.UTF8.GetBytes(text), keySize, key);

            return Convert.ToBase64String(encrypted);
            // //we have a public key ... let's get a new csp and load that key

            // var sr = new System.IO.StringReader(key);
            // //we need a deserializer
            // var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            // //get the object back from the stream
            //var pubKey = (RSAParameters)xs.Deserialize(sr);

            //var csp = new RSACryptoServiceProvider();
            // csp.ImportParameters(pubKey);

            // //we need some data to encrypt
            // //var plainTextData = "foobar";

            // //for encryption, always handle bytes...
            // var bytesPlainTextData = System.Text.Encoding.Unicode.GetBytes(text);

            // //apply pkcs#1.5 padding and encrypt our data 
            // var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);

            // //we might want a string representation of our cypher text... base64 will do
            // string cypherText = Convert.ToBase64String(bytesCypherText);


            // return cypherText;

        }


        public string DeCrypt(string key, string text, int keySize = 2096)
        {
            //int keySize = 4096;
            var decrypted = Decrypt(Convert.FromBase64String(text), keySize, key);
            return Encoding.UTF8.GetString(decrypted);
        }



        private static byte[] Decrypt(byte[] data, int keySize, string publicAndPrivateKeyXml)
        {
            if (data == null || data.Length == 0) throw new ArgumentException("Data are empty", "data");
            if (!IsKeySizeValid(keySize)) throw new ArgumentException("Key size is not valid", "keySize");
            if (String.IsNullOrEmpty(publicAndPrivateKeyXml)) throw new ArgumentException("Key is null or empty", "publicAndPrivateKeyXml");
            using (var provider = new RSACryptoServiceProvider(keySize))
            {
                provider.FromXmlString(publicAndPrivateKeyXml);
                return provider.Decrypt(data, false);
            }
        }

        //public string DeCrypt(string key, string text, int keySize = 2048)
        //{

        //    string publicAndPrivateKeyXml = "";
        //    //  GetKeyFromEncryptionString(key, out keySize, out publicAndPrivateKeyXml);

        //    //Encoding.UTF8.GetBytes(text)
        //     //var decrypted = Decrypt(Encoding.UTF8.GetBytes(text), keySize, key);
        //    var decrypted = Decrypt(Convert.FromBase64String(text), keySize, key);

        //    return Encoding.UTF8.GetString(decrypted);

        //    //  //first, get our bytes back from the base64 string ...
        //    //  var bytesCypherText = Convert.FromBase64String(text);

        //    //  // convert string to Key
        //    //  var sr = new System.IO.StringReader(key);
        //    //  //we need a deserializer
        //    //  var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
        //    //  //get the object back from the stream
        //    //var  privKey = (RSAParameters)xs.Deserialize(sr);

        //    //  //we want to decrypt, therefore we need a csp and load our private key
        //    // var csp = new RSACryptoServiceProvider();
        //    // csp.ImportParameters(privKey);

        //    //  //decrypt and strip pkcs#1.5 padding
        //    //var  bytesPlainTextData = csp.Decrypt(bytesCypherText, false);

        //    //  //get our original plainText back...
        //    //string plainTextData = System.Text.Encoding.Unicode.GetString(bytesPlainTextData);

        //    //return plainTextData;

        //}



        //private static byte[] Decrypt(byte[] data, int keySize, string publicAndPrivateKeyXml)
        //{
        //    if (data == null || data.Length == 0) throw new ArgumentException("Data are empty", "data");
        //    if (!IsKeySizeValid(keySize)) throw new ArgumentException("Key size is not valid", "keySize");
        //    if (String.IsNullOrEmpty(publicAndPrivateKeyXml)) throw new ArgumentException("Key is null or empty", "publicAndPrivateKeyXml");
        //    using (var provider = new RSACryptoServiceProvider(keySize))
        //    {
        //        provider.FromXmlString(publicAndPrivateKeyXml);
        //        return provider.Decrypt(data, false);
        //    }
        //}


        public static byte[] Encrypt(byte[] data, int keySize, string publicKeyXml)
        {
            if (data == null || data.Length == 0) throw new ArgumentException("Data are empty", "data");
            int maxLength = GetMaxDataLength(keySize);
           // if (data.Length > maxLength) throw new ArgumentException(String.Format("Maximum data length is {0}", maxLength), "data");
            if (!IsKeySizeValid(keySize)) throw new ArgumentException("Key size is not valid", "keySize");
            if (String.IsNullOrEmpty(publicKeyXml)) throw new ArgumentException("Key is null or empty", "publicKeyXml");
            using (var provider = new RSACryptoServiceProvider(keySize))
            {
                provider.FromXmlString(publicKeyXml);
                return provider.Encrypt(data, false);
            }
        }



        private static string IncludeKeyInEncryptionString(string publicKey, int keySize)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(keySize.ToString() + "!" + publicKey));
        }


        public class RSAKeysTypes
        {
            public string PublicKey { get; set; }
            public string PrivateKey { get; set; }
        }


        private static int GetMaxDataLength(int keySize)
        {
            //if(_optimalAsymmetricEncryptionPadding)
            //{
            //    return ((keySize – 384) / 8) + 7;
            //}
            int size = ((keySize - 384) / 8) + 37;
            return size;  //((keySize – 384) / 8) + 37;
        }
        private static bool IsKeySizeValid(int keySize)
        {
            return keySize >= 384 && keySize <= 16384 && keySize % 8 == 0;
        }
        private static void GetKeyFromEncryptionString(string rawkey, out int keySize, out string xmlKey)
        {
            keySize = 0;
            xmlKey = "";
            if (rawkey != null && rawkey.Length > 0)
            {
                byte[] keyBytes = Convert.FromBase64String(rawkey);
                var stringKey = Encoding.UTF8.GetString(keyBytes);
                if (stringKey.Contains("!"))
                {
                    var splittedValues = stringKey.Split(new char[] { '!' }, 2);
                    try
                    {
                        keySize = int.Parse(splittedValues[0]);
                        xmlKey = splittedValues[1];
                    }
                    catch (Exception e) { }
                }
                else
                {
                    xmlKey = stringKey;
                }
            }
        }
    }
}