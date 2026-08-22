using System.Collections.Generic;

namespace FirmwareImageFormat
{
    /// <summary>
    /// Shared "append or merge" logic for building a FirmwareImage from a stream of
    /// (address, bytes) records, used by both IntelHexFormat.Read and SrecFormat.Read.
    /// </summary>
    internal static class FirmwareImageBuilder
    {
        public static void AppendOrMerge(FirmwareImage image, long address, byte[] data)
        {
            if (data == null || data.Length == 0)
                return;

            if (image.Sections.Count == 0)
            {
                image.Sections.Add(new Section { StartAddress = address, Bytes = new List<byte>(data) });
                return;
            }

            var last = image.Sections[image.Sections.Count - 1];
            var nextAddress = last.StartAddress + last.Bytes.Count;

            if (nextAddress == address)
                last.Bytes.AddRange(data);
            else
                image.Sections.Add(new Section { StartAddress = address, Bytes = new List<byte>(data) });
        }
    }

    /// <summary>
    /// Shared address-to-section lookup used by both IntelHexFormat.Write and SrecFormat.Write.
    /// </summary>
    internal static class FirmwareImageLookup
    {
        /// <summary>
        /// Finds the section that owns the given address, using the half-open range
        /// [StartAddress, StartAddress + Bytes.Count). Returns null if no section owns it.
        /// </summary>
        public static Section FindOwningSection(List<Section> sections, long address)
        {
            foreach (var section in sections)
            {
                if (address >= section.StartAddress && address < section.StartAddress + section.Bytes.Count)
                    return section;
            }
            return null;
        }
    }
}
