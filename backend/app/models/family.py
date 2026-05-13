from datetime import datetime
from enum import StrEnum
from uuid import UUID, uuid4

from sqlalchemy import DateTime, Enum, ForeignKey, String
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models import Base


class FamilyRole(StrEnum):
    PRIMARY = "primary"
    SIBLING = "sibling"
    SPOUSE = "spouse"
    OBSERVER = "observer"


class SubscriptionTier(StrEnum):
    FREE = "free"
    STANDARD = "standard"
    PREMIUM = "premium"


class Family(Base):
    __tablename__ = "families"

    id: Mapped[UUID] = mapped_column(primary_key=True, default=uuid4)
    name: Mapped[str] = mapped_column(String(120))
    subscription_tier: Mapped[SubscriptionTier] = mapped_column(
        Enum(SubscriptionTier), default=SubscriptionTier.FREE
    )
    created_at: Mapped[datetime] = mapped_column(DateTime(timezone=True))

    members: Mapped[list["FamilyMember"]] = relationship(back_populates="family")


class FamilyMember(Base):
    __tablename__ = "family_members"

    id: Mapped[UUID] = mapped_column(primary_key=True, default=uuid4)
    family_id: Mapped[UUID] = mapped_column(ForeignKey("families.id", ondelete="CASCADE"))
    user_id: Mapped[UUID]
    display_name: Mapped[str] = mapped_column(String(80))
    role: Mapped[FamilyRole] = mapped_column(Enum(FamilyRole))
    joined_at: Mapped[datetime] = mapped_column(DateTime(timezone=True))

    family: Mapped[Family] = relationship(back_populates="members")
