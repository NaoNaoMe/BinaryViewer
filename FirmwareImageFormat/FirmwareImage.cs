using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirmwareImageFormat
{
    public enum FormatType : int
    {
        Unknown = 0,
        IntelHexLinear,
        IntelHexExLinear,
        IntelHexExSegment,
        IntelHexCombined,
        S1Record,
        S2Record,
        S3Record,
        CombinedSRecord,
        Binary
    }

    public class Section
    {
        public long StartAddress { set; get; }
        public List<byte> Bytes { set; get; }

        public Section()
        {
            StartAddress = 0;
            Bytes = new List<byte>();

        }

        public Section(Section other)
        {
            StartAddress = other.StartAddress;
            Bytes = new List<byte>(other.Bytes);

        }

    }


    public class FirmwareImage
    {
        public FormatType Format { get; set; }
        public long OffsetAddress { set; get; }
        public List<Section> Sections { get; set; }

        public FirmwareImage()
        {
            Format = FormatType.Unknown;
            OffsetAddress = 0;
            Sections = new List<Section>();
        }

        public FirmwareImage(FirmwareImage other)
        {
            Format = other.Format;
            OffsetAddress = other.OffsetAddress;
            Sections = new List<Section>(other.Sections);

        }

        public static FirmwareImage Pad(FirmwareImage image, long blockSize, byte padValue = 0xFF)
        {
            // --- Validate blockSize: must be a power of 2 ---
            if (blockSize <= 0 || (blockSize & (blockSize - 1)) != 0)
            {
                throw new ArgumentException(
                    $"blockSize must be a power of 2, but was {blockSize} (0x{blockSize:X}).",
                    nameof(blockSize));
            }
            if (image.Sections == null || image.Sections.Count == 0)
            {
                return new FirmwareImage
                {
                    Format = image.Format,
                    Sections = new List<Section>()
                };
            }
            var localImage = new FirmwareImage
            {
                Format = image.Format,
                Sections = new List<Section>()
            };
            // --- Phase 1: Merge adjacent sections ---
            // If the gap between the end of the previous section and the start of the next is within blockSize, merge them with padding
            Section working = null;
            foreach (var item in image.Sections)
            {
                if (working == null)
                {
                    working = new Section(item);
                    continue;
                }
                long workingEnd = working.StartAddress + working.Bytes.Count;
                long gap = item.StartAddress - workingEnd;
                if (gap < 0)
                {
                    throw new InvalidOperationException(
                        $"Overlapping sections detected: working ends at 0x{workingEnd:X}, " +
                        $"next starts at 0x{item.StartAddress:X}.");
                }
                if (gap <= blockSize)
                {
                    // Fill the gap with padding and merge
                    for (long i = 0; i < gap; i++)
                        working.Bytes.Add(padValue);
                    working.Bytes.AddRange(item.Bytes);
                }
                else
                {
                    // Sections are too far apart; finalize working and start a new one
                    localImage.Sections.Add(working);
                    working = new Section(item);
                }
            }
            // Add the last working section
            if (working != null)
            {
                localImage.Sections.Add(working);
            }
            // --- Phase 2: Align each section's start address down to a blockSize boundary ---
            long alignMask = blockSize - 1; // e.g. blockSize=0x100 → alignMask=0xFF
            for (int idx = 0; idx < localImage.Sections.Count; idx++)
            {
                var section = localImage.Sections[idx];
                // Round down the start address to the alignment boundary
                // The target device expects the start address to be a power of 2
                long alignedStart = section.StartAddress & ~alignMask;
                long headPadding = section.StartAddress - alignedStart;
                if (headPadding > 0)
                {
                    // Insert padding at the head
                    var padBytes = new byte[headPadding];
                    for (int i = 0; i < headPadding; i++)
                        padBytes[i] = padValue;
                    section.Bytes.InsertRange(0, padBytes);
                    section.StartAddress = alignedStart;
                }
                // Tail is not extended to the alignment boundary
                // This is to reduce the amount of data transferred
            }
            return localImage;
        }

        public static FirmwareImage Fill(FirmwareImage image, long entireSize, byte padValue = 0xFF)
        {
            var combinedImage = Pad(image, entireSize, padValue);

            if (combinedImage.Sections.Count() != 1)
                throw new InvalidOperationException(
                    $"Fill requires exactly one contiguous block within entireSize=0x{entireSize:X}, but {combinedImage.Sections.Count()} block(s) remained.");

            var remaining = entireSize - combinedImage.Sections[0].Bytes.Count;

            for (int i = 0; i < remaining; i++)
                combinedImage.Sections[0].Bytes.Add(padValue);

            return combinedImage;
        }

        /// <summary>
        /// Try-pattern wrapper for Fill, for callers that want a clean failure
        /// path instead of catching InvalidOperationException.
        /// </summary>
        public static bool TryFill(FirmwareImage image, long entireSize, out FirmwareImage result, byte padValue = 0xFF)
        {
            try
            {
                result = Fill(image, entireSize, padValue);
                return true;
            }
            catch (InvalidOperationException)
            {
                result = null;
                return false;
            }
        }

    }

}
