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
using System.Runtime.InteropServices;

namespace TidyManaged.Interop
{
	internal class PInvoke
	{
		[DllImport("libtidy.dll")]
		internal static extern IntPtr tidyCreate();

		[DllImport("libtidy.dll")]
		internal static extern void tidyRelease(IntPtr tdoc);

		[DllImport("libtidy.dll")]
		internal static extern IntPtr tidyReleaseDate();

		[DllImport("libtidy.dll")]
		internal static extern IntPtr tidyOptGetValue(IntPtr tdoc, TidyOptionId optId);

		[DllImport("libtidy.dll")]
		internal static extern bool tidyOptSetValue(IntPtr tdoc, TidyOptionId optId, string val);

		[DllImport("libtidy.dll")]
		internal static extern uint tidyOptGetInt(IntPtr tdoc, TidyOptionId optId);

		[DllImport("libtidy.dll")]
		internal static extern bool tidyOptSetInt(IntPtr tdoc, TidyOptionId optId, uint val);

		[DllImport("libtidy.dll")]
		internal static extern bool tidyOptGetBool(IntPtr tdoc, TidyOptionId optId);

		[DllImport("libtidy.dll")]
		internal static extern bool tidyOptSetBool(IntPtr tdoc, TidyOptionId optId, bool val);

		[DllImport("libtidy.dll")]
		internal static extern int tidyParseFile(IntPtr tdoc, string filename);

		[DllImport("libtidy.dll")]
		internal static extern int tidyParseString(IntPtr tdoc, string content);

		[DllImport("libtidy.dll")]
		internal static extern int tidyParseSource(IntPtr tdoc, ref TidyInputSource source);

		[DllImport("libtidy.dll")]
		internal static extern int tidyCleanAndRepair(IntPtr tdoc);

		[DllImport("libtidy.dll")]
		internal static extern int tidySaveFile(IntPtr tdoc, string filname);

		[DllImport("libtidy.dll")]
		internal static extern int tidySaveString(IntPtr tdoc, IntPtr buffer, ref uint buflen);

		[DllImport("libtidy.dll")]
		internal static extern int tidySaveSink(IntPtr tdoc, ref TidyOutputSink sink);

		internal static string tidyOptGetValueString(IntPtr tdoc, TidyOptionId optId)
		{
			return Marshal.PtrToStringAnsi(tidyOptGetValue(tdoc, optId));
		}
	}
}
