using System;

namespace HWSProxyGen
{
	/// <summary>
	/// Summary description for VoidCannotBeReturnedException.
	/// </summary>
	public class VoidCannotBeReturnedException: Exception
	{
		public VoidCannotBeReturnedException() : base("It is not possible to return a void type")
		{
			
		}
	}
}
