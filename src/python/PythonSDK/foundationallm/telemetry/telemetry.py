import sys
import logging
import platform
import foundationallm
from azure.monitor.opentelemetry import configure_azure_monitor
from logging import getLogger

from azure.monitor.opentelemetry.exporter import AzureMonitorTraceExporter
from opentelemetry import trace
from opentelemetry.trace import Span, Status, StatusCode, Tracer
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.resources import SERVICE_NAME, Resource, SERVICE_INSTANCE_ID, SERVICE_VERSION, SERVICE_NAMESPACE
from opentelemetry.sdk.trace.export import (
    BatchSpanProcessor,
    ConsoleSpanExporter,
)

from azure.monitor.opentelemetry.exporter import (  # pylint: disable=import-error,no-name-in-module
    ApplicationInsightsSampler,
    AzureMonitorLogExporter,
    AzureMonitorMetricExporter,
    AzureMonitorTraceExporter,
)

from opentelemetry.sdk._logs import LoggerProvider, LoggingHandler
from opentelemetry.sdk._logs.export import BatchLogRecordProcessor

from foundationallm.config import Configuration

from opentelemetry._logs import (
    get_logger_provider,
    set_logger_provider,
)

from opentelemetry.sdk._logs import (
    LoggerProvider,
    LoggingHandler,
)

from langchain.globals import set_debug, set_verbose

# Custom filter to exclude trace logs
class ExcludeTraceLogsFilter(logging.Filter):
    def filter(self, record):
        filter_out = 'applicationinsights' not in record.getMessage().lower()
        filter_out = filter_out and 'response status' not in record.getMessage().lower()
        filter_out = filter_out and 'transmission succeeded' not in record.getMessage().lower()
        return filter_out

class Telemetry:
    """
    Manages logging and the recording of application telemetry.
    """

    log_level : int = logging.WARNING
    langchain_log_level : int = logging.NOTSET
    api_name : str = None
    telemetry_connection_string : str = None

    @staticmethod
    def configure_monitoring(config: Configuration, telemetry_connection_string: str, api_name : str):
        """
        Configures monitoring and sends logs, metrics, and events to Azure Monitor.

        Parameters
        ----------
        config : Configuration
            Configuration class used for retrieving application settings from
            Azure App Configuration.
        telemetry_connection_string : str
            The connection string used to connect to Azure Application Insights.
        """

        Telemetry.telemetry_connection_string = config.get_value(telemetry_connection_string)
        Telemetry.api_name = api_name
        resource = Resource.create(
            {
                SERVICE_NAME: f"{Telemetry.api_name}",
                SERVICE_NAMESPACE : f"FoundationaLLM",
                SERVICE_VERSION: f"{foundationallm.__version__}",
                SERVICE_INSTANCE_ID: f"{platform.node()}"
            })

        # Configure Azure Monitor defaults
        configure_azure_monitor(
            connection_string=Telemetry.telemetry_connection_string,
            disable_offline_storage=True,
            disable_metrics=True,
            disable_tracing=False,
            disable_logging=False,
            resource=resource
        )

        #Configure telemetry logging
        Telemetry.configure_logging(config)

    @staticmethod
    def configure_logging(config: Configuration):
        #Get dotnet log level
        str_log_level = config.get_value("Logging:LogLevel:Default")

        #Get environment (prod vs dev)
        env = config.get_value("FOUNDATIONALLM_ENV")

        #Log output handlers
        handlers = []

        #map log level to python log level - console is only added for Information or higher
        if str_log_level == "Debug":
            Telemetry.log_level = logging.DEBUG
            set_debug(True)
            handlers.append(logging.StreamHandler())
        elif str_log_level == "Trace":
            Telemetry.log_level = logging.DEBUG
            handlers.append(logging.StreamHandler())
            set_verbose(True)
        elif str_log_level == "Information":
            Telemetry.log_level = logging.INFO
            handlers.append(logging.StreamHandler())
            set_debug(False)
        elif str_log_level == "Warning":
            set_debug(False)
            Telemetry.log_level = logging.WARNING
        elif str_log_level == "Error":
            set_debug(False)
            Telemetry.log_level = logging.ERROR
        elif str_log_level == "Critical":
            set_debug(False)
            Telemetry.log_level = logging.CRITICAL
        else:
            set_debug(False)
            Telemetry.log_level = logging.NOTSET

        #Logging configuration
        LOGGING = {
            'version': 1,
            'disable_existing_loggers': False,
            'formatters': {
                'default': {
                    'format': '[%(asctime)s] [%(levelname)s] %(name)s: %(message)s'
                },
                'standard': {
                    'format': '[%(asctime)s] [%(levelname)s] %(name)s: %(message)s'
                },
                'azure': {
                    'format': '%(name)s: %(message)s'
                },
                'error': {
                    'format': '[%(asctime)s] [%(levelname)s] %(name)s %(process)d::%(module)s|%(lineno)s:: %(message)s'
                }
            },
            'handlers': {
                'default': {
                    'level': Telemetry.log_level,
                    'formatter': 'standard',
                    'class': 'logging.StreamHandler',
                    'filters' : ['exclude_trace_logs'],
                    'stream': 'ext://sys.stdout',
                },
                'console': {
                    'level': Telemetry.log_level,
                    'formatter': 'standard',
                    'class': 'logging.StreamHandler',
                    'filters' : ['exclude_trace_logs'],
                    'stream': 'ext://sys.stdout'
                },
                "azure": {
                    'formatter': 'azure',
                    'level': Telemetry.log_level,
                    "class": "opentelemetry.sdk._logs.LoggingHandler",
                    'filters' : ['exclude_trace_logs'],
                }
            },
            'filters': {
                'exclude_trace_logs': {
                    '()': 'foundationallm.telemetry.ExcludeTraceLogsFilter',
                },
            },
            'loggers': {
                'azure': {  # Adjust the logger name accordingly
                    'level': Telemetry.log_level,  # Set to WARNING or higher
                    "class": "opentelemetry.sdk._logs.LoggingHandler",
                    'filters': ['exclude_trace_logs']
                },
                '': {
                    'handlers': ['console'],
                    'level': Telemetry.log_level,
                    'filters': ['exclude_trace_logs'],
                },
            },
            "root": {
                "handlers": ["azure", "console"],
                "level": Telemetry.log_level,
            }
        }

        #remove console if prod env (cut down on duplicate log data)
        if env == "prod":
            LOGGING['root']['handlers'] = ["azure"]

        #set the logging configuration
        logging.config.dictConfig(LOGGING)

    @staticmethod
    def get_logger(name: str = None, level: int = logging.INFO) -> logging.Logger:
        """
        Creates a logger by the specified name and logging level.

        Parameters
        ----------
        name : str
            The name to assign to the logger instance.
        level : int
            The logging level to assign.

        Returns
        -------
        Logger
            Returns a logger object with the specified name and logging level.
        """
        logger = logging.getLogger(name)

        #if telemetry is configured, add the azure monitor exporter to the logger - by default only the root logger gets it
        if Telemetry.api_name is not None and Telemetry.telemetry_connection_string is not None:

            logger_provider = get_logger_provider()

            if logger_provider is None:
                #set the service name
                resource = Resource.create(
                    {
                        SERVICE_NAME: f"{Telemetry.api_name}",
                        SERVICE_NAMESPACE : f"FoundationaLLM",
                        SERVICE_VERSION: f"{foundationallm.__version__}",
                        SERVICE_INSTANCE_ID: f"{platform.node()}"
                    })
                logger_provider = LoggerProvider(resource=resource)

            handler = LoggingHandler(logger_provider=logger_provider)

            exists = False

            for h in logger.handlers:
                if h.__class__ == handler.__class__:
                    exists = True
                    break

            if not exists:
                log_exporter = AzureMonitorLogExporter(
                    connection_string=Telemetry.telemetry_connection_string
                )
                log_record_processor = BatchLogRecordProcessor(
                    log_exporter,
                )
                logger_provider.add_log_record_processor(log_record_processor)
                set_logger_provider(logger_provider)
                handler.setLevel(Telemetry.log_level)
                logger = getLogger()
                logger.addHandler(handler)

        return logger

    @staticmethod
    def get_tracer(name: str) -> Tracer:
        """
        Creates an OpenTelemetry tracer with the specified name.

        Parameters
        ----------
        name : str
            The name to assign to the tracer.

        Returns
        -------
        Tracer
            Returns an OpenTelemetry tracer for span creation and in-process context propagation.
        """
        return trace.get_tracer(name)

    @staticmethod
    def record_exception(span: Span, ex: Exception):
        """
        Associates an exception with an OpenTelemetry Span and logs it.

        Parameters
        ----------
        span : Span
            The OpenTelemetry span to which the execption should be associated.
        ex : Exception
            The exception that occurred.
        """
        span.set_status(Status(StatusCode.ERROR))
        span.record_exception(ex)
