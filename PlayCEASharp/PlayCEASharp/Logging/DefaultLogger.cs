using System;
namespace PlayCEASharp.Configuration
{
	/// <summary>
	/// Class to allow clients to configure how log messages are reported.
	/// </summary>
	internal class DefaultLogger : ILogger
	{
		/// <summary>
		/// Logs a message to the console.
		/// </summary>
		/// <param name="message">Message to log.</param>
		void ILogger.Log(string message)
		{
            Console.WriteLine(message);
        }
	}
}

