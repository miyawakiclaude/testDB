# backend

AI遠距離介護コーディネーター バックエンド（FastAPI + Claude Agent SDK）。

## セットアップ

```bash
cd backend
python -m venv .venv
source .venv/bin/activate
pip install -e ".[dev]"
cp .env.example .env  # 値を埋める
```

## ローカル起動

```bash
uvicorn app.main:app --reload
```

`http://localhost:8000/healthz` で疎通確認。

## テスト

```bash
pytest
```

## ディレクトリ

- `app/main.py` — FastAPI エントリ
- `app/agents/` — Claude Agent SDK のエージェント群
  - `orchestrator.py` — 中核エージェント
  - `situation_summarizer.py` — 週次サマリー
  - その他のサブエージェントは Phase 2 で追加
- `app/tools/family_tools.py` — エージェントから呼ぶ `@tool` 群
- `app/models/` — SQLAlchemy
- `supabase/migrations/0001_init.sql` — 初期スキーマと RLS
- `data/care_eval_scenarios.yaml` — ゴールデンセット（pytestで検証）

## Phase 1 のスコープ

- [x] FastAPI 骨組み
- [x] Claude Agent SDK 統合の枠組み
- [x] DB スキーマと RLS
- [x] ゴールデンセット 10 シナリオ
- [ ] Supabase Auth JWT 検証ミドルウェア（W2）
- [ ] DB 接続を実装し `@tool` を完成（W2）
- [ ] `situation_summarizer` を実データで動作させる（W3）
- [ ] Cloud Run / Cloud Tasks 配備（W4）
