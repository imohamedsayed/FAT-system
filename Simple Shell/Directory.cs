using System.Text;

namespace simple_Shell
{
    class Directory : DirectoryEntry
    {
        public Directory parent;
        public List<DirectoryEntry> entries;
        public Directory(string name, byte attr, int fcluster,Directory parent,int filesize=0) : base(name, attr, fcluster,filesize)
        {
            entries = new List<DirectoryEntry>();

            if (parent != null) { 
                this.parent = parent;
            }
        }
        public void updateContent(DirectoryEntry d)
        {
            ReadDirectory();
            string dName = new string(d.dir_name);
            int index = searchDirectory(dName);
            if (index != -1) {
                entries.RemoveAt(index);
                entries.Insert(index, d);
            }
        }

        public DirectoryEntry getDirectoryEntry()
        {
            DirectoryEntry me = new DirectoryEntry(new string(this.dir_name), this.dir_attr, this.firs_cluster,this.dir_fileSize);
            return me;
        }

        public int searchDirectory(string name) 
        {
            ReadDirectory();
            if (name.Length < 11)
            {
                name += "\0";
                for (int i = name.Length + 1; i < 12; i++)
                    name += " ";
            }
            else
            {
                name = name.Substring(0, 11);
            }
            for (int i = 0; i < entries.Count; i++) 
            {
                string Dname = new string(entries[i].dir_name);

                if (Dname.Equals(name))
                    return i;
            }
            return -1;
        }

        public void addEntry(DirectoryEntry d)
        {
            entries.Add(d);
            WriteDirectory();
        }
        public void removeEntry(DirectoryEntry d)
        {
            ReadDirectory();
            string searchName = new string(d.dir_name);
            int index = searchDirectory(searchName);
            entries.RemoveAt(index);
            WriteDirectory();
        }
        public void updateContent(DirectoryEntry old, DirectoryEntry neW)
        {
            ReadDirectory();
            string oldName = new string(old.dir_name);
            int index = searchDirectory(oldName);
            if (index != -1)
            {
                entries.RemoveAt(index);
                entries.Add(neW);
            }

        }

        public void ReadDirectory()
        {
            if (this.firs_cluster != 0)
            {
                entries = new List<DirectoryEntry>();
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
                for (int i = 0; i < ls.Count; i++)
                {
                    byte[] b = new byte[32];
                    for (int k = i * 32, m = 0; m < b.Length && k < ls.Count; m++, k++)
                    {
                        b[m] = ls[k];
                    }
                    if (b[0] == 0)
                        break;
                    entries.Add(makeDirectory(b));
                }
            }
        }


       
        public void sliceRange(byte[] b_arr, List<byte> src_list)
        {
            for (int i = 0; i < 32; i++)
            {
                b_arr[i] = src_list[i];
            }
        }


        public DirectoryEntry makeDirectory(byte[] b)
        {
            char[] name = new char[11];
            for (int i = 0; i < name.Length; i++)
            {
                name[i] = (char)b[i];
            }

            byte attr = b[11];

 
            byte[] empty = new byte[12];

            int j = 12;

            for (int i = 0; i < empty.Length; i++)
            {
                empty[i] = b[i];
                j++;

            }
            byte[] firstClusteByte = new byte[4];

            for (int i = 0; i < firstClusteByte.Length; i++)
            {
                firstClusteByte[i] = b[j];
                j++;
            }
            int firstcluster = BitConverter.ToInt32(firstClusteByte, 0);

            byte[] sizeByte = new byte[4];
            for (int i = 0; i < sizeByte.Length; i++)
            {
                sizeByte[i] = b[j];
                j++;
            }
            int filesize = BitConverter.ToInt32(sizeByte, 0);

            DirectoryEntry newD = new DirectoryEntry(new string(name), attr, firstcluster,dir_fileSize);

            newD.dir_empty    = empty;
            newD.dir_fileSize = filesize;

            return newD;

        }
        public void WriteDirectory()
        {
            byte[] dirsorfilesBYTES = new byte[entries.Count * 32];
            for (int i = 0; i < entries.Count; i++)
            {
                byte[] b = DirToByte(this.entries[i]);

                for (int j = i * 32, k = 0; k < b.Length; k++, j++)
                    dirsorfilesBYTES[j] = b[k];
            }
            List<byte[]> bytesls = VirtualDisk.splitBytes(dirsorfilesBYTES);
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
            for (int i = 0; i < bytesls.Count; i++)
            {
                if (clusterFATIndex != -1)
                {
                    VirtualDisk.writeCluster(clusterFATIndex,bytesls[i]);
                    FAT.setClusterNext(clusterFATIndex, -1);
                    if (lastCluster != -1)
                        FAT.setClusterNext(lastCluster, clusterFATIndex);
                    lastCluster = clusterFATIndex;
                    clusterFATIndex = FAT.GetEmptyCulster();
                }
            }
            if (entries.Count == 0)
            {
                if (firs_cluster != 0)
                    FAT.setClusterNext(firs_cluster, 0);
                firs_cluster = 0;
            }
            if (this.parent != null)
            {
                this.parent.updateContent(this.getDirectoryEntry());
                this.parent.WriteDirectory();
            }
            FAT.writeFat();
        }

        public void deleteDirectory()
        {
            clearDirSize();
            if (this.parent != null)
            {
                int index = this.parent.searchDirectory(new string(this.dir_name));
                if (index != -1)
                {
                    this.parent.ReadDirectory();
                    this.parent.entries.RemoveAt(index);
                    this.parent.WriteDirectory();
                }
            }
            if (Program.current == this)
            {
                if (this.parent != null)
                {
                    Program.current = this.parent;
                    Program.currentPath = Program.currentPath.Substring(0, Program.currentPath.LastIndexOf('\\'));
                    Program.current.ReadDirectory();
                }
            }
            FAT.writeFat();
        }

        
        public void clearDirSize()
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
                {
                    next = FAT.getClusterNext(clusterIndex);
                }

            } while (clusterIndex != -1);


            FAT.writeFat();
        }
        public int getMySizeOnDisk()
        {
            int size = 0;

            int clusterIndex = this.firs_cluster;

            int next = FAT.getClusterNext(clusterIndex);

            do
            {
                size++;
                clusterIndex = next;

                if (clusterIndex != -1)
                {
                    next = FAT.getClusterNext(clusterIndex);
                }

            } while (clusterIndex != -1);


           return size;
        }


        public bool canAddEntry(DirectoryEntry d)
        {
            bool can = false;
            int needeSize = (entries.Count + 1) * 32;
            int neededClusters = needeSize / 1024;
            int rem = needeSize % 1024;
            if (rem > 0)
                neededClusters++;
            neededClusters += d.dir_fileSize / 1024;
            int rem2= d.dir_fileSize % 1024;

            if (rem2 > 0)
                neededClusters++;
            if(getMySizeOnDisk()+FAT.getAvailableClusters() > neededClusters)
                can = true;

            return can;
        }
        public static byte[] DirToByte(DirectoryEntry d)
        {
            byte[] bytes = new byte[32];
            for (int i = 0; i < d.dir_name.Length; i++)
            {
                bytes[i] = (byte)d.dir_name[i];
            }
            bytes[11] = d.dir_attr;
            int j = 12;
            for (int i = 0; i < d.dir_empty.Length; i++)
            {
                bytes[j] = d.dir_empty[i];
                j++;
            }
            byte[] firstClusterInBytes = BitConverter.GetBytes(d.firs_cluster);
            for (int i = 0; i < firstClusterInBytes.Length; i++)
            {
                bytes[j] = firstClusterInBytes[i];
                j++;
            }
            byte[] SizeInBytes = BitConverter.GetBytes(d.dir_fileSize);
            for (int i = 0; i < SizeInBytes.Length; i++)
            {
                bytes[j] = SizeInBytes[i];
                j++;
            }
            return bytes;
        }

    }
}


/*public void ReadDirectory()
       {
           List<byte> ls = new List<byte>();
           entries = new List<DirectoryEntry>();
           if (firs_cluster != 0)
           {
               int clusterIndex = firs_cluster;
               int next = FAT.getClusterNext(clusterIndex);

               do
               {
                   ls.AddRange(VirtualDisk.readCluster(clusterIndex));

                   clusterIndex = next;

                   if (clusterIndex != -1)
                   {
                       next = FAT.getClusterNext(clusterIndex);
                   }

               } while (clusterIndex != -1);


               for (int i = 0; i < ls.Count; i++)
               {
                   byte[] data = new byte[32];

                   for (int k = i * 32, m = 0; m < data.Length && k < ls.Count; m++, k++)
                   {
                       data[m] = ls[k];
                   }
                   if (data[0] == 0)
                       break;

                   entries.Add(makeDirectory(data));

               }
           }    

       }//Read Directory*/

/*public void WriteDirectory()
{
    List<byte> ls = new List<byte>();

    for (int i = 0; i < entries.Count; i++)
    {
        byte[] b = Encoding.ASCII.GetBytes(entries[i].NameToString());
        ls.AddRange(b);
        ls.Add(entries[i].dir_attr);
        for (int j = 0; j < 3; j++)
        {
            ls.AddRange(BitConverter.GetBytes(entries[i].dir_empty[j]));
        }
        ls.AddRange(BitConverter.GetBytes(entries[i].firs_cluster));
        ls.AddRange(BitConverter.GetBytes(entries[i].dir_fileSize));
    } // preparing the list of bytes Entries -> Bytes

    List<dirCluster> listOfClusters = new List<dirCluster>();

    double length = ls.Count;

    for (int i = 0; i < Math.Ceiling(length / 1024); i++)
    {
        if (ls.Count >= 1024)
        {
            for (int j = 0; j < 1024; j++)
            {
                listOfClusters[i].b[j] = ls[j];
            }
        }
        else
        {
            for (int j = 0; j < ls.Count; j++)
            {
                listOfClusters[i].b[j] = ls[j];
            }
        }
        ls.RemoveRange(0, 1024);
    }

    int clusterIndex;

    if (firs_cluster == 0)
    {
        clusterIndex = FAT.GetEmptyCulster();
        firs_cluster = clusterIndex;
    }
    else
    {
        clusterIndex = firs_cluster;
        clearDirSize();
    }


    int lastCluster = -1;

    for (int i = 0; i < listOfClusters.Count; i++)
    {
        if (clusterIndex != -1)
        {
            // check fully disk

            VirtualDisk.writeCluster(clusterIndex, listOfClusters[i].b);
            FAT.setClusterNext(clusterIndex, -1);

            if (lastCluster != -1)
                FAT.setClusterNext(lastCluster, clusterIndex);

            lastCluster = clusterIndex;

            clusterIndex = FAT.GetEmptyCulster();
        }
    }

    if (this.parent != null)
    {
        parent.updateContent(this.getDirectoryEntry());
        parent.WriteDirectory();

    }


    FAT.writeFat();
}//WriteDirectory
*/