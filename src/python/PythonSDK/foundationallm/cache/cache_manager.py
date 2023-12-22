import os
import datetime
import pickle
import hashlib
from typing import Dict, Union, Optional

from .cache_frequency import CacheFrequency

from foundationallm.storage import StorageManagerBase

class CacheManager:
    def __init__(self, layer_one_storage_manager: StorageManagerBase,
                 layer_two_storage_manager: Optional[StorageManagerBase]=None):
        """
        Initializes the CacheManager. The first layer of caching is always used. The second layer of caching is optional and
        used as a fallback if the first layer of caching does not contain data.

        Args:
            layer_one_storage_manager (StorageManagerBase): The storage manager to use for the first layer of caching.
            layer_two_storage_manager (Optional[StorageManagerBase], optional): The storage manager to use for the second layer of caching. Defaults to None.
        
        Use cases:
        1. LocalStorageManager
        2. BlobStorageManager
        3. LocalStorageManager + BlobStorageManager

        When you assign a LocalStorageManager to the first layer and assign a BlobStorageManager to the second layer,
        the cache will check the local disk first. This is preferable in most cases because it's faster than checking
        the cloud storage. If the cache is not found on the local disk, it will check the cloud storage. If the cache
        is found on the cloud storage, it will be downloaded to the local disk and used for the cache. If the cache
        is not found on the cloud storage, the cache will be saved to the local disk and the cloud storage.

        """
        self.layer_one_storage_manager = layer_one_storage_manager
        self.layer_two_storage_manager = layer_two_storage_manager
    
    def __get_cache_key(self, cache_key_values: (Union[Dict, str])) -> str:
        """
        Generates a cache key from a dictionary of values or a single string value.

        Args:
            cache_key_values (Union[Dict, str]): A dictionary of values or a single string value to use in generating the cache key.

        Returns:
            str: The cache key as a hexadecimal string.
        """
        if isinstance(cache_key_values, dict):
            key_str = str(sorted(cache_key_values.items())).encode('utf-8')
        elif isinstance(cache_key_values, str):
            key_str = cache_key_values.encode('utf-8')
        else:
            raise TypeError('Expected dictionary or string type for cache_key_values parameter')

        return hashlib.sha256(key_str).hexdigest()
    
    def __get_cache_path(self, cache_key_values: (Union[Dict, str]),
                         cache_frequency: Optional[CacheFrequency]=None) -> str:
        """
        Generates a cache path from a dictionary of values or a single string value.
        Args:
            cache_key_values (Union[Dict, str]): A dictionary of values or a single string value to use in generating the cache key.
            cache_frequency (Optional[CacheFrequency], optional): The frequency of the cache. Defaults to None.
        """
        # Get the cache key
        cache_key = self.__get_cache_key(cache_key_values)
        file_name = f'{cache_key}.pkl'

        if cache_frequency is not None:
            prefix = ''
            check = datetime.datetime.utcnow()

            if cache_frequency == CacheFrequency.Daily:
                prefix = check.strftime('%Y-%m-%d')
            if cache_frequency == CacheFrequency.Hourly:
                prefix = check.strftime('%Y-%m-%d-%H')
            if cache_frequency == CacheFrequency.Monthly:
                prefix = check.strftime('%Y-%m')
            if cache_frequency == CacheFrequency.Yearly:
                prefix = check.strftime('%Y')
            if prefix == '':
                raise ValueError('Unrecognized cache frequency')
            cache_path = os.path.join(prefix, file_name)
        else:
            cache_path = file_name

        return cache_path
    
    def __read_cached_data(self, cache_path: str, storage_manager: StorageManagerBase) -> Optional[object]:
        if storage_manager.file_exists(cache_path):
            serialized_data = storage_manager.read_file_content(cache_path)
            return pickle.loads(serialized_data)

        return None
    
    def __write_data_to_cache(self, cache_path: str, serialized_data: bytes,
                              cache_key_values: (Union[Dict, str]), layer_one_only: bool = False):
        # Save the serialized data to the first caching layer
        self.layer_one_storage_manager.write_file_content(cache_path, serialized_data)
        self.__write_mapping_file(cache_path, cache_key_values, self.layer_one_storage_manager)
        
        # If a second caching layer is specified, save the serialized data to the second caching layer
        if self.layer_two_storage_manager is not None and not layer_one_only:
            self.layer_two_storage_manager.write_file_content(cache_path, serialized_data)
            self.__write_mapping_file(cache_path, cache_key_values, self.layer_two_storage_manager)

    def __write_mapping_file(self, cache_path: str, cache_key_values: Union[Dict, str],
                             storage_manager: StorageManagerBase):
        folder_path = os.path.dirname(cache_path)
        mapping_file_path = os.path.join(folder_path, "cache_mapping.txt")
        mapping_entry = f"{os.path.basename(cache_path)} | {cache_key_values}\n"

        if storage_manager.file_exists(mapping_file_path):
            mapping_file = storage_manager.read_file_content(mapping_file_path)
            mapping_file = mapping_file.decode("utf-8") if mapping_file is not None else ""
            # Exit if the cache key already exists
            for line in mapping_file.splitlines():
                if line.startswith(os.path.basename(cache_path)):
                    return

            mapping_file += mapping_entry
        else:
            mapping_file = mapping_entry

        storage_manager.write_file_content(mapping_file_path, mapping_file.encode("utf-8"))


    def set_cached_data(self, cache_key_values: (Union[Dict, str]), data,
                       cache_frequency: Optional[CacheFrequency]=CacheFrequency.Hourly,
                       layer_one_only: bool = False):
        cache_path = self.__get_cache_path(cache_key_values, cache_frequency)
        
        serialized_data = pickle.dumps(data)
        self.__write_data_to_cache(cache_path, serialized_data, cache_key_values, layer_one_only)

    def get_cached_data(self, cache_key_values: (Union[Dict, str]),
                        cache_frequency: Optional[CacheFrequency]=CacheFrequency.Hourly):
        cache_path = self.__get_cache_path(cache_key_values, cache_frequency)

        data = self.__read_cached_data(cache_path, self.layer_one_storage_manager)
        if data is not None:
            return data

        if self.layer_two_storage_manager is not None:
            data = self.__read_cached_data(cache_path, self.layer_two_storage_manager)
            if data is not None:
                # Save the data to the first caching layer since it was not found there
                self.set_cached_data(cache_key_values, data, cache_frequency, layer_one_only=True)
                return data

        return None
    
    def purge_cached_data(self, cache_key_values: (Union[Dict, str]),
                          cache_frequency: Optional[CacheFrequency]=CacheFrequency.Hourly):
        cache_path = self.__get_cache_path(cache_key_values, cache_frequency)

        if self.layer_one_storage_manager.file_exists(cache_path):
            self.layer_one_storage_manager.delete_file(cache_path)

        if self.layer_two_storage_manager is not None and self.layer_two_storage_manager.file_exists(cache_path):
            self.layer_two_storage_manager.delete_file(cache_path)
