using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirmwareImageFormat
{
    public class SrecFormat
    {
        private readonly static string TAG = "Srec";

        public enum RecordType : int
        {
            NotData = -1,
            S1 = 1,
            S2 = 2,
            S3 = 3
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

            internal static readonly RawData Empty = new RawData(0, RecordType.NotData, new byte[0]);
        }

        public static RawData DecodeLine(string line)
        {
            if (string.IsNullOrEmpty(line))
                return RawData.Empty;

            // A line that isn't S-record shaped just isn't ours to decode — report it
            // quietly instead of throwing, so format autodetection can fall through to
            // another format without relying on exceptions as control flow.
            if (line.Length < 2 || line.Substring(0, 1) != "S")
                return RawData.Empty;

            int typeNum;
            var buffer = new List<byte>();
            int cs = 0;
            try
            {
                typeNum = int.Parse(line.Substring(1, 1), System.Globalization.NumberStyles.HexNumber);

                var payload = line.Remove(0, 2);
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

            if ((cs & 0xFF) != 0xFF)
                throw new ArgumentException(TAG + ": " + "Failed Checking sum");

            if (buffer.Count != (1 + buffer[0]))
                throw new ArgumentException(TAG + ": " + "Invalid record length");

            RecordType type = (RecordType)typeNum;

            switch (type)
            {
                case RecordType.S1:
                    return new RawData(
                        (UInt32)((buffer[1] << 8) + buffer[2]),
                        type,
                        buffer.Skip(3).Take(buffer.Count - 4).ToArray());

                case RecordType.S2:
                    return new RawData(
                        (UInt32)((buffer[1] << 16) + (buffer[2] << 8) + buffer[3]),
                        type,
                        buffer.Skip(4).Take(buffer.Count - 5).ToArray());

                case RecordType.S3:
                    return new RawData(
                        (UInt32)((buffer[1] << 24) + (buffer[2] << 16) + (buffer[3] << 8) + buffer[4]),
                        type,
                        buffer.Skip(5).Take(buffer.Count - 6).ToArray());

                default:
                    return RawData.Empty;
            }
        }

        public static string EncodeData(RawData input)
        {
            string payload = string.Empty;

            switch (input.Type)
            {
                case RecordType.S1:
                    payload = input.Address.ToString("X4");
                    if (payload.Length > 4)
                        throw new ArgumentException("Address is overflowed");
                    break;

                case RecordType.S2:
                    payload = input.Address.ToString("X6");
                    if (payload.Length > 6)
                        throw new ArgumentException("Address is overflowed");
                    break;

                case RecordType.S3:
                    payload = input.Address.ToString("X8");
                    if (payload.Length > 8)
                        throw new ArgumentException("Address is overflowed");
                    break;

                default:
                    break;

            }

            foreach (var item in input.Data)
                payload += item.ToString("X2");

            string byteCountText = (payload.Length / 2 + 1).ToString("X2");
            payload = byteCountText + payload;

            int cs = 0;
            for (int i = 0; i < payload.Length; i += 2)
                cs += int.Parse(payload.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);

            cs = cs & 0xFF;
            cs = 0xFF - cs;
            cs = cs & 0xFF;

            payload += cs.ToString("X2");

            string line = string.Empty;
            var tmp = (int)input.Type;
            line += tmp.ToString("X1");


            line += payload;

            return "S" + line;
        }

        public static FirmwareImage Read(string[] textArray)
        {
            var image = new FirmwareImage();
            FormatType format = FormatType.Unknown;

            foreach (string text in textArray)
            {
                var output = DecodeLine(text);
                if (output.Data.Length <= 0)
                    continue;

                switch (output.Type)
                {
                    case RecordType.S1:
                        if (format == FormatType.Unknown)
                            format = FormatType.S1Record;
                        else if (format != FormatType.S1Record)
                            format = FormatType.CombinedSRecord;
                        break;

                    case RecordType.S2:
                        if (format == FormatType.Unknown)
                            format = FormatType.S2Record;
                        else if (format != FormatType.S2Record)
                            format = FormatType.CombinedSRecord;
                        break;

                    case RecordType.S3:
                        if (format == FormatType.Unknown)
                            format = FormatType.S3Record;
                        else if (format != FormatType.S3Record)
                            format = FormatType.CombinedSRecord;
                        break;

                    default:
                        continue;

                }

                FirmwareImageBuilder.AppendOrMerge(image, output.Address, output.Data);
            }

            if (image.Sections.Count != 0)
                image.Format = format;

            return image;

        }

        /// <summary>
        /// Generates a complete Motorola S-Record file from scratch (for new files with no original RawText).
        /// Supports S1Record (16-bit address) and S3Record (32-bit address).
        /// </summary>
        public static List<string> Generate(FirmwareImage image, FormatType format)
        {
            const int bytesPerLine = 16;
            var lines = new List<string>();

            // S0 header record (empty module name, address=0x0000)
            // Byte count = 3 (2 address bytes + 1 checksum), checksum = ~(0x03+0x00+0x00) & 0xFF = 0xFC
            lines.Add("S0030000FC");

            RecordType dataType = (format == FormatType.S3Record) ? RecordType.S3 : RecordType.S1;

            foreach (var section in image.Sections)
            {
                int offset = 0;
                while (offset < section.Bytes.Count)
                {
                    int count = Math.Min(bytesPerLine, section.Bytes.Count - offset);
                    long address = section.StartAddress + offset;

                    var data = new RawData(address, dataType, section.Bytes.Skip(offset).Take(count).ToArray());
                    lines.Add(EncodeData(data));

                    offset += count;
                }
            }

            // End record: S9 for S1 (16-bit), S7 for S3 (32-bit)
            // S9: byte count=03, address=0x0000, checksum=0xFC  → S9030000FC
            // S7: byte count=05, address=0x00000000, checksum=0xFA → S70500000000FA
            if (format == FormatType.S3Record)
                lines.Add("S70500000000FA");
            else
                lines.Add("S9030000FC");

            return lines;
        }

        public static List<string> Write(string[] originalTextArray, FirmwareImage image)
        {
            List<string> texts = new List<string>();

            foreach (string text in originalTextArray)
            {
                string line = text;
                var decoded = DecodeLine(line);

                if (decoded.Type != RecordType.NotData)
                {
                    var owner = FirmwareImageLookup.FindOwningSection(image.Sections, decoded.Address);
                    if (owner != null)
                    {
                        var offset = (int)(decoded.Address - owner.StartAddress);
                        var newData = new byte[decoded.Data.Length];
                        Array.Copy(owner.Bytes.Skip(offset).Take(newData.Length).ToArray(), newData, newData.Length);
                        line = EncodeData(new RawData(decoded.Address + image.OffsetAddress, decoded.Type, newData));
                    }
                    // else: no owning section for this address -> leave `line` as the original text
                }

                texts.Add(line);
            }

            return texts;

        }

    }

}
