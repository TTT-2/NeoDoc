using System.Collections.Generic;
using System.Text.RegularExpressions;
using NeoDoc.Params;

namespace NeoDoc.DataStructures
{
    public abstract class DataStructure
    {
        public Param[] ParamsList;

        public abstract Regex GetRegex(); // returns the exact RegEx to match e.g. the Function
        public abstract void Process(string line); // process data based on given line string
        public abstract bool Check(string line); // returns whether the current DocTarget is matched in this line
        public abstract string GetName(); // returns an identification name
        public abstract string GetData(); // returns data
    }
}
