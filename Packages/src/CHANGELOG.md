## [3.3.2](https://github.com/mob-sakai/SoftMaskForUGUI/compare/3.3.1...3.3.2) (2025-04-23)


### Bug Fixes

* NullReferenceException in MaskingShape component when in prefab mode (editor) ([9cc6e68](https://github.com/mob-sakai/SoftMaskForUGUI/commit/9cc6e68ca748bc6ab20a055a348f68f29f03ce37)), closes [#240](https://github.com/mob-sakai/SoftMaskForUGUI/issues/240)

## [3.3.1](https://github.com/mob-sakai/SoftMaskForUGUI/compare/3.3.0...3.3.1) (2025-04-18)


### Bug Fixes

* ShaderGraphs invisible when not masked in scene view ([105713b](https://github.com/mob-sakai/SoftMaskForUGUI/commit/105713b0e82cdf4cb434892469f2200310ed0561)), closes [#238](https://github.com/mob-sakai/SoftMaskForUGUI/issues/238)

# [3.3.0](https://github.com/mob-sakai/SoftMaskForUGUI/compare/3.2.4...3.3.0) (2025-03-28)


### Bug Fixes

* soft-masked graphics are not displayed correctly in the game view (editor, OpenGLES3) ([2bf00fc](https://github.com/mob-sakai/SoftMaskForUGUI/commit/2bf00fccde0ec2aea68852833bf31c23b30c4df7)), closes [#232](https://github.com/mob-sakai/SoftMaskForUGUI/issues/232)


### Features

* add a tool that automatically fixes shaders when 'TextMeshPro Essential Resources' are located in a non-default path ([03b5e47](https://github.com/mob-sakai/SoftMaskForUGUI/commit/03b5e47e08e000adcef3246edc320a6ff9d62e81)), closes [#233](https://github.com/mob-sakai/SoftMaskForUGUI/issues/233)
* add shaders for TextMeshPro Overlay/SSD ([e4d974e](https://github.com/mob-sakai/SoftMaskForUGUI/commit/e4d974e76d7f8766739afb40af9f77dfff4bd8f9)), closes [#230](https://github.com/mob-sakai/SoftMaskForUGUI/issues/230)
* shader graph support no longer requires an additional render feature ([bb057af](https://github.com/mob-sakai/SoftMaskForUGUI/commit/bb057af80fb239d5b76e66dc7b28e65c5eab45db))
* support `UIEffect` package with `SoftMaskable` ([47dcacb](https://github.com/mob-sakai/SoftMaskForUGUI/commit/47dcacb1d322e7b5327c929a48c61fff360894ac)), closes [#229](https://github.com/mob-sakai/SoftMaskForUGUI/issues/229)

## [3.2.4](https://github.com/mob-sakai/SoftMaskForUGUI/compare/3.2.3...3.2.4) (2025-03-06)


### Bug Fixes

* 'IconAttribute' is inaccessible due to its protection level on older versions of Unity ([4ad0c6b](https://github.com/mob-sakai/SoftMaskForUGUI/commit/4ad0c6b5a7d8b8887b5b555cf6183fbbd6e70808)), closes [#234](https://github.com/mob-sakai/SoftMaskForUGUI/issues/234)
* IL2CPP build fails on older versions of Unity ([8a9a1a1](https://github.com/mob-sakai/SoftMaskForUGUI/commit/8a9a1a11f6f28befdce2dcdec5cc8d779df492ac))

## [3.2.3](https://github.com/mob-sakai/SoftMaskForUGUI/compare/3.2.2...3.2.3) (2025-02-28)


### Bug Fixes

* component icons will no longer be displayed in the scene view (Unity 2021.2 or later) ([b38dd4c](https://github.com/mob-sakai/SoftMaskForUGUI/commit/b38dd4c46b386e7780e60752b8bd39203cd8d811))
* fix potential issues ([3377af2](https://github.com/mob-sakai/SoftMaskForUGUI/commit/3377af2d2a62dfe790e92d76b91f04b43c8ae763))
* remove warn CS0618 ([04f587c](https://github.com/mob-sakai/SoftMaskForUGUI/commit/04f587c5d54462e43074629225662d18e007c1be))

## [3.2.2](https://github.com/mob-sakai/SoftMaskForUGUI/compare/3.2.1...3.2.2) (2025-01-18)


### Bug Fixes

* addressable build error in Unity 6 ([0f25706](https://github.com/mob-sakai/SoftMaskForUGUI/commit/0f257062a664f9d34b4d4782a44c642abecac798)), closes [#224](https://github.com/mob-sakai/SoftMaskForUGUI/issues/224)

## [3.2.1](https://github.com/mob-sakai/SoftMaskForUGUI/compare/3.2.0...3.2.1) (2025-01-06)


### Bug Fixes

* fix SoftMask with TextMeshPro ([abe9388](https://github.com/mob-sakai/SoftMaskForUGUI/commit/abe9388c38778f5e4a47fc9a0e38c84fad3afe6f)), closes [#223](https://github.com/mob-sakai/SoftMaskForUGUI/issues/223)

# [3.2.0](https://github.com/mob-sakai/SoftMaskForUGUI/compare/3.1.2...3.2.0) (2025-01-03)


### Features

* automatically display a dialog to import samples for TextMeshPro or Spine ([a122057](https://github.com/mob-sakai/SoftMaskForUGUI/commit/a122057ea878d844599e81f8b602d26a3cb9f5fe)), closes [#219](https://github.com/mob-sakai/SoftMaskForUGUI/issues/219)
* change the minimum value of `SoftMaskable.power` from 1 to 0.5, allowing for smoother softness transitions ([3d59755](https://github.com/mob-sakai/SoftMaskForUGUI/commit/3d597554b71e1b4517a663835908efe4996717d1))
* Spine support ([7677a49](https://github.com/mob-sakai/SoftMaskForUGUI/commit/7677a49ea2ef42689d171a3006f2e0316bf8226a)), closes [#218](https://github.com/mob-sakai/SoftMaskForUGUI/issues/218) [#221](https://github.com/mob-sakai/SoftMaskForUGUI/issues/221)
* support `TextMeshPro/Bitmap` and `TextMeshPro/Mobile/Bitmap` shaders. ([6d4900c](https://github.com/mob-sakai/SoftMaskForUGUI/commit/6d4900ce640ec83a2d5aa478bc97720c5fb005d0))

## [3.1.2](https://github.com/mob-sakai/SoftMaskForUGUI/compare/3.1.1...3.1.2) (2024-12-25)


### Bug Fixes

* The `SoftMask` component was blocking raycasts to the `MaskingShape (Subtract)` area. ([828c869](https://github.com/mob-sakai/SoftMaskForUGUI/commit/828c8690900bb80103c846196dcbd1ae525739ce)), closes [#220](https://github.com/mob-sakai/SoftMaskForUGUI/issues/220)

## [3.1.1](https://github.com/mob-sakai/SoftMaskForUGUI/compare/3.1.0...3.1.1) (2024-12-23)


### Bug Fixes

* fix NRE ([190e149](https://github.com/mob-sakai/SoftMaskForUGUI/commit/190e149956abe33838abbc94edb7f8180f955e26))

# [3.1.0](https://github.com/mob-sakai/SoftMaskForUGUI/compare/3.0.1...3.1.0) (2024-12-22)


### Bug Fixes

* soft masking for SubMesheUI (TextMeshProUGUI) may not display correctly ([d417c06](https://github.com/mob-sakai/SoftMaskForUGUI/commit/d417c060596ab58179d6b3e622bd40309b49a3b8)), closes [#216](https://github.com/mob-sakai/SoftMaskForUGUI/issues/216)


### Features

* maskingShape with TextMeshPro ([858655f](https://github.com/mob-sakai/SoftMaskForUGUI/commit/858655f24be33aff8b5188fde4bd0362b391a129)), closes [#217](https://github.com/mob-sakai/SoftMaskForUGUI/issues/217)

## [3.0.1](https://github.com/mob-sakai/SoftMaskForUGUI/compare/3.0.0...3.0.1) (2024-12-10)


### Bug Fixes

* shaders with `(SoftMaskable)` in their names are ignored ([6e5eaa5](https://github.com/mob-sakai/SoftMaskForUGUI/commit/6e5eaa50ef8dc3c8748e9b83b8254bd400395c06))

# [3.0.0](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.4.6...3.0.0) (2024-12-01)


### Bug Fixes

* fix anti-aliasing issues ([e68ee7b](https://github.com/mob-sakai/SoftMaskForUGUI/commit/e68ee7bf16a8a2d43bcf0fc74500fc9d5e355be8))


### Features

* add 'SOFTMASKABLE' shader feature to improve shader build ([eec3212](https://github.com/mob-sakai/SoftMaskForUGUI/commit/eec3212a2d3d93e87ae191230a99f8d9c5937190))
* asset upgrade system 'SOFTMASKABLE' shader feature ([d36f932](https://github.com/mob-sakai/SoftMaskForUGUI/commit/d36f9323bacc13df79aff80777ecac66678fc97c))
* MaskingShape components always use stencil mask ([6c6ac59](https://github.com/mob-sakai/SoftMaskForUGUI/commit/6c6ac590d2f4bb35548358c73f5c16458adeb0fa)), closes [#203](https://github.com/mob-sakai/SoftMaskForUGUI/issues/203) [#209](https://github.com/mob-sakai/SoftMaskForUGUI/issues/209)
* strip shaders using ShaderVariantCollection instead of 'Always Include Shaders' ([5f4c435](https://github.com/mob-sakai/SoftMaskForUGUI/commit/5f4c43550e664aad50cd159777bd857f9ec15a69)), closes [#211](https://github.com/mob-sakai/SoftMaskForUGUI/issues/211)
* update asset upgrade system to unregister always included shaders ([906bdb6](https://github.com/mob-sakai/SoftMaskForUGUI/commit/906bdb69ceae5aee25b12b6fc6de4243a4845ebf))


### BREAKING CHANGES

* SoftMaskable shaders now require the `SOFTMASKABLE` shader feature.
Please upgrade assets from `Project Settings > UI > Soft Mask` or refer to the 'README > Usage with Your Custom Shaders' section to manually update your shaders.

## [2.4.6](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.4.5...v2.4.6) (2024-11-22)


### Bug Fixes

* build AssetBundle error ([005a91d](https://github.com/mob-sakai/SoftMaskForUGUI/commit/005a91d493da9c8677b469b39dac4f45111ce841)), closes [#213](https://github.com/mob-sakai/SoftMaskForUGUI/issues/213)

## [2.4.5](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.4.4...v2.4.5) (2024-11-17)


### Bug Fixes

* all SoftMaskable components will be removed on upgrade ([c0593d4](https://github.com/mob-sakai/SoftMaskForUGUI/commit/c0593d4d704198c69d5666478b1f25cb78de5dda))
* set component icons ([7cf7336](https://github.com/mob-sakai/SoftMaskForUGUI/commit/7cf73367227e421b8e2894ff3cefafec4c673a13))

## [2.4.4](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.4.3...v2.4.4) (2024-11-16)


### Bug Fixes

* accessing game object transform hierarchy before loading of scene has completed ([9dfacbf](https://github.com/mob-sakai/SoftMaskForUGUI/commit/9dfacbfe14668b6d409cdaf8c1ae20d520e70d8f)), closes [#208](https://github.com/mob-sakai/SoftMaskForUGUI/issues/208)
* softmask elements stop drawing on resolution change sometimes in editor ([f67d025](https://github.com/mob-sakai/SoftMaskForUGUI/commit/f67d025683116934bf4946a7b13605519140882c)), closes [#208](https://github.com/mob-sakai/SoftMaskForUGUI/issues/208)

## [2.4.3](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.4.2...v2.4.3) (2024-11-15)


### Reverts

* Revert "fix: softmask elements stop drawing on resolution change sometimes in editor" ([6c864ac](https://github.com/mob-sakai/SoftMaskForUGUI/commit/6c864ac89e42338ec7cc9271682d01e2015a5ae6))

## [2.4.2](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.4.1...v2.4.2) (2024-11-14)


### Bug Fixes

* softmask elements stop drawing on resolution change sometimes in editor ([380fdc6](https://github.com/mob-sakai/SoftMaskForUGUI/commit/380fdc6c4378fe284457451e77139552a83ff438))

## [2.4.1](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.4.0...v2.4.1) (2024-11-14)


### Bug Fixes

* alpha hit test not working in demo scene ([135b123](https://github.com/mob-sakai/SoftMaskForUGUI/commit/135b12313b977750e0797dceb1ad08cf87ce607a)), closes [#210](https://github.com/mob-sakai/SoftMaskForUGUI/issues/210)
* If not configured as a preloaded asset, the project settings asset will be regenerated ([cca61d7](https://github.com/mob-sakai/SoftMaskForUGUI/commit/cca61d7143d47ebb5190d7bfdce870e5043cd5e9)), closes [#200](https://github.com/mob-sakai/SoftMaskForUGUI/issues/200) [#212](https://github.com/mob-sakai/SoftMaskForUGUI/issues/212)
* SoftMaskable is not editable ([f9937a9](https://github.com/mob-sakai/SoftMaskForUGUI/commit/f9937a9d610b559e9dbc227e9a29b30fe0c7f483))
* TerminalMaskingShape is hidden even with `HideGeneratedComponents=false` ([8ddb586](https://github.com/mob-sakai/SoftMaskForUGUI/commit/8ddb58638ebd0a08ce0dc9287854ffcad1d7746b))

# [2.4.0](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.3.8...v2.4.0) (2024-11-01)


### Bug Fixes

* argumentException: RenderTextureDesc width must be greater than zero when setting the Down Sampling Rate to X8. ([3ffe109](https://github.com/mob-sakai/SoftMaskForUGUI/commit/3ffe109c456e7d97f41d909b11426fe48c10596d)), closes [#204](https://github.com/mob-sakai/SoftMaskForUGUI/issues/204)
* compile error in UniversalRP 16 (Unity 2023.2) ([80413fc](https://github.com/mob-sakai/SoftMaskForUGUI/commit/80413fc4473f3ccf76e34a43b2a09267f0fd2f4c)), closes [#206](https://github.com/mob-sakai/SoftMaskForUGUI/issues/206)


### Features

* add `ignoreSelf` and `ignoreChildren` option for SoftMaskable element ([315b86c](https://github.com/mob-sakai/SoftMaskForUGUI/commit/315b86c08452731674b3fe1e31cfb8cf19f9dde8))
* add `power` option for SoftMaskable element ([1362b18](https://github.com/mob-sakai/SoftMaskForUGUI/commit/1362b1874b9d65bcd29e8ea013c296f2d1e29014))

## [2.3.8](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.3.7...v2.3.8) (2024-09-29)


### Bug Fixes

* if `UniversalRenderPipelineAsset.renderScale` is not 1, soft masking will not be rendered correctly ([6e5a7f9](https://github.com/mob-sakai/SoftMaskForUGUI/commit/6e5a7f97a40d96f0493ec74bf8185d57bc646a8f)), closes [#194](https://github.com/mob-sakai/SoftMaskForUGUI/issues/194)
* in URP, dynamic resolution does not work when render scale is close to 1 ([afed40a](https://github.com/mob-sakai/SoftMaskForUGUI/commit/afed40a0e85aa83194d0f28f5305d4312b015b17))
* recreates ProjectSettings scriptable object when the project is pulled from GitHub ([9f6ea22](https://github.com/mob-sakai/SoftMaskForUGUI/commit/9f6ea2293607e36104371c633fd899ded1b2305a)), closes [#200](https://github.com/mob-sakai/SoftMaskForUGUI/issues/200)
* when using dynamic resolution, soft masking will not be rendered correctly ([5100e3f](https://github.com/mob-sakai/SoftMaskForUGUI/commit/5100e3f092d831c3e0a40b2a7b543412601545c3)), closes [#199](https://github.com/mob-sakai/SoftMaskForUGUI/issues/199)

## [2.3.7](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.3.6...v2.3.7) (2024-09-16)


### Bug Fixes

* alias name conflict ([4b847ad](https://github.com/mob-sakai/SoftMaskForUGUI/commit/4b847adba35b8469dc8fbd6465e0db9f22aaff52)), closes [#198](https://github.com/mob-sakai/SoftMaskForUGUI/issues/198)

## [2.3.6](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.3.5...v2.3.6) (2024-09-16)


### Bug Fixes

* version 2.3.5 does not work ([23d4aa9](https://github.com/mob-sakai/SoftMaskForUGUI/commit/23d4aa90aa9260a8be6619438840dcb044b74259))

## [2.3.5](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.3.4...v2.3.5) (2024-09-15)


### Bug Fixes

* alphaHitTestMinimumThreshold should not be modified on a texture not readeable or not using Crunch Compression ([98783ec](https://github.com/mob-sakai/SoftMaskForUGUI/commit/98783eceae6a7442f45cb1d829abc4d57c35efd4)), closes [#197](https://github.com/mob-sakai/SoftMaskForUGUI/issues/197)

## [2.3.4](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.3.3...v2.3.4) (2024-09-14)


### Bug Fixes

* MaskingShapeContainer NullReferenceException in demo ([#195](https://github.com/mob-sakai/SoftMaskForUGUI/issues/195)) ([96a090f](https://github.com/mob-sakai/SoftMaskForUGUI/commit/96a090f09d16979d4b837da0d5c0095c5886bb3a))
* ShaderGraph is not working correctly ([#191](https://github.com/mob-sakai/SoftMaskForUGUI/issues/191)) ([82c38dd](https://github.com/mob-sakai/SoftMaskForUGUI/commit/82c38dd290cd98b1daf9cfd3dc74334a53dbde8f))
* subtract mode `MaskingShape` does not display correctly in the editor ([f83a647](https://github.com/mob-sakai/SoftMaskForUGUI/commit/f83a647c30a4b8df8a1f1d80ddfef299dc123408))

## [2.3.3](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.3.2...v2.3.3) (2024-09-06)


### Bug Fixes

* when the player window moved to a different resolution display, soft masking does not work property ([79cac0e](https://github.com/mob-sakai/SoftMaskForUGUI/commit/79cac0e065c358b21b9f70278821929532f10878)), closes [#193](https://github.com/mob-sakai/SoftMaskForUGUI/issues/193)

## [2.3.2](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.3.1...v2.3.2) (2024-09-06)


### Bug Fixes

* SetDirtyAndNotify causes null exceptions on GameObject delete ([706f022](https://github.com/mob-sakai/SoftMaskForUGUI/commit/706f022cb3d503fc57d72f5c0bb716d7a918cdf7)), closes [#192](https://github.com/mob-sakai/SoftMaskForUGUI/issues/192)

## [2.3.1](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.3.0...v2.3.1) (2024-09-05)


### Bug Fixes

* ShaderGraph support is not included ([ce29e1a](https://github.com/mob-sakai/SoftMaskForUGUI/commit/ce29e1a5a7f7a353d4308e44d83e4cbfe22cddcd))

# [2.3.0](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.2.4...v2.3.0) (2024-09-05)


### Bug Fixes

* `MaskingShape.maskingMethod = Subtract` is not working correctly ([56e1262](https://github.com/mob-sakai/SoftMaskForUGUI/commit/56e12621267d9e3b03272101989d3956b424af7a))
* nested `MaskingShape` is not working correctly ([b1331cf](https://github.com/mob-sakai/SoftMaskForUGUI/commit/b1331cf21376936ff4ad7fa57146e666698e4eb4)), closes [#189](https://github.com/mob-sakai/SoftMaskForUGUI/issues/189)
* when `UseStencilOutsideScreen = true`, objects inside `SoftMask` are not batched in the editor ([21dfae7](https://github.com/mob-sakai/SoftMaskForUGUI/commit/21dfae7c2c4d53273df4649891e038eb41cb911f)), closes [#188](https://github.com/mob-sakai/SoftMaskForUGUI/issues/188)


### Features

* ShaderGraph support ([c4ebe58](https://github.com/mob-sakai/SoftMaskForUGUI/commit/c4ebe58cfa14140ab87cc889ceff214d97b3e2ba)), closes [#190](https://github.com/mob-sakai/SoftMaskForUGUI/issues/190)

## [2.2.4](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.2.3...v2.2.4) (2024-09-04)


### Bug Fixes

* stencil masking does not working correctly when `SoftMask` component is placed under `Mask` component ([07084aa](https://github.com/mob-sakai/SoftMaskForUGUI/commit/07084aaa3e51ec2726af964613618f5443116e7e)), closes [#183](https://github.com/mob-sakai/SoftMaskForUGUI/issues/183)

## [2.2.3](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.2.2...v2.2.3) (2024-08-22)


### Bug Fixes

* in linear color mode, `TextMeshProUGUI` appeared darker ([4105efd](https://github.com/mob-sakai/SoftMaskForUGUI/commit/4105efd9d0fe4c105a67b021db86506549873a4d)), closes [#187](https://github.com/mob-sakai/SoftMaskForUGUI/issues/187)

## [2.2.2](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.2.1...v2.2.2) (2024-08-17)


### Bug Fixes

* soft mask is upside down on some devices (refactor) ([d17784e](https://github.com/mob-sakai/SoftMaskForUGUI/commit/d17784e03eb38f3eb0aac93830093d45dcf63bc4))

## [2.2.1](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.2.0...v2.2.1) (2024-08-16)


### Bug Fixes

* soft mask is upside down on some devices ([6e2ad42](https://github.com/mob-sakai/SoftMaskForUGUI/commit/6e2ad42423d43b7e3345efb54ab226d5388c88ab)), closes [#185](https://github.com/mob-sakai/SoftMaskForUGUI/issues/185)
* soft masked image flickers or disappears when resizing canvas in editor ([d5568b8](https://github.com/mob-sakai/SoftMaskForUGUI/commit/d5568b86a69baa443e923bab0dd68958134673ea)), closes [#184](https://github.com/mob-sakai/SoftMaskForUGUI/issues/184)

# [2.2.0](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.1.3...v2.2.0) (2024-08-11)


### Bug Fixes

* `Stereo Enabled` option in preferences does not work ([552f806](https://github.com/mob-sakai/SoftMaskForUGUI/commit/552f806524a74ac253bfa8b23dfe23f13ba8bee4))
* if no `Canvas` in the parents of the `SoftMask`, error occurred ([7c401d7](https://github.com/mob-sakai/SoftMaskForUGUI/commit/7c401d72c842fcce7696f045e109798b0966511a)), closes [#182](https://github.com/mob-sakai/SoftMaskForUGUI/issues/182)
* placing a SoftMaskable object outside of a SoftMask makes the object invisible ([088b723](https://github.com/mob-sakai/SoftMaskForUGUI/commit/088b723dc6e7781d0f04bf3a359774bcf5ba7de3)), closes [#180](https://github.com/mob-sakai/SoftMaskForUGUI/issues/180)
* softmask-buffer is cleared when TMPSubMeshUI is activated via EditorGUI ([13648ca](https://github.com/mob-sakai/SoftMaskForUGUI/commit/13648ca1bc860d36d580792187b536ead212bd3a)), closes [#180](https://github.com/mob-sakai/SoftMaskForUGUI/issues/180)


### Features

* support for shaders with `(SoftMaskable)` in the name except at the end ([3755656](https://github.com/mob-sakai/SoftMaskForUGUI/commit/37556569cb561bd6e6ac9463d502ec60ad6a45ea))

## [2.1.3](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.1.2...v2.1.3) (2024-07-26)


### Bug Fixes

* soft mask buffer is fliped on UNITY_UV_STARTS_AT_TOP devices ([e9c7822](https://github.com/mob-sakai/SoftMaskForUGUI/commit/e9c782287065adca628a32a6222adab4210e73fd)), closes [#179](https://github.com/mob-sakai/SoftMaskForUGUI/issues/179)

## [2.1.2](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.1.1...v2.1.2) (2024-07-25)


### Bug Fixes

* shaders for SoftMask are not automatically included ([3a37659](https://github.com/mob-sakai/SoftMaskForUGUI/commit/3a376595d15ba1c7229e009e50f7cc84214d7093)), closes [#177](https://github.com/mob-sakai/SoftMaskForUGUI/issues/177) [#178](https://github.com/mob-sakai/SoftMaskForUGUI/issues/178)

## [2.1.1](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.1.0...v2.1.1) (2024-07-24)


### Bug Fixes

* flashing when SoftnessRange is changed in inspector ([d1c6235](https://github.com/mob-sakai/SoftMaskForUGUI/commit/d1c623588c5cfd37aa8f23c57d5e15f397049eda))

# [2.1.0](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v2.0.0...v2.1.0) (2024-07-23)


### Bug Fixes

* fix null exception ([15d6917](https://github.com/mob-sakai/SoftMaskForUGUI/commit/15d6917e80a58f35deabcbf7b8f3b8697f498d71))
* fix upgrading asset system (v1 to v2) ([e6d0c60](https://github.com/mob-sakai/SoftMaskForUGUI/commit/e6d0c60edf63fe71c1cf8457a8e22c20485711bb))
* support 'Apply display rotation during rendering' for Vulkan (experimental) ([f5c9b41](https://github.com/mob-sakai/SoftMaskForUGUI/commit/f5c9b414d420ce978e509af643ae51f92c1078d1)), closes [#171](https://github.com/mob-sakai/SoftMaskForUGUI/issues/171)


### Features

* add 'Hide Generated Components' option in project settings ([053e92b](https://github.com/mob-sakai/SoftMaskForUGUI/commit/053e92ba7a063b7a354dea19aefd1839e665c96a))
* add a help box to the auto-generated object inspector ([f2d2115](https://github.com/mob-sakai/SoftMaskForUGUI/commit/f2d211538d07427fc2a5c8ba2063f2ee944057db)), closes [#175](https://github.com/mob-sakai/SoftMaskForUGUI/issues/175)
* revive `SoftMask.alpha` ([8080cb2](https://github.com/mob-sakai/SoftMaskForUGUI/commit/8080cb24091ec49511e28c3c5ed92ff3477b5867)), closes [#172](https://github.com/mob-sakai/SoftMaskForUGUI/issues/172)
* softmask buffers are now baked considering `Graphic.color.a` ([2eaaad6](https://github.com/mob-sakai/SoftMaskForUGUI/commit/2eaaad639673f440e096fd1ab529607d88fd5ba0))

# [2.0.0](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v1.0.2...v2.0.0) (2024-07-17)


### Bug Fixes

* fix a major performance regression where if you had a lot of softmaskable components (even if the parent canvas was in-active) it would eat over 20ms doing a certain editor-only function ([6e9fb30](https://github.com/mob-sakai/SoftMaskForUGUI/commit/6e9fb303e0aacfae292c7c011630ba877fdb4b63))
* fix broken anchor on README.md ([03a0425](https://github.com/mob-sakai/SoftMaskForUGUI/commit/03a0425c83119e9ff35252c0d6fce8fe2e35758c))
* fix soft-maskable shaders for TextMeshPro v2.0/v2.1/v2.2/v3.0 ([51d84c9](https://github.com/mob-sakai/SoftMaskForUGUI/commit/51d84c96b0c9018826c9e757b489538cd626b515))
* when changing resolution, display, or screen orientation, soft-masking do not work properly ([b39fcd9](https://github.com/mob-sakai/SoftMaskForUGUI/commit/b39fcd98b01310254613915d90b284b8d82fd10c))


### Features

* 'SoftMask.partOfParent' is now obsolete. Please use 'MaskingShape' component instead. ([3ac1dd2](https://github.com/mob-sakai/SoftMaskForUGUI/commit/3ac1dd28d4e42f5ee258b3f82a8949dd610a1d3b))
* (editor) use stencil outside screen in scene view for development ([028b85b](https://github.com/mob-sakai/SoftMaskForUGUI/commit/028b85b0877397a45d52690714b4dca0dcc95ef0))
* add 'AntiAliasing' masking mode. This mode suppresses stencil-derived mask aliasing without using RenderTexture. ([efb7f64](https://github.com/mob-sakai/SoftMaskForUGUI/commit/efb7f64dc1f7f9d68752e6271bc4fb3f22a745a0))
* add 'SoftMask.softMaskingRange' option ([76f69cf](https://github.com/mob-sakai/SoftMaskForUGUI/commit/76f69cf859097409d1c022188a617d18ebf11418))
* add asset modify system to migrate from v1 to v2 ([c451cf5](https://github.com/mob-sakai/SoftMaskForUGUI/commit/c451cf5a156e6fb05a7fb14b7f50a41d2e556e38))
* add explicit dependencies ([90a97c6](https://github.com/mob-sakai/SoftMaskForUGUI/commit/90a97c604c017c49fb786bc7a8dceaaa657228ba))
* add project settings for SoftMask ([4a124d4](https://github.com/mob-sakai/SoftMaskForUGUI/commit/4a124d41fcac3d291e07613a067ff97fc1845042))
* add soft-maskable shaders for TextMeshPro v3.2/v4.0 ([df888e0](https://github.com/mob-sakai/SoftMaskForUGUI/commit/df888e0091d3ebd3449f470b6e0b8048167aeec7))
* add stereo mode to shader ([e2e6733](https://github.com/mob-sakai/SoftMaskForUGUI/commit/e2e67339676a1e09b235eabb148cef7c06ed7065))
* exclude unused shader variants from the build (option) ([87b6060](https://github.com/mob-sakai/SoftMaskForUGUI/commit/87b60600b5608a54a58c2e8d23e73818dc15ca31))
* support uGUI 2.0.0 ([725278a](https://github.com/mob-sakai/SoftMaskForUGUI/commit/725278a6b3fbe57b9bfc78d929a03b97a1405eb3))


### BREAKING CHANGES

* The implementation way of the 'SoftMaskable' shader has been changed. Please refer to the "Migrating from v1 to v2" section in the Readme for details.

## [1.0.2](https://github.com/mob-sakai/SoftMaskForUGUI/compare/1.0.1...1.0.2) (2022-05-15)


### Bug Fixes

* support TextMeshPro v2 or later ([c85409e](https://github.com/mob-sakai/SoftMaskForUGUI/commit/c85409e56ff09607244061c59518f5d1f460a918))

## [1.0.1](https://github.com/mob-sakai/SoftMaskForUGUI/compare/1.0.0...1.0.1) (2022-05-15)


### Bug Fixes

* applied a workaround to fix a Microsoft HLSL compiler issue ([50c41f2](https://github.com/mob-sakai/SoftMaskForUGUI/commit/50c41f29ccc9b70acdd7f15490debd8eacf5a102)), closes [#131](https://github.com/mob-sakai/SoftMaskForUGUI/issues/131)
* fixed shader compilation in some platforms ([40b450b](https://github.com/mob-sakai/SoftMaskForUGUI/commit/40b450ba24e77c34c97fe8411f7b0b1dd103d487))

# [1.0.0](https://github.com/mob-sakai/SoftMaskForUGUI/compare/0.9.1...1.0.0) (2021-02-24)


### Features

* release 1.0.0 ([127b455](https://github.com/mob-sakai/SoftMaskForUGUI/commit/127b455f38889dfe9a1c6ae5449d2c537d2a4d78))


### BREAKING CHANGES

* release 1.0.0

# [1.0.0-preview.14](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v1.0.0-preview.13...v1.0.0-preview.14) (2020-10-08)


### Bug Fixes

* incorrect behavior when a world space canvas and an overlay canvas are enabled together ([a6e82fa](https://github.com/mob-sakai/SoftMaskForUGUI/commit/a6e82fa2a7baa06aa4e1fb7e4a8099c5e1039d67)), closes [#107](https://github.com/mob-sakai/SoftMaskForUGUI/issues/107)

# [1.0.0-preview.13](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v1.0.0-preview.12...v1.0.0-preview.13) (2020-10-01)


### Bug Fixes

* screen resolution in full screen mode is incorrect ([39e3084](https://github.com/mob-sakai/SoftMaskForUGUI/commit/39e3084ec840293f2ad461f50d51eeafe66cbebf))

# [1.0.0-preview.12](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v1.0.0-preview.11...v1.0.0-preview.12) (2020-09-28)


### Bug Fixes

* clickable area inverted on Metal ([3c189ff](https://github.com/mob-sakai/SoftMaskForUGUI/commit/3c189ffed61baa6806aadc6ff89c41b9102491b5))
* in Unity 2018.x, sample import failed on Windows ([207ea9c](https://github.com/mob-sakai/SoftMaskForUGUI/commit/207ea9c1dc4117ab6c00e70290d5f7651fa906d8))

# [1.0.0-preview.11](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v1.0.0-preview.10...v1.0.0-preview.11) (2020-09-27)


### Bug Fixes

* an exception is thrown when the game view is inactive ([97e5a21](https://github.com/mob-sakai/SoftMaskForUGUI/commit/97e5a21b784ae9081aec8f17603355eef7e8b2b9)), closes [#104](https://github.com/mob-sakai/SoftMaskForUGUI/issues/104)
* graphic.materialForRendering always returns different material ([3f6acec](https://github.com/mob-sakai/SoftMaskForUGUI/commit/3f6acec67b3c0467670eb8f4b10928fa8724e082)), closes [#103](https://github.com/mob-sakai/SoftMaskForUGUI/issues/103)

# [1.0.0-preview.10](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v1.0.0-preview.9...v1.0.0-preview.10) (2020-09-14)


### Bug Fixes

* if the package was installed via openupm, an unintended directory 'Samples' was included ([d8fd47a](https://github.com/mob-sakai/SoftMaskForUGUI/commit/d8fd47aadbbb99fc6ebd830820abaee4ab1d9cf2)), closes [#102](https://github.com/mob-sakai/SoftMaskForUGUI/issues/102)

# [1.0.0-preview.9](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v1.0.0-preview.8...v1.0.0-preview.9) (2020-09-08)


### Bug Fixes

* TextMeshPro not work TMP_SubMeshUI ([56995e9](https://github.com/mob-sakai/SoftMaskForUGUI/commit/56995e9f81218a40cfd777f51b9f11a86775a131)), closes [#94](https://github.com/mob-sakai/SoftMaskForUGUI/issues/94)

# [1.0.0-preview.8](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v1.0.0-preview.7...v1.0.0-preview.8) (2020-09-08)


### Features

* if the 'UIMask' sprite is specified as the source image, it is suggested to use another image ([ea904db](https://github.com/mob-sakai/SoftMaskForUGUI/commit/ea904dbe3afd9f18eab0d449bd08bf78375fd53d)), closes [#82](https://github.com/mob-sakai/SoftMaskForUGUI/issues/82)
* option to disable softening completely ([dedd847](https://github.com/mob-sakai/SoftMaskForUGUI/commit/dedd847fd0c5faa5094a08293600cbb8aa4b6456)), closes [#98](https://github.com/mob-sakai/SoftMaskForUGUI/issues/98)
* use the stencil buffer outside the scene view canvas for editing ([dbab85c](https://github.com/mob-sakai/SoftMaskForUGUI/commit/dbab85c0f0bd8a58b8ab09306bed351ad1cf6375)), closes [#100](https://github.com/mob-sakai/SoftMaskForUGUI/issues/100)

# [1.0.0-preview.7](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v1.0.0-preview.6...v1.0.0-preview.7) (2020-08-17)


### Features

* memoryless mode (depth) ([8cf202f](https://github.com/mob-sakai/SoftMaskForUGUI/commit/8cf202f958be902d34994f3c07082f893f9b455b))

# [1.0.0-preview.6](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v1.0.0-preview.5...v1.0.0-preview.6) (2020-08-17)


### Bug Fixes

* Always Include TextMeshPro Shaders ([7f24280](https://github.com/mob-sakai/SoftMaskForUGUI/commit/7f24280b9586f5ccc50d233d0eb4241bb8cd9b10)), closes [#99](https://github.com/mob-sakai/SoftMaskForUGUI/issues/99)

# [1.0.0-preview.5](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v1.0.0-preview.4...v1.0.0-preview.5) (2020-06-09)


### Bug Fixes

* screen space overlay bug with game window resize ([91b7788](https://github.com/mob-sakai/SoftMaskForUGUI/commit/91b77885bc5f60cb59e8081009982a4635f9245c)), closes [#93](https://github.com/mob-sakai/SoftMaskForUGUI/issues/93)

# [1.0.0-preview.4](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v1.0.0-preview.3...v1.0.0-preview.4) (2020-06-07)


### Bug Fixes

* fix the tooltip text ([f38f034](https://github.com/mob-sakai/SoftMaskForUGUI/commit/f38f0341480cfd8eda6bea3e488c7ae052b89924))


### Features

* add a new option to ignore stencil if it is not needed as a mask ([9edcb2d](https://github.com/mob-sakai/SoftMaskForUGUI/commit/9edcb2d22be25c285b0a7f853314884c3e9df499))
* add a new public property 'useStencil' ([09dbaad](https://github.com/mob-sakai/SoftMaskForUGUI/commit/09dbaad203fa9eaaa16abf683add04267f82439d))
* improve performance ([018ec78](https://github.com/mob-sakai/SoftMaskForUGUI/commit/018ec78c759745907c2e06d3ab09167939bdb05a))

# [1.0.0-preview.3](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v1.0.0-preview.2...v1.0.0-preview.3) (2020-06-04)


### Bug Fixes

* outside interaction doesn't work when the RaycastFilter option is off ([1935650](https://github.com/mob-sakai/SoftMaskForUGUI/commit/19356500c5b777aa5857fa5176fc09f0fd7951cb))


### Features

* Add ignore self graphic option. ([91c0099](https://github.com/mob-sakai/SoftMaskForUGUI/commit/91c00993b9afbdda0386b8e426e181f8f31618b9))
* TextMeshPro support ([5b0906b](https://github.com/mob-sakai/SoftMaskForUGUI/commit/5b0906b6086193bc8f62fa174955c9df901ef3f0))


### BREAKING CHANGES

* TextMeshPro support is now an option.
If a shader or material has errors after a version upgrade, you will need to import the asset.
Please see the README for more information.

# [1.0.0-preview.2](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v1.0.0-preview.1...v1.0.0-preview.2) (2020-05-13)


### Bug Fixes

* compile error ([56b1791](https://github.com/mob-sakai/SoftMaskForUGUI/commit/56b1791bd3995ba387b1dc866ea68325b56a8830)), closes [#87](https://github.com/mob-sakai/SoftMaskForUGUI/issues/87)

# [1.0.0-preview.1](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v0.10.0-preview.3...v1.0.0-preview.1) (2020-05-11)


### Bug Fixes

* Unintentional material destruction ([bf17b19](https://github.com/mob-sakai/SoftMaskForUGUI/commit/bf17b19ef29ea35b54cbaf5473611ad58136540a))


### change

* change namespace ([0347b04](https://github.com/mob-sakai/SoftMaskForUGUI/commit/0347b04fb70d970b3558ebb454ecd2dbbd3dfce0))


### Features

* support graphic connector ([3451521](https://github.com/mob-sakai/SoftMaskForUGUI/commit/34515216a39d69601595dffbbac1803da3a27379)), closes [#75](https://github.com/mob-sakai/SoftMaskForUGUI/issues/75) [#76](https://github.com/mob-sakai/SoftMaskForUGUI/issues/76) [#80](https://github.com/mob-sakai/SoftMaskForUGUI/issues/80)


### BREAKING CHANGES

* If your code contained the SoftMask API, it would fail to compile. Please change the namespace from `Coffee.UIExtensions` to `Coffee.UISoftMask`.
* The name of the custom SoftMaskable shader must be changed. For more information, see the ‘Support soft masks with your custom shaders’ section of the README.

# [0.10.0-preview.3](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v0.10.0-preview.2...v0.10.0-preview.3) (2020-05-08)


### Bug Fixes

* compile error on build ([e7ff660](https://github.com/mob-sakai/SoftMaskForUGUI/commit/e7ff660aa5539ae0a2fb88b49582a5b7f7c11c45)), closes [#84](https://github.com/mob-sakai/SoftMaskForUGUI/issues/84)
* visual bug with ScreenSpaceCamera canvas on editor ([ec9ac44](https://github.com/mob-sakai/SoftMaskForUGUI/commit/ec9ac4481f9b1ecaf4044743efe02533e7f1ff66))

# [0.10.0-preview.2](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v0.10.0-preview.1...v0.10.0-preview.2) (2020-05-03)


### Bug Fixes

* visual bug with ScreenSpaceCamera canvas on editor ([482b967](https://github.com/mob-sakai/SoftMaskForUGUI/commit/482b96709b9dce680e48214df26c81b7e963dc09)), closes [#78](https://github.com/mob-sakai/SoftMaskForUGUI/issues/78)

# [0.10.0-preview.1](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v0.9.1...v0.10.0-preview.1) (2020-05-01)


### Bug Fixes

* softmask not working on PS4 ([b391c10](https://github.com/mob-sakai/SoftMaskForUGUI/commit/b391c103c57cdb3a848701ee5663b86b962031cd)), closes [#74](https://github.com/mob-sakai/SoftMaskForUGUI/issues/74)


### Features

* add sample importer ([d98a241](https://github.com/mob-sakai/SoftMaskForUGUI/commit/d98a241e78f81a92bf22181d94a7622c6a65b589))
* raycast filter is now optional feature ([3b42727](https://github.com/mob-sakai/SoftMaskForUGUI/commit/3b427274c9842c46dd90fa8845dc5156822b04e1)), closes [#73](https://github.com/mob-sakai/SoftMaskForUGUI/issues/73)

# Changelog

## [v0.9.1](https://github.com/mob-sakai/SoftMaskForUGUI/tree/v0.9.1) (2020-01-28)

[Full Changelog](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v0.9.0...v0.9.1)

**Implemented enhancements:**

- Add the parameter to control mask transparency [\#62](https://github.com/mob-sakai/SoftMaskForUGUI/pull/62) ([IIzzaya](https://github.com/IIzzaya))

**Fixed bugs:**

- Projection Matrix check always true when using world space canvas [\#67](https://github.com/mob-sakai/SoftMaskForUGUI/issues/67)
- Update softmask not working when canvas component was deactivated [\#66](https://github.com/mob-sakai/SoftMaskForUGUI/issues/66)
- Raycast coordinates are incorrect [\#52](https://github.com/mob-sakai/SoftMaskForUGUI/issues/52)

## [v0.9.0](https://github.com/mob-sakai/SoftMaskForUGUI/tree/v0.9.0) (2019-08-27)

[Full Changelog](https://github.com/mob-sakai/SoftMaskForUGUI/compare/v0.8.1...v0.9.0)

**Implemented enhancements:**

- Improved work in prefab view [\#55](https://github.com/mob-sakai/SoftMaskForUGUI/pull/55) ([ManeFunction](https://github.com/ManeFunction))

**Fixed bugs:**

- Prefab Mode in Play Mode is not supported [\#60](https://github.com/mob-sakai/SoftMaskForUGUI/issues/60)
- Missing .meta file [\#59](https://github.com/mob-sakai/SoftMaskForUGUI/issues/59)

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
