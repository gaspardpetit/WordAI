using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Net; // for WebUtility.HtmlEncode/HtmlDecode
using Microsoft.Office.Interop.Word;
using System.Diagnostics;

namespace WordAI
{
    public static class WordXmlConverter
    {
        /// <summary>
        /// Converts the given Word range to an XML fragment that encodes both the named style and only the formatting properties
        /// that differ from that style's defaults. The output is a string of one or more <style> elements without a wrapping root element.
        /// </summary>
        /// <param name="range">The Range to extract text from.</param>
        /// <returns>A string containing XML fragments with formatting info.</returns>
        public static string ConvertRangeToXmlFragmentOld(Range range)
        {
            if (range == null)
                throw new ArgumentNullException(nameof(range));

            var sb = new StringBuilder();
            int charCount = range.Characters.Count;
            if (charCount == 0)
                return string.Empty;

            // Use 1-based indexing for Word's Characters collection.
            int segmentStartIndex = 1;
            var lastAttrs = GetFormattingAttributes(range.Characters[1]);

            // Iterate over the characters to detect formatting changes.
            for (int i = 2; i <= charCount; i++)
            {
                var currentAttrs = GetFormattingAttributes(range.Characters[i]);
                if (!AttributesEqual(lastAttrs, currentAttrs))
                {
                    // Create a subrange for the current segment.
                    Range segRange = range.Duplicate;
                    segRange.Start = range.Characters[segmentStartIndex].Start;
                    segRange.End = range.Characters[i - 1].End;

                    // Escape HTML reserved characters.
                    string segmentText = WebUtility.HtmlEncode(segRange.Text);
                    sb.Append(BuildXmlTag(lastAttrs, segmentText));

                    // Start a new segment.
                    segmentStartIndex = i;
                    lastAttrs = currentAttrs;
                }
            }
            // Process the final segment.
            Range lastSegment = range.Duplicate;
            lastSegment.Start = range.Characters[segmentStartIndex].Start;
            lastSegment.End = range.Characters[charCount].End;
            string lastText = WebUtility.HtmlEncode(lastSegment.Text);
            sb.Append(BuildXmlTag(lastAttrs, lastText));

            return sb.ToString();
        }
        public static string ConvertRangeToXmlFragmentChar(Range range)
        {
            if (range == null)
                throw new ArgumentNullException(nameof(range));

            var sb = new StringBuilder();
            int charCount = range.Characters.Count;
            if (charCount == 0)
                return string.Empty;

            // Reset profiling timers
            totalAttrTime.Reset();
            styleFetchTime.Reset();
            fontFetchTime.Reset();
            colorFetchTime.Reset();

            Stopwatch totalTime = Stopwatch.StartNew();
            Stopwatch attrTime = new Stopwatch();
            Stopwatch loopTime = new Stopwatch();
            Stopwatch buildXmlTime = new Stopwatch();

            int segmentStartIndex = 1;

            attrTime.Start();
            var lastAttrs = GetFormattingAttributes(range.Characters[1]); // First character attributes
            attrTime.Stop();

            loopTime.Start();
            for (int i = 2; i <= charCount; i++)
            {
                attrTime.Start();
                var currentAttrs = GetFormattingAttributes(range.Characters[i]);
                attrTime.Stop();

                if (!AttributesEqual(lastAttrs, currentAttrs))
                {
                    // Create a subrange for the current segment.
                    Range segRange = range.Duplicate;
                    segRange.Start = range.Characters[segmentStartIndex].Start;
                    segRange.End = range.Characters[i - 1].End;

                    // Escape HTML reserved characters.
                    string segmentText = WebUtility.HtmlEncode(segRange.Text);

                    buildXmlTime.Start();
                    sb.Append(BuildXmlTag(lastAttrs, segmentText));
                    buildXmlTime.Stop();

                    // Start a new segment.
                    segmentStartIndex = i;
                    lastAttrs = currentAttrs;
                }
            }
            loopTime.Stop();

            // Process the final segment.
            Range lastSegment = range.Duplicate;
            lastSegment.Start = range.Characters[segmentStartIndex].Start;
            lastSegment.End = range.Characters[charCount].End;
            string lastText = WebUtility.HtmlEncode(lastSegment.Text);

            buildXmlTime.Start();
            sb.Append(BuildXmlTag(lastAttrs, lastText));
            buildXmlTime.Stop();

            totalTime.Stop();

            // Print profiling results
            Debug.WriteLine($"Total Execution Time: {totalTime.ElapsedMilliseconds} ms");
            Debug.WriteLine($"Time Spent in GetFormattingAttributes: {totalAttrTime.ElapsedMilliseconds} ms");
            Debug.WriteLine($"   - Style Fetch Time: {styleFetchTime.ElapsedMilliseconds} ms");
            Debug.WriteLine($"   - Font Fetch Time: {fontFetchTime.ElapsedMilliseconds} ms");
            Debug.WriteLine($"   - Color Fetch Time: {colorFetchTime.ElapsedMilliseconds} ms");
            Debug.WriteLine($"Time Spent in Character Iteration: {loopTime.ElapsedMilliseconds} ms");
            Debug.WriteLine($"Time Spent in XML Construction: {buildXmlTime.ElapsedMilliseconds} ms");

            return sb.ToString();
        }
        public static string ConvertRangeToXmlFragment(Range range)
        {
            if (range == null)
                throw new ArgumentNullException(nameof(range));

            var sb = new StringBuilder();
            int wordCount = range.Words.Count;
            if (wordCount == 0)
                return string.Empty;

            // Reset profiling timers
            totalAttrTime.Reset();
            styleFetchTime.Reset();
            fontFetchTime.Reset();
            colorFetchTime.Reset();

            Stopwatch totalTime = Stopwatch.StartNew();
            Stopwatch attrTime = new Stopwatch();
            Stopwatch loopTime = new Stopwatch();
            Stopwatch buildXmlTime = new Stopwatch();

            int segmentStartIndex = 1;

            attrTime.Start();
            var lastAttrs = GetFormattingAttributes(range.Words[1]); // First word's attributes
            attrTime.Stop();

            loopTime.Start();
            for (int i = 2; i <= wordCount; i++)
            {
                attrTime.Start();
                var currentAttrs = GetFormattingAttributes(range.Words[i]);
                attrTime.Stop();

                if (!AttributesEqual(lastAttrs, currentAttrs))
                {
                    // Create a subrange for the current segment.
                    Range segRange = range.Duplicate;
                    segRange.Start = range.Words[segmentStartIndex].Start;
                    segRange.End = range.Words[i - 1].End;

                    // Escape HTML reserved characters.
                    string segmentText = WebUtility.HtmlEncode(segRange.Text);

                    buildXmlTime.Start();
                    sb.Append(BuildXmlTag(lastAttrs, segmentText));
                    buildXmlTime.Stop();

                    // Start a new segment.
                    segmentStartIndex = i;
                    lastAttrs = currentAttrs;
                }
            }
            loopTime.Stop();

            // Process the final segment.
            Range lastSegment = range.Duplicate;
            lastSegment.Start = range.Words[segmentStartIndex].Start;
            lastSegment.End = range.Words[wordCount].End;
            string lastText = WebUtility.HtmlEncode(lastSegment.Text);

            buildXmlTime.Start();
            sb.Append(BuildXmlTag(lastAttrs, lastText));
            buildXmlTime.Stop();

            totalTime.Stop();

            // Print profiling results
            Debug.WriteLine($"Total Execution Time: {totalTime.ElapsedMilliseconds} ms");
            Debug.WriteLine($"Time Spent in GetFormattingAttributes: {totalAttrTime.ElapsedMilliseconds} ms");
            Debug.WriteLine($"   - Style Fetch Time: {styleFetchTime.ElapsedMilliseconds} ms");
            Debug.WriteLine($"   - Font Fetch Time: {fontFetchTime.ElapsedMilliseconds} ms");
            Debug.WriteLine($"   - Color Fetch Time: {colorFetchTime.ElapsedMilliseconds} ms");
            Debug.WriteLine($"Time Spent in Word Iteration: {loopTime.ElapsedMilliseconds} ms");
            Debug.WriteLine($"Time Spent in XML Construction: {buildXmlTime.ElapsedMilliseconds} ms");

            return sb.ToString();
        }

        private class Segment
        {
            public string Text { get; set; }
            public string StyleName { get; set; }
            public string FontName { get; set; }
            public string SizeStr { get; set; }
            public string BoldStr { get; set; }
            public string ItalicStr { get; set; }
            public string Underline { get; set; }
            public string Strike { get; set; }
            // New properties for colors.
            public string Color { get; set; }
            public string Background { get; set; }
            public string Foreground { get; set; }
            public string Highlight { get; set; }
        }

        /// <summary>
        /// Inserts XML content into a Word range.
        /// First, it inserts all the text at once, then applies formatting in a second pass.
        /// The XML fragment should be a series of <style> elements with formatting attributes.
        /// </summary>
        /// <param name="xmlFragment">The XML fragment with formatting data.</param>
        /// <param name="targetRange">The Range where the content should be inserted.</param>
        public static void InsertXmlFragmentIntoRange(string xmlFragment, Range targetRange)
        {
            if (targetRange == null)
                throw new ArgumentNullException(nameof(targetRange));
            if (string.IsNullOrEmpty(xmlFragment))
                return;

            // make sure the xml does not contain unknown XML entities (ex. &eacute;) which
            // are valid in HTML but not in XML - XDocument is pretty strict and will throw otherwisw
            string decodedXml = WebUtility.HtmlDecode(xmlFragment);

            // Parse the XML fragment by wrapping it in a temporary root element.
            string wrappedXml = $"<root>{decodedXml}</root>";
            XDocument xdoc = XDocument.Parse(wrappedXml, LoadOptions.PreserveWhitespace);

            // Build a list of segments from the XML.
            // Build a list of segments from the XML by iterating over all child nodes.
            List<Segment> segments = new List<Segment>();
            foreach (var node in xdoc.Root.Nodes())
            {
                if (node is XElement element && element.Name.LocalName == "style")
                {
                    // Process <style> elements with formatting attributes.
                    var seg = new Segment
                    {
                        StyleName = (string)element.Attribute("name") ?? "",
                        FontName = (string)element.Attribute("font") ?? "",
                        SizeStr = (string)element.Attribute("size") ?? "",
                        BoldStr = (string)element.Attribute("bold") ?? "",
                        ItalicStr = (string)element.Attribute("italic") ?? "",
                        Underline = (string)element.Attribute("underline") ?? "",
                        Strike = (string)element.Attribute("strike") ?? "",
                        Color = (string)element.Attribute("color") ?? "",
                        Background = (string)element.Attribute("background") ?? "",
                        Foreground = (string)element.Attribute("foreground") ?? "",
                        Highlight = (string)element.Attribute("highlight") ?? ""
                    };

                    // Decode text from the element.
                    seg.Text = WebUtility.HtmlDecode(element.Value);
                    segments.Add(seg);
                }
                else if (node is XText textNode)
                {
                    // Process plain text nodes.
                    var seg = new Segment
                    {
                        Text = WebUtility.HtmlDecode(textNode.Value)
                    };
                    segments.Add(seg);
                }
                // Optionally, you can handle other node types (like CDATA) if needed.
            }

            // Concatenate all segments' text.
            StringBuilder sbAllText = new StringBuilder();
            foreach (var seg in segments)
            {
                sbAllText.Append(seg.Text);
            }
            string fullText = sbAllText.ToString();

            // First pass: Clear target range and insert all text.
            targetRange.Text = "";
            targetRange.Collapse(WdCollapseDirection.wdCollapseStart);
            int insertStart = targetRange.Start;


            // Apply font formatting only if different.
            targetRange.Font.Reset();
            targetRange.InsertAfter(fullText);

            // Second pass: Apply formatting to each segment.
            int offset = 0;
            foreach (var seg in segments)
            {
                int segmentStart = insertStart + offset;
                int segmentEnd = segmentStart + seg.Text.Length;
                Range segmentRange = targetRange.Document.Range(segmentStart, segmentEnd);

                // Apply the named style only if different.
                if (!string.IsNullOrEmpty(seg.StyleName))
                {
                    try
                    {
                        Style currentStyle = segmentRange.get_Style() as Style;
                        if (currentStyle == null || !currentStyle.NameLocal.Equals(seg.StyleName, StringComparison.OrdinalIgnoreCase))
                        {
                            segmentRange.set_Style(seg.StyleName);
                        }
                    }
                    catch
                    {
                        // Optionally handle missing style errors.
                    }
                }

                // Apply font formatting only if different.
                if (!string.IsNullOrEmpty(seg.FontName) && segmentRange.Font.Name != seg.FontName)
                    segmentRange.Font.Name = seg.FontName;

                if (!string.IsNullOrEmpty(seg.SizeStr) && float.TryParse(seg.SizeStr, out float size) && segmentRange.Font.Size != size)
                    segmentRange.Font.Size = size;

                if (!string.IsNullOrEmpty(seg.BoldStr))
                {
                    int desiredBold = (seg.BoldStr.ToLower() == "true" ? -1 : 0);
                    if (segmentRange.Font.Bold != desiredBold)
                        segmentRange.Font.Bold = desiredBold;
                }

                if (!string.IsNullOrEmpty(seg.ItalicStr))
                {
                    int desiredItalic = (seg.ItalicStr.ToLower() == "true" ? -1 : 0);
                    if (segmentRange.Font.Italic != desiredItalic)
                        segmentRange.Font.Italic = desiredItalic;
                }

                if (!string.IsNullOrEmpty(seg.Underline))
                {
                    // Try to parse the string to a WdUnderline enum value (case-insensitive)
                    if (Enum.TryParse(seg.Underline, true, out WdUnderline parsedUnderline))
                    {
                        if (segmentRange.Font.Underline != parsedUnderline)
                            segmentRange.Font.Underline = parsedUnderline;
                    }
                }

                if (!string.IsNullOrEmpty(seg.Strike))
                {
                    int desiredStrike = (seg.Strike.ToLower() == "true" ? -1 : 0);
                    if (segmentRange.Font.StrikeThrough != desiredStrike)
                        segmentRange.Font.StrikeThrough = desiredStrike;
                }

                // Apply Foreground (Text) Color.
                if (!string.IsNullOrEmpty(seg.Color))
                {
                    WdColor wdColor = RGBStringToWdColor(seg.Color);
                    if (segmentRange.Font.Color != wdColor)
                        segmentRange.Font.Color = wdColor;
                }

                // Apply Background (Shading) Color.
                if (!string.IsNullOrEmpty(seg.Background))
                {
                    WdColor wdBackColor = RGBStringToWdColor(seg.Background);
                    if (segmentRange.Shading.BackgroundPatternColor != wdBackColor)
                        segmentRange.Shading.BackgroundPatternColor = wdBackColor;
                }

                // Apply Background (Shading) Color.
                if (!string.IsNullOrEmpty(seg.Foreground))
                {
                    WdColor wdBackColor = RGBStringToWdColor(seg.Foreground);
                    if (segmentRange.Shading.ForegroundPatternColor != wdBackColor)
                        segmentRange.Shading.ForegroundPatternColor = wdBackColor;
                }

                // Apply Highlight Color.
                if (!string.IsNullOrEmpty(seg.Highlight))
                {
                    if (int.TryParse(seg.Highlight, out int highlightValue))
                    {
                        WdColorIndex wdHighlight = (WdColorIndex)highlightValue;
                        if (segmentRange.HighlightColorIndex != wdHighlight)
                            segmentRange.HighlightColorIndex = wdHighlight;
                    }
                }

                offset += seg.Text.Length;
            }
        }

        #region Helper Methods

        /// <summary>
        /// Converts a WdColor to an RGB string (e.g., "RGB(255,0,0)").
        /// </summary>
        private static string WdColorToRGB(WdColor color)
        {
            int colorValue = (int)color;
            int r = colorValue & 0xFF;
            int g = (colorValue >> 8) & 0xFF;
            int b = (colorValue >> 16) & 0xFF;
            return $"RGB({r},{g},{b})";
        }

        /// <summary>
        /// Converts an RGB string (e.g., "RGB(255,0,0)") back to a WdColor.
        /// If parsing fails, returns wdColorAutomatic.
        /// </summary>
        private static WdColor RGBStringToWdColor(string rgb)
        {
            // Expected format: "RGB(r,g,b)"
            if (string.IsNullOrEmpty(rgb))
                return WdColor.wdColorAutomatic;
            try
            {
                // Remove the "RGB(" prefix and ")" suffix.
                string inner = rgb.Substring(4, rgb.Length - 5);
                string[] parts = inner.Split(',');
                if (parts.Length == 3)
                {
                    int r = int.Parse(parts[0].Trim());
                    int g = int.Parse(parts[1].Trim());
                    int b = int.Parse(parts[2].Trim());
                    int colorValue = r | (g << 8) | (b << 16);
                    return (WdColor)colorValue;
                }
            }
            catch { }
            return WdColor.wdColorAutomatic;
        }

        private static WdColor GetBackgroundColor(Range charRange, Style style)
        {
            try
            {
                return charRange.Shading.BackgroundPatternColor;
            }
            catch
            {
                // If accessing the shading property fails, assume default.
                return WdColor.wdColorAutomatic;
            }
        }

        private static Stopwatch totalAttrTime = new Stopwatch();
        private static Stopwatch styleFetchTime = new Stopwatch();
        private static Stopwatch fontFetchTime = new Stopwatch();
        private static Stopwatch colorFetchTime = new Stopwatch();

        private static Dictionary<string, string> GetFormattingAttributes(Range charRange)
        {
            totalAttrTime.Start();
            var attrs = new Dictionary<string, string>();

            // Fetch style and font in one call
            styleFetchTime.Start();
            Range duplicateRange = charRange.Duplicate;
            Style style = duplicateRange.get_Style() as Style;
            styleFetchTime.Stop();

            fontFetchTime.Start();
            Font font = duplicateRange.Font.Duplicate;
            fontFetchTime.Stop();

            if (style != null)
            {
                attrs["name"] = style.NameLocal;
                Font baseFont = style.Font;

                if (font.Name != baseFont.Name)
                    attrs["font"] = font.Name;
                if (font.Size != baseFont.Size)
                    attrs["size"] = font.Size.ToString();
                if (font.Bold != baseFont.Bold)
                    attrs["bold"] = (font.Bold == -1 ? "true" : "false");
                if (font.Italic != baseFont.Italic)
                    attrs["italic"] = (font.Italic == -1 ? "true" : "false");
                if (font.Underline != baseFont.Underline)
                    attrs["underline"] = font.Underline.ToString();
                if (font.StrikeThrough != baseFont.StrikeThrough)
                    attrs["strike"] = (font.StrikeThrough == -1 ? "true" : "false");

                // Foreground (Text) Color
                if (font.Color != baseFont.Color)
                    attrs["color"] = WdColorToRGB(font.Color);
            }
            else
            {
                // No style? Store all font attributes
                attrs["font"] = font.Name;
                attrs["size"] = font.Size.ToString();
                attrs["bold"] = (font.Bold == -1 ? "true" : "false");
                attrs["italic"] = (font.Italic == -1 ? "true" : "false");
                attrs["underline"] = font.Underline.ToString();
                attrs["strike"] = (font.StrikeThrough == -1 ? "true" : "false");
                attrs["color"] = WdColorToRGB(font.Color);
            }

            // Extract Background and Highlight Color in a single pass
            colorFetchTime.Start();
            Shading shading = duplicateRange.Shading;
            WdColor backgroundColor = shading.BackgroundPatternColor;
            if (backgroundColor != WdColor.wdColorAutomatic)
                attrs["background"] = WdColorToRGB(backgroundColor);

            WdColor foregroundColor = shading.ForegroundPatternColor;
            if (foregroundColor != WdColor.wdColorAutomatic)
                attrs["foreground"] = WdColorToRGB(foregroundColor);
            colorFetchTime.Stop();

            if (charRange.HighlightColorIndex != WdColorIndex.wdNoHighlight)
                attrs["highlight"] = ((int)duplicateRange.HighlightColorIndex).ToString();

            totalAttrTime.Stop();
            return attrs;
        }


        /// <summary>
        /// Extracts a dictionary of formatting attributes from a Word range (typically a single character).
        /// Only attributes that differ from the base style are included.
        /// </summary>
        private static Dictionary<string, string> GetFormattingAttributesOld(Range charRange)
        {
            var attrs = new Dictionary<string, string>();

            // Fetch style and font in one call
            Range duplicateRange = charRange.Duplicate;
            Style style = duplicateRange.get_Style() as Style;
            Font font = duplicateRange.Font.Duplicate;

            if (style != null)
            {
                attrs["name"] = style.NameLocal;
                Font baseFont = style.Font;

                if (font.Name != baseFont.Name)
                    attrs["font"] = font.Name;
                if (font.Size != baseFont.Size)
                    attrs["size"] = font.Size.ToString();
                if (font.Bold != baseFont.Bold)
                    attrs["bold"] = (font.Bold == -1 ? "true" : "false");
                if (font.Italic != baseFont.Italic)
                    attrs["italic"] = (font.Italic == -1 ? "true" : "false");
                if (font.Underline != baseFont.Underline)
                    attrs["underline"] = font.Underline.ToString();
                if (font.StrikeThrough != baseFont.StrikeThrough)
                    attrs["strike"] = (font.StrikeThrough == -1 ? "true" : "false");

                // Foreground (Text) Color
                if (font.Color != baseFont.Color)
                    attrs["color"] = WdColorToRGB(font.Color);
            }
            else
            {
                // No style? Store all font attributes
                attrs["font"] = font.Name;
                attrs["size"] = font.Size.ToString();
                attrs["bold"] = (font.Bold == -1 ? "true" : "false");
                attrs["italic"] = (font.Italic == -1 ? "true" : "false");
                attrs["underline"] = font.Underline.ToString();
                attrs["strike"] = (font.StrikeThrough == -1 ? "true" : "false");
                attrs["color"] = WdColorToRGB(font.Color);
            }

            // Extract Background and Highlight Color in a single pass
            WdColor backgroundColor = charRange.Shading.BackgroundPatternColor;
            if (backgroundColor != WdColor.wdColorAutomatic)
                attrs["background"] = WdColorToRGB(backgroundColor);

            WdColor foregroundColor = charRange.Shading.ForegroundPatternColor;
            if (foregroundColor != WdColor.wdColorAutomatic)
                attrs["foreground"] = WdColorToRGB(foregroundColor);

            if (charRange.HighlightColorIndex != WdColorIndex.wdNoHighlight)
                attrs["highlight"] = ((int)charRange.HighlightColorIndex).ToString();

            return attrs;
        }




        /// <summary>
        /// Compares two dictionaries of formatting attributes.
        /// </summary>
        private static bool AttributesEqual(Dictionary<string, string> a, Dictionary<string, string> b)
        {
            if (a.Count != b.Count)
                return false;
            foreach (var key in a.Keys)
            {
                if (!b.ContainsKey(key) || a[key] != b[key])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Builds an XML string for a segment given its attributes and text.
        /// This version omits any attribute whose value is "false" (since those are the default),
        /// and also omits "underline" if its value is "wdUnderlineNone".
        /// </summary>
        private static string BuildXmlTag(Dictionary<string, string> attrs, string text)
        {
            var element = new XElement("style");
            foreach (var kvp in attrs)
            {
                // Omit attributes with "false" values.
                if (kvp.Value == "false")
                    continue;
                // Omit underline if it is set to the default (wdUnderlineNone).
                if (kvp.Key == "underline" && kvp.Value == "wdUnderlineNone")
                    continue;
                element.SetAttributeValue(kvp.Key, kvp.Value);
            }
            // Replace any form feed with a custom break marker.
            text = text.Replace("\f", "<break/>");
            element.Value = text;
            return element.ToString();
        }

        #endregion
    }
}
