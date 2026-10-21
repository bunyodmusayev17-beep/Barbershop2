#!/bin/sh
# Android ilova "Knyaz Barber" (Capacitor): sayt build → android'ga ko'chirish → imzolangan APK va AAB.
# Git Bash'da: ./scripts/build-android.sh <versionCode> [versionName]     masalan: ./scripts/build-android.sh 2 1.1
#   APK — telefonga to'g'ridan-to'g'ri o'rnatish uchun, AAB — Play Market'ga yuklash uchun.
# Talab: D:/Coding/android-tools ichida jdk21/ va sdk/; imzo: keys/knyazbarber-signing.properties (git'da yo'q).
set -eu
CODE=${1:?"versionCode kerak (har Play yuklashda oshiriladi): ./scripts/build-android.sh 1"}
NAME=${2:-1.0.$CODE}
TOOLS=${ANDROID_TOOLS:-D:/Coding/android-tools}
cd "$(dirname "$0")/.."

JAVA_HOME="$TOOLS/jdk21/$(ls "$TOOLS/jdk21" | head -1)"
ANDROID_HOME="$TOOLS/sdk"
export JAVA_HOME ANDROID_HOME
PATH="$JAVA_HOME/bin:$PATH"

(cd frontend && npm run build && npx cap sync android)
(cd frontend/android && ./gradlew.bat assembleRelease bundleRelease -PversionCode="$CODE" -PversionName="$NAME" --no-daemon)

mkdir -p android-release
cp frontend/android/app/build/outputs/apk/release/app-release.apk "android-release/knyaz-barber-$NAME.apk"
cp frontend/android/app/build/outputs/bundle/release/app-release.aab "android-release/knyaz-barber-$NAME.aab"
ls -la android-release
