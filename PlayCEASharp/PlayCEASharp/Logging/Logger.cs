using System;
namespace PlayCEASharp.Configuration
{
	/// <summary>
	/// Class to allow clients to configure how log messages are reported.
	/// </summary>
	internal class Logger
	{
		/// <summary>
		/// Method to log a string.
		/// </summary>
		internal static void Log(string message)
		{
			Console.WriteLine(message);
		}
	}
}

