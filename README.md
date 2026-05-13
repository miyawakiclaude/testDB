# AI遠距離介護コーディネーター

50-60代の働く子世代が、地方に住む80代の親を兄弟姉妹と協力して遠距離介護するための、AIエージェント駆動のネイティブアプリ。

## 解く課題

- 親の医療・服薬・生活情報が断片化（薬手帳・通院記録・ケアマネ連絡が別々）
- 兄弟姉妹間で介護方針が対立、誰が何をするかでもめる
- 「気づいたら認知症」を回避するための継続的な見守りが手作業
- 通院前に医師に渡せる症状サマリーを作る時間がない

## コア機能（MVP）

- 家族グループ（親を中心に兄弟姉妹を招待）
- 親カルテ（病歴・服薬・保険・ケアマネ）
- AI週次サマリー（日曜夜に「今週の親」を配信）
- 医師連絡パック（通院前1枚生成）
- 兄弟対立の中立調停（AIが論点整理）
- 大選択の意思決定支援（施設入所か在宅か等）
- タスク公平配分（通院当番など）

## 技術スタック

| 層 | 技術 |
|---|---|
| モバイル | Flutter (iOS + Android) |
| バックエンド | Python 3.12 + FastAPI |
| AI | Claude Agent SDK (`claude-sonnet-4-6`) |
| DB | Supabase (Postgres + pgvector + RLS) |
| 課金 | RevenueCat |
| 配備 | Google Cloud Run (asia-northeast1) |
| HIL コンソール | Next.js |

## ディレクトリ

```
backend/        # FastAPI + Claude Agent SDK
mobile/         # Flutter app (Phase 1.5)
admin/          # HIL レビューコンソール (Phase 2)
docs/legal/     # 規約・AI開示・医療免責文言
archive/        # 旧 testDB の資産（無関係）
```

## マネタイズ

- 月額 ¥2,980 / 家族（兄弟3人で割れば1人¥1,000相当）
- 6か月で 340 家族 = MRR ¥1,013,000 が目標

## 詳細プラン

`/root/.claude/plans/100-ai-dapper-patterson.md` 参照。
