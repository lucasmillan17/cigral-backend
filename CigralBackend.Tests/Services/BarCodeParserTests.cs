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
        public void Parse_RealScannerFormat_ShouldParseProperly()
        {
            // Arrange - Real scanner format from user
            var code = "0110610075099396172801113051$103A061";

            // Act
            var result = _parser.Parse(code);

            // Assert
            Assert.True(result.EsValido);
            Assert.Equal("10610075099396", result.Gtin);
            Assert.Equal(new DateTime(2028, 1, 11), result.FechaVencimiento);
            Assert.Equal(5, result.Cantidad);
            Assert.Equal("3A061", result.Lote);
            Assert.Null(result.NumeroSerie);
        }

        [Fact]
        public void Parse_OnlyGTIN_ShouldParseCorrectly()
        {
            // Arrange
            var code = "0112345678901234";

            // Act
            var result = _parser.Parse(code);

            // Assert
            Assert.True(result.EsValido);
            Assert.Equal("12345678901234", result.Gtin);
            Assert.Null(result.Lote);
            Assert.Null(result.NumeroSerie);
            Assert.Null(result.FechaVencimiento);
            Assert.Equal(1, result.Cantidad); // Default
        }

        [Fact]
        public void Parse_GTINAndDate_ShouldParseCorrectly()
        {
            // Arrange
            var code = "01123456789012341730630";

            // Act
            var result = _parser.Parse(code);

            // Assert
            Assert.True(result.EsValido);
            Assert.Equal("12345678901234", result.Gtin);
            Assert.Equal(new DateTime(2017, 6, 30), result.FechaVencimiento);
            Assert.Null(result.Lote);
            Assert.Null(result.NumeroSerie);
            Assert.Equal(1, result.Cantidad);
        }

        [Fact]
        public void Parse_GTINDateAndQuantity_ShouldParseCorrectly()
        {
            // Arrange
            var code = "01123456789012341730630305$";

            // Act
            var result = _parser.Parse(code);

            // Assert
            Assert.True(result.EsValido);
            Assert.Equal("12345678901234", result.Gtin);
            Assert.Equal(new DateTime(2017, 6, 30), result.FechaVencimiento);
            Assert.Equal(5, result.Cantidad);
            Assert.Null(result.Lote);
            Assert.Null(result.NumeroSerie);
        }

        [Fact]
        public void Parse_AllFieldsWithLot_ShouldParseCorrectly()
        {
            // Arrange
            var code = "01123456789012341730630305$10LOTE123";

            // Act
            var result = _parser.Parse(code);

            // Assert
            Assert.True(result.EsValido);
            Assert.Equal("12345678901234", result.Gtin);
            Assert.Equal(new DateTime(2017, 6, 30), result.FechaVencimiento);
            Assert.Equal(5, result.Cantidad);
            Assert.Equal("LOTE123", result.Lote);
            Assert.Null(result.NumeroSerie);
        }

        [Fact]
        public void Parse_AllFieldsWithSerial_ShouldParseCorrectly()
        {
            // Arrange
            var code = "01123456789012341730630305$10LOTE123$21SN001";

            // Act
            var result = _parser.Parse(code);

            // Assert
            Assert.True(result.EsValido);
            Assert.Equal("12345678901234", result.Gtin);
            Assert.Equal(new DateTime(2017, 6, 30), result.FechaVencimiento);
            Assert.Equal(5, result.Cantidad);
            Assert.Equal("LOTE123", result.Lote);
            Assert.Equal("SN001", result.NumeroSerie);
        }

        [Fact]
        public void Parse_LotWithNumbers_ShouldParseCorrectly()
        {
            // Arrange
            var code = "0112345678901234$10LOTE10ABC";

            // Act
            var result = _parser.Parse(code);

            // Assert
            Assert.True(result.EsValido);
            Assert.Equal("12345678901234", result.Gtin);
            Assert.Equal("LOTE10ABC", result.Lote);
        }

        [Fact]
        public void Parse_SerialWithNumbers_ShouldParseCorrectly()
        {
            // Arrange
            var code = "0111111111111111$21SN123456789";

            // Act
            var result = _parser.Parse(code);

            // Assert
            Assert.True(result.EsValido);
            Assert.Equal("11111111111111", result.Gtin);
            Assert.Equal("SN123456789", result.NumeroSerie);
        }

        [Fact]
        public void Parse_LargQuantity_ShouldParseCorrectly()
        {
            // Arrange
            var code = "0112345678901234$30999999$10LOT";

            // Act
            var result = _parser.Parse(code);

            // Assert
            Assert.True(result.EsValido);
            Assert.Equal("12345678901234", result.Gtin);
            Assert.Equal(999999, result.Cantidad);
            Assert.Equal("LOT", result.Lote);
        }

        [Fact]
        public void Parse_InvalidDate_ShouldIgnoreDateField()
        {
            // Arrange
            var code = "0112345678901234$179999991$10LOTE";

            // Act
            var result = _parser.Parse(code);

            // Assert
            Assert.True(result.EsValido);
            Assert.Equal("12345678901234", result.Gtin);
            Assert.Null(result.FechaVencimiento);
            Assert.Equal("LOTE", result.Lote);
        }

        [Fact]
        public void Parse_IncompleteGTIN_ShouldReturnInvalid()
        {
            // Arrange
            var code = "011234567890123"; // Only 13 digits

            // Act
            var result = _parser.Parse(code);

            // Assert
            Assert.False(result.EsValido);
            Assert.Null(result.Gtin);
        }

        [Fact]
        public void Parse_EmptyCode_ShouldReturnInvalid()
        {
            // Arrange
            var code = "";

            // Act
            var result = _parser.Parse(code);

            // Assert
            Assert.False(result.EsValido);
        }

        [Fact]
        public void Parse_OnlyAI_ShouldNotParse()
        {
            // Arrange
            var code = "01";

            // Act
            var result = _parser.Parse(code);

            // Assert
            Assert.False(result.EsValido);
        }

        [Fact]
        public void Parse_DifferentAIOrder_ShouldStillWork()
        {
            // Arrange
            var code = "30501012345$0112345678901234";

            // Act
            var result = _parser.Parse(code);

            // Assert
            Assert.True(result.EsValido);
            Assert.Equal("12345678901234", result.Gtin);
            Assert.Equal("12345", result.Lote);
            Assert.Equal(5, result.Cantidad);
        }

        [Fact]
        public void Parse_QuantityAsFirstVariable_ShouldParseCorrectly()
        {
            // Arrange
            var code = "0112345678901234305$10LOTE$21SN";

            // Act
            var result = _parser.Parse(code);

            // Assert
            Assert.True(result.EsValido);
            Assert.Equal("12345678901234", result.Gtin);
            Assert.Equal(5, result.Cantidad);
            Assert.Equal("LOTE", result.Lote);
            Assert.Equal("SN", result.NumeroSerie);
        }

        [Fact]
        public void Parse_LotWithSpecialChars_ShouldPreserveChars()
        {
            // Arrange
            var code = "0112345678901234$10ABC-123_XYZ";

            // Act
            var result = _parser.Parse(code);

            // Assert
            Assert.True(result.EsValido);
            Assert.Equal("ABC-123_XYZ", result.Lote);
        }

        [Fact]
        public void Parse_SerialWithSpecialChars_ShouldPreserveChars()
        {
            // Arrange
            var code = "0112345678901234$21SN-2024/001";

            // Act
            var result = _parser.Parse(code);

            // Assert
            Assert.True(result.EsValido);
            Assert.Equal("SN-2024/001", result.NumeroSerie);
        }
    }
}
