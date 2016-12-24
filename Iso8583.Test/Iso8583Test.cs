using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Iso8583.Test
{
    [TestClass]
    public class Iso8583Test
    {
        [TestMethod]
        public void TestSmartBitmap()
        {
            //智能组包
            Iso8583Package sendPackage = new Iso8583Package();
            sendPackage.MessageType = "0210";
            sendPackage.SetString(2, "8888888888888888888");
            sendPackage.SetString(3, "171111");
            sendPackage.SetMoney(4, 12.34M);
            sendPackage.SetDateTime(7, DateTime.Now);
            sendPackage.SetNumber(11, 111);
            sendPackage.SetDateTime(12, DateTime.Now);
            sendPackage.SetDateTime(13, DateTime.Now);
            sendPackage.SetString(25, "20");
            sendPackage.SetNumber(28, 123);
            sendPackage.SetString(32, "00489900");
            sendPackage.SetString(33, "00489900");
            sendPackage.SetString(41, "03056505");
            sendPackage.SetString(42, "00489900");
            sendPackage.SetString(47, "");
            sendPackage.SetString(48, "99          20080422              01                   7512IX13                       ");
            sendPackage.SetString(53, "");
            sendPackage.SetString(102, "03056505100001080044");
            byte[] data = sendPackage.GetSendBuffer();
            Assert.AreEqual(0xF2, data[4]);

            Iso8583Package package = new Iso8583Package();
            package.ParseBuffer(data, true);
            Assert.AreEqual("0210", package.MessageType);
        }
        [TestMethod]
        public void TestSchemaFile()
        {
            //智能组包
            Iso8583Package sendPackage = new Iso8583Package("TestSchema.xml");
            sendPackage.MessageType = "0210";
            sendPackage.SetString(2, "8888888888888888888");
            sendPackage.SetString(3, "171111");
            sendPackage.SetMoney(4, 12.34M);
            sendPackage.SetDateTime(7, DateTime.Now);
            sendPackage.SetNumber(11, 111);
            sendPackage.SetDateTime(12, DateTime.Now);
            sendPackage.SetDateTime(13, DateTime.Now);
            sendPackage.SetString(25, "20");
            sendPackage.SetNumber(28, 123);
            sendPackage.SetString(32, "00489900");
            sendPackage.SetString(33, "00489900");
            sendPackage.SetString(41, "03056505");
            sendPackage.SetString(42, "00489900");
            sendPackage.SetString(47, "");
            sendPackage.SetString(48, "99          20080422              01                   7512IX13                       ");
            sendPackage.SetString(53, "");
            sendPackage.SetString(102, "03056505100001080044");
            byte[] data = sendPackage.GetSendBuffer();
            Assert.AreEqual(0xF2, data[4]);

            Iso8583Package package = new Iso8583Package("TestSchema.xml");
            package.ParseBuffer(data, true);
            Assert.AreEqual("0210", package.MessageType);
        }
    }
}
