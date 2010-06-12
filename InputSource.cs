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

using System;
using System.IO;

namespace TidyManaged
{
	internal class InputSource
	{
		internal InputSource(Stream stream)
		{
			this.stream = stream;
			this.TidyInputSource = new Interop.TidyInputSource(new Interop.TidyGetByteFunc(OnGetByte), new Interop.TidyUngetByteFunc(OnUngetByte), new Interop.TidyEOFFunc(OnEOF));
		}

		Stream stream;
		internal Interop.TidyInputSource TidyInputSource;

		byte OnGetByte(IntPtr sinkData)
		{
			return (byte) this.stream.ReadByte();
		}

		void OnUngetByte(IntPtr sinkData, byte bt)
		{
			if (this.stream.Position > 0) this.stream.Position--;
		}

		bool OnEOF(IntPtr sinkData)
		{
			return (this.stream.Position >= this.stream.Length);
		}
	}
}
