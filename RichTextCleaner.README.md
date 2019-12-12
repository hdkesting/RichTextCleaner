Rich Text Cleaner
=================

Purpose
-------

Remove all styling from text that is copied from a webpage, while preserving the HTML element structure. This helps getting clean text into a content management system.

Installation
------------

It is a Windows desktop application.
Unpack the RichTextCleanerSetup.zip file, which is protected by the highly secure password "wk" (lowercase, no quotes).
Run the .msi to install the application. 
*Warning* This application isn't known, so Windows and Symantec consider this a dangerous application. Both have ways to "install anyway". 

Startup
-------

The installer doesn't place an shortcut on the desktop, you'll need to find the application in the start-menu (search for "RichTextCleaner").

Usage
-----

Copy text from a webpage (select the fragment you want and hit Ctrl-C), then use the "Paste" button to import it into this application. You can also use (Ctrl-)V.

Use the "Clear Styling" button (or use (Ctrl-)C) to clean up the styling in the HTML fragment:

* remove all "style" and "class" attributes, 
* remove any "script", "noscript" and "iframe" elements
* change "font" elements into "span"s.
* and change "b" elements into "strong" and "i" into "em".

Optionally you can also remove the "strong" and "em" elements (=replace by "span").

This also copies the cleaned text onto the clipboard, so you can use (Ctrl-)V in a rich-text editor to paste it.

The "Get text only" button (hotkey: (Ctrl-)T) removes all HTML elements, leaving only the plain text (with some attempt at sane formatting). Here also this text is placed on the clipboard.

Source
------

For anyone that is really interested, the sourcecode of this app is on GitHub: https://github.com/hdkesting/RichTextCleaner
