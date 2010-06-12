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

namespace TidyManaged.Interop
{
	internal enum TidyOptionId
	{
		TidyUnknownOption,   /*< Unknown option! */
		TidyIndentSpaces,    /*< Indentation n spaces */
		TidyWrapLen,         /*< Wrap margin */
		TidyTabSize,         /*< Expand tabs to n spaces */
		TidyCharEncoding,    /*< In/out character encoding */
		TidyInCharEncoding,  /*< Input character encoding (if different) */
		TidyOutCharEncoding, /*< Output character encoding (if different) */
		TidyNewline,         /*< Output line ending (default to platform) */
		TidyDoctypeMode,     /*< See doctype property */
		TidyDoctype,         /*< User specified doctype */
		TidyDuplicateAttrs,  /*< Keep first or last duplicate attribute */
		TidyAltText,         /*< Default text for alt attribute */

		[Obsolete]
		TidySlideStyle,      /*< Style sheet for slides: not used for anything yet */

		TidyErrFile,         /*< File name to write errors to */
		TidyOutFile,         /*< File name to write markup to */
		TidyWriteBack,       /*< If true then output tidied markup */
		TidyShowMarkup,      /*< If false, normal output is suppressed */
		TidyShowWarnings,    /*< However errors are always shown */
		TidyQuiet,           /*< No 'Parsing X', guessed DTD or summary */
		TidyIndentContent,   /*< Indent content of appropriate tags */
		/*< "auto" does text/block level content indentation */
		TidyHideEndTags,     /*< Suppress optional end tags */
		TidyXmlTags,         /*< Treat input as XML */
		TidyXmlOut,          /*< Create output as XML */
		TidyXhtmlOut,        /*< Output extensible HTML */
		TidyHtmlOut,         /*< Output plain HTML, even for XHTML input.
                           Yes means set explicitly. */
		TidyXmlDecl,         /*< Add <?xml?> for XML docs */
		TidyUpperCaseTags,   /*< Output tags in upper not lower case */
		TidyUpperCaseAttrs,  /*< Output attributes in upper not lower case */
		TidyMakeBare,        /*< Make bare HTML: remove Microsoft cruft */
		TidyMakeClean,       /*< Replace presentational clutter by style rules */
		TidyLogicalEmphasis, /*< Replace i by em and b by strong */
		TidyDropPropAttrs,   /*< Discard proprietary attributes */
		TidyDropFontTags,    /*< Discard presentation tags */
		TidyDropEmptyParas,  /*< Discard empty p elements */
		TidyFixComments,     /*< Fix comments with adjacent hyphens */
		TidyBreakBeforeBR,   /*< Output newline before <br> or not? */

		[Obsolete]
		TidyBurstSlides,     /*< Create slides on each h2 element */

		TidyNumEntities,     /*< Use numeric entities */
		TidyQuoteMarks,      /*< Output " marks as &quot; */
		TidyQuoteNbsp,       /*< Output non-breaking space as entity */
		TidyQuoteAmpersand,  /*< Output naked ampersand as &amp; */
		TidyWrapAttVals,     /*< Wrap within attribute values */
		TidyWrapScriptlets,  /*< Wrap within JavaScript string literals */
		TidyWrapSection,     /*< Wrap within <![ ... ]> section tags */
		TidyWrapAsp,         /*< Wrap within ASP pseudo elements */
		TidyWrapJste,        /*< Wrap within JSTE pseudo elements */
		TidyWrapPhp,         /*< Wrap within PHP pseudo elements */
		TidyFixBackslash,    /*< Fix URLs by replacing \ with / */
		TidyIndentAttributes,/*< Newline+indent before each attribute */
		TidyXmlPIs,          /*< If set to yes PIs must end with ?> */
		TidyXmlSpace,        /*< If set to yes adds xml:space attr as needed */
		TidyEncloseBodyText, /*< If yes text at body is wrapped in P's */
		TidyEncloseBlockText,/*< If yes text in blocks is wrapped in P's */
		TidyKeepFileTimes,   /*< If yes last modied time is preserved */
		TidyWord2000,        /*< Draconian cleaning for Word2000 */
		TidyMark,            /*< Add meta element indicating tidied doc */
		TidyEmacs,           /*< If true format error output for GNU Emacs */
		TidyEmacsFile,       /*< Name of current Emacs file */
		TidyLiteralAttribs,  /*< If true attributes may use newlines */
		TidyBodyOnly,        /*< Output BODY content only */
		TidyFixUri,          /*< Applies URI encoding if necessary */
		TidyLowerLiterals,   /*< Folds known attribute values to lower case */
		TidyHideComments,    /*< Hides all (real) comments in output */
		TidyIndentCdata,     /*< Indent <!CDATA[ ... ]]> section */
		TidyForceOutput,     /*< Output document even if errors were found */
		TidyShowErrors,      /*< Number of errors to put out */
		TidyAsciiChars,      /*< Convert quotes and dashes to nearest ASCII char */
		TidyJoinClasses,     /*< Join multiple class attributes */
		TidyJoinStyles,      /*< Join multiple style attributes */
		TidyEscapeCdata,     /*< Replace <![CDATA[]]> sections with escaped text */
#if SUPPORT_ASIAN_ENCODINGS
		TidyLanguage,        /*< Language property: not used for anything yet */
		TidyNCR,             /*< Allow numeric character references */
#else
		TidyLanguageNotUsed,
		TidyNCRNotUsed,
#endif
#if SUPPORT_UTF16_ENCODINGS
		TidyOutputBOM,      /**< Output a Byte Order Mark (BOM) for UTF-16 encodings */
		                    /**< auto: if input stream has BOM, we output a BOM */
#else
		TidyOutputBOMNotUsed,
#endif
		TidyReplaceColor,    /*< Replace hex color attribute values with names */
		TidyCSSPrefix,       /*< CSS class naming for -clean option */
		TidyInlineTags,      /*< Declared inline tags */
		TidyBlockTags,       /*< Declared block tags */
		TidyEmptyTags,       /*< Declared empty tags */
		TidyPreTags,         /*< Declared pre tags */
		TidyAccessibilityCheckLevel, /*< Accessibility check level
                                   0 (old style), or 1, 2, 3 */
		TidyVertSpace,       /*< degree to which markup is spread out vertically */
#if SUPPORT_ASIAN_ENCODINGS
		TidyPunctWrap,       /*< consider punctuation and breaking spaces for wrapping */
#else
		TidyPunctWrapNotUsed,
#endif
		TidyMergeDivs,       /*< Merge multiple DIVs */
		TidyDecorateInferredUL,  /*< Mark inferred UL elements with no indent CSS */
		TidyPreserveEntities,    /*< Preserve entities */
		TidySortAttributes,      /*< Sort attributes */
		TidyMergeSpans,       /*< Merge multiple SPANs */
		TidyAnchorAsName,    /*< Define anchors as name attributes */
		N_TIDY_OPTIONS       /*< Must be last */
	}
}
