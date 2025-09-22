# Contributor Guide

## Local Setup

- Use four-space indentation and LF line endings.
- Restore NuGet packages with `nuget restore WordAI.VSTO/WordAI.VSTO.csproj -SolutionDirectory .` if Visual Studio does not do it automatically.

## Testing

- Run the prompt manager unit tests with `DOTNET_ROLL_FORWARD=Major dotnet test WordAI.Tests/WordAI.Tests.csproj`.

## Building the VSTO Add-in

- Build on a Windows machine with Visual Studio 2022 or later and the .NET Framework 4.8 tooling installed.
- Use the `Release` configuration to generate the ClickOnce output in `WordAI.VSTO/bin/Release/`.

