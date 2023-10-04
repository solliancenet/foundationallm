from foundationallm.hubs.prompt import PromptRepository, PromptResolver
from foundationallm.hubs import BaseHub

class PromptHub(BaseHub):
    """The PromptHub class is responsible for managing and resolving prompts."""

    def __init__(self):
        self.repository = PromptRepository()
        self.resolver = PromptResolver()