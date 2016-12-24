//作者：刘飞廷 lairship@163.com

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml.Serialization;
using System.IO;

namespace Iso8583
{
    /// <summary>
    /// 表示 ISO 8583 包的所有数据域集合
    /// </summary>
    public class Iso8583Schema
    {
        internal Iso8583Bitmap bitmap;
        internal SortedList<int, Iso8583Field> fields;

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public Iso8583Schema()
        {
            this.bitmap = new Iso8583Bitmap();
            this.fields = new SortedList<int, Iso8583Field>(Iso8583Bitmap.FieldCount);
        }
        /// <summary>
        /// 从指定文件加载并构造实例
        /// </summary>
        /// <param name="fileName"></param>
        public Iso8583Schema(string fileName)
            : this()
        {
            this.LoadFromFile(fileName);
        }

        /// <summary>
        /// 获取一个值，指示包定义是否是全128字段的。
        /// </summary>
        public bool IsFullBitmap
        {
            get
            {
                return this.bitmap.IsFull;
            }
        }

        /// <summary>
        /// 增加一个数据域
        /// </summary>
        /// <param name="field">数据域信息</param>
        public void AddField(Iso8583Field field)
        {
            if (field == null)
                throw new ArgumentNullException("field");
            if (this.fields.ContainsKey(field.BitNum))
                throw new Exception("数据域字义已经存在。");
            if (field.BitNum < 1 || field.BitNum > Iso8583Bitmap.FieldCount)
                throw new Exception("位域值不合法。");
            this.fields.Add(field.BitNum, field);
            this.bitmap.Set(field.BitNum, true);
        }
        /// <summary>
        /// 移除一个数据域
        /// </summary>
        /// <param name="bitNum"></param>
        public void RemoveField(int bitNum)
        {
            this.fields.Remove(bitNum);
            this.bitmap.Set(bitNum, false);
        }

        /// <summary>
        /// 从Xml文本导入架构
        /// </summary>
        /// <param name="xml">xml文本</param>
        /// <returns></returns>
        public void LoadFromXml(string xml)
        {
            XmlSerializer serial = new XmlSerializer(typeof(Iso8583Field[]));
            StringReader reader = new StringReader(xml);
            Iso8583Field[] array = serial.Deserialize(reader) as Iso8583Field[];
            foreach (Iso8583Field field in array)
            {
                this.AddField(field);
            }
        }
        /// <summary>
        /// 从文件导入架构
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public void LoadFromFile(string fileName)
        {
            StreamReader reader = new StreamReader(fileName);
            string xml = reader.ReadToEnd();
            reader.Close();
            this.LoadFromXml(xml);
        }
        /// <summary>
        /// 把架构导出成文本
        /// </summary>
        /// <returns></returns>
        public string ExportToXml()
        {
            Iso8583Field[] array = new Iso8583Field[this.fields.Count];
            int i = 0;
            foreach (KeyValuePair<int, Iso8583Field> kvp in this.fields)
            {
                array[i++] = kvp.Value;
            }
            XmlSerializer serial = new XmlSerializer(typeof(Iso8583Field[]));
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            serial.Serialize(writer, array);
            return sb.ToString();
        }
        /// <summary>
        /// 保存架构到文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        public void SaveToFile(string fileName)
        {
            string xml = this.ExportToXml();
            StreamWriter writer = new StreamWriter(fileName);
            writer.Write(xml);
            writer.Close();
        }
    }
}
