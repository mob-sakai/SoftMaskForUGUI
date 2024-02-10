#!/bin/bash -e

# Switch the shaders for TextMeshPro (v3.2 or v4.0)
target="TextMeshPro Support"
sample="TextMeshPro Support v3.2 or v4.0"
[ -d "Assets/$target" ] && rm -rf "Assets/$target"
ln -s "../Packages/src/Samples~/$sample~" Assets
mv "Assets/$sample~" "Assets/$target"