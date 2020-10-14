using System.Collections.Generic;
using NeoDoc.DataStructures;
using NeoDoc.DataStructures.Lua;

namespace NeoDoc.Params
{
	public class FunctionParam : TextParam
	{
		public override string GetName()
		{
			return "function";
		}

		public override void ModifyFileParser(FileParser fileParser)
		{
			new Function().Initialize(fileParser);

			// cleans the params list to be used for the next function or whatever, even if there is no dataStructure match
			fileParser.paramsList.Clear();
		}
	}
}
