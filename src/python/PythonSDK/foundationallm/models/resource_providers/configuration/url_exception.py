from pydantic import BaseModel, Field

class UrlException(BaseModel):
    user_principal_name: str = Field(description="The user principal name.")
    url: str = Field(description="The alternative URL.")
    enabled: bool = Field(default=False, description="Whether the URL exception is enabled.")
