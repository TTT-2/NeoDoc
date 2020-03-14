﻿namespace NeoDoc.Params
{
    public class ThreeDParam : MarkParam
    {
        public override string GetName()
        {
            return "3D";
        }

        public override string GetOutput()
        {
            return "This is a 3D method"; // nothing to output
        }
    }
}