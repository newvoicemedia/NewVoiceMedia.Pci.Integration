using System;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Linq;
using System.IO;
using System.Reflection;

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
			var nodes = xmlDoc.DocumentElement.ChildNodes.Cast<XmlNode>()
				.ToDictionary(n => n.Name, n => Convert.FromBase64String(n.InnerText.Trim()));
			object parameters = new RSAParameters(); //boxing needed to set values
			foreach (var node in nodes)
				parameters.GetType().GetField(node.Key).SetValue(parameters, node.Value);
			rsa.ImportParameters((RSAParameters)parameters);
		}
		
		public static string ToXmlString(RSA rsa, bool includePrivateParameters)
		{
			var parameterValues = rsa.ExportParameters(includePrivateParameters);
			var stringBuilder = new StringBuilder();
			const string rootTagName = "RSAKeyValue";
			stringBuilder.Append($"<{rootTagName}>");
			var parameters = parameterValues.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
			foreach (var parameter in parameters.Where(p => !p.IsNotSerialized || includePrivateParameters))
			{
				var base64 = Convert.ToBase64String((byte[]) parameter.GetValue(parameterValues));
				stringBuilder.Append($"<{parameter.Name}>{base64}</{parameter.Name}>");
			}
			stringBuilder.Append($"</{rootTagName}>");
			return stringBuilder.ToString();
		}
	}
}