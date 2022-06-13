namespace simple_Shell
{
    internal class FAT
    {
        static int[]  fat = new int[1024];
        public static void prepareFat()
        {

            for (int i = 0; i < fat.Length; i++)
            {
                if (i == 0 || i == 4)
                {
                    fat[i] = -1;
                }
                else if (i > 0 && i <= 3)
                {
                    fat[i] = i + 1;
                }
                else
                {
                    fat[i] = 0;
                }
            }
        }//prepareFat

      

        public static void writeFat()
        {
            byte[] bytes = new byte[fat.Length * sizeof(int)]; 

            Buffer.BlockCopy(fat, 0, bytes, 0, bytes.Length);

            List<byte[]> ls = VirtualDisk.splitBytes(bytes);

            for (int i = 0; i < ls.Count; i++)
            {
                VirtualDisk.writeCluster(i + 1, ls[i]);
            }

        }//writeFat



        public static void readFat()
        {
            byte[] buffer = new byte[4096];
            List<byte> ls = new List<byte>();

            for (int i = 0; i <4 ; i++)
            {
                ls.AddRange(VirtualDisk.readCluster(i + 1));
            }

            byte[] bytes = ls.ToArray();
            int[] fatAsIntegers = new int[bytes.Length / sizeof(int)];

            Buffer.BlockCopy(bytes, 0, fatAsIntegers, 0, bytes.Length);

            fat = fatAsIntegers;

        }//readFat


        private static void sotreInBuffer(byte[] buffer, int offset, byte[] b)
        {
            int c = 0;
            for (int i = offset; i < offset + 1024; i++)
            {
                buffer[i] = b[c];
                c++;
            }
        }//StoreInBuffer

        private static void copyBuffer(byte[] src, byte[] dest, int srcOffset, int srcEnd)
        {
            int c = 0;
            for (int i = srcOffset; i < srcEnd; i++)
            {
                dest[c] = src[i];
                c++;
            }
        }//CopyBuffer

        public static int GetEmptyCulster()
        {
            for(int i = 0; i < fat.Length; i++)
            {
                if(fat[i] == 0)
                {
                    return i;
                }
            }
            return -1;
        }


        public static int getClusterNext(int index) {
            if (index >= 0 && index < fat.Length)
                return fat[index];
            return -1;
        }

        public static void setClusterNext(int index, int next)
        {
            fat[index] = next;
        }

        public static int getAvailableClusters()
        {
            int count = 0;
            for (int i = 0; i < fat.Length; i++)
            {
                if (fat[i] == 0)
                    count++;
            }
            return count;
        }

    }//Fat Calss
}
