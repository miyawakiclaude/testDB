#!/usr/bin/env bash
# Unity を開かずに C# の型チェックだけ走らせる。
#
# Assets/ 以下のスクリプトを、Tools/UnityStubs.cs に置いた Unity API の最小スタブと
# つき合わせてコンパイルする。実行はできないが、綴り間違い・引数の数・存在しない
# メンバーといったコンパイルエラーは、エディタを起動する前にここで見つかる。
#
# 必要なもの: mono の C# コンパイラ
#   Debian / Ubuntu:  sudo apt-get install mono-mcs
#   macOS (Homebrew): brew install mono
#
# 使い方:  ./Tools/typecheck.sh
#
# 注意: Tools/ は Assets/ の外にあるので Unity は読み込まない。スタブがゲームの
# ビルドに混ざることはない。スタブは実物の Unity ではないため、ここが通っても
# Unity 側の API 変更まで保証はできない。あくまで一次検査として使う。

set -u
cd "$(dirname "$0")/.."

if ! command -v mcs >/dev/null 2>&1; then
  echo "mcs が見つかりません。mono-mcs を入れてください。" >&2
  exit 127
fi

out="$(mktemp -d)/typecheck.dll"
files=$(find Assets -name '*.cs' | sort)
count=$(echo "$files" | wc -l | tr -d ' ')

echo "Assets/ の ${count} ファイルを検査します..."
# 未使用フィールドなど、Unity では通常の書き方になる警告は落とす。
if mcs -target:library -langversion:Latest -out:"$out" \
      -nowarn:0169,0414,0649,0108,0219,0162 \
      $files Tools/UnityStubs.cs; then
  echo "型チェック OK"
else
  echo "型チェックで問題が見つかりました" >&2
  exit 1
fi
