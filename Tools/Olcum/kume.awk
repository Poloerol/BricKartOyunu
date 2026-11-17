# Temel Mantik (Fallback) kumeleri: hangi teklif durumlarinda yedek mantik devrede ve bordlar ne kadar kotu?
# Kullanim: awk -v ORT=5.19 -f kume.awk taban.tsv attr.tsv
#   taban.tsv : bord TAB ihale TAB sapma   (Taban.exe)
#   attr.tsv  : bord TAB ihale TAB konvansiyon1|konvansiyon2|...   (Attr.exe; teklif basina konvansiyon adi)
# Anahtar: teklifi veren oyuncunun gordugu ihale (Pas'lar atlanir; M=ben, P=ortak, O=rakip) + yedek mantigin teklifi.
#   Ornek:  "P:1S [2D]"  = ortak 1S acti, ben (yedek mantikla) 2D dedim.   "M:1H O:2C [Dbl]" vb.
# Cikti sutunlari: bord_sayisi, teklif_sayisi, ort_sapma, fazla_sapma(ort. ustu toplam), anahtar  (SIRALAMAYI cagiran yapar)
BEGIN { FS = "\t" }
FNR == NR { sap[$1] = $3; next }
{
  b = $1; if (!(b in sap)) next
  n = split($2, t, " "); m = split($3, c, "|")
  if (n != m) next
  for (i = 1; i <= n; i++) {
    if (c[i] !~ /Fallback/ || t[i] == "Pas") continue
    s = ""
    for (j = 1; j < i; j++) {
      if (t[j] == "Pas") continue
      d = (i - j) % 4
      who = (d == 0) ? "M" : (d == 2) ? "P" : "O"
      s = s (s == "" ? "" : " ") who ":" t[j]
    }
    key = (s == "" ? "(acilis)" : s) " [" t[i] "]"
    cnt[key]++
    if (!((key SUBSEP b) in seen)) { seen[key, b] = 1; bc[key]++; bs[key] += sap[b] }
  }
}
END {
  for (k in bc) { ex = bs[k] - ORT * bc[k]; printf "%d\t%d\t%.2f\t%.0f\t%s\n", bc[k], cnt[k], bs[k] / bc[k], ex, k }
}
