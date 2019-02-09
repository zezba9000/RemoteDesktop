using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteDesktop.Client.Android
{

    /// <summary> 
    /// This class is a very fast and threadsafe FIFO buffer 
    /// </summary> 
    public class ByteFifo
    {
        private List<byte> mi_FifoData = new List<byte>();

        /// <summary> 
        /// Get the count of bytes in the Fifo buffer 
        /// </summary> 
        public int Count
        {
            get
            {
                lock (mi_FifoData)
                {
                    return mi_FifoData.Count;
                }
            }
        }

        /// <summary> 
        /// Clears the Fifo buffer 
        /// </summary> 
        public void Clear()
        {
            lock (mi_FifoData)
            {
                mi_FifoData.Clear();
            }
        }

        /// <summary> 
        /// Append data to the end of the fifo 
        /// </summary> 
        public void Push(byte[] u8_Data)
        {
            lock (mi_FifoData)
            {
                // Internally the .NET framework uses Array.Copy() which is extremely fast 
                mi_FifoData.AddRange(u8_Data);
            }
        }

        /// <summary> 
        /// Get data from the beginning of the fifo. 
        /// returns null if s32_Count bytes are not yet available. 
        /// </summary> 
        public byte[] Pop(int s32_Count)
        {
            lock (mi_FifoData)
            {
                if (mi_FifoData.Count < s32_Count)
                    return null;

                // Internally the .NET framework uses Array.Copy() which is extremely fast 
                byte[] u8_PopData = new byte[s32_Count];
                mi_FifoData.CopyTo(0, u8_PopData, 0, s32_Count);
                mi_FifoData.RemoveRange(0, s32_Count);
                return u8_PopData;
            }
        }

        /// <summary> 
        /// Gets a byte without removing it from the Fifo buffer 
        /// returns -1 if the index is invalid 
        /// </summary> 
        public int PeekAt(int s32_Index)
        {
            lock (mi_FifoData)
            {
                if (s32_Index < 0 || s32_Index >= mi_FifoData.Count)
                    return -1;

                return mi_FifoData[s32_Index];
            }
        }
    }

}
