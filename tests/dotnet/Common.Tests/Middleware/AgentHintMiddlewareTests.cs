using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Middleware;
using FoundationaLLM.Common.Models.Authentication;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FoundationaLLM.Common.Models.Metadata;
using FoundationaLLM.Common.Models.Configuration.Instance;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Common.Tests.Middleware
{
    public class CallContextMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_WithAuthenticatedUser_ShouldSetCurrentUserIdentity()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var claimsProviderService = Substitute.For<IUserClaimsProviderService>();
            var callContext = Substitute.For<ICallContext>();
            var instanceSettings = Options.Create<InstanceSettings>(Substitute.For<InstanceSettings>());
            var middleware = new CallContextMiddleware(next: _ => Task.FromResult(0));
            context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.Email, "testuser@example.com"),
                new Claim(ClaimTypes.Role, "admin")
            }, "mock"));

            // Act
            await middleware.InvokeAsync(context, claimsProviderService, callContext, instanceSettings);

            // Assert
            claimsProviderService.Received(1).GetUserIdentity(context.User);
            callContext.Received(1).CurrentUserIdentity = Arg.Any<UnifiedUserIdentity>();
        }

        [Fact]
        public async Task InvokeAsync_WithUnauthenticatedUser_ShouldSetCurrentUserIdentityFromHeader()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var claimsProviderService = Substitute.For<IUserClaimsProviderService>();
            var callContext = Substitute.For<ICallContext>();
            var instanceSettings = Options.Create<InstanceSettings>(Substitute.For<InstanceSettings>());
            var middleware = new CallContextMiddleware(next: _ => Task.FromResult(0));
            var userIdentity = new UnifiedUserIdentity { Username = "testuser@example.com", UPN = "testuser@example.com", Name = "testuser" };
            context.Request.Headers[Constants.HttpHeaders.UserIdentity] = JsonConvert.SerializeObject(userIdentity);

            // Act
            await middleware.InvokeAsync(context, claimsProviderService, callContext, instanceSettings);

            // Assert
            callContext.Received(1).CurrentUserIdentity = Arg.Is<UnifiedUserIdentity>(x => x.Username == userIdentity.Username && x.UPN == userIdentity.UPN && x.Name == userIdentity.Name);
        }

        [Fact]
        public async Task InvokeAsync_WithAgentHint_ShouldSetAgentHint()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var claimsProviderService = Substitute.For<IUserClaimsProviderService>();
            var callContext = Substitute.For<ICallContext>();
            var instanceSettings = Options.Create<InstanceSettings>(Substitute.For<InstanceSettings>());
            var middleware = new CallContextMiddleware(next: _ => Task.FromResult(0));
            var agentHint = new Agent
            {
                Name = "test-agent-hint",
                Private = false
            };
            context.Request.Headers[Constants.HttpHeaders.AgentHint] = JsonConvert.SerializeObject(agentHint);

            // Act
            await middleware.InvokeAsync(context, claimsProviderService, callContext, instanceSettings);

            // Assert
            callContext.Received(1).AgentHint = Arg.Is<Agent>(x => x.Name == agentHint.Name && x.Private == agentHint.Private);
        }
    }
}
