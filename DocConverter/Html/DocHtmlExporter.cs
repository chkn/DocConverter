using System;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Drawing;
using System.Drawing.Imaging;

using DocLib.Ast;

namespace DocLib.Html {

	public class DocHtmlExporter : DocumentVisitor, IDisposable {
		/* METADATA:
		 <meta id="[GUID]" title="" subtitle="" ttc="">
			<brief></brief>
			<resources>
				<samplecode></samplecode>
				<slides></slides>
			</resources>
			<related>
				<articles></articles>
				<recipes></recipes>
				<videos></video>
				<samples></samples>
				<api></api>
				<sdk></sdk>
			</related>
		</meta>
		*/

		const string BriefText = "BRIEF";
		const string SampleCodeText = "Sample Code:";
		const string RelatedArticlesText = "Related Articles:";

		TextWriter writer;
		string imagesDir;

		XElement meta;
		XElement currentBlock, currentBlockGroup, currentRunFormat;
		bool hadFirstHeading;

		public DocHtmlExporter (string destFile)
		{
			var dir = Path.GetDirectoryName (destFile);
			var baseName = Path.GetFileNameWithoutExtension (destFile).Replace (' ', '_');
			var htmlFile = Path.Combine (dir, baseName + ".html");
			imagesDir = Path.Combine (dir, baseName, "Images");

			this.writer = new StreamWriter (htmlFile, append: false);
		}

		public void Dispose ()
		{
			if (writer != null) {
				writer.Flush ();
				writer.Dispose ();
				writer = null;
			}
		}

		public override void Visit (Body body)
		{
			// Try to extract metadata
			meta = new XElement ("meta",
				new XAttribute ("id", Guid.NewGuid ().ToString ()),
				new XAttribute ("title", ""),
				new XAttribute ("subtitle", "")
			);

			var brief = new XElement ("brief");
			meta.Add (brief);
			var resources = new XElement ("resources");
			meta.Add (resources);
			var samplecode = new XElement ("samplecode");
			resources.Add (samplecode);
			var slides = new XElement ("slides");
			resources.Add (slides); // FIXME: try to extract slides from doc
			var related = new XElement ("related");
			meta.Add (related);
			//FIXME: handle related section

			for (var i = 0; i < body.Blocks.Count; i++) {
				var block = body.Blocks [i];
				var title = block.GetPlainText (defaultValue: "").Trim ();

				// The brief is the first paragraph following a paragraph with text BriefText in BriefStyle
				if (block.Style.Type == Styles.Heading1 && string.Equals (title, BriefText, StringComparison.OrdinalIgnoreCase)) {
					body.Blocks.RemoveAt (i);
					brief.Value = body.Blocks [i].GetPlainText ();
					body.Blocks.RemoveAt (i);

					while (true) {
						// Extract other metadata from BRIEF section...
						block = body.Blocks [i];
						title = block.GetPlainText (defaultValue: "").Trim ();
						if (string.Equals (title, SampleCodeText, StringComparison.OrdinalIgnoreCase)) {
							body.Blocks.RemoveAt (i);
							samplecode.Value = body.Blocks [i].GetPlainText ();
						} else if (string.Equals (title, RelatedArticlesText, StringComparison.OrdinalIgnoreCase)) {
							body.Blocks.RemoveAt (i);
							//FIXME: handle hyperlinks
							related.Value = body.Blocks [i].GetPlainText ();
						} else if (!string.IsNullOrWhiteSpace (title)) {
							break;
						}
						body.Blocks.RemoveAt (i);
					}
					break;
				}
			}
			base.Visit (body);
		}

		public override void Visit (Block block)
		{
			if (currentBlock != null)
				throw new Exception ("currentBlock != null");

			if (block.Style.Type == Styles.Title) {
				meta.SetAttributeValue ("title", block.GetPlainText ());
				return;
			}
			if (block.Style.Type == Styles.Subtitle) {
				meta.SetAttributeValue ("subtitle", block.GetPlainText ());
				return;
			}

			var needsGroup = false;
			switch (block.Style.Type) {

			case Styles.Heading1:
				currentBlock = new XElement ("h1");
				hadFirstHeading = true;
				break;

			case Styles.Heading2:
				currentBlock = new XElement ("h2");
				hadFirstHeading = true;
				break;

			case Styles.Heading3:
				currentBlock = new XElement ("h3");
				hadFirstHeading = true;
				break;

			case Styles.Heading4:
				currentBlock = new XElement ("h4");
				hadFirstHeading = true;
				break;

			case Styles.BulletList:
				if (currentBlockGroup == null)
					currentBlockGroup = new XElement ("ul");
				currentBlock = new XElement ("li");
				needsGroup = true;
				break;

			case Styles.NumberedList:
				if (currentBlockGroup == null)
					currentBlockGroup = new XElement ("ol");
				currentBlock = new XElement ("li");
				needsGroup = true;
				break;
			}
			if (currentBlock == null) {
				if (!hadFirstHeading)
					return;
				currentBlock = new XElement ("p");
			}
			if (meta != null) {
				writer.WriteLine (meta.ToString ());
				meta = null;
			}
			base.Visit (block);
			if (needsGroup) {
				currentBlockGroup.Add (currentBlock);
			} else if (currentBlockGroup != null) {
				writer.WriteLine (currentBlockGroup.ToString ());
				currentBlockGroup = null;
			}
			if (currentBlockGroup == null)
				writer.WriteLine (currentBlock.ToString ());
			currentBlock = null;
		}

		public override void Visit (Run run)
		{
			if (currentRunFormat != null)
				throw new Exception ("currentRunFormat != null");

			switch (run.Style.Type) {

			case Styles.Code:
				currentRunFormat = new XElement ("code");
				break;

			}
			base.Visit (run);
			if (currentRunFormat != null) {
				currentBlock.Add (currentRunFormat);
				currentRunFormat = null;
			}
		}

		public override void Visit (string text)
		{
			if (!string.IsNullOrEmpty (text))
				(currentRunFormat ?? currentBlock).Add (new XText (text));
		}

		public override void Visit (DocLib.Ast.Image image)
		{
			if (!Directory.Exists (imagesDir))
				Directory.CreateDirectory (imagesDir);
			var fileName = Path.ChangeExtension (image.Name, "png");
			try {
				image.Bitmap.Save (Path.Combine (imagesDir, fileName), ImageFormat.Png);
				(currentRunFormat ?? currentBlock).Add (new XElement ("img", new XAttribute ("src", "Images/" + fileName)));
			} catch (Exception e) {
				Console.Error.WriteLine (e);
			}
		}
	}

	static class Helpers {

		public static Block FirstBlockWithStyle (this Body body, Styles styleType)
		{
			return body.Blocks.FirstOrDefault (b => b.Style.Type == styleType);
		}

		public static string GetPlainText (this Block block, string defaultValue = null)
		{
			return block == null ? defaultValue : string.Concat (block.Runs.Select (r => r.Text));
		}
	}
}

