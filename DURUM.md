# Durum (son güncelleme: 2026-11-17)

## Şu an
- Dal `main`; son commit `9e8a44e` (push edildi). Başka bir Claude oturumu aynı motorda çalışabiliyor: ölçüme ya da motor düzenlemesine başlamadan önce `git log -5` ve `git status` bak.
- Testler: ihale testlerinin tamamı geçiyor (267 test, 0 başarısız; `TestRunner` içinden çalışır).
- Derleme: `dotnet build BricKartOyunu.sln` ya da MSBuild. Uygulama açıkken `bin\Debug\BricKartOyunu.exe` kilitlenir; bu derleme hatası değil kopyalama hatasıdır.

## Ölçüm yöntemi (Motor Karnesi, arayüzsüz)
- Araçlar repoda: `Tools/Olcum/` (README'ye bak). `Tools/Olcum/derle.sh` ana projeyi ayrı klasöre derler ve araçları yapar: `Taban` (taban çizgisi), `Measure` (tabana göre değişen bordların sapma farkı), `PbnRun` (PBN bordlarını motorla oynat), `Probe`, `Seeds`, `Attr`.
- Kural: her değişikliği 30.000 bordda ölç, üç 10.000'lik kısımda aynı yönde olmasını doğrula. DD bakış açısı iyimserdir; masada gerçek kazanç biraz daha az olabilir.
- Güncel motor ortalama sapması: yaklaşık 5,19 (30.000 bord; başlangıç 5,30). Temel Mantık yedeği son tekliflerin yaklaşık %54'ünü hâlâ veriyor.

## Son yapılanlar (Kasım 2026)
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
