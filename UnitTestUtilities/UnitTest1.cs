using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using FirmwareImageFormat;

namespace UnitTestUtilities
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var image = new FirmwareImage();

            var section1 = new Section();
            section1.StartAddress = 0;
            section1.Bytes = new List<byte> { 0x00, 0x01, 0x02 };
            image.Sections.Add(section1);

            var section2 = new Section();
            section2.StartAddress = 4;
            section2.Bytes = new List<byte> { 0x04, 0x05, 0x06, 0x07 };
            image.Sections.Add(section2);

            var exceptedBytes = new List<byte>();
            exceptedBytes.AddRange(section1.Bytes);
            exceptedBytes.Add(0xFF);
            exceptedBytes.AddRange(section2.Bytes);

            long blockSize = 4;

            var combinedImage = FirmwareImage.Pad(image, blockSize);

            Assert.AreEqual(combinedImage.Sections.Count, 1);
            CollectionAssert.AreEqual(exceptedBytes, combinedImage.Sections[0].Bytes);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var image = new FirmwareImage();

            var section1 = new Section();
            section1.StartAddress = 0;
            section1.Bytes = new List<byte> { 0x00, 0x01, 0x02 };
            image.Sections.Add(section1);

            var section2 = new Section();
            section2.StartAddress = 4;
            section2.Bytes = new List<byte> { 0x04, 0x05, 0x06, 0x07 };
            image.Sections.Add(section2);

            long entireSize = 128;

            var combinedImage = FirmwareImage.Fill(image, entireSize);

            Assert.AreEqual(combinedImage.Sections.Count, 1);
            Assert.AreEqual(entireSize, combinedImage.Sections[0].Bytes.Count);
        }

        [TestMethod]
        public void TestMethod_RomImageMergeAndAlign()
        {
            var image = new FirmwareImage();
            // sec1: start=0x00090100, len=11766
            var sec1 = new Section();
            sec1.StartAddress = 0x00090100;
            sec1.Bytes = new List<byte>(new byte[11766]);
            // Insert markers at the head and tail for identification
            sec1.Bytes[0] = 0xA1;
            sec1.Bytes[11765] = 0xA2;
            image.Sections.Add(sec1);
            // sec2: start=0x00092EF8, len=28
            var sec2 = new Section();
            sec2.StartAddress = 0x00092EF8;
            sec2.Bytes = new List<byte>(new byte[28]);
            sec2.Bytes[0] = 0xB1;
            sec2.Bytes[27] = 0xB2;
            image.Sections.Add(sec2);
            // sec3: start=0x0010FFFC, len=4
            var sec3 = new Section();
            sec3.StartAddress = 0x0010FFFC;
            sec3.Bytes = new List<byte> { 0xC1, 0xC2, 0xC3, 0xC4 };
            image.Sections.Add(sec3);
            long blockSize = 0x100;
            var result = FirmwareImage.Pad(image, blockSize);
            // --- Verify merge results ---
            Assert.AreEqual(2, result.Sections.Count, "sec1+sec2 merged, sec3 independent: 2 sections total");
            // --- Factor[0]: sec1 + gap(2) + sec2 ---
            var f0 = result.Sections[0];
            Assert.AreEqual(0x00090100L, f0.StartAddress, "sec1 is already aligned, so start address unchanged");
            Assert.AreEqual(11766 + 2 + 28, f0.Bytes.Count, "sec1(11766) + gap pad(2) + sec2(28) = 11796");
            // Verify sec1 markers
            Assert.AreEqual(0xA1, f0.Bytes[0], "sec1 head marker");
            Assert.AreEqual(0xA2, f0.Bytes[11765], "sec1 tail marker");
            // Verify gap padding
            Assert.AreEqual(0xFF, f0.Bytes[11766], "gap padding byte 1");
            Assert.AreEqual(0xFF, f0.Bytes[11767], "gap padding byte 2");
            // Verify sec2 markers
            Assert.AreEqual(0xB1, f0.Bytes[11768], "sec2 head marker");
            Assert.AreEqual(0xB2, f0.Bytes[11795], "sec2 tail marker");
            // --- Factor[1]: sec3 after alignment rollback ---
            var f1 = result.Sections[1];
            Assert.AreEqual(0x0010FF00L, f1.StartAddress, "0x10FFFC → aligned back to 0x10FF00");
            Assert.AreEqual(252 + 4, f1.Bytes.Count, "headPad(252) + original data(4) = 256");
            // Verify head padding
            for (int i = 0; i < 252; i++)
            {
                Assert.AreEqual(0xFF, f1.Bytes[i], $"head padding [{i}]");
            }
            // Verify sec3 original data
            Assert.AreEqual(0xC1, f1.Bytes[252]);
            Assert.AreEqual(0xC2, f1.Bytes[253]);
            Assert.AreEqual(0xC3, f1.Bytes[254]);
            Assert.AreEqual(0xC4, f1.Bytes[255]);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestMethod_Fill_NonConvergent_Throws()
        {
            var image = new FirmwareImage();

            // Two sections too far apart to merge within entireSize -> Pad will leave 2
            // sections, which Fill cannot converge to a single block.
            var section1 = new Section { StartAddress = 0, Bytes = new List<byte> { 0x00 } };
            image.Sections.Add(section1);

            var section2 = new Section { StartAddress = 0x1000, Bytes = new List<byte> { 0x01 } };
            image.Sections.Add(section2);

            FirmwareImage.Fill(image, 0x100);
        }

        [TestMethod]
        public void TestMethod_TryFill_NonConvergent_ReturnsFalse()
        {
            var image = new FirmwareImage();
            image.Sections.Add(new Section { StartAddress = 0, Bytes = new List<byte> { 0x00 } });
            image.Sections.Add(new Section { StartAddress = 0x1000, Bytes = new List<byte> { 0x01 } });

            bool succeeded = FirmwareImage.TryFill(image, 0x100, out var result);

            Assert.IsFalse(succeeded);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void TestMethod_FirmwareImageReader_TryRead_IntelHex_SucceedsEvenThoughSrecTriedFirst()
        {
            // End-to-end regression test for the format-autodetection bug: FirmwareImageReader
            // always tries S-record first, but must still succeed on a real Intel-Hex file
            // instead of aborting.
            var dataRecord = new IntelHexFormat.RawData(
                0x0100, IntelHexFormat.RecordType.Data,
                new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
            var eofRecord = new IntelHexFormat.RawData(0, IntelHexFormat.RecordType.EndOfFile, new byte[0]);
            string[] lines = new string[]
            {
                IntelHexFormat.EncodeData(dataRecord),
                IntelHexFormat.EncodeData(eofRecord),
            };

            bool succeeded = FirmwareImageReader.TryRead(lines, out var image, out var format);

            Assert.IsTrue(succeeded);
            Assert.AreEqual(FormatType.IntelHexLinear, format);
            Assert.AreEqual(1, image.Sections.Count);
            Assert.AreEqual(0x0100, image.Sections[0].StartAddress);
            CollectionAssert.AreEqual(
                new List<byte> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 },
                image.Sections[0].Bytes);
        }

        [TestMethod]
        public void TestMethod_FirmwareImageReader_TryRead_SRecord_Succeeds()
        {
            string[] lines = new string[]
            {
                "S1137AF00A0A0D0000000000000000000000000061",
            };

            bool succeeded = FirmwareImageReader.TryRead(lines, out var image, out var format);

            Assert.IsTrue(succeeded);
            Assert.AreEqual(FormatType.S1Record, format);
            Assert.AreEqual(1, image.Sections.Count);
        }

        [TestMethod]
        public void TestMethod_FirmwareImageReader_TryRead_UnrecognizedInput_ReturnsFalse()
        {
            string[] lines = new string[] { "this is not a valid firmware image line" };

            bool succeeded = FirmwareImageReader.TryRead(lines, out var image, out var format);

            Assert.IsFalse(succeeded);
            Assert.AreEqual(FormatType.Unknown, format);
        }

    }
}
