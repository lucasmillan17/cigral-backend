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
    public class ExistenciaServiceTests
    {
        private readonly Mock<IRepository> _mockRepository;
        private readonly ExistenciaService _service;

        public ExistenciaServiceTests()
        {
            _mockRepository = new Mock<IRepository>();
            _service = new ExistenciaService(_mockRepository.Object);
        }

        #region AumentarStock Tests

        [Fact]
        public async Task AumentarStock_NuevaExistencia_DeberiaCrearExistencia()
        {
            // Arrange
            var request = new ExistenciaModelRequest(1, 1, null, null, null, 10);

            var producto = new Producto { Id = 1, Nombre = "Producto Test", GTIN = "1234567890123", EsUnitario = false };
            var deposito = new Deposito { Id = 1, Nombre = "Deposito Test" };

            _mockRepository.Setup(r => r.GetById<Producto>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(producto);

            _mockRepository.Setup(r => r.GetById<Deposito>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(deposito);

            _mockRepository.Setup(r => r.First<Existencia>(It.IsAny<Expression<Func<Existencia, bool>>>()))
                          .ReturnsAsync((Existencia)null);

            _mockRepository.Setup(r => r.Add<Existencia>(It.IsAny<Existencia>()))
                          .ReturnsAsync((Existencia e) => { e.Id = 1; return e; });

            // Act
            var result = await _service.AumentarStock(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ProductoId);
            Assert.Equal(1, result.DepositoId);
            Assert.Equal(10, result.Cantidad);
            _mockRepository.Verify(r => r.Add<Existencia>(It.IsAny<Existencia>()), Times.Once);
        }

        [Fact]
        public async Task AumentarStock_ExistenciaExiste_DeberiaSumarCantidad()
        {
            // Arrange
            var request = new ExistenciaModelRequest(1, 1, null, null, null, 10);

            var producto = new Producto { Id = 1, Nombre = "Producto Test", GTIN = "123", EsUnitario = false };
            var deposito = new Deposito { Id = 1, Nombre = "Deposito Test" };
            var existenciaExistente = new Existencia 
            { 
                Id = 1, 
                ProductoId = 1, 
                DepositoId = 1, 
                Cantidad = 5 
            };

            _mockRepository.Setup(r => r.GetById<Producto>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(producto);

            _mockRepository.Setup(r => r.GetById<Deposito>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(deposito);

            _mockRepository.Setup(r => r.First<Existencia>(It.IsAny<Expression<Func<Existencia, bool>>>()))
                          .ReturnsAsync(existenciaExistente);

            _mockRepository.Setup(r => r.Update<Existencia>(It.IsAny<Existencia>()))
                          .ReturnsAsync((Existencia e) => e);

            // Act
            var result = await _service.AumentarStock(request);

            // Assert
            Assert.Equal(15, result.Cantidad); // 5 + 10
            _mockRepository.Verify(r => r.Update<Existencia>(It.IsAny<Existencia>()), Times.Once);
            _mockRepository.Verify(r => r.Add<Existencia>(It.IsAny<Existencia>()), Times.Never);
        }

        [Fact]
        public async Task AumentarStock_ProductoNoExiste_DeberiaLanzarNotFoundException()
        {
            // Arrange
            var request = new ExistenciaModelRequest(1, 999, null, null, null, 10);

            _mockRepository.Setup(r => r.GetById<Producto>(999, It.IsAny<string[]>()))
                          .ReturnsAsync((Producto)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.AumentarStock(request)
            );

            Assert.Equal("Producto", exception.EntityName);
            Assert.Equal(999, exception.Key);
        }

        [Fact]
        public async Task AumentarStock_DepositoNoExiste_DeberiaLanzarNotFoundException()
        {
            // Arrange
            var request = new ExistenciaModelRequest(999, 1, null, null, null, 10);

            var producto = new Producto { Id = 1, Nombre = "Producto Test", EsUnitario = false };

            _mockRepository.Setup(r => r.GetById<Producto>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(producto);

            _mockRepository.Setup(r => r.GetById<Deposito>(999, It.IsAny<string[]>()))
                          .ReturnsAsync((Deposito)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.AumentarStock(request)
            );

            Assert.Equal("Deposito", exception.EntityName);
            Assert.Equal(999, exception.Key);
        }

        [Fact]
        public async Task AumentarStock_LoteNoExiste_DeberiaLanzarNotFoundException()
        {
            // Arrange
            var request = new ExistenciaModelRequest(1, 1, null, 999, null, 10);

            var producto = new Producto { Id = 1, Nombre = "Producto Test", EsUnitario = false };
            var deposito = new Deposito { Id = 1, Nombre = "Deposito Test" };

            _mockRepository.Setup(r => r.GetById<Producto>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(producto);

            _mockRepository.Setup(r => r.GetById<Deposito>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(deposito);

            _mockRepository.Setup(r => r.GetById<Lote>(999, It.IsAny<string[]>()))
                          .ReturnsAsync((Lote)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.AumentarStock(request)
            );

            Assert.Equal("Lote", exception.EntityName);
            Assert.Equal(999, exception.Key);
        }

        [Fact]
        public async Task AumentarStock_CantidadCero_DeberiaLanzarDomainException()
        {
            // Arrange
            var request = new ExistenciaModelRequest(1, 1, null, null, null, 0);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.AumentarStock(request)
            );

            Assert.Equal(DomainErrorCode.CantidadInvalida, exception.Code);
        }

        [Fact]
        public async Task AumentarStock_ProductoUnitarioConCantidadMayorA1_DeberiaLanzarDomainException()
        {
            // Arrange
            var request = new ExistenciaModelRequest(1, 1, null, null, null, 5);

            var producto = new Producto { Id = 1, Nombre = "Producto Unitario", EsUnitario = true };

            _mockRepository.Setup(r => r.GetById<Producto>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(producto);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.AumentarStock(request)
            );

            Assert.Equal(DomainErrorCode.ProductoUnitarioCantidadInvalida, exception.Code);
        }

        [Fact]
        public async Task AumentarStock_LoteVencido_DeberiaLanzarDomainException()
        {
            // Arrange
            var request = new ExistenciaModelRequest(1, 1, null, 1, null, 10);

            var producto = new Producto { Id = 1, Nombre = "Producto Test", EsUnitario = false };
            var deposito = new Deposito { Id = 1, Nombre = "Deposito Test" };
            var lote = new Lote { Id = 1, CodigoLote = "LOTE001", FechaVencimiento = DateTime.Now.AddDays(-1) };

            _mockRepository.Setup(r => r.GetById<Producto>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(producto);

            _mockRepository.Setup(r => r.GetById<Deposito>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(deposito);

            _mockRepository.Setup(r => r.GetById<Lote>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(lote);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.AumentarStock(request)
            );

            Assert.Equal(DomainErrorCode.LoteVencido, exception.Code);
        }

        [Fact]
        public async Task AumentarStock_NumSerieDuplicado_DeberiaLanzarDomainException()
        {
            // Arrange
            var request = new ExistenciaModelRequest(1, 1, "SERIE001", null, null, 1);

            var producto = new Producto { Id = 1, Nombre = "Producto Test", EsUnitario = false };
            var deposito = new Deposito { Id = 1, Nombre = "Deposito Test" };

            _mockRepository.Setup(r => r.GetById<Producto>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(producto);

            _mockRepository.Setup(r => r.GetById<Deposito>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(deposito);

            // Primera llamada: verificar número de serie duplicado
            _mockRepository.SetupSequence(r => r.First<Existencia>(It.IsAny<Expression<Func<Existencia, bool>>>()))
                          .ReturnsAsync(new Existencia { Id = 2, NumSerie = "SERIE001", ProductoId = 1 }) // NumSerie duplicado
                          .ReturnsAsync((Existencia)null); // No existe para crear

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.AumentarStock(request)
            );

            Assert.Equal(DomainErrorCode.SerieDuplicada, exception.Code);
        }

        #endregion

        #region DisminuirStock Tests

        [Fact]
        public async Task DisminuirStock_ConStockSuficiente_DeberiaDisminuirCantidad()
        {
            // Arrange
            var request = new ExistenciaModelRequest(1, 1, null, null, null, 5);

            var producto = new Producto { Id = 1, Nombre = "Producto Test", GTIN = "123", EsUnitario = false };
            var deposito = new Deposito { Id = 1, Nombre = "Deposito Test" };
            var existencia = new Existencia 
            { 
                Id = 1, 
                ProductoId = 1, 
                DepositoId = 1, 
                Cantidad = 10 
            };

            _mockRepository.Setup(r => r.GetById<Producto>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(producto);

            _mockRepository.Setup(r => r.GetById<Deposito>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(deposito);

            _mockRepository.Setup(r => r.First<Existencia>(It.IsAny<Expression<Func<Existencia, bool>>>()))
                          .ReturnsAsync(existencia);

            _mockRepository.Setup(r => r.Update<Existencia>(It.IsAny<Existencia>()))
                          .ReturnsAsync((Existencia e) => e);

            // Act
            var result = await _service.DisminuirStock(request);

            // Assert
            Assert.Equal(5, result.Cantidad); // 10 - 5
            _mockRepository.Verify(r => r.Update<Existencia>(It.IsAny<Existencia>()), Times.Once);
        }

        [Fact]
        public async Task DisminuirStock_ExistenciaNoExiste_DeberiaLanzarNotFoundException()
        {
            // Arrange
            var request = new ExistenciaModelRequest(1, 1, null, null, null, 5);

            var producto = new Producto { Id = 1, Nombre = "Producto Test", EsUnitario = false };
            var deposito = new Deposito { Id = 1, Nombre = "Deposito Test" };

            _mockRepository.Setup(r => r.GetById<Producto>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(producto);

            _mockRepository.Setup(r => r.GetById<Deposito>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(deposito);

            _mockRepository.Setup(r => r.First<Existencia>(It.IsAny<Expression<Func<Existencia, bool>>>()))
                          .ReturnsAsync((Existencia)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.DisminuirStock(request)
            );

            Assert.Equal("Existencia", exception.EntityName);
        }

        [Fact]
        public async Task DisminuirStock_StockInsuficiente_DeberiaLanzarDomainException()
        {
            // Arrange
            var request = new ExistenciaModelRequest(1, 1, null, null, null, 15);

            var producto = new Producto { Id = 1, Nombre = "Producto Test", EsUnitario = false };
            var deposito = new Deposito { Id = 1, Nombre = "Deposito Test" };
            var existencia = new Existencia 
            { 
                Id = 1, 
                ProductoId = 1, 
                DepositoId = 1, 
                Cantidad = 10 
            };

            _mockRepository.Setup(r => r.GetById<Producto>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(producto);

            _mockRepository.Setup(r => r.GetById<Deposito>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(deposito);

            _mockRepository.Setup(r => r.First<Existencia>(It.IsAny<Expression<Func<Existencia, bool>>>()))
                          .ReturnsAsync(existencia);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.DisminuirStock(request)
            );

            Assert.Equal(DomainErrorCode.StockInsuficiente, exception.Code);
        }

        [Fact]
        public async Task DisminuirStock_CantidadCero_DeberiaLanzarDomainException()
        {
            // Arrange
            var request = new ExistenciaModelRequest(1, 1, null, null, null, 0);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.DisminuirStock(request)
            );

            Assert.Equal(DomainErrorCode.CantidadInvalida, exception.Code);
        }

        [Fact]
        public async Task DisminuirStock_ProductoUnitarioConCantidadDistintaDe1_DeberiaLanzarDomainException()
        {
            // Arrange
            var request = new ExistenciaModelRequest(1, 1, null, null, null, 5);

            var producto = new Producto { Id = 1, Nombre = "Producto Unitario", EsUnitario = true };

            _mockRepository.Setup(r => r.GetById<Producto>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(producto);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.DisminuirStock(request)
            );

            Assert.Equal(DomainErrorCode.ProductoUnitarioCantidadInvalida, exception.Code);
        }

        #endregion

        #region GetExistenciaById Tests

        [Fact]
        public async Task GetExistenciaById_ExistenciaExiste_DeberiaRetornarExistencia()
        {
            // Arrange
            var existencia = new Existencia
            {
                Id = 1,
                ProductoId = 1,
                DepositoId = 1,
                Cantidad = 10,
                Producto = new Producto { Nombre = "Producto Test", GTIN = "1234567890123" },
                Deposito = new Deposito { Nombre = "Deposito Test" }
            };

            _mockRepository.Setup(r => r.GetById<Existencia>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(existencia);

            // Act
            var result = await _service.GetExistenciaById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Producto Test", result.ProductoNombre);
            Assert.Equal("Deposito Test", result.DepositoNombre);
        }

        [Fact]
        public async Task GetExistenciaById_ExistenciaNoExiste_DeberiaLanzarNotFoundException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetById<Existencia>(999, It.IsAny<string[]>()))
                          .ReturnsAsync((Existencia)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.GetExistenciaById(999)
            );

            Assert.Equal("Existencia", exception.EntityName);
            Assert.Equal(999, exception.Key);
        }

        #endregion

        #region DeleteExistencia Tests

        [Fact]
        public async Task DeleteExistencia_ConCantidadCero_DeberiaEliminar()
        {
            // Arrange
            var existencia = new Existencia { Id = 1, ProductoId = 1, DepositoId = 1, Cantidad = 0 };

            _mockRepository.Setup(r => r.GetById<Existencia>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(existencia);

            _mockRepository.Setup(r => r.Delete<Existencia>(It.IsAny<Existencia>()))
                          .ReturnsAsync(existencia);

            // Act
            await _service.DeleteExistencia(1);

            // Assert
            _mockRepository.Verify(r => r.Delete<Existencia>(It.Is<Existencia>(e => e.Id == 1)), Times.Once);
        }

        [Fact]
        public async Task DeleteExistencia_ExistenciaNoExiste_DeberiaLanzarNotFoundException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetById<Existencia>(999, It.IsAny<string[]>()))
                          .ReturnsAsync((Existencia)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.DeleteExistencia(999)
            );

            Assert.Equal("Existencia", exception.EntityName);
            Assert.Equal(999, exception.Key);
        }

        [Fact]
        public async Task DeleteExistencia_ConStock_DeberiaLanzarDomainException()
        {
            // Arrange
            var existencia = new Existencia { Id = 1, ProductoId = 1, DepositoId = 1, Cantidad = 10 };

            _mockRepository.Setup(r => r.GetById<Existencia>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(existencia);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.DeleteExistencia(1)
            );

            Assert.Equal(DomainErrorCode.StockInsuficiente, exception.Code);
            _mockRepository.Verify(r => r.Delete<Existencia>(It.IsAny<Existencia>()), Times.Never);
        }

        #endregion

        #region GetExistencias Tests

        [Fact]
        public async Task GetExistencias_DeberiaRetornarExistenciasPaginadas()
        {
            // Arrange
            var filters = new ExistenciaFilters(null, null, null, 1, 10);

            var existencias = new PagedResult<Existencia>
            {
                Items = new List<Existencia>
                {
                    new Existencia
                    {
                        Id = 1,
                        ProductoId = 1,
                        DepositoId = 1,
                        Cantidad = 10,
                        Producto = new Producto { Nombre = "Producto 1", GTIN = "123" },
                        Deposito = new Deposito { Nombre = "Deposito 1" }
                    }
                },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 10
            };

            _mockRepository.Setup(r => r.GetFiltered<Existencia>(
                It.IsAny<Expression<Func<Existencia, bool>>>(),
                1,
                10,
                It.IsAny<string[]>()))
                          .ReturnsAsync(existencias);

            // Act
            var result = await _service.GetExistencias(filters);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task GetExistencias_ConFiltros_DeberiaFiltrarCorrectamente()
        {
            // Arrange
            var filters = new ExistenciaFilters(1, 2, 3, 1, 10);

            var existencias = new PagedResult<Existencia>
            {
                Items = new List<Existencia>
                {
                    new Existencia
                    {
                        Id = 1,
                        ProductoId = 2,
                        DepositoId = 1,
                        LoteId = 3,
                        Cantidad = 10,
                        Producto = new Producto { Nombre = "Producto Filtrado", GTIN = "123" },
                        Deposito = new Deposito { Nombre = "Deposito Filtrado" },
                        Lote = new Lote { CodigoLote = "LOTE001" }
                    }
                },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 10
            };

            _mockRepository.Setup(r => r.GetFiltered<Existencia>(
                It.IsAny<Expression<Func<Existencia, bool>>>(),
                1,
                10,
                It.IsAny<string[]>()))
                          .ReturnsAsync(existencias);

            // Act
            var result = await _service.GetExistencias(filters);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("Producto Filtrado", result.Items[0].ProductoNombre);
        }

        #endregion
    }
}
