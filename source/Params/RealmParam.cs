namespace NeoDoc.Params
{
	public class RealmParam : StateParam
	{
		public override void Process(string[] paramData)
		{
			if (paramData.Length < 1)
				return;

			string tmp = paramData[0];

			if (CheckRealm(tmp))
				Value = tmp;
		}

		public bool CheckRealm(string str)
		{
			if (str.Equals("shared") || str.Equals("client") || str.Equals("server"))
				return true;

			return false;
		}

		public override string GetName()
		{
			return "realm";
		}
	}
}
