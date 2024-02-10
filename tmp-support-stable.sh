#!/bin/bash -e

# Switch the shaders for TextMeshPro (stable)
target="TextMeshPro Support"
sample="TextMeshPro Support"
[ -d "Assets/$target" ] && rm -rf "Assets/$target"
ln -s "../Packages/src/Samples~/$sample~" Assets
mv "Assets/$sample~" "Assets/$target"