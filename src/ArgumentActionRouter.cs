namespace DirectoryWatchDog
{
    using System;
    using System.Threading.Tasks;

    public static class ArgumentActionRouter
    {
        public static void RunAsync(params (string option, Action action)[] actionRoutes)
        {
            "Please enter the number of the targeted action:".Print();

            Console.WriteLine();

            for (int i = 0; i < actionRoutes.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {actionRoutes[i].option}");
            }

            Console.WriteLine();

            var dataNumber = Console.ReadLine();

            Console.WriteLine();

            if (int.TryParse(dataNumber, out int targetNumber) &&
                actionRoutes.Length >= targetNumber &&
                actionRoutes[targetNumber - 1].action != null)
            {
                actionRoutes[targetNumber - 1].action();
            }
            else
            {
                "Invalid target number!".Print();
            }

            Console.WriteLine();
            "================================================".Print(ConsoleColor.Blue);
            Console.WriteLine();
        }

        internal static void RunAsync(object p1, object p2, object p3, object p4, object p5, object p6, (string, object) p7, object p8)
        {
            throw new NotImplementedException();
        }
    }
}
