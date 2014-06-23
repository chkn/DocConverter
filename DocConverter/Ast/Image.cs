using System;
using System.Drawing;

namespace DocLib.Ast {

	public class Image {

		public string Name {
			get;
			private set;
		}

		public Bitmap Bitmap {
			get;
			private set;
		}

		public Image (string name, Bitmap bitmap)
		{
			Name = name;
			Bitmap = bitmap;
		}
	}
}

