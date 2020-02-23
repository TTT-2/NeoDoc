﻿namespace NeoDoc.Params
{
    public class Desc : Param
    {
        public string Description { get; set; }

        public override string GetName()
        {
            return "desc";
        }

        public override string GetOutput()
        {
            return Description;
        }

        public override void Process(string[] paramData)
        {
            Description = string.Join(" ", paramData);
        }
    }
}
