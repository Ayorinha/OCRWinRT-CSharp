using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Globalization;

namespace OcrWinRT
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Uso: OcrWinRT.exe <caminho-da-imagem> [arquivo-de-saida]");
                return;
            }

            string inputPath = Path.GetFullPath(args[0]);
            string outputPath = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(Environment.CurrentDirectory, "saida.txt");

            if (!File.Exists(inputPath))
            {
                Console.WriteLine($"Arquivo não encontrado: {inputPath}");
                return;
            }

            try
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(inputPath);

                using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                    SoftwareBitmap bitmap = await decoder.GetSoftwareBitmapAsync();

                    if (bitmap == null)
                    {
                        Console.WriteLine("Erro: não foi possível carregar a imagem.");
                        return;
                    }

                    var language = new Language("pt-BR");
                    OcrEngine engine = OcrEngine.TryCreateFromLanguage(language);

                    if (engine == null)
                    {
                        Console.WriteLine("Erro: o mecanismo OCR pt-BR não está disponível neste sistema.");
                        return;
                    }

                    OcrResult result = await engine.RecognizeAsync(bitmap);
                    SaveTextToFile(result.Text, outputPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro durante o OCR: {ex.Message}");
            }
        }

        private static void SaveTextToFile(string text, string filePath)
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, text);
            Console.WriteLine($"Texto salvo com sucesso em: {filePath}");
        }
    }
}
