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

## Yedek mantık payı azaltma, 2. tur (2026-11-18 gece; yeni taban 5,16'lık motor, 30.000 bord, üç kısımda aynı yönde, toplam −381 IMP)
- `MudahaleOyunCevap` genişletildi: (a) ortağın 3 seviye majör müdahalesine zayıf iki sonrası 3+ destek ve 8+ HP ile 4M (−82); (b) zayıf ikiye takeout kontra yapıp ortağın 3 seviye cevabından sonra 12+ HP ve rakip renginde durdurucuyla 3NT (−190).
- Yeni `Açıcı 1NT Sonrası`: 1x-1NT'den sonra 18+ HP açıcı 3NT, 17 HP açıcı 2NT (5-5 ve 6+ kartlı eller hariç); yalnız 17+ → 3NT −131 idi, mevcut testlerle (N1, V84) uyum için 17 HP davet bırakıldı (−115).
- Denenip eklenmeyenler: 2NT açılışına 3NT (tutarsız), kontra sonrası 5m ve 4M (kötü/tetiklenmedi), 3. seviye 5m.
- Denendi, KAZANÇ YOK (kod geri alındı): (1) cevaplayanın 10-11 HP ile açıcının minimum tekrarına davet: uyumlu majör 3M ve eşik 10/11 → fark gürültü düzeyinde (−10); 11 HP'de 2NT daveti +100, 10 HP'de 2NT daha da kötü (aşırı teklif); (2) açıcının 1x-1M sonrası tam 3 kart destekle 2M/3M yükseltmesi (açıcının 2♦ rebid kümesinin 86 bordu): +226/+77/+350 ve 3M sıçraması da kötü (≈ +200). Sonuç: bu iki sızıntı basit davet/destek kuralıyla kapanmıyor; yedek mantığın Pas'ı ortalamada doğru. Daha ince ayrım (dağılım, renk kalitesi) gerekir.
- Kalan sızıntılar: `O:2H M:2NT P:3C [Pas]` ailesi, açıcının 2♦ rebid'i (291 bord, ort. sapma 7,0; çoğu 15-16 HP, 1-2 maçalı eller). Analiz: `kume.awk`, `pas_sizinti.awk`; deney: `varyant.sh` + `Deney.Al` (V ve OUT ortam değişkenleri veri ve çıktı klasörünü seçer).
- Void bildirimi denemesi (1♠-2♣-2♥-3♠ sonrası açıcı ortağın doğal minör rengine void ise 4m, `IkiBirOyunZorlama`): 30.000 bordda yalnızca 2-3 bordun ihalesi değişti (iyileşen 0, kötüleşen 1, net +12 IMP); Kuzey 4m cevabını yorumlayıp slama devam edecek kural olmadığı için tek adım işe yaramıyor, geri alındı. Slam için iki adımlı (void bildirimi + cevap) kural gerekir.
- 4NT cevabında void (19 Kasım): ortağın doğal teklif ettiği, kozdan düşük renkte void + en az 1 kilit kart varsa 4NT cevabı o rengin 6 seviyesi (`RKCB.VoidCevabi`; Blackwood ve RKCB ortak kullanır); soran taraf o renkte 6+ kartla pas, yoksa 6 koz (`TemelMantik`). `AnlasilanKoz`: 4. seviyede ortağın doğal rengine yapılan teklif (1♠-2♣-2♥-3♠-4♣) kozu sinek yapmaz, maça kalır. 30.000 bordda koz düzeltmesi −18 IMP, void cevabı ≈ +20 IMP (nötr); kullanıcı isteğiyle bırakıldı. Testler YA17-YA19 (874 test geçti).

## Oyun analizi ve teklif açıklamaları (2026-11-18)
- Eylemler > **Oyun Analizi (hamle hamle)…**: o ana kadar oynanan her kartı çift dummy ile değerlendirir (kayıp löve, en iyi kart(lar); kırmızı deklaran tarafı, mavi savunma). Çekirdek `Class/DD/OyunAnalizi.cs` (`DDSolver.KartHedefeUlasir`: tek kart için "bu değere ulaşır mı" sorgusu; tam değer hesabına göre ~10 kat hızlı, bord başına 1-80 sn arka planda). Doğruluk kimliği (atak öncesi DD + savunma kaybı − deklaran kaybı = gerçek löve) testlerde (AN6, AN7, AN11) ve 13 bordluk elle taramada tutarlı. Not: çözücü değerleri kalan löveler cinsinden verir, tamamlanmış elleri analizde ayrıca sayarız.
- Eylemler > **Teklif Açıklamaları…**: ihaleyi tüm ellerle yeniden oynatır; her teklif için konvansiyon adı, açıklaması ve elin özeti (HP, dağılım). Gerçek teklif motor önerisinden farklıysa sarı satır ve motorun önerisi. (İnsan teklifinin gerçek anlamı bilinemez; açıklama motorun o elle vereceği çağrıya aittir.)

## Oynama karnesi (2026-11-18, ilk ölçüm: 299 bord, boards 1-300)
- Araç: `Class/DD/OynamaKarnesi.cs`, arayüzde Eylemler > Oynama Karnesi…, arayüzsüz `Tools/Olcum/OynamaOlc.exe`. İhale motorunun kontratını kart oynama motoru dört elde oynar; fark = deklaran tarafının gerçek lövesi − çift dummy lövesi.
- PIMC açık (900 ms): ortalama |fark| 0,90 löve; DD ile aynı %38 (fark −1: %24, +1: %19, ±2+: %17); deklaran eksik 96 / savunma eksik 89 bord; ortalama fark +0,03 (iki taraf dengeli).
- PIMC kapalı (yalnız kurallar): ortalama |fark| 0,94; DD ile aynı %40; ortalama fark −0,10 (deklaran tarafı biraz geride). PIMC'nin net katkısı küçük (~0,13 löve/bord, esas olarak deklaran oyununda).
- Zayıf nokta NT kontratlar: ortalama |fark| 1,28 (PIMC açık) / 1,53 (kapalı); suit kontratlarda 0,70-0,79. NT'de fark +0,27: savunma (özellikle atak) DD'nin gerisinde kalıyor.
- Yorum: DD ile aynı olma oranı %40 civarı beklenen aralıkta (motor saklı elleri görmüyor); asıl ölçü, ayarlar değişince |fark| ve ortalama IMP'nin düşmesi. Sıradaki iş: NT atak/savunma kuralları (`OyunZekasi` atak seçimi) ve oyun sonrası analizle hamle bazlı hataları bulmak.
- Hamle bazlı kayıp dağılımı (157 bord, PIMC kapalı, `AnalizOlc`): bord başına 2,97 löve kaybı (iki taraf toplamı); atak %15 (64 bordda kayıp, 0,44 löve/bord), 2.-6. eller %55, 7.-12. eller %30. Belirgin tek sızıntı yok; kayıp eller arasında dengeli, atak kaybı saklı elleri görmeyen bir motor için olağan düzeyde. Basit kuralla kazanç beklenmiyor.

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
- Bord 3 ve Bord 12 (1970 Mixed Pairs PBN) güncel motorla yeniden denendi: Bord 3 4♠ ile par (sapma 0), Bord 12 3♥ ile sapma 1 IMP; ikisi de çözülmüş sayılır.
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
