using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Drawing;
using System.Collections.Generic;

using ICSharpCode.SharpZipLib.Zip;

using DocLib.Ast;

namespace DocLib.Docx {

	public class DocxImporter {

		const string WordEntryBase = "word/";
		const string DocumentXmlEntry = WordEntryBase + "document.xml";
		const string DocumentRelsEntry = WordEntryBase + "_rels/document.xml.rels";

		static readonly XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
		static readonly XNamespace a = "http://schemas.openxmlformats.org/drawingml/2006/main";
		static readonly XNamespace r = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

		static readonly XNamespace rels = "http://schemas.openxmlformats.org/package/2006/relationships";

		ZipFile zip;
		XDocument docXml;
		XDocument relsXml;

		public DocxImporter (string file)
		{
			zip = new ZipFile (file);

			var docEntry = zip.GetEntry (DocumentXmlEntry);
			if (docEntry == null)
				throw new DocxFormatException (string.Format ("Zip entry '{0}' not found", DocumentXmlEntry));

			docXml = XDocument.Load (zip.GetInputStream (docEntry.ZipFileIndex));

			var relsEntry = zip.GetEntry (DocumentRelsEntry);
			if (relsEntry != null)
				relsXml = XDocument.Load (zip.GetInputStream (relsEntry.ZipFileIndex));
		}

		public Document ImportDocument ()
		{
			var doc = new Document ();
			ImportDocument (doc, docXml.Root);
			return doc;
		}
		void ImportDocument (Document doc, XElement elem)
		{
			ImportBody (doc.Body, elem.Element (w + "body"));
		}

		void ImportBody (Body body, XElement elem)
		{
			if (elem == null)
				throw new DocxFormatException ("Body element not found");

			ImportParagraphs (body.Blocks, elem.Elements (w + "p"));
		}

		void ImportParagraphs (List<Block> blocks, IEnumerable<XElement> elems)
		{
			foreach (var elem in elems) {
				var block = new Block ();
				ImportParagraph (block, elem);
				blocks.Add (block);
			}
		}
		void ImportParagraph (Block block, XElement elem)
		{
			foreach (var ppr in elem.Elements (w + "pPr"))
				ImportParagraphProperties (block.Style, ppr);

			foreach (var rElem in elem.Elements()) {
				var run = new Run ();
				if (rElem.Name == w + "hyperlink"){
					ImportHyperlink (run, rElem);
				}
				ImportRun (run, rElem);
				block.Runs.Add (run);
			}
		}

		void ImportHyperlink (Run run, XElement elem)
		{
			var rid = (string)elem.Attribute (r + "id");
			if (rid == null){
				throw new DocxFormatException ("No id found on Hyperlink");
			}
			var rElem = GetRelationship (rid);
			if (rElem == null){
				throw new DocumentFormatException ("No relationship found");
			}

			run.Hyperlink = (string)rElem.Attribute ("Target");
		}

		void ImportRun (Run run, XElement elem)
		{
			foreach (var rpr in elem.Elements (w + "rPr"))
				ImportRunProperties (run.Style, rpr);

			run.Text = (string)elem.Element (w + "t");
			run.Image = ImportDrawing (elem.Element (w + "drawing"));
		}


		DocLib.Ast.Image ImportDrawing (XElement elem)
		{
			if (elem == null)
				return null;

			//FIXME: very limited support right now..
			var blip = elem.Descendants (a + "blip").FirstOrDefault ();
			if (blip == null) {
				Console.Error.WriteLine ("Warning: Cannot import drawing: {0}{1}", Environment.NewLine, elem);
				return null;
			}

			var relElem = GetRelationship ((string)blip.Attribute (r + "embed"));
			if (relElem == null) {
				Console.Error.WriteLine ("Warning: Cannot find embed relation for drawing: {0}{1}", Environment.NewLine, elem);
				return null;
			}

			var target = relElem.Attribute ("Target");
			var entry = zip.GetEntry (WordEntryBase + (string)target);
			if (entry == null) {
				Console.Error.WriteLine ("Warning: Cannot find zip entry for drawing: {0}{1}", Environment.NewLine, elem);
				return null;
			}

			try {
				return new DocLib.Ast.Image (
					Path.GetFileNameWithoutExtension ((string)target),
					new Bitmap (zip.GetInputStream (entry.ZipFileIndex))
				);
			} catch (Exception e) {
				Console.Error.WriteLine (e);
			}
			return null;
		}

		void ImportParagraphProperties (Style style, XElement pr)
		{
			var named = pr.Element (w + "pStyle");
			if (named != null)
				style.Type = ImportNamedStyle (named);

			//FIXME: properly support all the nuances of numbering
			var numbering = pr.Element (w + "numPr");
			if (numbering != null)
				style.Type = Styles.NumberedList;

			//FIXME: The rest
		}

		void ImportRunProperties (Style style, XElement pr)
		{
			var named = pr.Element (w + "rStyle");
			if (named != null)
				style.Type = ImportNamedStyle (named);

			//FIXME: The rest
		}

		Styles ImportNamedStyle (XElement pStyle)
		{
			Styles type;
			var str = (string)pStyle.Attribute (w + "val");
			if (!Enum.TryParse (str, out type)) {
				if (string.Equals (str, "CodeInline", StringComparison.Ordinal))
					type = Styles.Code;
				else
					type = Styles.Custom;
			}
			return type;
		}

		XElement GetRelationship (string id)
		{
			if (id == null || relsXml == null)
				return null;
			return relsXml.Root.Elements (rels + "Relationship").FirstOrDefault (r => id == (string)r.Attribute ("Id"));
		}
	}
}

