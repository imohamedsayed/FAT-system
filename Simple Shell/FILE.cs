using System.Text;
namespace simple_Shell
{
   class FILE : DirectoryEntry
    {
        public Directory parent;
        public string content;

        public FILE(string name, byte attr, int fcluster,Directory parent,string content,int filesize) : base(name, attr, fcluster,filesize)
        {
            this.content = content;
            if(parent !=null)
                this.parent = parent;
        }


        public DirectoryEntry GetDirectory_Entry()
        {
            DirectoryEntry me = new DirectoryEntry(new string(this.dir_name), this.dir_attr, this.firs_cluster,this.dir_fileSize);
            return me;
        }

        /*public struct dirCluster
        {
            public dirCluster()
            { }

            public byte[] b = new byte[1024];
        }*/



        public void writeFile() 
        {
            byte[] byteContent = ConvertContentToBytes(content);
            List<byte[]> listOfArrayOfBytes = VirtualDisk.splitBytes(byteContent);

            int clusterFATIndex;
            if (this.firs_cluster != 0)
            {
                clusterFATIndex = this.firs_cluster;
            }
            else
            {
                clusterFATIndex = FAT.GetEmptyCulster();
                this.firs_cluster = clusterFATIndex;
            }
            int lastCluster = -1;
            for (int i = 0; i < listOfArrayOfBytes.Count; i++)
            {
                if (clusterFATIndex != -1)
                {
                    VirtualDisk.writeCluster(clusterFATIndex,listOfArrayOfBytes[i]);
                    FAT.setClusterNext(clusterFATIndex, -1);
                    if (lastCluster != -1)
                        FAT.setClusterNext(lastCluster, clusterFATIndex);
                    lastCluster = clusterFATIndex;
                    clusterFATIndex = FAT.GetEmptyCulster();
                }
            }
        }

        public void ReadFile()
        {
            if (this.firs_cluster != 0)
            {
                content = string.Empty;
                int cluster = this.firs_cluster;
                int next = FAT.getClusterNext(cluster);
                List<byte> ls = new List<byte>();
                do
                {
                    ls.AddRange(VirtualDisk.readCluster(cluster));
                    cluster = next;
                    if (cluster != -1)
                        next = FAT.getClusterNext(cluster);
                }
                while (next != -1);
                content = ConvertBytesToContent(ls.ToArray());

                //ASCIIEncoding encoding = new ASCIIEncoding();
                //content = encoding.GetString(ls.ToArray());

            }
 
        }

        public void deleteFile()
        {
            if (this.firs_cluster != 0)
            {
                clearFileSize();
            }
            if (this.parent != null)
            {
                string dirName = new string(dir_name);
                int index = this.parent.searchDirectory(dirName);

                if(index != -1)
                {
                    this.parent.entries.RemoveAt(index);
                    this.parent.WriteDirectory();
                    FAT.writeFat();       
                }
            }
        }



        public void clearFileSize()
        {
            int clusterIndex = this.firs_cluster;

            int next = FAT.getClusterNext(clusterIndex);

            if (clusterIndex == 5 && next == 0)
                return;

            do
            {
                FAT.setClusterNext(clusterIndex, 0);
                clusterIndex = next;
                if (clusterIndex != -1)
                    next = FAT.getClusterNext(clusterIndex);

            } while (clusterIndex != -1);

        }

        public static byte[] ConvertContentToBytes(string Con)
        {
            byte[] contentBytes = new byte[Con.Length];
            for (int i = 0; i < Con.Length; i++)
            {
                contentBytes[i] = (byte)Con[i];
            }
            return contentBytes;
        }// this function will iterate in whole content and take each character and convert it into byte format.
        public static string ConvertBytesToContent(byte[] bytes)
        {
            string con = string.Empty;
            for (int i = 0; i < bytes.Length; i++)
            {
                if ((char)bytes[i] != '\0')
                    con += (char)bytes[i];
                else
                    break;
            }
            return con;
        }
    }
}
