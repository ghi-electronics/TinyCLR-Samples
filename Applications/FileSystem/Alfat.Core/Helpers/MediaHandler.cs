using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Alfat.Core
{
    public class MediaHandler
    {
        public Hashtable HandleList { get; set; }
        public MediaHandler()
        {
            HandleList = new  Hashtable();
        }

        public bool IsExist(char HandleName)
        {
            return HandleList.Contains(HandleName);
        }
        public bool AddHandle(MediaHandle newItem)
        {
            try
            {
                if (HandleList.Contains(newItem.HandleName))
                {
                    HandleList[newItem.HandleName] = newItem;
                }
                else
                {
                    HandleList.Add(newItem.HandleName,newItem);
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
            var allItems = new MediaHandle[HandleList.Count];
            for(int i=0;i<HandleList.Count;i++)
            {
                allItems[i] = HandleList[i] as MediaHandle;
            }
            return allItems;
        }

        public MediaHandle GetHandle(char HandleName)
        {
            if (HandleList.Contains(HandleName))
            {
                return HandleList[HandleName] as MediaHandle;
            }
            return null;
        }

        public bool RemoveHandle(char HandleName)
        {
            if (HandleList.Contains(HandleName))
            {
                HandleList.Remove(HandleName);
                return true;
            }
            return false;
        }
    }
}
