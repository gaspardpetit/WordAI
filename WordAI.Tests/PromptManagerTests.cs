using System;
using System.IO;
using Xunit;
using WordAI; // namespace of PromptManager and PromptEntry

namespace WordAI.Tests
{
    public class PromptManagerTests
    {
        [Fact]
        public void SaveAndLoad_PreservesPromptEntries()
        {
            // Create unique temp home to isolate file path
            string tempHome = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(tempHome);
            string originalHome = Environment.GetEnvironmentVariable("HOME");
            Environment.SetEnvironmentVariable("HOME", tempHome);
            try
            {
                // first manager
                var manager = new PromptManager();
                manager.Prompts.Clear();
                var entry = new PromptEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    Label = "Test",
                    Prompt = "Say hello",
                    Model = "gpt",
                    Context = ContextType.prefix.ToString(),
                    Output = OutputType.comments.ToString(),
                    Mode = ChunkingMode.ListAware
                };
                manager.Prompts.Add(entry);
                manager.Save();

                // new manager loads existing data
                var manager2 = new PromptManager();
                Assert.Single(manager2.Prompts);
                var loaded = manager2.Prompts[0];
                Assert.Equal(entry.Id, loaded.Id);
                Assert.Equal(entry.Label, loaded.Label);
                Assert.Equal(entry.Prompt, loaded.Prompt);
                Assert.Equal(entry.Model, loaded.Model);
                Assert.Equal(entry.Context, loaded.Context);
                Assert.Equal(entry.Output, loaded.Output);
                Assert.Equal(entry.Mode, loaded.Mode);
            }
            finally
            {
                Environment.SetEnvironmentVariable("HOME", originalHome);
                System.IO.Directory.Delete(tempHome, true);
            }
        }

        [Fact]
        public void Load_FillsMissingFieldsWithDefaults()
        {
            string tempHome = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempHome);
            string originalHome = Environment.GetEnvironmentVariable("HOME");
            Environment.SetEnvironmentVariable("HOME", tempHome);
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string folder = Path.Combine(appData, "WordAI");
                Directory.CreateDirectory(folder);
                string path = Path.Combine(folder, "prompts.json");
                File.WriteAllText(path, "[{\"Label\":\"T\",\"Prompt\":\"Hi\"}]");

                var manager = new PromptManager();
                Assert.Single(manager.Prompts);
                var loaded = manager.Prompts[0];
                Assert.False(string.IsNullOrEmpty(loaded.Id));
                Assert.Equal(ContextType.document.ToString(), loaded.Context);
                Assert.Equal(OutputType.text.ToString(), loaded.Output);
            }
            finally
            {
                Environment.SetEnvironmentVariable("HOME", originalHome);
                Directory.Delete(tempHome, true);
            }
        }

        [Fact]
        public void Get_ReturnsExistingOrNewEntry()
        {
            string tempHome = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempHome);
            string originalHome = Environment.GetEnvironmentVariable("HOME");
            Environment.SetEnvironmentVariable("HOME", tempHome);
            try
            {
                var manager = new PromptManager();
                manager.Prompts.Clear();
                var entry = new PromptEntry { Id = Guid.NewGuid().ToString(), Label = "A" };
                manager.Prompts.Add(entry);

                var found = manager.Get(entry.Id);
                Assert.Equal(entry.Id, found.Id);

                var missing = manager.Get(Guid.NewGuid().ToString());
                Assert.Null(missing.Id);
            }
            finally
            {
                Environment.SetEnvironmentVariable("HOME", originalHome);
                Directory.Delete(tempHome, true);
            }
        }
    }
}
