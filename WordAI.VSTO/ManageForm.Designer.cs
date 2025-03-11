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
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxContextPreceding = new System.Windows.Forms.CheckBox();
            this.checkBoxContextFollowing = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // listBoxPrompts
            // 
            this.listBoxPrompts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBoxPrompts.FormattingEnabled = true;
            this.listBoxPrompts.Location = new System.Drawing.Point(30, 24);
            this.listBoxPrompts.Name = "listBoxPrompts";
            this.listBoxPrompts.Size = new System.Drawing.Size(236, 394);
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
            this.textBoxPrompt.Location = new System.Drawing.Point(305, 89);
            this.textBoxPrompt.Multiline = true;
            this.textBoxPrompt.Name = "textBoxPrompt";
            this.textBoxPrompt.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxPrompt.Size = new System.Drawing.Size(467, 233);
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
            this.labelPrompt.Location = new System.Drawing.Point(302, 73);
            this.labelPrompt.Name = "labelPrompt";
            this.labelPrompt.Size = new System.Drawing.Size(43, 13);
            this.labelPrompt.TabIndex = 4;
            this.labelPrompt.Text = "Prompt:";
            // 
            // buttonDelete
            // 
            this.buttonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonDelete.Location = new System.Drawing.Point(305, 401);
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
            this.buttonSave.Location = new System.Drawing.Point(697, 402);
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
            this.labelModel.Location = new System.Drawing.Point(302, 331);
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
            this.comboBoxModel.Location = new System.Drawing.Point(348, 328);
            this.comboBoxModel.Name = "comboBoxModel";
            this.comboBoxModel.Size = new System.Drawing.Size(424, 21);
            this.comboBoxModel.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(305, 362);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Context";
            // 
            // checkBoxContextPreceding
            // 
            this.checkBoxContextPreceding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxContextPreceding.AutoSize = true;
            this.checkBoxContextPreceding.Location = new System.Drawing.Point(354, 361);
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
            this.checkBoxContextFollowing.Location = new System.Drawing.Point(458, 361);
            this.checkBoxContextFollowing.Name = "checkBoxContextFollowing";
            this.checkBoxContextFollowing.Size = new System.Drawing.Size(94, 17);
            this.checkBoxContextFollowing.TabIndex = 11;
            this.checkBoxContextFollowing.Text = "Following Text";
            this.checkBoxContextFollowing.UseVisualStyleBackColor = true;
            // 
            // ManageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(811, 443);
            this.Controls.Add(this.checkBoxContextFollowing);
            this.Controls.Add(this.checkBoxContextPreceding);
            this.Controls.Add(this.label1);
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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxContextPreceding;
        private System.Windows.Forms.CheckBox checkBoxContextFollowing;
    }
}