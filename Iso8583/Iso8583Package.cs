//���ߣ�����͢ lairship@163.com

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.IO;

namespace Iso8583
{
    /// <summary>
    /// ISO 8583 ���ݰ���
    /// </summary>
    public class Iso8583Package
    {
        private string messageType;
        private Iso8583Schema schema;
        private Iso8583Bitmap bitmap = new Iso8583Bitmap();
        private SortedList<int, object> values = new SortedList<int, object>(Iso8583Bitmap.FieldCount);
        private bool smartBitmap = false;

        #region ���캯��
        /// <summary>
        /// ʹ����Ƕ��ȫ128�ֶε� Schema �������ݰ���
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
        /// ʹ��ָ���� Schema ʵ���������ݰ���
        /// </summary>
        /// <param name="schema">Schema ʵ��</param>
        public Iso8583Package(Iso8583Schema schema)
        {
            this.schema = schema;
        }
        /// <summary>
        /// ʹ��ָ���� Schema �ļ��������ݰ���
        /// </summary>
        /// <param name="schemaFile">Schema �ļ�</param>
        public Iso8583Package(string schemaFile)
        {
            this.schema = new Iso8583Schema(schemaFile);
        }
        #endregion

        #region ��������
        /// <summary>
        /// ��Ϣ����
        /// </summary>
        public string MessageType
        {
            get { return this.messageType; }
            set
            {
                if (value.Length != 4)
                    throw new Exception("���Ȳ���ȷ��");
                this.messageType = value;
            }
        }
        /// <summary>
        /// ָʾ�Ƿ�ʹ������λͼģʽ��������ͽ����
        /// ���� true ʱ��Ҫ Schema Ϊȫ128�ֶεĶ��塣
        /// </summary>
        public bool SmartBitmap
        {
            get { return this.smartBitmap; }
            set
            {
                if (value)
                {
                    if (!this.schema.IsFullBitmap)
                        throw new Exception("�ܹ����岻��ȫ128�ֶεģ����ܿ�������λͼģʽ��������ͽ��");
                }
                this.smartBitmap = value;
            }
        }
        #endregion

        #region Ϊ����������ֵ
        /// <summary>
        /// ����������ݡ�
        /// </summary>
        public void Clear()
        {
            this.bitmap = new Iso8583Bitmap();
            this.values = new SortedList<int, object>(Iso8583Bitmap.FieldCount);
        }
        /// <summary>
        /// Ϊָ������������һ���ַ���ֵ
        /// </summary>
        /// <param name="bitNum">������</param>
        /// <param name="value">�ַ���ֵ</param>
        public void SetString(int bitNum, string value)
        {
            if (!this.schema.fields.ContainsKey(bitNum))
                throw new Exception(String.Format("���ݰ����岻��������{0}", bitNum));
            Iso8583Field field = this.schema.fields[bitNum];
            if (Encoding.Default.GetByteCount(value) > field.Length)
                throw new Exception("���ȹ�����");
            switch (field.DataType)
            {
                case Iso8583DataType.B:
                    throw new Exception("��ʽ������");
                default:
                    values[bitNum] = value;
                    break;
            }
            this.bitmap.Set(bitNum, value != null);
        }
        /// <summary>
        /// Ϊָ������������һ������ֵ
        /// </summary>
        /// <param name="bitNum">������</param>
        /// <param name="value">����ֵ</param>
        public void SetNumber(int bitNum, int value)
        {
            if (!this.schema.fields.ContainsKey(bitNum))
                throw new Exception(String.Format("���ݰ����岻��������{0}", bitNum));
            Iso8583Field field = this.schema.fields[bitNum];
            string strValue = value.ToString();
            if (strValue.Length > field.Length)
                throw new ArgumentException("��ֵ����", "value");
            switch (field.DataType)
            {
                case Iso8583DataType.B:
                    throw new Exception("��ʽ������");
                default:
                    values[bitNum] = new string('0', field.Length - strValue.Length) + strValue;
                    break;
            }
            this.bitmap.Set(bitNum, true);
        }
        /// <summary>
        /// Ϊָ������������һ�����ֵ
        /// </summary>
        /// <param name="bitNum">������</param>
        /// <param name="money">���ֵ</param>
        public void SetMoney(int bitNum, decimal money)
        {
            int value = Convert.ToInt32(money * 100);
            this.SetNumber(bitNum, value);
        }
        /// <summary>
        /// Ϊָ������������һ������ֵ
        /// </summary>
        /// <param name="bitNum">������</param>
        /// <param name="time">����ֵ</param>
        public void SetDateTime(int bitNum, DateTime time)
        {
            if (!this.schema.fields.ContainsKey(bitNum))
                throw new Exception(String.Format("���ݰ����岻��������{0}", bitNum));
            Iso8583Field field = this.schema.fields[bitNum];
            switch (field.DataType)
            {
                case Iso8583DataType.B:
                    throw new Exception("��ʽ������");
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
                            throw new Exception("��ʽ������");
                    }
                    break;
            }
            this.bitmap.Set(bitNum, true);
        }
        /// <summary>
        /// Ϊָ������������һ��������ֵ
        /// </summary>
        /// <param name="bitNum">������</param>
        /// <param name="data">������ֵ</param>
        public void SetArrayData(int bitNum, byte[] data)
        {
            if (!this.schema.fields.ContainsKey(bitNum))
                throw new Exception(String.Format("���ݰ����岻��������{0}", bitNum));
            Iso8583Field field = this.schema.fields[bitNum];
            if (data.Length > field.Length)
                throw new Exception("���ȹ�����");
            switch (field.DataType)
            {
                case Iso8583DataType.B:
                    values[bitNum] = data;
                    break;
                default:
                    throw new Exception("��ʽ������");
            }
            this.bitmap.Set(bitNum, data != null);
        }
        #endregion

        #region ���������ȡֵ
        /// <summary>
        /// ��ȡĳ�������Ƿ������Чֵ��
        /// </summary>
        /// <param name="bitNum">������</param>
        /// <returns></returns>
        public bool ExistValue(int bitNum)
        {
            return this.values.ContainsKey(bitNum) && (this.values[bitNum] != null);
        }
        /// <summary>
        /// ��ָ���������ȡ�ַ���ֵ
        /// </summary>
        /// <param name="bitNum">������</param>
        /// <returns></returns>
        public string GetString(int bitNum)
        {
            if (!this.schema.fields.ContainsKey(bitNum))
                throw new Exception(String.Format("���ݰ����岻��������{0}", bitNum));
            Iso8583Field field = this.schema.fields[bitNum];
            if (!this.values.ContainsKey(bitNum) || (this.values[bitNum] == null))
                throw new Exception(String.Format("������ {0} �������κ���Чֵ��", bitNum));
            switch (field.DataType)
            {
                case Iso8583DataType.B:
                    throw new Exception("��ʽ������");
                default:
                    return this.values[bitNum].ToString();
            }
        }
        /// <summary>
        /// ��ָ���������ȡ����ֵ
        /// </summary>
        /// <param name="bitNum">������</param>
        /// <returns></returns>
        public int GetNumber(int bitNum)
        {
            if (!this.schema.fields.ContainsKey(bitNum))
                throw new Exception(String.Format("���ݰ����岻��������{0}", bitNum));
            Iso8583Field field = this.schema.fields[bitNum];
            if (!this.values.ContainsKey(bitNum) || (this.values[bitNum] == null))
                throw new Exception(String.Format("������ {0} �������κ���Чֵ��", bitNum));
            switch (field.DataType)
            {
                case Iso8583DataType.N:
                    return Convert.ToInt32(this.values[bitNum]);
                default:
                    throw new Exception("��ʽ������");
            }
        }
        /// <summary>
        /// ��ָ���������ȡ���ֵ
        /// </summary>
        /// <param name="bitNum">������</param>
        /// <returns></returns>
        public decimal GetMoney(int bitNum)
        {
            decimal money = this.GetNumber(bitNum);
            return money / 100;
        }
        /// <summary>
        /// ��ָ���������ȡ����ֵ
        /// </summary>
        /// <param name="bitNum">������</param>
        /// <returns></returns>
        public DateTime GetDateTime(int bitNum)
        {
            if (!this.schema.fields.ContainsKey(bitNum))
                throw new Exception(String.Format("���ݰ����岻��������{0}", bitNum));
            Iso8583Field field = this.schema.fields[bitNum];
            if (!this.values.ContainsKey(bitNum) || (this.values[bitNum] == null))
                throw new Exception(String.Format("������ {0} �������κ���Чֵ��", bitNum));
            switch (field.DataType)
            {
                case Iso8583DataType.B:
                    throw new Exception("��ʽ������");
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
                            throw new Exception("��ʽ������");
                    }
            }
        }
        /// <summary>
        /// ��ָ���������ȡ������ֵ
        /// </summary>
        /// <param name="bitNum">������</param>
        /// <returns></returns>
        public byte[] GetArrayData(int bitNum)
        {
            if (!this.schema.fields.ContainsKey(bitNum))
                throw new Exception(String.Format("���ݰ����岻��������{0}", bitNum));
            Iso8583Field field = this.schema.fields[bitNum];
            if (!this.values.ContainsKey(bitNum) || (this.values[bitNum] == null))
                throw new Exception(String.Format("������ {0} �������κ���Чֵ��", bitNum));
            switch (field.DataType)
            {
                case Iso8583DataType.B:
                    return (byte[])this.values[bitNum];
                default:
                    throw new Exception(String.Format("������ {0} ��ʽ���Ƕ����ơ�", bitNum));
            }
        }
        #endregion

        #region ���
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
        /// ���һ�� ISO 8583 ���ݰ�
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

        #region ���
        /// <summary>
        /// ���һ�� ISO 8583 ���ݰ�
        /// </summary>
        /// <param name="buf">���ݰ�</param>
        /// <param name="haveMT">���ݰ��Ƿ����4�ֽڵ�MessageType</param>
        public void ParseBuffer(byte[] buf, bool haveMT)
        {
            int pos = 0;
            if (buf == null)
                throw new ArgumentNullException("buf");
            if (buf.Length < 20)
                throw new ArgumentException("���ݰ����Ȳ����϶���", "buf");
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
                throw new Exception("���ݰ���λͼ��Ͷ���Ĳ�һ��");
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
                    throw new ArgumentException("���ݰ����Ȳ����϶���", "buf");
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
        /// ��ȡһ���ʺ�����־��������ַ���
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
