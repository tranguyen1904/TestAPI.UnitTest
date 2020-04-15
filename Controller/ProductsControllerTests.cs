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
    public class ProductControllerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockrepo;
        private readonly ProductsController _controller;
        private readonly IMapper _mapper;
        private readonly Mock<ILoggerManager> _logger;
        public ProductControllerTests()
        {
            var config = new MapperConfiguration(opts =>
            {
                opts.AddProfile<ModelToViewModelProfile>();
                opts.AddProfile<ViewModelToModelProfile>();
            });
            _mapper = config.CreateMapper();
            _logger = new Mock<ILoggerManager>();
            _mockrepo = new Mock<IRepositoryWrapper>();
            _controller = new ProductsController(_mapper, _mockrepo.Object, _logger.Object);
        }

        [Fact]
        public async Task GetProducts_ActionExecute_ReturnAllProducts()
        {
            // Arrange
            _mockrepo.Setup(repo => repo.Product.GetProductsAsync())
                .ReturnsAsync(GetTestProducts());
            // Act
            var result = await _controller.GetProducts();
            // Assert
            var okresult = Assert.IsType<OkObjectResult>(result);
            var products = Assert.IsAssignableFrom<IEnumerable<ProductViewModel>>(okresult.Value);
            products.Should().HaveCount(2);
            //Assert.Equal(2, products.Should().HaveCount(2));
        }

        [Fact]

        public async Task GetProductById_ActionExecute_ReturnProductById()
        {
            // Arrange
            int id = 1;
            _mockrepo.Setup(repo => repo.Product.GetProductById(id))
                .ReturnsAsync(GetTestProducts().FirstOrDefault(
                    c => c.Id == id));

            // Act
            var result = await _controller.GetProduct(id);
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var product = Assert.IsType<ProductViewModel>(okResult.Value);
            Assert.Equal(id, product.Id);
            Assert.Equal("Product1", product.Name);
        }

        [Fact]
        public async Task PostProduct_ReturnBadRequest_IdExist()
        {
            //Arrange
            ProductViewModel productViewModel = new ProductViewModel()
            {
                Id = 1,
                Name = "Prodcut1"
            };
            _mockrepo.Setup(repo => repo.Product.GetProductById(productViewModel.Id))
                .ReturnsAsync(GetTestProducts().FirstOrDefault(
                    c => c.Id == productViewModel.Id));
            //Act
            var result = await _controller.PostProduct(productViewModel);
            //Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            badResult.Value.Should().Be(LogMessage.ExistsId(nameof(Product), productViewModel.Id));
        }

        [Fact]
        public async Task PostProduct_ReturnBadRequest_IdNull()
        {
            // Arrange
            ProductViewModel productViewModel = new ProductViewModel();
            _mockrepo.Setup(repo => repo.Product.GetProductById(productViewModel.Id))
                .ReturnsAsync((Product)null);
            //Act
            var result = await _controller.PostProduct(productViewModel);
            //Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            badResult.Value.Should().Be(LogMessage.InvalidId(nameof(Product)));
        }

        [Fact]
        public async Task PostProduct_ReturnCreatedResponse()
        {
            //Arrange
            ProductViewModel productVM = new ProductViewModel()
            {
                Id = 1,
                Name = "Product1"
            };
            _mockrepo.Setup(repo => repo.Product.CreateProduct(_mapper.Map<Product>(productVM)));

            //Act
            var result = await _controller.PostProduct(productVM);
            //Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<ProductViewModel>(actionResult.Value);
            _mockrepo.Verify();
            Assert.Equal(productVM, returnValue);
        }

        [Fact]
        public async Task PutProduct_ReturnBadRequest_IdNotMatch()
        {
            //Arrange
            int id = 1;
            ProductViewModel productVM = new ProductViewModel()
            {
                Id = 2,
                Name = "Product2"
            };
            // Act
            var result = await _controller.PutProduct(id, productVM);
            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            badResult.Value.Should().Be(LogMessage.IdNotMatch());
        }

        [Fact]
        public async Task PutProduct_ReturnNoContent()
        {
            //Arrange
            int id = 1;
            ProductViewModel productVM = new ProductViewModel()
            {
                Id = 1,
                Name = "Trang Uyen"
            };
            _mockrepo.Setup(repo => repo.Product.UpdateProduct(_mapper.Map<Product>(productVM)));
            // Act
            var result = await _controller.PutProduct(id, productVM);
            // Assert
            var badResult = Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_ReturnBadRequest_ExistRelatedEntity()
        {
            //Arrange
            int id = 1;
            List<OrderDetail> list = new List<OrderDetail>();
            list.Add(new OrderDetail());
            _mockrepo.Setup(repo => repo.OrderDetail.OrderDetailsByProduct(id)).ReturnsAsync(list);
            _mockrepo.Setup(repo => repo.Product.DeleteProduct((Product)null));
            //Act
            var result = await _controller.DeleteProduct(id);
            //Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            badRequest.Value.Should().Be(LogMessage.DeleteError(nameof(Product), id, nameof(OrderDetail)));
        }

        [Fact]
        public async Task DeleteProduct_ReturnNoContent()
        {
            //Arrange
            int id = 1;
            List<OrderDetail> list = new List<OrderDetail>();
            _mockrepo.Setup(repo => repo.OrderDetail.OrderDetailsByProduct(id)).ReturnsAsync(list);
            _mockrepo.Setup(repo => repo.Product.DeleteProduct((Product)null));
            //Act
            var result = await _controller.DeleteProduct(id);
            //Assert
            var badRequest = Assert.IsType<NoContentResult>(result);
        }

        private List<Product> GetTestProducts()
        {
            var products = new List<Product>();
            products.Add(new Product()
            {
                Id = 1,
                Name = "Product1",
                UnitPrice = 5000
            });
            products.Add(new Product()
            {
                Id = 2,
                Name = "Product2",
                UnitPrice = 7000                
            });
            return products;
        }
    }
}
