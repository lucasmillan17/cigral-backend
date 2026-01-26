using CigralBackend.Application.Dtos;
using CigralBackend.Application.Services;
using CigralBackend.Domain;
using CigralBackend.Domain.Enums;
using CigralBackend.Domain.Exceptions;
using CigralBackend.Domain.Wrappers;
using CigralBackend.Infraestructure.Database.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace CigralBackend.Tests.Services
{
    public class ProductoServiceTests
    {
        private readonly Mock<IRepository> _mockRepository;
        private readonly ProductoService _service;

        public ProductoServiceTests()
        {
            _mockRepository = new Mock<IRepository>();
            _service = new ProductoService(_mockRepository.Object);
        }

        #region CreateProducto Tests

        [Fact]
        public async Task CreateProducto_ConDatosValidos_DeberiaCrearProducto()
        {
            // Arrange
            var request = new ProductoModelRequest(
                "Producto Test",
                "Descripcion Test",
                "12345678901234",
                true,
                100.50m,
                null
            );

            _mockRepository.Setup(r => r.First<Producto>(It.IsAny<Expression<Func<Producto, bool>>>()))
                          .ReturnsAsync((Producto)null);

            _mockRepository.Setup(r => r.Add<Producto>(It.IsAny<Producto>()))
                          .ReturnsAsync((Producto p) => p);

            // Act
            var result = await _service.CreateProducto(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Producto Test", result.Nombre);
            Assert.Equal("12345678901234", result.GTIN);
            Assert.Equal(100.50m, result.Precio);
            _mockRepository.Verify(r => r.Add<Producto>(It.IsAny<Producto>()), Times.Once);
        }

        [Fact]
        public async Task CreateProducto_GTINDuplicado_DeberiaLanzarDomainException()
        {
            // Arrange
            var request = new ProductoModelRequest(
                "Producto Test",
                "Descripcion",
                "12345678901234",
                true,
                100m,
                null
            );

            var productoExistente = new Producto { GTIN = "12345678901234" };

            _mockRepository.Setup(r => r.First<Producto>(It.IsAny<Expression<Func<Producto, bool>>>()))
                          .ReturnsAsync(productoExistente);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.CreateProducto(request)
            );

            Assert.Equal(DomainErrorCode.GtinDuplicado, exception.Code);
            Assert.Contains("12345678901234", exception.Message);
            _mockRepository.Verify(r => r.Add<Producto>(It.IsAny<Producto>()), Times.Never);
        }

        [Fact]
        public async Task CreateProducto_NombreDuplicado_DeberiaLanzarDomainException()
        {
            // Arrange
            var request = new ProductoModelRequest(
                "Producto Existente",
                "Descripcion",
                "12345678901234",
                true,
                100m,
                null
            );

            _mockRepository.SetupSequence(r => r.First<Producto>(It.IsAny<Expression<Func<Producto, bool>>>()))
                          .ReturnsAsync((Producto)null) // Primera llamada (GTIN) - no existe
                          .ReturnsAsync(new Producto { Nombre = "Producto Existente" }); // Segunda llamada (Nombre) - existe

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.CreateProducto(request)
            );

            Assert.Equal(DomainErrorCode.NombreProductoDuplicado, exception.Code);
            Assert.Contains("Producto Existente", exception.Message);
        }

        [Fact]
        public async Task CreateProducto_MarcaNoExiste_DeberiaLanzarDomainException()
        {
            // Arrange
            var request = new ProductoModelRequest(
                "Producto Test",
                "Descripcion",
                "12345678901234",
                true,
                100m,
                "Marca Inexistente" // Marca que no existe
            );

            _mockRepository.Setup(r => r.First<Producto>(It.IsAny<Expression<Func<Producto, bool>>>()))
                          .ReturnsAsync((Producto)null);

            _mockRepository.Setup(r => r.First<Marca>(It.IsAny<Expression<Func<Marca, bool>>>()))
                          .ReturnsAsync((Marca)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.CreateProducto(request)
            );

            Assert.Equal(DomainErrorCode.MarcaNoValida, exception.Code);
            Assert.Contains("Marca Inexistente", exception.Message);
        }

        [Fact]
        public async Task CreateProducto_ConMarcaValida_DeberiaCrearProducto()
        {
            // Arrange
            var request = new ProductoModelRequest(
                "Producto Test",
                "Descripcion",
                "12345678901234",
                true,
                100m,
                "Marca Test" // Marca existente
            );

            _mockRepository.Setup(r => r.First<Producto>(It.IsAny<Expression<Func<Producto, bool>>>()))
                          .ReturnsAsync((Producto)null);

            _mockRepository.Setup(r => r.First<Marca>(It.IsAny<Expression<Func<Marca, bool>>>()))
                          .ReturnsAsync(new Marca { Id = 1, Nombre = "Marca Test" });

            _mockRepository.Setup(r => r.Add<Producto>(It.IsAny<Producto>()))
                          .ReturnsAsync((Producto p) => p);

            // Act
            var result = await _service.CreateProducto(request);

            // Assert
            Assert.NotNull(result);
            _mockRepository.Verify(r => r.First<Marca>(It.IsAny<Expression<Func<Marca, bool>>>()), Times.Once);
        }

        #endregion

        #region GetProductoById Tests

        [Fact]
        public async Task GetProductoById_ProductoExiste_DeberiaRetornarProducto()
        {
            // Arrange
            var producto = new Producto
            {
                Id = 1,
                Nombre = "Producto Test",
                GTIN = "12345678901234",
                Descripcion = "Descripcion",
                Precio = 100m,
                Marca = new Marca { Nombre = "Marca Test" }
            };

            _mockRepository.Setup(r => r.GetById<Producto>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(producto);

            // Act
            var result = await _service.GetProductoById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Producto Test", result.Nombre);
            Assert.Equal("Marca Test", result.Marca);
        }

        [Fact]
        public async Task GetProductoById_ProductoNoExiste_DeberiaLanzarNotFoundException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetById<Producto>(999, It.IsAny<string[]>()))
                          .ReturnsAsync((Producto)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.GetProductoById(999)
            );

            Assert.Equal("Producto", exception.EntityName);
            Assert.Equal(999, exception.Key);
        }

        #endregion

        #region UpdateProducto Tests

        [Fact]
        public async Task UpdateProducto_ProductoExiste_DeberiaActualizar()
        {
            // Arrange
            var productoExistente = new Producto
            {
                Id = 1,
                Nombre = "Producto Viejo",
                GTIN = "11111111111111",
                Precio = 50m
            };

            var request = new ProductoModelRequest(
                "Producto Actualizado",
                "Nueva Descripcion",
                "22222222222222",
                true,
                150m,
                null
            );

            _mockRepository.Setup(r => r.GetById<Producto>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(productoExistente);

            _mockRepository.Setup(r => r.First<Producto>(It.IsAny<Expression<Func<Producto, bool>>>()))
                          .ReturnsAsync((Producto)null);

            _mockRepository.Setup(r => r.Update<Producto>(It.IsAny<Producto>()))
                          .ReturnsAsync((Producto p) => p);

            // Act
            var result = await _service.UpdateProducto(1, request);

            // Assert
            Assert.Equal("Producto Actualizado", result.Nombre);
            Assert.Equal("22222222222222", result.GTIN);
            Assert.Equal(150m, result.Precio);
            _mockRepository.Verify(r => r.Update<Producto>(It.IsAny<Producto>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProducto_ProductoNoExiste_DeberiaLanzarNotFoundException()
        {
            // Arrange
            var request = new ProductoModelRequest("Test", "Desc", "12345678901234", true, 100m, null);

            _mockRepository.Setup(r => r.GetById<Producto>(999, It.IsAny<string[]>()))
                          .ReturnsAsync((Producto)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.UpdateProducto(999, request)
            );

            Assert.Equal("Producto", exception.EntityName);
            Assert.Equal(999, exception.Key);
        }

        [Fact]
        public async Task UpdateProducto_GTINDuplicadoEnOtroProducto_DeberiaLanzarDomainException()
        {
            // Arrange
            var productoExistente = new Producto
            {
                Id = 1,
                GTIN = "11111111111111"
            };

            var otroProducto = new Producto
            {
                Id = 2,
                GTIN = "22222222222222"
            };

            var request = new ProductoModelRequest(
                "Producto Test",
                "Desc",
                "22222222222222", // GTIN del otro producto
                true,
                100m,
                null
            );

            _mockRepository.Setup(r => r.GetById<Producto>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(productoExistente);

            _mockRepository.Setup(r => r.First<Producto>(It.IsAny<Expression<Func<Producto, bool>>>()))
                          .ReturnsAsync(otroProducto);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.UpdateProducto(1, request)
            );

            Assert.Equal(DomainErrorCode.GtinDuplicado, exception.Code);
        }

        #endregion

        #region DeleteProducto Tests

        [Fact]
        public async Task DeleteProducto_ProductoExiste_DeberiaEliminar()
        {
            // Arrange
            var producto = new Producto { Id = 1, Nombre = "Producto Test" };

            _mockRepository.Setup(r => r.GetById<Producto>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(producto);

            _mockRepository.Setup(r => r.Delete<Producto>(It.IsAny<Producto>()))
                          .ReturnsAsync(producto);

            // Act
            await _service.DeleteProducto(1);

            // Assert
            _mockRepository.Verify(r => r.Delete<Producto>(It.Is<Producto>(p => p.Id == 1)), Times.Once);
        }

        [Fact]
        public async Task DeleteProducto_ProductoNoExiste_DeberiaLanzarNotFoundException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetById<Producto>(999, It.IsAny<string[]>()))
                          .ReturnsAsync((Producto)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.DeleteProducto(999)
            );

            Assert.Equal("Producto", exception.EntityName);
            Assert.Equal(999, exception.Key);
            _mockRepository.Verify(r => r.Delete<Producto>(It.IsAny<Producto>()), Times.Never);
        }

        #endregion

        #region GetProductoFiltered Tests

        [Fact]
        public async Task GetProductoFiltered_PorNombre_DeberiaFiltrarCorrectamente()
        {
            // Arrange
            var filtros = new ProductoFilters("Coca", null, null, 1, 10);

            var productos = new PagedResult<Producto>
            {
                Items = new List<Producto>
                {
                    new Producto { Id = 1, Nombre = "Coca Cola", GTIN = "11111111111111" }
                },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 10
            };

            _mockRepository.Setup(r => r.GetFiltered<Producto>(
                It.IsAny<Expression<Func<Producto, bool>>>(),
                1,
                10,
                It.IsAny<string[]>()))
                          .ReturnsAsync(productos);

            // Act
            var result = await _service.GetProductoFiltered(filtros);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("Coca Cola", result.Items[0].Nombre);
        }

        [Fact]
        public async Task GetProductoFiltered_PorGTIN_DeberiaFiltrarCorrectamente()
        {
            // Arrange
            var filtros = new ProductoFilters(null, "1111", null, 1, 10);

            var productos = new PagedResult<Producto>
            {
                Items = new List<Producto>
                {
                    new Producto { Id = 1, Nombre = "Producto", GTIN = "11111111111111" }
                },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 10
            };

            _mockRepository.Setup(r => r.GetFiltered<Producto>(
                It.IsAny<Expression<Func<Producto, bool>>>(),
                1,
                10,
                It.IsAny<string[]>()))
                          .ReturnsAsync(productos);

            // Act
            var result = await _service.GetProductoFiltered(filtros);

            // Assert
            Assert.Single(result.Items);
            Assert.Contains("1111", result.Items[0].GTIN);
        }

        [Fact]
        public async Task GetProductoFiltered_SinFiltros_DeberiaRetornarTodos()
        {
            // Arrange
            var filtros = new ProductoFilters(null, null, null, 1, 10);

            var productos = new PagedResult<Producto>
            {
                Items = new List<Producto>
                {
                    new Producto { Id = 1, Nombre = "Producto 1", GTIN = "11111111111111" },
                    new Producto { Id = 2, Nombre = "Producto 2", GTIN = "22222222222222" }
                },
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10
            };

            _mockRepository.Setup(r => r.GetFiltered<Producto>(
                It.IsAny<Expression<Func<Producto, bool>>>(),
                1,
                10,
                It.IsAny<string[]>()))
                          .ReturnsAsync(productos);

            // Act
            var result = await _service.GetProductoFiltered(filtros);

            // Assert
            Assert.Equal(2, result.Items.Count);
        }

        #endregion
    }
}
