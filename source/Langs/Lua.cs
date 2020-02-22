using System;

namespace NeoDoc.Langs
{
    public class Lua : Lang
    {
        public override string GetName()
        {
            return "Lua";
        }

        public override string GetFileExtension()
        {
            return ".lua";
        }

        public override string GetCommentStyleRegex()
        {
            return @"-{2,}";
        }
    }
}
