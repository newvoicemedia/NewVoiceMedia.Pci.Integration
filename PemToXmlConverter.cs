using System;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace NewVoiceMedia.Pci.Integration
{
    internal static class PemToXmlConverter
    {
        public static string Convert(Stream pemData)
        {
            var pemObject = new PemReader(new StreamReader(pemData)).ReadObject();
            RSA key;
            bool isPrivate;
            if (pemObject is AsymmetricCipherKeyPair)
            {
                isPrivate = true;
                key = DotNetUtilities.ToRSA((RsaPrivateCrtKeyParameters)((AsymmetricCipherKeyPair)pemObject).Private);
            }
            else if (pemObject is RsaKeyParameters)
            {
                isPrivate = false;
                key = DotNetUtilities.ToRSA((RsaKeyParameters)pemObject);
            }
            else
            {
                throw new ArgumentException($"object is not PEM encoded RSA key (decoded: {pemObject.GetType().Name})", 
                    nameof(pemData));
            }
            return RsaKeyLoader.ToXmlString(key, isPrivate);
        }
    }
}