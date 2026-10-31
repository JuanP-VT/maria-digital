namespace MariaDigital.LocalScanner;

internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            var options = OllamaOptions.Resolve(args, Environment.GetEnvironmentVariable);
            Console.WriteLine($"Using {options.Model} at {options.BaseUrl}");
            return 0;
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
