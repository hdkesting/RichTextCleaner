Rich Text Cleaner
=================

A small tool (WPF, both .Net Core 3 and Framework 4.7.2) to help copying text from one website to a rich-text editor for another Content Managed system, 
by stripping all style information but keeping the structure intact.

Usage
-----

Copy text from a webpage (select the fragment you want and hit Ctrl-C), then use the "Paste" button to import it into this application. You can also use (Ctrl-)V.

Use the "Clear Styling" button (or use (Ctrl-)C) to remove all "style" and "class" attributes, remove any "script" elements and change "b"-elements into "strong".
This also copies the cleaned text onto the clipboard, so you can use Ctrl-V in a rich-text editor to paste it.

The "Get text only" button removes all HTML elements, leaving only the plain text (with some attempt at sane formatting). Here also this text is placed on the clipboard.
