namespace WordAI
{
    partial class SettingsForm
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
            this.textEndpoint = new System.Windows.Forms.TextBox();
            this.labelEndpoint = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textApiToken = new System.Windows.Forms.TextBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.comboBoxModel = new System.Windows.Forms.ComboBox();
            this.labelModel = new System.Windows.Forms.Label();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textEndpoint
            // 
            this.textEndpoint.Location = new System.Drawing.Point(112, 45);
            this.textEndpoint.Name = "textEndpoint";
            this.textEndpoint.Size = new System.Drawing.Size(338, 20);
            this.textEndpoint.TabIndex = 0;
            this.textEndpoint.Text = "https://api.openai.com/";
            this.textEndpoint.Validated += new System.EventHandler(this.textEndpoint_Validated);
            // 
            // labelEndpoint
            // 
            this.labelEndpoint.AutoSize = true;
            this.labelEndpoint.Location = new System.Drawing.Point(30, 48);
            this.labelEndpoint.Name = "labelEndpoint";
            this.labelEndpoint.Size = new System.Drawing.Size(69, 13);
            this.labelEndpoint.TabIndex = 1;
            this.labelEndpoint.Text = "API Endpoint";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 83);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "API Token";
            // 
            // textApiToken
            // 
            this.textApiToken.Location = new System.Drawing.Point(112, 80);
            this.textApiToken.Name = "textApiToken";
            this.textApiToken.PasswordChar = '*';
            this.textApiToken.Size = new System.Drawing.Size(338, 20);
            this.textApiToken.TabIndex = 3;
            this.textApiToken.Validated += new System.EventHandler(this.textApiToken_Validated);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(366, 166);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(285, 166);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // comboBoxModel
            // 
            this.comboBoxModel.FormattingEnabled = true;
            this.comboBoxModel.Location = new System.Drawing.Point(112, 118);
            this.comboBoxModel.Name = "comboBoxModel";
            this.comboBoxModel.Size = new System.Drawing.Size(298, 21);
            this.comboBoxModel.TabIndex = 6;
            // 
            // labelModel
            // 
            this.labelModel.AutoSize = true;
            this.labelModel.Location = new System.Drawing.Point(30, 121);
            this.labelModel.Name = "labelModel";
            this.labelModel.Size = new System.Drawing.Size(76, 13);
            this.labelModel.TabIndex = 7;
            this.labelModel.Text = "Default  Model";
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRefresh.Location = new System.Drawing.Point(416, 113);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(32, 32);
            this.buttonRefresh.TabIndex = 8;
            this.buttonRefresh.Text = "🗘";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(484, 224);
            this.ControlBox = false;
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.labelModel);
            this.Controls.Add(this.comboBoxModel);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textApiToken);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelEndpoint);
            this.Controls.Add(this.textEndpoint);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(500, 270);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 240);
            this.Name = "SettingsForm";
            this.Text = "Model Endpoint";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textEndpoint;
        private System.Windows.Forms.Label labelEndpoint;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textApiToken;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ComboBox comboBoxModel;
        private System.Windows.Forms.Label labelModel;
        private System.Windows.Forms.Button buttonRefresh;
    }
}