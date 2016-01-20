namespace DbAdvance.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var runner = new DbAdvanceRunner())
            {
                runner.Run(args);
            }
        }
    }
}