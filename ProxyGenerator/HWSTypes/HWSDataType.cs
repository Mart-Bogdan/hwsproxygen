using System;

namespace HWSProxyGen {
	/// <summary>
	/// Summary description for HWSDataTypeType.
	/// </summary>
	public class HWSDataTypeType : HWSType {
				
		private string name;
		public string Name {
			get {return name;}
			set {name = value;}
		}

		private bool hasParameters;
		public bool HasParameters {
			get {return hasParameters;}
			set {hasParameters = value;}
		}

		public override string HaskellReturnCode {
			get 
			{
				string result = "";
				if (this.HasParameters) {
					result = "fromXml";
				}
				else 
				{
					result = this.Name;
				}
				return result;
			}
		}

		public HWSDataTypeType(string name, bool hasParameters) {
			this.Name = name;
			this.HasParameters = hasParameters;
		}

		public override string ToString() {
			return this.Name;
		}
		
	}
}
