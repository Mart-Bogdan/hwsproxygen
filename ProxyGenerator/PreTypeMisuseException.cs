using System;

namespace HWSProxyGen
{
	/// <summary>
	/// Summary description for HaskellReturnCodeNotSupportedException.
	/// </summary>
	public class PreTypeMisuseException : Exception
	{
		public PreTypeMisuseException(string reason) : base("Internal type HWSPreType was misused: " + reason)
		{
			
		}
	}
}
