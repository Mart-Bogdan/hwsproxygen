using System;

namespace HWSProxyGen {
	
	public class HWSPrimitiveType: HWSType {
		private Type type;
		public HWSPrimitiveType(Type type) {
			this.type = type;
		}

		public Type Type {
			get {return type;}
		}

		public override string HaskellReturnCode {
			get { string result = "";
				if (type.Equals(typeof(int))
					|| type.Equals(typeof(float))
					|| type.Equals(typeof(long))
					|| type.Equals(typeof(double))) {
					result = "read";
				}
				else if (type.Equals(typeof(bool))) {
					result = @"(read.upperFirstChar)";
				}
				else if (type.Equals(typeof(string))) {
					result = "id";

				}
				else if (type.Equals(typeof(char))) {
					result = "(chr.read)";
				}
				else if (type.Equals(typeof(void))) {
					throw new VoidCannotBeReturnedException();
				}

				return result;
			}
		}

		public override string ToString() {
			String result = "";
			if (type.Equals(typeof(int)) ) {
				result = "Int";
			}
			else if (type.Equals(typeof(float))  ) {
				result = "Float";
			}
			else if ( type.Equals(typeof(double))) {
				result = "Double";
			}
			else if ( type.Equals(typeof(long))) {
				result = "Integer";
			}
			else if (type.Equals(typeof(string))) {
				result = "String";
			}
			else if (type.Equals(typeof(char))) {
				result = "Char";
			}
			else if (type.Equals(typeof(void))) {
				result = "()";
			}
			else if (type.Equals(typeof(bool))) {
				result = "Bool";
			} 
			else {
				throw new TypeNotSupportedException(type.ToString());
			}
			return result;
		}

		public override bool Equals(object obj) {
			return (obj is HWSPrimitiveType 
				&& ((HWSPrimitiveType) obj).Type.Equals(this.Type));
		}

	}
}
