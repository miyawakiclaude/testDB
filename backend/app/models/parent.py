from datetime import date, datetime
from uuid import UUID, uuid4

from sqlalchemy import Date, DateTime, ForeignKey, LargeBinary, String, Text
from sqlalchemy.orm import Mapped, mapped_column

from app.models import Base


class Parent(Base):
    __tablename__ = "parents"

    id: Mapped[UUID] = mapped_column(primary_key=True, default=uuid4)
    family_id: Mapped[UUID] = mapped_column(ForeignKey("families.id", ondelete="CASCADE"))
    display_name: Mapped[str] = mapped_column(String(80))
    date_of_birth: Mapped[date | None] = mapped_column(Date(), nullable=True)
    address_region: Mapped[str | None] = mapped_column(String(40), nullable=True)
    care_level: Mapped[str | None] = mapped_column(String(20), nullable=True)
    care_manager_name: Mapped[str | None] = mapped_column(String(80), nullable=True)
    care_manager_contact_enc: Mapped[bytes | None] = mapped_column(LargeBinary, nullable=True)
    notes: Mapped[str | None] = mapped_column(Text, nullable=True)
    created_at: Mapped[datetime] = mapped_column(DateTime(timezone=True))
    updated_at: Mapped[datetime] = mapped_column(DateTime(timezone=True))
