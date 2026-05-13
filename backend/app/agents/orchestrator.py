from uuid import UUID

from claude_agent_sdk import ClaudeAgentOptions, query

from app.agents.prompts.system import ORCHESTRATOR_SYSTEM
from app.config import get_settings
from app.tools.family_tools import FAMILY_TOOLS_SERVER


async def handle_family_query(family_id: UUID, user_message: str) -> str:
    settings = get_settings()
    options = ClaudeAgentOptions(
        model=settings.anthropic_model,
        system_prompt=ORCHESTRATOR_SYSTEM,
        mcp_servers={"family": FAMILY_TOOLS_SERVER},
        allowed_tools=[
            "mcp__family__get_family_state",
            "mcp__family__get_recent_messages",
            "mcp__family__get_parent_profile",
            "mcp__family__get_medical_history",
            "mcp__family__submit_for_human_review",
        ],
        setting_sources=None,
        max_turns=8,
    )

    chunks: list[str] = []
    prompt = f"[family_id={family_id}]\n{user_message}"
    async for event in query(prompt=prompt, options=options):
        text = getattr(event, "text", None)
        if text:
            chunks.append(text)
    return "".join(chunks)
