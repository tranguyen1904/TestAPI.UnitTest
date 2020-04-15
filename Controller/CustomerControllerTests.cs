using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TestAPI.Contracts;
using TestAPI.Controllers;
using TestAPI.Models;
using TestAPI.ViewModels;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TestAPI.Mapping;
using System.Linq;

namespace TestAPI.UnitTest.Controller
{
    public class CustomerControllerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockrepo;
        private readonly CustomersController _controller;
        private readonly IMapper _mapper;
        private readonly Mock<ILoggerManager> _logger;
        public CustomerControllerTests()
        {
            var config = new MapperConfiguration(opts=>
            {
                opts.AddProfile<ModelToViewModelProfile>();
                opts.AddProfile<ViewModelToModelProfile>();
            });
            _mapper = config.CreateMapper();
            _logger = new Mock<ILoggerManager>();
            _mockrepo = new Mock<IRepositoryWrapper>();
            _controller = new CustomersController(_mapper, _mockrepo.Object, _logger.Object);
        }

        [Fact]
        public async Task GetCustomers_ActionExecute_ReturnAllCustomers()
        {
            // Arrange
            _mockrepo.Setup(repo => repo.Customer.GetCustomersAsync())
                .ReturnsAsync(GetTestCustomers());
            // Act
            var result = await _controller.GetCustomers();
            // Assert
            var okresult = Assert.IsType<OkObjectResult>(result);
            var customers = Assert.IsAssignableFrom<IEnumerable<CustomerViewModel>>(okresult.Value);
            customers.Should().HaveCount(2);
            //Assert.Equal(2, customers.Should().HaveCount(2));
        }

        [Fact]

        public async Task GetCustomerById_ActionExecute_ReturnCustomerById()
        {
            // Arrange
            int id = 1;
            _mockrepo.Setup(repo => repo.Customer.GetCustomerById(id))
                .ReturnsAsync(GetTestCustomers().FirstOrDefault(
                    c => c.Id == id));

            // Act
            var result = await _controller.GetCustomer(id);
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var customer = Assert.IsType<CustomerViewModel>(okResult.Value);
            Assert.Equal(id, customer.Id);
            Assert.Equal("Trang Uyen", customer.Name);
        }

        [Fact]
        public async Task PostCustomer_ReturnBadRequest_IdExist()
        {
            //Arrange
            CustomerViewModel customerViewModel = new CustomerViewModel()
            {
                Id = 1,
                Name = "Trang Uyen"
            };
            _mockrepo.Setup(repo => repo.Customer.GetCustomerById(customerViewModel.Id))
                .ReturnsAsync(GetTestCustomers().FirstOrDefault(
                    c => c.Id == customerViewModel.Id));
            //Act
            var result = await _controller.PostCustomer(customerViewModel);
            //Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            badResult.Value.Should().Be(LogMessage.ExistsId(nameof(Customer), customerViewModel.Id));
        }

        [Fact]
        public async Task PostCustomer_ReturnBadRequest_IdNull()
        {
            // Arrange
            CustomerViewModel customerViewModel = new CustomerViewModel();
            _mockrepo.Setup(repo => repo.Customer.GetCustomerById(customerViewModel.Id))
                .ReturnsAsync((Customer)null);
            //Act
            var result = await _controller.PostCustomer(customerViewModel);
            //Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            badResult.Value.Should().Be(LogMessage.InvalidId(nameof(Customer)));
        }

        [Fact]
        public async Task PostCustomer_ReturnCreatedResponse()
        {
            //Arrange
            CustomerViewModel customerVM = new CustomerViewModel()
            {
                Id = 1,
                Name = "Trang Uyen"
            };
            _mockrepo.Setup(repo => repo.Customer.CreateCustomer(_mapper.Map<Customer>(customerVM)));

            //Act
            var result = await _controller.PostCustomer(customerVM);
            //Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<CustomerViewModel>(actionResult.Value);
            _mockrepo.Verify();
            Assert.Equal(customerVM, returnValue);
        }

        [Fact]
        public async Task PutCustomer_ReturnBadRequest_IdNotMatch()
        {
            //Arrange
            int id = 1;
            CustomerViewModel customerVM = new CustomerViewModel()
            {
                Id = 2,
                Name = "Trang Uyen"
            };
            // Act
            var result = await _controller.PutCustomer(id, customerVM);
            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            badResult.Value.Should().Be(LogMessage.IdNotMatch());
        }

        [Fact]
        public async Task PutCustomer_ReturnNoContent()
        {
            //Arrange
            int id = 1;
            CustomerViewModel customerVM = new CustomerViewModel()
            {
                Id = 1,
                Name = "Trang Uyen"
            };
            _mockrepo.Setup(repo => repo.Customer.UpdateCustomer(_mapper.Map<Customer>(customerVM)));
            // Act
            var result = await _controller.PutCustomer(id, customerVM);
            // Assert
            var badResult = Assert.IsType<NoContentResult>(result);            
        }

        [Fact]
        public async Task DeleteCustomer_ReturnBadRequest_ExistRelatedEntity()
        {
            //Arrange
            int id = 1;
            List<PurchaseOrder> list = new List<PurchaseOrder>();
            list.Add(new PurchaseOrder());
            _mockrepo.Setup(repo => repo.PurchaseOrder.PurchaseOrdersByCustomer(id)).ReturnsAsync(list);
            _mockrepo.Setup(repo => repo.Customer.DeleteCustomer((Customer)null));
            //Act
            var result = await _controller.DeleteCustomer(id);
            //Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be(LogMessage.DeleteError(nameof(Customer), id, nameof(PurchaseOrder)));
        }

        [Fact]
        public async Task DeleteCustomer_ReturnNoContent()
        {
            //Arrange
            int id = 1;
            List<PurchaseOrder> list = new List<PurchaseOrder>();
            _mockrepo.Setup(repo => repo.PurchaseOrder.PurchaseOrdersByCustomer(id)).ReturnsAsync(list);
            _mockrepo.Setup(repo => repo.Customer.DeleteCustomer((Customer)null));
            //Act
            var result = await _controller.DeleteCustomer(id);
            //Assert
            var badRequest = Assert.IsType<NoContentResult>(result);
        }

        private List<Customer> GetTestCustomers()
        {
            var customers = new List<Customer>();
            customers.Add(new Customer()
            {
                Id = 1,
                Name = "Trang Uyen",
                PhoneNumber = 123456789,
                Gender = "M"
            });
            customers.Add(new Customer()
            {
                Id = 2,
                Name = "Trang Uyen",
                PhoneNumber = 123456789,
                Gender = "F"
            });
            return customers;
        }
    }
}
