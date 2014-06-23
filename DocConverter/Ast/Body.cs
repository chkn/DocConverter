using System;
using System.Collections.Generic;

namespace DocLib.Ast {

	public class Body {

		public List<Block> Blocks {
			get;
			private set;
		}

		public Body ()
		{
			Blocks = new List<Block> ();
		}
	}
}

