namespace simple_Shell
{
    public struct Token
    {
        public string command;
        public string value;
        public string sec_value;
    }

    class Program
    {
        /*
         
            UPDATE THE PATH_ON_PC BERFOR RUNNING THE CODE !!!!

        */
        
        public static string PATH_ON_PC = "C:\\Users\\Dell\\Desktop\\My Projects\\Simple Shell\\Simple Shell\\miniFat.txt"; 
        public static Directory current;
        public static string currentPath;
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome, This Shell Is Developed By : ENG. Mohamed Sayed Osman");
            Console.WriteLine("Gmail : mohamedelsayed0901@gmail.com\n\n");
            VirtualDisk.initialize(PATH_ON_PC);


            currentPath = new string(current.dir_name);
            currentPath = currentPath.Trim();


            Parser parser = new Parser();



            while (true)
            {
                var currentLocation = currentPath;
                Console.Write(currentLocation + "\\>");
                current.ReadDirectory();

                string input = Console.ReadLine();
                parser.parse_input(input);
            }
        }
    }
}
