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
    public class EmployeeControllerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockrepo;
        private readonly EmployeesController _controller;
        private readonly IMapper _mapper;
        private readonly Mock<ILoggerManager> _logger;
        public EmployeeControllerTests()
        {
            var config = new MapperConfiguration(opts =>
            {
                opts.AddProfile<ModelToViewModelProfile>();
                opts.AddProfile<ViewModelToModelProfile>();
            });
            _mapper = config.CreateMapper();
            _logger = new Mock<ILoggerManager>();
            _mockrepo = new Mock<IRepositoryWrapper>();
            _controller = new EmployeesController(_mapper, _mockrepo.Object, _logger.Object);
        }

        [Fact]
        public async Task GetEmployees_ActionExecute_ReturnAllEmployees()
        {
            // Arrange
            _mockrepo.Setup(repo => repo.Employee.GetEmployeesAsync())
                .ReturnsAsync(GetTestEmployees());
            // Act
            var result = await _controller.GetEmployees();
            // Assert
            var okresult = Assert.IsType<OkObjectResult>(result);
            var employees = Assert.IsAssignableFrom<IEnumerable<EmployeeViewModel>>(okresult.Value);
            employees.Should().HaveCount(2);
            //Assert.Equal(2, employees.Should().HaveCount(2));
        }

        [Fact]

        public async Task GetEmployeeById_ActionExecute_ReturnEmployeeById()
        {
            // Arrange
            int id = 1;
            _mockrepo.Setup(repo => repo.Employee.GetEmployeeById(id))
                .ReturnsAsync(GetTestEmployees().FirstOrDefault(
                    c => c.Id == id));

            // Act
            var result = await _controller.GetEmployee(id);
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var employee = Assert.IsType<EmployeeViewModel>(okResult.Value);
            Assert.Equal(id, employee.Id);
            Assert.Equal("Employee1", employee.Name);
        }

        [Fact]
        public async Task PostEmployee_ReturnBadRequest_IdExist()
        {
            //Arrange
            EmployeeViewModel employeeViewModel = new EmployeeViewModel()
            {
                Id = 1,
                Name = "Employee1"
            };
            _mockrepo.Setup(repo => repo.Employee.GetEmployeeById(employeeViewModel.Id))
                .ReturnsAsync(GetTestEmployees().FirstOrDefault(
                    c => c.Id == employeeViewModel.Id));
            //Act
            var result = await _controller.PostEmployee(employeeViewModel);
            //Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            badResult.Value.Should().Be(LogMessage.ExistsId(nameof(Employee), employeeViewModel.Id));
        }

        [Fact]
        public async Task PostEmployee_ReturnBadRequest_IdNull()
        {
            // Arrange
            EmployeeViewModel employeeViewModel = new EmployeeViewModel();
            _mockrepo.Setup(repo => repo.Employee.GetEmployeeById(employeeViewModel.Id))
                .ReturnsAsync((Employee)null);
            //Act
            var result = await _controller.PostEmployee(employeeViewModel);
            //Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            badResult.Value.Should().Be(LogMessage.InvalidId(nameof(Employee)));
        }

        [Fact]
        public async Task PostEmployee_ReturnCreatedResponse()
        {
            //Arrange
            EmployeeViewModel employeeVM = new EmployeeViewModel()
            {
                Id = 1,
                Name = "Employee1"
            };
            _mockrepo.Setup(repo => repo.Employee.CreateEmployee(_mapper.Map<Employee>(employeeVM)));

            //Act
            var result = await _controller.PostEmployee(employeeVM);
            //Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<EmployeeViewModel>(actionResult.Value);
            _mockrepo.Verify();
            Assert.Equal(employeeVM, returnValue);
        }

        [Fact]
        public async Task PutEmployee_ReturnBadRequest_IdNotMatch()
        {
            //Arrange
            int id = 1;
            EmployeeViewModel employeeVM = new EmployeeViewModel()
            {
                Id = 2,
                Name = "Employee2"
            };
            // Act
            var result = await _controller.PutEmployee(id, employeeVM);
            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            badResult.Value.Should().Be(LogMessage.IdNotMatch());
        }

        [Fact]
        public async Task PutEmployee_ReturnNoContent()
        {
            //Arrange
            int id = 1;
            EmployeeViewModel employeeVM = new EmployeeViewModel()
            {
                Id = 1,
                Name = "Employee1"
            };
            _mockrepo.Setup(repo => repo.Employee.UpdateEmployee(_mapper.Map<Employee>(employeeVM)));
            // Act
            var result = await _controller.PutEmployee(id, employeeVM);
            // Assert
            var badResult = Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteEmployee_ReturnBadRequest_ExistRelatedEntity()
        {
            //Arrange
            int id = 1;
            List<PurchaseOrder> list = new List<PurchaseOrder>();
            list.Add(new PurchaseOrder());
            _mockrepo.Setup(repo => repo.PurchaseOrder.PurchaseOrdersByEmployee(id)).ReturnsAsync(list);
            _mockrepo.Setup(repo => repo.Employee.DeleteEmployee((Employee)null));
            //Act
            var result = await _controller.DeleteEmployee(id);
            //Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be(LogMessage.DeleteError(nameof(Employee), id, nameof(PurchaseOrder)));
        }

        [Fact]
        public async Task DeleteEmployee_ReturnNoContent()
        {
            //Arrange
            int id = 1;
            List<PurchaseOrder> list = new List<PurchaseOrder>();
            _mockrepo.Setup(repo => repo.PurchaseOrder.PurchaseOrdersByEmployee(id)).ReturnsAsync(list);
            _mockrepo.Setup(repo => repo.Employee.DeleteEmployee((Employee)null));
            //Act
            var result = await _controller.DeleteEmployee(id);
            //Assert
            var badRequest = Assert.IsType<NoContentResult>(result);
        }

        private List<Employee> GetTestEmployees()
        {
            var employees = new List<Employee>();
            employees.Add(new Employee()
            {
                Id = 1,
                Name = "Employee1",
                
            });
            employees.Add(new Employee()
            {
                Id = 2,
                Name = "Employee2",
                
            });
            return employees;
        }
    }
}
