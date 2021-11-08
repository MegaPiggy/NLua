using System;

namespace NLua.Method
{
    /// <summary>
    /// Parameter information
    /// </summary>
    class MethodArgs
	{
        /// <summary>
        /// Position of parameter
        /// </summary>
        public int Index;
		public Type ParameterType;

        /// <summary>
        /// Type-conversion function
        /// </summary>
        public ExtractValue ExtractValue;
		public bool IsParamsArray;
	}
}
