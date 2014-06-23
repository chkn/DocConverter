using System;

using DocLib.Docx;
using DocLib.Html;

namespace DocLib {

	public class Tool {

		public static int Main (string[] args)
		{
			try {
				if (args.Length < 1) {
					Console.Error.WriteLine ("Usage: mono doc.exe convert <input> [optional output]");
					return 1;
				}
				switch (args [0]) {

				case "convert":
					return Convert (args);

				default:
					Console.Error.WriteLine ("Don't know what to do!");
					return 1;
				}
			} catch (Exception ex) {
				Console.Error.WriteLine (ex);
				return 2;
			}
		}

		static int Convert (string[] args)
		{
			if (args.Length < 2) {
				Console.Error.WriteLine ("No input file name given");
				return 3;
			}
			var input = args [1];
			var output = input;
			if (args.Length > 2) {
				output = args [2];
			}

			var importer = new DocxImporter (input);
			var doc = importer.ImportDocument ();
			using (var exporter = new DocHtmlExporter (output))
				exporter.Visit (doc);

			return 0;
		}
	}
}
