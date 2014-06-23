using System;
using System.IO;

using DocLib.Ast;

namespace DocLib {

	public class PlainTextExporter : DocumentVisitor {

		TextWriter writer;

		public PlainTextExporter (TextWriter writer)
		{
			this.writer = writer;
		}

		public override void Visit (Block block)
		{
			writer.WriteLine ();
			base.Visit (block);
		}

		public override void Visit (string text)
		{
			writer.Write (text);
		}
	}
}

