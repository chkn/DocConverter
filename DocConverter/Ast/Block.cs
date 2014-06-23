using System;
using System.Collections.Generic;

namespace DocLib.Ast {

	public class Block : IStyled {

		public Style Style {
			get;
			private set;
		}

		public List<Run> Runs {
			get;
			private set;
		}

		public Block ()
		{
			Style = new Style ();
			Runs = new List<Run> ();
		}
	}
}

