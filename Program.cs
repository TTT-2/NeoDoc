using NeoDoc.Langs;
using System;
using System.IO;

namespace NeoDoc
{
    internal static class NeoDoc
	{
		private static void Main(string[] args)
		{
			if (args.Length < 1)
				return;

			string folder = args[0];

            if (string.IsNullOrEmpty(folder))
                return;

			// Build the file tree
			string[] files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);

			// Prepare the files
			//Dictionary<string, string> fileTypeDict = new Dictionary<string, string>();

			int amount = files.Length;

			LangParser langParser = new LangParser();

			for (int n = 0; n < amount; n++)
			{
				string file = files[n];

				string relPath = file.Remove(0, folder.Length);
				relPath = relPath.TrimStart('\\');
				relPath = relPath.Replace('\\', '/');

				Console.WriteLine("[" + (int)Math.Floor((n + 1) / (double)amount * 100.0) + "%] '" + relPath + "'");

				Lang lang = langParser.GetByFileExtension(Path.GetExtension(file));

				if (lang == null)
					continue;

				Console.WriteLine("Running " + lang.GetName() + " parser");

				// TODO run the parser

				Console.WriteLine("Finished parsing");
				Console.WriteLine("");
			}
		}
	}
}
