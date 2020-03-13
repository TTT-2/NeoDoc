﻿namespace NeoDoc.Params
{
    public class Note : Param
    {
        public string Description { get; set; } = "";

        public override string GetData()
        {
            return Description;
        }

        public override string GetName()
        {
            return "note";
        }

        public override string GetOutput()
        {
            return Description;
        }

        public override void Process(string[] paramData)
        {
            if (paramData.Length < 1)
                return;

            Description = string.Join(" ", paramData);
        }

        public override void ProcessAddition(string[] paramData)
        {
            if (paramData.Length < 1)
                return;

            if (!string.IsNullOrEmpty(Description))
            {
                Description += " ";
            }

            Description += string.Join(" ", paramData);
        }
    }
}
