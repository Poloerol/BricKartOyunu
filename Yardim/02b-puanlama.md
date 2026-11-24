# Oyun türleri ve puanlama

**Ayarlar > Oyun Ayarları > Oyun Planı > Oyun > Oyun Türü** bölümünden puanlama sistemini seçersiniz.

## Duplicate
- Her bord kendi başına puanlanır; zon 16 bordluk döngüden gelir.
- Skor: Bu Elin Puanlaması > Duplicate Skor.

## Rubber
- Löve puanı çizginin altına yazılır; 100'e ulaşan taraf **oyunu** alır. Yarım kalan parsiyeller sonraki bordlara taşınır.
- Fazla löve, slam bonusu, kontra ikramı ve batış cezası çizginin üstüne yazılır.
- İki oyunu alan taraf rubberı bitirir: rakibin oyunu yoksa **+700**, varsa **+500**.
- **Zon**, bir oyun kazanmış taraf demektir.
- Onör (honör) puanları hesaba katılmaz.

## Chicago (4 bord)
- 1. bord zonsuz, 2. ve 3. bordlarda dağıtıcının tarafı zonda, 4. bordda herkes zonda.
- **Chicago Rubber**: parsiyel taşınır; oyun bonusu zonsuz 300, zonda 500. Dördüncü bordun sonunda parsiyeli olan taraf +100 alır.
- **Chicago Duplicate**: her bord duplicate puanıyla yazılır, toplam dört bord boyunca tutulur.

## Skor penceresi
- Bord bitince **Bu Elin Puanlaması** bordları sırayla, zonu, çizgi üstü/altı puanı ve toplamı gösterir.
- **Temizle** yeni rubber ya da yeni Chicago başlatır. Oyun türünü değiştirince oturum puanları da sıfırlanır.
- Aynı bordu **Replay** ile tekrar oynarsanız son sonuç öncekinin yerine geçer.

## Henüz yok
- **Tek El** türü.
- Yarım kalan rubberın puanlanması ve onör puanları.
