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
    public class MarcaServiceTests
    {
        private readonly Mock<IRepository> _mockRepository;
        private readonly MarcaService _service;

        public MarcaServiceTests()
        {
            _mockRepository = new Mock<IRepository>();
            _service = new MarcaService(_mockRepository.Object);
        }

        #region CreateMarca Tests

        [Fact]
        public async Task CreateMarca_ConNombreValido_DeberiaCrearMarca()
        {
            // Arrange
            var request = new MarcaRequest("Marca Test");

            _mockRepository.Setup(r => r.First<Marca>(It.IsAny<Expression<Func<Marca, bool>>>()))
                          .ReturnsAsync((Marca)null);

            _mockRepository.Setup(r => r.Add<Marca>(It.IsAny<Marca>()))
                          .ReturnsAsync((Marca m) => { m.Id = 1; return m; });

            // Act
            var result = await _service.CreateMarca(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Marca Test", result.Nombre);
            Assert.Equal(1, result.Id);
            _mockRepository.Verify(r => r.Add<Marca>(It.IsAny<Marca>()), Times.Once);
        }

        [Fact]
        public async Task CreateMarca_NombreDuplicado_DeberiaLanzarDomainException()
        {
            // Arrange
            var request = new MarcaRequest("Marca Existente");

            var marcaExistente = new Marca { Id = 1, Nombre = "Marca Existente" };

            _mockRepository.Setup(r => r.First<Marca>(It.IsAny<Expression<Func<Marca, bool>>>()))
                          .ReturnsAsync(marcaExistente);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.CreateMarca(request)
            );

            Assert.Equal(DomainErrorCode.MarcaDuplicada, exception.Code);
            Assert.Contains("Marca Existente", exception.Message);
            _mockRepository.Verify(r => r.Add<Marca>(It.IsAny<Marca>()), Times.Never);
        }

        #endregion

        #region GetMarcaById Tests

        [Fact]
        public async Task GetMarcaById_MarcaExiste_DeberiaRetornarMarca()
        {
            // Arrange
            var marca = new Marca { Id = 1, Nombre = "Marca Test" };

            _mockRepository.Setup(r => r.GetById<Marca>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(marca);

            // Act
            var result = await _service.GetMarcaById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Marca Test", result.Nombre);
        }

        [Fact]
        public async Task GetMarcaById_MarcaNoExiste_DeberiaLanzarNotFoundException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetById<Marca>(999, It.IsAny<string[]>()))
                          .ReturnsAsync((Marca)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.GetMarcaById(999)
            );

            Assert.Equal("Marca", exception.EntityName);
            Assert.Equal(999, exception.Key);
        }

        #endregion

        #region UpdateMarca Tests

        [Fact]
        public async Task UpdateMarca_MarcaExiste_DeberiaActualizar()
        {
            // Arrange
            var marcaExistente = new Marca { Id = 1, Nombre = "Marca Vieja" };
            var request = new MarcaRequest("Marca Actualizada");

            _mockRepository.Setup(r => r.GetById<Marca>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(marcaExistente);

            _mockRepository.Setup(r => r.First<Marca>(It.IsAny<Expression<Func<Marca, bool>>>()))
                          .ReturnsAsync((Marca)null);

            _mockRepository.Setup(r => r.Update<Marca>(It.IsAny<Marca>()))
                          .ReturnsAsync((Marca m) => m);

            // Act
            var result = await _service.UpdateMarca(1, request);

            // Assert
            Assert.Equal("Marca Actualizada", result.Nombre);
            _mockRepository.Verify(r => r.Update<Marca>(It.IsAny<Marca>()), Times.Once);
        }

        [Fact]
        public async Task UpdateMarca_MarcaNoExiste_DeberiaLanzarNotFoundException()
        {
            // Arrange
            var request = new MarcaRequest("Marca Test");

            _mockRepository.Setup(r => r.GetById<Marca>(999, It.IsAny<string[]>()))
                          .ReturnsAsync((Marca)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.UpdateMarca(999, request)
            );

            Assert.Equal("Marca", exception.EntityName);
            Assert.Equal(999, exception.Key);
        }

        [Fact]
        public async Task UpdateMarca_NombreDuplicadoEnOtraMarca_DeberiaLanzarDomainException()
        {
            // Arrange
            var marcaExistente = new Marca { Id = 1, Nombre = "Marca 1" };
            var otraMarca = new Marca { Id = 2, Nombre = "Marca 2" };
            var request = new MarcaRequest("Marca 2"); // Nombre de otra marca

            _mockRepository.Setup(r => r.GetById<Marca>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(marcaExistente);

            _mockRepository.Setup(r => r.First<Marca>(It.IsAny<Expression<Func<Marca, bool>>>()))
                          .ReturnsAsync(otraMarca);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.UpdateMarca(1, request)
            );

            Assert.Equal(DomainErrorCode.MarcaDuplicada, exception.Code);
        }

        #endregion

        #region DeleteMarca Tests

        [Fact]
        public async Task DeleteMarca_MarcaSinProductos_DeberiaEliminar()
        {
            // Arrange
            var marca = new Marca { Id = 1, Nombre = "Marca Test" };

            _mockRepository.Setup(r => r.GetById<Marca>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(marca);

            _mockRepository.Setup(r => r.First<Producto>(It.IsAny<Expression<Func<Producto, bool>>>()))
                          .ReturnsAsync((Producto)null);

            _mockRepository.Setup(r => r.Delete<Marca>(It.IsAny<Marca>()))
                          .ReturnsAsync(marca);

            // Act
            await _service.DeleteMarca(1);

            // Assert
            _mockRepository.Verify(r => r.Delete<Marca>(It.Is<Marca>(m => m.Id == 1)), Times.Once);
        }

        [Fact]
        public async Task DeleteMarca_MarcaNoExiste_DeberiaLanzarNotFoundException()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetById<Marca>(999, It.IsAny<string[]>()))
                          .ReturnsAsync((Marca)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.DeleteMarca(999)
            );

            Assert.Equal("Marca", exception.EntityName);
            Assert.Equal(999, exception.Key);
            _mockRepository.Verify(r => r.Delete<Marca>(It.IsAny<Marca>()), Times.Never);
        }

        [Fact]
        public async Task DeleteMarca_MarcaTieneProductos_DeberiaLanzarDomainException()
        {
            // Arrange
            var marca = new Marca { Id = 1, Nombre = "Marca Test" };
            var producto = new Producto { Id = 1, MarcaId = 1, Nombre = "Producto Test" };

            _mockRepository.Setup(r => r.GetById<Marca>(1, It.IsAny<string[]>()))
                          .ReturnsAsync(marca);

            _mockRepository.Setup(r => r.First<Producto>(It.IsAny<Expression<Func<Producto, bool>>>()))
                          .ReturnsAsync(producto);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(
                () => _service.DeleteMarca(1)
            );

            Assert.Equal(DomainErrorCode.MarcaTieneProductos, exception.Code);
            Assert.Contains("Marca Test", exception.Message);
            _mockRepository.Verify(r => r.Delete<Marca>(It.IsAny<Marca>()), Times.Never);
        }

        #endregion

        #region GetMarcasAsync Tests

        [Fact]
        public async Task GetMarcasAsync_DeberiaRetornarTodasLasMarcas()
        {
            // Arrange
            var marcas = new PagedResult<Marca>
            {
                Items = new List<Marca>
                {
                    new Marca { Id = 1, Nombre = "Marca 1" },
                    new Marca { Id = 2, Nombre = "Marca 2" }
                },
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10
            };

            _mockRepository.Setup(r => r.GetAll<Marca>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string[]>()))
                          .ReturnsAsync(marcas);

            // Act
            var result = await _service.GetMarcasAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Marca 1", result[0].Nombre);
            Assert.Equal("Marca 2", result[1].Nombre);
        }

        [Fact]
        public async Task GetMarcasAsync_SinMarcas_DeberiaRetornarListaVacia()
        {
            // Arrange
            var marcas = new PagedResult<Marca>
            {
                Items = new List<Marca>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            };

            _mockRepository.Setup(r => r.GetAll<Marca>(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string[]>()))
                          .ReturnsAsync(marcas);

            // Act
            var result = await _service.GetMarcasAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetMarcasByNombre Tests

        [Fact]
        public async Task GetMarcasByNombre_ConCoincidencias_DeberiaRetornarMarcas()
        {
            // Arrange
            var marcas = new PagedResult<Marca>
            {
                Items = new List<Marca>
                {
                    new Marca { Id = 1, Nombre = "Coca Cola" },
                    new Marca { Id = 2, Nombre = "Pepsi Cola" }
                },
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 50
            };

            _mockRepository.Setup(r => r.GetFiltered<Marca>(
                It.IsAny<Expression<Func<Marca, bool>>>(),
                1,
                50,
                null,
                It.IsAny<string[]>()))
                          .ReturnsAsync(marcas);

            // Act
            var result = await _service.GetMarcasByNombre("Cola");

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetMarcasByNombre_SinCoincidencias_DeberiaRetornarListaVacia()
        {
            // Arrange
            var marcas = new PagedResult<Marca>
            {
                Items = new List<Marca>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 50
            };

            _mockRepository.Setup(r => r.GetFiltered<Marca>(
                It.IsAny<Expression<Func<Marca, bool>>>(),
                1,
                50,
                null,
                It.IsAny<string[]>()))
                          .ReturnsAsync(marcas);

            // Act
            var result = await _service.GetMarcasByNombre("NoExiste");

            // Assert
            Assert.Empty(result);
        }

        #endregion
    }
}
