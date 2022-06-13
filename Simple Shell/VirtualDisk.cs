namespace simple_Shell
{
    internal class VirtualDisk
    {
        public static FileStream Disk;
        public static void initialize(string path)
        {
            
            if (!File.Exists(path))
            {
                Disk = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                byte[] b = new byte[1024];
                for (int i = 0; i < b.Length; i++)
                    b[i] = 0;
                writeCluster(0,b);
                FAT.prepareFat();
                Directory root = new Directory("M:", 0x10, 5, null);
                root.WriteDirectory();
                Program.current = root;
                FAT.writeFat();
            }
            else
            {

                Disk = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                FAT.readFat();
                Directory root = new Directory("M:", 0x10, 5, null);
                root.ReadDirectory();
                Program.current = root;

            }


        }

        public static byte[] readCluster(int clusterIndex)
        {
            Disk.Seek(clusterIndex * 1024, SeekOrigin.Begin);
            byte[] b = new byte[1024];
            Disk.Read(b, 0,1024);
            return b;
        }

        public static void writeCluster(int clusterIndex, byte[] bytes)
        {
            Disk.Seek(clusterIndex * 1024, SeekOrigin.Begin);
            Disk.Write(bytes, 0, bytes.Length);
            Disk.Flush();
        }

        public static int getFreeSpace()
        {
            return FAT.getAvailableClusters()*1024;
        }

        public static List<byte[]> splitBytes(byte[] bytes)
        {
            List<byte[]> ls = new List<byte[]>();

            if (bytes.Length > 0)
            {
                int number_of_arrays = bytes.Length / 1024;
                int rem = bytes.Length % 1024;

                for (int i = 0; i < number_of_arrays; i++)
                {
                    byte[] b = new byte[1024];
                    for (int j = i * 1024, k = 0; k < 1024; j++, k++)
                    {
                        b[k] = bytes[j];
                    }
                    ls.Add(b);
                }
                if (rem > 0)
                {
                    byte[] b1 = new byte[1024];
                    for (int i = number_of_arrays * 1024, k = 0; k < rem; i++, k++)
                    {
                        b1[k] = bytes[i];
                    }
                    ls.Add(b1);
                }
            }
            else
            {
                byte[] b1 = new byte[1024];
                ls.Add(b1);
            }
            return ls;
        }

       



    }




}
