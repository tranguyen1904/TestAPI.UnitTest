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
    public class PurchaseOrderControllerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockrepo;
        private readonly PurchaseOrdersController _controller;
        private readonly IMapper _mapper;
        private readonly Mock<ILoggerManager> _logger;
        public PurchaseOrderControllerTests()
        {
            var config = new MapperConfiguration(opts =>
            {
                opts.AddProfile<ModelToViewModelProfile>();
                opts.AddProfile<ViewModelToModelProfile>();
            });
            _mapper = config.CreateMapper();
            _logger = new Mock<ILoggerManager>();
            _mockrepo = new Mock<IRepositoryWrapper>();
            _controller = new PurchaseOrdersController(_mapper, _mockrepo.Object, _logger.Object);
        }

        [Fact]
        public async Task GetPurchaseOrders_ActionExecute_ReturnAllPurchaseOrders()
        {
            // Arrange
            _mockrepo.Setup(repo => repo.PurchaseOrder.GetPurchaseOrdersAsync())
                .ReturnsAsync(GetTestPurchaseOrders());
            // Act
            var result = await _controller.GetPurchaseOrders();
            // Assert
            var okresult = Assert.IsType<OkObjectResult>(result);
            var purchaseOrders = Assert.IsAssignableFrom<IEnumerable<PurchaseOrderViewModel>>(okresult.Value);
            purchaseOrders.Should().HaveCount(2);
            //Assert.Equal(2, purchaseOrders.Should().HaveCount(2));
        }

        [Fact]

        public async Task GetPurchaseOrderById_ActionExecute_ReturnPurchaseOrderById()
        {
            // Arrange
            int id = 1;
            _mockrepo.Setup(repo => repo.PurchaseOrder.GetPurchaseOrderById(id))
                .ReturnsAsync(GetTestPurchaseOrders().FirstOrDefault(
                    c => c.Id == id));

            // Act
            var result = await _controller.GetPurchaseOrder(id);
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var purchaseOrder = Assert.IsType<PurchaseOrderViewModel>(okResult.Value);
            Assert.Equal(id, purchaseOrder.Id);
            Assert.Equal(1, purchaseOrder.CustomerId);
            Assert.Equal(1, purchaseOrder.EmployeeId);
        }

        [Fact]
        public async Task PostPurchaseOrder_ReturnBadRequest_IdExist()
        {
            //Arrange
            PurchaseOrderViewModel purchaseOrderViewModel = new PurchaseOrderViewModel()
            {
                Id = 1,
                CustomerId = 1,
                EmployeeId = 1
            };
            _mockrepo.Setup(repo => repo.PurchaseOrder.GetPurchaseOrderById(purchaseOrderViewModel.Id))
                .ReturnsAsync(GetTestPurchaseOrders().FirstOrDefault(
                    c => c.Id == purchaseOrderViewModel.Id));
            //Act
            var result = await _controller.PostPurchaseOrder(purchaseOrderViewModel);
            //Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            badResult.Value.Should().Be(LogMessage.ExistsId(nameof(PurchaseOrder), purchaseOrderViewModel.Id));
        }

        [Fact]
        public async Task PostPurchaseOrder_ReturnBadRequest_IdNull()
        {
            // Arrange
            PurchaseOrderViewModel purchaseOrderViewModel = new PurchaseOrderViewModel();
            _mockrepo.Setup(repo => repo.PurchaseOrder.GetPurchaseOrderById(purchaseOrderViewModel.Id))
                .ReturnsAsync((PurchaseOrder)null);
            //Act
            var result = await _controller.PostPurchaseOrder(purchaseOrderViewModel);
            //Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            badResult.Value.Should().Be(LogMessage.InvalidId(nameof(PurchaseOrder)));
        }

        [Fact]
        public async Task PostPurchaseOrder_ReturnCreatedResponse()
        {
            //Arrange
            PurchaseOrderViewModel purchaseOrderVM = new PurchaseOrderViewModel()
            {
                Id = 1,
                CustomerId = 1,
                EmployeeId = 1
            };
            _mockrepo.Setup(repo => repo.PurchaseOrder.CreatePurchaseOrder(_mapper.Map<PurchaseOrder>(purchaseOrderVM)));

            //Act
            var result = await _controller.PostPurchaseOrder(purchaseOrderVM);
            //Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<PurchaseOrderViewModel>(actionResult.Value);
            _mockrepo.Verify();
            Assert.Equal(purchaseOrderVM, returnValue);
        }

        [Fact]
        public async Task PutPurchaseOrder_ReturnBadRequest_IdNotMatch()
        {
            //Arrange
            int id = 1;
            PurchaseOrderViewModel purchaseOrderVM = new PurchaseOrderViewModel()
            {
                Id = 2,
                CustomerId = 2,
                EmployeeId = 2
            };
            _mockrepo.Setup(repo => repo.PurchaseOrder.UpdatePurchaseOrder(_mapper.Map<PurchaseOrder>(purchaseOrderVM)));
            // Act
            var result = await _controller.PutPurchaseOrder(id, purchaseOrderVM);
            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            badResult.Value.Should().Be(LogMessage.IdNotMatch());
        }

        [Fact]
        public async Task PutPurchaseOrder_ReturnNoContent()
        {
            //Arrange
            int id = 1;
            PurchaseOrderViewModel purchaseOrderVM = new PurchaseOrderViewModel()
            {
                Id = 1,
                CustomerId = 1,
                EmployeeId = 1
            };
            _mockrepo.Setup(repo => repo.PurchaseOrder.UpdatePurchaseOrder(_mapper.Map<PurchaseOrder>(purchaseOrderVM)));
            // Act
            var result = await _controller.PutPurchaseOrder(id, purchaseOrderVM);
            // Assert
            var badResult = Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeletePurchaseOrder_ReturnBadRequest_ExistRelatedEntity()
        {
            //Arrange
            int id = 1;
            List<OrderDetail> list = new List<OrderDetail>();
            list.Add(new OrderDetail());
            _mockrepo.Setup(repo => repo.OrderDetail.OrderDetailsByPurchaseOrder(id)).ReturnsAsync(list);
            _mockrepo.Setup(repo => repo.PurchaseOrder.DeletePurchaseOrder((PurchaseOrder)null));
            //Act
            var result = await _controller.DeletePurchaseOrder(id);
            //Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be(LogMessage.DeleteError(nameof(PurchaseOrder), id, nameof(OrderDetail)));
        }

        [Fact]
        public async Task DeletePurchaseOrder_ReturnNoContent()
        {
            //Arrange
            int id = 1;
            List<OrderDetail> list = new List<OrderDetail>();
            _mockrepo.Setup(repo => repo.OrderDetail.OrderDetailsByPurchaseOrder(id)).ReturnsAsync(list);
            _mockrepo.Setup(repo => repo.PurchaseOrder.DeletePurchaseOrder((PurchaseOrder)null));
            //Act
            var result = await _controller.DeletePurchaseOrder(id);
            //Assert
            var badRequest = Assert.IsType<NoContentResult>(result);
        }

        private List<PurchaseOrder> GetTestPurchaseOrders()
        {
            var purchaseOrders = new List<PurchaseOrder>();
            purchaseOrders.Add(new PurchaseOrder()
            {
                Id = 1,
                CustomerId = 1,
                EmployeeId = 1
            }) ;
            purchaseOrders.Add(new PurchaseOrder()
            {
                Id = 2,
                CustomerId = 2,
                EmployeeId = 2
            });
            return purchaseOrders;
        }
    }
}
