namespace CigralBackend.Application.Services.Interfaces
{
    public interface IBarCodeParser
    {
        BarCodeParsed Parse(string scannedCode);
    }
}