#!/usr/bin/env bash
# ゲームのロジックを一通り実際に動かして、実行時の不具合を洗う。
# 使い方: ./Tools/smoketest.sh
set -u
cd "$(dirname "$0")/.."
command -v mcs >/dev/null 2>&1 || { echo "mcs が必要です (apt-get install mono-mcs)" >&2; exit 127; }
command -v mono >/dev/null 2>&1 || { echo "mono が必要です" >&2; exit 127; }

out="$(mktemp -d)/smoke.exe"
mcs -target:exe -langversion:Latest -out:"$out" -nowarn:0169,0414,0649,0108,0219,0162 \
    $(find Assets -name '*.cs') Tools/UnityStubs.cs Tools/sim/SmokeTest.cs || exit 1
mono "$out"
