using System;

namespace HWSProxyGen
{
	/// <summary>
	/// Summary description for HWSEnumType.
	/// </summary>
	public class HWSEnumType: HWSType
	{
		private string name;

		public string Name {
			get {return name;}
			set {name = value;}
		}

		public override string HaskellReturnCode
		{
			get { return "read";}
		}


		public HWSEnumType(string name)
        {
			this.Name = name;
		}

		public override string ToString() {
			return this.name;
		}

	}
}
