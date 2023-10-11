from foundationallm.langchain.language_models.openai import LanguageModelBase

class OpenAIModelBase(LanguageModelBase):
    openai_api_base = str
    