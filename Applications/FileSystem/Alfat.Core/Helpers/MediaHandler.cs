using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Alfat.Core
{
    public class MediaHandler
    {
        public Hashtable HandleList { get; set; }
        public MediaHandler() => this.HandleList = new Hashtable();

        public bool IsExist(char handleName) => this.HandleList.Contains(handleName);
        public bool AddHandle(MediaHandle newItem)
        {
            try
            {
                if (this.HandleList.Contains(newItem.HandleName))
                {
                    this.HandleList[newItem.HandleName] = newItem;
                }
                else
                {
                    this.HandleList.Add(newItem.HandleName,newItem);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public MediaHandle[] GetAll()
        {
            var allItems = new MediaHandle[this.HandleList.Count];
            for(var i=0;i< this.HandleList.Count;i++)
            {
                allItems[i] = this.HandleList[i] as MediaHandle;
            }
            return allItems;
        }

        public MediaHandle GetHandle(char handleName)
        {
            if (this.HandleList.Contains(handleName))
            {
                return this.HandleList[handleName] as MediaHandle;
            }
            return null;
        }

        public bool RemoveHandle(char handleName)
        {
            if (this.HandleList.Contains(handleName))
            {
                this.HandleList.Remove(handleName);
                return true;
            }
            return false;
        }
    }
}
