from pydantic import BaseModel

class DataSourceConfiguration(BaseModel):
    prompt_tempate: str