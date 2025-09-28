# <img alt="logo" height="26" src="https://github.com/mob-sakai/mob-sakai/assets/12690315/05eae124-58aa-414d-9e9f-cc65022e9854"/> SoftMaskForUGUI v3 <!-- omit in toc -->

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
    - [Import Additional Resources](#import-additional-resources)
- [🔄 Upgrade All Assets For V3](#-upgrade-all-assets-for-v3)
- [🚀 Usage](#-usage)
    - [Getting Started](#getting-started)
    - [RectMask2D vs SoftMask](#rectmask2d-vs-softmask)
    - [Comparison of Masking Mode](#comparison-of-masking-mode)
    - [Component: SoftMask](#component-softmask)
    - [Component: SoftMaskable](#component-softmaskable)
    - [Component: MaskingShape](#component-maskingshape)
    - [Component: RectTransformFitter](#component-recttransformfitter)
    - [Project Settings](#project-settings)
    - [Usage with Scripts](#usage-with-scripts)
    - [Usage with TextMeshPro or Spine](#usage-with-textmeshpro-or-spine)
    - [Usage with Your Custom Shaders](#usage-with-your-custom-shaders)
    - [Usage with Canvas ShaderGraph](#usage-with-canvas-shadergraph)
    - [Usage with UIEffect](#usage-with-uieffect)
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
  ![](https://github.com/user-attachments/assets/d733597e-aa27-4d65-a6ba-f2246be88129)
- **Versatile Masking Options**: `Text`, `Image`, `RawImage` can be used as a masking graphic.
- **Support for Multiple Sprites and SpriteAtlas**: `SoftMask` supports multiple sprites and `SpriteAtlas`.
- **Nested Soft Masks**: `SoftMask` supports up to 4 nested soft masks.  
  ![](https://github.com/user-attachments/assets/c3a6ab32-9d1a-4eff-9747-2df9ab28eee4)
- **ScrollRect Support**: `SoftMask` supports `ScrollRect` component.
- **All Render Mode Support**: `SoftMask` supports overlay, camera space, and world space.
- **Soft-Maskable UI Shader Included**: The package includes a soft-maskable UI shader for `UI/Dafault`.
- **Custom Shader Support**: You can make your custom shaders soft-maskable with little modification. For details,
  please see [soft-maskable shader](#usage-with-your-custom-shaders).
- **Performance/Quality Adjustment**: You can adjust the soft mask buffer size to improve performance or quality.  
  ![](https://github.com/user-attachments/assets/66440b45-1777-4ed9-8706-b407616865f5)
- **Efficient Rendering**: The soft mask buffer will be updated only when needed to improve performance.
- **SoftMaskable Component**: `SoftMaskable` component will be added automatically at runtime as needed.
- **Soft Mask Buffer Preview**: You can preview the soft mask buffer in the inspector.  
  ![](https://github.com/user-attachments/assets/4ffaf563-a616-43e2-8638-3c8bdead51fa)
- **Anti-Alias Masking Mode**: If you don't need semi-transparent masks, you can use the more performant "Anti-Aliasing
  Masking Mode".  
  ![](https://github.com/user-attachments/assets/490fd9d8-aa0d-45e2-a094-311236850ca2)
- **Masking Shape**: You can add or remove mask region using `MaskingShape` component.  
  ![](https://github.com/user-attachments/assets/ad4db415-7457-4606-ac30-e8b6342e51d2)
- **Inverse Masking**: Use `MaskingShape` component to inverse masking. You can implement effects such as iris out.  
  ![](https://github.com/user-attachments/assets/fb0581c0-84a7-4c12-a1d1-af28ed0da9b4)
- **Ray-cast Filtering**: Ray-casts are filtered only for the visible part.
  This feature is useful for preventing clicks on masked parts during tutorials.  
  ![](https://github.com/user-attachments/assets/430735c7-7b85-46e8-bbf4-ab1fe70aa19a)
- **Stereo Support**: Soft masking for VR.  
  ![](https://github.com/user-attachments/assets/8ae038cd-b8da-4b83-ac48-15083fb2e3a6)
- **TextMeshProUGUI Support**: Support `TextMeshProUGUI` by importing additional shaders.  
  It also supports TextMeshPro v3.2/4.0 (pre-release) and uGUI 2.0 (Unity 2023.2+/6.0+).
  For details, please see [Support TextMeshPro or Spine](#usage-with-textmeshpro-or-spine).  
  ![](https://github.com/user-attachments/assets/6e33fce4-526c-41af-9894-36da1ccb2f51)
- **Better Editor Experience**: In the Scene view, objects outside the screen are displayed as stencil masks, allowing
  for more intuitive editing.  
  ![](https://github.com/user-attachments/assets/e8349fe7-b3a8-471f-a5a7-1ee00c431561)
- **Soft Maskable Shader Variant Stripping**: SoftMaskable shaders are automatically included at build time. You can
  remove unused shader variants.  
  ![](https://github.com/user-attachments/assets/e413a5a1-424e-4edb-9e5e-8639c0f3a967)
- **Spine Support**: Support [Spine (SkeletonGraphic)](https://esotericsoftware.com/spine-in-depth) by importing
  additional shaders.  
  For details, please see [Support TextMeshPro or Spine](#usage-with-textmeshpro-or-spine).  
  ![](https://github.com/user-attachments/assets/37f54634-0b52-4ba3-a322-22e0f45f60ee)

<br><br>

## 🎮 Demo

[WebGL Demo](https://mob-sakai.github.io/SoftMaskForUGUI/)

<br><br>

## ⚙ Installation

_This package requires **Unity 2019.4 or later**._

### Install via OpenUPM

- This package is available on [OpenUPM](https://openupm.com) package registry.
- This is the preferred method of installation, as you can easily receive updates as they're released.
- If you have [openupm-cli](https://github.com/openupm/openupm-cli) installed, then run the following command in your
  project's directory:
  ```
  openupm add com.coffee.softmask-for-ugui
  ```
- To update the package, use Package Manager UI (`Window > Package Manager`) or run the following command with
  `@{version}`:
  ```
  openupm add com.coffee.softmask-for-ugui@3.2.0
  ```

### Install via UPM (with Package Manager UI)

- Click `Window > Package Manager` to open Package Manager UI.
- Click `+ > Add package from git URL...` and input the repository URL:
  `https://github.com/mob-sakai/SoftMaskForUGUI.git?path=Packages/src`  
  ![](https://github.com/user-attachments/assets/f88f47ad-c606-44bd-9e86-ee3f72eac548)
- To update the package, change suffix `#{version}` to the target version.
    - e.g. `https://github.com/mob-sakai/SoftMaskForUGUI.git?path=Packages/src#3.2.0`

### Install via UPM (Manually)

- Open the `Packages/manifest.json` file in your project. Then add this package somewhere in the `dependencies` block:
  ```json
  {
    "dependencies": {
      "com.coffee.softmask-for-ugui": "https://github.com/mob-sakai/SoftMaskForUGUI.git?path=Packages/src",
      ...
    }
  }
  ```

- To update the package, change suffix `#{version}` to the target version.
    - e.g. `"com.coffee.softmask-for-ugui": "https://github.com/mob-sakai/SoftMaskForUGUI.git?path=Packages/src#3.2.0",`

### Install as Embedded Package

1. Download the `Source code (zip)` file from [Releases](https://github.com/mob-sakai/SoftMaskForUGUI/releases) and
   extract it.
2. Move the `<extracted_dir>/Packages/src` directory into your project's `Packages` directory.  
   ![](https://github.com/user-attachments/assets/187cbcbe-5922-4ed5-acec-cf19aa17d208)
    - You can rename the `src` directory if needed.
    - If you intend to fix bugs or add features, installing it as an embedded package is recommended.
    - To update the package, re-download it and replace the existing contents.

### Import Additional Resources

Additional resources can be imported to extend functionality.

- [Usage with TextMeshPro or Spine](#usage-with-textmeshpro-or-spine).
- [Usage with Canvas ShaderGraph](#usage-with-canvas-shadergraph).

<br><br>

## 🔄 Upgrade All Assets For V3

If you are currently using SoftMaskForUGUI v1.x/v2.x, the following breaking changes are included when upgrading to v3:

1. **(From v1) API changes**: Some APIs are obsolete.
    - `SoftMask.alpha`: Use `Graphic.color.a` instead.
    - `SoftMask.softness`: Use `SoftMask.softnessRange` instead.
    - `SoftMask.partOfParent`: Use `MaskingShape` component instead.
    - `SoftMask.ignoreParent`: Removed.
    - `SoftMask.ignoreSelfGraphic`: Removed.
    - `SoftMask.ignoreSelfStencil`: Removed.
    - `SoftMaskable.useStencil`: Removed.
    - `SoftMaskable.raycastFilter`: Use `SoftMask.alphaHitTest` instead.
    - `SoftMaskable.maskInteraction`: If you want to use as inverse mask, use `MaskingShape` component and
      `MaskingMethod=Subtract`.

2. **(From v1) `SoftMaskable` component**: `SoftMaskable` component is no longer required to be added explicitly.
    - It will be added automatically at runtime as needed.

3. **(From v1) `SoftMaskable` shader**: `SoftMask()` function has been updated with additional arguments.
   ```shaderlab
   // Before
   color.a *= SoftMask(IN.vertex, IN.worldPosition);
   // After
   color.a *= SoftMask(IN.vertex, IN.worldPosition, color.a);
   ```

4. **(From v2) `SoftMaskable` shader**: `SOFTMASKABLE` shader feature is now required.
   ```shaderlab
   #pragma shader_feature_local _ SOFTMASKABLE
   ```

5. If you are installing via git URL, add `?path=Packages/src`.
   ```json
   // v1/v2
   "com.coffee.softmask-for-ugui": "https://github.com/mob-sakai/SoftMaskForUGUI.git",
   
   // v3
   "com.coffee.softmask-for-ugui": "https://github.com/mob-sakai/SoftMaskForUGUI.git?path=Packages/src",
   ```

6. `Hidden/UI/SoftMask`, `Hidden/UI/TerminalMaskingShape` and `SoftMaskable` shader variants used at runtime must be
   registered in the [Project Settings](#project-settings).
    - In v2, the SoftMaskable shaders were included in the "Always Included Shaders", but in v3, they must be registered
      automatically or manually.
    - You can strip unnecessary shader variants to reduce build time and file size.
    - If `SoftMask` does not display correctly in the player, open/play the scenes or prefabs in the editor. The shader
      variants will be automatically registered.

<br>

To apply these changes automatically, please follow the steps below:

1. Click `Edit > Project Settings` to open the Project Settings window and select `UI > SoftMask` category.

2. Click `Upgrade All Assets For V3` to upgrade the assets.

- ⚠️ If you select `Dry Run`, you can check the changes before upgrading.  
  ![](https://github.com/user-attachments/assets/9e5a4d2a-e072-4074-8be4-e6dd48a537bc)

<br><br>

## 🚀 Usage

### Getting Started

1. [Install the package](#-installation).

2. Add a `SoftMask` component instead of `Mask` component.  
   Or, convert an existing `Mask` component to `SoftMask` component from the context menu (`Convert To SoftMask`).  
   ![](https://github.com/user-attachments/assets/80a8d642-2ba4-4845-8d73-e9844122811e)

3. Adjust the soft mask parameters in the inspector.  
   ![](https://github.com/user-attachments/assets/d733597e-aa27-4d65-a6ba-f2246be88129)

4. (Optional) By placing the `MaskingShape` component under `SoftMask`, you can add or remove the masking region.  
   ![](https://github.com/user-attachments/assets/711313be-29f5-45af-846a-49cdc3094e59)

5. Enjoy!

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

### Comparison of Masking Mode

![](https://github.com/user-attachments/assets/5215eb21-3c1c-47f3-bd3d-accc6538dd23)

- **Soft Masking**: Smooth mask with semi-transparency.  
  Requires memory for `RenderTexture` and [soft-maskable shader](#usage-with-your-custom-shaders).
- **Anti-Aliasing**: Less jagged stencil mask.  
  It does not require `RenderTexture` or soft-maskable shader, and works faster.
- **Normal**: Same as `Mask` component's stencil mask.

<br><br>

### Component: SoftMask

![](https://github.com/user-attachments/assets/d733597e-aa27-4d65-a6ba-f2246be88129)

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
- **Show Mask Graphic** (SoftMasking or Normal): Show the graphic that is associated with the Mask render area.
- **Alpha Hit Test**: The transparent part of the mask cannot be clicked.
    - Alpha hit test is not supported when the texture is in crunch format or non-readable.
    - ⚠️ Enable this only if necessary, as it will require more graphics memory and processing time.
- **Anti Aliasing Threshold** (AntiAliasing only): Threshold for anti-alias masking. The smaller this value, the less
  jagged it is.
- **Softness Range** (SoftMasking only): The minimum and maximum alpha values used for soft masking. The larger the gap
  between these values, the stronger the softness effect.
- **Down Sampling Rate** (SoftMasking only): The down sampling rate for soft mask buffer. The higher this value, the
  lower the quality of the soft masking, but the performance will improve.
- **Preview Soft Mask Buffer** (in editor, SoftMasking only): Preview the soft mask buffer in the inspector.

<br><br>

### Component: SoftMaskable

![](https://github.com/user-attachments/assets/3af167bf-23c0-42a1-a6b5-239fc25c2364)

- **Ignore Self**: The graphic is ignored when soft-masking.
- **Ignore Children**: The child graphics are ignored when soft-masking.
- **Power**: Soft masking power.
    - The higher this value, the faster it becomes transparent.
    - If overlapping objects appear see-through, please adjust this value.  
      ![](https://github.com/user-attachments/assets/435c94ee-d42a-4b74-9411-cd11db0a5b2f)

`SoftMaskable` components are added automatically to GameObjects under a `SoftMask` component.  
If the properties are set to their default values, it is marked as `HideFlag.DontSave` and will not be saved in the
scene or prefab.

<br><br>

### Component: MaskingShape

![](https://github.com/user-attachments/assets/ad4db415-7457-4606-ac30-e8b6342e51d2)

- **Masking Method**: `Additive`, `Subtract`
- **Show Mask Graphic** (SoftMasking or Normal): Show the graphic that is associated with the Mask render area.
- **Alpha Hit Test**: The transparent part of the mask cannot be clicked.
    - Alpha hit test is not supported when the texture is in crunch format or non-readable.
    - ⚠️ Enable this only if necessary, as it will require more graphics memory and processing time.
- **Softness Range** (SoftMasking only): Override the softness range of the parent `SoftMask` component.
- **Anti Aliasing Threshold** (AntiAliasing only): Override the anti-alias threshold of the parent `SoftMask` component.

`MaskingShape` component allows you to add or remove the masking region.  
Placing `MaskingShape` component (with any `Graphic`) under `SoftMask` component.  
![](https://github.com/user-attachments/assets/8325d190-0102-4677-9687-5c9bad3f9398)

You can use it not only with `SoftMask` component but also with `Mask` component.  
If the `MaskingMode` is `AntiAliasing` or `Normal`, or if you are using the `Mask` component, the `MaskingShape`
component must be placed above the masked `Graphic` in the hierarchy. This is a limitation based on the stencil mask.  
The available features depend on the `Masking Mode`.

<br><br>

### Component: RectTransformFitter

![](https://github.com/user-attachments/assets/faaeef91-7743-4d5b-8920-2ddffc9f2a7a)

- **Target**: The target RectTransform to follow.
- **Target Properties**: `Position (X/Y/Z)`, `Rotation (X/Y/Z)`, `Scale (X/Y/Z)`, `Delta Size (X/Y)`.

`RectTransformFitter` component follows the target RectTransform.  
You can specify the properties to follow (position, rotation, scale, delta size) with
`RectTransformFitter.targetProperties`.  
By combining it with the `MaskingShape` component, you can implement an effect that displays only the buttons during the
tutorial.

<br><br>

### Project Settings

![](https://github.com/mob-sakai/mob-sakai/releases/download/docs/1759018902322.png)

You can adjust the project-wide settings for SoftMaskForUGUI. (`Edit > Project Settings > UI > Soft Mask`)

- **Soft Mask Enabled**: Enable soft masking.
- **Stereo Enabled**: Enable VR mode.
- **Transform Sensitivity**: `Low`, `Medium`, `High`
    - Adjust the transformation sensitivity for the soft mask buffer update.
    - The higher the sensitivity, the more frequently the soft mask buffer is updated.
- **Soft Maskable**: Determines how to add `SoftMaskable` components under `SoftMask`.
    - `Automatic`: `SoftMaskable` components are added automatically as needed.
    - `Manual`: You need to add `SoftMaskable` components explicitly.
- **Hide Generated Component**: Automatically hide the generated `MaskingShapeContainer` and `TerminalMaskingShape`
  components.
- **Optional Shaders (SoftMaskable)**: A list of shaders that will be prioritized when a soft-maskable shader is
  requested.
    - If the shader is included in the list, that shader will be used.
    - If it is not in the list, the following shaders will be used in order:
        - If the shader name contains `(SoftMaskable)`, that shader will be used.
        - If `Hidden/<shader_name> (SoftMaskable)` exists, that shader will be used.
        - As a fallback, `UI/Default (SoftMaskable)` will be used.
- **Registered Variants**: A list of shader variants available at runtime. Use "-" button to remove unused variants,
  reducing build time and file size.
    - By default, the soft-maskable shaders will be included in the build, but you can remove it if you don't need it.
- **Unregistered Variants**: A list of shader variants that are not registered. Use "+" button to add variants.
- **Error On Unregistered Variant**: If enabled, an error will be displayed when an unregistered shader variant is used.
    - The shader variant will be automatically added to the `Unregistered Variants` list.
- **Upgrade All Assets For V3**: Upgrade all assets for v3.
    - ⚠️ This will apply the changes to all assets in the project.
    - For details, please see [Upgrade All Assets For V3](#-upgrade-all-assets-for-v3).

> [!IMPORTANT]
> - The setting file is usually saved in `Assets/ProjectSettings/UISoftMaskProjectSettings.asset`. Include this file in your version control system.
> - The setting file is automatically added as a preloaded asset in `ProjectSettings/ProjectSettings.asset`.

<br><br>

### Usage with Scripts

```csharp
var softMask = gameObject.GetComponent<SoftMask>();
softMask.maskingMode = SoftMask.MaskingMode.SoftMasking;
softMask.downSamplingRate = SoftMask.DownSamplingRate.x2;
softMask.softnessRange = new MinMax01(0.5f, 0.75f);
```

<br><br>

### Usage with TextMeshPro or Spine

To use SoftMask with TextMeshPro or Spine, you need to import additional resources.  
When a shader included in the samples is requested, an import dialog will automatically appear.  
Click the `Import` button.

![](https://github.com/user-attachments/assets/9d4c5f71-1d1a-4f2c-a04e-4549e384fa36)

Alternatively, you can manually import the resources by following these steps:

1. Open the `Package Manager` window and select the `UI Soft Mask` package from the package list.
2. Click the `Import` button for each sample to import the required resources.  
    ![](https://github.com/user-attachments/assets/d3f83102-4617-4993-9f00-c0c7641633cd)
    - TextMeshPro (Unity 2023.1 or earlier): `TextMeshPro Support`
    - TextMeshPro (Unity 2023.2, 6000.0 or later): `TextMeshPro Support (Unity 6)`
    - Spine: `Spine Support`

3. The assets will be imported under `Assets/Samples/UI Soft Mask/{version}`.

> [!TIP]
> If you have moved `TMPro_Properties.cginc` and `TMPro.cginc` from their default install path
> (`Assets/TextMesh Pro/Shaders/...`), you will need to manually update the paths in the shaders under
> `TextMeshPro Support` or `TextMeshPro Support (Unity 6)`.

<br><br>

### Usage with Your Custom Shaders

Here, let's
make [UI/Additive](https://raw.githubusercontent.com/mob-sakai/SoftMaskForUGUI/develop/Assets/Demos/SoftMaskable%20Shader/UI-Additive.shader)
custom shader soft-maskable.  
There are two ways to support SoftMask with custom shaders.

- **Hybrid (recommended)**: Add soft-maskable variants to the existing shader. (
  e.g. [UI/Additive (SoftMaskable)](https://raw.githubusercontent.com/mob-sakai/SoftMaskForUGUI/develop/Assets/Demos/SoftMaskable%20Shader/UI-Additive-SoftMaskable.shader))  
  Modify the shader as follows:
    ```shaderlab
    // Add the ` (SoftMaskable)` suffix to the shader name.
    Shader "UI/Additive (SoftMaskable)"
    
    // Import "SoftMask.cginc" and add shader features.
    #include "Packages/com.coffee.softmask-for-ugui/Shaders/SoftMask.cginc"
    #pragma shader_feature _ SOFTMASK_EDITOR
    #pragma shader_feature_local _ SOFTMASKABLE
    
    // "SoftMask" function returns [0-1]. Multiply this by the final output.
    color.a *= SoftMask(IN.vertex, IN.worldPosition, color.a);
    ```
- **Separate**: Create a new shader with soft-maskable variants. (
  e.g. [Hidden/UI/Additive (SoftMaskable)](https://raw.githubusercontent.com/mob-sakai/SoftMaskForUGUI/develop/Assets/Demos/SoftMaskable%20Shader/Hidden-UI-Additive-SoftMaskable.shader))  
  Use this way for built-in shaders that cannot be edited, like `UI/Default`.  
  Modify the shader as follows:
    ```shaderlab
    // Add the `Hidden/` prefix and ` (SoftMaskable)` suffix to the shader name.
    Shader "Hidden/UI/Additive (SoftMaskable)"
    
    // Import "SoftMask.cginc" and add shader features.
    #include "Packages/com.coffee.softmask-for-ugui/Shaders/SoftMask.cginc"
    #pragma shader_feature _ SOFTMASK_EDITOR
    #pragma shader_feature_local _ SOFTMASKABLE
    
    // "SoftMask" function returns [0-1]. Multiply this by the final output.
    color.a *= SoftMask(IN.vertex, IN.worldPosition, color.a);
    ```
  ![](https://github.com/user-attachments/assets/e7856aee-89f2-435e-8613-1b7ff706916f)

<br><br>

### Usage with Canvas ShaderGraph

NOTE: Unity 2023.2/6000.0+ is required.

1. Open the `Package Manager` window and select the `UI Soft Mask` package in the package list and click the
   `ShaderGraph Support > Import` button.

2. The sample includes `UIDefault (SoftMaskable).shadergraph` and `SoftMask.subshadergraph`.  
   You can use the sample as references to make your own shader graph compatible with `SoftMask`.
    1. Add ` (SoftMaskable)` at the end of the shader name.
    2. Add `SOFTMASK_EDITOR` and `SOFTMASKABLE` as a `Boolean Keyword (Shader Feature)`.
    3. Add the `Sub Graphs > SoftMask` node and connect it to the final alpha output.  
       ![](https://github.com/user-attachments/assets/67f77656-2541-4573-8875-e8053c946da6)

<br><br>

### Usage with UIEffect

![](https://github.com/user-attachments/assets/7701e765-896a-49a8-b1ed-22adb0ecce12)

[UIEffect](https://github.com/mob-sakai/UIEffect) is a package that allows you to intuitively apply rich Unity UI effects.

`SoftMaskForUGUI (v3.3.0+)` supports `UIEffect (v5.6.0+)`.  
When a shader included in the samples is requested, an import dialog will automatically appear.  
Click the `Import` button.

<br><br>

### :warning: Limitations

The following are the limitations of SoftMaskForUGUI.

- (Android) `RGB ETC1 (+ Split alpha channel)` texture format is not supported.
    - Use a format that supports alpha, such as `RGBA ETC2`.
    - Technically possible, but not supported
      because [ETC2 support rate is over 95%](https://developer.android.com/guide/playcore/asset-delivery/texture-compression).
    - If needed, feel free to create an issue.
- Alpha hit test is not supported when the texture is in crunch format or non-readable.
- `Hidden/UI/SoftMask`, `Hidden/UI/TerminalMaskingShape` and `SoftMaskable` shader variants used at runtime must be
  registered in the [Project Settings](#project-settings).
- If you are using `DynamicResolution` or `RenderScale (URP)`, the display may not work correctly.  
    - For details, see [the issue](https://github.com/mob-sakai/SoftMaskForUGUI/issues/227).

<br><br>

## 🤝 Contributing

### Issues

Issues are incredibly valuable to this project:

- Ideas provide a valuable source of contributions that others can make.
- Problems help identify areas where this project needs improvement.
- Questions indicate where contributors can enhance the user experience.

### Pull Requests

Pull requests offer a fantastic way to contribute your ideas to this repository.  
Please refer to [CONTRIBUTING.md](https://github.com/mob-sakai/SoftMaskForUGUI/tree/develop/CONTRIBUTING.md)
and [develop branch](https://github.com/mob-sakai/SoftMaskForUGUI/tree/develop).

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
* Change log : https://github.com/mob-sakai/SoftMaskForUGUI/blob/main/Packages/src/CHANGELOG.md
