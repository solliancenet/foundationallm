from foundationallm.hubs.prompt import PromptRepository, PromptResolver
from foundationallm.hubs import HubBase

class PromptHub(HubBase):
    """The PromptHub class is responsible for resolving prompts."""

    def __init__(self):
        self.repository = PromptRepository()
        self.resolver = PromptResolver()