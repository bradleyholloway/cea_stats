using System;
namespace PlayCEASharp.Configuration
{
	/// <summary>
	/// Class to allow clients to configure how log messages are reported.
	/// </summary>
	public class CeaSharpLogging
	{
		/// <summary>
		/// The logger to be used by the CeaSharpLibrary
		/// </summary>
		public static ILogger logger = new DefaultLogger();

		/// <summary>
		/// Method called by the library to log.
		/// </summary>
		/// <param name="message">The message to log.</param>
		internal static void Log(string message)
		{
			logger.Log(message);
		}
	}
}

