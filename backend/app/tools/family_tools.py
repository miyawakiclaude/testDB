from typing import Any
from uuid import UUID

from claude_agent_sdk import create_sdk_mcp_server, tool


@tool(
    "get_family_state",
    "家族グループの基本情報（メンバー、親本人、ケアマネ）を返す",
    {"family_id": str},
)
async def get_family_state(args: dict[str, Any]) -> dict[str, Any]:
    _ = UUID(args["family_id"])
    raise NotImplementedError("DB integration pending")


@tool(
    "get_recent_messages",
    "家族チャットの直近Nメッセージを返す",
    {"family_id": str, "limit": int},
)
async def get_recent_messages(args: dict[str, Any]) -> dict[str, Any]:
    _ = UUID(args["family_id"])
    raise NotImplementedError("DB integration pending")


@tool(
    "get_parent_profile",
    "親本人のカルテ（病歴・服薬・保険・ケアマネ）を返す",
    {"family_id": str},
)
async def get_parent_profile(args: dict[str, Any]) -> dict[str, Any]:
    _ = UUID(args["family_id"])
    raise NotImplementedError("DB integration pending")


@tool(
    "get_medical_history",
    "親の医療イベント履歴（受診・診断・処方変更）を期間指定で返す",
    {"family_id": str, "days": int},
)
async def get_medical_history(args: dict[str, Any]) -> dict[str, Any]:
    _ = UUID(args["family_id"])
    raise NotImplementedError("DB integration pending")


@tool(
    "get_medication_log",
    "服薬ログを返す（飲み忘れ・追加・中止）",
    {"family_id": str, "days": int},
)
async def get_medication_log(args: dict[str, Any]) -> dict[str, Any]:
    _ = UUID(args["family_id"])
    raise NotImplementedError("DB integration pending")


@tool(
    "submit_for_human_review",
    "出力の信頼度が低い、または機微な内容を含むため、人間レビュー待ち行列に投入する",
    {"family_id": str, "kind": str, "payload": str, "confidence": float},
)
async def submit_for_human_review(args: dict[str, Any]) -> dict[str, Any]:
    _ = UUID(args["family_id"])
    raise NotImplementedError("review queue integration pending")


FAMILY_TOOLS_SERVER = create_sdk_mcp_server(
    name="family",
    version="0.1.0",
    tools=[
        get_family_state,
        get_recent_messages,
        get_parent_profile,
        get_medical_history,
        get_medication_log,
        submit_for_human_review,
    ],
)
