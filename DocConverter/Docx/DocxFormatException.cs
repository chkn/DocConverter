using System;

namespace DocLib.Docx {

	public class DocxFormatException : DocumentFormatException {

		public DocxFormatException (string message)
			: base ("Invalid docx file: " + message)
		{
		}
	}
}

