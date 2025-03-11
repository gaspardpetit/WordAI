using Microsoft.Office.Core;
using Microsoft.Office.Interop.Word;
using Microsoft.Win32;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Office = Microsoft.Office.Core;
using DiffMatchPatch;
using System.Diagnostics;
using OpenAI;
using System.ClientModel;

namespace WordAI
{
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


        /// <summary>
        /// Processes each paragraph in the given range in beginning-to-end order.
        /// For each paragraph, it recalculates the paragraph’s start position by
        /// working back from the current document end, based on an initial snapshot.
        /// This helps accommodate changes at the top of the document.
        /// </summary>
        /// <param name="range">The range containing paragraphs to process.</param>
        /// <param name="processParagraph">The lambda to call for each paragraph’s range.</param>
        public static async System.Threading.Tasks.Task ProcessParagraphsWithDynamicRecalc(Range range, Func<Range, System.Threading.Tasks.Task> processParagraph)
        {
            if (range == null || range.Paragraphs.Count == 0 || processParagraph == null)
                return;

            // Create and show the progress form.
            ProgressForm progressForm = new ProgressForm();
            progressForm.Show();

            try
            {
                Document doc = range.Document;
                int totalParagraphs = range.Paragraphs.Count;


                // Capture an initial snapshot:
                // For each paragraph, record its length (excluding trailing paragraph mark)
                // and its starting position relative to the initial document end.
                int initialDocEnd = doc.Content.End;
                var paragraphSnapshots = new List<(int relativeOffset, int length)>();

                foreach (Paragraph para in range.Paragraphs)
                {
                    int paraStart = para.Range.Start;
                    int paraEnd = para.Range.End;
                    // Optionally remove the trailing paragraph mark (¶)
                    if (paraEnd > paraStart)
                        paraEnd--;

                    int length = paraEnd - paraStart;
                    // Calculate the relative offset from the document end at snapshot time.
                    // For example, if paraStart is 50 and initialDocEnd is 200, then relativeOffset is 150.
                    int relativeOffset = initialDocEnd - paraStart;

                    paragraphSnapshots.Add((relativeOffset, length));
                }

                // Iterate in natural (beginning-to-end) order
                for (int i = 0; i < paragraphSnapshots.Count; i++)
                {
                    var (relativeOffset, length) = paragraphSnapshots[i];
                    // Get the current document end.
                    int currentDocEnd = doc.Content.End;
                    // Recalculate the paragraph's start as the currentDocEnd minus the relative offset.
                    int newParaStart = currentDocEnd - relativeOffset;
                    int newParaEnd = newParaStart + length;

                    // Clamp the new end if it exceeds the document content.
                    if (newParaEnd > doc.Content.End)
                        newParaEnd = doc.Content.End;

                    // Create a fresh range for the paragraph.
                    Range paraRange = doc.Range(newParaStart, newParaEnd);

                    // Call the lambda.
                    await processParagraph(paraRange);
                    // Update progress.
                    progressForm.SetProgress(i + 1, totalParagraphs);
                    if (progressForm.isAborted)
                        break;
                    Globals.ThisAddIn.Application.StatusBar = $"Processing paragraph {i + 1} of {totalParagraphs}";
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

            // Accept all insertions temporarily to get the latest text
            StringBuilder newText = new StringBuilder();
            foreach (Revision rev in range.Revisions)
            {
                switch (rev.Type)
                {
                    case WdRevisionType.wdRevisionInsert:
                        // Append inserted text (latest version)
                        if (asXml)
                        {
                            string xmlFragment = WordXmlConverter.ConvertRangeToXmlFragment(rev.Range);
                            newText.Append(xmlFragment);
                        }
                        else
                        {
                            newText.Append(rev.Range.Text);
                        }
                        break;

                    case WdRevisionType.wdRevisionDelete:
                        // Ignore deleted text (not part of the latest version)
                        break;

                    default:
                        // Other revision types (formatting, etc.) are ignored
                        break;
                }
            }

            // If there are insertions, return them; otherwise, return the current range text
            if (newText.Length > 0)
                return newText.ToString();

            if (range.Text == null)
                return string.Empty;

            if (asXml)
                return WordXmlConverter.ConvertRangeToXmlFragment(range);

            return range.Text;
        }

        public static void ApplyTrackedChanges(List<Diff> diffs, Range selection, bool trackedChanges)
        {
            // Duplicate the current selection range.
            Range rng = selection.Duplicate;
            int basePos = rng.Start;

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
                        Range delRange = selection.Document.Range(basePos + offset, basePos + offset + diff.text.Length);
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
                        Range insRange = selection.Document.Range(basePos + offset, basePos + offset);
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
        }

        public static void ApplyTrackedChanges(string originalText, string modifiedText, Range selection, bool trackedChanges)
        {
            var dmp = new diff_match_patch();
            List<Diff> diffs = dmp.diff_main(originalText, modifiedText);
            dmp.diff_cleanupSemantic(diffs); // Optimize for better readability
            ApplyTrackedChanges(diffs, selection, trackedChanges);
        }

        public static async Task<string> GetResponseAsync(ModelSettings modelSettings, string selectionText, string documentText, string prompt)
        {
            try
            {
                if (string.IsNullOrEmpty(modelSettings.ApiToken))
                {
                    MessageBox.Show("API Key is missing. Please configure it in Settings.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return "Error: No API Key";
                }

                ChatMessage[] message = new ChatMessage[] {
                        // System messages represent instructions or other guidance about how the assistant should behave
                        new SystemChatMessage(prompt + documentText),
                        // User messages represent user input, whether historical or the most recen tinput
                        new UserChatMessage(selectionText),
                };

                OpenAIClientOptions options = new OpenAIClientOptions() { Endpoint = new Uri(modelSettings.Endpoint) };
                ApiKeyCredential credential = new ApiKeyCredential(modelSettings.ApiToken);
                ChatClient client = new ChatClient(model: modelSettings.DefaultModel, credential: credential, options: options);
                ChatCompletion completion = await client.CompleteChatAsync(message);

                return completion.Content[0].Text;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public static Range TrimSelection(Range selectionRange)
        {
            // Adjust the selection range to remove leading and trailing whitespace and control characters.
            string fullText = selectionRange.Text;
            if (string.IsNullOrEmpty(fullText))
                return selectionRange;

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

        private string dynamicMenuLabel;


        public string GetDynamicMenuLabel(Office.IRibbonControl control)
        {
            return dynamicMenuLabel;
        }


        public void OnCustomEntryClick(Office.IRibbonControl control)
        {
            // Remove the prefix 'p' to match the stored GUID.
            string guid = control.Id.Substring(1);
            _selectedPromptId = guid;
            PromptEntry prompt = new PromptManager().Get(_selectedPromptId);

            // Update the dynamic menu label to the selected prompt's name.
            dynamicMenuLabel = prompt.Label; // or use the prompt's display name if different

            // Force the Ribbon to refresh the dynamic menu's label.
            ribbon.InvalidateControl("DynamicMenu");
        }

        public async void OnCorrectButtonClick(IRibbonControl control)
        {
            try
            {
                bool trackedChanges = GetTrackedChangesSettings();

                var app = Globals.ThisAddIn.Application;
                Selection wholeSelection = app.Selection;
                var doc = app.ActiveDocument;

                await ProcessParagraphsWithDynamicRecalc(wholeSelection.Range, async selection =>
                {
                    Range trimedSelectionRange = TrimSelection(selection);

                    if (selection != null && selection.Text != null && selection.Text.Trim().Length > 0)
                    {
                        // Call OpenAI asynchronously
                        string aiResponse = await GetResponseAsync(ModelManager.FromSettings(), trimedSelectionRange.Text, doc.Content.Text, IMPROVE_PROMPT);

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

            string styleInstruction = string.Empty;
            if (preserveStyle)
            {
                styleInstruction = "In your answer, try to preserve the XML formatting.\n";
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

                await ProcessParagraphsWithDynamicRecalc(wholeSelection.Range, async selection =>
                {

                    var doc = app.ActiveDocument;

                    // Trim the selection to remove any leading/trailing whitespace.
                    Range trimmedSelectionRange = TrimSelection(selection);

                    if (selection != null && selection.Text != null && selection.Text.Trim().Length > 0)
                    {
                        // Get a range of text from the beginning of the document to the selection start.
                        Range prefixRange = doc.Content;
                        prefixRange.SetRange(0, selection.Start);
                        Range suffixRange = doc.Content;
                        suffixRange.SetRange(selection.End, doc.Content.End);

                        // Use the prompt text from the selected prompt.
                        string promptText = string.Empty;

                        // ideally we would provide the context with formatting, but in reality, COM interop is too slow
                        // and fetching style for an entire document can take minutes if not hours on long documents.
                        bool contextWithStyle = /*preserveStyle*/false;
                        string prefix = GetText(prefixRange, contextWithStyle).Trim();
                        string suffix = GetText(suffixRange, contextWithStyle).Trim();

                        prefix.Replace("\f", "<break/>");
                        suffix.Replace("\f", "<break/>");

                        if (prompt.Context == ContextType.none.ToString() || (string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(suffix)))
                        {
                            promptText = $@"
## INSTRUCTIONS

{prompt.Prompt}
{styleInstruction}

";
                        }
                        else if (prompt.Context == ContextType.prefix.ToString() || string.IsNullOrEmpty(suffix))
                        {
                            promptText = $@"

## INSTRUCTIONS

{prompt.Prompt}
{styleInstruction}

So far, this is the content of the document, provided here for context:

### TEXT PRECEEDING THE INSTRUCTIONS

<document>
{prefix}                            
</document>

";
                        }
                        else if (prompt.Context == ContextType.suffix.ToString() || string.IsNullOrEmpty(prefix))
                        {
                            promptText = $@"

## INSTRUCTIONS

{prompt.Prompt}
{styleInstruction}

Your answer will be inserted as a prefix to the following text, provided here for context (do not repeat it):

### TEXT FOLLOWING THE INSTRUCTIONS

<document>
{suffix}                            
</document>

";
                        }
                        else // if (prompt.Context == ContextType.suffix.ToString() || string.IsNullOrEmpty(prefix))
                        {
                            promptText = $@"

## INSTRUCTIONS

{prompt.Prompt}
{styleInstruction}

The document your are editing is between the following text, provided for context (do not repeat this text in your answer):

### TEXT IMMEDIATELY BEFORE YOUR ANSWER:

<document>
{prefix}
</document>

### TEXT IMMEDIATELY AFTER YOUR ANSWER:

<document>
{suffix}
</document>

";
                        }

                        // Call OpenAI asynchronously using the selected prompt.
                        string xmlTrimmedSelectionRange = WordXmlConverter.ConvertRangeToXmlFragment(trimmedSelectionRange);
                        string aiResponse = await GetResponseAsync(ModelManager.FromSettings(), xmlTrimmedSelectionRange, "", promptText);

                        bool prevTrackRevisionsState = doc.TrackRevisions;
                        if (preserveStyle)
                        {
                            WordXmlConverter.InsertXmlFragmentIntoRange(aiResponse, trimmedSelectionRange);
                        }
                        else
                        {
                            if (trackedChanges)
                                doc.TrackRevisions = true;

                            // Apply the changes to the document.
                            ApplyTrackedChanges(trimmedSelectionRange.Text, aiResponse, trimmedSelectionRange, trackedChanges);

                            if (trackedChanges)
                                doc.TrackRevisions = prevTrackRevisionsState;
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
            return GetResourceText("WordAI.AssistantRibbon.xml");
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
