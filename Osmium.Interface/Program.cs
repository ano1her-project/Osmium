namespace Osmium.Interface
{
    internal class Program
    {
        static void Main()
        {
            WaitForCommandAndReact();
        }

        static void WaitForCommandAndReact()
        {
            string input = Console.ReadLine();
            switch (input)
            {
                default:
                    WaitForCommandAndReact();
                    break;
            }
        }
    }
}
