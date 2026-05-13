from uuid import UUID

from claude_agent_sdk import ClaudeAgentOptions, query

from app.agents.prompts.system import SITUATION_SUMMARIZER_SYSTEM
from app.config import get_settings
from app.tools.family_tools import FAMILY_TOOLS_SERVER


async def generate_weekly_summary(family_id: UUID) -> str:
    settings = get_settings()
    options = ClaudeAgentOptions(
        model=settings.anthropic_model,
        system_prompt=SITUATION_SUMMARIZER_SYSTEM,
        mcp_servers={"family": FAMILY_TOOLS_SERVER},
        allowed_tools=[
            "mcp__family__get_recent_messages",
            "mcp__family__get_parent_profile",
            "mcp__family__get_medical_history",
            "mcp__family__get_medication_log",
        ],
        setting_sources=None,
        max_turns=4,
    )

    prompt = (
        f"family_id={family_id} の直近7日間のデータを取得し、"
        "システムプロンプトの6項目構造で『今週の親の状況』を作成してください。"
    )
    chunks: list[str] = []
    async for event in query(prompt=prompt, options=options):
        text = getattr(event, "text", None)
        if text:
            chunks.append(text)
    return "".join(chunks)
