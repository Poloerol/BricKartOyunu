# Durum (son güncelleme: 2026-11-17)

## Şu an
- Dal `main`; son commit `9e8a44e` (push edildi). Başka bir Claude oturumu aynı motorda çalışabiliyor: ölçüme ya da motor düzenlemesine başlamadan önce `git log -5` ve `git status` bak.
- Testler: ihale testlerinin tamamı geçiyor (267 test, 0 başarısız; `TestRunner` içinden çalışır).
- Derleme: `dotnet build BricKartOyunu.sln` ya da MSBuild. Uygulama açıkken `bin\Debug\BricKartOyunu.exe` kilitlenir; bu derleme hatası değil kopyalama hatasıdır.

## Ölçüm yöntemi (Motor Karnesi, arayüzsüz)
- Araçlar repoda: `Tools/Olcum/` (README'ye bak). `Tools/Olcum/derle.sh` ana projeyi ayrı klasöre derler ve araçları yapar: `Taban` (taban çizgisi), `Measure` (tabana göre değişen bordların sapma farkı), `PbnRun` (PBN bordlarını motorla oynat), `Probe`, `Seeds`, `Attr`.
- Kural: her değişikliği 30.000 bordda ölç, üç 10.000'lik kısımda aynı yönde olmasını doğrula. DD bakış açısı iyimserdir; masada gerçek kazanç biraz daha az olabilir.
- Güncel motor ortalama sapması: 5,1863 (30.000 bord, 17 Kasım tabanı; başlangıç 5,30); 18 Kasım düzeltmelerinden sonra yaklaşık 5,16. Temel Mantık yedeği Pas dışı tekliflerin yaklaşık %35'ini veriyor.
- Yedek payı analizi: `Tools/Olcum/kume.awk` (bid kümeleri) ve `pas_sizinti.awk` (yedeğin son pasları). En büyük sızıntı: yedeğin Pas'ları (birleşik 25+ HP, parsiyel kontrat; ~2000 bord, ~7000 IMP fazla sapma).

## Oynama karnesi (2026-11-18, ilk ölçüm: 299 bord, boards 1-300)
- Araç: `Class/DD/OynamaKarnesi.cs`, arayüzde Eylemler > Oynama Karnesi…, arayüzsüz `Tools/Olcum/OynamaOlc.exe`. İhale motorunun kontratını kart oynama motoru dört elde oynar; fark = deklaran tarafının gerçek lövesi − çift dummy lövesi.
- PIMC açık (900 ms): ortalama |fark| 0,90 löve; DD ile aynı %38 (fark −1: %24, +1: %19, ±2+: %17); deklaran eksik 96 / savunma eksik 89 bord; ortalama fark +0,03 (iki taraf dengeli).
- PIMC kapalı (yalnız kurallar): ortalama |fark| 0,94; DD ile aynı %40; ortalama fark −0,10 (deklaran tarafı biraz geride). PIMC'nin net katkısı küçük (~0,13 löve/bord, esas olarak deklaran oyununda).
- Zayıf nokta NT kontratlar: ortalama |fark| 1,28 (PIMC açık) / 1,53 (kapalı); suit kontratlarda 0,70-0,79. NT'de fark +0,27: savunma (özellikle atak) DD'nin gerisinde kalıyor.
- Yorum: DD ile aynı olma oranı %40 civarı beklenen aralıkta (motor saklı elleri görmüyor); asıl ölçü, ayarlar değişince |fark| ve ortalama IMP'nin düşmesi. Sıradaki iş: NT atak/savunma kuralları (`OyunZekasi` atak seçimi) ve oyun sonrası analizle hamle bazlı hataları bulmak.

## Son yapılanlar (Kasım 2026)
- 18 Kasım, yedek payı azaltma (30.000 bord, üç 10.000'likte de aynı yönde; toplam ~−718 IMP): (1) `CevapciIkinciTur`: açıcının 2x tekrarından sonra 12-13 HP ile Pas/2NT yerine 3NT (−310); (2) `OyunDavetiKabul`: 1M-1NT-3M sonrası 7+ HP ile 4M (−54); (3) yeni `Müdahale Oyun Cevabı`: zayıf iki açılışına ortağın 3 seviye minör müdahalesinden sonra 9+ HP ile 3NT (durdurucu varsa) / 5m (3+ destek) (−352). Denenip eklenmeyenler: 2 seviye majör müdahalesine 4M (−8, anlamsız), atlamalı rebid eşiği 17 (16: −668, 18: −692, 17: −718 en iyi). Testler: `TestRunner_YedekAciklari` (YA1-YA10), M1 beklentisi 3NT; toplam 862 test geçti.
- Majör oyun eşiği 28,5 → 24,5, davet 23,5 (`TemelMantik.HedefFit`): 30.000 bordda yaklaşık −3.484 IMP.
- Yeni konvansiyonlar: `Negatif Kontra Devam` (açıcı ortağın negatif kontrasını pas geçmez, −780 IMP), `Takeout Double Yükseltme` (kontra yapan ortağın majör cevabını yükseltir).
- `TakeoutDouble`: rakip renginde void ve 6+ kartlı yan renk (17 HP altı) varsa kontra yok.
- `PreemptCevap` / `ZayifIkiCevap`: güçlü ellerde (17+ HP) oyun/slam seviyesi; `IhaleKoruma` 3NT/slam/4.-5. seviye eşikleri ayarlandı.
- 7'li, dört büyük honörlü renkte atlamalı ikinci teklif değerine +1 (bord 9; genel ortalamaya etkisi nötr).
- Karne dışa aktarımı simgesiz (S/H/D/C); Çift Dummy Arama formu; PBN simultane bordlarını motorla karşılaştırma.
- **Akış Şeması** (Deklarasyon penceresindeki düğme): Bridge Baron `.fv` dosyalarını okuyan pencere (adım adım, tüm şema ağacı, açıklama). Dosyalar `D:\Games\Flowcharts` altından okunur (yoksa klasör sorulur). Türkçe çeviri sözlüğü `Resources/AkisSemasi_TR.tsv` YEREL kalır, git'e girmez (.gitignore; Bridge Baron metinlerinden türediği için). Dosya yoksa metinler İngilizce görünür. Görev çubuğunda ayrı düğmesi yok.
- Tek pencere mimarisi (BricOyna, AnaSayfa içine gömülü) tamamlandı; görev çubuğunda tek düğme.

## Açık noktalar
- **Denenmedi:** `BricOyna.BozukResimleriTemizle` (commit `ef94e34`): ihale penceresi açılırken `SetGameControlsEnabled` içinde "Parametre geçerli değil" `ArgumentException`'ı için koruma. Hata WndProc içinde oluştuğundan eski try/catch yakalamıyordu. Kullanıcı uygulamada deneyecek; hâlâ çıkarsa "Çağrı Yığınını Göster" çıktısı gerekli.
- Bord 3 (Doğu 1♠ sonra karoda void ile 2NT) ve Bord 12 (3♠ yükseltmesi sonrası 4♠ batıyor) düzeltilmedi.
- Temel Mantık yedeğinin payı yüksek: en sık düştüğü durumlar `Attr` ile ölçülüp tablo/konvansiyonla kapatılabilir.
- 4-4 majörde ♦'de uyum yoksa motor 1♠ diyor (♥ önceliği yalnızca minör uyum varken uygulanıyor).
- Zayıf 2♦ için anlaşma seçeneği (forcing değil / bir tur forcing / oyun forcing) istenirse eklenecek.
- Briç Maçı / Konvansiyon Pratiği / Dağılım Deposu akışları tek pencere geçişinden sonra kullanıcı tarafından tam denenmedi.
- `ManuelGirisAl` / `ElGirisiForm` kodu ölü duruyor; silinebilir. PBN'de `[Vulnerable]` okunmuyor (zon bord numarasından hesaplanıyor).
- Eski dallar: `claude/bridge-game-analysis-58f887`, `claude/lightshot-gorsel-kontrolu-2e1b91`, `origin/feature/1c-cevap-sistemi`, `origin/fix/deklaran-position-swap`; gerekmeyenler temizlenebilir.
- `ff4f89d` commit'inde Türkçe çeviri sözlüğü geçmişte duruyor (sonra depodan çıkarıldı). Repo herkese açıksa geçmişten tamamen silmek ayrı karardır (geçmişi yeniden yazmak geri alınamaz).

## Fikirler (sıralı)
1. Temel Mantık yedeğinin payını azalt (ölç, kümeleri bul, konvansiyon/tablo ekle).
2. Oynama karnesi: kart oynamanın double dummy'ye göre kaç IMP kaybettirdiğini ölç.
3. Oyun sonrası analiz (her hamle DD'ye göre) ve teklif açıklamaları (alert).
4. Kayıt/geçmiş ve PBN dışa aktarma; duplicate maç akışı.
5. Test altyapısı: MSTest/NUnit + GitHub Actions.
