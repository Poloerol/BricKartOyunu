# Sinyaller: Genel Sinyaller

**Sinyalleri Göster > Genel Sinyaller** sekmesi savunmada ortaklar arasındaki kart işaretleşmesini belirler: ortağın atağına verilen cevap, atma kartlarının anlamı ve bazı özel sinyaller.

## Temel kavramlar
- **Sinyal**: savunmacının, normal oynayışının dışında, bir kartın yüksek ya da alçak seçilmesiyle ortağına verdiği haber.
- **Tutum (attitude)**: "bu renkte devam et" ya da "devam etme" haberi.
- **Sayı (count)**: o renkte kaç kartınız olduğunun haberi (çift mi tek mi).

## Ortağın atağına cevap
- **Her Türlü Sinyal**: **Evet** ise ortağın atağına verilen küçük kartın bir anlamı vardır; **Hayır** ise yoktur.
- **Birincil Sinyal**: ortağın atağına verilen küçük kartın neyi anlattığı.
  - **Sayı**: kartlar rengin uzunluğunu gösterir.
  - **Tutum**: kartlar ortağın o renkten devam etmesini isteyip istemediğinizi gösterir.
  - **Tek/Çift**: tek rakamlı kart (3, 5, 7, 9) devam etmesini teşvik eder, çift rakamlı kart caydırır. Yalnızca tek rakamlı kartınız varsa caydırmak için en yüksek tek rakamlı kartı, yalnızca çift rakamlı kartınız varsa teşvik etmek için en yüksek çift rakamlı kartı oynarsınız.
- **Tutum (Yüksek)**: **Teşvik eder** ise tutum işaretinde yüksek kart "devam et", alçak kart "devam etme" demektir. **Caydırır** ise tersi geçerlidir (**UDCA**: yüksek kart devam etme, alçak kart devam et der).
- **Sayı Sinyali**: sayı işaretinde yüksek-alçak oynamanın anlamı: **Çift** ise önce yüksek sonra alçak çift sayıda kart, **Tek** ise tek sayıda kart gösterir.

## İlk atma
- **İlk Atma Anlamlı**: **Evet** ise savunmacının ilk atma kartının bir anlamı vardır.
- **İlk Atma Şekli**: atılan kartın neyi anlattığı (savunmacı atak rengini takip edemediğinde):
  - **Tutum**: atılan renkte gücünüz olup olmadığını söyler.
  - **Sayı**: atılan rengin uzunluğunu söyler.
  - **Tek/Çift**: tek rakamlı atma kartı atılan renkte gücünüz olduğunu, çift rakamlı kart zayıflığınızı gösterir.
  - **Lavinthal**: atılan renkte zayıflığı ve **başka bir renkte gücü** gösterir. Yüksek kart, atılan renk dışındaki iki rengin yüksek rütbelisinde gücünüz olduğunu, alçak kart alçak rütbelisinde gücünüz olduğunu söyler. Örneğin kupaya karşı maçadan atılan yüksek kart karoda, alçak kart sinekte güç gösterir.
  - **Dönüşümlü (Revolving / Attwood)**: Lavinthal'a benzer. Atılan kartın bulunduğu rengin **bir üstündeki** renk (yüksek kart) ya da **bir altındaki** renk (alçak kart) güçlüdür; üzerine atma yaptığınız (oynanan) renk teşvik edilemeyeceği için atlanır. Renk sırası döngüseldir: maçanın üstü sinek, sineğin altı maçadır. Örneğin maça rengi üzerinde karodan atılan yüksek kart kupayı, alçak kart sineği gösterir.
  - **Romen**: tek rakamlı kart (3, 5, 7, 9) atılan rengin kendisini ister; küçük çift (2, 4) ve büyük çift (6, 8) kartlar, koz ve atılan renk dışında kalan renklerden alçak ya da yüksek rütbelisini ister. Onörler sinyal sayılmaz.

## Smith echo (NT'ye karşı)
Atakla başlayan renk dışında bir renge deklaran ya da dummy ilk kez oynadığında, bir savunmacının verdiği küçük kart, atak rengine devam edilip edilmeyeceğini söyler (o anda sayı vermesi gerekmiyorsa).
- **Smith (Ortak)**: atak yapanın ortağının sinyali. **Smith (Açıcı)**: atak yapanın kendi sinyali. **Yok**: Smith kullanılmaz.
- **Yüksek-Alçak** (bir sonraki renge geçiş için): yüksek kart atak rengini **sürdürmeyi** caydırır, alçak kart teşvik eder. **Alçak-Yüksek**: tersi.
- Bilgisayarın savunması, **Smith (Ortak)** seçeneği **Yok** dışında olduğunda klasik Smith echo kuralını uygular: yüksek-alçak oynamak atak rengini sürdürmeyi ister. Yüksek-Alçak / Alçak-Yüksek ayrımı bilgisayarın oyununu şu an değiştirmez.

## Rakibin atağına sayı ve koz
- **Rakibe Sayı**: **Evet** ise deklaran ya da dummy bir renge oynadığında savunmacı, o renkteki kart sayısını gösteren küçük kart oynar. **Hayır** ise göstermez.
- **Koz Sinyali**: kozun oynanışına verilen cevapta:
  - **Yok**: koz sinyali yoktur.
  - **3+ Yüks-Alç**: koz atağına yüksek kartla cevap vermek üç ya da daha fazla kozu, alçak kart iki ya da daha az kozu gösterir.
  - **Vinje**: yüksek kart "tek el" (üç renkte tek sayıda kart: 4-3-3-3, 5-3-3-2, 5-4-3-1), alçak kart "çift el" (üç renkte çift sayıda kart: 4-4-3-2, 5-4-2-2, 5-4-4-0) gösterir.

## Bilgisayarın savunmasına yansıyanlar
Bu seçimler bilgisayarın karşı oyun kart seçimini gerçekten değiştirir:
- **Tutum (Yüksek) = Caydırır** (UDCA).
- **İlk Atma Şekli**: **Tek/Çift**, **Lavinthal** ve **Romen**.
- **Smith (Ortak)** için **Yok** dışında bir değer (Smith echo açılır). **Smith (Açıcı)** yalnızca saklanır.

Diğer seçenekler (Her Türlü Sinyal, İlk Atma Anlamlı, Birincil Sinyal, Sayı Sinyali, Rakibe Sayı, Koz Sinyali, İlk Atma Şekli'nin Tutum / Sayı / Dönüşümlü değerleri) profilde **saklanır**, ancak bilgisayarın oyununu şu an değiştirmez.

## Not
- Bu sayfadaki atma seçimleri, **Konvansiyonlar** ekranındaki sinyal seçenekleriyle (Standart, UDCA, Tek-Çift, Lavinthal, Smith echo, Romen) aynı bayrakları kullanır; birinde yaptığınız değişiklik diğerinde de görünür.
