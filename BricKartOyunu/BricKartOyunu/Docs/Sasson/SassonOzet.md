# Sasson (SAS) Sistemi: Proje Özeti

Kaynak: "sassonABYY" (kitabın kendisi repoda tutulmaz; `E:\Bridge Manuel\BriçEG` altındadır). Sayfa numaraları kitaptaki içindekiler tablosuna göredir.

## Sistemin tanımı
Doğal 5'li majör sistemi, kısa ♣ açılışı (SAS).

| Açılış | Anlam |
|---|---|
| 1♣ | 2+ ♣, 11-22 HCP (dengeli 12-14, tek renkli, iki renkli, 4-4-4-1) |
| 1♦ | 4+ ♦, 11-22 HCP |
| 1♥ / 1♠ | 5+ kart, 11-22 HCP |
| 1NT | (14)15-17(18) HCP; kitapta ayrıca 12-14 zayıf NT versiyonu var (s.75-90) |

Kompetitif ilkeler: her seviyedeki DBL genelde negatif, Inverted DBL, Forcing Pas kuralları (kitap s.6-7).

## Projede nasıl karşılığı var
- Hazır profil: `OrtaklikAnlasmasi.Sasson()` ("Sasson (SAS - Kısa Sinek)"), 2/1 Orta profilinin üzerine `MinorShortClub = 1` ve aşağıdaki dört ek.
- Kitaptan eklenen (`Class/Bidding/Conventions/Mudahale/RekabetEkleri.cs`): Good-Bad 2NT (s.95), Inverted DBL, Mixed Raise, FSJS (s.97-105).
- Zaten vardı: Lebensohl (s.93), Rubensohl (s.104), Cappelletti/Michaels, Jacoby 2NT, Splinter vb.

## Henüz motorda olmayanlar
| Konu | Kitap yeri |
|---|---|
| SAS röleleri (iki renkli el sorgulama, 1. step röle, 2. step tutuş) | s.91 |
| 2♣ Röle (açanın 1 seviyesindeki renk rebidine cevap) | s.91-92 |
| 2NTt Röle (açan 3♣ der; cevapçı Pas / renk ile işaret verir) | s.92 |
| Çift yönlü Checkback Stayman (1NT rebidine 2♣/2♦ röleleri) | s.92-93 |
| Müdahale sonrası transfer cevapları, Rubensohl uyarlaması | s.97-105 |
| Forcing Pas'ın tam kuralları | s.7 |
| 12-14 zayıf NT versiyonuna özgü cevaplar | s.75-90 |

## Notlar
- Kitabın metni OCR'dan geldiği için bazı sembol ve rakamlar karışık; uygulamadan önce ilgili sayfa kitaptan doğrulanmalı.
- Varsayılan motor dünya standardı kalır; Sasson yalnızca seçilebilir bir profildir.
