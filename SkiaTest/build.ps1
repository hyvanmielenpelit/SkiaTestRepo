$vsPath = & "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath
Import-Module "$vsPath\Common7\Tools\Microsoft.VisualStudio.DevShell.dll"
Enter-VsDevShell -VsInstallPath $vsPath -SkipAutomaticLocation -DevCmdArguments "-arch=x64"

$gitHash = git -C "C:\repos\SkiaSharp\externals\skia" rev-parse --short HEAD
cl /EHsc /MT /std:c++17 benchmark.cpp /D"SKIA_GIT_HASH=\`"$gitHash\`"" /I"C:\repos\SkiaSharp\externals\skia" /link /LIBPATH:"C:\repos\SkiaSharp\externals\skia\out\windows\x64" skia.lib opengl32.lib gdi32.lib user32.lib

