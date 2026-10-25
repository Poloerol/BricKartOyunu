# BricKartOyunu: YEREL proje klasoru -> GitHub (Poloerol/BricKartOyunu) senkronizasyonu.
# - Mevcut (dis) git deponuza DOKUNMAZ.
# - GitHub'i gecici klasore klonlar, yerel dosyalari uzerine kopyalar (yerel esas alinir),
#   farki gosterir, onayiniz olmadan commit/push YAPMAZ. Force push yoktur.
$ErrorActionPreference = 'Stop'

$yerel  = "$HOME\source\repos\Poloerol\BricKartOyunu\BricKartOyunu"
$uzak   = "https://github.com/Poloerol/BricKartOyunu.git"
$gecici = Join-Path $env:TEMP "BricKartOyunu_sync"

if (-not (Test-Path $yerel)) { throw "Yerel proje bulunamadi: $yerel" }
if (Test-Path $gecici) { Remove-Item $gecici -Recurse -Force }

Write-Host "GitHub klonlaniyor..." -ForegroundColor Cyan
git clone $uzak $gecici
if ($LASTEXITCODE -ne 0) { throw "Klonlama basarisiz (giris/yetki?). 'git config --global credential.helper manager' ile giris yapin." }

Write-Host "Yerel dosyalar kopyalaniyor (bin/obj/.vs/.git haric)..." -ForegroundColor Cyan
robocopy $yerel $gecici /E /XD .git .vs bin obj packages TestResults /XF *.user *.suo *.rar /NFL /NDL /NJH /NJS /NP | Out-Null
# robocopy 0-7 arasi donus kodlari basaridir

Set-Location $gecici

# Yerelde olmayan ama GitHub'da duran dosyalar
$eksik = git ls-files | Where-Object { -not (Test-Path (Join-Path $yerel $_)) }
if ($eksik) {
    Write-Host "`nGitHub'da var, yerelde YOK ($($eksik.Count) dosya):" -ForegroundColor Yellow
    $eksik | Select-Object -First 30 | ForEach-Object { Write-Host "  $_" }
    $c = Read-Host "Bunlar GitHub'dan da silinsin mi? (e/h)"
    if ($c -eq 'e') { $eksik | ForEach-Object { git rm -q -- $_ } }
}

git add -A
Write-Host "`n=== Degisiklik ozeti ===" -ForegroundColor Cyan
git status --short | Select-Object -First 80
git diff --cached --stat | Select-Object -Last 1

if (git diff --cached --quiet) { Write-Host "Fark yok, GitHub zaten guncel." -ForegroundColor Green; exit 0 }

$mesaj = Read-Host "`nCommit mesaji (bos = varsayilan)"
if ([string]::IsNullOrWhiteSpace($mesaj)) { $mesaj = "Ihale: IhaleKoruma, TemelMantik yeniden yazimi, oyun zekasi ve deklarasyon duzeltmeleri" }
$onay = Read-Host "Commit'lenip GitHub'a gonderilsin mi? (e/h)"
if ($onay -ne 'e') { Write-Host "Iptal edildi. Gecici klasor: $gecici"; exit 0 }

git commit -m $mesaj
git push origin HEAD
if ($LASTEXITCODE -eq 0) { Write-Host "Tamam: yerel dosyalar GitHub ile esitlendi." -ForegroundColor Green }
