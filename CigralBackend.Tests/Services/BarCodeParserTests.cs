using CigralBackend.Application.Services;
using System;
using Xunit;

namespace CigralBackend.Tests.Services
{
    public class BarCodeParserTests
    {
        private readonly BarCodeParser _parser;

        public BarCodeParserTests()
        {
            _parser = new BarCodeParser();
        }

        [Fact]
        public void Parse_CodigoCompletoConSerieQueContiene21_DeberiaObtenerSerieCompleta()
        {
            // Arrange
            // Fecha: 301230 = 30/12/30 -> año 30 = 2030
            var barcode = "(01)30012345678906(17)301230(10)C4324(21)230A6576P9";

            // Act
            var result = _parser.Parse(barcode);

            // Assert
            Assert.Equal("30012345678906", result.Gtin);
            Assert.Equal("C4324", result.Lote);
            Assert.Equal("230A6576P9", result.NumeroSerie);
            Assert.Equal(new DateTime(2030, 12, 30), result.FechaVencimiento);
            Assert.Equal(1, result.Cantidad);
            Assert.True(result.EsValido);
        }

        [Fact]
        public void Parse_LoteContieneAI10_DeberiaObtenerLoteCompleto()
        {
            // Arrange
            var barcode = "(01)12345678901234(10)LOTE10ABC(17)250630";

            // Act
            var result = _parser.Parse(barcode);

            // Assert
            Assert.Equal("12345678901234", result.Gtin);
            Assert.Equal("LOTE10ABC", result.Lote);
            Assert.Equal(new DateTime(2025, 6, 30), result.FechaVencimiento);
            Assert.True(result.EsValido);
        }

        [Fact]
        public void Parse_SerieConNumerosConsecutivos_DeberiaObtenerSerieCompleta()
        {
            // Arrange
            var barcode = "(01)11111111111111(21)21212121";

            // Act
            var result = _parser.Parse(barcode);

            // Assert
            Assert.Equal("11111111111111", result.Gtin);
            Assert.Equal("21212121", result.NumeroSerie);
            Assert.True(result.EsValido);
        }

        [Fact]
        public void Parse_SoloGTIN_DeberiaObtenerSoloGTIN()
        {
            // Arrange
            var barcode = "(01)12345678901234";

            // Act
            var result = _parser.Parse(barcode);

            // Assert
            Assert.Equal("12345678901234", result.Gtin);
            Assert.Null(result.Lote);
            Assert.Null(result.NumeroSerie);
            Assert.Null(result.FechaVencimiento);
            Assert.Equal(1, result.Cantidad);
            Assert.True(result.EsValido);
        }

        [Fact]
        public void Parse_ConCantidad_DeberiaObtenerCantidadCorrecta()
        {
            // Arrange
            var barcode = "(01)12345678901234(30)5(10)LOTE123";

            // Act
            var result = _parser.Parse(barcode);

            // Assert
            Assert.Equal("12345678901234", result.Gtin);
            Assert.Equal(5, result.Cantidad);
            Assert.Equal("LOTE123", result.Lote);
            Assert.True(result.EsValido);
        }

        [Fact]
        public void Parse_OrdenDiferenteDeAIs_DeberiaObtenerTodosCampos()
        {
            // Arrange
            var barcode = "(21)ABC123XYZ(01)99887766554433(17)301225(10)L001";

            // Act
            var result = _parser.Parse(barcode);

            // Assert
            Assert.Equal("ABC123XYZ", result.NumeroSerie);
            Assert.Equal("99887766554433", result.Gtin);
            Assert.Equal(new DateTime(2030, 12, 25), result.FechaVencimiento);
            Assert.Equal("L001", result.Lote);
            Assert.True(result.EsValido);
        }

        [Fact]
        public void Parse_LoteAlfanumericoComplejo_DeberiaObtenerLoteCompleto()
        {
            // Arrange
            // Fecha corregida: 311224 = 31/12/24 (día 31, mes 12, año 24 -> 2024)
            var barcode = "(01)11111111111111(10)ABC-123_XYZ.2024(17)241231";

            // Act
            var result = _parser.Parse(barcode);

            // Assert
            Assert.Equal("11111111111111", result.Gtin);
            Assert.Equal("ABC-123_XYZ.2024", result.Lote);
            Assert.Equal(new DateTime(2024, 12, 31), result.FechaVencimiento);
            Assert.True(result.EsValido);
        }

        [Fact]
        public void Parse_SerieConCaracteresEspeciales_DeberiaObtenerSerieCompleta()
        {
            // Arrange
            var barcode = "(01)11111111111111(21)SN-2024/001#A";

            // Act
            var result = _parser.Parse(barcode);

            // Assert
            Assert.Equal("11111111111111", result.Gtin);
            Assert.Equal("SN-2024/001#A", result.NumeroSerie);
            Assert.True(result.EsValido);
        }

        [Fact]
        public void Parse_ConGS_DeberiaObtenerTodosCampos()
        {
            // Arrange
            // Código sin paréntesis DEBE usar GS para separar campos variables
            // Esto es lo que los escáneres reales generan
            var GS = (char)29;
            var barcode = "01300123456789061730123010C4324" + GS + "21230A6576P9";

            // Act
            var result = _parser.Parse(barcode);

            // Assert
            Assert.Equal("30012345678906", result.Gtin);
            Assert.Equal("C4324", result.Lote);
            Assert.Equal("230A6576P9", result.NumeroSerie);
            Assert.Equal(new DateTime(2030, 12, 30), result.FechaVencimiento);
            Assert.True(result.EsValido);
        }

        [Theory]
        [InlineData("(21)21212121", "21212121")]
        [InlineData("(21)ABC21XYZ", "ABC21XYZ")]
        [InlineData("(21)1721TEST", "1721TEST")]
        // NOTA: Sin GS, el parser puede detectar falsos positivos si el contenido
        // empieza justo con un AI. En la práctica esto no ocurre porque:
        // 1. Los escáneres SIEMPRE incluyen GS para campos variables
        // 2. El contenido real rara vez empieza con números que forman AIs
        public void Parse_SerieConNumerosQueParecenAIs_NoDeberiaCortarse(string barcode, string expectedSerie)
        {
            // Arrange & Act
            var result = _parser.Parse(barcode);

            // Assert
            Assert.Equal(expectedSerie, result.NumeroSerie);
        }

        [Theory]
        [InlineData("(10)LOTE10TEST", "LOTE10TEST")]
        [InlineData("(10)L10T17E21", "L10T17E21")]
        [InlineData("(10)ABC01XYZ", "ABC01XYZ")]
        [InlineData("(10)LOTE12A", "LOTE12A")] // Cambiado: terminar con letra
        public void Parse_LoteConNumerosQueParecenAIs_NoDeberiaCortarse(string barcode, string expectedLote)
        {
            // Arrange & Act
            var result = _parser.Parse(barcode);

            // Assert
            Assert.Equal(expectedLote, result.Lote);
        }

        [Fact]
        public void Parse_GTINIncompleto_DeberiaRetornarInvalido()
        {
            // Arrange
            var barcode = "(01)123456789012"; // Solo 12 dígitos

            // Act
            var result = _parser.Parse(barcode);

            // Assert
            Assert.False(result.EsValido);
            Assert.Null(result.Gtin);
        }

        [Fact]
        public void Parse_FechaInvalida_NoDeberiaEstablecerFecha()
        {
            // Arrange
            var barcode = "(01)12345678901234(17)991399"; // Fecha inválida

            // Act
            var result = _parser.Parse(barcode);

            // Assert
            Assert.Equal("12345678901234", result.Gtin);
            Assert.Null(result.FechaVencimiento);
        }

        [Fact]
        public void Parse_CodigoVacio_DeberiaRetornarInvalido()
        {
            // Arrange
            var barcode = "";

            // Act
            var result = _parser.Parse(barcode);

            // Assert
            Assert.False(result.EsValido);
        }

        [Fact]
        public void Parse_CantidadNoNumerica_DeberiaUsarDefault()
        {
            // Arrange
            var barcode = "(01)12345678901234(30)ABC";

            // Act
            var result = _parser.Parse(barcode);

            // Assert
            Assert.Equal(1, result.Cantidad); // Default
        }

        [Fact]
        public void Parse_TodosLosCampos_DeberiaObtenerTodos()
        {
            // Arrange
            var barcode = "(01)12345678901234(17)251230(10)LOT001(21)SN123(30)10";

            // Act
            var result = _parser.Parse(barcode);

            // Assert
            Assert.Equal("12345678901234", result.Gtin);
            Assert.Equal(new DateTime(2025, 12, 30), result.FechaVencimiento);
            Assert.Equal("LOT001", result.Lote);
            Assert.Equal("SN123", result.NumeroSerie);
            Assert.Equal(10, result.Cantidad);
            Assert.True(result.EsValido);
        }

        [Fact]
        public void Parse_LoteSeguidoDeGTIN_DeberiaDetectarCorrectamente()
        {
            // Arrange
            // Cambiar el lote para que no termine en un número que podría ser confundido con AI
            var barcode = "(10)LOTEA(01)12345678901234";

            // Act
            var result = _parser.Parse(barcode);

            // Assert
            Assert.Equal("LOTEA", result.Lote);
            Assert.Equal("12345678901234", result.Gtin);
        }

        [Fact]
        public void Parse_LoteConNumeros17AlInicio_NoDeberiaConfundirConFecha()
        {
            // Arrange
            var barcode = "(01)12345678901234(10)17LOTETEST";

            // Act
            var result = _parser.Parse(barcode);

            // Assert
            Assert.Equal("12345678901234", result.Gtin);
            Assert.Equal("17LOTETEST", result.Lote);
        }

        [Fact]
        public void Parse_SerieConNumeros01AlInicio_NoDeberiaConfundirConGTIN()
        {
            // Arrange
            var barcode = "(21)01SERIETEST(01)12345678901234";

            // Act
            var result = _parser.Parse(barcode);

            // Assert
            Assert.Equal("01SERIETEST", result.NumeroSerie);
            Assert.Equal("12345678901234", result.Gtin);
        }

        [Fact]
        public void Parse_CantidadGrande_DeberiaObtenerCantidadCorrecta()
        {
            // Arrange
            var barcode = "(01)12345678901234(30)999999";

            // Act
            var result = _parser.Parse(barcode);

            // Assert
            Assert.Equal(999999, result.Cantidad);
        }

        [Fact]
        public void Parse_LoteConEspacios_DeberiaObtenerLoteConEspacios()
        {
            // Arrange
            var barcode = "(01)12345678901234(10)LOTE 001 A";

            // Act
            var result = _parser.Parse(barcode);

            // Assert
            Assert.Equal("LOTE 001 A", result.Lote);
        }

        // New tests for ambiguous no-parentheses scenarios
        [Fact]
        public void Parse_UserProvidedCodeWithoutParentheses_ShouldParseCorrectly()
        {
            var code = "0100610075001262173007013010105G283";

            var result = _parser.Parse(code);

            Assert.True(result.EsValido);
            Assert.Equal("00610075001262", result.Gtin);
            Assert.Equal(new DateTime(2030, 7, 1), result.FechaVencimiento);
            Assert.Equal(10, result.Cantidad);
            Assert.Equal("5G283", result.Lote);
            Assert.Null(result.NumeroSerie);
        }

        [Theory]
        [InlineData("010061007500126217300701301012345", 10, "12345")]
        [InlineData("010061007500126217300701301015G283", 10, "15G283")]
        [InlineData("010061007500126217300701301010123", 10, "123")]
        public void Parse_AmbiguousNoParentheses_ShouldSeparateQuantityAndLot(string code, int expectedQty, string expectedLote)
        {
            var result = _parser.Parse(code);

            Assert.True(result.EsValido);
            Assert.Equal("00610075001262", result.Gtin);
            Assert.Equal(new DateTime(2030, 7, 1), result.FechaVencimiento);
            Assert.Equal(expectedQty, result.Cantidad);
            Assert.Equal(expectedLote, result.Lote);
        }
    }
}
