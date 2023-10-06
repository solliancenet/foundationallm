from foundationallm.hubs.agent import AgentRepository, AgentResolver
from foundationallm.hubs import HubBase

class AgentHub(HubBase):
    def __init__(self):
        self.repository = AgentRepository()
        self.resolver = AgentResolver()