# Sinyaller: Genel Sinyaller

**Sinyalleri Göster > Genel Sinyaller** sekmesi savunmada ortaklar arasındaki kart işaretleşmesini belirler: ortağın atağına nasıl cevap verileceği ve atma kartlarının anlamı.

## Bilgisayarın savunmasını etkileyen seçenekler
Bu üç seçim, bilgisayarın karşı oyun kart seçimini gerçekten değiştirir:
- **Tutum (Yüksek)**: **Teşvik eder** = yüksek kart "devam et" der (standart). **Caydırır** = yüksek kart "devam etme" der; alçak kart teşvik eder (**UDCA**: ters sayı ve tutum).
- **İlk Atma Şekli**: ilk atma kartının anlamı.
  - **Tek/Çift**: tek rakamlı kart (3, 5, 7, 9) rengi istediğinizi, çift rakamlı kart istemediğinizi söyler (Odd/Even).
  - **Lavinthal**: atılan kartın yüksek ya da alçak olması, istenen diğer rengi gösterir.
  - **Romen**: tek kart (3, 5, 7, 9) atılan rengin kendisini ister; küçük çift (2, 4) ve büyük çift (6, 8) kartlar, koz ve atılan renk dışında kalan renklerden alçak ya da yüksek rütbeli olanı ister. Onörler sinyal sayılmaz.
  - **Tutum**, **Sayı**, **Dönüşümlü**: şu an ayrı bir davranış değiştirmez, standart atma uygulanır.
- **Smith (Ortak)**: **Yok** dışında bir değer seçerseniz **Smith echo** açılır: NT'de deklaran bir renge ilk kez oynayınca, savunmacının yüksek-alçak oynaması "ilk atak rengini sürdür" demektir.

## Yalnızca saklanan seçenekler
Aşağıdakiler profilde saklanır, ancak bilgisayarın kart seçimini şu an değiştirmez:
- **Her Türlü Sinyal** (Evet/Hayır) ve **İlk Atma Anlamlı** (Evet/Hayır).
- **Birincil Sinyal**: **Sayı**, **Tutum** ya da **Tek/Çift**: ortağın atağına verilen cevabın türü.
- **Sayı Sinyali**: **Çift** (yüksek-alçak = çift sayıda kart) ya da **Tek**.
- **Rakibe Sayı**: rakibin renklerinde de sayı sinyali verilsin mi.
- **Smith (Açıcı)**: atağı yapan tarafın Smith echo kullanımı.
- **Koz Sinyali**: **Vinje** (Vinje yöntemi), **Yok** ya da **3+ Yüks-Alç** (kozda yüksek-alçak oynamak 3 ya da daha fazla kozu gösterir).

## Not
- Bu sayfadaki atma seçimleri, **Konvansiyonlar** ekranındaki sinyal seçenekleriyle (Standart, UDCA, Tek-Çift, Lavinthal, Smith echo, Romen) aynı bayrakları kullanır; birinde yaptığınız değişiklik diğerinde de görünür.
