using System;
using BinaryViewer.Controls;

namespace BinaryViewer
{
    /// <summary>
    /// Wraps an InMemoryByteBuffer but reports no Insert/Delete support, so HexBox
    /// refuses resize operations (Delete key, Insert-mode typing) at the key-handling
    /// level itself. WriteByte (normal overwrite editing) passes through unchanged.
    /// </summary>
    internal sealed class SizeLockedByteProvider : IByteProvider
    {
        private readonly InMemoryByteBuffer _inner;

        public SizeLockedByteProvider(InMemoryByteBuffer inner)
        {
            _inner = inner;
        }

        public byte ReadByte(long index) => _inner.ReadByte(index);
        public void WriteByte(long index, byte value) => _inner.WriteByte(index, value);
        public void InsertBytes(long index, byte[] bs) => throw new NotSupportedException();
        public void DeleteBytes(long index, long length) => throw new NotSupportedException();

        public long Length => _inner.Length;

        public event EventHandler LengthChanged
        {
            add { _inner.LengthChanged += value; }
            remove { _inner.LengthChanged -= value; }
        }

        public bool HasChanges() => _inner.HasChanges();
        public void ApplyChanges() => _inner.ApplyChanges();

        public event EventHandler Changed
        {
            add { _inner.Changed += value; }
            remove { _inner.Changed -= value; }
        }

        public bool SupportsWriteByte() => true;
        public bool SupportsInsertBytes() => false;
        public bool SupportsDeleteBytes() => false;
    }
}
