<p align="center">
  <picture>
    <img alt="YARG Gameplay" src="./Images/this_looks_so_bad_but_im_tired.png" width="100%">
  </picture>
</p>

<p align="center">
    <i>YARG XBOX (a.k.a. Yet Another Rhythm Game... but on Xbox)</i>
</p>

<p align="center">
  <a href="https://twitter.com/YARGGame">
    <img src="./Images/Socials/Twitter.png" width="38px" height="38px" alt="Twitter">
  </a>
  <a href="https://discord.gg/sqpu4R552r">
    <img src="./Images/Socials/Discord.png" height="38px" width="38px" alt="Discord">
  </a>
  <a href="https://reddit.com/r/yarg">
    <img src="./Images/Socials/Reddit.png" height="38px" width="38px" alt="Discord">
  </a>
</p>

---

YARG (a.k.a. Yet Another Rhythm Game) is a free, open-source, plastic guitar game that is still in development. It supports guitar (five fret), drums (plastic or e-kit), vocals, pro-guitar, and more! YARG is still in active development, so there may be bugs and missing features.

This is a fan-made fork of YARG made to run on Xbox One / Series consoles using the official Xbox Dev Mode application, with planned support for all compatible plastic instruments.

> [!WARNING]
>
> Keep in mind, this fork is NOT complete yet, and may not be developed very well. I am not a professional programmer or game developer, so do not expect a perfect result.
>
> If you *do* encounter any bugs or have any suggestions however, feel free to report them to help out the project! (See [[✍️ Contributing and Credits](#️-contributing-and-credits)] below)

## 👉 Disclaimer

> [!IMPORTANT]
> **YARG stands firmly against all forms of piracy.** We neither support nor endorse piracy, as it is a violation of copyright law with serious legal consequences. Our platform's importable content—designed for creators to share their work and for educational purposes—does not justify or excuse piracy.
>
> YARG itself **does not** use any ripped/pirated assets or music and never will. By using YARG, users agree not to promote or endorse piracy in any way through our platform. Upholding these principles ensures a community that respects copyright, creativity, and legal standards.
>
> YARG stands for "Yet Another Rhythm Game" and NOT for pirates.

## 📃 Table of Contents

- [👉 Disclaimer](#-disclaimer)
- [📃 Table of Contents](#-table-of-contents)
- [📥 Downloading and Playing](#-downloading-and-playing)
- [🔨 Building/Contributing](#-buildingcontributing)
  - [Setup Instructions](#setup-instructions)
  - [Unity YAML Merge Tool](#unity-yaml-merge-tool)
- [✍️ Contributing and Credits](#️-contributing-and-credits)
- [🛡️ License](#️-license)
- [🧰 External Licenses](#-external-licenses)
- [📦 External Assets and Libraries](#-external-assets-and-libraries)
- [💸 Donate](#-donate)

## 📥 Downloading and Playing

1. Make sure your Xbox console is in the Developer sandbox environment with an internet connection and an available account. If you haven't used Dev Mode before, there are many guides by the homebrew community such as the [Dev Mode Wiki](https://wiki.sternserv.xyz/).<br>(Note, The YARG Xbox project is not affiliated with the wiki.)

2. Download all .appx files from the latest release in the [Releases tab](https://github.com/gingerphoenix10/YARG-Xbox/Releases/Latest)

3. Log into your Xbox Device Portal by accessing the IP address shown in the Dev Home (commonly <a>h</a>ttps://192.168.x.xxx:11443). This may require you to set up a login in the Remote Access Settings on your console.

4. Under the "My games & apps" section of the device portal, click the Add button to install an app. Browse to the main YARG appx file downloaded from the [releases](https://github.com/gingerphoenix10/YARG-Xbox/Releases/Latest) page, select it in the "Deplay or Install Application" popup, and click Next.

5. When prompted for any necessary dependencies, add any other appx files downloaded along side the YARG app package. Then click the "Start" button to begin installing the game.

6. Once the installation is complete, the game should be launchable via the "Games and apps" section in the dev home, or via the regular "My games & apps" menu.

> [!IMPORTANT]
>
> If you have issues launching the game, or the game shows up under "Apps", highlight YARG in the Dev Home, press Select on your controller, Choose "View details", and make sure the App type is set to "Game".
>
> If you continue to have issues launching (common after updating), try uninstalling the game, restarting your console, and reinstalling via the Device Portal.

## 🔨 Building/Contributing

> [!IMPORTANT]
> #### ⚠️ If you wish to contribute, use the `dev` branch. Your PR will NOT be merged if it's on `master`. ⚠️

### Setup Instructions

> [!WARNING]
>
> If you would like to build the game yourself, please follow these instructions.
>
> If you don't follow these instructions, it is possible that **YOU WILL NOT BE ABLE TO RUN THE GAME**.

1. If you are planning to contribute, first make a fork of the project.
   - Be sure to *uncheck* the option to only include the `master` branch! You will not be able to set things up correctly otherwise, as the `dev` branch will not be included in your fork.

   ![](Images/Contributing/Include_All_Branches.png)

2. Make sure you have the latest version of [Blender](https://www.blender.org/) installed. This is for loading models, even if you don't plan on editing them.
3. Clone the repository. If you are not familiar with git, [GitHub Desktop](https://desktop.github.com/) or [Visual Studio Code](https://code.visualstudio.com/) plus the [GitLens extension](https://marketplace.visualstudio.com/items?itemName=eamodio.gitlens) are both good GUI options. If, however, you want to use the command-line:
   1. Download [Git](https://git-scm.com/downloads). Be sure it is added to system path.
   2. Also download [Git LFS](https://git-lfs.com), and install it with the `git lfs install` command.
   3. Open the command prompt in the directory you want to store the repository.
   4. Clone the project repository:
   5. Type in `git clone -b dev --recursive <repository url>.git`, replacing `<repository url>` with either:
      - your fork's URL if you made one, or
      - the main repository's URL (`https://github.com/YARC-Official/YARG`) if you just want to build the game.
      - A complete example using the main repository's URL is `git clone -b dev --recursive https://github.com/YARC-Official/YARG.git`.
   6. Because YARG contains submodules, you may need to do `git submodule update` when things get updated.
4. Install Unity 2021.3.36f1. Easiest method will be using Unity Hub:
   1. Download and install [Unity Hub](https://unity.com/download).
   2. Sign-in/create an account with a personal license (free).
   3. In Unity Hub, hit the arrow next to Add and select `Add project from disk`, then select the folder you cloned YARG to.
   4. Click on the added entry for YARG. It will warn you about a missing editor version, select 2021.3.36f1 and install it.
      - Unselect Visual Studio in the list of modules if you wish to use another editor or already have it installed.
5. Install the [.NET SDK](https://dotnet.microsoft.com/en-us/download/visual-studio-sdks). This is required to develop and build the submodules.
   - You will need the SDK specifically, not the runtime!
6. Open the project in Unity. When prompted about Safe Mode, click "Ignore".
   - Do *not* enter Safe Mode, otherwise scripts necessary to build/install dependencies will not run, and the errors will not resolve.

   ![](Images/Contributing/unityignore.png)

7. Click on `NuGet` on the top menu bar, then click on `Restore Packages`.
   - This should be performed automatically when Unity starts up, but it can be performend manually if needed.
8. You're ready to go!

### Linux

On certain distributions of Linux, Unity 2021.3.36f1 editor is broken and the YARG project (or any other project for that matter) cannot be imported. When trying to open the project, Unity will freeze during the import process.

   ![](Images/Contributing/Unity_Project_Import_Hang.png)

To fix this issue, follow the steps below:

1. Locate the Editor installation directory. By default, it is `${HOME}/Unity/Hub/Editor`, but it can be reconfigured in Unity Hub.
2. Enter the `2021.3.36f1/Editor/Data` directory.
3. Rename the `bee_backend` executable file to `bee_backend_real`.
4. Create a new text file named `bee_backend`.
5. Paste the following script into the `bee_backend` file:
  ```bash
  #!/bin/bash

  args=("$@")
  for ((i=0; i<"${#args[@]}"; ++i))
  do
      case ${args[i]} in
          --stdin-canary)
              unset args[i];
              break;;
      esac
  done
  ${0}_real "${args[@]}"
  ```
6. Launching the project should now work properly.

### Unity YAML Merge Tool

Sometimes merge conflicts may happen between Unity scenes. These can be much more difficult to resolve than other merge conflicts, so we recommend using the Unity YAML merge tool to resolve these instead of attempting to do so manually.

Setup:
1. Open a command prompt to the repository (on VS Code you can do Terminal > New Terminal)
2. Type in `git config --local --edit`
3. In the file that gets opened, go to the bottom and paste this in:
  ```
  [merge]
      tool = unityyamlmerge
  [mergetool "unityyamlmerge"]
      trustExitCode = false
      cmd = 'C:\\Program Files\\Unity\\Hub\\Editor\\2021.3.21f1\\Editor\\Data\\Tools\\UnityYAMLMerge.exe' merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED"
  ```
  - You may need to change the file path depending on where you installed Unity to.
4. Save and close the file.

Resolving conflicts:
1. Start the merge/cherry-pick which is causing conflicts.
2. If the conflict doesn't resolve automatically, open the command prompt and use `git merge-tool`.
3. Verify that the conflict was resolved correctly, then commit/continue the merge.

## ✍️ Contributing and Credits

If you would like to contribute to YARG Xbox specifically, feel free to leave any suggestions / bug reports in the [Issues](https://github.com/gingerphoenix10/YARG-Xbox/Issues). If you have the skills, also feel free to leave a pull request! This is a fully fan-driven project, and any form of contribution is welcome.

If you want to contribute to YARG as a whole, please feel free! It's recommended you join [the YARG Official Discord](https://discord.gg/sqpu4R552r) so the devs can provide feedback quickly.

In order to get your name added to the YARG credits, you need to first contribute to any of the following:
* YARG
* The Official Setlist and/or DLC
* YARC Launcher
* OpenSource
* The community (community moderator, socials manager, etc).

After that, you must create a pull request on [this repo](https://github.com/YARC-Official/Contributors) to get your name added. If you need help with this, feel free to ask in their Discord!

## 🛡️ License

YARG is licensed under the [GNU Lesser General Public License v3.0](https://www.gnu.org/licenses/lgpl-3.0.en.html) (or later) - see the [`LICENSE`](LICENSE) file for details.

## 🧰 External Licenses

Some libraries/assets are **packaged** with the source code have licenses that must be included.

| Library | License |
| --- | --- |
| [NuGet for Unity](https://github.com/GlitchEnzo/NuGetForUnity) | [MIT](https://github.com/GlitchEnzo/NuGetForUnity/blob/master/LICENSE)
| [Unity Standalone File Browser](https://github.com/gkngkc/UnityStandaloneFileBrowser) | [MIT](https://github.com/gkngkc/UnityStandaloneFileBrowser/blob/master/LICENSE.txt)
| [Discord GameSDK](https://discord.com/developers/docs/game-sdk/sdk-starter-guide) | Licenseless
| [Lucide](https://lucide.dev/) | [ISC](https://lucide.dev/license)
| [Unbounded](https://fonts.google.com/specimen/Unbounded), [Barlow](https://fonts.google.com/specimen/Barlow), [Noto Sans Japanese](https://fonts.google.com/noto/specimen/Noto+Sans+JP) and [Red Hat Display](https://fonts.google.com/specimen/Red+Hat+Display) | [Open Font License](https://scripts.sil.org/cms/scripts/page.php?site_id=nrsi&id=OFL)
| [PolyHaven](https://polyhaven.com/) | [CC0](https://creativecommons.org/publicdomain/zero/1.0/)
| [BASS](https://www.un4seen.com/) | [Proprietary](https://www.un4seen.com/) (free for non-commercial use)
| [Haukcode.sACN](https://github.com/HakanL/Haukcode.sACN) | [MIT](https://github.com/HakanL/Haukcode.sACN/blob/master/LICENSE) |

Please note that other libraries are **not** directly packaged within the source code, and are to be installed by NuGet, Unity's packaged manager, or via a Git submodule.

## 📦 External Assets and Libraries

These are assets that are installed by NuGet, Unity's packaged manager, or via a Git submodule. These have varying licenses, but can all be downloaded/accessed by the links given.

| Link | Type | Use |
| --- | --- | --- |
| [YARG.Core](https://github.com/YARC-Official/YARG.Core) | Library | Provides most of YARG's backend (engine, replays, etc.)
| [PlasticBand](https://github.com/TheNathannator/PlasticBand) | Reference | Controller Support Info
| [GuitarGame_ChartFormats](https://github.com/TheNathannator/GuitarGame_ChartFormats) | Reference | File Format Documentation
| [PlasticBand-Unity](https://github.com/TheNathannator/PlasticBand-Unity) | Library | GH/RB Controller Support
| [HIDrogen](https://github.com/TheNathannator/HIDrogen) | Library | Linux HID Controller Support
| [EasySharpIni](https://www.nuget.org/packages/EasySharpIni/) | Library | Parsing `song.ini` Files
| [DryWetMidi](https://www.nuget.org/packages/Melanchall.DryWetMidi) | Library | Parsing `.mid` Files
| [Minis](https://github.com/keijiro/Minis/tree/master) | Library | MIDI Input for Unity
| [DOTween](https://github.com/Demigiant/dotween) | Library | Animation Utility
| [UniTask](https://github.com/Cysharp/UniTask) | Library | Async Library
| [unity-toolbar-extender](https://github.com/marijnz/unity-toolbar-extender/) | Library | Unity Editor Utility
| [SoftMaskForUGUI](https://github.com/mob-sakai/SoftMaskForUGUI) | Library | UI Utility
| [Unity-Dependencies-Hunter](https://github.com/AlexeyPerov/Unity-Dependencies-Hunter) | Library | Unity Editor Utility
| [tmpro-dynamic-data-cleaner](https://github.com/STARasGAMES/tmpro-dynamic-data-cleaner) | Library | Prevent Git Change Spam

## 💸 Donate

Some people have expressed interest in donating. This is an open-source project and therefore donating is not required. If you do want to still help out, spread the word or contribute!
