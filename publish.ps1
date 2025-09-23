# Linux x64
dotnet publish ADAtickets.Installer.Desktop -c Release -r linux-x64
$files_linux_x64 = @(
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\linux-x64\publish\ADAticketsInstaller",
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\linux-x64\publish\libSkiaSharp.so",
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\linux-x64\publish\libHarfBuzzSharp.so"
)
Compress-Archive -Path $files_linux_x64 -DestinationPath "adaticketsinstaller-linux-x64.zip" -Force

# Linux ARM64
dotnet publish ADAtickets.Installer.Desktop -c Release -r linux-arm64
$files_linux_arm64 = @(
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\linux-arm64\publish\ADAticketsInstaller",
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\linux-arm64\publish\libSkiaSharp.so",
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\linux-arm64\publish\libHarfBuzzSharp.so"
)
Compress-Archive -Path $files_linux_arm64 -DestinationPath "adaticketsinstaller-linux-arm64.zip" -Force

# Windows x64
dotnet publish ADAtickets.Installer.Desktop -c Release -r win-x64
$files_win_x64 = @(
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\win-x64\publish\ADAticketsInstaller.exe",
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\win-x64\publish\av_libglesv2.dll",
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\win-x64\publish\libSkiaSharp.dll",
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\win-x64\publish\libHarfBuzzSharp.dll",
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\win-x64\publish\sni.dll"
)
Compress-Archive -Path $files_win_x64 -DestinationPath "adaticketsinstaller-win-x64.zip" -Force

# Windows ARM64
dotnet publish ADAtickets.Installer.Desktop -c Release -r win-arm64
$files_win_arm64 = @(
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\win-arm64\publish\ADAticketsInstaller.exe",
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\win-arm64\publish\av_libglesv2.dll",
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\win-arm64\publish\libSkiaSharp.dll",
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\win-arm64\publish\libHarfBuzzSharp.dll",
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\win-arm64\publish\sni.dll"
)
Compress-Archive -Path $files_win_arm64 -DestinationPath "adaticketsinstaller-win-arm64.zip" -Force

# MacOS x64
dotnet publish ADAtickets.Installer.Desktop -c Release -r osx-x64
$files_osx_x64 = @(
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\osx-x64\publish\ADAticketsInstaller",
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\osx-x64\publish\libAvaloniaNative.dylib",
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\osx-x64\publish\libSkiaSharp.dylib",
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\osx-x64\publish\libHarfBuzzSharp.dylib"
)
Compress-Archive -Path $files_osx_x64 -DestinationPath "adaticketsinstaller-osx-x64.zip" -Force

# MacOS ARM64
dotnet publish ADAtickets.Installer.Desktop -c Release -r osx-arm64
$files_osx_arm64 = @(
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\osx-arm64\publish\ADAticketsInstaller",
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\osx-arm64\publish\libAvaloniaNative.dylib",
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\osx-arm64\publish\libSkiaSharp.dylib",
    "ADAtickets.Installer.Desktop\bin\Release\net9.0\osx-arm64\publish\libHarfBuzzSharp.dylib"
)
Compress-Archive -Path $files_osx_arm64 -DestinationPath "adaticketsinstaller-osx-arm64.zip" -Force