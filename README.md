# <img alt="logo" height="26" src="https://github.com/mob-sakai/mob-sakai/assets/12690315/05eae124-58aa-414d-9e9f-cc65022e9854"/> SoftMaskForUGUI

[![](https://img.shields.io/npm/v/com.coffee.softmask-for-ugui?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.coffee.softmask-for-ugui/)
[![](https://img.shields.io/github/v/release/mob-sakai/SoftMaskForUGUI?include_prereleases)](https://github.com/mob-sakai/SoftMaskForUGUI/releases)
[![](https://img.shields.io/github/release-date/mob-sakai/SoftMaskForUGUI.svg)](https://github.com/mob-sakai/SoftMaskForUGUI/releases)  
![](https://img.shields.io/badge/Unity-2019.4+-57b9d3.svg?style=flat&logo=unity)
[![](https://img.shields.io/github/license/mob-sakai/SoftMaskForUGUI.svg)](https://github.com/mob-sakai/SoftMaskForUGUI/blob/main/LICENSE.txt)
[![](https://mob-sakai.github.io/DocFxForUnity/CodeCoverage/badge_linecoverage.svg)](https://mob-sakai.github.io/TestPkgDev/CodeCoverage/)  
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-orange.svg)](http://makeapullrequest.com)
[![](https://img.shields.io/github/watchers/mob-sakai/SoftMaskForUGUI.svg?style=social&label=Watch)](https://github.com/mob-sakai/SoftMaskForUGUI/subscription)
[![](https://img.shields.io/twitter/follow/mob_sakai.svg?label=Follow&style=social)](https://twitter.com/intent/follow?screen_name=mob_sakai)

<< [📝 Description](#-description) | [🎮 Demo](#-demo) | [⚙ Installation](#-installation) | [🚀 Usage](#-usage) | [🤝 Contributing](#-contributing) >>

## 📝 Description

<img src="https://github.com/mob-sakai/mob-sakai/assets/12690315/3cca20d0-8565-49d1-9f5c-0a6dbc2cb2eb" width="500"/>

This package provides a soft mask for Unity UI (uGUI).

#### Features

* SoftMask is compatible with Mask.
* You can adjust the visible part.  
![](https://user-images.githubusercontent.com/12690315/48661087-01ca9f00-eab0-11e8-8878-772a1ed1fb7b.gif)
* Text, Image, RawImage can be used as a masking.
* Support multiple-sprites and SpriteAtlas.
* Support up to 4 nested soft masks.  
![](https://user-images.githubusercontent.com/12690315/48708326-a0d4cf80-ec45-11e8-83b8-f55d29138db7.png)
* Support scroll view.  
![](https://user-images.githubusercontent.com/12690315/48708527-2b1d3380-ec46-11e8-9adf-9d33498f0353.png)
* Support inversed soft mask.  
![](https://user-images.githubusercontent.com/12690315/48708328-a0d4cf80-ec45-11e8-9945-e877faabc968.png)
* Support overlay, camera space and world space.  
![](https://user-images.githubusercontent.com/12690315/48708329-a0d4cf80-ec45-11e8-8328-16b697f981ec.png)
* (Option) Raycast is filtered only for the visible part.  
![](https://user-images.githubusercontent.com/12690315/48708330-a16d6600-ec45-11e8-94bf-afecd1bd9a39.png)
* Contain soft maskable UI shader.
* Support soft masks in your custom shaders by adding just 3 lines. For details, please see [Development Note](#support-soft-masks-with-your-custom-shaders). 
* Adjust soft mask buffer size to improve performance.
* Convert existing Mask to SoftMask from context menu.  
![](https://user-images.githubusercontent.com/12690315/48659018-902e2900-ea8e-11e8-9b6e-224365cdde7f.png)
* Render the soft mask buffer only when needed to improve performance.
* Add a SoftMaskable component to the child UI elements of SoftMask from the inspector.  
![](https://user-images.githubusercontent.com/12690315/50284153-76bc3a80-049b-11e9-8c55-719af897960a.png)
* Preview soft mask buffer in inspector.  
![](https://user-images.githubusercontent.com/12690315/50284151-7459e080-049b-11e9-9cd3-24fb476766dc.png)
* Make multiple holes on one background by 'Parts of parent' option.  
![](https://user-images.githubusercontent.com/12690315/54102470-f5c26e80-440b-11e9-89d1-899aa4dca00d.png)
* [Support TextMeshPro](#support-textmeshpro)

<br><br>

## 🎮 Demo

[WebGL Demo](http://mob-sakai.github.io/SoftMaskForUGUI/Demo/)

<br><br>

## ⚙ Installation

_This package requires **Unity 2019.4 or later**._

#### Install via OpenUPM

This package is available on [OpenUPM](https://openupm.com) package registry.
This is the preferred method of installation, as you can easily receive updates as they're released.

If you have [openupm-cli](https://github.com/openupm/openupm-cli) installed, then run the following command in your project's directory:

```
openupm add com.coffee.softmask-for-ugui
```

#### Install via UPM (using Git URL)

Navigate to your project's Packages folder and open the `manifest.json` file. Then add this package somewhere in the `dependencies` block:

```json
{
  "dependencies": {
    "com.coffee.softmask-for-ugui": "https://github.com/mob-sakai/SoftMaskForUGUI.git?path=Packages/src",
    ...
  },
}
```

To update the package, change suffix `#{version}` to the target version.

* e.g. `"com.coffee.softmask-for-ugui": "https://github.com/mob-sakai/SoftMaskForUGUI.git?path=Packages/src#2.0.0",`

<br><br>

## 🚀 Usage

1. Add a `SoftMask` component instead of `Mask` component.
   Or, convert an existing `Mask` component to `SoftMask` component from the context menu.
   ![](https://user-images.githubusercontent.com/12690315/48659018-902e2900-ea8e-11e8-9b6e-224365cdde7f.png)
2. (Optional) By placing the `MaskingShape` component under `SoftMask`, you can add or remove the mask range.
   ![](https://user-images.githubusercontent.com/12690315/48661087-01ca9f00-eab0-11e8-8878-772a1ed1fb7b.gif)
3. Enjoy!

<br><br>

#### Usage with TextMeshPro

Open the `Package Manager` window and select the `UI Soft Mask` package in the package list and click the `TextMeshPro Support > Import in project` button.

The assets will be imported into `Assets/Samples/UI Soft Mask/{version}/TextMeshPro Support`.

**NOTE:** You must import [TMP Essential Resources](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/index.html#installation) before using. If the shader error is not resolved, reimport the shader.

#### Usage with your custom shaders

There are two ways to support soft masks with custom shaders.

NOTE: To support soft masks, you need to add the `(SoftMaskable)` suffix to the shader name.

1. **Hybrid (recommended)**: Add a `SoftMaskable` variant to the existing shader.
   This way requires minimal changes (add few lines).
2. **Separate**: Create a new shader with a `SoftMaskable` variant.
   Use this way for built-in shaders that cannot be edited, like `UI/Default`.

<br><br>

## 🤝 Contributing

### Issues

Issues are incredibly valuable to this project:

- Ideas provide a valuable source of contributions that others can make.
- Problems help identify areas where this project needs improvement.
- Questions indicate where contributors can enhance the user experience.

### Pull Requests

Pull requests offer a fantastic way to contribute your ideas to this repository.  
Please refer to [CONTRIBUTING.md](https://github.com/mob-sakai/SoftMaskForUGUI/tree/main/CONTRIBUTING.md)
and [develop branch](https://github.com/mob-sakai/SoftMaskForUGUI/tree/develop) for guidelines.

### Support

This is an open-source project developed during my spare time.  
If you appreciate it, consider supporting me.  
Your support allows me to dedicate more time to development. 😊

[![](https://user-images.githubusercontent.com/12690315/50731629-3b18b480-11ad-11e9-8fad-4b13f27969c1.png)](https://www.patreon.com/join/2343451?)  
[![](https://user-images.githubusercontent.com/12690315/66942881-03686280-f085-11e9-9586-fc0b6011029f.png)](https://github.com/users/mob-sakai/sponsorship)

<br><br>

## License

* MIT

## Author

* ![](https://user-images.githubusercontent.com/12690315/96986908-434a0b80-155d-11eb-8275-85138ab90afa.png) [mob-sakai](https://github.com/mob-sakai) [![](https://img.shields.io/twitter/follow/mob_sakai.svg?label=Follow&style=social)](https://twitter.com/intent/follow?screen_name=mob_sakai) ![GitHub followers](https://img.shields.io/github/followers/mob-sakai?style=social)

## See Also

* GitHub page : https://github.com/mob-sakai/SoftMaskForUGUI
* Releases : https://github.com/mob-sakai/SoftMaskForUGUI/releases
* Issue tracker : https://github.com/mob-sakai/SoftMaskForUGUI/issues
* Change log : https://github.com/mob-sakai/SoftMaskForUGUI/blob/main/CHANGELOG.md
