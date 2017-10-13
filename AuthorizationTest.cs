using System;
using System.Net.Http;
using System.Security.Cryptography;
// using Newtonsoft.Json;
// using NUnit.Framework;

// for demonstration purposes - to avoid uneccessary dependencies, Newtonsoft.Json and NUnit.Framework are commented out
namespace NewVoiceMedia.Pci.Integration
{
	// [TestFixture]
	public class AuthorizationTest
	{
		// [Test]
		public void CorrectAuthorizationHeaderIsGeneratedForExampleData()
		{
			var exampleRsaKey = RSA.Create();
			RsaKeyLoader.FromXmlString(exampleRsaKey, 
				@"<RSAKeyValue>
					<Modulus>uLs+L/0KY4fExPmIy2DQz8eCxlChmPMYob86C9DbT0i5HUneIKoR5JOANbSwF3h6eH1xkVDHdjtUJkdMdSFdrQ==</Modulus>
					<Exponent>AQAB</Exponent>
					<P>8KGdnOfOX58i2xm+TBUjVXOzlU0uGkBbVwu32thkpoc=</P>
					<Q>xIemRRd3k3QBGgAIqt5UvK9FHswIVx5jBb7Qls8vsys=</Q>
					<DP>L9ANH0Y4DWvzYxGkbD2u/aW1wy7IwFKVU6BycbuZlDU=</DP>
					<DQ>a5Yq8qXfIwydUcN0+z1NPCHi//IIGtEaull0TSrM3RM=</DQ>
					<InverseQ>QNkMg1QOucqN2TpLv/1Bdlk52x/N+Y9QfIcIxEddPmk=</InverseQ>
					<D>QMWOeX8M3HcnXDVubHkm3iPDS8vLzXg3Q8dsD+aMbxCkZzSOAhs73w1wRjulNeVar9maIXmChDvR835I89ek9Q==</D>
				</RSAKeyValue>");

			var exampleRequest = new
			{
				Amount = "100.00",
				Currency = "GBP",
				Vendor = "newvoicemedia",
				VendorTxCode = "12_3456789.65",
				Description = "Description",
				CardType = "MC",
				CardHolder = "CardHolder",
				GiftAidPayment = "0",
				BillingSurname = "Billing Surname",
				BillingFirstnames = "Billing First Names",
				BillingAddress1 = "Billing Address 1",
				BillingAddress2 = "Billing Address 2",
				BillingCity = "Billing City",
				BillingPostCode = "rg214hg",
				BillingCountry = "US",
				BillingState = "NV",
				BillingPhone = "01234567890",
				DeliverySurname = "Billing Surname",
				DeliveryFirstnames = "Billing First Names",
				DeliveryAddress1 = "Billing Address 1",
				DeliveryAddress2 = "Billing Address 2",
				DeliveryCity = "Billing City",
				DeliveryPostCode = "rg214hg",
				DeliveryCountry = "US",
				DeliveryState = "NV",
				DeliveryPhone = "01234567890",
				CreateToken = "0",
				CustomerEmail = "customer@example.com",
				OtherFields = new {
					ForcePaymentOutcome = "success",
					Product = "Car Insurance"
				},
				AgentId = "1"
			};

			var handler = new PaymentHandler("12345678901", PaymentHandler.Gateway.SagePay, "paymentapi.contact-world.net", exampleRsaKey);
			// var payload = JsonConvert.SerializeObject(exampleRequest);
			var payload = @"{ ""Amount"": ""100.00"", ""Currency"": ""GBP"", ... , AgentId = ""1"" }";
			handler.MakePayment(payload).Wait();
		}
	}
}
