using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Core.Models.Orchestration
{
    internal class LangChainLanguageModel
    {
        public string type;
        public string subtype;
        public string provider;
        public float temperature;
    }

    internal class LangChainAgent
    {
        public string name;
        public string type;
        public string description;
        public string prompt_template;
        public LangChainLanguageModel language_model;
    }

    internal class LangChainSQLDataSourceConfiguration
    {
        public string dialect;
        public string host;
        public int port;
        public string database_name;
        public string username;
        public string password_secret_name;
        public string[] include_tables;
        public int few_shot_example_count;
    }

    internal class LangChainDataSource
    {
        public string name;
        public string type;
        public string description;
        public LangChainSQLDataSourceConfiguration configuration;
    }

    internal class LangChainCompletionRequest
    {
        public string user_prompt;
        public LangChainAgent agent;
        public LangChainDataSource data_source;
    }
}
