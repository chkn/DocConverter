using System;

namespace DocLib.Ast {

	public class Document {

		public Body Body {
			get;
			private set;
		}

		public Document ()
		{
			Body = new Body ();
		}
	}
}

