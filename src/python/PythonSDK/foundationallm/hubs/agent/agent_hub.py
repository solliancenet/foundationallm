from foundationallm.hubs.agent import AgentRepository, AgentResolver
from foundationallm.hubs import BaseHub

class AgentHub(BaseHub):
    def __init__(self):
        self.repository = AgentRepository()
        self.resolver = AgentResolver()