# Rich Text Cleaner

## Purpose

Remove all styling from text that is copied from a webpage, while preserving the HTML element structure, such as paragraphs, links, lists and tables. This helps getting clean text into a content management system.

Please do note that this app cannot distinguish between styling that should be removed and styling that is added on purpose. It will remove all.

## Startup

The installer doesn't place a shortcut on the desktop, you'll need to find the application in the start-menu (search for "RichTextCleaner"). After installing, you may need to wait a few seconds for it to show up.

## Usage

Copy text from a webpage (select the fragment you want and hit Ctrl-C), then use the "Paste" button to import it into this application. You can also use (Ctrl-)V to paste. It is also possible to copy/paste HTML source (for instance copied from the HTML tab of a rich-text editor).

A "Check links" button will open a new window with all links found in the text. These are tried out and the result is noted. See below.

Use the "Clear Styling & Copy" button (or use (Ctrl-)C) to clean up the styling in the HTML fragment:

* Replace all "`&nbsp;`" with a plain space - this does destroy attempts at layout-by-spaces that would need replacing anyway;
* Remove all "`<script>`", "`<noscript>`" and "`<time>`" elements including contents. iframes (that may contain videos) are left intact;
* Remove all "style" and "class" attributes;
* If a paragraph (`<p>`) contains only bold text, then it is converted to an `<h2>` header - this is not perfect but catches a lot;
* Replace "`<b>`" elements with "`<strong>`" and "`<i>`" with "`<em>`";
* Remove "`<font>`" and "`<span>`" elements while keeping their contents;
* (When checked) remove all "`<strong>`", "`<em>`" and "`<u>`" elements, keeping their contents;
* Replace any "`<span>`" and "`<p>`" elements that contain no text (but may contain whitespace) with a single space;
* Remove any "`<a name>`" elements, keeping any contents. If there is also an "href" attribute, only the "name" is removed;
* If a "`<td>`" or "`<li>`" contains just a single "`<p>`", then remove that p-element, keeping its contents;
* Consecutive `<a>` elements with the same "href" are combined into one;
* Leading or trailing spaces, commas and periods are moved out of the linked text;
* Links that contain just whitespace are replaced by that whitespace;
* In linked texts that seem to be a url, a leading "http(s)://" and trailing "/" are removed from the text (the real link is kept);

The cleaned text is immediately copied onto the clipboard, so you can use Ctrl-V in a rich-text editor to paste it, either in the rich-text tab or the html-source tab.

The "Get text only" button (hotkey: (Ctrl-)T) removes all HTML elements, leaving only the plain text (with some attempt at sane formatting). Here also this text is placed on the clipboard.

Through the "Settings" button you can set several checkboxes that influence the cleaning:

* "Remove [Style] Tags" (3x) - when checked, tags like "`<strong>`", "`<em>`" and "`<u>`" are removed, keeping their contents.
* "Add target=_blank" - when checked, this will add an attribute "target" with value "_blank" to all links (that don't already have this attribute), where "_blank" is HTML-speak for Sitecore's "New Window". This assumes that most links need this set anyway.
* "Change to fancy quotes" - this changes any regular quotes to "smart quotes", like Outlook and Word.

The settings are remembered (until a new version of this app is installed).

### Link Checker window

The "Check links" button opens a new window with all links that are found in the current html source. It will check those links.

* A "simple redirect" (change from http to https, addition or removal of a leading "www." and/or a trailing "/") will be automatically checkmarked for "update".
* An error or "not found" will be automatically checkmarked for marking.
* Local URLs (= those not starting with "http(s)://") are ignored. However this works best with text from the html (source) tab.

However, the checker has some issues:

* Some sites don't want to be queried by an app, so they might report an error that a user wouldn't see (Facebook and LinkedIn are ignored specifically because of this - so their URLs are considered to be fine)
* This app only notices the return status. So if the text on the page says "content not found" while the status says "ok" (instead of 404 (not found) as it should be), this app will consider the page to be fine.
* A redirect can be to the new location of the information, or to a generic page (because the specific content isn't available anymore). This app cannot distinguish between those and will report just the redirect. *It is up to you* to make the correct decision. Inspect the new URL or click on it to visit the page.

An "Update links in source" button in that Check Links window will update the source for the items that are checked in the "update" and "marked" columns:

* Items that are checked in the "mark" column will be marked with `[ ]` around the link text, in the source
* Items that are checked in the "update" column will have their URL updated in the source (except when "mark" is also checked)

## Hotkeys

* V - (or Ctrl-V) Paste the rich text or html source into the application
* C - (or Ctrl-C) Clean the HTML and copy it onto the clipboard
* T - Strip all tags, keeping plain text (with some text-only formatting) and copy onto clipboard
* H - Copy the HTML source onto the clipboard as text, without any cleaning
* L - Open the link checker with links found in the current text
* F - Open folder of log files

## Background info

When you copy formatted text from a webpage (such as a plain webpage or the Design tab of a Rich Text Editor), then the browser is in charge of putting HTML on the clipboard. It will add styling to the html-elements you copy that is found in the applicable CSS files.

When HTML source is copied from the HTML tab of a Rich Text Editor, then the browser considers it plain text and doesn't add extra styling. This application still treats this as HTML source and correctly handles this.

Do note that "italics" are sometimes used for emphasis (you probably want to clean this), but sometimes just to mark a title. That title-markup probably should not be cleaned. See also https://developer.mozilla.org/en-US/docs/Web/HTML/Element/i#Usage_notes

## Source

For anyone that is really interested, the full sourcecode of this app is on GitHub: https://github.com/hdkesting/RichTextCleaner
