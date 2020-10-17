using System.Text.RegularExpressions;

namespace NeoDoc.Params
{
	public class RefParam : StateParam
	{
		private readonly Regex URLRegex = new Regex(@"(?<Protocol>\w+):\/\/(?<Domain>[\w@][\w.:@]+)\/?[\w\.?=%&=\-@/$,]*");

		public override void Process(string[] paramData)
		{
			if (paramData.Length < 1)
				return;

			string tmp = paramData[0];

			if (CheckURL(tmp))
				Value = tmp;
		}

		private bool CheckURL(string url)
		{
			return URLRegex.Match(url).Success;
		}

		public override string GetName()
		{
			return "ref";
		}
	}
}
