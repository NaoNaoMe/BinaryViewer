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

        // ---- Read: end-of-file record ----

        [TestMethod]
        public void TestMethod_Read_StopsAtEndOfFile()
        {
            // A data record that follows the end-of-file record is not part of the image.
            string[] lines = new string[]
            {
                IntelHexFormat.EncodeData(new IntelHexFormat.RawData(0x0000, IntelHexFormat.RecordType.Data, new byte[] { 1, 2 })),
                IntelHexFormat.EncodeData(new IntelHexFormat.RawData(0, IntelHexFormat.RecordType.EndOfFile, new byte[0])),
                IntelHexFormat.EncodeData(new IntelHexFormat.RawData(0x0100, IntelHexFormat.RecordType.Data, new byte[] { 3 })),
            };

            var image = IntelHexFormat.Read(lines);

            Assert.AreEqual(1, image.Sections.Count, "The record after the end-of-file record must be ignored.");
            Assert.AreEqual(2, image.Sections[0].Bytes.Count);
        }

        // ---- Generate: address extension records and 64 KiB blocks ----

        private static FirmwareImage MakeImage(long startAddress, int length)
        {
            var section = new Section { StartAddress = startAddress };
            for (int i = 0; i < length; i++)
                section.Bytes.Add((byte)(i + 1));

            var image = new FirmwareImage();
            image.Sections.Add(section);
            return image;
        }

        private static void AssertRoundTrip(FirmwareImage expected, List<string> lines, FormatType expectedFormat)
        {
            var actual = IntelHexFormat.Read(lines.ToArray());

            Assert.AreEqual(expectedFormat, actual.Format);
            Assert.AreEqual(expected.Sections.Count, actual.Sections.Count);
            for (int i = 0; i < expected.Sections.Count; i++)
            {
                Assert.AreEqual(expected.Sections[i].StartAddress, actual.Sections[i].StartAddress);
                CollectionAssert.AreEqual(expected.Sections[i].Bytes, actual.Sections[i].Bytes);
            }
        }

        [TestMethod]
        public void TestMethod_Generate_ExLinear_Simple()
        {
            var image = MakeImage(0x100, 3);

            var lines = IntelHexFormat.Generate(image, FormatType.IntelHexExLinear);

            // Extended Linear Address (upper = 0), one data record, end of file.
            Assert.AreEqual(3, lines.Count);
            Assert.AreEqual(":020000040000FA", lines[0]);
            Assert.AreEqual(":00000001FF", lines[2]);
            AssertRoundTrip(image, lines, FormatType.IntelHexExLinear);
        }

        [TestMethod]
        public void TestMethod_Generate_ExLinear_RecordDoesNotCrossBlock()
        {
            // 16 bytes starting 8 bytes before the 64 KiB boundary: the record must be split in two,
            // with a new Extended Linear Address record in between.
            var image = MakeImage(0xFFF8, 16);

            var lines = IntelHexFormat.Generate(image, FormatType.IntelHexExLinear);

            Assert.AreEqual(5, lines.Count);
            Assert.AreEqual(8, IntelHexFormat.DecodeLine(lines[1]).Data.Length);
            Assert.AreEqual(IntelHexFormat.RecordType.ExtendedLinearAddress, IntelHexFormat.DecodeLine(lines[2]).Type);
            Assert.AreEqual(8, IntelHexFormat.DecodeLine(lines[3]).Data.Length);
            Assert.AreEqual(0x0000, IntelHexFormat.DecodeLine(lines[3]).Address);
            AssertRoundTrip(image, lines, FormatType.IntelHexExLinear);
        }

        [TestMethod]
        public void TestMethod_Generate_ExLinear_High32BitAddress()
        {
            var image = MakeImage(0x08000000, 40);

            var lines = IntelHexFormat.Generate(image, FormatType.IntelHexExLinear);

            AssertRoundTrip(image, lines, FormatType.IntelHexExLinear);
        }

        [TestMethod]
        public void TestMethod_Generate_ExSegment_Above64KiB()
        {
            // Before the fix, no Extended Segment Address record was written and the data wrapped around at 64 KiB.
            var image = MakeImage(0x1FFF0, 32);

            var lines = IntelHexFormat.Generate(image, FormatType.IntelHexExSegment);

            Assert.AreEqual(IntelHexFormat.RecordType.ExtendedSegmentAddress, IntelHexFormat.DecodeLine(lines[0]).Type);
            AssertRoundTrip(image, lines, FormatType.IntelHexExSegment);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Must be Failed")]
        public void TestMethod_Generate_ExSegment_Beyond1MiB_Fails()
        {
            // Ends at 0x100008: beyond what segment addressing can reach.
            IntelHexFormat.Generate(MakeImage(0xFFFF8, 16), FormatType.IntelHexExSegment);
        }

        [TestMethod]
        public void TestMethod_Generate_Linear_Within64KiB()
        {
            var image = MakeImage(0xFFF0, 16);

            var lines = IntelHexFormat.Generate(image, FormatType.IntelHexLinear);

            // No extension records: one data record and the end-of-file record.
            Assert.AreEqual(2, lines.Count);
            AssertRoundTrip(image, lines, FormatType.IntelHexLinear);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Must be Failed")]
        public void TestMethod_Generate_Linear_Above64KiB_Fails()
        {
            // Before the fix, the address silently wrapped around to 0x0000.
            IntelHexFormat.Generate(MakeImage(0x10000, 16), FormatType.IntelHexLinear);
        }

    }
}
