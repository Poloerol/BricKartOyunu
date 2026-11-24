# Oyun türleri ve puanlama

**Ayarlar > Oyun Ayarları > Oyun > Oyun Türü** bölümünden seçilir. Zon (hangi tarafın zonda olduğu) ve puan hesabı türe göre değişir.

## Duplicate
- Her bord kendi başına puanlanır; zon 16 bordluk standart döngüden gelir.
- Skor: **Bu Elin Puanlaması > Duplicate Skor**.

## Tek El
- Bordlar tek tek oynanır; hiçbir puan bir sonrakine taşınmaz. Puan duplicate sistemiyle hesaplanır, oturum toplamı tutulmaz.

## Rubber
- Löve puanı çizginin altına yazılır; 100'e ulaşan taraf **oyunu** alır. Yarım kalan parsiyel (örneğin 60) sonraki bordlara taşınır.
- Fazla löve, slam bonusu, kontra ikramı ve batış cezası çizginin üstüne yazılır.
- İki oyunu alan taraf rubberı bitirir: rakibin oyunu yoksa **+700**, varsa **+500**.
- **Zon**, bir oyun kazanmış taraf demektir.
- Onör puanları hesaba katılmaz.

## Chicago (dört bord)
- 1. bord zonsuz, 2. ve 3. bordlarda dağıtıcının tarafı zonda, 4. bordda herkes zonda.
- **Chicago Rubber**: çizgi altı parsiyel taşınır; oyun bonusu zonsuz 300, zonda 500. Dördüncü bordun sonunda parsiyeli olan taraf +100 alır.
- **Chicago Duplicate**: her bord duplicate puanıyla yazılır; toplam dört bord boyunca tutulur.

## Skor penceresi
- Bord bitince **Bu Elin Puanlaması** bordları sırayla, zonu, çizgi üstü/altı puanı ve toplamı gösterir.
- **Temizle** yeni rubber ya da yeni Chicago başlatır. Oyun türünü değiştirince oturum puanları da sıfırlanır.
- Aynı bordu **Bu Eli Tekrar Oyna** ile tekrar oynarsanız son sonuç öncekinin yerine geçer.

## Duplicate puan tablosu (özet)
- Löve puanı: minör 20, majör 30, NT ilk löve 40 sonrakiler 30; kontra ile ikiye, redkontra ile dörde katlanır.
- Oyun bonusu: kontrat puanı 100 ve üzeriyse zonsuz 300, zonda 500; altındaysa parsiyel 50.
- Slam bonusu: küçük slam 500 (zonda 750), büyük slam 1000 (zonda 1500).
- Batış: zonsuz löve başına 50, zonda 100 (kontralıda artan tablo).
- Kontralı ya da redkontralı kontrat yapılırsa ikram 50 ya da 100.

## Henüz yok
- Yarım kalan rubberın puanlanması ve onör puanları.
