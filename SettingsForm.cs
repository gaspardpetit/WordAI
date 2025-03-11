using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace WordAI
{
    public partial class SettingsForm : Form
    {
        private const string RegistryPath = @"Software\AssistantWordAddin"; // Custom registry key

        private List<string> _previousList = new List<string>();
        private ModelSettings _modelSettings = null;

        public SettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        void RefreshModels()
        {
            // Remember the current selection, if any.
            string previousSelection = comboBoxModel.SelectedItem as string;

            string apiKey = this.textApiToken.Text;
            string endpoint = this.textEndpoint.Text;

            this._modelSettings.SetEndpoint(endpoint, apiKey);
            List<string> models = this._modelSettings.Models;
            if (models == _previousList)
                return;

            _previousList = models;

            comboBoxModel.Items.Clear();

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
                // Otherwise, select the first item.
                comboBoxModel.SelectedIndex = 0;
            }
        }

        private void SelectModel(string model)
        {
            comboBoxModel.SelectedItem = model;
        }


        private void LoadSettings()
        {
            this._modelSettings = ModelManager.LoadSettings();
            string currentModel = "gpt-4o";
            ModelManager modelManager = new ModelManager();
            this.textApiToken.Text = _modelSettings.ApiToken;
            this.textEndpoint.Text = _modelSettings.Endpoint;
            currentModel = _modelSettings.DefaultModel;

            RefreshModels();
            SelectModel(currentModel);
        }


        private void buttonOK_Click(object sender, EventArgs e)
        {
            _modelSettings.DefaultModel = this.comboBoxModel.Text;
            ModelManager.PersistSettings(this._modelSettings);
            this.Close();
        }

        private void textEndpoint_Validated(object sender, EventArgs e)
        {
            try
            {
                RefreshModels();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error refreshing models: " + ex.Message);
                // Optionally, display an error message or log it.
            }
        }

        private void textApiToken_Validated(object sender, EventArgs e)
        {
            try
            {
                RefreshModels();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error refreshing models: " + ex.Message);
                // Optionally, display an error message or log it.
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            this._modelSettings = new ModelSettings();
            RefreshModels();
        }
    }
}
