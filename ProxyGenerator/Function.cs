using System.Collections;

namespace HWSProxyGen
{
	/// <summary>
	/// Classe que representa cada uma das funções disponíveis no WSDL.
	/// </summary>
	public class Function {
		private string name;
		private HWSType hwsType;
		private string soapAction;
		private ArrayList parameters;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="soapAction"></param>
		/// <param name="parameters"></param>
		public Function(string name, HWSType hwsType, string soapAction, ArrayList parameters) {
			this.Name = name;
			this.HWSType = hwsType;
			this.SoapAction = soapAction;
			this.Parameters = parameters;
		}

		public string Name {
			get { return name; }
			set { name = value;}
		}

		public HWSType HWSType {
			get { return hwsType; }
			set {hwsType = value;}
		}

		public string SoapAction {
			get { return soapAction; }
			set { soapAction = value; }
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
