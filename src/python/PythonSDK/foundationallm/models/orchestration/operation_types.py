from enum import Enum

class OperationTypes(str, Enum):
   """Enumerator of the Operation Types."""
   ASSISTANTS_API = "assistants_api"
   CHAT = "chat"
   COMPLETIONS = "completions"
   IMAGE_SERVICES = "image_services"
