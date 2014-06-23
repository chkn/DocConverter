using System;
using System.Drawing;

namespace DocLib.Ast {

	public abstract class DocumentVisitor {

		public virtual void Visit (Document doc)
		{
			Visit (doc.Body);
		}

		public virtual void Visit (Body body)
		{
			foreach (var block in body.Blocks)
				Visit (block);
		}

		public virtual void Visit (Block block)
		{
			Visit ((IStyled)block);
			foreach (var run in block.Runs)
				Visit (run);
		}

		public virtual void Visit (Run run)
		{
			Visit ((IStyled)run);
			if (run.Text != null)
				Visit (run.Text);
			if (run.Image != null)
				Visit (run.Image);
		}

		public virtual void Visit (string text)
		{
		}

		public virtual void Visit (Image image)
		{
		}

		public virtual void Visit (IStyled styled)
		{
		}
	}
}

