using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FirmwareImageFormat;

namespace UnitTestIntelHexFormat
{
    [TestClass]
    public class UnitTest1
    {
        public string MakeCheckSum(string payload)
        {
            int cs = 0;
            for (int i = 0; i < payload.Length; i += 2)
                cs += int.Parse(payload.Substring(i, 2), NumberStyles.HexNumber);

            cs = cs & 0xFF;
            cs = 0xFF - cs;
            cs = (cs + 1) & 0xFF;

            return cs.ToString("X2");
        }

        [TestMethod]
        public void TestMethod1()
        {
            string line = ":02000004CAFE32";

            var data = IntelHexFormat.DecodeLine(line);
            var answer = IntelHexFormat.EncodeData(data);

            System.Diagnostics.Trace.WriteLine("Length = " + data.Data.Length);
            System.Diagnostics.Trace.WriteLine("Address = " + data.Address);
            System.Diagnostics.Trace.WriteLine("Type = " + data.Type);

            Assert.AreEqual(2, data.Data.Length);
            Assert.AreEqual(0, data.Address);
            Assert.AreEqual(IntelHexFormat.RecordType.ExtendedLinearAddress, data.Type);
            Assert.AreEqual(answer, line);
        }

        [TestMethod]
        public void TestMethod2()
        {
            string line = ":1001000055AA55AA55AA55AA55AA55AA55AA55AAF7";

            var data = IntelHexFormat.DecodeLine(line);
            var answer = IntelHexFormat.EncodeData(data);

            System.Diagnostics.Trace.WriteLine("Length = " + data.Data.Length);
            System.Diagnostics.Trace.WriteLine("Address = " + data.Address);
            System.Diagnostics.Trace.WriteLine("Type = " + data.Type);

            Assert.AreEqual(16, data.Data.Length);
            Assert.AreEqual(0x0100, data.Address);
            Assert.AreEqual(IntelHexFormat.RecordType.Data, data.Type);
            Assert.AreEqual(answer, line);
        }

        [TestMethod]
        public void TestMethod3()
        {
            string payload = "02000004CAFE";
            string line = ":" + payload + MakeCheckSum(payload);

            var data = IntelHexFormat.DecodeLine(line);
            var answer = IntelHexFormat.EncodeData(data);

            System.Diagnostics.Trace.WriteLine("Length = " + data.Data.Length);
            System.Diagnostics.Trace.WriteLine("Address = " + data.Address);
            System.Diagnostics.Trace.WriteLine("Type = " + data.Type);

            Assert.AreEqual(2, data.Data.Length);
            Assert.AreEqual(0, data.Address);
            Assert.AreEqual(IntelHexFormat.RecordType.ExtendedLinearAddress, data.Type);
            Assert.AreEqual(answer, line);
        }

        [TestMethod]
        public void TestMethod4()
        {
            string payload = "02FFFF04CAFE";
            string line = ":" + payload + MakeCheckSum(payload);

            var data = IntelHexFormat.DecodeLine(line);
            var answer = IntelHexFormat.EncodeData(data);

            System.Diagnostics.Trace.WriteLine("Length = " + data.Data.Length);
            System.Diagnostics.Trace.WriteLine("Address = " + data.Address);
            System.Diagnostics.Trace.WriteLine("Type = " + data.Type);

            Assert.AreEqual(2, data.Data.Length);
            Assert.AreEqual(0xFFFF, data.Address);
            Assert.AreEqual(IntelHexFormat.RecordType.ExtendedLinearAddress, data.Type);
            Assert.AreEqual(answer, line);
        }

        [TestMethod]
        public void TestMethod_invalid1()
        {
            string payload = "02000004CAFE";
            string line = payload + MakeCheckSum(payload);

            var data = IntelHexFormat.DecodeLine(line);

            System.Diagnostics.Trace.WriteLine("Length = " + data.Data.Length);
            System.Diagnostics.Trace.WriteLine("Address = " + data.Address);
            System.Diagnostics.Trace.WriteLine("Type = " + data.Type);

            Assert.AreEqual(0, data.Data.Length);
            Assert.AreEqual(0, data.Address);
            Assert.AreEqual(IntelHexFormat.RecordType.Undefined, data.Type);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Must be Failed")]
        public void TestMethod_invalid2()
        {
            string payload = "02000004CA";
            string line = ":" + payload + MakeCheckSum(payload);

            var data = IntelHexFormat.DecodeLine(line);

            System.Diagnostics.Trace.WriteLine("Length = " + data.Data.Length);
            System.Diagnostics.Trace.WriteLine("Address = " + data.Address);
            System.Diagnostics.Trace.WriteLine("Type = " + data.Type);

            Assert.AreEqual(0, data.Data.Length);
            Assert.AreEqual(0, data.Address);
            Assert.AreEqual(IntelHexFormat.RecordType.Undefined, data.Type);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Must be Failed")]
        public void TestMethod_invalid3()
        {
            string payload = "01000004CA";
            string line = ":" + payload + ":" + MakeCheckSum(payload);

            var data = IntelHexFormat.DecodeLine(line);

            System.Diagnostics.Trace.WriteLine("Length = " + data.Data.Length);
            System.Diagnostics.Trace.WriteLine("Address = " + data.Address);
            System.Diagnostics.Trace.WriteLine("Type = " + data.Type);

            Assert.AreEqual(0, data.Data.Length);
            Assert.AreEqual(0, data.Address);
            Assert.AreEqual(IntelHexFormat.RecordType.Undefined, data.Type);

        }

        [TestMethod]
        public void TestMethod_Write_TouchingSections_NoDuplicateLine()
        {
            // Two sections whose ranges touch exactly at 0x0110: [0x100,0x110) and [0x110,0x120).
            var originalRecord = new IntelHexFormat.RawData(0x110, IntelHexFormat.RecordType.Data, new byte[0x10]);
            string[] originalLines = new string[] { IntelHexFormat.EncodeData(originalRecord) };

            var image = new FirmwareImage();
            image.Sections.Add(new Section
            {
                StartAddress = 0x100,
                Bytes = new List<byte>(new byte[0x10])
            });
            image.Sections.Add(new Section
            {
                StartAddress = 0x110,
                Bytes = Enumerable.Repeat((byte)0xAB, 0x10).ToList()
            });

            var result = IntelHexFormat.Write(originalLines, image);

            Assert.AreEqual(1, result.Count, "Address 0x110 must resolve to exactly one owning section, not both.");

            var decoded = IntelHexFormat.DecodeLine(result[0]);
            Assert.AreEqual(0x110, decoded.Address);
            Assert.IsTrue(decoded.Data.All(b => b == 0xAB), "Line at 0x110 must reflect the second section's data.");
        }

    }
}
