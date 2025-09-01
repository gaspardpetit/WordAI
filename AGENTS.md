# Guidance for Contributors

- Use four-space indentation and LF line endings.
- Run tests with `DOTNET_ROLL_FORWARD=Major dotnet test WordAI.Tests/WordAI.Tests.csproj` before committing.
- The `WordAI.VSTO` project targets .NET Framework 4.8 and requires Visual Studio on Windows; avoid building it on Linux.
- Update this file and the README when build or test instructions change.
