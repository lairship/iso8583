//作者：刘飞廷 lairship@163.com

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Iso8583
{
    internal class Iso8583Bitmap
    {
        //ISO 8583 包的最大域数：128
        internal const int FieldCount = 128;
        private BitArray array;

        public Iso8583Bitmap()
        {
            this.array = new BitArray(FieldCount);
        }
        public Iso8583Bitmap(byte[] map)
        {
            Array.Reverse(map);
            this.array = new BitArray(map);
            Array.Reverse(map);
        }
        /// <summary>
        /// 获取一个值，指示位图是否是全128字段的。
        /// </summary>
        public bool IsFull
        {
            get
            {
                byte[] thisMap = this.GetBytes();
                for (int i = 0; i < thisMap.Length; i++)
                {
                    if (thisMap[i] != 255)
                        return false;
                }
                return true;
            }
        }

        public byte[] GetBytes()
        {
            byte[] map = new byte[16];
            this.array.CopyTo(map, 0);
            Array.Reverse(map);
            return map;
        }

        public bool Get(int bitNum)
        {
            return this.array.Get(FieldCount - bitNum);
        }
        public void Set(int bitNum, bool value)
        {
            this.array.Set(FieldCount - bitNum, value);
        }
        public void CopyTo(Array array, int index)
        {
            byte[] map = this.GetBytes();
            map.CopyTo(array, index);
        }
        public bool IsEqual(byte[] map)
        {
            byte[] thisMap = this.GetBytes();
            if (thisMap.Length != map.Length)
                return false;
            for (int i = 0; i < map.Length; i++)
                if (map[i] != thisMap[i])
                    return false;
            return true;
        }
    }
}
