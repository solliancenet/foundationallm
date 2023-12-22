import os
import pandas as pd
from io import BytesIO, StringIO
from foundationallm.storage import StorageManagerBase

class LocalStorageManager(StorageManagerBase):
    def __init__(self, base_path):
        self.base_path = base_path
    
    def get_file_path(self, key):
        
        if (not key.startswith('\\')):
            key = '\\' + key

        return self.base_path + key
    
    def file_exists(self, path):
        return os.path.exists(self.get_file_path(path))
    
    def read_file_content(self, path):
        with open(self.get_file_path(path), "rb") as f:
            return f.read()
    
    def write_file_content(self, path, content, overwrite=False):
        
        folder_path = os.path.dirname(self.get_file_path(path))
        
        if not os.path.exists(folder_path):
            os.makedirs(folder_path)

        if (type(content) is str):
            content = content.encode()
        
        with open(self.get_file_path(path), "wb") as f:
            f.write(content)
    
    def delete_file(self, path):
        os.remove(self.get_file_path(path))

    def read_dataframe(self, path, format='csv', containerName=None, root_path=None):

        if format == 'csv':
            return pd.read_csv(StringIO(self.read_file_content(path).decode('utf-8')))
        elif format == 'parquet':
            file_content = self.read_file_content(path)
            table = pq.read_table(BytesIO(file_content))
            df = table.to_pandas()
            return df

        return None

    def write_dataframe(self, path, df, format='csv', overwrite=True, containerName=None, root_path=None,
                        remove_duplicate_columns=False, lease=None):

        if ( df is None):
            df = pd.DataFrame()

        if ( remove_duplicate_columns):
            df = df.loc[:,~df.columns.duplicated()].copy()

        if format == 'csv':
            self.write_file_content(path,
                df.to_csv(index=False).encode(),
                overwrite=overwrite)
        elif format == 'parquet':
            self.write_file_content(path,
                df.to_parquet(engine = 'pyarrow', index=False), overwrite=overwrite)