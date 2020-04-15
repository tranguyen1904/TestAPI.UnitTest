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
    public class OrderDetailControllerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockrepo;
        private readonly OrderDetailsController _controller;
        private readonly IMapper _mapper;
        private readonly Mock<ILoggerManager> _logger;
        public OrderDetailControllerTests()
        {
            var config = new MapperConfiguration(opts =>
            {
                opts.AddProfile<ModelToViewModelProfile>();
                opts.AddProfile<ViewModelToModelProfile>();
            });
            _mapper = config.CreateMapper();
            _logger = new Mock<ILoggerManager>();
            _mockrepo = new Mock<IRepositoryWrapper>();
            _controller = new OrderDetailsController(_mapper, _mockrepo.Object, _logger.Object);
        }

        [Fact]
        public async Task GetOrderDetails_ActionExecute_ReturnAllOrderDetails()
        {
            // Arrange
            _mockrepo.Setup(repo => repo.OrderDetail.GetOrderDetailsAsync())
                .ReturnsAsync(GetTestOrderDetails());
            // Act
            var result = await _controller.GetOrderDetails();
            // Assert
            var okresult = Assert.IsType<OkObjectResult>(result);
            var orderDetails = Assert.IsAssignableFrom<IEnumerable<OrderDetailViewModel>>(okresult.Value);
            orderDetails.Should().HaveCount(2);
            //Assert.Equal(2, orderDetails.Should().HaveCount(2));
        }

        [Fact]

        public async Task GetOrderDetailById_ActionExecute_ReturnOrderDetailById()
        {
            // Arrange
            int id = 1;
            _mockrepo.Setup(repo => repo.OrderDetail.GetOrderDetailById(id))
                .ReturnsAsync(GetTestOrderDetails().FirstOrDefault(
                    c => c.Id == id));

            // Act
            var result = await _controller.GetOrderDetail(id);
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var orderDetail = Assert.IsType<OrderDetailViewModel>(okResult.Value);
            Assert.Equal(id, orderDetail.Id);
            Assert.Equal(1, orderDetail.OrderId);
            Assert.Equal(1, orderDetail.ProductId);
        }

        [Fact]
        public async Task PostOrderDetail_ReturnBadRequest_IdExist()
        {
            //Arrange
            OrderDetailViewModel orderDetailViewModel = new OrderDetailViewModel()
            {
                Id = 1,
                OrderId = 1,
                ProductId = 1
            };
            _mockrepo.Setup(repo => repo.OrderDetail.GetOrderDetailById(orderDetailViewModel.Id))
                .ReturnsAsync(GetTestOrderDetails().FirstOrDefault(
                    c => c.Id == orderDetailViewModel.Id));
            //Act
            var result = await _controller.PostOrderDetail(orderDetailViewModel);
            //Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            badResult.Value.Should().Be(LogMessage.ExistsId(nameof(OrderDetail), orderDetailViewModel.Id));
        }

        [Fact]
        public async Task PostOrderDetail_ReturnBadRequest_IdNull()
        {
            // Arrange
            OrderDetailViewModel orderDetailViewModel = new OrderDetailViewModel();
            _mockrepo.Setup(repo => repo.OrderDetail.GetOrderDetailById(orderDetailViewModel.Id))
                .ReturnsAsync((OrderDetail)null);
            //Act
            var result = await _controller.PostOrderDetail(orderDetailViewModel);
            //Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            badResult.Value.Should().Be(LogMessage.InvalidId(nameof(OrderDetail)));
        }

        [Fact]
        public async Task PostOrderDetail_ReturnCreatedResponse()
        {
            //Arrange
            OrderDetailViewModel orderDetailVM = new OrderDetailViewModel()
            {
                Id = 1,
                OrderId = 1,
                ProductId = 1
            };
            _mockrepo.Setup(repo => repo.OrderDetail.CreateOrderDetail(_mapper.Map<OrderDetail>(orderDetailVM)));

            //Act
            var result = await _controller.PostOrderDetail(orderDetailVM);
            //Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<OrderDetailViewModel>(actionResult.Value);
            _mockrepo.Verify();
            Assert.Equal(orderDetailVM, returnValue);
        }

        [Fact]
        public async Task PutOrderDetail_ReturnBadRequest_IdNotMatch()
        {
            //Arrange
            int id = 1;
            OrderDetailViewModel orderDetailVM = new OrderDetailViewModel()
            {
                Id = 2,
                OrderId = 2,
                ProductId = 2
            };
            // Act
            var result = await _controller.PutOrderDetail(id, orderDetailVM);
            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            badResult.Value.Should().Be(LogMessage.IdNotMatch());
        }

        [Fact]
        public async Task PutOrderDetail_ReturnNoContent()
        {
            //Arrange
            int id = 1;
            OrderDetailViewModel orderDetailVM = new OrderDetailViewModel()
            {
                Id = 1,
                OrderId = 1,
                ProductId = 1
            };
            _mockrepo.Setup(repo => repo.OrderDetail.UpdateOrderDetail(_mapper.Map<OrderDetail>(orderDetailVM)));
            // Act
            var result = await _controller.PutOrderDetail(id, orderDetailVM);
            // Assert
            var badResult = Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteOrderDetail_ReturnNoContent()
        {
            //Arrange
            int id = 1;
            _mockrepo.Setup(repo => repo.OrderDetail.DeleteOrderDetail((OrderDetail)null));
            //Act
            var result = await _controller.DeleteOrderDetail(id);
            //Assert
            var badRequest = Assert.IsType<NoContentResult>(result);
        }

        private List<OrderDetail> GetTestOrderDetails()
        {
            var orderDetails = new List<OrderDetail>();
            orderDetails.Add(new OrderDetail()
            {
                Id = 1,
                OrderId=1,
                ProductId=1

            });
            orderDetails.Add(new OrderDetail()
            {
                Id = 2,
                OrderId=2,
                ProductId=2

            });
            return orderDetails;
        }
    }
}
