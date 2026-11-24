# Analiz araçları

Masadaki **Eylemler** menüsünde, bordu bitirdikten ya da oynarken kullanabileceğiniz analiz araçları bulunur. Çoğu **çift dummy** (double dummy) hesabına dayanır: dört el de açıkmış gibi ve her oyuncu en iyi oynarmış gibi sonuç bulunur. Gerçek masada saklı eller göründüğü için bu sonuç iyimserdir.

## Çift Dummy Analizi / Double Dummy Search
- Bordun çift dummy löve tablosunu verir: her oyuncunun her kozda kaç löve alabileceği.
- **Double Dummy Search** oynanmakta olan pozisyonu aranır; hangi kartın en iyi olduğunu gösterir.

## İhale Analizi
- **Par kontratı** (iki taraf da mükemmel oynarsa ortaya çıkan kontrat ve skor), en çok löve yapılan kontrat ve her taraf için en iyi skorlu kontratlar listelenir. **Yazdır** ile çıktı alırsınız.

## Oyun Analizi (hamle hamle)
- O ana kadar oynanan her kartı çift dummy ile değerlendirir: kaybedilen löve ve en iyi kart(lar) gösterilir. Kırmızı satırlar deklaran tarafı, mavi satırlar savunmadır.
- Bord sonrası "hangi kartı yanlış oynadım?" sorusunun cevabıdır. Hesap birkaç saniye ile bir dakika sürebilir ve arka planda yapılır.

## Teklif Açıklamaları
- İhaleyi tüm ellerle yeniden oynatır; her teklifin konvansiyon adı, açıklaması, elin özeti (HP, dağılım). Gerçek teklif motorun önerisinden farklıysa sarı satır ve motorun önerisi görünür. İnsan oyuncunun teklifinin gerçek anlamı bilinemediğinden açıklama motorun o elle vereceği çağrıya aittir.

## Oynama Karnesi ve Motor Karnesi
- **Oynama Karnesi**: ihale motorunun bulduğu kontratı kart oynama motorunun dört elde oynayıp çift dummy sonucuna göre ne kadar löve kaybettiğini ölçer.
- **Motor Karnesi**: ihale motorunun başarısını çift dummy par sonucuna göre toplu ölçer (geliştirici aracı).
- **Pratik Listesi**: konvansiyon pratiğinde seed kopyalamak ya da bord seçmek için.
