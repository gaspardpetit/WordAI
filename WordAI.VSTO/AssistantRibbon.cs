using DiffMatchPatch;
using Markdig;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Word;
using Microsoft.Win32;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Office = Microsoft.Office.Core;

namespace WordAI
{
	public interface IParagraphChunkingStrategy
	{
		List<Range> Split(Range fullRange);
	}

	public class ParagraphByParagraphStrategy : IParagraphChunkingStrategy
	{
		public List<Range> Split(Range fullRange)
		{
			return fullRange.Paragraphs
							.Cast<Paragraph>()
							.Select(p => p.Range)
							.ToList();
		}
	}

    public class ListAwareChunkingStrategy : IParagraphChunkingStrategy
    {
		List<Range> ExtractProcessingChunks(Range range)
		{
			List<Range> chunks = new List<Range>();

			var paragraphs = range.Paragraphs;
			int i = 1;

			while (i <= paragraphs.Count)
			{
				Paragraph para = paragraphs[i];
				var listType = para.Range.ListFormat?.ListType;

				if (listType != WdListType.wdListNoNumbering)
				{
					// start of a list block
					int start = para.Range.Start;
					int j = i + 1;

					while (j <= paragraphs.Count &&
							paragraphs[j].Range.ListFormat?.ListType == listType)
					{
						j++;
					}

					int end = paragraphs[j - 1].Range.End;
					chunks.Add(range.Document.Range(start, end));
					i = j;
				}
				else
				{
					// process individually or in small batches
					chunks.Add(para.Range);
					i++;
				}
			}

			return chunks;
		}

		public List<Range> Split(Range fullRange)
        {
            // Your hybrid logic as discussed earlier
            return ExtractProcessingChunks(fullRange);
        }
	}



	public class WholeRangeStrategy : IParagraphChunkingStrategy
	{
		public List<Range> Split(Range fullRange)
		{
			return new List<Range> { fullRange };
		}
	}



	[ComVisible(true)]
    public class AssistantRibbon : Office.IRibbonExtensibility
    {
        private const string RegistryPath = @"Software\AssistantWordAddin"; // Custom registry key

        private const string IMPROVE_PROMPT = @"
Review the following text for spelling errors and provide accurate corrections.
Ensure that all words are spelled correctly, and make necessary adjustments to enhance the overall spelling accuracy of the text.
The CONTEXT is provided so that you can ensure consistency of style across the whole document, but you should y review and correct the part provided.
When no spelling error is detected, return the original text.
You only provide the corrected text. You do not provide any additional comment.

### CONTEXT
                            
";

        private Office.IRibbonUI ribbon;
        public static string _selectedPromptId = string.Empty;

        public AssistantRibbon()
        {
            if (_selectedPromptId == string.Empty)
                _selectedPromptId = GetAssistantId();
        }

		public static async System.Threading.Tasks.Task ProcessWithStrategy(Range range, IParagraphChunkingStrategy splitter, Func<Range, Action<string, float>, System.Threading.Tasks.Task> processParagraph)
        {
			if (range == null || splitter == null || processParagraph == null)
				return;

			var chunks = splitter.Split(range);
			int total = chunks.Count;

			ProgressForm progressForm = new ProgressForm();
			progressForm.Show();

			try
			{
				for (int i = 0; i < total; i++)
				{
					Range chunk = chunks[i];
					await processParagraph(chunk, (status, progress) =>
					{
						progressForm.SetProgress((int)Math.Round(i * 100 + progress * 100), total * 100);
						progressForm.SetStatus(status);
					});

					if (progressForm.isAborted)
						break;
				}
			}
			finally
			{
				progressForm.Close();
				Globals.ThisAddIn.Application.StatusBar = string.Empty;
			}
		}



		public static List<Range> FindFormattingRuns(Range range)
        {
            List<Range> ranges = new List<Range>();
            if (range == null || string.IsNullOrWhiteSpace(range.Text))
                return ranges;

            Style lastStyle = null;
            Font lastFont = null;
            int segmentStart = 1;  // Word Interop uses 1-based indexing

            for (int i = 1; i <= range.Characters.Count; i++)
            {
                Range charRange = range.Characters[i];
                Style currentStyle = charRange.get_Style() as Style;
                Font currentFont = charRange.Font;

                if (lastStyle == null)
                {
                    lastStyle = currentStyle;
                    lastFont = currentFont;
                    continue;
                }

                // If formatting or style changes, process the previous segment
                if (!AreStylesEqual(lastStyle, currentStyle) || !AreFontsEqual(lastFont, currentFont))
                {
                    Range styleSegment = range.Duplicate;
                    styleSegment.Start = range.Characters[segmentStart].Start;
                    styleSegment.End = range.Characters[i - 1].End;

                    Console.WriteLine($"Style: {lastStyle.NameLocal} | Text: \"{styleSegment.Text.Trim()}\" | Formatting: {FontDescription(lastFont)}");
                    ranges.Add(styleSegment);

                    // Start a new segment
                    segmentStart = i;
                    lastStyle = currentStyle;
                    lastFont = currentFont;
                }
            }

            // Capture the last segment
            if (segmentStart <= range.Characters.Count)
            {
                Range lastSegment = range.Duplicate;
                lastSegment.Start = range.Characters[segmentStart].Start;
                lastSegment.End = range.Characters[range.Characters.Count].End;

                Console.WriteLine($"Style: {lastStyle.NameLocal} | Text: \"{lastSegment.Text.Trim()}\" | Formatting: {FontDescription(lastFont)}");
                ranges.Add(lastSegment);
            }
            return ranges;
        }

        // Compare styles
        public static bool AreStylesEqual(Style style1, Style style2)
        {
            if (style1 == null || style2 == null)
                return false;
            return style1.NameLocal == style2.NameLocal;
        }

        // Compare fonts (checks all relevant formatting properties)
        public static bool AreFontsEqual(Font font1, Font font2)
        {
            if (font1 == null || font2 == null)
                return false;

            return font1.Bold == font2.Bold &&
                   font1.Italic == font2.Italic &&
                   font1.Underline == font2.Underline &&
                   font1.StrikeThrough == font2.StrikeThrough &&
                   font1.DoubleStrikeThrough == font2.DoubleStrikeThrough &&
                   font1.Color == font2.Color &&
                   font1.Size == font2.Size &&
                   font1.Name == font2.Name &&
                   font1.Superscript == font2.Superscript &&
                   font1.Subscript == font2.Subscript &&
                   font1.Shadow == font2.Shadow &&
                   font1.Outline == font2.Outline &&
                   font1.Emboss == font2.Emboss &&
                   font1.Engrave == font2.Engrave;
        }

        // Helper function to describe font formatting in text
        public static string FontDescription(Font font)
        {
            return $"Bold: {(font.Bold == -1 ? "Yes" : "No")}, " +
                   $"Italic: {(font.Italic == -1 ? "Yes" : "No")}, " +
                   $"Underline: {font.Underline}, " +
                   $"Strikethrough: {(font.StrikeThrough == -1 ? "Yes" : "No")}, " +
                   $"Double Strikethrough: {(font.DoubleStrikeThrough == -1 ? "Yes" : "No")}, " +
                   $"Color: {font.Color}, " +
                   $"Size: {font.Size}, " +
                   $"Font Name: {font.Name}, " +
                   $"Superscript: {(font.Superscript == -1 ? "Yes" : "No")}, " +
                   $"Subscript: {(font.Subscript == -1 ? "Yes" : "No")}, " +
                   $"Shadow: {(font.Shadow == -1 ? "Yes" : "No")}, " +
                   $"Outline: {(font.Outline == -1 ? "Yes" : "No")}, " +
                   $"Emboss: {(font.Emboss == -1 ? "Yes" : "No")}, " +
                   $"Engrave: {(font.Engrave == -1 ? "Yes" : "No")}";
        }


		public static HeaderFooter GetCurrentHeaderFooter(Range range)
		{
			Section section = range.Sections[1];

			switch(range.StoryType)
            {
                case WdStoryType.wdPrimaryHeaderStory:
                    return section.Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary];
                case WdStoryType.wdFirstPageHeaderStory:
                    return section.Headers[WdHeaderFooterIndex.wdHeaderFooterFirstPage];
                case WdStoryType.wdEvenPagesHeaderStory:
                    return section.Headers[WdHeaderFooterIndex.wdHeaderFooterEvenPages];
                case WdStoryType.wdPrimaryFooterStory:
                    return section.Footers[WdHeaderFooterIndex.wdHeaderFooterPrimary];
                case WdStoryType.wdFirstPageFooterStory:
                    return section.Footers[WdHeaderFooterIndex.wdHeaderFooterFirstPage];
                case WdStoryType.wdEvenPagesFooterStory:
                    return section.Footers[WdHeaderFooterIndex.wdHeaderFooterEvenPages];
                default:
                    return null;
			}
		}


		/// <summary>
		/// Processes each paragraph in the given range in beginning-to-end order.
		/// For each paragraph, it recalculates the paragraph’s start position by
		/// working back from the current document end, based on an initial snapshot.
		/// This helps accommodate changes at the top of the document.
		/// </summary>
		/// <param name="range">The range containing paragraphs to process.</param>
		/// <param name="processParagraph">The lambda to call for each paragraph’s range.</param>
		public static async System.Threading.Tasks.Task ProcessParagraphsWithDynamicRecalc(Range range,
	        IParagraphChunkingStrategy chunkingStrategy,
	        Func<Range, Action<string, float>, System.Threading.Tasks.Task> processParagraph
        )
        {
            if (range == null || range.Paragraphs.Count == 0 || processParagraph == null)
                return;

            // Create and show the progress form.
            ProgressForm progressForm = new ProgressForm();
            progressForm.Show();

            try
            {
				bool isInHeader = range.StoryType == WdStoryType.wdPrimaryHeaderStory ||
								  range.StoryType == WdStoryType.wdFirstPageHeaderStory ||
								  range.StoryType == WdStoryType.wdEvenPagesHeaderStory;

				bool isInFooter = range.StoryType == WdStoryType.wdPrimaryFooterStory ||
								  range.StoryType == WdStoryType.wdFirstPageFooterStory ||
								  range.StoryType == WdStoryType.wdEvenPagesFooterStory;

                bool isInDocument = range.StoryType == WdStoryType.wdMainTextStory;

                bool isInFootnotes = range.StoryType == WdStoryType.wdFootnotesStory;
                bool isInEndnotes = range.StoryType == WdStoryType.wdEndnotesStory;
                bool isInComments = range.StoryType == WdStoryType.wdCommentsStory;

                bool isInTextFrame = range.StoryType == WdStoryType.wdTextFrameStory;

                bool isInFootnoteSeparator = range.StoryType == WdStoryType.wdFootnoteSeparatorStory;
				bool isFootnoteContinuationSeparator = range.StoryType == WdStoryType.wdFootnoteContinuationSeparatorStory;
				bool isEndnoteSeparatorStory = range.StoryType == WdStoryType.wdEndnoteSeparatorStory;
				bool isEndnoteContinuationSeparator = range.StoryType == WdStoryType.wdEndnoteContinuationSeparatorStory;
				bool isEndnoteContinuationNotice = range.StoryType == WdStoryType.wdEndnoteContinuationNoticeStory;

				HeaderFooter section = GetCurrentHeaderFooter(range);
                if (section != null)
                {
					// Create a fresh range for the paragraph.
					Range paraRange = range;

                    int i = 0;
                    int count = 1;
					// Call the lambda.
					await processParagraph(paraRange, (string status, float progress) => {
						progressForm.SetProgress((int)Math.Round(i * 100 + progress * 100), count * 100);
						progressForm.SetStatus(status);
					});
				}
				else
                {
                    Document doc = range.Document;
                    Paragraphs paragraphs = range.Paragraphs;

                    int totalParagraphs = range.Paragraphs.Count;

                    // Capture an initial snapshot:
                    // For each paragraph, record its length (excluding trailing paragraph mark)
                    // and its starting position relative to the initial document end.
                    int initialDocEnd = doc.Content.End;

					List<Range> initialChunks = chunkingStrategy.Split(range);

					List<(int relativeOffset, int length)> stableChunks = initialChunks.Select(chunk =>
					{
						int chunkStart = chunk.Start;
						int chunkEnd = chunk.End;
						if (chunkEnd > chunkStart)
							chunkEnd--; // Strip paragraph mark
						int length = chunkEnd - chunkStart;
						int relativeOffset = initialDocEnd - chunkStart;
						return (relativeOffset, length);
					}).ToList();

                    // Iterate in natural (beginning-to-end) order
                    for (int i = 0; i < stableChunks.Count; i++)
                    {
                        var (relativeOffset, length) = stableChunks[i];
                        // Get the current document end.
                        int currentDocEnd = doc.Content.End;
                        // Recalculate the paragraph's start as the currentDocEnd minus the relative offset.
                        int newParaStart = currentDocEnd - relativeOffset;
                        int newParaEnd = newParaStart + length;

                        // Clamp the new end if it exceeds the document content.
                        if (newParaEnd > doc.Content.End)
                            newParaEnd = doc.Content.End;

                        if (i == 0)
                        {
                            if (newParaStart < range.Start)
                            {
                                // first paragraph may be partially selected
                                newParaStart = range.Start;
                            }
                        }

                        if (i == stableChunks.Count - 1)
                        {
                            if (newParaEnd > range.End)
                            {
                                // last paragraph may be partially selected
                                newParaEnd = range.End;
                            }
                        }

                        // Create a fresh range for the paragraph.
                        Range paraRange = doc.Range(newParaStart, newParaEnd);

                        // Call the lambda.
                        await processParagraph(paraRange, (string status, float progress) => {
                            progressForm.SetProgress((int)Math.Round(i * 100 + progress * 100), totalParagraphs * 100);
                            progressForm.SetStatus(status);
                        });
                        // Update progress.
                        progressForm.SetProgress(i + 1, totalParagraphs);
                        if (progressForm.isAborted)
                            break;
                        Globals.ThisAddIn.Application.StatusBar = $"Processing paragraph {i + 1} of {totalParagraphs}";
                    }
                }
                progressForm.Close();
                Globals.ThisAddIn.Application.StatusBar = string.Empty;
            }
            catch(Exception)
            {
                progressForm.Close();
                throw;
            }

        }


        /// <summary>
        /// Returns the original text from a range, ignoring insertions and including deleted text.
        /// </summary>
        public static string GetOriginalText(Range range)
        {
            if (range == null)
                return string.Empty;

            StringBuilder originalText = new StringBuilder();

            foreach (Revision rev in range.Revisions)
            {
                switch (rev.Type)
                {
                    case WdRevisionType.wdRevisionInsert:
                        // Ignore inserted text (not part of the original)
                        break;

                    case WdRevisionType.wdRevisionDelete:
                        // Append deleted text (part of the original)
                        originalText.Append(rev.Range.Text);
                        break;

                    default:
                        // Other revision types (formatting, etc.) are ignored
                        break;
                }
            }

            // If no revisions exist, return the current text
            string text = originalText.Length > 0 ? originalText.ToString() : range.Text;
            if (text == null)
                return string.Empty;
            return text;
        }

		/// <summary>
		/// Returns the latest version of the text from a range, ignoring deletions and including insertions.
		/// </summary>
		public static string GetText(Range range, bool asXml)
		{
			if (range == null)
				return string.Empty;

			StringBuilder newText = new StringBuilder();
			bool hasInsertions = false;

			foreach (Revision rev in range.Revisions)
			{
				if (rev.Type == WdRevisionType.wdRevisionInsert)
				{
					hasInsertions = true;

					foreach (Paragraph para in rev.Range.Paragraphs)
					{
						string listPrefix = "";
						if (para.Range.ListFormat != null && para.Range.ListFormat.List != null)
						{
							listPrefix = para.Range.ListFormat.ListString + " ";
						}

						string paragraphText;
						if (asXml)
						{
							paragraphText = WordXmlConverter.ConvertRangeToXmlFragment(para.Range);
						}
						else
						{
							paragraphText = para.Range.Text;
						}

						newText.Append(listPrefix + paragraphText);
					}
				}
			}

			if (hasInsertions)
				return WordXmlConverter.SanitizeWordText(newText.ToString());

			// No insertions – fallback to range itself
			StringBuilder fallbackText = new StringBuilder();
			foreach (Paragraph para in range.Paragraphs)
			{
				string listPrefix = "";
				if (para.Range.ListFormat != null && para.Range.ListFormat.List != null)
				{
					listPrefix = para.Range.ListFormat.ListString + " ";
				}

				string paragraphText;
				if (asXml)
				{
					paragraphText = WordXmlConverter.ConvertRangeToXmlFragment(para.Range);
				}
				else
				{
					paragraphText = para.Range.Text;
				}

				fallbackText.Append(listPrefix + paragraphText);
			}

			return WordXmlConverter.SanitizeWordText(fallbackText.ToString());
		}


		public static Range ApplyTrackedChanges(List<Diff> diffs, Range selection, bool trackedChanges)
        {
			// Duplicate the current selection range.
			Range rng = selection.Duplicate;
            int basePos = rng.Start;

			var chars = rng.NewRange(1, 13).Characters;
            string act = string.Empty;
			for (int i = 1; i <= chars.Count; i++)
			{
				char c = chars[i].Text[0]; // Note: COM collections are 1-based
                act += c;
			}


			// Use the provided option to determine deletion behavior.
			// If trackedChanges is true, deletions are simulated (text remains but is marked deleted)
			// and we advance the offset. Otherwise, deletions actually remove text and we leave the offset.
			bool simulateTrackedChanges = trackedChanges;

            // The offset tracks our position in the original text.
            int offset = 0;

            foreach (var diff in diffs)
            {
                Debug.WriteLine(selection.Text);
                if (diff.operation == Operation.EQUAL)
                {
                    // For equal text, simply advance the offset.
                    offset += diff.text.Length;
                }
                else if (diff.operation == Operation.DELETE)
                {
                    try
                    {
                        // Create a range covering the text to delete.
                        Range delRange = selection.NewRange(basePos + offset, basePos + offset + diff.text.Length);
                        // don't use .Delete as it triggers auto formatting rules, such as fusion of spaces
                        delRange.Text = "";

                        // When simulated tracked changes is enabled, deletion doesn't remove text,
                        // so we advance the offset as if it did.
                        if (simulateTrackedChanges)
                        {
                            offset += diff.text.Length;
                        }

                        // Otherwise, when tracked changes is off, deletion actually removes text,
                        // so the underlying text shrinks and we do not modify the offset.
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error applying deletion diff: " + ex.Message);
                    }
                }
                else if (diff.operation == Operation.INSERT)
                {
                    try
                    {
                        // Create a range at the current offset.
                        Range insRange = selection.NewRange(basePos + offset, basePos + offset);
                        insRange.InsertAfter(diff.text);
                        // Advance the offset by the inserted text's length.
                        offset += diff.text.Length;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error applying insertion diff: " + ex.Message);
                    }
                }
            }
            return selection.Document.Range(basePos, basePos + offset);
        }

        public static Range ApplyTrackedChanges(string originalText, string modifiedText, Range selection, bool trackedChanges)
        {
            // normalize line breaks

            string normalizedOriginalText = originalText
                .Replace("\r\n", "\r")
                .Replace("\n", "\r");

            string normalizedModifiedText = modifiedText
                .Replace("\r\n", "\r")
                .Replace("\n", "\r");

            var dmp = new diff_match_patch();
            List<Diff> diffs = dmp.diff_main(normalizedOriginalText, normalizedModifiedText);
            dmp.diff_cleanupSemantic(diffs); // Optimize for better readability
            return ApplyTrackedChanges(diffs, selection, trackedChanges);
        }

        public static AsyncCollectionResult<StreamingChatCompletionUpdate> GetResponseAsync(Uri apiEndpoint, string apiToken, string model, string selectionText, string documentText, string prompt)
        {
            /*if (string.IsNullOrEmpty(modelSettings.ApiToken))
            {
                MessageBox.Show("API Key is missing. Please configure it in Settings.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "Error: No API Key";
            }
            */

            ChatMessage[] message = new ChatMessage[] {
                    // System messages represent instructions or other guidance about how the assistant should behave
                    new SystemChatMessage(prompt + documentText),
                    // User messages represent user input, whether historical or the most recen tinput
                    new UserChatMessage(selectionText),
            };

            OpenAIClientOptions options = new OpenAIClientOptions() { Endpoint = apiEndpoint };
            ApiKeyCredential credential = new ApiKeyCredential(apiToken);
            ChatClient client = new ChatClient(model: model, credential: credential, options: options);
            AsyncCollectionResult<StreamingChatCompletionUpdate> completion = client.CompleteChatStreamingAsync(message);
            return completion;
        }

        public static Range TrimSelection(Range selectionRange)
        {
            // Adjust the selection range to remove leading and trailing whitespace and control characters.
            string fullText = selectionRange.Text;
            if (string.IsNullOrEmpty(fullText))
                return selectionRange;

            // skip leading control characters
            while(selectionRange.End - selectionRange.Start > 0 && selectionRange.NewRange(selectionRange.Start, selectionRange.Start + 1).Text == null)
			{
                selectionRange.SetRange(selectionRange.Start + 1, selectionRange.End);
			}

            int len = fullText.Length;
            int leading = 0;
            while (leading < len && (char.IsWhiteSpace(fullText[leading]) || char.IsControl(fullText[leading])))
            {
                leading++;
            }

            int trailing = 0;
            while (trailing < len && (char.IsWhiteSpace(fullText[len - 1 - trailing]) || char.IsControl(fullText[len - 1 - trailing])))
            {
                trailing++;
            }

            int newStart = selectionRange.Start + leading;
            int newEnd = selectionRange.End - trailing;
            if (newStart > newEnd)
            {
                // If the entire text is trimmed, set an empty range.
                newStart = newEnd;
            }
            selectionRange.SetRange(newStart, newEnd);
            return selectionRange;
        }



        public string GetDynamicMenuContent(Office.IRibbonControl control)
        {
            List<PromptEntry> customEntries = new PromptManager().Prompts;
            StringBuilder sb = new StringBuilder();

            sb.Append("<menu xmlns='http://schemas.microsoft.com/office/2009/07/customui'>");
            foreach (var entry in customEntries)
            {
                // Prepend a letter to ensure the ID is valid.
                string validId = "p" + entry.Id;
                sb.AppendFormat(
                    "<button id='{0}' label='{1}' imageMso='PictureBrightnessGallery' onAction='OnCustomEntryClick'/>",
                    validId, entry.Label);
            }
            sb.Append("</menu>");

            return sb.ToString();
        }

        public void SetAssistant(string id, string label)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                key.SetValue("CurrentAssistantId", id);
                key.SetValue("CurrentAssistantLabel", label);
                key.SetValue("CurrentAssistant", label); // legacy
            }
        }

        public string GetAssistantId()
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                return key.GetValue("CurrentAssistantId", "").ToString();
            }
        }

        public string GetAssistantLabel()
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                string label = key.GetValue("CurrentAssistantLabel", "").ToString();
                if (string.IsNullOrEmpty(label))
                {
                    label = key.GetValue("CurrentAssistant", "").ToString();
                    if (string.IsNullOrEmpty(label))
                    {
                        string id = key.GetValue("CurrentAssistantId", "").ToString();
                        if (!string.IsNullOrEmpty(id))
                        {
                            var prompt = new PromptManager().Get(id);
                            label = prompt?.Label ?? string.Empty;
                        }
                    }
                }
                return label;
            }
        }


        public string GetDynamicMenuLabel(Office.IRibbonControl control)
        {
            return GetAssistantLabel();
        }


        public void InsertMarkdown(string markdownText)
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var document = Markdig.Markdown.Parse(markdownText, pipeline);

            // Insert into Word with proper styling
            //InsertMarkdownToWord(document);
        }

        public void OnCustomEntryClick(Office.IRibbonControl control)
        {
            // Remove the prefix 'p' to match the stored GUID.
            string guid = control.Id.Substring(1);
            _selectedPromptId = guid;
            PromptEntry prompt = new PromptManager().Get(_selectedPromptId);

            // Update the stored assistant id and label.
            SetAssistant(prompt.Id, prompt.Label);

            // Force the Ribbon to refresh the dynamic menu's label.
            ribbon.InvalidateControl("DynamicMenu");
        }

        public void OnPinnedAssistantClick(Office.IRibbonControl control)
        {
            // Remove the prefix 'pin' to match the stored GUID
            string guid = control.Id.Substring(3);
            _selectedPromptId = guid;
            PromptEntry prompt = new PromptManager().Get(_selectedPromptId);
            SetAssistant(prompt.Id, prompt.Label);
            ribbon.InvalidateControl("DynamicMenu");
            OnExecuteButtonClick(control);
        }

        public async void OnCorrectButtonClick(IRibbonControl control)
        {
            try
            {
                bool trackedChanges = GetTrackedChangesSettings();

                var app = Globals.ThisAddIn.Application;
                Selection wholeSelection = app.Selection;
                var doc = app.ActiveDocument;

				await ProcessParagraphsWithDynamicRecalc(
                    wholeSelection.Range,
					new ParagraphByParagraphStrategy(),
                    async (selection, progress) =>
                {
                    Range trimedSelectionRange = TrimSelection(selection);
                    StringBuilder aiResponseBuilder = new StringBuilder();

                    if (selection != null && selection.Text != null && selection.Text.Trim().Length > 0)
                    {
                        // Call OpenAI asynchronously
                        var modelSettings = ModelManager.FromSettings();
                        string model = modelSettings.DefaultModel;

						AsyncCollectionResult<StreamingChatCompletionUpdate> completionUpdates = GetResponseAsync(new Uri(modelSettings.Endpoint), modelSettings.ApiToken, model, trimedSelectionRange.Text, doc.Content.Text, IMPROVE_PROMPT);
                        IAsyncEnumerator<StreamingChatCompletionUpdate> enumerator = completionUpdates.GetAsyncEnumerator();
                        while (await enumerator.MoveNextAsync())
                        {
                            StreamingChatCompletionUpdate completionUpdate = enumerator.Current;
                            aiResponseBuilder.Append(completionUpdate.ContentUpdate[0].Text);
                        }

                        string aiResponse = aiResponseBuilder.ToString().Trim();

                        bool prevTrackRevisionsState = doc.TrackRevisions;
                        if (trackedChanges)
                            doc.TrackRevisions = true;

                        ApplyTrackedChanges(trimedSelectionRange.Text, aiResponse, trimedSelectionRange, trackedChanges);

                        if (trackedChanges)
                            doc.TrackRevisions = prevTrackRevisionsState;
					}
				}
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Assistant Error");
            }
        }

        public void OnSettingsButtonClick(IRibbonControl control)
        {
            var settingsForm = new SettingsForm();
            settingsForm.ShowDialog();
        }

        public void OnManageButtonClick(IRibbonControl control)
        {
            var settingsForm = new ManageForm();
            settingsForm.ShowDialog();

            // Refresh the dynamic menu to include any changes made
            ribbon?.InvalidateControl("DynamicMenu");

            // Ensure the selected assistant still exists after edits
            string currentLabel = GetAssistantLabel();
            if (!string.IsNullOrEmpty(currentLabel))
            {
                var pm = new PromptManager();
                if (!pm.Prompts.Exists(p => p.Label.Equals(currentLabel, StringComparison.OrdinalIgnoreCase)))
                {
                    // Clear the selection if the assistant was removed or renamed
                    SetAssistant(string.Empty, string.Empty);
                    _selectedPromptId = string.Empty;
                }
            }
        }

        public void OnTypeDiffButtonClick(IRibbonControl control)
        {
            // Retrieve text from the clipboard.
            string clipboardText = Clipboard.GetText();

            // Optionally, you may check if the clipboard text is not empty.
            if (string.IsNullOrEmpty(clipboardText))
            {
                // Optionally, notify the user or simply return.
                return;
            }

            var app = Globals.ThisAddIn.Application;
            Selection selection = app.Selection;

            ApplyTrackedChanges(selection.Text, clipboardText, selection.Range, app.ActiveDocument.TrackRevisions);
        }

        public void OnTrackChangeClick(Office.IRibbonControl control, bool pressed)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                key.SetValue("TrackedChanges", pressed);
            }

        }

        public void OnPreserveStyleClick(Office.IRibbonControl control, bool pressed)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                key.SetValue("PreserveStyle", pressed);
            }
        }


        public bool GetCheckBoxTrackChangesState(Office.IRibbonControl control)
        {
            return GetTrackedChangesSettings();
        }

        public bool GetCheckBoxPreserveStyleState(Office.IRibbonControl control)
        {
            return GetPreserveStyleSettings();
        }

        public void OnThoughtsAsCommentsClick(Office.IRibbonControl control, bool pressed)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                key.SetValue("ThoughtsAsComments", pressed);
            }
        }


        public static bool GetThoughtsAsCommentsSettings()
        {
            bool checkBoxTrackChanges = false;
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
            {
                if (key != null)
                {
                    checkBoxTrackChanges = bool.Parse(key.GetValue("ThoughtsAsComments", false).ToString());
                }
            }
            return checkBoxTrackChanges;
        }
        public bool GetCheckboxThoughtsAsCommentsState(Office.IRibbonControl control)
        {
            return GetThoughtsAsCommentsSettings();
        }

        public static bool GetPreserveStyleSettings()
        {
            bool checkBoxTrackChanges = false;
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
            {
                if (key != null)
                {
                    checkBoxTrackChanges = bool.Parse(key.GetValue("PreserveStyle", false).ToString());
                }
            }
            return checkBoxTrackChanges;
        }

        public static bool GetTrackedChangesSettings()
        {
            bool checkBoxTrackChanges = false;
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
            {
                if (key != null)
                {
                    checkBoxTrackChanges = bool.Parse(key.GetValue("TrackedChanges", false).ToString());
                }
            }
            return checkBoxTrackChanges;
        }

        public async void OnExecuteButtonClick(Office.IRibbonControl control)
        {
            bool preserveStyle = GetPreserveStyleSettings();

            string promptIntro = "You are a text transformation assistant. Respond only with the transformed text, omitting any introductions or conclusions";

			string styleInstruction = string.Empty;
            if (preserveStyle)
            {
                styleInstruction = @"
#### XML Formatting
When the text contain XML formatting, you preserve the XML formatting in your response.

";
            }
            else
            {
				styleInstruction = @"
#### Formatting
You preserve the formatting of the original text, if any.

";

			}

			try
            {
                // Ensure that a prompt has been selected.
                PromptEntry prompt = new PromptManager().Get(_selectedPromptId);

                if (prompt == null)
                {
                    MessageBox.Show("Please select a custom prompt from the dynamic menu before executing.",
                                    "No Prompt Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool trackedChanges = GetTrackedChangesSettings();

                var app = Globals.ThisAddIn.Application;
                Selection wholeSelection = app.Selection;

                prompt.Mode = ChunkingMode.Paragraph;

				IParagraphChunkingStrategy strategy = null;
				switch (prompt.Mode)
                {
					case ChunkingMode.WholeBlock:
						strategy = new WholeRangeStrategy();
                        break;
                    case ChunkingMode.Paragraph:
                        strategy = new ParagraphByParagraphStrategy();
                        break;
				    case ChunkingMode.ListAware:
				    default:
						strategy = new ListAwareChunkingStrategy();
						break;
				}

				await ProcessParagraphsWithDynamicRecalc(wholeSelection.Range, strategy, async (selection, progress) =>
                {
                    progress("formatting question", 0.0f);


                    if (selection != null && selection.Text != null && selection.Text.Trim().Length > 0)
                    {
						string thoughts = string.Empty;

						// Trim the selection to remove any leading/trailing whitespace.
						Range trimmedSelectionRange = TrimSelection(selection);

						// Use the prompt text from the selected prompt.
						string promptText = string.Empty;

                        string prefix = string.Empty;
                        string suffix = string.Empty;
                        string wholeText = string.Empty;

						HeaderFooter section = GetCurrentHeaderFooter(selection);
                        Range wholeRange = null;
                        if (section != null)
                        {
                            wholeRange = section.Range;
                        }
                        else
                        {
                            wholeRange = selection.Document.Content;
                        }

						// Get a range of text from the beginning of the document to the selection start.
						Range prefixRange = wholeRange.NewRange(0, selection.Start);
						Range suffixRange = wholeRange.NewRange(selection.End, wholeRange.End);

						// ideally we would provide the context with formatting, but in reality, COM interop is too slow
						// and fetching style for an entire document can take minutes if not hours on long documents.
						bool contextWithStyle = /*preserveStyle*/false;
						prefix = GetText(prefixRange, contextWithStyle).Trim();
						suffix = GetText(suffixRange, contextWithStyle).Trim();
						wholeText = GetText(wholeRange, contextWithStyle).Trim();
						prefix = prefix.Replace("\f", "<break/>");
						suffix = suffix.Replace("\f", "<break/>");
						prefix = prefix.Replace("\u000B", "<vt/>");
						suffix = suffix.Replace("\u000B", "<vt/>");

						if (prompt.Context == ContextType.none.ToString() || (string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(suffix)))
						{
                            prefix = string.Empty;
							suffix = string.Empty;
							wholeText = string.Empty;
						}
						else if (prompt.Context == ContextType.prefix.ToString() || string.IsNullOrEmpty(suffix))
                        {
							suffix = string.Empty;
							wholeText = string.Empty;
						}
						else if (prompt.Context == ContextType.suffix.ToString() || string.IsNullOrEmpty(prefix))
                        {
							prefix = string.Empty;
							wholeText = string.Empty;
						}
                        else
                        {
							prefix = string.Empty;
							suffix = string.Empty;
						}


						if (string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(suffix) && string.IsNullOrEmpty(wholeText))
                        {
                            promptText = $@"
{promptIntro}
{prompt.Prompt}
{styleInstruction}

";
                        }
                        else if (!string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(suffix) && string.IsNullOrEmpty(wholeText))
                        {
                            promptText = $@"
{promptIntro}
{prompt.Prompt}
{styleInstruction}

### Context

The document so far contains the following text - your response will be appended directly after:

{prefix}                            

";
                        }
                        else if (string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(suffix) && string.IsNullOrEmpty(wholeText))
                        {
                            promptText = $@"
{promptIntro}
{prompt.Prompt}
{styleInstruction}

### Context
Your response will be inserted immediately before the following text (do not repeat it):

{suffix}                            

";
                        }
                        else // if (prompt.Context == ContextType.suffix.ToString() || string.IsNullOrEmpty(prefix))
                        {
                            promptText = $@"

{promptIntro}
{prompt.Prompt}
{styleInstruction}

### Context

The text you are editing is part of the following document:

{wholeText}

";
                        }

                        // Call OpenAI asynchronously using the selected prompt.
                        string xmlTrimmedSelectionRange = string.Empty;
                        if (preserveStyle)
                            xmlTrimmedSelectionRange = WordXmlConverter.ConvertRangeToXmlFragment(trimmedSelectionRange);
                        else
                            xmlTrimmedSelectionRange = trimmedSelectionRange.Text;

                        // Call OpenAI asynchronously
                        StringBuilder aiResponseBuilder = new StringBuilder();
                        var modelSettings = ModelManager.FromSettings();
                        string model = modelSettings.DefaultModel;
                        if (string.IsNullOrEmpty(prompt.Model) == false)
                        {
                            model = prompt.Model;
                        }

                        progress("waiting response from " + model, 0.1f);
                        string instructions = $@"
Work on the following text (and only the following text):

########## TEXT BEGINS HERE
{xmlTrimmedSelectionRange}
########## TEXT ENDS HERE
";

						AsyncCollectionResult<StreamingChatCompletionUpdate> completionUpdates = GetResponseAsync(new Uri(modelSettings.Endpoint), modelSettings.ApiToken, model, instructions, "", promptText);
                        var enumerator = completionUpdates.GetAsyncEnumerator();
                        while (await enumerator.MoveNextAsync())
                        {
                            StreamingChatCompletionUpdate completionUpdate = enumerator.Current;
                            if (completionUpdate.ContentUpdate.Count > 0)
                            {
                                aiResponseBuilder.Append(completionUpdate.ContentUpdate[0].Text);
                                if (aiResponseBuilder.ToString().StartsWith("<think>") && aiResponseBuilder.ToString().Contains("</think>") == false)
                                {
                                    progress("thinking... (" + aiResponseBuilder.Length + ")", 0.5f);
                                }
                                else
                                {
                                    progress("receiving response (" + aiResponseBuilder.Length + ")", 0.75f);
                                }
                            }
                        }

                        progress("formatting response", 0.9f);


                        string aiResponse = aiResponseBuilder.ToString().Trim();

                        if (aiResponse.Trim().StartsWith("<think>")) // this is a thinking model
                        {
                            string pattern = @"\s*<think>(.*?)</think>\s*(.*)";
                            Match match = Regex.Match(aiResponse, pattern, RegexOptions.Singleline);

                            if (match.Success)
                            {
                                thoughts = match.Groups[1].Value;
                                aiResponse = match.Groups[2].Value;
                            }
                        }

                        if (prompt.Output == OutputType.text.ToString())
                        {
                            bool prevTrackRevisionsState = selection.Document.TrackRevisions;
                            Range insertRange = null;
                            if (preserveStyle)
                            {
                                insertRange = WordXmlConverter.InsertXmlFragmentIntoRange(aiResponse, trimmedSelectionRange);
                            }
                            else
                            {
                                if (trackedChanges)
									selection.Document.TrackRevisions = true;

                                // Apply the changes to the document.
                                insertRange = ApplyTrackedChanges(trimmedSelectionRange.Text, aiResponse, trimmedSelectionRange, trackedChanges);

                                if (trackedChanges)
									selection.Document.TrackRevisions = prevTrackRevisionsState;
                            }

                            if (GetThoughtsAsCommentsSettings())
                            {
                                // comments are not supported in headers and footers
                                if (selection.Comments != null)
                                {
									if (string.IsNullOrWhiteSpace(thoughts) == false && insertRange != null)
									{
										Range commentRange = insertRange.NewRange(insertRange.Start, insertRange.Start);

										selection.Comments.Add(commentRange, thoughts.Trim());
									}
								}
							}
                        }
                        else if (prompt.Output == OutputType.comments.ToString())
                        {
                            Range commentRange = trimmedSelectionRange.NewRange(trimmedSelectionRange.Start, trimmedSelectionRange.Start);

							if (GetThoughtsAsCommentsSettings())
                            {
                                // comments are not supported in headers and footers
                                if (selection.Comments != null)
                                {
									if (string.IsNullOrWhiteSpace(thoughts) == false)
										selection.Comments.Add(commentRange, thoughts.Trim());
								}
							}
                            if (string.IsNullOrWhiteSpace(aiResponse) == false && aiResponse.Trim().ToUpper() != "NO COMMENTS")
                            {
                                if (selection.Comments != null)
                                {
									selection.Comments.Add(commentRange, aiResponse);
                                }
							}
						}
                        else
                        {
                            throw new Exception("Unknown output type: " + prompt.Output);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Assistant Error");
            }
        }



        #region IRibbonExtensibility Members

        public string GetCustomUI(string ribbonID)
        {
            string xml = GetResourceText("WordAI.AssistantRibbon.xml");
            string placeholder = "<!--AGENTS-->";
            int index = xml.IndexOf(placeholder);
            if (index >= 0)
            {
                string buttons = BuildPinnedButtons();
                xml = xml.Replace(placeholder, buttons);
            }
            return xml;
        }

        private string BuildPinnedButtons()
        {
            var sb = new StringBuilder();
            var pm = new PromptManager();
            foreach (var entry in pm.Prompts)
            {
                if (entry.Pinned)
                {
                    string validId = "pin" + entry.Id;
                    sb.AppendLine($"        <button id='{validId}' label='{entry.Label}' size='large' imageMso='LightningBolt' onAction='OnPinnedAssistantClick'/>"
                    );
                }
            }
            if (sb.Length > 0)
            {
                return $"<group id='Agents' label='Agents'>\n" + sb.ToString() + "        </group>";
            }
            return string.Empty;
        }

        #endregion

        #region Ribbon Callbacks
        //Create callback methods here. For more information about adding callback methods, visit https://go.microsoft.com/fwlink/?LinkID=271226

        public void Ribbon_Load(Office.IRibbonUI ribbonUI)
        {
            this.ribbon = ribbonUI;
        }

        #endregion

        #region Helpers

        private static string GetResourceText(string resourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resourceNames = asm.GetManifestResourceNames();
            for (int i = 0; i < resourceNames.Length; ++i)
            {
                if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    using (StreamReader resourceReader = new StreamReader(asm.GetManifestResourceStream(resourceNames[i])))
                    {
                        if (resourceReader != null)
                        {
                            return resourceReader.ReadToEnd();
                        }
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
