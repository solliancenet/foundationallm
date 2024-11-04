import json
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
    FunctionToolCall,
    RunStep
)
from foundationallm.models.constants import AgentCapabilityCategories
from foundationallm.models.orchestration import (
    OpenAIFilePathMessageContentItem,
    OpenAIImageFileMessageContentItem,
    OpenAITextMessageContentItem,
    AnalysisResult,
    FunctionResult
)

class OpenAIAssistantsHelpers:

    @staticmethod
    def parse_run_step(run_step: RunStep) -> tuple:
        """
        Parses a run step from the OpenAI Assistants API.

        Parameters
        ----------
        run_step : RunStep
            The run step to parse.

        Returns
        -------
        tuple
            Returns a tuple containing lists of analysis results and function call results.
        """
        analysis_results = []
        function_results = []
        step_details = run_step.step_details
        if step_details and step_details.type == "tool_calls":
            for call in step_details.tool_calls:
                if isinstance(call, CodeInterpreterToolCall):
                    analysis_result = AnalysisResult(
                        agent_capability_category = AgentCapabilityCategories.OPENAI_ASSISTANTS,
                        tool_name = call.type,
                        tool_input = call.code_interpreter.input  # Source code
                    )
                    for output in call.code_interpreter.outputs:  # Tool execution output
                        if hasattr(output, 'image') and output.image:
                            analysis_result.tool_output += "# Generated image file: " + output.image.file_id
                        elif hasattr(output, 'logs') and output.logs:
                            analysis_result.tool_output += output.logs
                    analysis_results.append(analysis_result)
                elif isinstance(call, FunctionToolCall):
                    function_result = FunctionResult(
                        agent_capability_category = AgentCapabilityCategories.OPENAI_ASSISTANTS,
                        function_name = call.function.name,
                        function_input = json.loads(call.function.arguments)
                    )
                    if call.function.output:
                        fn_output = json.loads(call.function.output)
                        # TODO: On the function tool, provide a property to specify the response content (e.g., "data"),
                        # so the if statement below can be more dynamic and based on the expected response from the function
                        if 'data' in fn_output:
                            output_data = json.loads(call.function.output)['data'][0]
                            function_result.function_output = output_data #json.dumps({"url": output_data['url'], "description": output_data['revised_prompt']})
                        else:
                            # Indicative of a failure during the function call, append error message to output
                            function_result.function_output = fn_output #json.dumps(fn_output)
                    function_results.append(function_result)

        return analysis_results, function_results

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
