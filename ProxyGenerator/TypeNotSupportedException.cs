using System;

namespace HWSProxyGen
{
	/// <summary>
	/// Summary description for TypeNotSupportedException.
	/// </summary>
	public class TypeNotSupportedException : Exception
	{
		public TypeNotSupportedException(string type) : base("Type not supported: " + type)
		{
			
		}
	}
}
