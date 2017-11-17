using System;

namespace NewVoiceMedia.Pci.Integration
{
	internal class CommandAttribute : Attribute
	{
		public readonly string Description;

		public CommandAttribute(string description)
		{
			this.Description = description;
		}
	}
}
