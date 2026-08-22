using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FirmwareImageFormat;

namespace UnitTestSrecFormat
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            string line = "S1137AF00A0A0D0000000000000000000000000061";

            var data = SrecFormat.DecodeLine(line);
            var answer = SrecFormat.EncodeData(data);

            System.Diagnostics.Trace.WriteLine("Length = " + data.Data.Length);
            System.Diagnostics.Trace.WriteLine("Address = " + data.Address);
            System.Diagnostics.Trace.WriteLine("Type = " + data.Type);

            Assert.AreEqual((0x13 - 2 - 1), data.Data.Length);
            Assert.AreEqual(0x7AF0, data.Address);
            Assert.AreEqual(SrecFormat.RecordType.S1, data.Type);
            Assert.AreEqual(answer, line);
        }

        [TestMethod]
        public void TestMethod2()
        {
            string line = "S313FFFC000054657374526F6D5F415F323033008F";

            var data = SrecFormat.DecodeLine(line);
            var answer = SrecFormat.EncodeData(data);

            System.Diagnostics.Trace.WriteLine("Length = " + data.Data.Length);
            System.Diagnostics.Trace.WriteLine("Address = " + data.Address);
            System.Diagnostics.Trace.WriteLine("Type = " + data.Type);

            Assert.AreEqual((0x13 - 4 - 1), data.Data.Length);
            Assert.AreEqual(0xFFFC0000, data.Address);
            Assert.AreEqual(SrecFormat.RecordType.S3, data.Type);
            Assert.AreEqual(answer, line);
        }

        [TestMethod]
        public void TestMethod3()
        {
            string[] lines = new string[] { 
                "S1137AF00A0A0D0000000000000000000000000061" ,
                "S313FFFC000054657374526F6D5F415F323033008F"
            };

            var image = SrecFormat.Read(lines);

            System.Diagnostics.Trace.WriteLine("Count = " + image.Sections.Count);

            Assert.AreEqual(FormatType.CombinedSRecord, image.Format);
        }

        [TestMethod]
        public void TestMethod_DecodeLine_IntelHexPrefix_ReturnsNotDataWithoutThrowing()
        {
            // Regression test for the format-autodetection bug: feeding an Intel-Hex line
            // into SrecFormat.DecodeLine must NOT throw (it used to throw "Missing srec mark"),
            // so that "try S-record, then try Intel-Hex" autodetection can fall through cleanly.
            string line = ":10010000" + "00000000000000000000000000000000" + "EE";

            var data = SrecFormat.DecodeLine(line);

            Assert.AreEqual(SrecFormat.RecordType.NotData, data.Type);
            Assert.AreEqual(0, data.Data.Length);
        }

        [TestMethod]
        public void TestMethod_Write_TouchingSections_NoDuplicateLine()
        {
            // Two sections whose ranges touch exactly at 0x000110: [0x100,0x110) and [0x110,0x120).
            var originalRecord = new SrecFormat.RawData(0x110, SrecFormat.RecordType.S1, new byte[0x10]);
            string[] originalLines = new string[] { SrecFormat.EncodeData(originalRecord) };

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

            var result = SrecFormat.Write(originalLines, image);

            Assert.AreEqual(1, result.Count, "Address 0x110 must resolve to exactly one owning section, not both.");

            var decoded = SrecFormat.DecodeLine(result[0]);
            Assert.AreEqual(0x110, decoded.Address);
            Assert.IsTrue(decoded.Data.All(b => b == 0xAB), "Line at 0x110 must reflect the second section's data.");
        }

        [TestMethod]
        public void TestMethod_Write_UnmatchedAddress_PreservesOriginalLine()
        {
            // Deliberate behavior fix: a data line whose address matches no section must be
            // preserved as-is (matching IntelHexFormat.Write), not silently dropped.
            var originalRecord = new SrecFormat.RawData(0x9999, SrecFormat.RecordType.S1, new byte[] { 0x01, 0x02 });
            string originalLine = SrecFormat.EncodeData(originalRecord);

            var image = new FirmwareImage();
            image.Sections.Add(new Section { StartAddress = 0x100, Bytes = new List<byte> { 0xFF } });

            var result = SrecFormat.Write(new string[] { originalLine }, image);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(originalLine, result[0]);
        }

    }
}
