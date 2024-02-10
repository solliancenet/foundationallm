from unittest.mock import Mock, sentinel, patch
from foundationallm.langchain.language_models.openai import OpenAIModel
from foundationallm.models.language_models import LanguageModelType
from foundationallm.langchain.language_models import LanguageModelFactory


def instantiate_language_model(language_model_type):
    language_model = Mock()
    language_model.type = language_model_type
    return language_model


def instantiate_language_model_factory(language_model):
    return LanguageModelFactory(language_model, sentinel.config)


class LanguageModelFactoryTests:
    """
    LanguageModelFactoryTests verifies that LanguageModelFactory provisions the correct Language Model based on the LanguageModelType enum.
    """

    def test_get_openai_model(self):
        with patch.object(OpenAIModel, "__init__", return_value=None) as constructor:
            language_model = instantiate_language_model(LanguageModelType.OPENAI)
            language_model_factory = instantiate_language_model_factory(language_model)
            assert type(language_model_factory.get_llm()) == OpenAIModel
            constructor.assert_called_once_with(config=sentinel.config)

    def test_get_fallback_model(self):
        with patch.object(OpenAIModel, "__init__", return_value=None) as constructor:
            language_model = instantiate_language_model("Meta")
            language_model_factory = instantiate_language_model_factory(language_model)
            assert type(language_model_factory.get_llm()) == OpenAIModel
            constructor.assert_called_once_with(config=sentinel.config)