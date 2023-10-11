from foundationallm.langchain.language_models import LanguageModelBase

class ChatModelBase(LanguageModelBase):
    openai_api_base = str = None
    openai_api_key: str = None
    openai_api_type: str
    openai_api_version: str