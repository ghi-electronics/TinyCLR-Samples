using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace uAlfat.Core
{
    public class StorageContainer
    {
        Hashtable Containers;
        public StorageContainer()
        {
            Containers = new Hashtable();
        }

        public int Size
        {
            get
            {
                return Containers.Count;
            }
        }

        public StorageInfo GetStorageByIndex(int index)
        {
         
            int count = 0;
            foreach(var item in Containers.Values)
            {
                if(count == index)
                {
                    return item as StorageInfo;
                }
                count++;
            }
            return null;
        }

        public bool AddStorage(StorageInfo info)
        {
            try
            {
                if (Containers.Contains(info.Name))
                {
                    Containers[info.Name] = info;
                }
                else
                {
                    Containers.Add(info.Name, info);
                }
               
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        public StorageInfo GetStorage(string Name)
        {
            if (Containers.Contains(Name))
            {
                return Containers[Name] as StorageInfo;
            }
            return null;
        }

        public bool RemoveStorage(string Name)
        {

            if (Containers.Contains(Name))
            {
                Containers.Remove(Name);
                return true;
            }
            return false;
        }
    }
}
