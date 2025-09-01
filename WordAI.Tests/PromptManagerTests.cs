using System;
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
                    ,Pinned = true
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
                Assert.Equal(entry.Pinned, loaded.Pinned);
            }
            finally
            {
                Environment.SetEnvironmentVariable("HOME", originalHome);
                System.IO.Directory.Delete(tempHome, true);
            }
        }
    }
}
