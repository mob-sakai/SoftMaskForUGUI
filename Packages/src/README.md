# <img alt="logo" height="26" src="https://github.com/mob-sakai/mob-sakai/assets/12690315/05eae124-58aa-414d-9e9f-cc65022e9854"/> SoftMaskForUGUI v2 <!-- omit in toc -->

[![](https://img.shields.io/npm/v/com.coffee.softmask-for-ugui?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.coffee.softmask-for-ugui/)
[![](https://img.shields.io/github/v/release/mob-sakai/SoftMaskForUGUI?include_prereleases)](https://github.com/mob-sakai/SoftMaskForUGUI/releases)
[![](https://img.shields.io/github/release-date/mob-sakai/SoftMaskForUGUI.svg)](https://github.com/mob-sakai/SoftMaskForUGUI/releases)  
![](https://img.shields.io/badge/Unity-2019.4+-57b9d3.svg?style=flat&logo=unity)
![](https://img.shields.io/badge/uGUI_2.0_Ready-57b9d3.svg?style=flat)
![](https://img.shields.io/badge/UPR%2FHDPR_Ready-57b9d3.svg?style=flat)
![](https://img.shields.io/badge/VR_Ready-57b9d3.svg?style=flat)  
[![](https://img.shields.io/github/license/mob-sakai/SoftMaskForUGUI.svg)](https://github.com/mob-sakai/SoftMaskForUGUI/blob/main/LICENSE.txt)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-orange.svg)](http://makeapullrequest.com)
[![](https://img.shields.io/github/watchers/mob-sakai/SoftMaskForUGUI.svg?style=social&label=Watch)](https://github.com/mob-sakai/SoftMaskForUGUI/subscription)
[![](https://img.shields.io/twitter/follow/mob_sakai.svg?label=Follow&style=social)](https://twitter.com/intent/follow?screen_name=mob_sakai)

<< [📝 Description](#-description-) | [📌 Key Features](#-key-features) | [🎮 Demo](#-demo) | [⚙ Installation](#-installation) | [🚀 Usage](#-usage) | [🤝 Contributing](#-contributing) >>

## 📝 Description <!-- omit in toc -->

Enhance Unity UI (uGUI) with advanced soft-masking features to create more visually appealing effects!  

![](https://github.com/user-attachments/assets/51f6d528-cb36-429c-afe7-e021d807fb96)

- [📌 Key Features](#-key-features)
- [🎮 Demo](#-demo)
- [⚙ Installation](#-installation)
  - [Install via OpenUPM](#install-via-openupm)
  - [Install via UPM (with Package Manager UI)](#install-via-upm-with-package-manager-ui)
  - [Install via UPM (Manually)](#install-via-upm-manually)
  - [Install as Embedded Package](#install-as-embedded-package)
- [🔄 Upgrading from v1 to v2](#-upgrading-from-v1-to-v2)
- [🚀 Usage](#-usage)
  - [Getting Started](#getting-started)
  - [Comparison of Masking Mode](#comparison-of-masking-mode)
  - [RectMask2D vs SoftMask](#rectmask2d-vs-softmask)
  - [Component: SoftMask](#component-softmask)
  - [Component: MaskingShape](#component-maskingshape)
  - [Component: RectTransformFitter](#component-recttransformfitter)
  - [Project Settings](#project-settings)
  - [Usage with Scripts](#usage-with-scripts)
  - [Usage with TextMeshPro](#usage-with-textmeshpro)
  - [Usage with Your Custom Shaders](#usage-with-your-custom-shaders)
  - [:warning: Limitations](#warning-limitations)
- [🤝 Contributing](#-contributing)
  - [Issues](#issues)
  - [Pull Requests](#pull-requests)
  - [Support](#support)
- [License](#license)
- [Author](#author)
- [See Also](#see-also)

<br><br>

## 📌 Key Features

- **Compatibility with Mask**: SoftMask is fully compatible with the existing `Mask` component.
  You can convert an existing `Mask` to `SoftMask` from the context menu.  
  ![](https://github.com/user-attachments/assets/cf1d4545-d7a2-4b88-96d4-82f9a149cf69)
- **Adjustable Visible Part**: You can freely adjust the visible part of the mask.  
  ![](https://github.com/user-attachments/assets/171bb9e9-5392-4c60-9e61-e76a7576b421)
- **Versatile Masking Options**: `Text`, `Image`, `RawImage` can be used as a masking graphic.
- **Support for Multiple Sprites and SpriteAtlas**: `SoftMask` supports multiple sprites and `SpriteAtlas`.
- **Nested Soft Masks**: `SoftMask` supports up to 4 nested soft masks.  
  ![](https://github.com/user-attachments/assets/c3a6ab32-9d1a-4eff-9747-2df9ab28eee4)
- **ScrollRect Support**: `SoftMask` supports `ScrollRect` component.
- **All Render Mode Support**: `SoftMask` supports overlay, camera space, and world space.
- **Soft-Maskable UI Shader Included**: The package includes a soft-maskable UI shader for `UI/Dafault`.
- **Custom Shader Support**: You can make your custom shaders soft-maskable with little modification. For details, please see [soft-maskable shader](#usage-with-your-custom-shaders).
- **Performance/Quality Adjustment**: You can adjust the soft mask buffer size to improve performance or quality.  
  ![](https://github.com/user-attachments/assets/66440b45-1777-4ed9-8706-b407616865f5)
- **Efficient Rendering**: The soft mask buffer will be updated only when needed to improve performance.
- **SoftMaskable Component**: `SoftMaskable` component will be added automatically at runtime as needed.
- **Soft Mask Buffer Preview**: You can preview the soft mask buffer in the inspector.  
  ![](https://github.com/user-attachments/assets/4ffaf563-a616-43e2-8638-3c8bdead51fa)
- **Anti-Alias Masking Mode**: If you don't need semi-transparent masks, you can use the more performant "Anti-Aliasing Masking Mode".  
  ![](https://github.com/user-attachments/assets/490fd9d8-aa0d-45e2-a094-311236850ca2)
- **Masking Shape**: You can add or remove mask region using `MaskingShape` component.  
  ![](https://github.com/user-attachments/assets/4a568aba-99f6-46c5-98d9-30eb673c9026)
- **Inverse Masking**: Use `MaskingShape` component to inverse masking. You can implement effects such as iris out.  
  ![](https://github.com/user-attachments/assets/fb0581c0-84a7-4c12-a1d1-af28ed0da9b4)
- **Ray-cast Filtering**: Ray-casts are filtered only for the visible part.
  This feature is useful for preventing clicks on masked parts during tutorials.  
  ![](https://github.com/user-attachments/assets/430735c7-7b85-46e8-bbf4-ab1fe70aa19a)
- **Stereo Support**: Soft masking for VR.  
  ![](https://github.com/user-attachments/assets/8ae038cd-b8da-4b83-ac48-15083fb2e3a6)
- **TextMeshProUGUI Support**: Support `TextMeshProUGUI` by importing additional shaders.  
  It also supports TextMeshPro v3.2/4.0 (pre-release) and uGUI 2.0 (Unity 2023.2+/6.0+).
  For details, please see [Support TextMeshPro](#usage-with-textmeshpro).  
  ![](https://github.com/user-attachments/assets/6e33fce4-526c-41af-9894-36da1ccb2f51)
- **Better Editor Experience**: In the Scene view, objects outside the screen are displayed as stencil masks, allowing for more intuitive editing.  
  ![](https://github.com/user-attachments/assets/e8349fe7-b3a8-471f-a5a7-1ee00c431561)
- **Automatic Shader Include and Stripping**: SoftMaskable shaders are automatically included at build time, and unused shader variants are removed.  
  ![](https://github.com/user-attachments/assets/0671b44e-6e0c-41d4-a92a-c3c56b8e8611)

<br><br>

## 🎮 Demo

[WebGL Demo](https://mob-sakai.github.io/demos/SoftMaskForUGUI/)

<br><br>

## ⚙ Installation

_This package requires **Unity 2019.4 or later**._

### Install via OpenUPM

- This package is available on [OpenUPM](https://openupm.com) package registry.
- This is the preferred method of installation, as you can easily receive updates as they're released.
- If you have [openupm-cli](https://github.com/openupm/openupm-cli) installed, then run the following command in your project's directory:
  ```
  openupm add com.coffee.softmask-for-ugui
  ```
- To update the package, use Package Manager UI (`Window > Package Manager`) or run the following command with `@{version}`:
  ```
  openupm add com.coffee.softmask-for-ugui@2.0.0
  ```

### Install via UPM (with Package Manager UI)

- Click `Window > Package Manager` to open Package Manager UI.
- Click `+ > Add package from git URL...` and input the repository URL: `https://github.com/mob-sakai/SoftMaskForUGUI.git`  
  ![](https://github.com/user-attachments/assets/f88f47ad-c606-44bd-9e86-ee3f72eac548)
- To update the package, change suffix `#{version}` to the target version.
    - e.g. `https://github.com/mob-sakai/SoftMaskForUGUI.git#2.0.0`

### Install via UPM (Manually)

- Open the `Packages/manifest.json` file in your project. Then add this package somewhere in the `dependencies` block:
  ```json
  {
    "dependencies": {
      "com.coffee.softmask-for-ugui": "https://github.com/mob-sakai/SoftMaskForUGUI.git",
      ...
    }
  }
  ```

- To update the package, change suffix `#{version}` to the target version.
  - e.g. `"com.coffee.softmask-for-ugui": "https://github.com/mob-sakai/SoftMaskForUGUI.git#2.0.0",`

### Install as Embedded Package

1. Download a source code zip file from [Releases](https://github.com/mob-sakai/SoftMaskForUGUI/releases) and extract it.
2. Place it in your project's `Packages` directory.  
 ![](https://github.com/user-attachments/assets/af639cfa-d0b4-4370-acb9-3fe4db451f47)
- If you want to fix bugs or add features, install it as an embedded package.
- To update the package, you need to re-download it and replace the contents.

<br><br>

## 🔄 Upgrading from v1 to v2

If you are currently using SoftMaskForUGUI v1.x, the following breaking changes are included when upgrading to v2:

1. **API changes**: Some APIs are obsolete.
    - `SoftMask.softness`: Use `SoftMask.softnessRange` instead.
    - `SoftMask.partOfParent`: Use `MaskingShape` component instead.

2. **`SoftMaskable` component**: `SoftMaskable` component is no longer required to be added explicitly.
   It will be added automatically at runtime as needed.

3. **`SoftMaskable` shader**: `SoftMask()` function has been updated with additional arguments.
   ```shaderlab
   // Before
   color.a *= SoftMask(IN.vertex, IN.worldPosition);
   // After
   color.a *= SoftMask(IN.vertex, IN.worldPosition, color.a);
   ```

<br>

To apply these changes automatically, please follow the steps below:

1. Click `Edit > Project Settings` to open the Project Settings window and select `UI > SoftMask` category.

2. Click on "Upgrade All Assets V1 to V2" to modify the assets.  
   ![](https://github.com/user-attachments/assets/54415f0b-e952-4271-ac6c-545f675030e3)

- ⚠️ If you select "Dry Run", you can check the changes before upgrading.  
  ![](https://github.com/user-attachments/assets/9e5a4d2a-e072-4074-8be4-e6dd48a537bc)  
  ![](https://github.com/user-attachments/assets/02d0add8-005b-4fe3-8e9a-480221feb503)

<br><br>

## 🚀 Usage

### Getting Started

1. [Install the package](#-installation).

2. Add a `SoftMask` component instead of `Mask` component.  
   Or, convert an existing `Mask` component to `SoftMask` component from the context menu (`Convert To SoftMask`).  
   ![](https://github.com/user-attachments/assets/80a8d642-2ba4-4845-8d73-e9844122811e)

3. Adjust the soft mask parameters in the inspector.  
   ![](https://github.com/user-attachments/assets/171bb9e9-5392-4c60-9e61-e76a7576b421)

4. (Optional) By placing the `MaskingShape` component under `SoftMask`, you can add or remove the masking region.  
   ![](https://github.com/user-attachments/assets/711313be-29f5-45af-846a-49cdc3094e59)

5. Enjoy!

<br><br>

### Comparison of Masking Mode

![](https://github.com/user-attachments/assets/5215eb21-3c1c-47f3-bd3d-accc6538dd23)

- **Soft Masking**: Smooth mask with semi-transparency.  
  Requires memory for `RenderTexture` and [soft-maskable shader](#usage-with-your-custom-shaders).
- **Anti-Aliasing**: Less jagged stencil mask.  
  It does not require `RenderTexture` or soft-maskable shader, and works faster.
- **Normal**: Same as `Mask` component's stencil mask.

<br><br>

### RectMask2D vs SoftMask

`RectMask2D` is a built-in component that supports soft masking.  
`SoftMask` provides more advanced soft masking.

![](https://github.com/user-attachments/assets/1651030f-6750-415b-8ae1-aa3323637ce1)

- **RectMask2D**:
  - uGUI built-in
  - Very fast
  - Supports only rectangular shapes
  - Supports only one level of nesting
  - Supports only limited rotation
- **SoftMask**:
  - Advanced soft masking
  - Supports any graphic shape
  - Supports up to 4 levels of nesting soft-masking
  - Supports fully rotation

<br><br>

### Component: SoftMask

![](https://github.com/user-attachments/assets/171bb9e9-5392-4c60-9e61-e76a7576b421)

- **Masking Mode**:
  - `SoftMasking`: 
    - Smooth mask with semi-transparency.
    - Requires memory for `RenderTexture` and [soft-maskable shader](#usage-with-your-custom-shaders).
    - Use RenderTexture as a soft mask buffer.
    - The alpha of the masking graphic can be used.
  - `AntiAliasing`:
    - Less jagged stencil mask.
    - It does not require `RenderTexture` or soft-maskable shader, and works faster.
    - Suppress the jaggedness of the masking graphic.
    - The masking graphic cannot be displayed.
  - `Normal`: Same as Mask component's stencil mask.  
    ![](https://github.com/user-attachments/assets/5215eb21-3c1c-47f3-bd3d-accc6538dd23)
- **Show Mask Graphic**:
  - Show the graphic that is associated with the Mask render area.
- **Alpha Hit Test**:
  - The transparent part of the mask cannot be clicked.
  - This can be achieved by enabling Read/Write enabled in the Texture Import Settings for the texture.
  - ⚠️ Enable this only if necessary, as it will require more graphics memory and processing time.
- **Anti Aliasing Threshold** (AntiAliasing only):
    - Threshold for anti-alias masking.
    - The smaller this value, the less jagged it is.
- **Softness Range** (SoftMasking only):
  - The minimum and maximum alpha values used for soft masking.
  - The larger the gap between these values, the stronger the softness effect.
- **Down Sampling Rate** (SoftMasking only):
    - The down sampling rate for soft mask buffer.
    - The higher this value, the lower the quality of the soft masking, but the performance will improve.
- **Preview Soft Mask Buffer** (editor, SoftMasking only):
  - Preview the soft mask buffer in the inspector.

<br><br>

### Component: MaskingShape

![](https://github.com/user-attachments/assets/4a568aba-99f6-46c5-98d9-30eb673c9026)

- `MaskingShape` component allows you to add or remove the masking region.
- Placing `MaskingShape` component (with any `Graphic`) under `SoftMask` component.  
  ![](https://github.com/user-attachments/assets/8325d190-0102-4677-9687-5c9bad3f9398)
- You can use it not only with `SoftMask` component but also with `Mask` component.
- If the `MaskingMode` is `AntiAliasing` or `Normal`, or if you are using the `Mask` component, the `MaskingShape` component must be placed above the masked `Graphic` in the hierarchy. This is a limitation based on the stencil mask.
- The available features depend on the `Masking Mode`.  
  ![](https://github.com/user-attachments/assets/f9ba1d8a-594b-4488-acf3-bed8bad5eacc)  
  ![](https://github.com/user-attachments/assets/c120cbed-4ba9-4813-9a6f-59ad6c581856)  
  ![](https://github.com/user-attachments/assets/b944f863-e070-4069-a54b-409484e43a5a)


<br><br>

### Component: RectTransformFitter

![](https://github.com/user-attachments/assets/98878d4f-91a5-48dc-b44c-6a2192f0c0fc)

- `RectTransformFitter` component follows the target RectTransform.
- You can specify the properties to follow (position, rotation, scale, delta size) with `RectTransformFitter.targetProperties`.
- By combining it with the `MaskingShape` component, you can implement an effect that displays only the buttons during the tutorial.

<br><br>

### Project Settings

![](https://github.com/user-attachments/assets/9013169b-516a-4502-b069-86439a32b712)

- Click `Edit > Project Settings` to open the Project Settings window and then select `UI > SoftMask` category.
- By default, the soft-maskable shaders will be included in the build, but you can remove it if you don't need it.

<br><br>

### Usage with Scripts

```csharp
var softMask = gameObject.GetComponent<SoftMask>();
softMask.maskingMode = SoftMask.MaskingMode.SoftMasking;
softMask.downSamplingRate = SoftMask.DownSamplingRate.x2;
softMask.softnessRange = new MinMax01(0.5f, 0.75f);
```

<br><br>

### Usage with TextMeshPro

1. First, you must import [TMP Essential Resources](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/index.html#installation) before using.  
   ![](https://github.com/user-attachments/assets/aaaf6b5f-02a5-458c-a46a-162534d8e8c2)

2. Open the `Package Manager` window and select the `UI Soft Mask` package in the package list and click the `TextMeshPro Support > Import` button.  
   ⚠️ If you are using `ugui 2.0 (=Unity 2023.2+/6.0+)` or `TextMeshPro 3.2+/4.0+`, click the `TextMeshPro Support (ugui 2.0) > Import` button instead.  
   ![](https://github.com/user-attachments/assets/6182396f-65db-489e-a5ea-e5e2d5b693a9)

3. The assets will be imported under `Assets/Samples/UI Soft Mask/{version}`.  
   ![](https://github.com/user-attachments/assets/834be6c2-1cb8-4e6c-917b-d62c596ed68b)

<br><br>

### Usage with Your Custom Shaders

Here, let's make [UI/Additive](https://raw.githubusercontent.com/mob-sakai/SoftMaskForUGUI/develop/Assets/Demos/SoftMaskable%20Shader/UI-Additive.shader) custom shader soft-maskable. There are two ways to support SoftMask with custom shaders.

- **Hybrid (recommended)**: Add soft-maskable variants to the existing shader.  
  Modify the shader as follows:
    ```shaderlab
    // Add the ` (SoftMaskable)` suffix to the shader name.
    Shader "UI/Additive (SoftMaskable)"
    
    // Import "UISoftMask.cginc" and add shader variants for editor.
    #include "Packages/com.coffee.softmask-for-ugui/Shaders/SoftMask.cginc"
    #pragma multi_compile_local _ SOFTMASK_EDITOR // soft-maskable for editor
    
    // "SoftMask" function returns [0-1]. Multiply this by the final output.
    color.a *= SoftMask(IN.vertex, IN.worldPosition, color.a);
    ```
    - Result: [UI/Additive (SoftMaskable)](https://raw.githubusercontent.com/mob-sakai/SoftMaskForUGUI/develop/Assets/Demos/SoftMaskable%20Shader/UI-Additive-SoftMaskable.shader)
- **Separate**: Create a new shader with soft-maskable variants.  
  Use this way for built-in shaders that cannot be edited, like `UI/Default`.  
  Modify the shader as follows:
    ```shaderlab
    // Add the `Hidden/` prefix and ` (SoftMaskable)` suffix to the shader name.
    Shader "Hidden/UI/Additive (SoftMaskable)"
    
    // Import "UISoftMask.cginc" and add shader variants for editor.
    #include "Packages/com.coffee.softmask-for-ugui/Shaders/SoftMask.cginc"
    #pragma multi_compile_local _ SOFTMASK_EDITOR
    
    // "SoftMask" function returns [0-1]. Multiply this by the final output.
    color.a *= SoftMask(IN.vertex, IN.worldPosition, color.a);
    ```
    - Result: [Hidden/UI/Additive (SoftMaskable)](https://raw.githubusercontent.com/mob-sakai/SoftMaskForUGUI/develop/Assets/Demos/SoftMaskable%20Shader/Hidden-UI-Additive-SoftMaskable.shader)

  ![](https://github.com/user-attachments/assets/e7856aee-89f2-435e-8613-1b7ff706916f)

<br><br>

### :warning: Limitations

The following are the limitations of SoftMaskForUGUI.

- (Android) `RGB ETC1 (+ Split alpha channel)` texture format is not supported.
  - Use a format that supports alpha, such as `RGBA ETC2`.
  - Technically possible, but not supported because [ETC2 support rate is over 95%](https://developer.android.com/guide/playcore/asset-delivery/texture-compression).

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
