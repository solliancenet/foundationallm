import re

class ObjectUtils:

    @staticmethod
    def pascal_to_snake(name):  
        # Convert PascalCase or camelCase to snake_case  
        s1 = re.sub('(.)([A-Z][a-z]+)', r'\1_\2', name)  
        return re.sub('([a-z0-9])([A-Z])', r'\1_\2', s1).lower()  

    @staticmethod
    def translate_keys(obj):  
        if isinstance(obj, dict):  
            new_dict = {}  
            for key, value in obj.items():  
                new_key = ObjectUtils.pascal_to_snake(key)  
                new_dict[new_key] = ObjectUtils.translate_keys(value)  # Recursively apply to values  
            return new_dict  
        elif isinstance(obj, list):  
            return [ObjectUtils.translate_keys(item) for item in obj]  # Apply to each item in the list  
        else:  
            return obj  # Return the item itself if it's not a dict or list
