#!/bin/bash
# Ölçüm araçlarını derler: önce ana projeyi AYRI bir klasöre (arayüz exe'sini kilitlemesin), sonra araçları.
# Kullanım:  Tools/Olcum/derle.sh            -> Tools/Olcum/out/ içinde Measure, Taban, Attr, Probe, Seeds, PbnRun, Testler
# Ortam değişkenleri (gerekirse): MSBUILD, CSC
set -e
KOK="$(cd "$(dirname "$0")/../.." && pwd)"
OUT="${OUT:-$KOK/Tools/Olcum/out}"
MSBUILD="${MSBUILD:-/c/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe}"
CSC="${CSC:-/c/Windows/Microsoft.NET/Framework64/v4.0.30319/csc.exe}"
mkdir -p "$OUT"
cd "$KOK"
"$MSBUILD" BricKartOyunu.csproj //p:Configuration=Debug //p:OutDir="$(cygpath -w "$OUT")\\" //v:q //nologo | grep -E "error" || true
cd "$OUT"
for arac in Measure Taban Attr Probe Seeds PbnRun Testler OynamaOlc AnalizOlc; do
  "$CSC" //nologo //platform:x86 //target:exe //out:$arac.exe //reference:BricKartOyunu.exe \
    //reference:System.Windows.Forms.dll //reference:System.Drawing.dll //reference:System.Core.dll \
    "$(cygpath -w "$KOK/Tools/Olcum/$arac.cs")"
  echo "derlendi: $arac.exe"
done
