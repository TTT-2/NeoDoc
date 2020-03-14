namespace NeoDoc.Params
{
    public abstract class Param
    {
        public abstract string GetName(); // returns the name of the param
        public abstract void Process(string[] paramData); // paramData = everything except the @param prefix part
        public abstract void ProcessAddition(string[] paramData); // paramData = everything of the line that already hadn't a param part
        public abstract string GetOutput(); // returns the output used for the website
        public abstract string GetData(); // returns the data that should get returned, e.g. Name, Text, etc.
    }
}
