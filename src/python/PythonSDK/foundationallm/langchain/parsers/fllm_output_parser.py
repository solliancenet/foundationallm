import re
from typing import Union
from langchain.schema import AgentAction, AgentFinish, OutputParserException
from langchain.agents.agent import AgentOutputParser
from langchain.agents.mrkl.prompt import FORMAT_INSTRUCTIONS

FINAL_ANSWER_ACTION = "Final Answer:"

QUESTION_ACTION = "Question:"

ACTION_ACTION = "Action:"

MISSING_ACTION_AFTER_THOUGHT_ERROR_MESSAGE = (
    "Invalid Format: Missing 'Action:' after 'Thought:"
)
MISSING_ACTION_INPUT_AFTER_ACTION_ERROR_MESSAGE = (
    "Invalid Format: Missing 'Action Input:' after 'Action:'"
)
FINAL_ANSWER_AND_PARSABLE_ACTION_ERROR_MESSAGE = (
    "Parsing LLM output produced both a final answer and a parse-able action:"
)


class FLLMOutputParser(AgentOutputParser):
    def get_format_instructions(self) -> str:
        return FORMAT_INSTRUCTIONS

    def parse(self, text: str) -> Union[AgentAction, AgentFinish]:
        regex = (
            r"Action\s*\d*\s*:[\s]*(.*?)[\s]*Action\s*\d*\s*Input\s*\d*\s*:[\s]*(.*)"
        )

        action_match = re.search(regex, text, re.DOTALL)

        # remove any AI generated questions and go with the last result...
        if text.find(QUESTION_ACTION) != -1 and (
            text.find(QUESTION_ACTION) < text.find(action_match.group(0))
        ):
            start_index = text.find(QUESTION_ACTION) + len(QUESTION_ACTION)
            end_index = text.find("\n", start_index)
            text = "Final Answer: " + text.replace(
                f"{QUESTION_ACTION} {text[0:start_index].strip()}", ""
            )

        includes_answer = FINAL_ANSWER_ACTION in text

        if action_match and includes_answer:
            if text.find(FINAL_ANSWER_ACTION) < text.find(action_match.group(0)):
                # if final answer is before the hallucination, return final answer
                start_index = text.find(FINAL_ANSWER_ACTION) + len(FINAL_ANSWER_ACTION)
                end_index = text.find("\n\n", start_index)
                return AgentFinish(
                    {"output": text[start_index:end_index].strip()}, text[:end_index]
                )
            else:
                raise OutputParserException(
                    f"{FINAL_ANSWER_AND_PARSABLE_ACTION_ERROR_MESSAGE}: {text}"
                )

        if action_match:
            action = action_match.group(1).strip()
            action_input = action_match.group(2)
            tool_input = action_input.strip(" ")

            if action == "sql_db_schema":
                # Remove leading/trailing single quotes from SQL queries
                if tool_input.startswith("'") and tool_input.endswith("'"):
                    tool_input = tool_input[1:-1]

            # Ensure that if a well-formed SQL query is provided, we don't remove any trailing double quotes
            if tool_input.startswith("SELECT ") is False:
                tool_input = tool_input.strip('"')

            return AgentAction(action, tool_input, text)

        elif includes_answer:
            return AgentFinish(
                {"output": text.split(FINAL_ANSWER_ACTION)[-1].strip()}, text
            )

        if not re.search(r"Action\s*\d*\s*:[\s]*(.*?)", text, re.DOTALL):
            return AgentFinish({"output": text}, text)
        elif not re.search(
            r"[\s]*Action\s*\d*\s*Input\s*\d*\s*:[\s]*(.*)", text, re.DOTALL
        ):
            raise OutputParserException(
                f"Could not parse LLM output: `{text}`",
                observation=MISSING_ACTION_INPUT_AFTER_ACTION_ERROR_MESSAGE,
                llm_output=text,
                send_to_llm=True,
            )
        else:
            raise OutputParserException(f"Could not parse LLM output: `{text}`")

    @property
    def _type(self) -> str:
        return "fllm"
