using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Reflection;


namespace HWSProxyGen {
	
	public class Generator {

		static string moduleName;

		const string TAB = "      ";
			
		public static void CreateSpecificModule(ProxyModel model, string outputPath) {
			moduleName = model.Name;
			string completePath = outputPath + @"\" + moduleName + ".hs";
			StreamWriter writer = new StreamWriter(completePath);
			StringBuilder sbuilder = new StringBuilder();
			sbuilder.Append("module " + model.Name);
			sbuilder.Append(MakeExportList(model));
			sbuilder.Append(" where");
			sbuilder.Append("\n");
			sbuilder.Append("\n");
			sbuilder.Append("import Char");
			sbuilder.Append("\n");
			sbuilder.Append("import SoapHttpClientProtocol");
			sbuilder.Append("\n");
			sbuilder.Append("\n");
			CreateSpecificModuleBody(model, outputPath, writer, sbuilder);
			GenerateSupportFile(outputPath, "SoapHttpClientProtocol.hs");
			GenerateSupportFile(outputPath, "MD5.lhs");
			GenerateSupportFile(outputPath, "Zord64_HARD.lhs");
			GenerateSupportFile(outputPath, "Base64.hs");
		}

		public static void CreateSpecificModuleBody(ProxyModel model, string outputPath, StreamWriter writer, StringBuilder sbuilder) {
			string completePath = outputPath + @"\" + model.Name + ".hs";	
			foreach (Enumeration en in model.Enumerations) {
				sbuilder.Append(MakeEnumeration(model,en));
				sbuilder.Append("\n");
				sbuilder.Append("\n");
			}

			foreach (DataType dt in model.DataTypes) {
				sbuilder.Append(MakeDataTypes(model,dt));
				sbuilder.Append("\n");
				sbuilder.Append("\n");
			}

			foreach (Function function in model.Functions) {
				sbuilder.Append(MakeFunction(model,function));
				sbuilder.Append("\n");
			}

			writer.Write(sbuilder.ToString());
			writer.Close();
			Console.WriteLine("File {0} generated", completePath);
		}

		private static void GenerateSupportFile(string outputPath, string fileName) {
			string completePath = outputPath + @"\" + fileName;
			if (!File.Exists(completePath)) {
				Assembly appAssembly = Assembly.GetExecutingAssembly();
				Stream stream = appAssembly.GetManifestResourceStream("HWSProxyGen." + fileName);
				StreamReader reader = new StreamReader(stream);
				string content = reader.ReadToEnd();
				reader.Close();
				StreamWriter writer = new StreamWriter(completePath);
				writer.Write(content);
				writer.Close();
				Console.WriteLine("File {0} generated", completePath);
			} else {
				Console.WriteLine("File {0} already exists; not generated", completePath);
			}
		}


		private static string MakeDataTypeParameter(Parameter p) {
			string result = IdentLevel(1);
			result += LowerFirstChar(p.Name) + " :: " + p.HWSType;
			return result;
		}

		private static String MakeDataTypes(ProxyModel model, DataType dt) {
			
			string result = "";
			result += "data " + dt.Name + " = " + dt.Name;
			if (dt.Parameters.Count > 0) {
				result += " {";
				result += MakeDataTypeParameter((Parameter)dt.Parameters[0]);
				for (int i = 1; i < dt.Parameters.Count; i++) {
					result += ",";
					result += MakeDataTypeParameter((Parameter)dt.Parameters[i]);
				}
				result += "\n";
				result += "}";
			}
			result += " deriving (Show)";
			result += "\n\n";
			result += MakeDataTypeXmlSerializableInstance(dt);			
			return result;
		}

		private static String MakeEnumeration(ProxyModel model, Enumeration en) {
			string result = "";
			result += "data " + en.Name;
			result += IdentLevel(1);
			result += "= " + en.EnumMembers[0];
			for (int i = 1; i < en.EnumMembers.Count; i++) {
				result += IdentLevel(1);
				result += "| " + en.EnumMembers[i];
			}
			result += IdentLevel(1);
			result += "deriving(Read, Show)";

			return result;
		}

		private static String MakeFunction(ProxyModel model, Function function) {
			StringBuilder sbuilder = new StringBuilder();
			sbuilder.Append(MakeFunctionHeader(function));
			sbuilder.Append("\n");
			sbuilder.Append(MakeFunctionBody(model,function));
			sbuilder.Append("\n");

			return sbuilder.ToString();
			
		}

		
		private static string LowerFirstChar(string str) {
			string result;
			if (str.Length > 0) {
				string firstLetter = str[0].ToString().ToLower();
				result = firstLetter + str.Substring(1);
			} else {
				result = str;
			}
			return result;
		}

		private static string UpperFirstChar(string str) 
		{
			string result;
			if (str.Length > 0) 
			{
				string firstLetter = str[0].ToString().ToUpper();
				result = firstLetter + str.Substring(1);
			} 
			else 
			{
				result = str;
			}
			return result;
		}

		private static String MakeFunctionHeader(Function function) {
			
			StringBuilder sbuilder = new StringBuilder();

			// adiciona o nome da função e os "::"
			sbuilder.Append(LowerFirstChar(function.Name));
			sbuilder.Append(" :: ");

			// adiciona os parâmetros
			for (int i = 0; i < function.ParametersCount() ; i++) {
				Parameter p = function[i];
				sbuilder.Append(p.HWSType.ToString());
				sbuilder.Append(" -> ");
			}

			// adiciona o tipo de retorno
			sbuilder.Append("IO " + function.HWSType.ToString());

			return sbuilder.ToString();
		}


		private static String MakeFunctionBody(ProxyModel model, Function function) {
			StringBuilder sbuilder = new StringBuilder();

			// adiciona o nome da função
			sbuilder.Append(LowerFirstChar(function.Name));

			// adiciona os parâmetros
			for (int i = 0; i < function.ParametersCount() ; i++) {
				Parameter p = function[i];
				sbuilder.Append(" ");
				sbuilder.Append(LowerFirstChar(p.Name));
			}

			// adiciona a palavra "do"
			sbuilder.Append(" = do ");
			sbuilder.Append(IdentLevel(1));
			sbuilder.Append("strResult <- invokeWS uriStr name action namespace parameters response");

			// adiciona a parte referente ao retorno na função
			sbuilder.Append(MakeFunctionResult(function));


			// monta a cláusula where
			sbuilder.Append(MakeWhereClause(model,function));
			
			// monta a resposta
			sbuilder.Append(IdentLevel(2));
			sbuilder.Append("response = ");
			sbuilder.Append("\"" + function.Name + "Result" + "\"");

			return sbuilder.ToString();
		}



		private static String MakeFunctionResult(Function function) {

			String indent = "\n" + TAB;
			String result = indent + "return ";
			
			HWSType hwsType = function.HWSType;
			if (hwsType is HWSPrimitiveType
				&& ((HWSPrimitiveType) hwsType).Type.Equals(typeof(void))) 
			{
				result += "()";
			} 
			else 
			{
				result += "$ " + hwsType.HaskellReturnCode + " strResult";
			}
			return result;
		}

		

		private static String MakeWhereClause(ProxyModel model, Function function) {
			
			StringBuilder sbuilder = new StringBuilder();	
			sbuilder.Append(IdentLevel(1));
			sbuilder.Append("where");
			sbuilder.Append(IdentLevel(2));
			sbuilder.Append("uriStr = ");
			sbuilder.Append("\"" + model.Uri + "\"");
			sbuilder.Append(IdentLevel(2));
			sbuilder.Append("name = ");
			sbuilder.Append("\"" + function.Name + "\"");
			sbuilder.Append(IdentLevel(2));
			sbuilder.Append("action = ");
			sbuilder.Append("\"" + function.SoapAction+ "\"");
			sbuilder.Append(IdentLevel(2));
			sbuilder.Append("namespace = ");
			sbuilder.Append("\"" + model.NameSpace+ "\"");
			sbuilder.Append(MakeParameterClause(function));

			return sbuilder.ToString();
		
		}

	  
		public static String MakeParameterClause(Function function) {
			StringBuilder sbuilder = new StringBuilder();
			if (function.ParametersCount() > 0) {
				// adiciona os parâmetros
				sbuilder.Append(IdentLevel(2));
				sbuilder.Append("parametersValue = [");
				sbuilder.Append(MakeParameter((Parameter)function[0]));
				for (int i = 1; i < function.ParametersCount(); i++) {
					sbuilder.Append(", ");
					sbuilder.Append(MakeParameter((Parameter)function[i]));
				}
				sbuilder.Append("]");

				sbuilder.Append(IdentLevel(2));
				sbuilder.Append("parametersName = [");
				sbuilder.Append(MakeParameterName(function[0]));
				for (int i = 1; i < function.ParametersCount(); i++) {
					sbuilder.Append(", ");
					sbuilder.Append(MakeParameterName(function[i]));
				}
				sbuilder.Append("]");
				sbuilder.Append(IdentLevel(2));
				sbuilder.Append("parameters = zip parametersName parametersValue");
			} else {
				sbuilder.Append(IdentLevel(2));
				sbuilder.Append("parameters = []");
			}
			return sbuilder.ToString();
		}

		public static string MakeParameter(Parameter p) {
			string result = "";
			string parameterName = LowerFirstChar(p.Name);
			if (p.HWSType is HWSPrimitiveType) {
				HWSPrimitiveType primType = p.HWSType as HWSPrimitiveType;
				if (primType.Type.Equals(typeof(char))) 
				{
					result = "(primitiveToStr (ord " + parameterName + "))";
				} 
				else if (primType.Type.Equals(typeof(bool))) 
				{
					result = "(lowerFirstChar (primitiveToStr " + parameterName + "))";
				}
				else 
				{
					result = "(primitiveToStr " + parameterName + ")";
				}
			} else if (p.HWSType is HWSEnumType) {
				result = "(show " + parameterName + ")";
			} else if (p.HWSType is HWSDataTypeType) {
				result = "(toXml " + parameterName + ")";
			} else if (p.HWSType is HWSListType) {
				HWSListType listType = p.HWSType as HWSListType;
				if (listType.Parameter.HWSType is HWSPrimitiveType) {
					result = "(buildPrimitiveXmlList " + parameterName + " \"" + listType.Parameter.Name + "\")";
				} else {
					result = "(buildComplexXmlList " + parameterName + " \"" + listType.Parameter.Name + "\")";
				}
			} else {
				throw new TypeNotSupportedException(p.HWSType.ToString());
			}
			return result;
		}

		public static string MakeParameterName(Parameter p) {
			return ("\"" + LowerFirstChar(p.Name) + "\"");
		}

		public static string IdentLevel(int n) {
			string result = "\n";
			for (int i = 0; i < n; i++) {
				result += TAB;
			}
			return result;
		}

		/// <summary>
		/// Escreve o cabeçalho do módulo. Exportando as funções, as enumerações o os
		/// DataTypes	
		/// </summary>
		/// <param name="model"> </param>
		private static string MakeExportList(ProxyModel model){
		
			StringBuilder sbuilder = new StringBuilder();			
			
			// se algum dos conjuntos  não for vazio, coloca o "("  inicial
			// da lista de exportação e concatena os objetos exportados
			if (model.Enumerations.Count > 0 || model.Functions.Count > 0 
				|| model.DataTypes.Count > 0) {
				sbuilder.Append(" (");

				// adiciona as enumeracoes. 
				if (model.Enumerations.Count > 0) {
				
					for(int i = 0; i < model.Enumerations.Count; i++) {
						Enumeration enumeration = (Enumeration) model.Enumerations[i];
						sbuilder.Append(IdentLevel(1));
						sbuilder.Append(model.Name + ".");
						sbuilder.Append(enumeration.Name);
						sbuilder.Append("(..),");
					}										
				}
				// adiciona os dataTypes
				if (model.DataTypes.Count > 0) {
				
					for(int j = 0; j < model.DataTypes.Count; j++) {
						DataType dataType = (DataType) model.DataTypes[j];
						sbuilder.Append(IdentLevel(1));
						sbuilder.Append(model.Name + ".");
						sbuilder.Append(dataType.Name);
						sbuilder.Append("(..),");
					}				
				}
			

				// adiciona as funções
				if (model.Functions.Count > 0) {
				
					for(int i = 0; i < model.Functions.Count; i++) {
						Function function = (Function) model.Functions[i];
						sbuilder.Append(IdentLevel(1));
						sbuilder.Append(model.Name + ".");
						sbuilder.Append(LowerFirstChar(function.Name));
						sbuilder.Append(",");
					}
				}
			
				// remove a última vírgula. que se encontra na ultima posicao da string				
				sbuilder.Remove(sbuilder.Length - 1,1);
				sbuilder.Append("\n");
				sbuilder.Append(")");
			}
						
			return sbuilder.ToString();			
		}

		public static string MakeDataTypeXmlSerializableInstance(DataType dataType) {

			StringBuilder sbuilder = new StringBuilder();
			sbuilder.Append("instance XmlSerializable ");
			sbuilder.Append(dataType.Name);
			sbuilder.Append(" where");
			sbuilder.Append(IdentLevel(1));
			sbuilder.Append("toXml ");
			sbuilder.Append(LowerFirstChar(dataType.Name));
			sbuilder.Append(" = ");
			
			if (dataType.Parameters.Count > 0) {
				Parameter param = (Parameter)dataType.Parameters[0];
				sbuilder.Append(MakeParameterXMLTag(dataType,param));
				for(int i = 1; i < dataType.Parameters.Count; i++) {
					param = (Parameter) dataType.Parameters[i];
					sbuilder.Append(" ++ ");					
					sbuilder.Append(MakeParameterXMLTag(dataType,param));
				}
			}
			else {
				// concatenates the empty string
				sbuilder.Append("\"\"");
			}
			sbuilder.Append(IdentLevel(1));
			sbuilder.Append("fromXml str");
			sbuilder.Append(" = ");
			if (dataType.Parameters.Count > 0) 
			{
				string dataTypeName = dataType.Name;
				sbuilder.Append(LowerFirstChar(dataTypeName));
				sbuilder.Append(IdentLevel(2));
				sbuilder.Append("where");
				sbuilder.Append(IdentLevel(3));
				sbuilder.Append(LowerFirstChar(dataTypeName));
				sbuilder.Append(" = ");
				sbuilder.Append(dataTypeName + " {");
				
				sbuilder.Append(MakeDataTypeParameterReference(dataType, 0));
				for (int i = 1; i < dataType.Parameters.Count; i++) 
				{
					sbuilder.Append(",");
					sbuilder.Append(MakeDataTypeParameterReference(dataType, i));
				}
				
				sbuilder.Append(IdentLevel(3));
				sbuilder.Append("}");

				sbuilder.Append(MakeDataTypeParameterDefinition(dataType, 0));
				for (int i = 1; i < dataType.Parameters.Count; i++) 
				{
					sbuilder.Append(MakeDataTypeParameterDefinition(dataType, i));
				}
			}
			else 
			{
				// concatenates the empty string
				sbuilder.Append(dataType.Name);
			}
			return sbuilder.ToString();
		
		}

		public static string MakeDataTypeParameterReference(DataType dt, int index) 
		{
			Parameter p = dt.Parameters[index] as Parameter;
			StringBuilder sbuilder = new StringBuilder();
			sbuilder.Append(IdentLevel(4));
			sbuilder.Append(moduleName + "." + LowerFirstChar(p.Name));
			sbuilder.Append(" = ");
			sbuilder.Append(LowerFirstChar(dt.Name) + UpperFirstChar(p.Name));
			return sbuilder.ToString();
		}

		public static string MakeDataTypeParameterDefinition(DataType dt, int index) {
			Parameter p = dt.Parameters[index] as Parameter;
			StringBuilder sbuilder = new StringBuilder();
			sbuilder.Append(IdentLevel(3));
			sbuilder.Append(LowerFirstChar(dt.Name) + UpperFirstChar(p.Name));
			sbuilder.Append(" = ");

			

			HWSType hwsType = p.HWSType;
			sbuilder.Append(hwsType.HaskellReturnCode);
			sbuilder.Append(" $ ");
			sbuilder.Append("headStr $ getNodeValues str \"" + p.Name + "\"");

			return sbuilder.ToString();
		}


 
		// "<nome>" ++ (show (nome cliente)) ++ "</nome>"
		private static string MakeParameterXMLTag(DataType dtype, Parameter param) {
			StringBuilder sbuilder = new StringBuilder();			
			
			sbuilder.Append(IdentLevel(2));
			sbuilder.Append("(xmlTagStart \"");
			sbuilder.Append(param.Name);
			sbuilder.Append("\")");
			sbuilder.Append(" ++ ");
			if (param.HWSType is HWSPrimitiveType) 
			{
				if (((HWSPrimitiveType) param.HWSType).Equals(typeof(bool))) 
				{
					sbuilder.Append("((lowerFirstChar.primitiveToStr) (");
				} 
				else 
				{
					sbuilder.Append("(primitiveToStr (");
				}
			} 
			else if (param.HWSType is HWSListType) 
			{
				sbuilder.Append("(primitiveToStr (");
			}
			else 
			{
				sbuilder.Append("(toXml (");
			}
			
			sbuilder.Append(moduleName + "." + LowerFirstChar(param.Name));
			sbuilder.Append(" " + LowerFirstChar(dtype.Name) + "))");				
			sbuilder.Append(" ++ ");
			sbuilder.Append("(xmlTagEnd \"");
			sbuilder.Append(param.Name);
			sbuilder.Append("\")");
			return sbuilder.ToString();

		}
	}
}
