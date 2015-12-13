# TidyHtml5Managed Releases

## v1.0.0

Download URL: http://bit.ly/tidyhtml5managed-1_0_0

**Release note**:
The main theme of this release is to implement new features of Tidy HTML5 library
and drop support for the old Tidy library:
- upgrade to .Net Framework 4.6
- change output library name from "TidyManaged.dll" to "TidyHtml5Managed.dll"
- change searched underlying library name from "libtidy.dll" to "tidy.dll"
- minimum supported Tidy library is version 5.0.0 now
- add new options in TidyOptionId enum: CoerceEndTags, DropEmptyElements, CleanGoogleDocs,
MergeEmphasis, OmitOptionalTags, SkipNestedTags, ShowInfo, IndentWithTabs
- add LibraryVersion property on the Document object
- make ReleaseDate property obsolete in Document object
