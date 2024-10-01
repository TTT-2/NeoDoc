using System.Collections.Generic;
using System.Text.RegularExpressions;
using NeoDoc.Params;

namespace NeoDoc.DataStructures.Lua
{
	public class Accessor : DataStructure
	{
		public override Regex GetRegex()
		{
			return new Regex(@"\s*AccessorFunc\s*\((\w|,|\s)+\)");
		}

		public override bool CheckMatch(string line)
		{
			return GetRegex().Match(line).Success;
		}

		public override void Process(FileParser fileParser)
		{
			if (Ignore)
				return;

			string[] typs = null;
			string typDesc = null;
			string name = null;
			bool local = false;

			if (ParamsList != null)
			{
				for (int i = 0; i < ParamsList.Count; i++)
				{
					List<Param> copyParamsList = new List<Param>();
					Param currentParam = ParamsList[i];

					if (currentParam is AccessorParam accessorParam)
					{
						typs = accessorParam.Typs;
						typDesc = accessorParam.Description;
					}
					else if (currentParam is NameParam nameParam)
						name = nameParam.Value;
					else if (currentParam is LocalParam)
						local = true;
					else
						copyParamsList.Add(currentParam);
				}

				if (ParamsList.Count == 0)
					ParamsList = null;
			}

			if (local)
				Ignore = true;

			string line = "";

			for (int j = 0; j < fileParser.CurrentMatchedLines; j++)
			{
				line = line + fileParser.Lines[fileParser.CurrentLineCount + j];
			}

			if (typs == null)
				NeoDoc.WriteErrors("Missing essential param", new List<string>{
					"Missing '@accessor' in '" + GetName() + "' datastructure"
				}, fileParser.relPath, fileParser.CurrentLineCount + 1, (int)NeoDoc.ERROR_CODES.MISSING_ESSENTIAL_PARAMS);

			Match splitMatch = GetRegex().Match(line);

			List<string> tmpData = NeoDoc.GetEntriesFromString(line.Substring(splitMatch.Index, line.Length - splitMatch.Index), out _);

			string wrapperName = tmpData[0].Trim();
			string varName = tmpData[1].Trim('"');
			string funcPartName = tmpData[2].Trim('"');

			// create a setter func
			Function setterFunc = new Function
			{
				// set meta information
				FoundLine = fileParser.CurrentLineCount + 1,
				FoundPath = fileParser.relPath
			};

			setterFunc.Line = line;
			setterFunc.Local = local;
			setterFunc.FunctionData = line;
			setterFunc.Name = wrapperName + ":" + "Set" + (name ?? funcPartName);

			List<Param> _tmpList;

			if (ParamsList == null)
				_tmpList = new List<Param>();
			else
				_tmpList = new List<Param>(ParamsList);

			setterFunc.ParamsList = _tmpList;

			setterFunc.ParamsList.Add(new ParamParam()
			{
				Name = varName,
				Typs = typs,
				Description = typDesc
			});

			// create a getter func
			Function getterFunc = new Function
			{
				// set meta information
				FoundLine = fileParser.CurrentLineCount + 1,
				FoundPath = fileParser.relPath
			};

			getterFunc.Line = line;
			getterFunc.Local = local;
			getterFunc.FunctionData = line;
			getterFunc.Name = wrapperName + ":" + "Get" + (name ?? funcPartName);

			if (ParamsList == null)
				_tmpList = new List<Param>();
			else
				_tmpList = new List<Param>(ParamsList);

			getterFunc.ParamsList = _tmpList;

			getterFunc.ParamsList.Add(new ReturnParam()
			{
				Typs = typs,
				Description = typDesc
			});

			// now add the datastructure into the current section of the current container
			if (!fileParser.CurrentSection.DataStructureDict.TryGetValue(getterFunc.GetName(), out List<DataStructure> dsList))
			{
				dsList = new List<DataStructure>();

				fileParser.CurrentSection.DataStructureDict.Add(getterFunc.GetName(), dsList);
			}

			dsList.Add(setterFunc);
			dsList.Add(getterFunc);
		}

		public override void Initialize(FileParser fileParser)
		{
			if (fileParser.paramsList.Count > 0)
				ParamsList = new List<Param>(fileParser.paramsList); // set the params with a copy of the list

			ProcessDatastructure(fileParser);
		}

		public override string GetName()
		{
			return "accessor";
		}

		public override string GetDatastructureName()
		{
			return "accessor";
		}

		public override object GetData()
		{
			return null;
		}
	}
}
