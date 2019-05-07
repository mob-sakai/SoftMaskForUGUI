# Changelog

## [v0.8.1](https://github.com/mob-sakai/SoftMaskForUGUI/tree/v0.8.1) (2019-05-07)

[Full Changelog](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v0.8.0...v0.8.1)

**Fixed bugs:**

- Unity 2018.x compile failed [\#51](https://github.com/mob-sakai/SoftMaskForUGUI/issues/51)

## [v0.8.0](https://github.com/mob-sakai/SoftMaskForUGUI/tree/v0.8.0) (2019-05-01)

[Full Changelog](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v0.7.2...v0.8.0)

Camera movement affects the mask rendering when on a World Space Canvas.  
![](https://user-images.githubusercontent.com/12690315/57015752-68540b80-6c51-11e9-8511-2d4534dd9d58.gif)

**Fixed bugs:**

- In overlay mode, mask will be incorrect if the root canvas's parent position are not zero [\#47](https://github.com/mob-sakai/SoftMaskForUGUI/issues/47)

## [v0.7.2](https://github.com/mob-sakai/SoftMaskForUGUI/tree/v0.7.2) (2019-03-16)

[Full Changelog](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v0.7.1...v0.7.2)

**Fixed bugs:**

- The masked images all disappear if game view is maximized [\#44](https://github.com/mob-sakai/SoftMaskForUGUI/issues/44)
-  Pixels out of range may be read by raycaster [\#43](https://github.com/mob-sakai/SoftMaskForUGUI/issues/43)
- The masked images all disappear when the game view is resized [\#42](https://github.com/mob-sakai/SoftMaskForUGUI/issues/42)
- Doesn't work with Screen-Space Overlay [\#41](https://github.com/mob-sakai/SoftMaskForUGUI/issues/41)

## [v0.7.1](https://github.com/mob-sakai/SoftMaskForUGUI/tree/v0.7.1) (2019-03-11)

[Full Changelog](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v0.7.0...v0.7.1)

**Fixed bugs:**

- if canvas camera is not set in world space mode, masked contents are not displayed [\#36](https://github.com/mob-sakai/SoftMaskForUGUI/issues/36)

## [v0.7.0](https://github.com/mob-sakai/SoftMaskForUGUI/tree/v0.7.0) (2019-03-11)

[Full Changelog](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v0.6.0...v0.7.0)

Add 'part of parent' option to make multiple holes on one background  
![](https://user-images.githubusercontent.com/12690315/54102470-f5c26e80-440b-11e9-89d1-899aa4dca00d.png)

**Implemented enhancements:**

- 'Parts of parent' option [\#29](https://github.com/mob-sakai/SoftMaskForUGUI/issues/29)

**Fixed bugs:**

- scene view flickering [\#38](https://github.com/mob-sakai/SoftMaskForUGUI/issues/38)
- Flipped soft mask texture on Windows [\#37](https://github.com/mob-sakai/SoftMaskForUGUI/issues/37)

## [v0.6.0](https://github.com/mob-sakai/SoftMaskForUGUI/tree/v0.6.0) (2019-02-07)

[Full Changelog](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v0.5.0...v0.6.0)

![](https://user-images.githubusercontent.com/12690315/52258046-e2ca0380-2960-11e9-8cdb-46ee4a5f880f.png)  
Scene view bugs have been fixed.


**Fixed bugs:**

- Shaders for TMPro have compile errors [\#33](https://github.com/mob-sakai/SoftMaskForUGUI/issues/33)
- SoftMask does not display properly at the edge of the canvas [\#32](https://github.com/mob-sakai/SoftMaskForUGUI/issues/32)

**Closed issues:**

- Shaders for TMPro are no longer support d3d9 & d3d11\_9x [\#34](https://github.com/mob-sakai/SoftMaskForUGUI/issues/34)

## [v0.5.0](https://github.com/mob-sakai/SoftMaskForUGUI/tree/v0.5.0) (2019-02-01)

[Full Changelog](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v0.4.0...v0.5.0)

**Implemented enhancements:**

- Mask interaction for each layer [\#31](https://github.com/mob-sakai/SoftMaskForUGUI/issues/31)

**Fixed bugs:**

- SoftMask is not clipped by RectMask2D [\#30](https://github.com/mob-sakai/SoftMaskForUGUI/issues/30)
- Does not work with flipped/rotated images [\#27](https://github.com/mob-sakai/SoftMaskForUGUI/issues/27)
- SceneView does not display SoftMask properly [\#16](https://github.com/mob-sakai/SoftMaskForUGUI/issues/16)

## [v0.4.0](https://github.com/mob-sakai/SoftMaskForUGUI/tree/v0.4.0) (2019-01-13)

[Full Changelog](https://github.com/mob-sakai/SoftMaskForUGUI/compare/0.4.0...v0.4.0)

**Implemented enhancements:**

- Integrate with UnityPackageManager [\#22](https://github.com/mob-sakai/SoftMaskForUGUI/issues/22)

**Fixed bugs:**

- Flipped soft mask texture [\#25](https://github.com/mob-sakai/SoftMaskForUGUI/issues/25)

## [v0.3.0](https://github.com/mob-sakai/SoftMaskForUGUI/tree/v0.3.0) (2019-01-07)

[Full Changelog](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v0.2.0...v0.3.0)

**Implemented enhancements:**

- Remove TMPro resources in repo [\#21](https://github.com/mob-sakai/SoftMaskForUGUI/issues/21)

**Fixed bugs:**

- An error occur when SoftMask is destroyed on editor [\#23](https://github.com/mob-sakai/SoftMaskForUGUI/issues/23)

## [v0.2.0](https://github.com/mob-sakai/SoftMaskForUGUI/tree/v0.2.0) (2018-12-21)

[Full Changelog](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v0.1.0...v0.2.0)

**Implemented enhancements:**

- Set default material on disable [\#17](https://github.com/mob-sakai/SoftMaskForUGUI/issues/17)
- Component icon [\#15](https://github.com/mob-sakai/SoftMaskForUGUI/issues/15)
- Support TextMeshPro [\#14](https://github.com/mob-sakai/SoftMaskForUGUI/issues/14)
- Preview soft mask buffer in inspector [\#13](https://github.com/mob-sakai/SoftMaskForUGUI/issues/13)
- Add a SoftMaskable component to the child UI elements of SoftMask From the inspector [\#12](https://github.com/mob-sakai/SoftMaskForUGUI/issues/12)
- Render the soft mask buffer only when needed to improve performance [\#11](https://github.com/mob-sakai/SoftMaskForUGUI/issues/11)

**Fixed bugs:**

- Doesn't work with overlay canvas on 2018.3 [\#20](https://github.com/mob-sakai/SoftMaskForUGUI/issues/20)

## [v0.1.0](https://github.com/mob-sakai/SoftMaskForUGUI/tree/v0.1.0) (2018-11-20)

[Full Changelog](https://github.com/mob-sakai/SoftMaskForUGUI/compare/0d87935fa566cd1cb5e54a6f8826bb72fffb29b8...v0.1.0)

**Implemented enhancements:**

- Convert existing Mask to SoftMask from context menu [\#10](https://github.com/mob-sakai/SoftMaskForUGUI/issues/10)
- Desample soft mask buffer to improve performance [\#9](https://github.com/mob-sakai/SoftMaskForUGUI/issues/9)
- Custom shaders supporting [\#8](https://github.com/mob-sakai/SoftMaskForUGUI/issues/8)
- Filter raycast only for the visible part [\#7](https://github.com/mob-sakai/SoftMaskForUGUI/issues/7)
- Inverse soft mask [\#6](https://github.com/mob-sakai/SoftMaskForUGUI/issues/6)
- Nested soft masks [\#5](https://github.com/mob-sakai/SoftMaskForUGUI/issues/5)
- Support multiple-sprites and SpriteAtlas [\#4](https://github.com/mob-sakai/SoftMaskForUGUI/issues/4)
- Adjust the visible part [\#3](https://github.com/mob-sakai/SoftMaskForUGUI/issues/3)
- Compatible with Mask [\#2](https://github.com/mob-sakai/SoftMaskForUGUI/issues/2)
- Screen space soft masking [\#1](https://github.com/mob-sakai/SoftMaskForUGUI/issues/1)



\* *This Changelog was automatically generated by [github_changelog_generator](https://github.com/github-changelog-generator/github-changelog-generator)*
