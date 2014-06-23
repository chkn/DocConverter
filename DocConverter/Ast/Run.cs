using System;
using System.Drawing;

namespace DocLib.Ast {

	public class Run : IStyled {

		public Style Style {
			get;
			private set;
		}

		// Either Text or Image will be non-null:

		public string Text {
			get;
			set;
		}

		public Image Image {
			get;
			set;
		}

		public Run ()
		{
			Style = new Style ();
		}
	}
}

