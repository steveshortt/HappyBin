# Project Description

HappyBin is an auto-updater for .NET apps. It is designed as an api, and can be used as a boot-strap or a passive updater. Every app deserves a HappyBin!

## How does HappyBin AutoUpdater work?
HappyBin AutoUpdater is designed to be a compiled exe that sits next your primary application. When launched, the auto-updater will check a Http/Ftp/UNC for an update config file, compare the advertised version with the current version of your exe, download a zip, extract the files to apply the update, and then (re-)launch your exe. This process can run as a boot-strap or passive/background (see below) to suit your needs. Either way, the integration with your code is absolutely minimal, requiring, at most, one line of code, and no dependencies.

## Using HappyBin AutoUpdater
You can use the autoupdater as-is with no modifications, or you can customise the UI or the update workflow. The Updater class presents four public methods to download and install updates, and the MainDlg implements a typical update workflow. Change either to your liking.

## HappyBin AutoUpdater can run in three modes:

- Boot-strap: Runs ahead of primary application and downloads/installs patches. This mode requires no code changes to your primary application.
- Passive/Background: Runs after app launch and silently downloads updates; prompts for install. This is designed in the style of Firefox/Chrome updates and requires only a Process.Start call from your primary application.
- AboutBox: Runs at user discretion; prompts for install. This is designed in the style of Firefox/Chrome updates and requires only a Process.Start call from your primary application.

## Functionality

- Update files are maintained as zips, downloaded, and then unzipped to the target folder.
- Supported download protocols are: Http, Ftp, and UNC.
- HappyBin will also delete files from the target folder.

## Planned

- Release notes dialog.

