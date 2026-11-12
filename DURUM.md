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
