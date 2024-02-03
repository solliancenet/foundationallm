using FoundationaLLM.Common.Extensions;

namespace FoundationaLLM.Common.Tests.Extensions
{
    public class NeutralUrlTestData: TheoryData<string, string>
    {
        public NeutralUrlTestData()
        {
            Add("FLLM:fllm01#blob#core#windows#net", "https://fllm01.blob.core.windows.net");
            Add("FLLM:fllm01#blob#core#windows#net/", "https://fllm01.blob.core.windows.net/");
            Add("FLLM:fllm01#dfs#core#windows#net", "https://fllm01.dfs.core.windows.net");
            Add("FLLM:fllm01#sharepoint#com", "https://fllm01.sharepoint.com");

            Add("fllm01.blob.core.windows.net", "https://fllm01.blob.core.windows.net");
            Add("fllm01.blob.core.windows.net/", "https://fllm01.blob.core.windows.net/");
            Add("fllm01.privatelink.blob.core.windows.net/", "https://fllm01.privatelink.blob.core.windows.net/");
            Add("fllm01.dfs.core.windows.net", "https://fllm01.dfs.core.windows.net");
            Add("fllm01.sharepoint.com", "https://fllm01.sharepoint.com");
            Add("FLLM01.SharePoint.COM", "https://fllm01.sharepoint.com");

            Add("https://fllm01.blob.core.windows.net", "https://fllm01.blob.core.windows.net");
            Add("http://fllm01.blob.core.windows.net", "http://fllm01.blob.core.windows.net");
            Add("https://solliance.net", "https://solliance.net");
            Add("http://solliance.net", "http://solliance.net");

            Add("microsoft.com", "microsoft.com");
            Add("/microsoft.com", "/microsoft.com");
            Add("microsoft.blob.core.windows.com", "microsoft.blob.core.windows.com");
        }
    }


    public class StringExtensionsTests
    {
        [Theory]
        [ClassData(typeof(NeutralUrlTestData))]
        public void FromKnownNeutralUrl_TransformsCorrectly(string inputString, string transformedString)
        {
            Assert.Equal(inputString.FromKnownNeutralUrl(), transformedString);
        }
    }
}
