from uuid import UUID

from fastapi import APIRouter, HTTPException

from app.agents.situation_summarizer import generate_weekly_summary

router = APIRouter()


@router.post("/{family_id}/weekly")
async def trigger_weekly_summary(family_id: UUID) -> dict[str, str]:
    try:
        result = await generate_weekly_summary(family_id)
    except NotImplementedError as exc:
        raise HTTPException(status_code=501, detail=str(exc)) from exc
    return {"family_id": str(family_id), "summary": result}
