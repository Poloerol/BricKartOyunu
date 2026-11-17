# Yedek mantigin (Temel Mantik) verdigi SON PAS'larda "birlesik HP yuksek ama kontrat parsiyel" sizintisi.
# Kullanim: awk -v ORT=5.19 -v MINHP=25 -f pas_sizinti.awk elli_taban.tsv attr.tsv
#   elli_taban.tsv : bord TAB dagitici TAB ihale TAB sapma TAB elN TAB elE TAB elS TAB elW   (ds.tsv bicimi; ilk 8 sutun)
#   attr.tsv       : bord TAB ihale TAB konvansiyon|konvansiyon...
# Son teklifi veren tarafin ORTAGI (son tekliften 2 sira sonra) yedek mantikla pas dediyse ve taraf birlesik HP >= MINHP, kontrat 3NT/4M altindaysa sayilir.
function hcp(h,  i,c,s,a,n,t,x){s=0;n=split(h,a," ");for(i=1;i<=n;i++){t=substr(a[i],2);gsub(/[^AKQJ]/,"",t);for(c=1;c<=length(t);c++){x=substr(t,c,1);s+=(x=="A")?4:(x=="K")?3:(x=="Q")?2:1}}return s}
BEGIN { FS = "\t" }
FNR == NR { sap[$1] = $4; di[$1] = index("NESW", $2) - 1; for (q = 1; q <= 4; q++) H[$1, q] = $(4 + q); next }
{
  b = $1; if (!(b in sap)) next
  n = split($2, t, " "); split($3, c, "|"); if (n < 4) next
  f = 0; for (i = n; i >= 1; i--) if (t[i] != "Pas") { f = i; break }
  if (f == 0 || f + 2 > n) next
  p = f + 2                                   # ortak(son teklifi verenin) pas sirasi
  if (c[p] !~ /Fallback/ || t[p] != "Pas") next
  seat = (di[b] + p - 1) % 4 + 1; part = (seat + 1) % 4 + 1
  tot = hcp(H[b, seat]) + hcp(H[b, part])
  if (tot < MINHP) next
  lv = substr(t[f], 1, 1) + 0; su = substr(t[f], 2)
  if (t[f] == "Dbl" || t[f] == "Rdbl") next
  game = (t[f] ~ /^3NT/ || (lv >= 4 && su != "C" && su != "D") || lv >= 5 || t[f] ~ /^[67]/)
  if (game) next
  s = ""
  for (j = 1; j < p; j++) { if (t[j] == "Pas") continue; d = (p - j) % 4; who = (d == 0) ? "M" : (d == 2) ? "P" : "O"; s = s (s == "" ? "" : " ") who ":" t[j] }
  key = s " [Pas]"
  bc[key]++; bs[key] += sap[b]; hp[key] += tot
}
END { for (k in bc) printf "%d\t%.2f\t%.1f\t%.0f\t%s\n", bc[k], bs[k] / bc[k], hp[k] / bc[k], bs[k] - ORT * bc[k], k }
