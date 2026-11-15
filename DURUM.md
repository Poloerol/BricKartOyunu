# Durum (son güncelleme: 2026-11-12)

## Son yapılanlar
- `25b7800`: `OyunDavetiKabul` (1M-2M-3M sonrası 10+ HCP ile 4M, aksi Pas; testler CT7/CT8). Cevapçı ikinci turda 2NT daveti kaldırıldı, 3M daveti 12+ HCP.
- `25b7800`: "Kullanıcı girişi" dağıtımı artık PBN dosyasından okunuyor (`PbnOkuyucu`, Oyun Planı'nda dosya seçimi). Elle giriş formu devre dışı (`ManuelGirisModu()` hep false).
- Bu hafta: Lebensohl/Cappelletti/Jacoby düzeltmeleri, Motor Karnesi üst sınırı 50000.

## Devam eden
- Motor Karnesi 1-20000 taraması çalışıyor; rapor gelince bulgular yorumlanacak.

## Açık noktalar
- `ManuelGirisAl` / `ElGirisiForm` kodu ölü duruyor; silinebilir.
- PBN'de `[Vulnerable]` okunmuyor, zon bord numarasından hesaplanıyor.
- `IhaleMotoru.cs` git'te satır sonu gürültüsüyle tamamen değişmiş görünüyor (gerçek fark tek satır).
- `feature/1c-cevap-sistemi` dalında push edilmemiş 1 commit; `master` üzerinde "Klasör yapısı düzeltmesi öncesi" stash'i var.

## Motor Karnesi (2026-11-12, 2000 dağıtımlık özet)
- 792 test geçti, 0 başarısız; toplu denetimde istisna 0. (1-20000 taraması ayrı, hâlâ bekliyor.)
- Bilgi amaçlı kümeler (Temel Mantık Fallback): kozda 8 karttan az uyum seviye 3+ = 68 (%3,4); 9+ majör uyumu kaçtı = 22; 27+ HCP'li taraf oyuna gidemedi = 10.
- Öncelik: 27+ HCP kümesi. Örnek eller #440 (1♠-1NT-2♦ Pas, Kuzey 20 HCP), #32 ve #160 (7'li ♥, 2♥'de kalıyor). Önce test olarak ekle, sonra düzelt.
- Müdahale sonrası konvansiyonlarda (Unusual 2NT, Michaels, Lebensohl, Preempt, DONT) 2-6'şar el; çoğu doğal rekabet olabilir.

## Sıradaki adım
- Tarama raporunu incele, sapmaları öncelik sırasıyla düzelt.

## Oturum: "Anasayfayı kaldırma oturumu" (2026-11-14 gece, devam edilecek)
Amaç: Görev çubuğunda tek ve sabit düğme (Bridge Baron gibi). Kullanıcı kök çözüm istiyor: AnaSayfa + BricOyna iki ayrı pencere yerine tek pencere; "Oyna" tıklanınca ana sayfa öğeleri gizlenir, oyun masası paneli/menüsü yüklenir, pencere başlığı sayfaya göre değişir (düğme adı da değişir). Henüz kodlanmadı; yalnızca fikir alışverişi yapıldı, kullanıcı "kaldığımız yerden devam" dedi.

### Neden iki pencere sorun çıkarıyor (bugünkü bulgular)
- Uygulama 32 bit çalışıyor (SetWindowLongPtrW 32 bitte yok, SetWindowLongW gerekir).
- ShowDialog ile açılmış formun sahibi küçülürse ya da form Hide() edilirse modal döngü biter, oyun kapanır.
- Sahipli (owned) BricOyna + ShowInTaskbar=false: görev çubuğunda düğme yok ama kendi başına küçülünce sol altta (0,1012) küçük çubuk bırakır.
- Sahibi sökülen (sahipsiz) BricOyna kendi başına küçülünce ekran dışına gider ama Windows ona ayrı görev çubuğu düğmesi açar; ITaskbarList.DeleteTab bunu kaldırmadı. Yani "tek düğme + küçültme" iki pencereyle güvenilir sağlanamıyor.
- Görev çubuğu düğmesi eklenip silinirken Windows kendi kayma animasyonunu oynatır (iki pencerede kaçınılmaz).
- Bridge Baron'da tek düğme sabit; ana sayfada "Bridge Baron", oyun sayfasında "Play Bridge" yazıyor.
- Yarım kalan deney: sahipli pencerede SetWindowPlacement ile küçük çubuğu ekran dışına atma (WPF_SETMINPOSITION) denendi, sonuç net değil.

### Hedef tasarım ve öneri
- AnaSayfa'yı SİLMEK yerine tek pencere (kabuk) olarak TUTMAK ve BricOyna'yı içine gömmek: BricOyna.TopLevel=false, FormBorderStyle=None, Dock=Fill, AnaSayfa içindeki panele eklenir. BricOyna kodu/designer'ı neredeyse olduğu gibi kalır. Ana sayfa öğeleri gizlenir, menü değişir, Text güncellenir.
- Kazanç: tek sabit düğme, kayma yok, normal küçültme/geri getirme; Opacity=0 hilesi, Hazir olayı, ShowDialog bekleme döngüsü, görev çubuğu gizleme kodu gider; geçiş hızlanır.
- Riskler: (1) BricOyna kendini pencere gibi kullanıyor: this.Close(), FormClosing/FormClosed (MacDurumu.Bitir), ShowDialog(this), Application.OpenForms.OfType<BricOyna>(), WindowState, form boyutuna göre KartYerlesimiHazirla; (2) Deklarasyon/ihale/notlar pencereleri BricOyna'ya sahipli açılıyor, sahipliği ana pencereye taşımak gerek (en riskli); (3) X'in anlamı değişir; (4) iki menü/araç çubuğu görünürlük yönetimi; (5) Konvansiyon Pratiği, Dağılım Deposu, Briç Maçı akışları BricOyna'yı AnaSayfa'dan açıyor; (6) oyun masasını Oyna'da oluşturup çıkışta atmak (bellek, sıfırlama); (7) GUI'yi Claude çalıştıramıyor, her aşamayı kullanıcı deneyecek.
- Önerilen yol: ayrı dalda, küçük adımlarla: (1) BricOyna'yı AnaSayfa içine gömüp açılış/kapanışı çalıştır, (2) sahip pencereler/Geri/kapatma yollarını düzelt, (3) eski geçici kodları temizle. Önce kapsam çıkarılabilir: BricOyna'nın pencere gibi kullanıldığı yerleri say, kod değiştirme.

### Kullanıcıdan alınacak kararlar
- Oyunda X: program mı kapansın, ana sayfaya mı dönsün? (Öneri: X programı kapatır, "Geri" ana sayfaya döner.)
- AnaSayfa sınıfı tek pencere (kabuk) olarak kalsın mı? (Öneri: evet.)

### Çalışma ağacı durumu (HİÇBİR ŞEY COMMİTLENMEDİ; main = origin/main = 52d67ef)
- İhale düzeltmeleri (sağlam iş): Class/Bidding/Conventions/Yedek/BasitCevap.cs (1♥ açılışına 4+ maçayla 1♠), TemelMantik.cs (1♥-2x sonrası 17+ HP 5♥-4♠ elde 2♠), Class/Bidding/Tests/TestRunner_Duzeltmeler.cs (N4-N8 testleri). N4-N8 testleri HENÜZ ÇALIŞTIRILMADI.
- Görev çubuğu/simge durumu denemeleri (DENEYSEL, tek pencereye geçilince büyük kısmı atılacak): AnaSayfa.cs, Forms/BricOyna.cs, Forms/DeklarasyonForm.Ogrenme.cs, Class/EkranYardimci.cs. EkranYardimci'de ITaskbarList yardımcıları, SahibiniKaldir, SimgedenGeriYukle ve GEÇİCİ Gunluk (bin\Debug\GorevCubugu.log) var; BricOyna.cs'de ShowInTaskbar=false, Shown'da SahibiniKaldir, simge durumunda AnaSayfa'yı küçültme ve log satırları; AnaSayfa.cs'de OnSizeChanged geri yükleme + log. Bu haliyle iki düğme çıkıyor (sahipsiz BricOyna düğme alıyor); çalışan/bitmiş bir çözüm değil.
- Yarın ilk adım önerisi: ihale düzeltmelerini testle doğrulayıp ayrı commitle; görev çubuğu denemelerini commitlemeden geri al ya da ayrı dalda tut; sonra tek pencere işine kapsam çıkarmakla başla.
- .zip yedek alınmadı (kullanıcı kararı: aynı yerden devam). Scratchpad'deki yakalama/deney betikleri geçici, projeye ait değil.
### Ek (2026-11-14 03:30): Bridge Baron tek pencere davranışı (D:\Projects\Görseller\Lightshot\Screenshot_1-3.png, 03:18)
- Tek üst düzey pencere. Menü ve araç çubuğu ana ekranda ve oyunda AYNI; yalnızca başlık değişiyor ("Bridge Baron 17" / "Play Bridge"), durum çubuğu içeriği değişiyor. Yeni pencere/sayfa yüklenmiyor: paneller gösterilip gizleniyor.
- Ana sayfaya dönüşte ana sayfa paneli oyun görünümünün üstüne çiziliyor; panel dışı kenarlarda oyundan kalma kartlar/yazılar kalıyor (yeniden çizim hatası). Bizde sayfa paneli tam boy (Dock=Fill) olmalı ve arka plan temizlenmeli.
- AnaSayfa ve BricOyna menü başlıkları zaten aynı (Dosya, Ayarlar, Eylemler, Görünüm, Değerlendirme, Eller, İpucu, Program, Yardım); araç çubuğu simgeleri farklı. Tek menü tutup öğeleri sayfaya göre etkinleştirme/gizleme daha kolay olabilir (kod incelemesinde teyit edilecek).
### Uygulandı (2026-11-15, dal `tek-pencere`, COMMİTLENMEDİ)
- Oyun masası (BricOyna) AnaSayfa'nın İÇİNE gömülür (TopLevel=false, Dock=Fill); AnaSayfa'nın görünen tüm öğeleri gizlenir, kapanınca geri gelir. Görev çubuğunda tek düğme. Başlık oyun başlığını izler (TextChanged).
- Değişen dosyalar: AnaSayfa.cs (OyunuGomuluAc: BricOynaAc/PratikOyunuAc/DagilimDeposuAc/MacOyunuBaslat artık BEKLEMEZ; "bitince" geri çağrısı; WS_EX_COMPOSITED oyun sırasında kapatılır; oyun sırasında açılan pencereler dönüşte kapatılır; sahipli pencereler gizlenip geri açılır), BricOyna.cs (Opacity hileleri kaldırıldı; SahipPencere ve Bul() eklendi), BricOyna.Menu.cs (ihale yorumu konumu ekran koordinatı), BricMaci.cs ve KartDagitici.cs (ShowDialog sonrası kodlar geri çağrıya taşındı), ElleriDegistir.cs (Owner yerine BricOyna.Bul), DeklarasyonForm.cs (Owner = SahipPencere), DealCompleteForm.cs (PointToScreen), EkranYardimci.cs (gömülü masa için sahip bulma).
- Doğrulanan (UI Automation ile Claude kendi sürdü): tek pencere, tek görev çubuğu düğmesi; Oyna → Briç Oyna → masa gömülü tam boy; küçült/geri yükle; Geri → ana sayfa; Oyundan Çık → ana sayfa ve açık pencere kalmıyor; ikinci girişte yine çalışıyor. Derleme 0 hata.
- KULLANICI TARAFINDAN DENENECEK (Claude doğrulayamadı): Briç Maçı akışı (MacDurumu.Bitir artık bitince geri çağrısında), Konvansiyon Pratiği ve Kart Dağıtıcı'dan pratik, Dağılım Deposu, görev çubuğu düğmesine tıklayarak geri getirme, kısayol tuşları, menü ve araç çubuğu görünümü, titreme/çizim hızı (WS_EX_COMPOSITED kapatıp açma), pencere X'i (artık programı kapatır; ana sayfaya dönüş için Geri/Oyundan Çık).
- Eski görev çubuğu denemeleri `git stash` içinde ("Gorev cubugu denemeleri (tek pencere oncesi, 2026-11-15)"); gerekmezse `git stash drop`.
### Güncelleme (2026-11-15 akşam): tek pencere tamamlandı ve commitlendi (dal `tek-pencere`)
- Sayfa başlıkları görev çubuğu düğmesine yansır (Bridge Baron gibi): Briç Oyna, Briç Maçı, Konvansiyon Pratiği, Konvansiyon Öğrenme, Zor Eller, Turnuva, Kart Dağıtıcı, Dağılım Deposu.
- Geçişte çizim dondurulur (EkranYardimci.CizimiDondur/CizimiAc): yırtık/yarım kareler giderildi. Oyun sırasında ana pencere küçülünce açık pencereler (Öğrenme Notları vb.) gizlenip geri açılır.
- BricOyna.SetGameControlsEnabled masa kapanırken/ayrıkken atlanır (ArgumentException "Parametre geçerli değil" düzeltmesi).
- İhale: ortağın 1 seviye minör araya girmesine (1♣-(1♦)-Pas) 4+ kartlık majörle 1 seviyede yeni renk (2 seviyede 5+ kart, 10+ HP); 4-4 majörde ♥. TemelMantik.YeniMajorOnceligi. Testler N9-N13 eklendi; tam set 809 geçti, başarısız yok.
- Kart Dağıtıcı bulgusu: 1♣-1♠-2♣ sonrası Kuzey/Doğu 2♦ motorda yok (anlaşma ayarlarının hiçbiri üretmiyor); gerçek bordda (vugraph board 3) Can 2♣'ye pas dedi, doğru çağrı 2♠ (motor 2♠ veriyor, Kart Dağıtıcı bu diziyi buluyor). Zayıf 2♦ için anlaşma seçeneği (forcing değil / bir tur forcing / oyun forcing) istenirse eklenecek.
- Açık: 4-4 majörde ♦'de uyum yoksa motor 1♠ diyor (♥ önceliği yalnızca minör uyum varken uygulanıyor); Briç Maçı/Pratik/Dağılım Deposu akışları kullanıcı tarafından tam denenmedi.