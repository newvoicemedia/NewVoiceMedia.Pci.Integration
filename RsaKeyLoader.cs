using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Linq;
using System.IO;

namespace NewVoiceMedia.Pci.Integration
{
	public static class RsaKeyLoader
	{
		public static RSA FromXmlFile(string path)
		{
			var privateKey = RSA.Create();
			FromXmlString(privateKey, File.ReadAllText(path));
			return privateKey;
		}

		// not always implemented in https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.rsa
		public static void FromXmlString(RSA rsa, string xmlString)
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xmlString);
			if (!xmlDoc.DocumentElement.Name.Equals("RSAKeyValue"))
				throw new ArgumentException(
					$"Invalid XML RSA key (root node is '{xmlDoc.DocumentElement.Name}' instead of 'RSAKeyValue')",
					nameof(xmlString));

			var nodes = xmlDoc.DocumentElement.ChildNodes.Cast<XmlNode>()
				.ToDictionary(n => n.Name, n => Convert.FromBase64String(n.InnerText.Trim()));
			rsa.ImportParameters(new RSAParameters
			{
				Exponent = nodes["Exponent"],
				Modulus = nodes["Modulus"],
				D = nodes.GetValueOrDefault("D"),
				DP = nodes.GetValueOrDefault("DP"),
				DQ = nodes.GetValueOrDefault("DQ"),
				InverseQ = nodes.GetValueOrDefault("InverseQ"),
				P = nodes.GetValueOrDefault("P"),
				Q = nodes.GetValueOrDefault("Q")
			});
		}
		
		public static string ToXmlString(RSA rsa, bool includePrivateParameters)
		{
			var rsaParameters = rsa.ExportParameters(includePrivateParameters);
			var stringBuilder = new StringBuilder();
			stringBuilder.Append("<RSAKeyValue>");
			stringBuilder.Append("<Modulus>" + Convert.ToBase64String(rsaParameters.Modulus) + "</Modulus>");
			stringBuilder.Append("<Exponent>" + Convert.ToBase64String(rsaParameters.Exponent) + "</Exponent>");
			if (includePrivateParameters)
			{
				stringBuilder.Append("<P>" + Convert.ToBase64String(rsaParameters.P) + "</P>");
				stringBuilder.Append("<Q>" + Convert.ToBase64String(rsaParameters.Q) + "</Q>");
				stringBuilder.Append("<DP>" + Convert.ToBase64String(rsaParameters.DP) + "</DP>");
				stringBuilder.Append("<DQ>" + Convert.ToBase64String(rsaParameters.DQ) + "</DQ>");
				stringBuilder.Append("<InverseQ>" + Convert.ToBase64String(rsaParameters.InverseQ) + "</InverseQ>");
				stringBuilder.Append("<D>" + Convert.ToBase64String(rsaParameters.D) + "</D>");
			}
			stringBuilder.Append("</RSAKeyValue>");
			return stringBuilder.ToString();
		}
	}
}