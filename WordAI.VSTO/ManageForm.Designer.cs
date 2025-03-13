namespace WordAI
{
    partial class ManageForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listBoxPrompts = new System.Windows.Forms.ListBox();
            this.textBoxPrompt = new System.Windows.Forms.TextBox();
            this.textBoxPromptName = new System.Windows.Forms.TextBox();
            this.labelPromptName = new System.Windows.Forms.Label();
            this.labelPrompt = new System.Windows.Forms.Label();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.labelModel = new System.Windows.Forms.Label();
            this.comboBoxModel = new System.Windows.Forms.ComboBox();
            this.labelContext = new System.Windows.Forms.Label();
            this.checkBoxContextPreceding = new System.Windows.Forms.CheckBox();
            this.checkBoxContextFollowing = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.radioButtonText = new System.Windows.Forms.RadioButton();
            this.radioButtonComments = new System.Windows.Forms.RadioButton();
            this.groupBoxOutput = new System.Windows.Forms.GroupBox();
            this.groupBoxOutput.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBoxPrompts
            // 
            this.listBoxPrompts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxPrompts.FormattingEnabled = true;
            this.listBoxPrompts.Location = new System.Drawing.Point(30, 24);
            this.listBoxPrompts.Name = "listBoxPrompts";
            this.listBoxPrompts.Size = new System.Drawing.Size(236, 355);
            this.listBoxPrompts.TabIndex = 0;
            this.listBoxPrompts.SelectedIndexChanged += new System.EventHandler(this.listBoxPrompts_SelectedIndexChanged);
            // 
            // textBoxPrompt
            // 
            this.textBoxPrompt.AcceptsReturn = true;
            this.textBoxPrompt.AcceptsTab = true;
            this.textBoxPrompt.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPrompt.Location = new System.Drawing.Point(305, 77);
            this.textBoxPrompt.Multiline = true;
            this.textBoxPrompt.Name = "textBoxPrompt";
            this.textBoxPrompt.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxPrompt.Size = new System.Drawing.Size(467, 203);
            this.textBoxPrompt.TabIndex = 1;
            // 
            // textBoxPromptName
            // 
            this.textBoxPromptName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPromptName.Location = new System.Drawing.Point(343, 24);
            this.textBoxPromptName.Name = "textBoxPromptName";
            this.textBoxPromptName.Size = new System.Drawing.Size(429, 20);
            this.textBoxPromptName.TabIndex = 2;
            // 
            // labelPromptName
            // 
            this.labelPromptName.AutoSize = true;
            this.labelPromptName.Location = new System.Drawing.Point(302, 30);
            this.labelPromptName.Name = "labelPromptName";
            this.labelPromptName.Size = new System.Drawing.Size(35, 13);
            this.labelPromptName.TabIndex = 3;
            this.labelPromptName.Text = "Name";
            // 
            // labelPrompt
            // 
            this.labelPrompt.AutoSize = true;
            this.labelPrompt.Location = new System.Drawing.Point(305, 61);
            this.labelPrompt.Name = "labelPrompt";
            this.labelPrompt.Size = new System.Drawing.Size(43, 13);
            this.labelPrompt.TabIndex = 4;
            this.labelPrompt.Text = "Prompt:";
            // 
            // buttonDelete
            // 
            this.buttonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonDelete.Location = new System.Drawing.Point(305, 377);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(75, 23);
            this.buttonDelete.TabIndex = 5;
            this.buttonDelete.Text = "Delete";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.Location = new System.Drawing.Point(697, 378);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 6;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // labelModel
            // 
            this.labelModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelModel.AutoSize = true;
            this.labelModel.Location = new System.Drawing.Point(305, 293);
            this.labelModel.Name = "labelModel";
            this.labelModel.Size = new System.Drawing.Size(36, 13);
            this.labelModel.TabIndex = 7;
            this.labelModel.Text = "Model";
            // 
            // comboBoxModel
            // 
            this.comboBoxModel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxModel.FormattingEnabled = true;
            this.comboBoxModel.Location = new System.Drawing.Point(351, 290);
            this.comboBoxModel.Name = "comboBoxModel";
            this.comboBoxModel.Size = new System.Drawing.Size(424, 21);
            this.comboBoxModel.TabIndex = 8;
            // 
            // labelContext
            // 
            this.labelContext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelContext.AutoSize = true;
            this.labelContext.Location = new System.Drawing.Point(305, 323);
            this.labelContext.Name = "labelContext";
            this.labelContext.Size = new System.Drawing.Size(43, 13);
            this.labelContext.TabIndex = 9;
            this.labelContext.Text = "Context";
            // 
            // checkBoxContextPreceding
            // 
            this.checkBoxContextPreceding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxContextPreceding.AutoSize = true;
            this.checkBoxContextPreceding.Location = new System.Drawing.Point(354, 322);
            this.checkBoxContextPreceding.Name = "checkBoxContextPreceding";
            this.checkBoxContextPreceding.Size = new System.Drawing.Size(98, 17);
            this.checkBoxContextPreceding.TabIndex = 10;
            this.checkBoxContextPreceding.Text = "Preceding Text";
            this.checkBoxContextPreceding.UseVisualStyleBackColor = true;
            // 
            // checkBoxContextFollowing
            // 
            this.checkBoxContextFollowing.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxContextFollowing.AutoSize = true;
            this.checkBoxContextFollowing.Location = new System.Drawing.Point(458, 322);
            this.checkBoxContextFollowing.Name = "checkBoxContextFollowing";
            this.checkBoxContextFollowing.Size = new System.Drawing.Size(94, 17);
            this.checkBoxContextFollowing.TabIndex = 11;
            this.checkBoxContextFollowing.Text = "Following Text";
            this.checkBoxContextFollowing.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(305, 351);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Output";
            // 
            // radioButtonText
            // 
            this.radioButtonText.AutoSize = true;
            this.radioButtonText.Location = new System.Drawing.Point(9, 9);
            this.radioButtonText.Name = "radioButtonText";
            this.radioButtonText.Size = new System.Drawing.Size(46, 17);
            this.radioButtonText.TabIndex = 13;
            this.radioButtonText.TabStop = true;
            this.radioButtonText.Text = "Text";
            this.radioButtonText.UseVisualStyleBackColor = true;
            // 
            // radioButtonComments
            // 
            this.radioButtonComments.AutoSize = true;
            this.radioButtonComments.Location = new System.Drawing.Point(66, 9);
            this.radioButtonComments.Name = "radioButtonComments";
            this.radioButtonComments.Size = new System.Drawing.Size(74, 17);
            this.radioButtonComments.TabIndex = 14;
            this.radioButtonComments.TabStop = true;
            this.radioButtonComments.Text = "Comments";
            this.radioButtonComments.UseVisualStyleBackColor = true;
            // 
            // groupBoxOutput
            // 
            this.groupBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBoxOutput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.groupBoxOutput.Controls.Add(this.radioButtonComments);
            this.groupBoxOutput.Controls.Add(this.radioButtonText);
            this.groupBoxOutput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBoxOutput.Location = new System.Drawing.Point(354, 340);
            this.groupBoxOutput.Name = "groupBoxOutput";
            this.groupBoxOutput.Size = new System.Drawing.Size(159, 31);
            this.groupBoxOutput.TabIndex = 15;
            this.groupBoxOutput.TabStop = false;
            // 
            // ManageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(811, 419);
            this.Controls.Add(this.groupBoxOutput);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBoxContextFollowing);
            this.Controls.Add(this.checkBoxContextPreceding);
            this.Controls.Add(this.labelContext);
            this.Controls.Add(this.comboBoxModel);
            this.Controls.Add(this.labelModel);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.labelPrompt);
            this.Controls.Add(this.labelPromptName);
            this.Controls.Add(this.textBoxPromptName);
            this.Controls.Add(this.textBoxPrompt);
            this.Controls.Add(this.listBoxPrompts);
            this.Name = "ManageForm";
            this.Text = "ManageForm";
            this.Load += new System.EventHandler(this.ManageForm_Load);
            this.groupBoxOutput.ResumeLayout(false);
            this.groupBoxOutput.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxPrompts;
        private System.Windows.Forms.TextBox textBoxPrompt;
        private System.Windows.Forms.TextBox textBoxPromptName;
        private System.Windows.Forms.Label labelPromptName;
        private System.Windows.Forms.Label labelPrompt;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Label labelModel;
        private System.Windows.Forms.ComboBox comboBoxModel;
        private System.Windows.Forms.Label labelContext;
        private System.Windows.Forms.CheckBox checkBoxContextPreceding;
        private System.Windows.Forms.CheckBox checkBoxContextFollowing;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton radioButtonText;
        private System.Windows.Forms.RadioButton radioButtonComments;
        private System.Windows.Forms.GroupBox groupBoxOutput;
    }
}