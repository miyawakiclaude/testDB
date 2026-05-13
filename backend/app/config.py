from functools import lru_cache

from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", extra="ignore")

    environment: str = Field(default="dev")
    database_url: str
    supabase_jwt_secret: str
    anthropic_api_key: str
    anthropic_model: str = Field(default="claude-sonnet-4-6")
    revenuecat_webhook_secret: str = Field(default="")
    encryption_key: str
    cloud_tasks_queue: str = Field(default="")
    cloud_tasks_location: str = Field(default="asia-northeast1")
    sentry_dsn: str = Field(default="")


@lru_cache
def get_settings() -> Settings:
    return Settings()
