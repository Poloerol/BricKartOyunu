# BB – BKO kıyas defteri

Amaç: aynı bordları Bridge Baron'da ve BKO'da (aynı tarafta oturarak) oynayıp sonuçları yan yana toplamak;
toplam IMP farkını ve kayıpların hangi kategoriden geldiğini (ihale / slam / rekabet / atak / oynama / savunma) görmek.

## Nasıl doldurulur
1. Bir bordu BB'de ve BKO'da oyna (PBN ile aynı eller; BKO'da Dağılım Deposu → PBN, BB'de Deal Library).
2. Her ikisinde de **Kuzey-Güney açısından skoru** yaz (NS lehine +, aleyhine −). Örnek: 4♠ yapıldı (NS deklaran, +620) → `620`.
3. `kiyas.tsv`'ye satır ekle: `bord  <TAB>  bko  <TAB>  bb  <TAB>  kategori  <TAB>  not`.
4. Özet için: `Tools/Kiyas/derle.sh` (bir kez), sonra `Tools/Kiyas/Ozet.exe Tools/Kiyas/kiyas.tsv`.

## Çıktı
- Bord başına IMP farkı (+ BKO iyi, − BB iyi), toplam IMP, bord başına ortalama, kazanılan/kaybedilen bord sayısı.
- Kategori bazında toplam: en çok kaybettiğimiz alan en üstte.

## Notlar
- 100+ bord toplanınca anlamlı; 10-20 bordda tek bir slam sonucu toplamı belirler.
- BB ve BKO'nun aynı tarafta (örn. ikisinde de Güney'de) oturması şart; yoksa skor işareti karışır.
- Kategori, farkın asıl sebebi: ihale = yanlış kontrat/seviye; slam = slam kaçırma/yersiz slam; rekabet = müdahale/kontra; atak, oynama, savunma.
