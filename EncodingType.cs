// Copyright (c) 2009 Mark Beaton
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

namespace TidyManaged
{
	/// <summary>
	/// Represents the supported encodings.
	/// </summary>
	public enum EncodingType
	{
		/// <summary>
		/// No or unknown encoding.
		/// </summary>
		Raw = 0,

		/// <summary>
		/// The American Standard Code for Information Interchange (ASCII) encoding scheme.
		/// </summary>
		Ascii = 1,

		/// <summary>
		/// The ISO/IEC 8859-15 encoding scheme, also knows as Latin-0 and Latin-9.
		/// </summary>
		Latin0 = 2,

		/// <summary>
		/// The ISO/IEC 8859-1 encoding scheme, also knows as Latin-1.
		/// </summary>
		Latin1 = 3,

		/// <summary>
		/// The UTF-8 encoding scheme.
		/// </summary>
		Utf8 = 4,

		/// <summary>
		/// The ISO/IEC 2022 encoding scheme.
		/// </summary>
		Iso2022 = 5,

		/// <summary>
		/// The MacRoman encoding scheme.
		/// </summary>
		MacRoman = 6,

		/// <summary>
		/// The Windows-1252 encoding scheme.
		/// </summary>
		Win1252 = 7,

		/// <summary>
		/// The Code page 858 encoding scheme, also know as CP 858, IBM 858, or OEM 858.
		/// </summary>
		Ibm858 = 8,

#if SUPPORT_UTF16_ENCODINGS

		/// <summary>
		/// The UTF-16LE (Little Endian) encoding scheme.
		/// </summary>
		Utf16LittleEndian = 9,

		/// <summary>
		/// The UTF-16BE (Big Endian) encoding scheme.
		/// </summary>
		Utf16BigEndian = 10,

		/// <summary>
		/// The UTF-16 encoding scheme, with endianess detected using a BOM.
		/// </summary>
		Utf16 = 11,

#endif

#if SUPPORT_ASIAN_ENCODINGS
#if SUPPORT_UTF16_ENCODINGS

		/// <summary>
		/// The Big-5 or Big5 encoding scheme, used in Taiwan, Hong Kong, and Macau for Traditional Chinese characters.
		/// </summary>
		Big5 = 12,

		/// <summary>
		/// The Shift JIS encoding scheme for Japanese characters.
		/// </summary>
		ShiftJIS = 13

#else

		/// <summary>
		/// The Big-5 or Big5 encoding scheme, used in Taiwan, Hong Kong, and Macau for Traditional Chinese characters.
		/// </summary>
		Big5 = 9,

		/// <summary>
		/// The Shift JIS encoding scheme for Japanese characters.
		/// </summary>
		ShiftJIS = 10

#endif
#endif
	}
}
