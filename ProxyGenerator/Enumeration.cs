using System.Collections;

namespace HWSProxyGen
{
	/// <summary>
	/// Summary description for Enumerations.
	/// </summary>
	public class Enumeration {
		private string name;
		private ArrayList enumMembers;

		public string Name {
			get {return name;}
		}

		public ArrayList EnumMembers {
			get {return enumMembers;}
		}

		public Enumeration(string name, ArrayList enumMembers) {
			this.name = name;
			this.enumMembers = enumMembers;
		}
	}
}
