# Guidance for Contributors

- Use four-space indentation and LF line endings.
- Run tests with `DOTNET_ROLL_FORWARD=Major dotnet test WordAI.Tests/WordAI.Tests.csproj` before committing.
- The `WordAI.VSTO` project targets .NET Framework 4.8 and requires Visual Studio on Windows; avoid building it on Linux.
- Tag releases as `vX.Y.Z` and push the tag to trigger the GitHub Actions release workflow.
- Update this file, the README, `CONTRIB.md`, and `RELEASE.md` when build or test instructions change.

