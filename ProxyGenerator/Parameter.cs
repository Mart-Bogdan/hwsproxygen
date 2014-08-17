using System;

namespace HWSProxyGen {
	/// <summary>
	/// Summary description for Parameter.
	/// </summary>
	public class Parameter {

		private string name;
		private HWSType hwsType;

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public HWSType HWSType {
			get { return hwsType; }
			set { hwsType = value; }
		}

		public Parameter(string name, HWSType hwsType) {
			this.Name = name;
			this.HWSType = hwsType;
		}

		public Parameter() {
		}
		
	}
}
