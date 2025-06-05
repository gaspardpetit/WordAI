using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WordAI
{
    public partial class ManageForm : Form
    {
        // Constant representing the "New..." entry in the ListBox.
        private const string NEW_ENTRY = "New...";
        private const string DEFAULT_MODEL = "[Default Model]";

        private readonly PromptManager promptManager;

        public ManageForm()
        {
            InitializeComponent();
            promptManager = new PromptManager();
        }

        void RefreshModels()
        {
            // Remember the current selection, if any.
            string previousSelection = comboBoxModel.SelectedItem as string;

            ModelSettings modelSettings = ModelManager.FromSettings();
            List<string> models = modelSettings.Models;

            comboBoxModel.Items.Clear();
            comboBoxModel.Items.Add(DEFAULT_MODEL);

            foreach (string model in models)
            {
                comboBoxModel.Items.Add(model);
            }

            // If the previous selection still exists, re-select it.
            if (!string.IsNullOrEmpty(previousSelection) && comboBoxModel.Items.Contains(previousSelection))
            {
                comboBoxModel.SelectedItem = previousSelection;
            }
            else if (comboBoxModel.Items.Count > 0)
            {
                comboBoxModel.SelectedIndex = 0;
            }
        }

        // Form Load event: load the prompt entries into the listBox.
        private void ManageForm_Load(object sender, EventArgs e)
        {
            RefreshModels();
            RefreshListBox();
        }

        // Populates the listBox with saved prompts plus a "New..." option.
        private void RefreshListBox()
        {
            listBoxPrompts.Items.Clear();

            // Add all saved prompt names.
            foreach (var prompt in promptManager.Prompts)
            {
                if (prompt.Label == null)
                    prompt.Label = GenerateNewName();
                listBoxPrompts.Items.Add(prompt.Label);
            }
            // Append the "New..." entry.
            listBoxPrompts.Items.Add(NEW_ENTRY);
        }

        // When the user selects an item, load its details into the text boxes.
        private void listBoxPrompts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxPrompts.SelectedItem == null)
                return;

            string selectedName = listBoxPrompts.SelectedItem.ToString();

            if (selectedName == NEW_ENTRY)
            {
                // If "New..." is selected, assign a default new name.
                textBoxPromptName.Text = GenerateNewName();
                textBoxPrompt.Text = string.Empty;
                checkBoxContextPreceding.Checked = true;
                checkBoxContextFollowing.Checked = true;
                comboBoxModel.SelectedItem = DEFAULT_MODEL;
                radioButtonText.Checked = true;
            }
            else
            {
                // Load the details of the selected prompt.
                var prompt = promptManager.Prompts.Find(p => p.Label.Equals(selectedName, StringComparison.OrdinalIgnoreCase));
                if (prompt != null)
                {
                    textBoxPromptName.Text = prompt.Label;
                    textBoxPrompt.Text = prompt.Prompt;
                    checkBoxContextPreceding.Checked = string.IsNullOrEmpty(prompt.Context) || (prompt.Context == ContextType.document.ToString() || prompt.Context == ContextType.prefix.ToString());
                    checkBoxContextFollowing.Checked = string.IsNullOrEmpty(prompt.Context) || (prompt.Context == ContextType.document.ToString() || prompt.Context == ContextType.suffix.ToString());
                    comboBoxModel.SelectedItem = string.IsNullOrEmpty(prompt.Model) ? DEFAULT_MODEL : prompt.Model;
                    radioButtonText.Checked = (prompt.Output == OutputType.text.ToString());
                    radioButtonComments.Checked = (prompt.Output != OutputType.text.ToString());
                }
            }
        }

        // Generate a default new name ("Untitled #") ensuring uniqueness.
        private string GenerateNewName()
        {
            int count = 1;
            string newName = $"Untitled {count}";
            while (promptManager.Prompts.Exists(p => p.Label != null && p.Label.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            {
                count++;
                newName = $"Untitled {count}";
            }
            return newName;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            string name = textBoxPromptName.Text.Trim();
            string promptText = textBoxPrompt.Text.Trim();

            string promptModel = comboBoxModel.Text.Trim();
            if (promptModel == DEFAULT_MODEL)
                promptModel = string.Empty;

            ContextType contextType = ContextType.none;
            if (checkBoxContextPreceding.Checked && checkBoxContextFollowing.Checked)
                contextType = ContextType.document;
            else if (checkBoxContextPreceding.Checked)
                contextType = ContextType.prefix;
            else if (checkBoxContextFollowing.Checked)
                contextType = ContextType.suffix;
            else
                contextType = ContextType.none;

            string output = OutputType.text.ToString();
            if (radioButtonText.Checked)
                output = OutputType.text.ToString();
            else if (radioButtonComments.Checked)
                output = OutputType.comments.ToString();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a name for the prompt.");
                return;
            }

            // Determine if the user selected an existing prompt or "New..."
            string selectedName = listBoxPrompts.SelectedItem?.ToString();

            // If "New..." is selected or the name doesn't exist, add as a new prompt.
            var existing = promptManager.Prompts.Find(p => p.Label.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (selectedName == NEW_ENTRY || existing == null)
            {
                // Create a new prompt entry.
                var newEntry = new PromptEntry {
                    Label = name,
                    Prompt = promptText,
                    Context = contextType.ToString(),
                    Model = promptModel,
                    Output = output,
                    Id = Guid.NewGuid().ToString()
                };
                promptManager.Prompts.Add(newEntry);
            }
            else
            {
                // If the user changed the name, create a new entry (keeping the old one intact).
                // Otherwise, update the existing prompt's text.
                if (!selectedName.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    var newEntry = new PromptEntry { 
                        Label= name, 
                        Prompt = promptText, 
                        Id = Guid.NewGuid().ToString(),
                        Context = contextType.ToString(),
                        Model = promptModel,
                        Output = output,
                    };
                    promptManager.Prompts.Add(newEntry);
                }
                else
                {
                    existing.Prompt = promptText;
                    existing.Context = contextType.ToString();
                    existing.Model = promptModel;
                    existing.Output = output;
                }
            }

            promptManager.Save();
            RefreshListBox();
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            string selectedName = listBoxPrompts.SelectedItem?.ToString();
            if (selectedName == null || selectedName == NEW_ENTRY)
            {
                // Nothing to delete if "New..." is selected.
                return;
            }

            var prompt = promptManager.Prompts.Find(p => p.Label.Equals(selectedName, StringComparison.OrdinalIgnoreCase));
            if (prompt != null)
            {
                var result = MessageBox.Show($"Are you sure you want to delete prompt '{selectedName}'?",
                                             "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    promptManager.Prompts.Remove(prompt);
                    promptManager.Save();
                    RefreshListBox();
                }
            }
        }
    }

}
