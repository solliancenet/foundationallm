using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace FoundationaLLM.Common.Tests.Authentication
{
    public class APIKeyAuthenticationFilterTests
    {
        [Fact]
        public void APIKeyAuthenticationFilter_OnAuthorization_ReturnsMissingAPIKey()
        {
            // Arrange
            var apiKeyValidation = Substitute.For<IAPIKeyValidationService>();
            var filters = new List<IFilterMetadata>();
            var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
            var context = new AuthorizationFilterContext(actionContext, filters);
            var filter = new APIKeyAuthenticationFilter(apiKeyValidation);

            // Act
            filter.OnAuthorization(context);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(context.Result);
            Assert.Equal("The X-API-KEY header is missing.", (context.Result as UnauthorizedObjectResult)?.Value);
        }

        [Fact]
        public void APIKeyAuthenticationFilter_OnAuthorization_ReturnsInvalidAPIKey()
        {
            // Arrange
            var apiKeyValidation = Substitute.For<IAPIKeyValidationService>();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HttpHeaders.APIKey] = "TEST-API-KEY";
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var context = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
            var filter = new APIKeyAuthenticationFilter(apiKeyValidation);

            // Act
            filter.OnAuthorization(context);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(context.Result);
            Assert.Equal("The provided X-API-KEY is invalid.", (context.Result as UnauthorizedObjectResult)?.Value);
        }
    }
}
