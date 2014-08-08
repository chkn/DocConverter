using System;

namespace DocLib.Ast {


	public enum Styles {
		Custom,

		// These are named to coincide with the Word template style names.
		//  If that needs to be changed, we must update DocxImporter.ImportNamedStyle
		Title,
		Subtitle,
		Heading1,
		Heading2,
		Heading3,
		Heading4,
		SubtleEmphasis,
		Strong,
		Code,
		UIItem,
		BulletList,
		Hyperlink,

		// These are named arbitrarily
		NumberedList
	};

	public static class StylesEx {

		public static bool IsHeading (this Styles type)
		{
			switch (type) {
			case Styles.Heading1:
			case Styles.Heading2:
			case Styles.Heading3:
			case Styles.Heading4:
				return true;
			}
			return false;
		}
	}

	public class Style {

		public Styles Type {
			get;
			set;
		}

		public Style (Styles type = Styles.Custom)
		{
			Type = type;
		}
	}
}

