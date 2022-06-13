namespace simple_Shell
{
    class Parser
    {
        public void parse_input(string str)
        {
            Token token = new Token();
            var argument = str.Split(' ');
            if (argument.Length == 1)
            {
                token.command = argument[0];
                token.value = null;
                token.sec_value = null;
                action(token);
            }
            else if (argument.Length == 2)
            {
                token.command = argument[0];
                token.value = argument[1];
                token.sec_value = null;
                action(token);
            }
            else if (argument.Length == 3)
            {
                token.command=argument[0];
                token.value=argument[1];
                token.sec_value=argument[2];
                action(token); 
            }
        }

        void action(Token token)
        {
            switch (token.command)
            {
                case "cls":
                    Console.Clear();
                    break;
                case "quit":
                    Environment.Exit(0);
                    break;
                case "help":
                    Help helper = new Help(token);
                    break;
                case "cd":
                    if (token.value == null)
                    {
                        return;
                    }
                    else {
                        cd(token.value);
                    }
                    break;

                case "md":
                    if (token.value == null)
                    {
                        Console.WriteLine("ERROR, you shold specify folder name to make\n md[path]name");
                        return;
                    }
                    else
                    {
                        md(token.value);
                    }
                    break;
                case "dir":
                    dir();
                    break;
                case "rd":
                    if (token.value == null)
                    {
                        Console.WriteLine("ERROR,\n you shold specify folder name to delete\n rd[pah]Name");
                    }
                    else
                    {
                        rd(token.value);
                    }
                    break;
                case "import":
                    if (token.value == null)
                    {
                        Console.WriteLine("ERROR\n, you shold specify File name to import\n import [dest]filename");
                    }
                    else
                    {
                        import(token.value);
                    }
                    break;
                case "type":
                    if (token.value == null || token.sec_value!=null)
                    {
                        Console.WriteLine("ERROR\n, you shold specify file name to show its contnet\n type [dest]filename");
                    }
                    else
                    {
                        type(token.value);
                    }
                    break;
                case "export":
                    if (token.value == null || token.sec_value == null)
                    {
                        Console.WriteLine("ERROR,\n");
                        Console.WriteLine("The Correct syntax is \n import   [Source File] [destination]\n");
                    }
                    else
                    {
                        export(token.value,token.sec_value);
                    }
                    break;
                case "rename":
                    if (token.value == null || token.sec_value == null)
                    {
                        Console.WriteLine("ERROR,\n");
                        Console.WriteLine("The Correct syntax is \n rename   [old name] [new name]\n");
                    }
                    else
                    {
                        rename(token.value, token.sec_value);
                    }
                    break;
                case "del":
                    if (token.value == null)
                    {
                        Console.WriteLine("ERROR,\n");
                        Console.WriteLine("The Correct syntax is \n del   [file name]\n");
                    }
                    else
                    {
                        del(token.value);
                    }
                    break;
                case "copy":
                    if (token.value == null || token.sec_value == null)
                    {
                        Console.WriteLine("ERROR,\n");
                        Console.WriteLine("The Correct syntax is \n copy   [Source File] [destination]\n");
                    }
                    else
                    {
                        copy(token.value, token.sec_value);
                    }
                    break;
                default:
                    Console.WriteLine("Unknown Command..");
                    break;

                

            }
        }

        public static void type(string name)
        {
            string[] path = name.Split("\\");
            if (path.Length > 1)
            {
                Directory dir = changeMyCurrentDirectory(name, false, false);
                if (dir == null)
                    Console.WriteLine($"The Path {name} Is not exist");
                else
                {
                    name = path[path.Length - 1];
                    int j = dir.searchDirectory(name);
                    if (j != -1)
                    {
                        int fc = dir.entries[j].firs_cluster;
                        int sz = dir.entries[j].dir_fileSize;
                        string content = null;
                        FILE file = new FILE(name, 0x0, fc, dir, content, sz);
                        file.ReadFile();
                        Console.WriteLine(file.content);
                    }
                    else
                    {
                        Console.WriteLine("The System could not found the file specified");
                    }
                }
            }
            else
            {
                int j = Program.current.searchDirectory(name);
                if (j != -1)
                {
                    int fc = Program.current.entries[j].firs_cluster;
                    int sz = Program.current.entries[j].dir_fileSize;
                    string content = null;
                    FILE file = new FILE(name, 0x0, fc, Program.current, content, sz);
                    file.ReadFile();
                    Console.WriteLine(file.content);
                }
                else
                {
                    Console.WriteLine("The System could not found the file specified");
                }
            }
        }
        public static void cd(string path) 
        {
            Directory dir = changeMyCurrentDirectory(path,true,false);
            if (dir != null)
            {
                dir.ReadDirectory();
                Program.current = dir;
            }
            else
            {
                Console.WriteLine($"Eroor : path {path} is not exists!");
            }
        }
        public static void moveToDirUsedInAnother(string path)
        {
            Directory dir = changeMyCurrentDirectory(path, false, false);
            if (dir != null)
            {
                dir.ReadDirectory();
                Program.current = dir;
            }
            else
            {
                Console.WriteLine("the system cannot find the specified folder.!");
            }
        }

        private static Directory changeMyCurrentDirectory(string p,bool usedInCD,bool isUsedInRD)
        {
            Directory d = null;
            string[] arr = p.Split('\\');
            string path;
            if (arr.Length==1) // cd dirName
            {
                if (arr[0] != "..")
                {
                    int i = Program.current.searchDirectory(arr[0]);
                    if (i == -1)
                        return null;//the directory is not found
                    else
                    {
                        string nameOfDiserableFolder = new string(Program.current.entries[i].dir_name); // we get the name of the directory se seek to move to it
                        byte attr = Program.current.entries[i].dir_attr;//also we get its arrtributes
                        int fisrtcluster = Program.current.entries[i].firs_cluster;
                        d = new Directory(nameOfDiserableFolder, attr, fisrtcluster, Program.current); //we take object of it to read its content and to return it as a current path
                        d.ReadDirectory();
                        path = Program.currentPath; // we take the current path to add to it the new directory
                        path += "\\" + nameOfDiserableFolder.Trim();
                        if(usedInCD)
                            Program.currentPath = path;//here we upadted the path M:>> -> m:/mohamed>>
                    }
                }
                else // .. means the user want to go to the previous folder(parent)
                {
                    if (Program.current.parent != null)// the current folder has a previous folder to back to it
                    {
                        d = Program.current.parent;
                        d.ReadDirectory();
                        path = Program.currentPath;
                        path = path.Substring(0, path.LastIndexOf('\\')); // updating the current path M:/mohamed -> M: 
                        if(usedInCD)
                            Program.currentPath = path;
                    }
                    else // the current folder is the root and there is no previous folder to go to it.
                    {
                        d = Program.current;
                        d.ReadDirectory();
                    }
                }
            }
            else if (arr.Length > 1)//the user enterd a full path to go 
            {
                
                List<string> ListOfHandledPath = new List<string>();
                for (int i = 0; i < arr.Length; i++)
                    if (arr[i] != " ")
                        ListOfHandledPath.Add(arr[i]);



                Directory rootDirectory = new Directory("M:", 0x10, 5, null);
                rootDirectory.ReadDirectory();


                if (ListOfHandledPath[0].Equals("m:") || ListOfHandledPath[0].Equals("M:")) // check if the root folder the user entered is correct.
                {
                    path = "M:";  
                    int howLongIsMyWay;
                    if (isUsedInRD || usedInCD)  //cd m:\mohamed\sayed   | rename m:\mohamed\sayed\mohamed.txt  any.txt
                    {
                        howLongIsMyWay = ListOfHandledPath.Count;
                    }
                    else {
                        howLongIsMyWay = ListOfHandledPath.Count-1;
                    }
                    for (int i = 1; i < howLongIsMyWay; i++) //ListOfHandledPath -> mohamed sayed
                    {
                        int j = rootDirectory.searchDirectory(ListOfHandledPath[i]); // serach for the next folder in the path
                        if (j != -1) // if found
                        {                         
                            Directory tempOfParent = rootDirectory;
                            string newName = new string(rootDirectory.entries[j].dir_name);// we get the name of the directory se seek to move to it
                            byte attr = rootDirectory.entries[j].dir_attr;//also we get its arrtributes
                            int fc = rootDirectory.entries[j].firs_cluster;
                            rootDirectory = new Directory(newName, attr, fc, tempOfParent);
                            rootDirectory.ReadDirectory();
                            path += "\\" + newName.Trim();
                        }
                        else//not found
                        {
                            return null;
                        }
                    }
                    d = rootDirectory;
                    if(usedInCD)
                        Program.currentPath = path;
                }
                else if (ListOfHandledPath[0] == "..")//want to go back
                {
                    d = Program.current;
                    for (int i = 0; i < ListOfHandledPath.Count; i++)
                    {
                        if (d.parent != null)
                        {
                            d = d.parent;
                            d.ReadDirectory();
                            path = Program.currentPath;
                            path = path.Substring(0, path.LastIndexOf('\\'));
                            if(usedInCD)
                                Program.currentPath = path;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                    return null;
            }
            return d;
        }


        public static void md(string name)
        {
            string[] arr = name.Split('\\');
            if (arr.Length == 1) // md [folderName]
            {
                if (Program.current.searchDirectory(arr[0]) == -1)// there is no folder with the name user entered
                {
                    DirectoryEntry d = new DirectoryEntry(arr[0], 0x10, 0, 0);

                    if (FAT.GetEmptyCulster() != -1)//there is empty clusters (free space) to make a new folder
                    {
                        Program.current.entries.Add(d);
                        Program.current.WriteDirectory();
                        if (Program.current.parent != null)
                        {
                            Program.current.parent.updateContent(Program.current.getDirectoryEntry());
                            Program.current.parent.WriteDirectory();
                        }
                        FAT.writeFat();
                    }
                    else
                        Console.WriteLine("The Disk is Full :(");
                }
                else
                    Console.WriteLine($"{arr[0]} is aready existed :(");
            }
            else if (arr.Length > 1)// md [fullPath] 
            {
                Directory dir = changeMyCurrentDirectory(name,false,false); // md m:\mohamed\sayed
                if (dir == null)
                    Console.WriteLine($"The Path {name} Is not exist");
                else
                {
                    if (FAT.GetEmptyCulster() != -1)//not full
                    {

                        DirectoryEntry d = new DirectoryEntry(arr[arr.Length - 1], 0x10, 0,0); //making the new folder
                        dir.entries.Add(d);// add it to the the folder we want to create into it a new folder
                        dir.WriteDirectory();
                        dir.parent.updateContent(dir.getDirectoryEntry());
                        dir.parent.WriteDirectory();
                        FAT.writeFat();
                    }
                    else
                        Console.WriteLine("The Disk is Full :(");
                }
            }

        }
        public static void dir()
        {
            int fc = 0,dc = 0,fz_sum = 0;
            Console.WriteLine("Directory of " + Program.currentPath);
            Console.WriteLine();
            for (int i = 0; i < Program.current.entries.Count; i++)
            {
                if (Program.current.entries[i].dir_attr == 0x0)
                {
                    Console.WriteLine($"\t{Program.current.entries[i].dir_fileSize} \t {new string(Program.current.entries[i].dir_name)}");
                    fc++;
                    fz_sum += Program.current.entries[i].dir_fileSize;
                }
                else if (Program.current.entries[i].dir_attr == 0x10)
                {
                    Console.WriteLine($"\t<DIR> {new string(Program.current.entries[i].dir_name)}");
                    dc++;
                }
            }
            Console.WriteLine($"{"\t\t"}{fc} File(s)    {fz_sum} bytes");
            Console.WriteLine($"{"\t\t"}{dc} Dir(s)    {VirtualDisk.getFreeSpace()} bytes free");
        }


        public static void rd(string name) 
        {
      

            string[] arr = name.Split('\\');
            Directory dir = changeMyCurrentDirectory(name,false,true);
            if (dir != null)
            {
                 Console.Write($"Are you sure that you want to delete {new string(dir.dir_name).Trim()} , please enter Y for yes or N for no:");
                 string choice = Console.ReadLine().ToLower();
                 if (choice.Equals("y"))
                    dir.deleteDirectory();
            }
            else
               Console.WriteLine($"directory \" {arr[arr.Length-1]} \" is not exists!");

        }
      
        public static void import(string dest)
        {
            if (File.Exists(dest))
            {
                string content = File.ReadAllText(dest);
                int size = content.Length;
                string[] names = dest.Split("\\"); // import E:\mohamed.txt
                string name = names[names.Length-1];
                int j = Program.current.searchDirectory(name);
                if (j == -1)
                {
                    int fc;
                    if (size > 0)
                    {
                        fc = FAT.GetEmptyCulster();
                    }
                    else
                    {
                        fc = 0;
                    }
                    FILE newFile = new FILE(name, 0X0, fc, Program.current, content, size);

                    newFile.writeFile();

                    DirectoryEntry d = new DirectoryEntry(new string(name), 0X0, fc, size);
                    Program.current.entries.Add(d);
                    Program.current.WriteDirectory();
                }
                else {
                    Console.WriteLine($"{name} is already exist in your virtual disk");
                }

            }
            else {
                Console.WriteLine("The file you specified does not exist in your compuret");
            }
        }




        public static void export(string source, string dest)
        {
            string[] path = source.Split("\\");
            if (path.Length > 1)
            {
                Directory dir = changeMyCurrentDirectory(source, false, false);
                if (dir == null)
                    Console.WriteLine($"The Path {source} Is not exist");
                else
                {
                    source = path[path.Length - 1];

                    int j = dir.searchDirectory(source);
                    if (j != -1)
                    {
                        if (System.IO.Directory.Exists(dest))
                        {
                            int fc = dir.entries[j].firs_cluster;
                            int sz = dir.entries[j].dir_fileSize;
                            string content = null;
                            FILE file = new FILE(source, 0x0, fc, dir, content, sz);
                            file.ReadFile();
                            StreamWriter sw = new StreamWriter(dest + "\\" + source);
                            sw.Write(file.content);
                            sw.Flush();
                            sw.Close();
                        }
                        else
                        {
                            Console.WriteLine("The system cannot find the path specified in hte coputer dis");
                        }

                    }
                    else
                    {
                        Console.WriteLine("The system cannot find the file you want to export in the virtual disk");
                    }
                }

            }
            else
            {
                int j = Program.current.searchDirectory(source);
                if (j != -1)
                {
                    if (System.IO.Directory.Exists(dest))
                    {
                        int fc = Program.current.entries[j].firs_cluster;
                        int sz = Program.current.entries[j].dir_fileSize;
                        string content = null;
                        FILE file = new FILE(source, 0x0, fc, Program.current, content, sz);
                        file.ReadFile();
                        StreamWriter sw = new StreamWriter(dest + "\\" + source);
                        sw.Write(file.content);
                        sw.Flush();
                        sw.Close();
                    }
                    else
                    {
                        Console.WriteLine("The system cannot find the path specified in hte coputer dis");
                    }

                }
                else
                {
                    Console.WriteLine("The system cannot find the file you want to export in the virtual disk");
                }
            }                   
        }

        public static void rename(string oldName, string newName)
        {
            string[] path = oldName.Split("\\"); //old name could be path
            if (path.Length > 1)
            {
                Directory dir = changeMyCurrentDirectory(oldName, false, false);
                if (dir == null)
                    Console.WriteLine($"The Path {oldName} Is not exist");
                else
                {
                    oldName = path[path.Length - 1];

                    int j = dir.searchDirectory(oldName);
                    if (j != -1)
                    {
                        if (dir.searchDirectory(newName) == -1)
                        {
                            DirectoryEntry d = dir.entries[j];

                            if (d.dir_attr == 0x0)
                            {
                                string[] fileName = newName.Split('.');
                                char[] goodName = getProperFileName(fileName[0].ToCharArray(), fileName[1].ToCharArray());
                                d.dir_name = goodName;
                            }
                            else if (d.dir_attr == 0x10)
                            {
                                char[] goodName = getProperDirName(newName.ToCharArray());
                                d.dir_name = goodName;
                            }

                            // dir.updateContent(j,d);

                            dir.entries.RemoveAt(j);
                            dir.entries.Insert(j, d);
                            dir.WriteDirectory(); 
                        }
                        else
                        {
                            Console.WriteLine("Doublicate File Name exist or file cannot be found");
                        }
                    }
                    else
                    {
                        Console.WriteLine("The System Cannot Find the File specified");
                    }

                }
            }
            else
            {
                int j = Program.current.searchDirectory(oldName);
                if (j != -1)
                {
                    if (Program.current.searchDirectory(newName) == -1)
                    {
                        DirectoryEntry d = Program.current.entries[j];



                        if (d.dir_attr == 0x0)
                        {
                            string[] fileName = newName.Split('.');
                            char[] goodName = getProperFileName(fileName[0].ToCharArray(), fileName[1].ToCharArray());
                            d.dir_name = goodName;
                        }
                        else if (d.dir_attr == 0x10)
                        {
                            char[] goodName = getProperDirName(newName.ToCharArray());
                            d.dir_name = goodName;
                        }

                        // dir.updateContent(j,d);
                        Program.current.entries.RemoveAt(j);
                        Program.current.entries.Insert(j, d);
                        Program.current.WriteDirectory();
                    }
                    else
                    {
                        Console.WriteLine("Doublicate File Name exist or file cannot be found");
                    }
                }
                else
                {
                    Console.WriteLine("The System Cannot Find the File specified");
                }
            }

        }

        public static void del(string fileName)
        {
            string[] path = fileName.Split("\\");
            if (path.Length > 1)
            {
                Directory dir = changeMyCurrentDirectory(fileName, false, false);
                if (dir == null)
                    Console.WriteLine($"The Path {fileName} Is not exist");
                else
                {
                    fileName = path[path.Length - 1];

                    int j = dir.searchDirectory(fileName);
                    if (j != -1)
                    {
                        if (dir.entries[j].dir_attr == 0x0)
                        {
                            int fc = dir.entries[j].firs_cluster;
                            int sz = dir.entries[j].dir_fileSize;

                            FILE file = new FILE(fileName, 0x0, fc, dir, null, sz);
                            file.deleteFile();

                        }
                        else
                        {
                            Console.WriteLine($"The System Cannot Find The file specified {fileName}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("The System Cannot Find The file specified");
                    }
                }        
            }
            else 
            {
                int j = Program.current.searchDirectory(fileName);
                if (j != -1)
                {
                    if (Program.current.entries[j].dir_attr == 0x0)
                    {
                        int fc = Program.current.entries[j].firs_cluster;
                        int sz = Program.current.entries[j].dir_fileSize;

                        FILE file = new FILE(fileName, 0x0, fc, Program.current, null, sz);
                        file.deleteFile();

                    }
                    else
                    {
                        Console.WriteLine("The System Cannot Find The file specified");
                    }
                }
                else
                {
                    Console.WriteLine("The System Cannot Find The file specified");
                }
            }

        }

        public static void copy(string source, string dest)
        {
            int j = Program.current.searchDirectory(source);
            int fc;
            int sz;

            if (source == dest)
            {
                Console.WriteLine("the file cannot be copied onto itself");
                return;
            }


            if (j != -1)
            {
                fc = FAT.GetEmptyCulster();
                sz =Program.current.entries[j].dir_fileSize;
                //string[] myWay = dest.Split("\\");

                Directory dir = changeMyCurrentDirectory(dest, false, true);
                if (dir == null)
                {
                    Console.WriteLine($"The Path {source} Is not exist");
                    return;
                }

                int x = dir.searchDirectory(source);
                if (x != -1)
                {
                    Console.Write("The File is aleary existed, Do you want to overwrite it ?, please enter Y for yes or N for no:");
                    string choice = Console.ReadLine().ToLower();
                    if (choice.Equals("y"))
                    {

                        // extracting the content from the original file
                        int f = Program.current.entries[j].firs_cluster;                       
                        string content = null;
                        FILE file = new FILE(source, 0x0, f, dir, content, sz);
                        file.ReadFile();
                        content = file.content; 


                        // making the copy file 
                        FILE newFile = new FILE(source, 0X0, fc, dir, content, sz);
                        newFile.writeFile();

                        DirectoryEntry d = new DirectoryEntry(new string(source), 0X0, fc, sz);
                        dir.entries.Add(d);
                        dir.WriteDirectory();

                    }
                    else
                    {
                        return;
                    }
                }
                else
                {

                    // extracting the content from the original file
                    int f = Program.current.entries[j].firs_cluster;
                    string content = null;
                    FILE file = new FILE(source, 0x0, f, dir, content, sz);
                    file.ReadFile();
                    content = file.content;


                    // making the copy file 
                    FILE newFile = new FILE(source, 0X0, fc, dir, content, sz);
                    newFile.writeFile();

                    DirectoryEntry d = new DirectoryEntry(new string(source), 0X0, fc, sz);
                    dir.entries.Add(d);
                    dir.WriteDirectory();

                }
            }
            else
            {
                Console.WriteLine($"The File ${source} Is Not Existed In your disk");
            }
        }//copy function

        public static char[] getProperFileName(char[] fname, char[] extension)// asd1   .   txt
        {
            char[] dir_name = new char[11];

            int length = fname.Length, count = 0, lenOfEx = extension.Length;
            if (fname.Length >= 7)
            {
                for (int i = 0; i < 7; i++)
                {
                    dir_name[count] = fname[i];
                    count++;
                }
                dir_name[count] = '.';
                count++;

            }
            else if (length < 7)
            {
                for (int i = 0; i < length; i++)
                {
                    dir_name[count] = fname[i];
                    count++;
                }
                for (int i = 0; i < 7 - length; i++)
                {
                    dir_name[count] = '_';
                    count++;
                }
                dir_name[count] = '.';
                count++;
            }
            for (int i = 0; i < lenOfEx; i++)
            {
                dir_name[count] = extension[i];
                count++;
            }
            for (int i = 0; i < 3 - lenOfEx; i++)
            {
                dir_name[count] = ' ';
                count++;
            }
            return dir_name;
        }

        public static char[] getProperDirName(char[] name)
        {
            char[] dir_name = new char[11];

            if (name.Length <= 11)
            {
                int j = 0;
                for (int i = 0; i < name.Length; i++)
                {
                    j++;
                    dir_name[i] = name[i];
                }
                for (int i = ++j; i < dir_name.Length; i++)
                {
                    dir_name[i] = ' ';
                }
            }
            else
            {
                int j = 0;
                for (int i = 0; i < 11; i++)
                {
                    j++;
                    dir_name[i] = name[i];
                }
            }
            return dir_name;
        }


    }

}
