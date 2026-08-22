using System;

namespace FirmwareImageFormat
{
    /// <summary>
    /// Centralized format autodetection: tries S-record first, then Intel HEX.
    /// Replaces the duplicated (and previously inconsistent) try/catch dances that
    /// used to live in each consumer of IntelHexFormat.Read / SrecFormat.Read.
    /// </summary>
    public static class FirmwareImageReader
    {
        public static bool TryRead(string[] textArray, out FirmwareImage image, out FormatType format)
        {
            if (TryReadOne(() => SrecFormat.Read(textArray), out image))
            {
                format = image.Format;
                return true;
            }
            if (TryReadOne(() => IntelHexFormat.Read(textArray), out image))
            {
                format = image.Format;
                return true;
            }

            image = new FirmwareImage();
            format = FormatType.Unknown;
            return false;
        }

        private static bool TryReadOne(Func<FirmwareImage> read, out FirmwareImage image)
        {
            try
            {
                image = read();
                return image.Format != FormatType.Unknown;
            }
            catch (ArgumentException)
            {
                // The line prefix matched this format but the content was corrupt
                // (bad checksum/length) — fall through to the next format, exactly
                // like the two-try/catch pattern this replaces.
                image = null;
                return false;
            }
        }
    }
}
