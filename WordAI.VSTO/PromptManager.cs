using System;
using System.Collections.Generic;

namespace WordAI
{
    public enum ContextType
    {
        none = 0,
        prefix = 1,
        suffix = 2,
        document = 3,
    }

	public enum ChunkingMode
	{
		Paragraph,     // Default: one paragraph per chunk
		ListAware,     // Group contiguous list items into a single chunk
		WholeBlock,    // Process the entire selection as one chunk
		Sentence,      // (Optional) Split by sentence boundaries (if you add NLP support)
		Hybrid         // (Optional) Use heuristics based on size + structure
	}


	public enum OutputType
    {
        text = 0,
        comments = 1,
    }

    // Our prompt model (from ManageForm)
    public class PromptEntry
    {
        public string Id { get; set; }    // Unique identifier (GUID as string)
        public string Label { get; set; }
        public string Prompt { get; set; }
        public string Model { get; set; }
        public string Context { get; set; }
        public string Output { get; set; }
		public ChunkingMode Mode { get; set; }  // Enum: Paragraph, ListAware, WholeBlock
	}

    // A manager that loads and saves prompts from a JSON file.
    public class PromptManager
    {
        public List<PromptEntry> Prompts { get; set; }
        private readonly string filePath;

        public PromptManager()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folder = System.IO.Path.Combine(appData, "WordAI");
            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }
            filePath = System.IO.Path.Combine(folder, "prompts.json");
            Load();
        }

        public void Load()
        {
            if (System.IO.File.Exists(filePath))
            {
                string json = System.IO.File.ReadAllText(filePath);
                Prompts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PromptEntry>>(json) ?? new List<PromptEntry>();
                foreach (PromptEntry entry in Prompts)
                {
                    if (entry.Id == null)
                        entry.Id = Guid.NewGuid().ToString();
                    if (entry.Context == null)
                        entry.Context = ContextType.document.ToString();
                    if (entry.Output == null)
                        entry.Output = OutputType.text.ToString();
                }
            }
            else
            {
                Prompts = new List<PromptEntry>();
            }
        }

        public void Save()
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(Prompts, Newtonsoft.Json.Formatting.Indented);
            System.IO.File.WriteAllText(filePath, json);
        }

        internal PromptEntry Get(string guid)
        {
            foreach (PromptEntry entry in Prompts)
            {
                if (entry.Id == guid)
                    return entry;
            }
            return new PromptEntry();
        }
    }
}
