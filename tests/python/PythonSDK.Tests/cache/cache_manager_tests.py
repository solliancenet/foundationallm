from datetime import datetime
import os
import sys
import pandas as pd

from foundationallm.cache import CacheManager
from foundationallm.storage import LocalStorageManager

local_storage_manager = LocalStorageManager('./.cache')
cache = CacheManager(layer_one_storage_manager=local_storage_manager, layer_two_storage_manager=None)

default_agent_filename = "default.json"
default_agent_data = {
	"name": "default",
	"type": "blob-storage",
	"description": "Useful for answering questions from users.",
	"allowed_data_source_names": [ "about-foundationallm" ],
	"language_model": {
		"type": "openai",
		"provider": "microsoft",
		"temperature": 0
	}
}

# Persist the default agent to the cache.
cache.set_cached_data(cache_key_values=default_agent_filename, data=default_agent_data)

# Retrieve the default agent from the cache.
cached_default_agent_data = cache.get_cached_data(default_agent_filename)

# The cached default agent data must match the original default agent data.
assert cached_default_agent_data == default_agent_data

# Purge the default agent from the cache.
cache.purge_cached_data(default_agent_filename)

# The cached default agent should no longer be in the cache.
assert cache.get_cached_data(default_agent_filename) is None

# Create a dataframe to cache.
df = pd.DataFrame({
    'date': [datetime(2023, 11, 1), datetime(2023, 11, 2), datetime(2023, 11, 3)],
    'open': [1, 2, 3],
    'high': [4, 5, 6],
    'low': [7, 8, 9],
    'close': [10, 11, 12],
    'volume': [13, 14, 15]
})

# Create a metadata object for the dataframe that serves as the cache key.
df_metadata = {
    'symbol': 'AAPL',
    'interval': '1d'
}

# Persist the dataframe to the cache.
cache.set_cached_data(cache_key_values=df_metadata, data=df)

# Retrieve the dataframe from the cache.
cached_df = cache.get_cached_data(df_metadata)

# The cached dataframe must match the original dataframe.
assert cached_df.equals(df)
