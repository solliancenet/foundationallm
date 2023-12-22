from enum import Enum

class CacheFrequency(Enum):

    Minutes15 = 1
    Minutes30 = 2
    Hourly = 3
    Daily = 4
    Weekly = 5
    Monthly = 6
    Yearly = 7