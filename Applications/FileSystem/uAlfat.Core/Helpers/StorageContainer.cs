using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace uAlfat.Core
{
    public class StorageContainer
    {
        Hashtable containers;
        public StorageContainer() => this.containers = new Hashtable();

        public int Size => this.containers.Count;

        public StorageInfo GetStorageByIndex(int index)
        {
         
            var count = 0;
            foreach(var item in this.containers.Values)
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
                if (this.containers.Contains(info.Name))
                {
                    this.containers[info.Name] = info;
                }
                else
                {
                    this.containers.Add(info.Name, info);
                }
               
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        public StorageInfo GetStorage(string name)
        {
            if (this.containers.Contains(name))
            {
                return this.containers[name] as StorageInfo;
            }
            return null;
        }

        public bool RemoveStorage(string name)
        {

            if (this.containers.Contains(name))
            {
                this.containers.Remove(name);
                return true;
            }
            return false;
        }
    }
}
