from foundationallm.config import Configuration

class APIKeyValidator:
    """
    Validates an API key.
    """
    
    def __init__(self, api_key_name: str, config: Configuration):
        self.api_key_value = config.get_value(api_key_name)

    def validate_api_key(self, x_api_key: str) -> bool:
        return x_api_key == self.api_key_value
