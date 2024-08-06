# CenturionOverlay

## 前提

- [OverlayPlugin](https://github.com/OverlayPlugin/OverlayPlugin) を併設してある

```tree
+ CenturionOverlay  <-- parent directory
  + Centurion       <-- this directory
+ OverlayPlugin     <-- cloned and pulled up to date.
```

## 初期構築

1. 以下のファイルを OverlayPlugin からコピー

   - .editorconfig

2. dotnet コマンドでプロジェクトとソリューションを作成

```powershell
> dotnet new classlib --name Centurion
> dotnet new sln --name Centurion
> dotnet sln add .\Centurion\Centurion.csproj
```

3. Centurion.csproj を AddonExample.csprojのTargetFrameworkをnet48に変更
