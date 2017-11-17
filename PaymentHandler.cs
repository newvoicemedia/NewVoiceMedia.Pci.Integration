using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NewVoiceMedia.Pci.Integration
{
	public class PaymentHandler
	{
		public const int ProtocolVersion = 1;
		public static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA1;
		public static readonly RSASignaturePadding SignaturePadding = RSASignaturePadding.Pkcs1;
		public static readonly Encoding DigestEncoding = Encoding.UTF8;
		public const int KeySize = 4096;
		private const string TransportProtocol = "https://";

		private readonly string _accountKey;
		private readonly Gateway _gateway;
		private readonly string _hostname;
		private readonly RSA _privateKey;

		public enum Gateway
		{
			SagePay,
			WorldPay,
			SmartPay,
			CyberSource,
			Realex,
			Generic
		}

		public PaymentHandler(string accountKey, Gateway gateway, string hostname, RSA privateKey)
		{
			if (privateKey.KeySize != KeySize)
				throw new ArgumentException($"required key size {KeySize}", nameof(privateKey));
			_accountKey = accountKey;
			_gateway = gateway;
			_hostname = hostname;
			_privateKey = privateKey;
		}

		public async Task<string> MakePayment(string payload, Action<string> progressTracker = null)
		{
			progressTracker = progressTracker ?? (s => { });
			var query = $"/v{ProtocolVersion}/{_accountKey}/Payment/{_gateway}";
			var uri = new Uri($"{TransportProtocol}{_hostname}{query}");
			var contentType = "application/" + (payload.StartsWith('<') ? "xml" : "json");
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Accept.Add(
					new MediaTypeWithQualityHeaderValue(contentType));
				var response = await SendRequest(client, uri, contentType, payload);
				var pollingUrl = response.NextLocation;
				progressTracker(response.Payload);
				if (! SuccessfulStatusCodes.Contains(response.StatusCode))
					throw new Exception("Error has occured on first API request: " + 
					                    $"HTTP response code {response.StatusCode}");
				Uri finalResourceUrl = null;
				while (finalResourceUrl == null)
				{
					response = await SendRequest(client, pollingUrl, contentType);
					finalResourceUrl = response.NextLocation;
					progressTracker(response.Payload);
				}
				return (await SendRequest(client, finalResourceUrl, contentType)).Payload;
			}
		}

		private static readonly ISet<HttpStatusCode> SuccessfulStatusCodes = 
			new HashSet<HttpStatusCode>(new []
			{ 
				HttpStatusCode.OK, 
				HttpStatusCode.Accepted, 
				HttpStatusCode.SeeOther
			});

		private class Response
		{
			public Uri NextLocation { get; set; }
			public string Payload { get; set; }
			public HttpStatusCode StatusCode { get; set; }
		}

		private async Task<Response> SendRequest(HttpClient client, 
		                                         Uri uri, 
		                                         string contentType, 
		                                         string payload = null)
		{
			var timestamp = DateTime.UtcNow;
			var method = payload == null ? HttpMethod.Get : HttpMethod.Post;
			var authorization = Sign(method, uri, payload ?? String.Empty, timestamp);
			client.DefaultRequestHeaders.Date = timestamp;
			client.DefaultRequestHeaders.Authorization = authorization;
			HttpResponseMessage httpResponse;
			if (payload != null)
			{
				var content = new StringContent(payload, Encoding.UTF8, contentType);
				httpResponse = await client.PostAsync(uri, content);
			}
			else
				httpResponse = await client.GetAsync(uri);
			var locationHeader = httpResponse.Headers.Location;
			return new Response
			{
				NextLocation = (locationHeader != null) ? 
					new Uri(locationHeader.AbsoluteUri) : 
					null,
				Payload = await httpResponse.Content.ReadAsStringAsync(),
				StatusCode = httpResponse.StatusCode
			};
		}

		private AuthenticationHeaderValue Sign(
			HttpMethod httpMethod,
			Uri uri,
			string payload,
			DateTime timestamp)
		{
			var joinedString = String.Join(String.Empty,
				httpMethod.Method,
				uri.Host,
				uri.PathAndQuery,
				payload,
				timestamp.ToString("yyyy-MM-ddTHH:mm:ss"));
			var bytes = DigestEncoding.GetBytes(joinedString.ToLowerInvariant());
			var signedDigest = _privateKey.SignData(bytes, HashAlgorithm, SignaturePadding);
			return new AuthenticationHeaderValue(
				"NVM", $"{_accountKey}:{Convert.ToBase64String(signedDigest)}");
		}
	}
}
