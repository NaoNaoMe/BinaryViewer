using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirmwareImageFormat
{
    public class IntelHexFormat
    {
        private readonly static string TAG = "IntelHex";

        public enum RecordType : int
        {
            Undefined = -1,

            /// <summary>
            /// Contains data and a 16-bit starting address for the data.
            /// The byte count specifies the number of data bytes in the record.
            /// </summary>
            Data = 0,

            /// <summary>
            /// Must occur exactly once per file, in the last line of the file.
            /// The data field is empty (byte count 00) and the address field is typically 0000.
            /// </summary>
            EndOfFile = 1,

            /// <summary>
            /// Data field contains a 16-bit segment base address (byte count 02), compatible with
            /// 80x86 real mode addressing. The address field (typically 0000) is ignored. The segment
            /// address from the most recent record is multiplied by 16 and added to each subsequent
            /// data record's address, allowing up to one megabyte of address space.
            /// </summary>
            ExtendedSegmentAddress = 2,

            /// <summary>
            /// For 80x86 processors: specifies the initial content of the CS:IP registers.
            /// Address field is 0000, byte count is 04 (first two bytes CS, latter two IP).
            /// </summary>
            StartSegmentAddress = 3,

            /// <summary>
            /// Allows 32-bit addressing (up to 4GiB). Address field is ignored (typically 0000) and
            /// byte count is always 02. The two big-endian data bytes specify the upper 16 bits of
            /// the 32-bit absolute address for all subsequent Data records, until the next record of
            /// this type. If none precedes a Data record, the upper 16 bits default to 0000.
            /// </summary>
            ExtendedLinearAddress = 4,

            /// <summary>
            /// Address field is 0000 (unused), byte count is 04. The four data bytes represent the
            /// 32-bit value loaded into the EIP register of the 80386 and higher CPU.
            /// </summary>
            StartLinearAddress = 5
        }

        public readonly struct RawData
        {
            public long Address { get; }
            public RecordType Type { get; }
            public byte[] Data { get; }

            public RawData(long address, RecordType type, byte[] data)
            {
                Address = address;
                Type = type;
                Data = data ?? new byte[0];
            }

            internal static readonly RawData Empty = new RawData(0, RecordType.Undefined, new byte[0]);
        }

        public static RawData DecodeLine(string line)
        {
            if (string.IsNullOrEmpty(line))
                return RawData.Empty;

            if (line.Substring(0, 1) != ":")
                return RawData.Empty;

            var payload = line.Remove(0, 1);

            var buffer = new List<byte>();
            int cs = 0;
            try
            {
                for (int i = 0; i < (payload.Length / 2); i++)
                {
                    var tmp = byte.Parse(payload.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
                    buffer.Add(tmp);
                    cs += tmp;
                }
            }
            catch (FormatException fe)
            {
                throw new ArgumentException(TAG + ": " + "Invalid hex characters in record", fe);
            }

            if ((cs & 0xFF) != 0x00)
                throw new ArgumentException(TAG + ": " + "Failed Checking sum");

            int length = buffer[0];

            if (buffer.Count != (1 + 2 + 1 + length + 1))
                throw new ArgumentException(TAG + ": " + "Invalid record length");

            long address = (UInt32)((buffer[1] << 8) + buffer[2]);
            RecordType type = (RecordType)buffer[3];
            byte[] data = buffer.Skip(4).Take(length).ToArray();

            return new RawData(address, type, data);
        }

        public static string EncodeData(RawData input)
        {
            string line = string.Empty;
            line += input.Data.Length.ToString("X2");
            line += input.Address.ToString("X4");
            var tmp = (int)input.Type;
            line += tmp.ToString("X2");
            foreach (var item in input.Data)
                line += item.ToString("X2");

            int cs = 0;
            for (int i = 0; i < line.Length; i += 2)
                cs += int.Parse(line.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);

            cs = cs & 0xFF;
            cs = 0xFF - cs;
            cs = (cs + 1) & 0xFF;

            line += cs.ToString("X2");

            return ":" + line;
        }

        public static FirmwareImage Read(string[] textArray)
        {
            var image = new FirmwareImage();
            FormatType format = FormatType.Unknown;

            long extendedAddress = 0;

            foreach (string text in textArray)
            {
                var output = DecodeLine(text);
                if (output.Data.Length <= 0)
                    continue;

                bool isBreak = false;
                switch (output.Type)
                {
                    case RecordType.Data:
                        var address = extendedAddress + output.Address;
                        FirmwareImageBuilder.AppendOrMerge(image, address, output.Data);
                        break;

                    case RecordType.EndOfFile:
                        isBreak = true;
                        break;

                    case RecordType.ExtendedSegmentAddress:
                        if (format == FormatType.Unknown)
                            format = FormatType.IntelHexExSegment;
                        else if (format != FormatType.IntelHexExSegment)
                            format = FormatType.IntelHexCombined;

                        extendedAddress = (UInt32)(((output.Data[0] << 8) + output.Data[1]) * 16);

                        break;

                    case RecordType.StartSegmentAddress:
                        break;

                    case RecordType.ExtendedLinearAddress:
                        if (format == FormatType.Unknown)
                            format = FormatType.IntelHexExLinear;
                        else if (format != FormatType.IntelHexExLinear)
                            format = FormatType.IntelHexCombined;

                        extendedAddress = (UInt32)(((output.Data[0] << 8) + output.Data[1]) * 65536);

                        break;

                    case RecordType.StartLinearAddress:
                        break;

                    default:
                        break;

                }

                if (isBreak)
                    break;
            }


            if (image.Sections.Count != 0)
            {
                if (format == FormatType.Unknown)
                    format = FormatType.IntelHexLinear;
                image.Format = format;
            }

            return image;
        }

        /// <summary>
        /// Generates a complete Intel HEX file from scratch (for new files with no original RawText).
        /// Supports IntelHexLinear (16-bit) and IntelHexExLinear (32-bit Extended Linear Address).
        /// </summary>
        public static List<string> Generate(FirmwareImage image, FormatType format)
        {
            const int bytesPerLine = 16;
            var lines = new List<string>();

            long currentUpper = -1;

            foreach (var section in image.Sections)
            {
                int offset = 0;
                while (offset < section.Bytes.Count)
                {
                    int count = Math.Min(bytesPerLine, section.Bytes.Count - offset);
                    long absoluteAddress = section.StartAddress + offset;

                    // Emit Extended Linear Address record when the upper 16 bits change
                    if (format == FormatType.IntelHexExLinear)
                    {
                        long upper = (absoluteAddress >> 16) & 0xFFFF;
                        if (upper != currentUpper)
                        {
                            currentUpper = upper;
                            var ela = new RawData(0, RecordType.ExtendedLinearAddress,
                                new byte[] { (byte)(upper >> 8), (byte)(upper & 0xFF) });
                            lines.Add(EncodeData(ela));
                        }
                    }

                    var data = new RawData(
                        absoluteAddress & 0xFFFF, // lower 16 bits
                        RecordType.Data,
                        section.Bytes.Skip(offset).Take(count).ToArray());
                    lines.Add(EncodeData(data));

                    offset += count;
                }
            }

            // EOF record
            var eof = new RawData(0, RecordType.EndOfFile, new byte[0]);
            lines.Add(EncodeData(eof));

            return lines;
        }

        public static List<string> Write(string[] originalTextArray, FirmwareImage image)
        {
            FormatType format = FormatType.Unknown;

            long extendedAddress = 0;
            long modifiedAddress = 0;

            List<string> outputList = new List<string>();

            foreach (string text in originalTextArray)
            {
                string line = text;
                var output = DecodeLine(line);

                switch (output.Type)
                {
                    case RecordType.Data:
                        var address = extendedAddress + output.Address;

                        var owner = FirmwareImageLookup.FindOwningSection(image.Sections, address);
                        if (owner != null)
                        {
                            var offset = (int)(address - owner.StartAddress);
                            var newData = new byte[output.Data.Length];
                            Array.Copy(owner.Bytes.Skip(offset).Take(newData.Length).ToArray(), newData, newData.Length);
                            line = EncodeData(new RawData(output.Address, output.Type, newData));
                        }
                        // else: no owning section for this address -> leave `line` as the original text

                        break;

                    case RecordType.EndOfFile:
                        break;

                    case RecordType.ExtendedSegmentAddress:
                        if (format == FormatType.Unknown)
                            format = FormatType.IntelHexExSegment;
                        else if (format != FormatType.IntelHexExSegment)
                            throw new ArgumentException(TAG + ": " + "Extended Segment Address Error");

                        extendedAddress = (UInt32)(((output.Data[0] << 8) + output.Data[1]) * 16);

                        modifiedAddress = extendedAddress + image.OffsetAddress;
                        modifiedAddress = modifiedAddress / 16;
                        output.Data[0] = (byte)((modifiedAddress >> 8) & 0xFF);
                        output.Data[1] = (byte)((modifiedAddress) & 0xFF);
                        line = EncodeData(output);

                        break;

                    case RecordType.StartSegmentAddress:
                        break;

                    case RecordType.ExtendedLinearAddress:
                        if (format == FormatType.Unknown)
                            format = FormatType.IntelHexExLinear;
                        else if (format != FormatType.IntelHexExLinear)
                            throw new ArgumentException(TAG + ": " + "Extended Linear Address Error");

                        extendedAddress = (UInt32)(((output.Data[0] << 8) + output.Data[1]) * 65536);

                        modifiedAddress = extendedAddress + image.OffsetAddress;
                        modifiedAddress = modifiedAddress / 65536;
                        output.Data[0] = (byte)((modifiedAddress >> 8) & 0xFF);
                        output.Data[1] = (byte)((modifiedAddress) & 0xFF);
                        line = EncodeData(output);

                        break;

                    case RecordType.StartLinearAddress:
                        break;

                    default:
                        break;

                }

                outputList.Add(line);
            }

            return outputList;

        }

    }

}
