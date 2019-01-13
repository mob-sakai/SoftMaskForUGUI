#!/bin/bash -e

# NOTE: Run the following command at the prompt
#   bash <(curl -sL 'https://gist.github.com/mob-sakai/a883999a32dd8b1927639e46b3cd6801/raw/unity_release.sh')
# NOTE: Set an environment variable `CHANGELOG_GITHUB_TOKEN` by running the following command at the prompt, or by adding it to your shell profile (e.g., ~/.bash_profile or ~/.zshrc):
#   export CHANGELOG_GITHUB_TOKEN="«your-40-digit-github-token»"



# 1. << Input release version >>
echo -e ">> Start Github Release:"
PACKAGE_NAME=`node -pe 'require("./package.json").name'`
echo -e ">> Package name: ${PACKAGE_NAME}"
CURRENT_VERSION=`grep -o -e "\"version\".*$" package.json | sed -e "s/\"version\": \"\(.*\)\".*$/\1/"`

read -p "[? (1/8) Input release version (for current: ${CURRENT_VERSION}): " RELEASE_VERSION
[ -z "${RELEASE_VERSION}" ] && exit

read -p "[? Are the issues on this release closed all? (y/N):" yn
case "$yn" in [yY]*) ;; *) exit ;; esac

read -p "[? Is package editor only? (y/N):" yn
case "$yn" in [yY]*) EDITOR_ONLY=true;; *) ;; esac

read -p "[? Is package for UnityPackageManager? (y/N):" yn
case "$yn" in [yY]*) UNITY_PACKAGE_MANAGER=true;; *) ;; esac

echo -e ">> OK"



# 2. << Update version in package.json >>
echo -e "\n>> (2/8) Update version... package.json"
[ -L package.json ] && PKG_JSON_PATH=`readlink package.json` || PKG_JSON_PATH=package.json
git checkout -B release develop
sed -i -e "s/\"version\": \(.*\)/\"version\": \"${RELEASE_VERSION}\",/g" "${PKG_JSON_PATH}"
rm "${PKG_JSON_PATH}-e"
echo -e ">> OK"



# 3. << Check unity editor is exist and no compile error >>
UNITY_VER=`sed -e "s/m_EditorVersion: \(.*\)/\1/g" ProjectSettings/ProjectVersion.txt`
UNITY_EDITOR="/Applications/Unity/Hub/Editor/${UNITY_VER}/Unity.app/Contents/MacOS/Unity"
UNITY_LOG="unity.log"
UNITY_ARGS="-quit -batchmode -projectPath `pwd` -logFile $UNITY_LOG"
UNITY_PACKAGE_SRC=`node -pe 'require("./package.json").src'`
UNITY_PACKAGE_NAME="${PACKAGE_NAME}_${RELEASE_VERSION}.unitypackage"
echo -e "\n>> (3/8) Check exporting package is available..."
echo -e "Version: $UNITY_VER ($UNITY_EDITOR)"
echo -e "Package Source: $UNITY_PACKAGE_SRC"

#   3-1. Is src directory exist?
[ ! -d "$UNITY_PACKAGE_SRC" ] && echo -e "\n>> Error : $UNITY_PACKAGE_SRC is not exist." && exit

#   3-2. Is runtime compile successfully?
set +e
if [ "$EDITOR_ONLY" != "true" ]; then
  echo -e "\n>> compile for runtime..."
  "$UNITY_EDITOR" $UNITY_ARGS -buildOSX64Player `pwd`/build.app
  [ $? != 0 ] && echo -e "\n>> Error : \n`cat $UNITY_LOG | grep -E ': error CS|Fatal Error'`" && exit
  echo -e ">> OK"
fi

#   3-3. Is exporting package successfully?
echo -e "\n>> Pre export package..."
"$UNITY_EDITOR" $UNITY_ARGS -exportpackage $UNITY_PACKAGE_SRC $UNITY_PACKAGE_NAME
[ $? != 0 ] && echo -e "\n>> Error : \n`cat $UNITY_LOG | grep -E ': error CS|Fatal Error'`" && exit
echo -e ">> OK"
set -e



# 4. << Generate change log >>
CHANGELOG_GENERATOR_ARG=`grep -o -e ".*git\"$" package.json | sed -e "s/^.*\/\([^\/]*\)\/\([^\/]*\).git.*$/--user \1 --project \2/"`
CHANGELOG_GENERATOR_ARG="--future-release ${RELEASE_VERSION} ${CHANGELOG_GENERATOR_ARG}"
echo -e "\n>> (4/8) Generate change log... ${CHANGELOG_GENERATOR_ARG}"
github_changelog_generator ${CHANGELOG_GENERATOR_ARG}

[ -L CHANGELOG.md ] && git diff -- `readlink CHANGELOG.md` || git diff -- CHANGELOG.md
read -p "[? Is the change log correct? (y/N):" yn
case "$yn" in [yY]*) ;; *) exit ;; esac
echo -e ">> OK"



# 5. << Export unitypackage >>
echo -e "\n>> (5/8) Export unitypackage..."
set +e
"$UNITY_EDITOR" $UNITY_ARGS -exportpackage $UNITY_PACKAGE_SRC $UNITY_PACKAGE_NAME
[ $? != 0 ] && echo -e "\n>> Error : \n`cat $UNITY_LOG | grep -E ': error CS|Fatal Error'`" && exit
set -e
echo -e ">> OK"



# 6. << Commit release files >>
echo -e "\n>> (6/8) Commit release files..."
[ -L CHANGELOG.md ] && git add -- "`readlink CHANGELOG.md`" || git add -- CHANGELOG.md
[ -L package.json ] && git add -- "`readlink package.json`" || git add -- package.json
git commit -m "update change log"
echo -e ">> OK"



#  7. << Split for upm >>
if [ "$UNITY_PACKAGE_MANAGER" == "true" ]; then
  echo -e "\n>> Split for upm..."
  git subtree split --prefix="$UNITY_PACKAGE_SRC" --branch upm
  git push origin upm
fi



# 7. << Merge and push master and develop branch >>
echo -e "\n>> (7/8) Merge and push..."
git checkout master
git merge --no-ff release -m "release $RELEASE_VERSION"
git branch -D release
git push origin master
git checkout develop
git merge --ff master
git push origin develop
echo -e ">> OK"



# 8. << Upload unitypackage and release on Github >>
echo -e "\n>> (8/8) Releasing..."
[ "$UNITY_PACKAGE_MANAGER" == "true" ] && git checkout upm -f
gh-release --assets $UNITY_PACKAGE_NAME --name $RELEASE_VERSION --tag_name $RELEASE_VERSION
echo -e ">> OK"




echo -e "\n\n>> $PACKAGE_NAME $RELEASE_VERSION has been successfully released!\n"
