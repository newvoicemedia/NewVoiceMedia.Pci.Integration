using System;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace NewVoiceMedia.Pci.Integration
{
	public class Utils
	{
		[Command("Writes new generated key pair to given paths")]
		public static string GenerateKeys(string privateKeyPath = "private-key.xml", string publicKeyPath = "public-key.xml")
		{
			var key = RSA.Create();
			key.KeySize = PaymentHandler.KeySize;
			File.WriteAllText(privateKeyPath, RsaKeyLoader.ToXmlString(key, includePrivateParameters: true));
			File.WriteAllText(publicKeyPath, RsaKeyLoader.ToXmlString(key, includePrivateParameters: false));

			return $"Files written to {Path.GetDirectoryName(Path.GetFullPath(privateKeyPath))}\n" +
				   $"	{privateKeyPath}: keep this one secure and never pass to anyone\n" + 
				   $"	{publicKeyPath}:  pass this one to NewVoiceMedia operations";
		}

		[Command("Checks whether given xml keys match each other (true) or not (false)")]
		public static bool CheckKeys(string privateKeyPath = "private-key.xml", string publicKeyPath = "public-key.xml")
		{
			var privateKey = RsaKeyLoader.FromXmlFile(privateKeyPath);
			var publicKey = RsaKeyLoader.FromXmlFile(publicKeyPath);
			var testData = PaymentHandler.DigestEncoding.GetBytes("NewVoiceMedia Rulezzz");
			var signature = privateKey.SignData(testData, PaymentHandler.HashAlgorithm, PaymentHandler.SignaturePadding);
			return publicKey.VerifyData(testData, signature, PaymentHandler.HashAlgorithm, PaymentHandler.SignaturePadding);
		}

		[Command("Signs given string with given private key and returns base64 encoded signature")]
		public static string SignData(string data, string privateKeyPath = "private-key.xml")
		{
			var privateKey = RsaKeyLoader.FromXmlFile(privateKeyPath);
			var signature = privateKey.SignData(PaymentHandler.DigestEncoding.GetBytes(data), PaymentHandler.HashAlgorithm, PaymentHandler.SignaturePadding);
			return Convert.ToBase64String(signature);
		}

		[Command("Checks whether given signature matches given pair of data and public key (true) or not (false)")]
		public static bool VerifySignature(string signatureBase64, string data, string publicKeyPath = "public-key.xml")
		{
			var publicKey = RsaKeyLoader.FromXmlFile(publicKeyPath);
			var signature = Convert.FromBase64String(signatureBase64);
			var dataBytes = PaymentHandler.DigestEncoding.GetBytes(data);
			return publicKey.VerifyData(dataBytes, signature, PaymentHandler.HashAlgorithm, PaymentHandler.SignaturePadding);
		}

		[Command("Convert PEM keys to XML keys (uses BouncyCastle)")]
		public static void ConvertKeys(string privateKeyPath = "private-key.pem", string publicKeyPath = "public-key.pem")
		{
			object Read(string path) => 
				new Org.BouncyCastle.OpenSsl.PemReader(new StreamReader(File.OpenRead(path))).ReadObject();
			var privateKey = (Org.BouncyCastle.Crypto.Parameters.RsaPrivateCrtKeyParameters)((Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair)Read(privateKeyPath)).Private;
			var publicKey  = (Org.BouncyCastle.Crypto.Parameters.RsaKeyParameters)Read(publicKeyPath);

			var privateKeyXml = RsaKeyLoader.ToXmlString(Org.BouncyCastle.Security.DotNetUtilities.ToRSA(privateKey), true);
			var publicKeyXml = RsaKeyLoader.ToXmlString(Org.BouncyCastle.Security.DotNetUtilities.ToRSA(publicKey), false);

			string GetXmlPath(string pemPath) => 
				Path.Combine(Path.GetDirectoryName(pemPath), Path.GetFileNameWithoutExtension(pemPath)+".xml");
			File.WriteAllText(GetXmlPath(privateKeyPath), privateKeyXml);
			File.WriteAllText(GetXmlPath(publicKeyPath), publicKeyXml);
		}

		[Command("Sends payment request to server and returns response")]
		public static async Task<string> SendRequest(
			string accountKey, 
			PaymentHandler.Gateway gateway,
			string payloadPath = "payload.xml",
			string privateKeyPath = "private-key.xml",
			string hostname = "paymentapi.contact-world.net")
		{
			var payload = File.ReadAllText(payloadPath).Trim();
			var privateKey = RsaKeyLoader.FromXmlFile(privateKeyPath);
			var handler = new PaymentHandler(accountKey, gateway, hostname, privateKey);
			return await handler.MakePayment(payload, Console.WriteLine);
		}

		[Command("Relays payment request from standard input to server and returns response")]
		public static async Task<string> RelayRequest(
			string accountKey, 
			PaymentHandler.Gateway gateway,
			string privateKeyPath = "private-key.xml",
			string hostname = "paymentapi.contact-world.net")
		{
			var payload = new StringBuilder();
			string line;
			while ((line = Console.ReadLine()) != null)
				payload.AppendLine(line);
			var privateKey = RsaKeyLoader.FromXmlFile(privateKeyPath);
			var handler = new PaymentHandler(accountKey, gateway, hostname, privateKey);
			return await handler.MakePayment(payload.ToString(), Console.WriteLine);
		}

		[Command("Checks external IP address of current machine")]
		public static string CheckIp(string provider = "checkip.dyndns.org")
		{
			using (var client = new System.Net.Http.HttpClient())
			{
				var response = client.GetStringAsync("http://" + provider).Result;
				return Regex.Match(response, @"\d+\.\d+\.\d+\.\d+").Value;
			}
		}
	}
}
