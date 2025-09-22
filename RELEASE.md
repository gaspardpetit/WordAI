# Release Guide

## Automated Release Workflow

- The `Release WordAI Add-in` GitHub Actions workflow runs on tags that match `v*`.
- It restores NuGet packages, runs unit tests, publishes the ClickOnce installer with a temporary self-signed certificate, and zips the output (`setup.exe`, the `.vsto` manifest, and `Application Files/`).
- The zipped installer is attached to both the workflow run as an artifact and to the GitHub release generated for the tag.
- Tag versions in `major.minor.patch` form (for example `v1.2.3`). The workflow pads the ClickOnce `ApplicationVersion` with a `.0` revision segment when necessary.

## Publishing a Release

1. Push the commit you want to release to GitHub.
2. Create an annotated tag such as `v1.2.3` that reflects the desired version number.
3. Push the tag (`git push origin v1.2.3`).
4. Monitor the `Release WordAI Add-in` workflow run and confirm that the generated GitHub release includes the packaged installer.
