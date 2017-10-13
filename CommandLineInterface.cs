using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NewVoiceMedia.Pci.Integration
{
	public class CommandLineInterface
	{
		private readonly IEnumerable<Type> _apiTypes;
		private readonly List<MethodInfo> _commands;
		private static readonly Regex MiscCharacters = new Regex("[^\\w]+");

		public CommandLineInterface(params Type[] apiTypes)
		{
			_apiTypes = apiTypes.Append(this.GetType());
			_commands = _apiTypes
				.SelectMany(t => t
					.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public)
					.Where(f => f.GetCustomAttribute<CommandAttribute>() != null && 
						  (f.IsStatic || f.DeclaringType == this.GetType() || f.DeclaringType.GetConstructor(Type.EmptyTypes) != null)))
				.ToList();
		}

		private string ToShellCase(string camelCase)
		{
			var result = new StringBuilder();
			foreach(var c in camelCase.ToCharArray())
			{
				if (Char.IsUpper(c) && result.Length > 0 && result[result.Length-1] != '-')
					result.Append('-');
				result.Append(Char.ToLowerInvariant(c));
			}
			return result.ToString();
		}

		public void Run(string[] args)
		{
			if (args.Length == 0)
				args = new[] { nameof(Help) };
			var commandName = MiscCharacters.Replace(args[0], "");
			var command = _commands
				.SingleOrDefault(m => string.Equals(m.Name, commandName, StringComparison.InvariantCultureIgnoreCase));
			if (command == null)
			{
				Console.WriteLine($"'{ToShellCase(args[0])}' is not a known command.");
				Help();
			}
			else
			{
				var parameterTypes = command.GetParameters().Select(p => p.ParameterType).ToArray();
				var defaultArguments = Enumerable.Repeat(Type.Missing, parameterTypes.Length - (args.Length - 1));
				var arguments = args.Skip(1)
					.Zip(parameterTypes, (a, t) => t.IsEnum ? Enum.Parse(t, a) : Convert.ChangeType(a, t))
					.Concat(defaultArguments).ToArray();
				var obj = command.IsStatic ? null : 
					(command.DeclaringType == this.GetType() ? this : Activator.CreateInstance(command.DeclaringType));
				var result = command.Invoke(obj, arguments);
				if (result is Task<string>)
					result = ((Task<string>)result).Result;
				if (result != null)
					Console.WriteLine(result);
			}
		}

		[Command("Shows this help text")]
		public void Help()
		{
			Console.WriteLine($"Usage: {Assembly.GetEntryAssembly().GetName().Name} <command>");
			var apiTypeNames = String.Join(", ", _apiTypes.Where(t => t != this.GetType()).Select(t => t.FullName));
			Console.WriteLine($"Available commands (from public API{(_apiTypes.Count() > 2 ? "s" : "")} {apiTypeNames}): ");
			var longestCommandName = _commands.Max(c => ToShellCase(c.Name).Length);
			foreach (var command in _commands)
			{
				var parameters = String.Join(" ", command.GetParameters().Select(p => p.HasDefaultValue ? $"[{ToShellCase(p.Name)}={p.DefaultValue}]" : $"<{ToShellCase(p.Name)}>"));
				if (parameters != "")
					parameters += " ";
				var description = command.GetCustomAttribute<CommandAttribute>().Description;
				Console.WriteLine($"	{ToShellCase(command.Name).PadRight(longestCommandName)} {parameters}{description}");
			}
		}
	}
}
