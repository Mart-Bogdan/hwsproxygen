using System.Collections;

namespace HWSProxyGen {
	/// <summary>
	/// Classe que representa objetos no wsdl.
	/// </summary>
	public class DataType {
		private string name;
		private ArrayList parameters;

		public DataType(string name, ArrayList parameters) {
			this.Name = name;
			this.Parameters = parameters;
		}

		public string Name {
			get { return name; }
			set { name = value;}
		}

		public ArrayList Parameters {
			get { return parameters; }
			set { parameters = value; }
		}

		public Parameter this[int i] {
			
			get { return (Parameter)parameters[i];}
			set { parameters[i] = value; }
		}

		public int ParametersCount () {
			return parameters.Count;
		}


	}
}
