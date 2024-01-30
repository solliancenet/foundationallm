import pytest
from foundationallm.models.orchestration import MessageHistoryItem
from foundationallm.langchain.message_history import build_message_history

@pytest.fixture  
def message_history():  
    history = [MessageHistoryItem(sender="User", text="user chat 1"),  
            MessageHistoryItem(sender="Agent", text="agent chat 1"),  
            MessageHistoryItem(sender="User", text="user chat 2"),  
            MessageHistoryItem(sender="Agent", text="agent chat 2")]
    return history
    
class MessageHistoryTests:

    def test_build_message_history_content(self, message_history):
        history = build_message_history(message_history)        
        expected = "\n\nChat History:\nUser: user chat 1\nAgent: agent chat 1\nUser: user chat 2\nAgent: agent chat 2\n\n\n"
        assert history == expected
    
    def test_build_message_history_empty(self):
        history = build_message_history(None)
        assert history == ""

    def test_build_message_history_last_n_messages(self, message_history):
        history = build_message_history(messages=message_history, message_count=2)
        expected = "\n\nChat History:\nUser: user chat 2\nAgent: agent chat 2\n\n\n"
        assert history == expected

    def test_build_message_history_last_n_messages_out_of_range(self, message_history):
        history = build_message_history(messages=message_history, message_count=10)
        expected = "\n\nChat History:\nUser: user chat 1\nAgent: agent chat 1\nUser: user chat 2\nAgent: agent chat 2\n\n\n"
        assert history == expected
