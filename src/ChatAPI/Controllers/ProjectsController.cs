using Asp.Versioning;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models.Search;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.ChatAPI.Controllers
{
    [ApiVersion(1.0)]
    [ApiController]
    [Route("[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(IChatService chatService,
            ILogger<ProjectsController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        [HttpPut(Name = "AddProduct")]
        public async Task AddProduct(Product product)
        {
            await _chatService.AddProduct(product);
        }

        [HttpDelete("{productId}", Name = "DeleteProduct")]
        public async Task DeleteProduct(string productId, string categoryId)
        {
            await _chatService.DeleteProduct(productId, categoryId);
        }
    }
}
