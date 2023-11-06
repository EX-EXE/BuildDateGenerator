using BuildDateGenerator;

namespace SampleApp
{
    [GenerateBuildDate]
    internal partial class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"{nameof(Program)} : {BuildDate.ToString("o")}");
        }
    }
}