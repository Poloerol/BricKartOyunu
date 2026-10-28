<#
  BricKartOyunu kurulum paketini (Setup.exe) üretir.
  Gerekenler:  Visual Studio (MSBuild)  +  Inno Setup 6   (https://jrsoftware.org/isdl.php  veya:  winget install JRSoftware.InnoSetup)
  Kullanım:    PowerShell'de:  .\KurulumuOlustur.ps1
  Çıktı:       Kurulum\Cikti\BricKartOyunu-Kurulum-<sürüm>.exe  (arkadaşlarına bunu gönder)
#>
$ErrorActionPreference = 'Stop'
$kurulumKlasoru = $PSScriptRoot
$projeKlasoru   = Split-Path $kurulumKlasoru -Parent
$csproj         = Join-Path $projeKlasoru 'BricKartOyunu.csproj'

# 1) MSBuild'i bul (Visual Studio ile gelir)
$msbuild = $null
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (Test-Path $vswhere) {
    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
}
if (-not $msbuild) { throw 'MSBuild bulunamadı. Visual Studio (".NET masaüstü geliştirme" iş yükü) kurulu olmalı.' }

# 2) Inno Setup derleyicisini bul
$iscc = @(
    (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'),
    (Join-Path $env:ProgramFiles 'Inno Setup 6\ISCC.exe'),
    (Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe')
) | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $iscc) { throw 'Inno Setup 6 bulunamadı. Kur:  winget install JRSoftware.InnoSetup   (veya https://jrsoftware.org/isdl.php)' }

# 3) Release derle (NuGet paketlerini de geri yükler)
Write-Host 'Release derleniyor...' -ForegroundColor Cyan
& $msbuild $csproj /restore /t:Rebuild /p:Configuration=Release /p:Platform=AnyCPU /v:minimal
if ($LASTEXITCODE -ne 0) { throw 'Derleme başarısız.' }
if (-not (Test-Path (Join-Path $projeKlasoru 'bin\Release\BricKartOyunu.exe'))) { throw 'bin\Release\BricKartOyunu.exe oluşmadı.' }

# 4) Kurulum paketini üret
Write-Host 'Kurulum paketi hazırlanıyor...' -ForegroundColor Cyan
& $iscc (Join-Path $kurulumKlasoru 'BricKartOyunu.iss')
if ($LASTEXITCODE -ne 0) { throw 'Inno Setup derlemesi başarısız.' }

$cikti = Get-ChildItem (Join-Path $kurulumKlasoru 'Cikti') -Filter '*.exe' | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Write-Host ''
Write-Host ('Hazır: ' + $cikti.FullName) -ForegroundColor Green
Write-Host 'Bu dosyayı arkadaşlarına gönder. (Windows "bilinmeyen yayıncı" uyarısı çıkarsa: Ek bilgi > Yine de çalıştır.)'
