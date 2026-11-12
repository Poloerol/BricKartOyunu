# BricKartOyunu

C# WinForms briç (bridge) kart oyunu. İhale motoru ve konvansiyonlar `Class/Bidding` altında; formlar `Forms` altında.

**Oturuma başlarken önce `DURUM.md` dosyasını oku**, sonra `git log -5` ve `git status` ile doğrula.

## Yapı
- `Class/Bidding/IhaleMotoru.cs`: konvansiyon kayıt listesi ve ihale akışı
- `Class/Bidding/IhaleKoruma.cs`: konvansiyon kapıları / bağlam kontrolleri
- `Class/Bidding/Conventions/`: konvansiyonlar (`IKonvansiyon`: `UygunMu`, `TeklifVer`, `Oncelik`)
- `Class/KonvansiyonPratigi.cs`: konvansiyon listesi (UI gruplaması)
- `Class/Bidding/Tests/TestRunner_*.cs`: ihale testleri (`T(ad, el, beklenen, geçmiş...)`)
- `Forms/BricOyna*.cs`: masa (partial class); `Forms/OyunPlani*.cs`: oyun planı ayarları (PBN dahil)

## Kurallar
- Yanıtlar ve commit mesajları Türkçe.
- Kaynak dosyalar UTF-8 BOM + CRLF. Satır sonlarını bozma; toplu yeniden yazma yerine küçük düzenleme yap.
- Yeni konvansiyon eklerken dört yer: sınıf, `IhaleMotoru` kayıt listesi, `KonvansiyonPratigi` listesi, gerekirse `IhaleKoruma` muafiyeti; ayrıca test ekle.
- Derleme: `dotnet build BricKartOyunu.sln`. Hata ayıklayıcı açıksa exe kilitlenir; bu derleme hatası değil, kopyalama hatasıdır.
- Push ve geri alınamaz işlemler yalnızca kullanıcı isterse.
