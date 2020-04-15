using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TestAPI.Contracts;
using TestAPI;
using Xunit;

namespace TestAPI.UnitTest.Middleware
{
    public class ExceptionMiddlewareTests
    {
        private readonly Mock<ILoggerManager> _logger;
        private readonly ExceptionMiddleware _middleware;
        public ExceptionMiddlewareTests()
        {
            _logger = new Mock<ILoggerManager>();
            _middleware = new ExceptionMiddleware(
                next: (innerHttpContext) => 
                { 
                    return Task.CompletedTask; 
                }, 
                _logger.Object
            );
        }
        
    }
}
