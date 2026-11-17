# Ölçüm araçları (arayüzsüz Motor Karnesi)

Motor Karnesi taramasını arayüzden yapmak 30.000 bord için saatler sürer. Bu araçlar derlenmiş exe'yi
yükleyip ihale motorunu doğrudan çağırır; bir eşik ya da kural değişikliğinin etkisini dakikalar içinde ölçer.

## Derleme
```bash
Tools/Olcum/derle.sh
```
Ana proje `Tools/Olcum/out/` içine derlenir (`bin\Debug`'a değil; çalışan uygulama exe'yi kilitlemesin), araçlar da oraya konur.
`out/` ve `*.tsv` dosyaları git'e girmez. x86 çalışır (proje 32 bit).

## Araçlar
| Araç | Ne yapar |
|---|---|
| `Taban.exe ilk adet cikti.tsv` | Motorun **güncel** halinin taban çizgisini üretir (bord, ihale, sapma). Çift dummy kullandığı için uzun sürer. |
| `Measure.exe taban.tsv cikti.tsv` | Tabana göre **ihalesi değişen** bordları yeniden değerlendirir; toplam sapma farkı ve iyileşen/kötüleşen sayısını yazar. 10.000 bord ~2 dk. |
| `PbnRun.exe dosya.pbn [bord...]` | PBN'deki bordları (simultane) motorla oynatır; ihale, konvansiyon adları, par ve saha sonuçlarını gösterir. |
| `Seeds.exe seed tur [seed tur ...]` | Verilen dağılım numaralarının ellerini ve ihalesini gösterir. |
| `Probe.exe "S/H/D/C" geçmiş...` | Tek elle (Güney sırada) tek teklif sorar, hangi konvansiyonun verdiğini yazar. |
| `Attr.exe ilk adet cikti.tsv` | Bord aralığında her teklifin hangi konvansiyondan geldiğini döker (Temel Mantık payı, sapma kümeleri için). |

## Tipik akış (bir kuralı ölçmek)
1. Değişiklikten ÖNCE: `Taban.exe 1 10000 taban.tsv` (ya da mevcut bir taban dosyası).
2. Kodu değiştir, `derle.sh`'ı yeniden çalıştır.
3. `Measure.exe taban.tsv sonuc.tsv` → "eski=… yeni=…" satırındaki fark kazanç ya da kayıptır (negatif = iyi).
4. Tutarlılık için aralığı üçe böl (1-10000, 10001-20000, 20001-30000) ve üçünde de aynı yönde olduğunu doğrula; aşırı uyumdan kaçınmak için.

## Dikkat
- DD (çift dummy) bakış açısı biraz iyimserdir: masada savunma insan olduğu için gerçek kazanç genelde biraz daha azdır.
- Taban çizgisi her seferinde GÜNCEL motordan üretilmeli; başka bir oturumun/commit'in değişikliği ölçümü bozar.
- GUI taramasının (`Tarama_*.txt`) bord/ihale/sapma sütunları da taban olarak kullanılabilir (UTF-16 ya da CP1254 olabilir, `Measure` UTF-8 TSV bekler).

## Sızıntı analizi ve deney akışı (yedek mantık payını azaltma)
- `kume.awk`: Temel Mantık (yedek) teklif kümeleri — hangi ihale durumlarında yedek devrede, ortalama sapma ve ortalama üstü toplam sapma.
  `awk -v ORT=5.19 -f kume.awk veri/taban_tum.tsv veri/attr_a.tsv` (Taban.exe + Attr.exe çıktıları).
- `pas_sizinti.awk`: yedeğin verdiği SON PAS'larda "birleşik HP yüksek ama kontrat parsiyel" sızıntısı (el bilgisi gerekir: ds.tsv biçimi).
- `varyant.sh ad ORTAM=DEĞER ...`: bir deney varyantını üç 10.000'lik kısımda paralel ölçer. Kodda eşik `Deney.Al("BK_ADI", varsayılan)` ile okunur;
  yeniden derlemeden `BK_ADI=değer` ile denenir. Eşik netleşince sabit yazılır, `Deney.Al` çağrısı kaldırılır.
- `Testler.exe [cikti.txt]`: tüm ihale testlerini arayüzsüz çalıştırır, geçen/kalan sayısını ve başarısızları yazar.
- Taban dosyaları `Tools/Olcum/veri/` altına konur (git'e girmez): `taban_a|b|c.tsv` (1-10000, 10001-20000, 20001-30000).
