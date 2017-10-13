namespace NewVoiceMedia.Pci.Integration
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
			new CommandLineInterface(typeof(Utils)).Run(args);
		}
	}
}
