using System;
namespace TidyManaged.Interop
{
    internal interface IPInvoke
    {
        int tidyCleanAndRepair(IntPtr tdoc);
        IntPtr tidyCreate();
        bool tidyOptGetBool(IntPtr tdoc, TidyOptionId optId);
        uint tidyOptGetInt(IntPtr tdoc, TidyOptionId optId);
        IntPtr tidyOptGetValue(IntPtr tdoc, TidyOptionId optId);
        string tidyOptGetValueString(IntPtr tdoc, TidyOptionId optId);
        bool tidyOptSetBool(IntPtr tdoc, TidyOptionId optId, bool val);
        bool tidyOptSetInt(IntPtr tdoc, TidyOptionId optId, uint val);
        bool tidyOptSetValue(IntPtr tdoc, TidyOptionId optId, string val);
        int tidyParseFile(IntPtr tdoc, string filename);
        int tidyParseSource(IntPtr tdoc, ref TidyInputSource source);
        int tidyParseString(IntPtr tdoc, string content);
        void tidyRelease(IntPtr tdoc);
        IntPtr tidyReleaseDate();
        int tidySaveFile(IntPtr tdoc, string filname);
        int tidySaveSink(IntPtr tdoc, ref TidyOutputSink sink);
        int tidySaveString(IntPtr tdoc, IntPtr buffer, ref uint buflen);
    }
}
