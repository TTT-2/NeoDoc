using System;
namespace NeoDoc.Langs
{
    public abstract class Lang : ILang
    {
        public abstract string GetName();
        public abstract string GetFileExtension();
        public abstract string GetCommentStyleRegex();
    }
}
