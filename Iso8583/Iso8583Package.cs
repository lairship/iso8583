//作者：刘飞廷 lairship@163.com

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.IO;

namespace Iso8583
{
    /// <summary>
    /// ISO 8583 数据包类
    /// </summary>
    public class Iso8583Package
    {
        private string messageType;
        private Iso8583Schema schema;
        private Iso8583Bitmap bitmap = new Iso8583Bitmap();
        private SortedList<int, object> values = new SortedList<int, object>(Iso8583Bitmap.FieldCount);
        private bool smartBitmap = false;

        #region 构造函数
        /// <summary>
        /// 使用内嵌的全128字段的 Schema 构造数据包类
        /// </summary>
        public Iso8583Package()
        {
            using (StreamReader sr = new System.IO.StreamReader(
                this.GetType().Assembly.GetManifestResourceStream("Iso8583.FullSchema.xml")))
            {
                string xml = sr.ReadToEnd();
                this.schema = new Iso8583Schema();
                this.schema.LoadFromXml(xml);
                this.smartBitmap = true;
            }
        }
        /// <summary>
        /// 使用指定的 Schema 实例构造数据包类
        /// </summary>
        /// <param name="schema">Schema 实例</param>
        public Iso8583Package(Iso8583Schema schema)
        {
            this.schema = schema;
        }
        /// <summary>
        /// 使用指定的 Schema 文件构造数据包类
        /// </summary>
        /// <param name="schemaFile">Schema 文件</param>
        public Iso8583Package(string schemaFile)
        {
            this.schema = new Iso8583Schema(schemaFile);
        }
        #endregion

        #region 公共属性
        /// <summary>
        /// 消息类型
        /// </summary>
        public string MessageType
        {
            get { return this.messageType; }
            set
            {
                if (value.Length != 4)
                    throw new Exception("长度不正确。");
                this.messageType = value;
            }
        }
        /// <summary>
        /// 指示是否使用智能位图模式进行组包和解包。
        /// 设置 true 时需要 Schema 为全128字段的定义。
        /// </summary>
        public bool SmartBitmap
        {
            get { return this.smartBitmap; }
            set
            {
                if (value)
                {
                    if (!this.schema.IsFullBitmap)
                        throw new Exception("架构定义不是全128字段的，不能开启智能位图模式进行组包和解包");
                }
                this.smartBitmap = value;
            }
        }
        #endregion

        #region 为数据域设置值
        /// <summary>
        /// 清除所有数据。
        /// </summary>
        public void Clear()
        {
            this.bitmap = new Iso8583Bitmap();
            this.values = new SortedList<int, object>(Iso8583Bitmap.FieldCount);
        }
        /// <summary>
        /// 为指定数据域设置一个字符串值
        /// </summary>
        /// <param name="bitNum">数据域</param>
        /// <param name="value">字符串值</param>
        public void SetString(int bitNum, string value)
        {
            if (!this.schema.fields.ContainsKey(bitNum))
                throw new Exception(String.Format("数据包定义不包含此域：{0}", bitNum));
            Iso8583Field field = this.schema.fields[bitNum];
            if (Encoding.Default.GetByteCount(value) > field.Length)
                throw new Exception("长度过长。");
            switch (field.DataType)
            {
                case Iso8583DataType.B:
                    throw new Exception("格式不符。");
                default:
                    values[bitNum] = value;
                    break;
            }
            this.bitmap.Set(bitNum, value != null);
        }
        /// <summary>
        /// 为指定数据域设置一个数字值
        /// </summary>
        /// <param name="bitNum">数据域</param>
        /// <param name="value">数字值</param>
        public void SetNumber(int bitNum, int value)
        {
            if (!this.schema.fields.ContainsKey(bitNum))
                throw new Exception(String.Format("数据包定义不包含此域：{0}", bitNum));
            Iso8583Field field = this.schema.fields[bitNum];
            string strValue = value.ToString();
            if (strValue.Length > field.Length)
                throw new ArgumentException("数值过大。", "value");
            switch (field.DataType)
            {
                case Iso8583DataType.B:
                    throw new Exception("格式不符。");
                default:
                    values[bitNum] = new string('0', field.Length - strValue.Length) + strValue;
                    break;
            }
            this.bitmap.Set(bitNum, true);
        }
        /// <summary>
        /// 为指定数据域设置一个金额值
        /// </summary>
        /// <param name="bitNum">数据域</param>
        /// <param name="money">金额值</param>
        public void SetMoney(int bitNum, decimal money)
        {
            int value = Convert.ToInt32(money * 100);
            this.SetNumber(bitNum, value);
        }
        /// <summary>
        /// 为指定数据域设置一个日期值
        /// </summary>
        /// <param name="bitNum">数据域</param>
        /// <param name="time">日期值</param>
        public void SetDateTime(int bitNum, DateTime time)
        {
            if (!this.schema.fields.ContainsKey(bitNum))
                throw new Exception(String.Format("数据包定义不包含此域：{0}", bitNum));
            Iso8583Field field = this.schema.fields[bitNum];
            switch (field.DataType)
            {
                case Iso8583DataType.B:
                    throw new Exception("格式不符。");
                default:
                    switch (field.Format)
                    {
                        case Iso8583Format.YYMMDD:
                            values[bitNum] = time.ToString("yyMMdd");
                            break;
                        case Iso8583Format.YYMM:
                            values[bitNum] = time.ToString("yyMM");
                            break;
                        case Iso8583Format.MMDD:
                            values[bitNum] = time.ToString("MMdd");
                            break;
                        case Iso8583Format.hhmmss:
                            values[bitNum] = time.ToString("HHmmss");
                            break;
                        case Iso8583Format.MMDDhhmmss:
                            values[bitNum] = time.ToString("MMddHHmmss");
                            break;
                        default:
                            throw new Exception("格式不符。");
                    }
                    break;
            }
            this.bitmap.Set(bitNum, true);
        }
        /// <summary>
        /// 为指定数据域设置一个二进制值
        /// </summary>
        /// <param name="bitNum">数据域</param>
        /// <param name="data">二进制值</param>
        public void SetArrayData(int bitNum, byte[] data)
        {
            if (!this.schema.fields.ContainsKey(bitNum))
                throw new Exception(String.Format("数据包定义不包含此域：{0}", bitNum));
            Iso8583Field field = this.schema.fields[bitNum];
            if (data.Length > field.Length)
                throw new Exception("长度过长。");
            switch (field.DataType)
            {
                case Iso8583DataType.B:
                    values[bitNum] = data;
                    break;
                default:
                    throw new Exception("格式不符。");
            }
            this.bitmap.Set(bitNum, data != null);
        }
        #endregion

        #region 从数据域获取值
        /// <summary>
        /// 获取某个域上是否存在有效值。
        /// </summary>
        /// <param name="bitNum">数据域</param>
        /// <returns></returns>
        public bool ExistValue(int bitNum)
        {
            return this.values.ContainsKey(bitNum) && (this.values[bitNum] != null);
        }
        /// <summary>
        /// 从指定数据域获取字符串值
        /// </summary>
        /// <param name="bitNum">数据域</param>
        /// <returns></returns>
        public string GetString(int bitNum)
        {
            if (!this.schema.fields.ContainsKey(bitNum))
                throw new Exception(String.Format("数据包定义不包含此域：{0}", bitNum));
            Iso8583Field field = this.schema.fields[bitNum];
            if (!this.values.ContainsKey(bitNum) || (this.values[bitNum] == null))
                throw new Exception(String.Format("数据域 {0} 不包含任何有效值。", bitNum));
            switch (field.DataType)
            {
                case Iso8583DataType.B:
                    throw new Exception("格式不符。");
                default:
                    return this.values[bitNum].ToString();
            }
        }
        /// <summary>
        /// 从指定数据域获取数字值
        /// </summary>
        /// <param name="bitNum">数据域</param>
        /// <returns></returns>
        public int GetNumber(int bitNum)
        {
            if (!this.schema.fields.ContainsKey(bitNum))
                throw new Exception(String.Format("数据包定义不包含此域：{0}", bitNum));
            Iso8583Field field = this.schema.fields[bitNum];
            if (!this.values.ContainsKey(bitNum) || (this.values[bitNum] == null))
                throw new Exception(String.Format("数据域 {0} 不包含任何有效值。", bitNum));
            switch (field.DataType)
            {
                case Iso8583DataType.N:
                    return Convert.ToInt32(this.values[bitNum]);
                default:
                    throw new Exception("格式不符。");
            }
        }
        /// <summary>
        /// 从指定数据域获取金额值
        /// </summary>
        /// <param name="bitNum">数据域</param>
        /// <returns></returns>
        public decimal GetMoney(int bitNum)
        {
            decimal money = this.GetNumber(bitNum);
            return money / 100;
        }
        /// <summary>
        /// 从指定数据域获取日期值
        /// </summary>
        /// <param name="bitNum">数据域</param>
        /// <returns></returns>
        public DateTime GetDateTime(int bitNum)
        {
            if (!this.schema.fields.ContainsKey(bitNum))
                throw new Exception(String.Format("数据包定义不包含此域：{0}", bitNum));
            Iso8583Field field = this.schema.fields[bitNum];
            if (!this.values.ContainsKey(bitNum) || (this.values[bitNum] == null))
                throw new Exception(String.Format("数据域 {0} 不包含任何有效值。", bitNum));
            switch (field.DataType)
            {
                case Iso8583DataType.B:
                    throw new Exception("格式不符。");
                default:
                    string value = (string)this.values[bitNum];
                    switch (field.Format)
                    {
                        case Iso8583Format.YYMMDD:
                            return DateTime.ParseExact(value, "yyMMdd", null);
                        case Iso8583Format.YYMM:
                            return DateTime.ParseExact(value, "yyMM", null);
                        case Iso8583Format.MMDD:
                            return DateTime.ParseExact(value, "MMdd", null);
                        case Iso8583Format.hhmmss:
                            return DateTime.ParseExact(value, "HHmmss", null);
                        case Iso8583Format.MMDDhhmmss:
                            return DateTime.ParseExact(value, "MMddHHmmss", null);
                        default:
                            throw new Exception("格式不符。");
                    }
            }
        }
        /// <summary>
        /// 从指定数据域获取二进制值
        /// </summary>
        /// <param name="bitNum">数据域</param>
        /// <returns></returns>
        public byte[] GetArrayData(int bitNum)
        {
            if (!this.schema.fields.ContainsKey(bitNum))
                throw new Exception(String.Format("数据包定义不包含此域：{0}", bitNum));
            Iso8583Field field = this.schema.fields[bitNum];
            if (!this.values.ContainsKey(bitNum) || (this.values[bitNum] == null))
                throw new Exception(String.Format("数据域 {0} 不包含任何有效值。", bitNum));
            switch (field.DataType)
            {
                case Iso8583DataType.B:
                    return (byte[])this.values[bitNum];
                default:
                    throw new Exception(String.Format("数据域 {0} 格式不是二进制。", bitNum));
            }
        }
        #endregion

        #region 组包
        private int GetLength(int bitNum)
        {
            Debug.Assert(this.schema.fields.ContainsKey(bitNum));
            Iso8583Field field = this.schema.fields[bitNum];
            switch (field.DataType)
            {
                case Iso8583DataType.B:
                    return field.Length;
                default:
                    switch (field.Format)
                    {
                        case Iso8583Format.LVAR:
                        case Iso8583Format.LLVAR:
                        case Iso8583Format.LLLVAR:
                            string value = "";
                            int len = 0;
                            if (this.values.ContainsKey(bitNum) && (this.values[bitNum] != null))
                            {
                                value = (string)values[bitNum];
                                len = Encoding.Default.GetByteCount(value);
                            }
                            return len + field.Format - Iso8583Format.LVAR + 1;
                        default:
                            return field.Length;
                    }
            }
        }
        private void AppendData(string str, Array dst, ref int pos)
        {
            if (String.IsNullOrEmpty(str)) return;
            byte[] field = Encoding.Default.GetBytes(str);
            System.Buffer.BlockCopy(field, 0, dst, pos, field.Length);
            pos += field.Length;
        }

        /// <summary>
        /// 组包一个 ISO 8583 数据包
        /// </summary>
        /// <returns></returns>
        public byte[] GetSendBuffer()
        {
            int len = 16;
            if (!String.IsNullOrEmpty(this.messageType))
                len += this.messageType.Length;
            Iso8583Bitmap map = this.schema.bitmap;
            if (this.smartBitmap)
                map = this.bitmap;
            for (int bitNum = 2; bitNum <= Iso8583Bitmap.FieldCount; bitNum++)
            {
                if (map.Get(bitNum))
                {
                    len += this.GetLength(bitNum);
                    if (bitNum > 64)
                        map.Set(1, true);
                }
            }

            byte[] result = new byte[len];
            int pos = 0;
            this.AppendData(MessageType, result, ref pos);
            map.CopyTo(result, pos);
            pos += 16;
            for (int bitNum = 2; bitNum <= Iso8583Bitmap.FieldCount; bitNum++)
            {
                if (!map.Get(bitNum)) continue;
                Iso8583Field field = this.schema.fields[bitNum];
                switch (field.DataType)
                {
                    case Iso8583DataType.B:
                        if (this.ExistValue(bitNum))
                        {
                            byte[] data = (byte[])this.values[bitNum];
                            data.CopyTo(result, pos);
                        }
                        pos += field.Length;
                        break;
                    default:
                        string value = "";
                        len = 0;
                        if (this.ExistValue(bitNum))
                        {
                            value = (string)this.values[bitNum];
                            len = Encoding.Default.GetByteCount(value);
                        }
                        switch (field.Format)
                        {
                            case Iso8583Format.LVAR:
                                value = len.ToString("0") + value;
                                break;
                            case Iso8583Format.LLVAR:
                                value = len.ToString("00") + value;
                                break;
                            case Iso8583Format.LLLVAR:
                                value = len.ToString("000") + value;
                                break;
                            default:
                                if (len < field.Length)
                                {
                                    if (field.DataType == Iso8583DataType.N)
                                    {
                                        value = new string('0', field.Length - len) + value;
                                    }
                                    else
                                    {
                                        value += new string(' ', field.Length - len);
                                    }
                                }
                                break;
                        }
                        this.AppendData(value, result, ref pos);
                        break;
                }
            }
            return result;
        }
        #endregion

        #region 解包
        /// <summary>
        /// 解包一个 ISO 8583 数据包
        /// </summary>
        /// <param name="buf">数据包</param>
        /// <param name="haveMT">数据包是否包含4字节的MessageType</param>
        public void ParseBuffer(byte[] buf, bool haveMT)
        {
            int pos = 0;
            if (buf == null)
                throw new ArgumentNullException("buf");
            if (buf.Length < 20)
                throw new ArgumentException("数据包长度不符合定义", "buf");
            if (haveMT)
            {
                this.messageType = Encoding.Default.GetString(buf, pos, 4);
                pos += 4;
            }
            byte[] data = new byte[16];
            Array.Copy(buf, pos, data, 0, 16);
            pos += 16;
            this.bitmap = new Iso8583Bitmap(data);
            if (!this.smartBitmap && !this.schema.bitmap.IsEqual(data))
                throw new Exception("数据包的位图表和定义的不一致");
            for (int bitNum = 2; bitNum <= Iso8583Bitmap.FieldCount; bitNum++)
            {
                if (!bitmap.Get(bitNum)) continue;
                Iso8583Field field = this.schema.fields[bitNum];
                int len = 0;
                switch (field.DataType)
                {
                    case Iso8583DataType.B:
                        len = field.Length;
                        break;
                    default:
                        switch (field.Format)
                        {
                            case Iso8583Format.LVAR:
                            case Iso8583Format.LLVAR:
                            case Iso8583Format.LLLVAR:
                                int varLen = field.Format - Iso8583Format.LVAR + 1;
                                len = int.Parse(Encoding.Default.GetString(buf, pos, varLen));
                                pos += varLen;
                                break;
                            default:
                                len = field.Length;
                                break;
                        }
                        break;
                }
                if (buf.Length < pos + len)
                    throw new ArgumentException("数据包长度不符合定义", "buf");
                switch (field.DataType)
                {
                    case Iso8583DataType.B:
                        if (len > 0)
                        {
                            data = new byte[len];
                            Array.Copy(buf, pos, data, 0, len);
                            this.values[bitNum] = data;
                        }
                        break;
                    default:
                        this.values[bitNum] = Encoding.Default.GetString(buf, pos, len);
                        break;
                }
                pos += len;
            }
        }
        #endregion

        #region Util
        /// <summary>
        /// 获取一个适合在日志中输出的字符串
        /// </summary>
        /// <returns></returns>
        public string GetLogText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Package(MessageType:{0}):", this.messageType);
            sb.AppendLine();
            sb.AppendLine("{");
            foreach (KeyValuePair<int, object> kvp in this.values)
            {
                Iso8583Field field = this.schema.fields[kvp.Key];
                string value = "";
                if (kvp.Value != null)
                {
                    switch (field.DataType)
                    {
                        case Iso8583DataType.B:
                            value = BitConverter.ToString((byte[])kvp.Value);
                            break;
                        default:
                            value = (string)kvp.Value;
                            break;
                    }
                }
                sb.AppendFormat("    [{0}]:{1}", field.FieldName, value);
                sb.AppendLine();
            }
            sb.AppendLine("}");
            return sb.ToString();
        }
        #endregion
    }
}
