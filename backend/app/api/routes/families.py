from uuid import UUID

from fastapi import APIRouter, HTTPException
from pydantic import BaseModel

router = APIRouter()


class CreateFamilyRequest(BaseModel):
    name: str
    primary_parent_name: str


class FamilyResponse(BaseModel):
    id: UUID
    name: str


@router.post("", response_model=FamilyResponse)
async def create_family(_: CreateFamilyRequest) -> FamilyResponse:
    raise HTTPException(status_code=501, detail="not implemented")


@router.get("/{family_id}", response_model=FamilyResponse)
async def get_family(family_id: UUID) -> FamilyResponse:
    raise HTTPException(status_code=501, detail="not implemented")
