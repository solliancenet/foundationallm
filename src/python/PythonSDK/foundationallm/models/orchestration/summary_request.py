from .orchestration_request_base import OrchestrationRequestBase

class SummaryRequest(OrchestrationRequestBase):
    prompt_template: str = """Write a concise two word summary of the following:
    "{text}"
    CONCISE SUMMARY IN TWO WORDS:"""
