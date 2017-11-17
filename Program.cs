using System;

namespace NewVoiceMedia.Pci.Integration
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			try
			{
				System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
				new CommandLineInterface(typeof(Utils)).Run(args);
			}
			catch (Exception e)
			{
				Console.WriteLine("\n\n\nClient-side exception has occured. Stack-trace below is NOT part of response from server.");
				Console.WriteLine(e);
			}
		}
	}
}
