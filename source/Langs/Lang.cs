namespace NeoDoc.Langs
{
    public abstract class Lang
    {
        public abstract string GetName();
        public abstract string GetFileExtension();
        public abstract string GetCommentStyleRegex();
        public abstract char GetSingleCommentChar();
        public abstract string GetCommentStartRegex();
    }
}
