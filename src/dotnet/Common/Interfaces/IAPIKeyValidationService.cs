namespace FoundationaLLM.Common.Interfaces
{
    public interface IAPIKeyValidationService
    {
        bool IsValid(string apiKey);
    }
}
