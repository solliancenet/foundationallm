from openai.types.beta.threads import (
    FileCitationAnnotation,
    FilePathAnnotation,
    ImageFileContentBlock,
    ImageURLContentBlock,
    Message,
    TextContentBlock
)
from openai.types.beta.threads.runs import (
    CodeInterpreterToolCall,
    RunStep
)
from foundationallm.models.constants import AgentCapabilityCategories
from foundationallm.models.orchestration import (
    OpenAIFilePathMessageContentItem,
    OpenAIImageFileMessageContentItem,
    OpenAITextMessageContentItem,
    AnalysisResult
)

class OpenAIAssistantsHelpers:

    @staticmethod
    def parse_run_step(run_step: RunStep) -> AnalysisResult:
        """
        Parses a run step from the OpenAI Assistants API.

        Parameters
        ----------
        run_step : RunStep
            The run step to parse.

        Returns
        -------
        AnalysisResult
            The analysis result from the run step.
        OR None
            If the run step does not contain a tool call
            to the code interpreter tool.
        """
        step_details = run_step.step_details
        if step_details and step_details.type == "tool_calls":
            tool_call_detail = step_details.tool_calls
            for details in tool_call_detail:
                if isinstance(details, CodeInterpreterToolCall):
                    result = AnalysisResult(
                        tool_name = details.type,
                        agent_capability_category = AgentCapabilityCategories.OPENAI_ASSISTANTS
                    )
                    result.tool_input += details.code_interpreter.input  # Source code
                    for output in details.code_interpreter.outputs:  # Tool execution output
                        if hasattr(output, 'image') and output.image:
                            result.tool_output += "# Generated image file: " + output.image.file_id
                        elif hasattr(output, 'logs') and output.logs:
                            result.tool_output += output.logs
                    return result
        return None

    @staticmethod
    def parse_message(message: Message):
        """
        Parses a message from the OpenAI Assistants API.

        Parameters
        ----------
        message : Message
            The message to parse.

        Returns
        -------
        List[MessageContentItemBase]
            The content items within the message along with any annotations.
        """
        ret_content = []
        # for each content item in the message
        for ci in message.content:
                match ci:
                    case TextContentBlock():
                        text_ci = OpenAITextMessageContentItem(
                            value=ci.text.value,
                            agent_capability_category = AgentCapabilityCategories.OPENAI_ASSISTANTS
                        )
                        for annotation in ci.text.annotations:
                            match annotation:
                                case FilePathAnnotation():
                                    file_an = OpenAIFilePathMessageContentItem(
                                        file_id=annotation.file_path.file_id,
                                        start_index=annotation.start_index,
                                        end_index=annotation.end_index,
                                        text=annotation.text,
                                        agent_capability_category = AgentCapabilityCategories.OPENAI_ASSISTANTS
                                    )
                                    text_ci.annotations.append(file_an)
                                case FileCitationAnnotation():
                                    file_cit = OpenAIFilePathMessageContentItem(
                                        file_id=annotation.file_citation.file_id,
                                        start_index=annotation.start_index,
                                        end_index=annotation.end_index,
                                        text=annotation.text,
                                        agent_capability_category = AgentCapabilityCategories.OPENAI_ASSISTANTS
                                    )
                                    text_ci.annotations.append(file_cit)
                        ret_content.append(text_ci)
                    case ImageFileContentBlock():
                        ci_img = OpenAIImageFileMessageContentItem(
                            file_id=ci.image_file.file_id,
                            agent_capability_category = AgentCapabilityCategories.OPENAI_ASSISTANTS
                        )
                        ret_content.append(ci_img)
                    case ImageURLContentBlock():
                        ci_img_url = OpenAIImageFileMessageContentItem(
                            file_url=ci.image_url.url,
                            agent_capability_category = AgentCapabilityCategories.OPENAI_ASSISTANTS
                        )
                        ret_content.append(ci_img_url)
        return ret_content
