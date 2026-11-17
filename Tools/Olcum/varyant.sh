#!/bin/bash
# Kullanim: varyant.sh ad [ORTAM=DEGER ...]   -> Tools/Olcum/veri/taban_a|b|c.tsv uzerinde 3 paralel Measure; ozet satirlari yazar.
# Ornek:    varyant.sh nt3m8 BK_NT3M=8
KOK="$(cd "$(dirname "$0")/../.." && pwd)"; OUT="${OUT:-$KOK/Tools/Olcum/out2}"; V="$KOK/Tools/Olcum/veri"
ad="$1"; shift
for k in a b c; do
  ( cd "$OUT" && env "$@" ./Measure.exe "$(cygpath -w "$V/taban_$k.tsv")" "$(cygpath -w "$V/m_${ad}_$k.tsv")" > "$V/m_${ad}_$k.log" 2>&1 ) &
done
wait
for k in a b c; do echo "$ad [$k] $(tail -n 1 "$V/m_${ad}_$k.log")"; done
awk -F'\t' -v ad="$ad" 'FNR>0{} END{}' /dev/null
