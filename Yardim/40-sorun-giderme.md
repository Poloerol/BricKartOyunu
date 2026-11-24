# Sorun giderme

## Yardım penceresi boş
- Yardım konuları exe yanındaki **Yardim** klasöründeki `.md` dosyalarıdır. Kurulumda ya da derlemede bu klasör kopyalanmamışsa liste boş görünür; pencere aradığı klasörün yolunu gösterir.
- Kurulum paketini yeniden oluşturun (`Kurulum\KurulumuOlustur.ps1`) ya da `Yardim` klasörünü elle exe'nin yanına kopyalayın.

## "Masada tam olarak bir insan oyuncu olmalıdır" uyarısı
- **Oyun Ayarları > Oyuncu Türleri** sekmesinde yalnızca bir koltuğu **İnsan** yapın.

## Bilgisayar çok yavaş ya da çok hızlı oynuyor
- **Oyun Ayarları > Oyun > Yapay Zekâ İhale/Oyun Gecikmesi**: 0, 1 ya da 2 saniye. **Her zaman hızlı otomatik oyna** otomatik oynatmayı hızlandırır.

## Zon beklediğim gibi değil
- Zon oyun türüne göre belirlenir: Duplicate'ta 16 bordluk döngü, Rubber'da kazanılan oyunlar, Chicago'da dört bordluk döngü. Oyun türünü **Oyun Ayarları > Oyun** sekmesinden kontrol edin.

## Skor penceresi başka türde açılıyor
- Rubber ve Chicago seçiliyken **Bu Elin Puanlaması** rubber/Chicago skor penceresini, Duplicate ve Tek El seçiliyken Duplicate Skor penceresini açar. Briç Maçı sırasında maç skoru gösterilir.

## Ayarlar kayboldu
- Ayarlar `%AppData%\BricKartOyunu` klasöründe JSON dosyaları olarak saklanır (örneğin oyun planı için `oyun_plani.json`). **Ayarlar > Ayarları Sıfırla** hepsini varsayılana döndürür.

## Uygulama açıkken derleme hatası
- Visual Studio'da hata ayıklayıcı açıkken `bin\Debug\BricKartOyunu.exe` kilitlenir. Bu bir kod hatası değil, kopyalama hatasıdır; uygulamayı kapatıp tekrar derleyin.

## Hata bildirmek
- Sorunun oluştuğu bordun numarasını **Eylemler > İhale Numarasını Kopyala** ile kopyalayıp bildirin; aynı dağılım yeniden açılabilir.
