using OpenAI.Models;
using OpenAI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Win32;
using System.Net;
using System.ClientModel;

namespace WordAI
{
    public class ModelSettings
    {
        public string Endpoint { get; set; }
        public string ApiToken { get; set; }
        public string DefaultModel { get; set; }

        private List<string> MODELS = null;

        public ModelSettings()
        {
        }

        public ModelSettings(ModelSettings rhs)
        {
            Endpoint = rhs.Endpoint;
            ApiToken = rhs.ApiToken;
            DefaultModel = rhs.DefaultModel;
            if (rhs.MODELS != null)
            {
                MODELS = new List<string>(rhs.MODELS);
            }
        }

        public void SetEndpoint(string endpoint, string apiToken)
        {
            if (endpoint == Endpoint && apiToken == ApiToken)
                return;

            this.ApiToken = apiToken;
            this.Endpoint = endpoint;
            MODELS = null;
        }

        public void RefreshModels()
        {
            try
            {
                OpenAIClientOptions options = new OpenAIClientOptions() { Endpoint = new Uri(Endpoint) };
                ApiKeyCredential credential = new ApiKeyCredential(ApiToken);

                OpenAIModelClient client = new OpenAIModelClient(credential: credential, options: options);
                ClientResult<OpenAIModelCollection> models = client.GetModels();
                List<string> allModels = new List<string>();
                foreach (OpenAIModel model in models.Value)
                {
                    allModels.Add(model.Id);
                }
                MODELS = allModels;
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                MODELS = new List<string> { ex.Message };
            }
        }

        public List<string> Models { 
            get {
                if (MODELS == null)
                    RefreshModels();
                return MODELS; 
            }
        }
    }



    public class ModelManager
    {
        private static ModelSettings GLOBAL = null;

        private const string RegistryPath = @"Software\AssistantWordAddin"; // Custom registry key

        public static ModelSettings FromSettings()
        {
            if (GLOBAL == null)
            {
                GLOBAL = LoadSettings();
            }

            return new ModelSettings(GLOBAL);
        }

        public ModelManager()
        {
        }

        public static ModelSettings LoadSettings()
        {
            ModelSettings settings = new ModelSettings();
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
            {
                if (key != null)
                {
                    settings.ApiToken = key.GetValue("OpenAIKey", "").ToString();
                    settings.Endpoint = key.GetValue("OpenAIEndpoint", "https://api.openai.com/").ToString();
                    settings.DefaultModel = key.GetValue("Model", "gpt-4o").ToString();
                }
            }
            return settings;
        }

        public static void PersistSettings(ModelSettings settings)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
            {
                key.SetValue("OpenAIKey", settings.ApiToken);
                key.SetValue("OpenAIEndpoint", settings.Endpoint);
                key.SetValue("Model", settings.DefaultModel);
            }
            GLOBAL = settings;
        }
    }
}
