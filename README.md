# ChromeCastPlugin

The ChromeCastPlugin is a Loupedeck plugin that allows you to control your Google Chromecast with a Loupedeck device. With this plugin you can e.g, control the volume, start casting a stream available via a URL and stop a cast.

## Installation

Get the plugin package by either building it or downloading it from ...

To install the package file:

- Open the Loupedeck software
- From the account menu: **Add-on manager** > **+install from file**
- Select the package file and continue with installation

## Usage

From the Loupedeck software you can map following actions to your Loupedeck device:

- **Select Chromecast** - to connect to a Chromecast
- **Chromecast Volume** - to control the volume of the selected Chromecast
- **Play Cast** - to play online media which is available via a public URL
- **Stop Cast** - to stop an ongoing cast

## Building and debugging

### Prerequisits

- The referenced GoogleCast package requires .Net 2.0
- Get the [LoupedeckPackageTool](https://loupedeck.com/developer/) to build the plugin package file.
- Ensure the path to the package tool in the `ChromeCastPlugin.csproj` project file is correct. Currently, it is set to `C:\Program Files (x86)\Loupedeck\LoupedeckPluginTool\LoupedeckPluginTool.exe`

### Visual Studio

From Visual Studio, you can build and debug the project. When the project is build, the project output files are copied to Chromecast plugin folder in `%localappdata%\Loupedeck\Plugins\Chromecast`. Running the project will start Loupedeck and load the plugin from the Chromecast plugin folder. 

In case Loupedeck doesn't start on debug, ensure that it is configured correctly in the project settings:

- From the `ChromeCastPlugin` project settings, go to the **Debug** tab and select **Start external program** and then browse to the Loupedeck application which is normally installed at `C:\Program Files (x86)\Loupedeck\Loupedeck2\Loupedeck2.exe`

## Contributing

Pull requests are welcome. For major changes, please open an issue first
to discuss what you would like to change.

<!-- ## License

[MIT](https://choosealicense.com/licenses/mit/) -->

## More info

- [Loupedeck.com](https://www.loupedeck.com)
- Loupedeck plugin [development guide](https://github.com/Loupedeck/PluginSdk/wiki) on Github
