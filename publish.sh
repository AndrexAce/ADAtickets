#!/bin/bash

# Linux x64
dotnet publish ADAtickets.Installer.Desktop -c Release -r linux-x64
zip -j "adaticketsinstaller-linux-x64.zip" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/linux-x64/publish/ADAticketsInstaller" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/linux-x64/publish/libSkiaSharp.so" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/linux-x64/publish/libHarfBuzzSharp.so"

# Linux ARM64
dotnet publish ADAtickets.Installer.Desktop -c Release -r linux-arm64
zip -j "adaticketsinstaller-linux-arm64.zip" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/linux-arm64/publish/ADAticketsInstaller" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/linux-arm64/publish/libSkiaSharp.so" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/linux-arm64/publish/libHarfBuzzSharp.so"

# Windows x64
dotnet publish ADAtickets.Installer.Desktop -c Release -r win-x64
zip -j "adaticketsinstaller-win-x64.zip" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/win-x64/publish/ADAticketsInstaller.exe" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/win-x64/publish/av_libglesv2.dll" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/win-x64/publish/libSkiaSharp.dll" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/win-x64/publish/libHarfBuzzSharp.dll" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/win-x64/publish/sni.dll"

# Windows ARM64
dotnet publish ADAtickets.Installer.Desktop -c Release -r win-arm64
zip -j "adaticketsinstaller-win-arm64.zip" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/win-arm64/publish/ADAticketsInstaller.exe" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/win-arm64/publish/av_libglesv2.dll" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/win-arm64/publish/libSkiaSharp.dll" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/win-arm64/publish/libHarfBuzzSharp.dll" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/win-arm64/publish/sni.dll"

# MacOS x64
dotnet publish ADAtickets.Installer.Desktop -c Release -r osx-x64
zip -j "adaticketsinstaller-osx-x64.zip" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/osx-x64/publish/ADAticketsInstaller" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/osx-x64/publish/libAvaloniaNative.dylib" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/osx-x64/publish/libSkiaSharp.dylib" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/osx-x64/publish/libHarfBuzzSharp.dylib"

# MacOS ARM64
dotnet publish ADAtickets.Installer.Desktop -c Release -r osx-arm64
zip -j "adaticketsinstaller-osx-arm64.zip" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/osx-arm64/publish/ADAticketsInstaller" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/osx-x64/publish/libAvaloniaNative.dylib" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/osx-arm64/publish/libSkiaSharp.dylib" \
  "ADAtickets.Installer.Desktop/bin/Release/net10.0/osx-arm64/publish/libHarfBuzzSharp.dylib"