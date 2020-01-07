Rich Text Cleaner
=================

Purpose
-------

Remove all styling from text that is copied from a webpage, while preserving the HTML element structure, such as paragraphs, links, lists and tables. This helps getting clean text into a content management system.

Installation
------------

This is a Windows desktop application.  
Unpack the RichTextCleanerSetup.zip file, which is protected by the highly secure password "wk" (lowercase, no quotes).
Run the .msi or the setup.exe to install the application.

*Warning* This application isn't known, so Windows and Symantec consider this a dangerous application. Both have ways to "install anyway" that you'll need to use. 

Do note that this application isn't "auto updating" - you will have to check for new versions yourself. You can install that new version over the old version, no need to uninstall the previous one first.

Startup
-------

The installer doesn't place an shortcut on the desktop, you'll need to find the application in the start-menu (search for "RichTextCleaner").

Usage
-----

Copy text from a webpage (select the fragment you want and hit Ctrl-C), then use the "Paste" button to import it into this application. You can also use (Ctrl-)V to paste. It is also possible to copy/paste HTML source (for instance copied from the HTML tab of a rich-text editor).

Use the "Clear Styling" button (or use (Ctrl-)C) to clean up the styling in the HTML fragment:

* replace all "`&nbsp;`" with a plain space - this does destroy attempts at layout-by-spaces that would need replacing anyway;
* remove all "`<script>`" and "`<noscript>`" elements including contents. iframes (that may contain videos) are left intact;
* remove all "style" and "class" attributes;
* replace "`<b>`" elements with "`<strong>`" and "`<i>`" with "`<em>`";
* remove "`<font>`" and "`<span>`" elements while keeping their contents;
* (when checked) remove all "`<strong>`", "`<em>`" and "`<u>`" elements, keeping their contents;
* replace any "`<span>`" and "`<p>`" elements that contain no text (but may contain whitespace) with a single space;
* remove any "`<a name>`" elements, keeping any contents. If there is also an "href" attribute, only the "name" is removed.

This also copies the cleaned text onto the clipboard, so you can use Ctrl-V in a rich-text editor to paste it, either in the rich-text tab or the html-source tab.

The "Get text only" button (hotkey: (Ctrl-)T) removes all HTML elements, leaving only the plain text (with some attempt at sane formatting). Here also this text is placed on the clipboard.

Source
------

For anyone that is really interested, the sourcecode of this app is on GitHub: https://github.com/hdkesting/RichTextCleaner
