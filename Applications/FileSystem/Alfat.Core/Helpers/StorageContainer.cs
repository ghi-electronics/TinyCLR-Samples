using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Alfat.Core
{
    public class StorageContainer
    {
        Hashtable containers;
        public StorageContainer() => this.containers = new Hashtable();

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
