#!/bin/bash
# Kıyas özeti aracını derler: Tools/Kiyas/Ozet.exe
cd "$(dirname "$0")"
CSC="${CSC:-/c/Windows/Microsoft.NET/Framework64/v4.0.30319/csc.exe}"
"$CSC" //nologo //out:Ozet.exe Ozet.cs && echo "derlendi: Ozet.exe  →  ./Ozet.exe kiyas.tsv"
