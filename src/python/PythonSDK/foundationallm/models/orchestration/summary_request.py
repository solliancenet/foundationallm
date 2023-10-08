from .request_base import RequestBase

class SummaryRequest(RequestBase):
    prompt_template: str = """Write a concise two word summary of the following:
    "{text}"
    CONCISE SUMMARY IN TWO WORDS:"""
