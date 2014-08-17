using System;

namespace HWSProxyGen
{
	/// <summary>
	/// Summary description for HWSListType.
	/// </summary>
	public class HWSListType: HWSType
	{
		private Parameter parameter;
		public Parameter Parameter {
			get {return parameter;}
			set {parameter = value;}
		}

		private string name;
		public string Name {
			get {return name;}
			set {name = value;}
		}

		public override string HaskellReturnCode
		{
			get { 
				return "(\\xml -> map "
					+ Parameter.HWSType.HaskellReturnCode
					+ " (getNodeValues xml \"" + Parameter.Name + "\"))";
			}
		}


		public HWSListType(string name, Parameter parameter)
		{
			this.Parameter = parameter;
			this.Name = name;
		}

		public override string ToString() {
			return "[" + this.Parameter.HWSType.ToString() + "]";
		}
	}
}
