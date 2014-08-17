using System;

namespace HWSProxyGen
{
	/// <summary>
	/// Summary description for HWSPreType.
	/// </summary>
	public class HWSPreType: HWSType
	{
		private string name;

		public string Name 
		{
			get {return name;}
			set {name = value;}
		}

		public override string HaskellReturnCode
		{
			get { throw new PreTypeMisuseException("PreType does not have a haskell return code");}
		}


		public HWSPreType(string name)
		{
			this.Name = name;
		}
	}
}
