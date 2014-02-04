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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using TidyManaged.Interop;

namespace TidyManaged
{
	/// <summary>
	/// Represents an HTML document (or XML, XHTML) to be processed by Tidy.
	/// </summary>
	public class Document : IDisposable
	{
        internal static IPInvoke PInvoke = IntPtr.Size == 8 ? new PInvoke64() as IPInvoke : new PInvoke32() as IPInvoke;

		#region Constructors

		Document()
		{
			this.handle = PInvoke.tidyCreate();
			this.disposed = false;
		}

		Document(string htmlString)
			: this()
		{
			this.htmlString = htmlString;
			this.fromString = true;
		}


		Document(Stream stream)
			: this()
		{
			this.stream = stream;
		}

		#endregion

		#region Fields

		IntPtr handle;
		Stream stream;
		string htmlString;
		bool fromString;
		bool disposed;
		bool cleaned;

		#endregion

		#region Properties

		DateTime? _ReleaseDate;
		static readonly object releaseDateLock = new object();
		/// <summary>
		/// Gets the release date of the underlying Tidy library.
		/// </summary>
		public DateTime ReleaseDate
		{
			get
			{
				lock (releaseDateLock)
				{
					if (!_ReleaseDate.HasValue)
					{
						DateTime val = DateTime.MinValue;
						string release = Marshal.PtrToStringAnsi(PInvoke.tidyReleaseDate());
						if (release != null)
						{
							string[] tokens = release.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
							if (tokens.Length >= 3)
							{
								DateTime.TryParseExact(tokens[0] + " " + tokens[1] + " " + tokens[2], "d MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out val);
							}
						}
						_ReleaseDate = val;
					}
					return _ReleaseDate.Value;
				}
			}
		}

		#region HTML, XHTML, XML Options

		/// <summary>
		/// [add-xml-decl] Gets or sets whether Tidy should add the XML declaration when outputting XML or XHTML. Note that if the input already includes an &lt;?xml ... ?&gt; declaration then this option will be ignored. If the encoding for the output is different from "ascii", one of the utf encodings or "raw", the declaration is always added as required by the XML standard. Defaults to false.
		/// </summary>
		public bool AddXmlDeclaration
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyXmlDecl); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyXmlDecl, value); }
		}

		/// <summary>
		/// [add-xml-space] Gets or sets whether Tidy should add xml:space="preserve" to elements such as &lt;PRE&gt;, &lt;STYLE&gt; and &lt;SCRIPT&gt; when generating XML. This is needed if the whitespace in such elements is to be parsed appropriately without having access to the DTD. Defaults to false.
		/// </summary>
		public bool AddXmlSpacePreserve
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyXmlSpace); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyXmlSpace, value); }
		}

		/// <summary>
		/// [alt-text] Gets or sets the default "alt=" text Tidy uses for &lt;IMG&gt; attributes. This feature is dangerous as it suppresses further accessibility warnings. You are responsible for making your documents accessible to people who can not see the images!
		/// </summary>
		public string DefaultAltText
		{
			get { return PInvoke.tidyOptGetValueString(this.handle, TidyOptionId.TidyAltText); }
			set { PInvoke.tidyOptSetValue(this.handle, TidyOptionId.TidyAltText, value); }
		}

		/// <summary>
		/// [anchor-as-name] Gets or sets the deletion or addition of the name attribute in elements where it can serve as anchor. If set to true, a name attribute, if not already existing, is added along an existing id attribute if the DTD allows it. If set to false, any existing name attribute is removed if an id attribute exists or has been added. Defaults to true.
		/// </summary>
		public bool AnchorAsName
		{
			// Not available before until 18 Jun 2008
			get
			{
				if (this.ReleaseDate < new DateTime(2008, 6, 18))
				{
					Trace.WriteLine("AnchorAsName is not supported by your version of tidylib - ignoring.");
					return true;
				}
				return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyAnchorAsName);
			}
			set
			{
				if (this.ReleaseDate < new DateTime(2008, 6, 18))
					Trace.WriteLine("AnchorAsName is not supported by your version of tidylib - ignoring.");
				else
					PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyAnchorAsName, value);
			}
		}

		/// <summary>
		/// [assume-xml-procins] Gets or sets whether Tidy should change the parsing of processing instructions to require ?&gt; as the terminator rather than &gt;. This option is automatically set if the input is in XML. Defaults to false.
		/// </summary>
		public bool ChangeXmlProcessingInstructions
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyXmlPIs); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyXmlPIs, value); }
		}

		/// <summary>
		/// [bare] Gets or sets whether Tidy should strip Microsoft specific HTML from Word 2000 documents, and output spaces rather than non-breaking spaces where they exist in the input. Defaults to false.
		/// </summary>
		public bool MakeBare
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyMakeBare); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyMakeBare, value); }
		}

		/// <summary>
		/// [clean] Gets or sets whether Tidy should strip out surplus presentational tags and attributes replacing them by style rules and structural markup as appropriate. It works well on the HTML saved by Microsoft Office products. Defaults to false.
		/// </summary>
		public bool MakeClean
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyMakeClean); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyMakeClean, value); }
		}

		/// <summary>
		/// [css-prefix] Gets or sets the prefix that Tidy uses for styles rules. By default, "c" will be used.
		/// </summary>
		public string CssPrefix
		{
			get { return PInvoke.tidyOptGetValueString(this.handle, TidyOptionId.TidyCSSPrefix); }
			set { PInvoke.tidyOptSetValue(this.handle, TidyOptionId.TidyCSSPrefix, value); }
		}

		/// <summary>
		/// [decorate-inferred-ul] Gets or sets whether Tidy should decorate inferred UL elements with some CSS markup to avoid indentation to the right. Defaults to false.
		/// </summary>
		public bool DecorateInferredUL
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyDecorateInferredUL); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyDecorateInferredUL, value); }
		}

		/// <summary>
		/// [doctype] Gets or sets the DOCTYPE declaration generated by Tidy. If set to "Omit" the output won't contain a DOCTYPE declaration. If set to "Auto" (the default) Tidy will use an educated guess based upon the contents of the document. If set to "Strict", Tidy will set the DOCTYPE to the strict DTD. If set to "Loose", the DOCTYPE is set to the loose (transitional) DTD. Alternatively, you can supply a string for the formal public identifier (FPI).
		/// <para>
		/// For example:
		/// doctype: "-//ACME//DTD HTML 3.14159//EN"
		/// </para>
		/// If you specify the FPI for an XHTML document, Tidy will set the system identifier to an empty string. For an HTML document, Tidy adds a system identifier only if one was already present in order to preserve the processing mode of some browsers. Tidy leaves the DOCTYPE for generic XML documents unchanged. "Omit" implies OutputNumericEntities = true. This option does not offer a validation of the document conformance. 
		/// </summary>
		public DocTypeMode DocType
		{
			get { return (DocTypeMode) PInvoke.tidyOptGetInt(this.handle, TidyOptionId.TidyDoctypeMode); }
			set { PInvoke.tidyOptSetInt(this.handle, TidyOptionId.TidyDoctypeMode, (uint) value); }
		}

		/// <summary>
		/// [drop-empty-paras] Gets or sets whether Tidy should discard empty paragraphs. Defaults to true.
		/// </summary>
		public bool DropEmptyParagraphs
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyDropEmptyParas); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyDropEmptyParas, value); }
		}

		/// <summary>
		/// [drop-font-tags] Gets or sets whether Tidy should discard &lt;FONT&gt; and &lt;CENTER&gt; tags without creating the corresponding style rules. This option can be set independently of the MakeClean option. Defaults to false.
		/// </summary>
		public bool DropFontTags
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyDropFontTags); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyDropFontTags, value); }
		}

		/// <summary>
		/// [drop-proprietary-attributes] Gets or sets whether Tidy should strip out proprietary attributes, such as MS data binding attributes. Defaults to false.
		/// </summary>
		public bool DropProprietaryAttributes
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyDropPropAttrs); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyDropPropAttrs, value); }
		}

		/// <summary>
		/// [enclose-block-text] Gets or sets whether Tidy should insert a &lt;P&gt; element to enclose any text it finds in any element that allows mixed content for HTML transitional but not HTML strict. Defaults to false.
		/// </summary>
		public bool EncloseBlockText
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyEncloseBlockText); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyEncloseBlockText, value); }
		}

		/// <summary>
		/// [enclose-text] Gets or sets whether Tidy should enclose any text it finds in the body element within a &lt;P&gt; element. This is useful when you want to take existing HTML and use it with a style sheet. Defaults to false.
		/// </summary>
		public bool EncloseBodyText
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyEncloseBodyText); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyEncloseBodyText, value); }
		}

		/// <summary>
		/// [escape-cdata] Gets or sets whether Tidy should convert &lt;![CDATA[]]&gt; sections to normal text. Defaults to false.
		/// </summary>
		public bool EscapeCdata
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyEscapeCdata); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyEscapeCdata, value); }
		}

		/// <summary>
		/// [fix-backslash] Gets or sets whether Tidy should replace backslash characters "\" in URLs with forward slashes "/". Defaults to true.
		/// </summary>
		public bool FixUrlBackslashes
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyFixBackslash); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyFixBackslash, value); }
		}

		/// <summary>
		/// [fix-bad-comments] Gets or sets whether Tidy should replace unexpected hyphens with "=" characters when it comes across adjacent hyphens. This option is provided for users of Cold Fusion which uses the comment syntax: &lt;!--- ---&gt;. Defaults to true.
		/// </summary>
		public bool FixBadComments
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyFixComments); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyFixComments, value); }
		}

		/// <summary>
		/// [fix-uri] Gets or sets whether Tidy should check attribute values that carry URIs for illegal characters and if such are found, escape them as HTML 4 recommends. Defaults to true.
		/// </summary>
		public bool FixAttributeUris
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyFixUri); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyFixUri, value); }
		}

		/// <summary>
		/// [hide-comments] Gets or sets whether Tidy should print out comments. Defaults to false.
		/// </summary>
		public bool RemoveComments
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyHideComments); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyHideComments, value); }
		}

		/// <summary>
		/// [hide-endtags] Gets or sets whether Tidy should omit optional end-tags when generating the pretty printed markup. This option is ignored if you are outputting to XML. Defaults to false.
		/// </summary>
		public bool RemoveEndTags
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyHideEndTags); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyHideEndTags, value); }
		}

		/// <summary>
		/// [indent-cdata] Gets or sets whether Tidy should indent &lt;![CDATA[]]&gt; sections. Defaults to false.
		/// </summary>
		public bool IndentCdata
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyIndentCdata); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyIndentCdata, value); }
		}

		/// <summary>
		/// [input-xml] Gets or sets whether Tidy use the XML parser rather than the error correcting HTML parser. Defaults to false.
		/// </summary>
		public bool UseXmlParser
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyXmlTags); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyXmlTags, value); }
		}

		/// <summary>
		/// [join-classes] Gets or sets whether Tidy should combine class names to generate a single new class name, if multiple class assignments are detected on an element. Defaults to false.
		/// </summary>
		public bool JoinClasses
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyJoinClasses); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyJoinClasses, value); }
		}

		/// <summary>
		/// [join-styles] Gets or sets whether Tidy should combine styles to generate a single new style, if multiple style values are detected on an element. Defaults to true.
		/// </summary>
		public bool JoinStyles
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyJoinStyles); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyJoinStyles, value); }
		}

		/// <summary>
		/// [literal-attributes] Gets or sets whether Tidy should ensure that whitespace characters within attribute values are passed through unchanged. Defaults to false.
		/// </summary>
		public bool EnsureLiteralAttributes
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyLiteralAttribs); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyLiteralAttribs, value); }
		}

		/// <summary>
		/// [logical-emphasis] Gets or sets whether Tidy should replace any occurrence of &lt;I&gt; by &lt;EM&gt; and any occurrence of &lt;B&gt; by &lt;STRONG&gt;. In both cases, the attributes are preserved unchanged. This option can be set independently of the "MakeClean" and "DropFontTags" properties. Defaults to false.
		/// </summary>
		public bool UseLogicalEmphasis
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyLogicalEmphasis); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyLogicalEmphasis, value); }
		}

		/// <summary>
		/// [lower-literals] Gets or sets whether Tidy should convert the value of an attribute that takes a list of predefined values to lower case. This is required for XHTML documents. Defaults to false.
		/// </summary>
		public bool LowerCaseLiterals
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyLowerLiterals); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyLowerLiterals, value); }
		}

		/// <summary>
		/// [merge-divs] Gets or sets whether Tidy should merge nested &lt;div&gt; such as "&lt;div&gt;&lt;divglt;...&lt;/div&gt;&lt;/div&gt;". If set to "Auto", the attributes of the inner &lt;div&gt; are moved to the outer one. As well, nested &lt;div&gt; with ID attributes are not merged. If set to "Yes", the attributes of the inner &lt;div&gt; are discarded with the exception of "class" and "style". Can be used to modify behavior of the "MakeClean" option. Defaults to Auto.
		/// </summary>
		public AutoBool MergeDivs
		{
			get { return (AutoBool) PInvoke.tidyOptGetInt(this.handle, TidyOptionId.TidyMergeDivs); }
			set { PInvoke.tidyOptSetInt(this.handle, TidyOptionId.TidyMergeDivs, (uint) value); }
		}

		/// <summary>
		/// [merge-spans] Gets or sets whether Tidy should merge nested &lt;span&gt; such as "&lt;span&gt;&lt;span;...&lt;/span&gt;&lt;/span&gt;". The algorithm is identical to the one used by MergeDivs. Can be used to modify behavior of the "MakeClean" option. Defaults to "Auto".
		/// </summary>
		public AutoBool MergeSpans
		{
			// Not available before until 13 Aug 2007
			get
			{
				if (this.ReleaseDate < new DateTime(2007, 8, 13))
				{
					Trace.WriteLine("MergeSpans is not supported by your version of tidylib - ignoring.");
					return AutoBool.No;
				}
				return (AutoBool) PInvoke.tidyOptGetInt(this.handle, TidyOptionId.TidyMergeSpans);
			}
			set
			{
				if (this.ReleaseDate < new DateTime(2007, 8, 13))
					Trace.WriteLine("MergeSpans is not supported by your version of tidylib - ignoring.");
				else
					PInvoke.tidyOptSetInt(this.handle, TidyOptionId.TidyMergeSpans, (uint) value);
			}
		}

#if SUPPORT_ASIAN_ENCODINGS
		/// <summary>
		/// [ncr] Gets or sets whether Tidy should allow numeric character references. Defaults to true.
		/// </summary>
		public bool AllowNumericCharacterReferences
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyNCR); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyNCR, value); }
		}
#endif

		/// <summary>
		/// [new-blocklevel-tags] Gets or sets new block-level tags. This option takes a space or comma separated list of tag names. Unless you declare new tags, Tidy will refuse to generate a tidied file if the input includes previously unknown tags. Note you can't change the content model for elements such as &lt;TABLE&gt;, &lt;UL&gt;, &lt;OL&gt; and &lt;DL&gt;. This option is ignored in XML mode.
		/// </summary>
		public string NewBlockLevelTags
		{
			get { return PInvoke.tidyOptGetValueString(this.handle, TidyOptionId.TidyBlockTags); }
			set { PInvoke.tidyOptSetValue(this.handle, TidyOptionId.TidyBlockTags, value); }
		}

		/// <summary>
		/// [new-empty-tags] Gets or sets new empty inline tags. This option takes a space or comma separated list of tag names. Unless you declare new tags, Tidy will refuse to generate a tidied file if the input includes previously unknown tags. This option is ignored in XML mode.
		/// </summary>
		public string NewEmptyInlineTags
		{
			get { return PInvoke.tidyOptGetValueString(this.handle, TidyOptionId.TidyEmptyTags); }
			set { PInvoke.tidyOptSetValue(this.handle, TidyOptionId.TidyEmptyTags, value); }
		}

		/// <summary>
		/// [new-inline-tags] Gets or sets new non-empty inline tags. This option takes a space or comma separated list of tag names. Unless you declare new tags, Tidy will refuse to generate a tidied file if the input includes previously unknown tags. This option is ignored in XML mode.
		/// </summary>
		public string NewInlineTags
		{
			get { return PInvoke.tidyOptGetValueString(this.handle, TidyOptionId.TidyInlineTags); }
			set { PInvoke.tidyOptSetValue(this.handle, TidyOptionId.TidyInlineTags, value); }
		}

		/// <summary>
		/// [new-pre-tags] Gets or sets new tags that are to be processed in exactly the same way as HTML's &lt;PRE&gt; element. This option takes a space or comma separated list of tag names. Unless you declare new tags, Tidy will refuse to generate a tidied file if the input includes previously unknown tags. Note you can not as yet add new CDATA elements (similar to &lt;SCRIPT&gt;). This option is ignored in XML mode.
		/// </summary>
		public string NewPreTags
		{
			get { return PInvoke.tidyOptGetValueString(this.handle, TidyOptionId.TidyPreTags); }
			set { PInvoke.tidyOptSetValue(this.handle, TidyOptionId.TidyPreTags, value); }
		}

		/// <summary>
		/// [numeric-entities] Gets or sets whether Tidy should output entities other than the built-in HTML entities (&amp;amp;, &amp;lt;, &amp;gt; and &amp;quot;) in the numeric rather than the named entity form. Only entities compatible with the DOCTYPE declaration generated are used. Entities that can be represented in the output encoding are translated correspondingly. Defaults to false.
		/// </summary>
		public bool OutputNumericEntities
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyNumEntities); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyNumEntities, value); }
		}

		/// <summary>
		/// [output-html] Gets or sets whether Tidy should generate pretty printed output, writing it as HTML. Defaults to false.
		/// </summary>
		public bool OutputHtml
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyHtmlOut); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyHtmlOut, value); }
		}

		/// <summary>
		/// [output-xhtml] Gets or sets whether Tidy should generate pretty printed output, writing it as extensible HTML. This option causes Tidy to set the DOCTYPE and default namespace as appropriate to XHTML. If a DOCTYPE or namespace is given they will checked for consistency with the content of the document. In the case of an inconsistency, the corrected values will appear in the output. For XHTML, entities can be written as named or numeric entities according to the setting of the "OutputNumericEntities" value. The original case of tags and attributes will be preserved, regardless of other options. Defaults to false.
		/// </summary>
		public bool OutputXhtml
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyXhtmlOut); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyXhtmlOut, value); }
		}

		/// <summary>
		/// [output-xml] Gets or sets whether Tidy should generate pretty printed output, writing it as well-formed XML. Any entities not defined in XML 1.0 will be written as numeric entities to allow them to be parsed by a XML parser. The original case of tags and attributes will be preserved, regardless of other options. Defaults to false.
		/// </summary>
		public bool OutputXml
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyXmlOut); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyXmlOut, value); }
		}

		/// <summary>
		/// [preserve-entities] Gets or sets whether Tidy should preserve the well-formed entitites as found in the input. Defaults to false.
		/// </summary>
		public bool PreserveEntities
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyPreserveEntities); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyPreserveEntities, value); }
		}

		/// <summary>
		/// [quote-ampersand] Gets or sets whether Tidy should output unadorned &amp; characters as &amp;amp;. Defaults to true.
		/// </summary>
		public bool QuoteAmpersands
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyQuoteAmpersand); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyQuoteAmpersand, value); }
		}

		/// <summary>
		/// [quote-marks] Gets or sets whether Tidy should output " characters as &amp;quot; as is preferred by some editing environments. The apostrophe character ' is written out as &amp;#39; since many web browsers don't yet support &amp;apos;. Defaults to false.
		/// </summary>
		public bool QuoteMarks
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyQuoteMarks); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyQuoteMarks, value); }
		}

		/// <summary>
		/// [quote-nbsp] Gets or sets whether Tidy should output non-breaking space characters as entities, rather than as the Unicode character value 160 (decimal). Defaults to true.
		/// </summary>
		public bool QuoteNonBreakingSpaces
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyQuoteNbsp); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyQuoteNbsp, value); }
		}

		/// <summary>
		/// [repeated-attributes] Gets or sets whether Tidy should keep the first or last attribute, if an attribute is repeated, e.g. has two align attributes. Defaults to "KeepLast".
		/// </summary>
		public RepeatedAttributeMode RepeatedAttributeMode
		{
			get { return (RepeatedAttributeMode) PInvoke.tidyOptGetInt(this.handle, TidyOptionId.TidyDuplicateAttrs); }
			set { PInvoke.tidyOptSetInt(this.handle, TidyOptionId.TidyDuplicateAttrs, (uint) value); }
		}

		/// <summary>
		/// [replace-color] Gets or sets whether Tidy should replace numeric values in color attributes by HTML/XHTML color names where defined, e.g. replace "#ffffff" with "white". Defaults to false.
		/// </summary>
		public bool UseColorNames
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyReplaceColor); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyReplaceColor, value); }
		}

		/// <summary>
		/// [show-body-only] Gets or sets whether Tidy should print only the contents of the body tag as an HTML fragment. If set to "Auto", this is performed only if the body tag has been inferred. Useful for incorporating existing whole pages as a portion of another page. This option has no effect if XML output is requested. Defaults to "No".
		/// </summary>
		public AutoBool OutputBodyOnly
		{
			// This option was changed from a Bool to an AutoBool on 24 May 2007.
			get
			{
				if (this.ReleaseDate < new DateTime(2007, 5, 24))
					return (PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyBodyOnly) ? AutoBool.Yes : AutoBool.No);
				else
					return (AutoBool) PInvoke.tidyOptGetInt(this.handle, TidyOptionId.TidyBodyOnly);
			}
			set
			{
				if (this.ReleaseDate < new DateTime(2007, 5, 24))
					PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyBodyOnly, (value == AutoBool.Yes));
				else
					PInvoke.tidyOptSetInt(this.handle, TidyOptionId.TidyBodyOnly, (uint) value);
			}
		}

		/// <summary>
		/// [uppercase-attributes] Gets or sets whether Tidy should output attribute names in upper case. The default is false, which results in lower case attribute names, except for XML input, where the original case is preserved.
		/// </summary>
		public bool UpperCaseAttributes
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyUpperCaseAttrs); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyUpperCaseAttrs, value); }
		}

		/// <summary>
		/// [uppercase-tags] Gets or sets whether Tidy should output tag names in upper case. The default is false, which results in lower case tag names, except for XML input, where the original case is preserved.
		/// </summary>
		public bool UpperCaseTags
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyUpperCaseTags); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyUpperCaseTags, value); }
		}

		/// <summary>
		/// [word-2000] Gets or sets whether Tidy should go to great pains to strip out all the surplus stuff Microsoft Word 2000 inserts when you save Word documents as "Web pages". Doesn't handle embedded images or VML. You should consider using Word's "Save As: Web Page, Filtered". Defaults to false.
		/// </summary>
		public bool CleanWord2000
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyWord2000); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyWord2000, value); }
		}

		#endregion

		#region Diagnostics Options

		/// <summary>
		/// [accessibility-check] Gets or sets the level of accessibility checking, if any, that Tidy should do. Defaults to TidyClassic.
		/// </summary>
		public AccessibilityCheckLevel AccessibilityCheckLevel
		{
			get { return (AccessibilityCheckLevel) PInvoke.tidyOptGetInt(this.handle, TidyOptionId.TidyAccessibilityCheckLevel); }
			set { PInvoke.tidyOptSetInt(this.handle, TidyOptionId.TidyAccessibilityCheckLevel, (uint) value); }
		}

		/// <summary>
		/// [show-errors] Gets or sets the number Tidy uses to determine if further errors should be shown. If set to 0, then no errors are shown. Defaults to 6.
		/// </summary>
		public int MaximumErrors
		{
			get { return (int) PInvoke.tidyOptGetInt(this.handle, TidyOptionId.TidyShowErrors); }
			set
			{
				if (value < 0) value = 0;
				PInvoke.tidyOptSetInt(this.handle, TidyOptionId.TidyShowErrors, (uint) value);
			}
		}

		/// <summary>
		/// [show-warnings] Gets or sets whether Tidy should suppress warnings. This can be useful when a few errors are hidden in a flurry of warnings. Defaults to true.
		/// </summary>
		public bool ShowWarnings
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyShowWarnings); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyShowWarnings, value); }
		}

		#endregion

		#region Pretty Print Options

		/// <summary>
		/// [break-before-br] Gets or sets whether Tidy should output a line break before each &lt;BR&gt; element. Defaults to false.
		/// </summary>
		public bool LineBreakBeforeBR
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyBreakBeforeBR); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyBreakBeforeBR, value); }
		}

		/// <summary>
		/// [indent] Gets or sets whether Tidy should indent block-level tags. If set to Auto, this option causes Tidy to decide whether or not to indent the content of tags such as TITLE, H1-H6, LI, TD, TD, or P depending on whether or not the content includes a block-level element. You are advised to avoid setting indent to Yes as this can expose layout bugs in some browsers. Defaults to No.
		/// </summary>
		public AutoBool IndentBlockElements
		{
			get { return (AutoBool) PInvoke.tidyOptGetInt(this.handle, TidyOptionId.TidyIndentContent); }
			set { PInvoke.tidyOptSetInt(this.handle, TidyOptionId.TidyIndentContent, (uint) value); }
		}

		/// <summary>
		/// [indent-attributes] Gets or sets whether Tidy should begin each attribute on a new line. Defaults to false.
		/// </summary>
		public bool IndentAttributes
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyIndentAttributes); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyIndentAttributes, value); }
		}

		/// <summary>
		/// [indent-spaces] Gets or sets the number of spaces Tidy uses to indent content, when indentation is enabled. Defaults to 2.
		/// </summary>
		public int IndentSpaces
		{
			get { return (int) PInvoke.tidyOptGetInt(this.handle, TidyOptionId.TidyIndentSpaces); }
			set { PInvoke.tidyOptSetInt(this.handle, TidyOptionId.TidyIndentSpaces, (uint) value); }
		}

		/// <summary>
		/// [markup] Gets or sets whether Tidy should generate a pretty printed version of the markup. Note that Tidy won't generate a pretty printed version if it finds significant errors (see ForceOutput). Defaults to true.
		/// </summary>
		public bool Markup
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyShowMarkup); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyShowMarkup, value); }
		}

#if SUPPORT_ASIAN_ENCODINGS
		/// <summary>
		/// [punctuation-wrap] Gets or sets whether Tidy should line wrap after some Unicode or Chinese punctuation characters. Defaults to false.
		/// </summary>
		public bool PunctuationWrap
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyPunctWrap); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyPunctWrap, value); }
		}
#endif

		/// <summary>
		/// [sort-attributes] Gets or sets how Tidy should sort attributes within an element using the specified sort algorithm. If set to Alpha, the algorithm is an ascending alphabetic sort. Defaults to None.
		/// </summary>
		public SortStrategy AttributeSortType
		{
			// Not available before until 6 Jun 2007
			get
			{
				if (this.ReleaseDate < new DateTime(2007, 6, 12))
				{
					Trace.WriteLine("AttributeSortType is not supported by your version of tidylib - ignoring.");
					return SortStrategy.None;
				}
				return (SortStrategy) PInvoke.tidyOptGetInt(this.handle, TidyOptionId.TidySortAttributes);
			}
			set
			{
				if (this.ReleaseDate < new DateTime(2007, 6, 12))
					Trace.WriteLine("AttributeSortType is not supported by your version of tidylib - ignoring.");
				else
					PInvoke.tidyOptSetInt(this.handle, TidyOptionId.TidySortAttributes, (uint) value);
			}
		}

		/// <summary>
		/// [tab-size] Gets or sets the number of columns that Tidy uses between successive tab stops. It is used to map tabs to spaces when reading the input. Tidy never outputs tabs. Defaults to 8.
		/// </summary>
		public int TabSize
		{
			get { return (int) PInvoke.tidyOptGetInt(this.handle, TidyOptionId.TidyTabSize); }
			set { PInvoke.tidyOptSetInt(this.handle, TidyOptionId.TidyTabSize, (uint) value); }
		}

		/// <summary>
		/// [vertical-space] Gets or sets whether Tidy should add some empty lines for readability. Defaults to false.
		/// </summary>
		public bool AddVerticalSpace
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyVertSpace); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyVertSpace, value); }
		}

		/// <summary>
		/// [wrap] Gets or sets the right margin Tidy uses for line wrapping. Tidy tries to wrap lines so that they do not exceed this length. Set wrap to zero if you want to disable line wrapping. Defaults to 68.
		/// </summary>
		public int WrapAt
		{
			get { return (int) PInvoke.tidyOptGetInt(this.handle, TidyOptionId.TidyWrapLen); }
			set { PInvoke.tidyOptSetInt(this.handle, TidyOptionId.TidyWrapLen, (uint) value); }
		}

		/// <summary>
		/// [wrap-asp] Gets or sets whether Tidy should line wrap text contained within ASP pseudo elements, which look like: &lt;% ... %&gt;. Defaults to true.
		/// </summary>
		public bool WrapAsp
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyWrapAsp); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyWrapAsp, value); }
		}

		/// <summary>
		/// [wrap-attributes] Gets or sets whether Tidy should line wrap attribute values, for easier editing. This option can be set independently of WrapAcriptLiterals. Defaults to false.
		/// </summary>
		public bool WrapAttributeValues
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyWrapAttVals); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyWrapAttVals, value); }
		}

		/// <summary>
		/// [wrap-jste] Gets or sets whether Tidy should line wrap text contained within JSTE  pseudo elements, which look like: &lt;# ... #&gt;. Defaults to true.
		/// </summary>
		public bool WrapJste
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyWrapJste); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyWrapJste, value); }
		}

		/// <summary>
		/// [wrap-php] Gets or sets whether Tidy should line wrap text contained within PHP pseudo elements, which look like: &lt;?php ... ?&gt;. Defaults to true.
		/// </summary>
		public bool WrapPhp
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyWrapPhp); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyWrapPhp, value); }
		}

		/// <summary>
		/// [wrap-script-literals] Gets or sets whether Tidy should line wrap string literals that appear in script attributes. Tidy wraps long script string literals by inserting a backslash character before the line break. Defaults to false.
		/// </summary>
		public bool WrapScriptLiterals
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyWrapScriptlets); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyWrapScriptlets, value); }
		}

		/// <summary>
		/// [wrap-sections] Gets or sets whether Tidy should line wrap text contained within &lt;![ ... ]&gt; section tags. Defaults to true.
		/// </summary>
		public bool WrapSections
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyWrapSection); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyWrapSection, value); }
		}

		#endregion

		#region Character Encoding Options

		/// <summary>
		/// [ascii-chars] Gets or sets whether &amp;emdash;, &amp;rdquo;, and other named character entities are downgraded to their closest ascii equivalents when the "MakeClean" option is set to true. Defaults to false.
		/// </summary>
		public bool AsciiEntities
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyAsciiChars); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyAsciiChars, value); }
		}

		/// <summary>
		/// [char-encoding] Gets or sets character encoding Tidy uses for both the input and output. For ascii, Tidy will accept Latin-1 (ISO-8859-1) character values, but will use entities for all characters whose value > 127. For raw, Tidy will output values above 127 without translating them into entities. For latin1, characters above 255 will be written as entities. For utf8, Tidy assumes that both input and output is encoded as UTF-8. You can use iso2022 for files encoded using the ISO-2022 family of encodings e.g. ISO-2022-JP. For mac and win1252, Tidy will accept vendor specific character values, but will use entities for all characters whose value > 127. For unsupported encodings, use an external utility to convert to and from UTF-8. Defaults to "Ascii".
		/// </summary>
		public EncodingType CharacterEncoding
		{
			get { return (EncodingType) PInvoke.tidyOptGetInt(this.handle, TidyOptionId.TidyCharEncoding); }
			set { PInvoke.tidyOptSetInt(this.handle, TidyOptionId.TidyCharEncoding, (uint) value); }
		}

		/// <summary>
		/// [input-encoding] Gets or sets character encoding Tidy uses for the input. See CharacterEncoding for more info. Defaults to "Latin1".
		/// </summary>
		public EncodingType InputCharacterEncoding
		{
			get { return (EncodingType) PInvoke.tidyOptGetInt(this.handle, TidyOptionId.TidyInCharEncoding); }
			set { PInvoke.tidyOptSetInt(this.handle, TidyOptionId.TidyInCharEncoding, (uint) value); }
		}

		/// <summary>
		/// [newline] Gets or sets the type of newline. The default is appropriate to the current platform: CRLF on PC-DOS, MS-Windows and OS/2, CR on Classic Mac OS, and LF everywhere else (Unix and Linux).
		/// </summary>
		public NewlineType NewLine
		{
			get { return (NewlineType) PInvoke.tidyOptGetInt(this.handle, TidyOptionId.TidyNewline); }
			set { PInvoke.tidyOptSetInt(this.handle, TidyOptionId.TidyNewline, (uint) value); }
		}

#if SUPPORT_UTF16_ENCODINGS
		/// <summary>
		/// [output-bom] Gets or sets whether Tidy should write a Unicode Byte Order Mark character (BOM; also known as Zero Width No-Break Space; has value of U+FEFF) to the beginning of the output; only for UTF-8 and UTF-16 output encodings. If set to "auto", this option causes Tidy to write a BOM to the output only if a BOM was present at the beginning of the input. A BOM is always written for XML/XHTML output using UTF-16 output encodings. Defaults to "Auto".
		/// </summary>
		public AutoBool OutputByteOrderMark
		{
			get { return (AutoBool) PInvoke.tidyOptGetInt(this.handle, TidyOptionId.TidyOutputBOM); }
			set { PInvoke.tidyOptSetInt(this.handle, TidyOptionId.TidyOutputBOM, (uint) value); }
		}
#endif

		/// <summary>
		/// [output-encoding] Gets or sets character encoding Tidy uses for the output. See CharacterEncoding for more info. May only be different from input-encoding for Latin encodings (ascii, latin0, latin1, mac, win1252, ibm858). Defaults to "Ascii".
		/// </summary>
		public EncodingType OutputCharacterEncoding
		{
			get { return (EncodingType) PInvoke.tidyOptGetInt(this.handle, TidyOptionId.TidyOutCharEncoding); }
			set { PInvoke.tidyOptSetInt(this.handle, TidyOptionId.TidyOutCharEncoding, (uint) value); }
		}

		#endregion

		#region Miscellaneous Options

		/// <summary>
		/// [error-file] Gets or sets the error file Tidy uses for errors and warnings. Normally errors and warnings are output to "stderr". Defaults to null.
		/// </summary>
		public string ErrorFile
		{
			get { return PInvoke.tidyOptGetValueString(this.handle, TidyOptionId.TidyErrFile); }
			set { PInvoke.tidyOptSetValue(this.handle, TidyOptionId.TidyErrFile, value); }
		}

		/// <summary>
		/// [force-output] Gets or sets whether Tidy should produce output even if errors are encountered. Use this option with care - if Tidy reports an error, this means Tidy was not able to, or is not sure how to, fix the error, so the resulting output may not reflect your intention. Defaults to false.
		/// </summary>
		public bool ForceOutput
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyForceOutput); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyForceOutput, value); }
		}

		/// <summary>
		/// [gnu-emacs] Gets or sets whether Tidy should change the format for reporting errors and warnings to a format that is more easily parsed by GNU Emacs. Defaults to false.
		/// </summary>
		public bool UseGnuEmacsErrorFormat
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyEmacs); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyEmacs, value); }
		}

		/// <summary>
		/// [keep-time] Gets or sets whether Tidy should keep the original modification time of files that Tidy modifies in place. The default is no. Setting the option to yes allows you to tidy files without causing these files to be uploaded to a web server when using a tool such as SiteCopy. Note this feature is not supported on some platforms. Defaults to false.
		/// </summary>
		public bool KeepModificationTimestamp
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyKeepFileTimes); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyKeepFileTimes, value); }
		}

		/// <summary>
		/// [output-file] Gets or sets the output file Tidy uses for markup. Normally markup is written to "stdout". Defaults to null.
		/// </summary>
		public string OutputFile
		{
			get { return PInvoke.tidyOptGetValueString(this.handle, TidyOptionId.TidyOutFile); }
			set { PInvoke.tidyOptSetValue(this.handle, TidyOptionId.TidyOutFile, value); }
		}

		/// <summary>
		/// [quiet] Gets or sets whether Tidy should output the summary of the numbers of errors and warnings, or the welcome or informational messages. Defaults to false.
		/// </summary>
		public bool Quiet
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyQuiet); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyQuiet, value); }
		}

		/// <summary>
		/// [tidy-mark] Gets or sets whether Tidy should add a meta element to the document head to indicate that the document has been tidied. Tidy won't add a meta element if one is already present. Defaults to true.
		/// </summary>
		public bool AddTidyMetaElement
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyMark); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyMark, value); }
		}

		/// <summary>
		/// [write-back] Gets or sets whether Tidy should write back the tidied markup to the same file it read from. You are advised to keep copies of important files before tidying them, as on rare occasions the result may not be what you expect. Defaults to false.
		/// </summary>
		public bool WriteBack
		{
			get { return PInvoke.tidyOptGetBool(this.handle, TidyOptionId.TidyWriteBack); }
			set { PInvoke.tidyOptSetBool(this.handle, TidyOptionId.TidyWriteBack, value); }
		}

		#endregion

		#endregion

		#region Methods

        /// <summary>
        /// Parses input markup, and executes configured cleanup and repair operations.
        /// </summary>
        /// <returns>A log of the errors encountered during the CleanAndRepair operation.</returns>
		public string CleanAndRepair()
		{
            using (Stream stream = new MemoryStream())
            {
                CleanAndRepair(stream);
                stream.Seek(0, SeekOrigin.Begin);
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
		}

        /// <summary>
        /// Parses input markup, and executes configured cleanup and repair operations.
        /// </summary>
        /// <param name="logStream">A stream to which errors encountered during the CleanAndRepair operation will be written to.</param>
        public void CleanAndRepair(Stream logStream)
        {
            //Config Error
            EncodingType tempOutEnc = this.OutputCharacterEncoding;
            this.OutputCharacterEncoding = EncodingType.Utf8;
            OutputSink sink = new OutputSink(logStream);
            PInvoke.tidySetErrorSink(this.handle, ref sink.TidyOutputSink);
            if (fromString)
            {
                EncodingType tempEnc = this.InputCharacterEncoding;
                this.InputCharacterEncoding = EncodingType.Utf8;
                PInvoke.tidyParseString(this.handle, this.htmlString);
                this.InputCharacterEncoding = tempEnc;
            }
            else
            {
                InputSource input = new InputSource(this.stream);
                PInvoke.tidyParseSource(this.handle, ref input.TidyInputSource);
            }
            PInvoke.tidyCleanAndRepair(this.handle);
            this.OutputCharacterEncoding = tempOutEnc;
            cleaned = true;
        }

		/// <summary>
		/// Saves the processed markup to a string.
		/// </summary>
		/// <returns>A string containing the processed markup.</returns>
		public string Save()
		{
			if (!cleaned)
				throw new InvalidOperationException("CleanAndRepair() must be called before Save().");

			var tempEnc = this.CharacterEncoding;
			var tempBOM = this.OutputByteOrderMark;
			this.OutputCharacterEncoding = EncodingType.Utf8;
			this.OutputByteOrderMark = AutoBool.No;

			uint bufferLength = 1;
			byte[] htmlBytes;
			GCHandle handle = new GCHandle();
			do
			{
				// Buffer was too small - bufferLength should now be the required length, so try again...
				if (handle.IsAllocated) handle.Free();

				// this setting appears to be reset by libtidy after calling tidySaveString; we need to set it each time
				this.OutputCharacterEncoding = EncodingType.Utf8;

				htmlBytes = new byte[bufferLength];
				handle = GCHandle.Alloc(htmlBytes, GCHandleType.Pinned);
			} while (PInvoke.tidySaveString(this.handle, handle.AddrOfPinnedObject(), ref bufferLength) == -12);

			handle.Free();

			this.OutputCharacterEncoding = tempEnc;
			this.OutputByteOrderMark = tempBOM;
			return Encoding.UTF8.GetString(htmlBytes);
		}

		/// <summary>
		/// Saves the processed markup to a file.
		/// </summary>
		/// <param name="filePath">The full filesystem path of the file to save the markup to.</param>
		public void Save(string filePath)
		{
			if (!cleaned)
				throw new InvalidOperationException("CleanAndRepair() must be called before Save().");

			PInvoke.tidySaveFile(this.handle, filePath);
		}

		/// <summary>
		/// Saves the processed markup to the supplied stream.
		/// </summary>
		/// <param name="stream">A <see cref="System.IO.Stream"/> to write the markup to.</param>
		public void Save(Stream stream)
		{
			if (!cleaned)
				throw new InvalidOperationException("CleanAndRepair() must be called before Save().");

			EncodingType tempEnc = this.OutputCharacterEncoding;
			if (fromString) this.OutputCharacterEncoding = EncodingType.Utf8;
			OutputSink sink = new OutputSink(stream);
			PInvoke.tidySaveSink(this.handle, ref sink.TidyOutputSink);
			if (fromString) this.OutputCharacterEncoding = tempEnc;
		}

		#endregion

		#region Static Methods

		/// <summary>
		/// Creates a new <see cref="Document"/> instance from a <see cref="System.String"/> containing HTML.
		/// </summary>
		/// <param name="htmlString">The HTML string to be processed.</param>
		public static Document FromString(string htmlString)
		{
			if (htmlString == null)
				throw new ArgumentNullException("htmlString");

			return new Document(htmlString);
		}

		/// <summary>
		/// Creates a new <see cref="Document"/> instance from a file.
		/// </summary>
		/// <param name="filePath">The full filesystem path of the HTML document to be processed.</param>
		public static Document FromFile(string filePath)
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException("File not found.", filePath);

			return new Document(new FileStream(filePath, FileMode.Open));
		}

		/// <summary>
		/// Creates a new <see cref="Document"/> instance from a <see cref="System.IO.Stream"/> instance.
		/// </summary>
		/// <param name="stream">A <see cref="System.IO.Stream"/> instance containing the HTML document to be processed.</param>
		public static Document FromStream(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");
			if (!stream.CanRead)
				throw new ArgumentException("Stream must be readable.");
			if (!stream.CanSeek)
				throw new ArgumentException("Stream must be seekable.");

			return new Document(stream);
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Disposes of all unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Disposes of all unmanaged resources.
		/// </summary>
		/// <param name="disposing">Indicates whether the the document is already being disposed of.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					if (this.stream != null) this.stream.Dispose();
					PInvoke.tidyRelease(this.handle);
				}
				this.handle = IntPtr.Zero;
				this.stream = null;
				this.disposed = true;
			}
		}

		#endregion
	}
}
