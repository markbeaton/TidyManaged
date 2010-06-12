# TidyManaged

This is a managed .NET/Mono wrapper for the open source, cross-platform Tidy library, a HTML/XHTML/XML markup parser & cleaner originally created by Dave Raggett.

I'm not going to explain Tidy's "raison d'être" - please read [Dave Raggett's original web page](http://www.w3.org/People/Raggett/tidy/) for more information, or the [SourceForge project](http://tidy.sourceforge.net/) that has taken over maintenance of the library.

## libtidy

This wrapper is written in C#, and makes use of .NET platform invoke (p/invoke) functionality to interoperate with the Tidy library "libtidy" (written in portable ANSI C).

Therefore, you'll also need a build of the binary appropriate for your platform. If you're after a 32 or 64 bit Windows build, or you want a more recent build for Mac OS X than the one that is bundled with the OS, visit the [downloads page](http://github.com/markbeaton/TidyManaged/downloads) at GitHub. Otherwise, grab the latest source from the [SourceForge project](http://tidy.sourceforge.net/), and roll your own.

## Sample Usage

Here's a quick'n'dirty example using a simple console app.  
Note: always remember to .Dispose() of your Document instance (or wrap it in a "using" statement), so the interop layer can clean up any unmanaged resources (memory, file handles etc) when it's done cleaning.
    
    using System;
    using TidyManaged;

    public class Test
    {
      public static void Main(string[] args)
      {
        using (Document doc = Document.FromString("<hTml><title>test</tootle><body>asd</body>"))
        {
          doc.ShowWarnings = false;
          doc.Quiet = true;
          doc.OutputXhtml = true;
          doc.CleanAndRepair();
          string parsed = doc.Save();
          Console.WriteLine(parsed);
        }
      }
    }

results in:

    <!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"
        "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
    <html xmlns="http://www.w3.org/1999/xhtml">
    <head>
    <meta name="generator" content=
    "HTML Tidy for Mac OS X (vers 31 October 2006 - Apple Inc. build 13), see www.w3.org" />
    <title>test</title>
    </head>
    <body>
    asd
    </body>
    </html>

## Notes for non-Windows platforms

Thanks to the platform-agnostic nature of ANSI C, and the excellent work of the people at the [Mono Project](http://www.mono-project.com/), you can use this wrapper library anywhere that Mono is supported, assuming you can have (or can build) a version of the underlying Tidy library for your platform. That shouldn't be too hard - it's a default part of a standard Mac OS X install, for example; it probably is for most Linux distributions as well.

Under Mono, you might need to re-map the p/invoke calls to the appropriate library - or you might find it just works. See [this page on DLL mapping](http://www.mono-project.com/Config_DllMap) for more information on achieving this. Note: the .config file needs to be configured for the TidyManaged DLL, NOT your application's binary.

### Example TidyManaged.dll.config
    <configuration>
      <dllmap dll="libtidy.dll" target="/Users/Mark/Code/Tidy/TestHarness/libtidy.dylib"/>
    </configuration>
    

## The API

At this stage I've just created a basic mapping of each of the configuration options made available by Tidy to properties of the main Document object - I've renamed a few things here & there, but it should be pretty easy to figure out what each property does (the documentation included in the code includes the original Tidy option name for each property). You can read the [Tidy configuration documentation here](http://tidy.sourceforge.net/docs/quickref.html).

## The Future

At some point I'll add a nicer ".NET-style" API layer over the top, as it's a bit clunky (although perfectly usable) at the moment.