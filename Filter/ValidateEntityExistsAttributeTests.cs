using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.Web.CodeGeneration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestAPI.Contracts;
using TestAPI.Controllers;
using TestAPI.Filters;
using TestAPI.Models;
using Xunit;

namespace TestAPI.UnitTest.Filter
{
    public class ValidateEntityExistsAttributeTests
    {
        private readonly Mock<IRepositoryWrapper> _mockrepo;
        private readonly Mock<ILoggerManager> _logger;
        private readonly ActionExecutingContext _context;
        private readonly ValidateEntityExistsAttribute<Customer> _filter;
        public ValidateEntityExistsAttributeTests()
        {
            _mockrepo = new Mock<IRepositoryWrapper>();
            _logger = new Mock<ILoggerManager>();
            _filter = new ValidateEntityExistsAttribute<Customer>(_mockrepo.Object, _logger.Object);            
        }

        [Fact]
        public async Task ReturnBadRequest_BadIdArgument()
        {
            //Arrange
            var httpContext = new DefaultHttpContext();
            Dictionary<string, object> actionArguments = new Dictionary<string, object>();
            var context = new ActionExecutingContext(
                getActionContext(),
                new List<IFilterMetadata>(),
                actionArguments,
                getController()
                );

            var excutedContext = new ActionExecutedContext(
                getActionContext(),
                new List<IFilterMetadata>(),
                null
                );
            
            // Act
            await _filter.OnActionExecutionAsync(context,
                next: async ()=>
                {
                    return excutedContext;
                });

            // Assert
            var result = context.Result;
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            badResult.Value.Should().Be("Bad Id parameter");
        }

        
        public async Task ReturnBadRequest_NotFound()
        {
            //Arrange
            var httpContext = new DefaultHttpContext();
            Dictionary<string, object> actionArguments = new Dictionary<string, object>();
            actionArguments.Add("id", 1);
            var context = new ActionExecutingContext(
                getActionContext(),
                new List<IFilterMetadata>(),
                actionArguments,
                getController()
                );

            var excutedContext = new ActionExecutedContext(
                getActionContext(),
                new List<IFilterMetadata>(),
                null
                );
            int id = 1;
            List<Customer> list = new List<Customer>();
            list.Add(new Customer() { Id = 1 });
            var customer = new Mock<ICustomerRepository>();
            customer.Setup(repo=>repo.FindByCondition(c => c.Id == id)).Returns(list.AsQueryable());
            _mockrepo.Setup(repo => repo.GetRepo<Customer>()).Returns(customer.Object);
            //_mockrepo.Setup(repo => repo.Customer).Returns(new Mock<ICustomerRepository>().Object);
            //_mockrepo.Setup(repo => repo.Customer.FindByCondition(c=>c.Id==id)).Returns(list.AsQueryable());
            // Act
            await _filter.OnActionExecutionAsync(context,
                next: async () =>
                {
                    return excutedContext;
                });

            // Assert
            var result = context.Result;
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            badResult.Value.Should().Be("");
        }
        private CustomersController getController()
        {                       
            return new CustomersController(
                new Mock<IMapper>().Object,
                new Mock<IRepositoryWrapper>().Object,
                new Mock<ILoggerManager>().Object);          
        }

        private ActionContext getActionContext()
        {
            return new ActionContext(
                new DefaultHttpContext(),
                new RouteData(),
                new ControllerActionDescriptor()
                {
                    //MethodInfo = t.GetMethod(nameof(TestClass.TestMethod))
                });
        }

    }
}
