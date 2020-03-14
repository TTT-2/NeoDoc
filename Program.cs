using NeoDoc.Langs;
using System;
using System.IO;

/*
 * TODO
 *
 * convert param[…] and return[default=…] too
 * @author
 * @register
 *
 * support @function for AccessorFunctions e.g.:
---
-- @function GetMin()
-- @return number
---
-- ...
AccessorFunc(PANEL, "m_iMin", "Min")
 *
 * Match module("...", ..., ...) as well ? No -> Add @module in documentation on your own!
 */

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

			LangMatcher langMatcher = new LangMatcher(); // lang processing system

			for (int n = 0; n < amount; n++)
			{
				string file = files[n];

				string relPath = file.Remove(0, folder.Length);
				relPath = relPath.TrimStart('\\');
				relPath = relPath.Replace('\\', '/');

				Console.WriteLine("[" + (int)Math.Floor((n + 1) / (double)amount * 100.0) + "%] '" + relPath + "'");

                // get the lang based on the file extension
				Lang lang = langMatcher.GetByFileExtension(Path.GetExtension(file));

				if (lang == null)
					continue;

				Console.WriteLine("Running '" + lang.GetName() + "' parser");

				FileParser fileParser = new FileParser(langMatcher, lang, file); // fileParser is used to process a file
				fileParser.CleanUp();
				fileParser.Process();

				Console.WriteLine("Finished parsing");
				Console.WriteLine("");
			}
		}
	}
}
