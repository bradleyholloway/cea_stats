using System;
namespace PlayCEASharp.Configuration
{
	/// <summary>
	/// Class to allow clients to configure how log messages are reported.
	/// </summary>
	public interface ILogger
	{
		/// <summary>
		/// Method to log a string.
		/// </summary>
		void Log(string message);
	}
}

