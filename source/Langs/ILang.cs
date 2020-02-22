using System;
namespace NeoDoc.Langs
{
    public interface ILang
    {
        string GetName();
        string GetFileExtension();
        string GetCommentStyleRegex();
    }
}
