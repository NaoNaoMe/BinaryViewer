using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Windows.Forms.VisualStyles;
using System.Text;
using System.Collections.Generic;
using Windows.Win32;

namespace BinaryViewer.Controls
{
    /// <summary>
    /// Represents a hex box control.
    /// </summary>
    [ToolboxBitmap(typeof(HexBox), "HexBox.bmp")]
    public class HexBox : Control
    {
        #region IKeyInterpreter interface
        /// <summary>
        /// Defines a user input handler such as for mouse and keyboard input
        /// </summary>
        interface IKeyInterpreter
        {
            /// <summary>
            /// Activates mouse events
            /// </summary>
            void Activate();
            /// <summary>
            /// Deactivate mouse events
            /// </summary>
            void Deactivate();
            /// <summary>
            /// Preprocesses WM_KEYUP window message.
            /// </summary>
            /// <param name="m">the Message object to process.</param>
            /// <returns>True, if the message was processed.</returns>
            bool PreProcessWmKeyUp(ref Message m);
            /// <summary>
            /// Preprocesses WM_CHAR window message.
            /// </summary>
            /// <param name="m">the Message object to process.</param>
            /// <returns>True, if the message was processed.</returns>
            bool PreProcessWmChar(ref Message m);
            /// <summary>
            /// Preprocesses WM_KEYDOWN window message.
            /// </summary>
            /// <param name="m">the Message object to process.</param>
            /// <returns>True, if the message was processed.</returns>
            bool PreProcessWmKeyDown(ref Message m);
            /// <summary>
            /// Gives some information about where to place the caret.
            /// </summary>
            /// <param name="byteIndex">the index of the byte</param>
            /// <returns>the position where the caret is to place.</returns>
            PointF GetCaretPointF(long byteIndex);
        }
        #endregion

        #region EmptyKeyInterpreter class
        /// <summary>
        /// Represents an empty input handler without any functionality. 
        /// If is set ByteProvider to null, then this interpreter is used.
        /// </summary>
        class EmptyKeyInterpreter : IKeyInterpreter
        {
            HexBox _hexBox;

            public EmptyKeyInterpreter(HexBox hexBox)
            {
                _hexBox = hexBox;
            }

            #region IKeyInterpreter Members
            public void Activate() { }
            public void Deactivate() { }

            public bool PreProcessWmKeyUp(ref Message m)
            { return _hexBox.BasePreProcessMessage(ref m); }

            public bool PreProcessWmChar(ref Message m)
            { return _hexBox.BasePreProcessMessage(ref m); }

            public bool PreProcessWmKeyDown(ref Message m)
            { return _hexBox.BasePreProcessMessage(ref m); }

            public PointF GetCaretPointF(long byteIndex)
            { return new PointF(); }

            #endregion
        }
        #endregion

        #region KeyInterpreter class
        /// <summary>
        /// Handles user input such as mouse and keyboard input during hex view edit
        /// </summary>
        class KeyInterpreter : IKeyInterpreter
        {
            /// <summary>
            /// Delegate for key-down processing.
            /// </summary>
            /// <param name="m">the message object contains key data information</param>
            /// <returns>True, if the message was processed</returns>
            delegate bool MessageDelegate(ref Message m);

            #region Fields
            /// <summary>
            /// Contains the parent HexBox control
            /// </summary>
            protected HexBox _hexBox;

            /// <summary>
            /// Contains True, if shift key is down
            /// </summary>
            protected bool _shiftDown;
            /// <summary>
            /// Contains True, if mouse is down
            /// </summary>
            bool _mouseDown;
            /// <summary>
            /// Contains the selection start position info
            /// </summary>
            BytePositionInfo _bpiStart;
            /// <summary>
            /// Contains the current mouse selection position info
            /// </summary>
            BytePositionInfo _bpi;
            /// <summary>
            /// Contains all message handlers of key interpreter key down message
            /// </summary>
            Dictionary<Keys, MessageDelegate> _messageHandlers;
            #endregion

            #region Ctors
            public KeyInterpreter(HexBox hexBox)
            {
                _hexBox = hexBox;
            }
            #endregion

            #region Activate, Deactive methods
            public virtual void Activate()
            {
                _hexBox.MouseDown += new MouseEventHandler(BeginMouseSelection);
                _hexBox.MouseMove += new MouseEventHandler(UpdateMouseSelection);
                _hexBox.MouseUp += new MouseEventHandler(EndMouseSelection);
            }

            public virtual void Deactivate()
            {
                _hexBox.MouseDown -= new MouseEventHandler(BeginMouseSelection);
                _hexBox.MouseMove -= new MouseEventHandler(UpdateMouseSelection);
                _hexBox.MouseUp -= new MouseEventHandler(EndMouseSelection);
            }
            #endregion

            #region Mouse selection methods
            void BeginMouseSelection(object sender, MouseEventArgs e)
            {
                System.Diagnostics.Debug.WriteLine("BeginMouseSelection()", "KeyInterpreter");

                if (e.Button != MouseButtons.Left)
                    return;

                _mouseDown = true;

                if (!_shiftDown)
                {
                    _bpiStart = new BytePositionInfo(_hexBox._bytePos, _hexBox._byteCharacterPos);
                    _hexBox.ReleaseSelection();
                }
                else
                {
                    UpdateMouseSelection(this, e);
                }
            }

            void UpdateMouseSelection(object sender, MouseEventArgs e)
            {
                if (!_mouseDown)
                    return;

                _bpi = GetBytePositionInfo(new Point(e.X, e.Y));
                long selEnd = _bpi.Index;
                long realselStart;
                long realselLength;

                if (selEnd < _bpiStart.Index)
                {
                    realselStart = selEnd;
                    realselLength = _bpiStart.Index - selEnd;
                }
                else if (selEnd > _bpiStart.Index)
                {
                    realselStart = _bpiStart.Index;
                    realselLength = selEnd - realselStart;
                }
                else
                {
                    realselStart = _hexBox._bytePos;
                    realselLength = 0;
                }

                if (realselStart != _hexBox._bytePos || realselLength != _hexBox._selectionLength)
                {
                    _hexBox.InternalSelect(realselStart, realselLength);
                    _hexBox.ScrollByteIntoView(_bpi.Index);
                }
            }

            void EndMouseSelection(object sender, MouseEventArgs e)
            {
                _mouseDown = false;
            }
            #endregion

            #region PrePrcessWmKeyDown methods
            public virtual bool PreProcessWmKeyDown(ref Message m)
            {
                System.Diagnostics.Debug.WriteLine("PreProcessWmKeyDown(ref Message m)", "KeyInterpreter");

                Keys vc = (Keys)m.WParam.ToInt32();

                Keys keyData = vc | Control.ModifierKeys;

                // detect whether key down event should be raised
                var hasMessageHandler = MessageHandlers.ContainsKey(keyData);
                if (hasMessageHandler && RaiseKeyDown(keyData))
                    return true;

                MessageDelegate messageHandler = hasMessageHandler
                    ? MessageHandlers[keyData]
                    : new MessageDelegate(PreProcessWmKeyDownDefault);

                return messageHandler(ref m);
            }

            protected bool PreProcessWmKeyDownDefault(ref Message m)
            {
                _hexBox.ScrollByteIntoView();
                return _hexBox.BasePreProcessMessage(ref m);
            }

            protected bool RaiseKeyDown(Keys keyData)
            {
                KeyEventArgs e = new KeyEventArgs(keyData);
                _hexBox.OnKeyDown(e);
                return e.Handled;
            }

            protected virtual bool PreProcessWmKeyDownLeft(ref Message m)
            {
                return PerformPosMoveLeftByNibble();
            }

            protected virtual bool PreProcessWmKeyDownUp(ref Message m)
            {
                long pos = _hexBox._bytePos;
                int nibblePos = _hexBox._byteCharacterPos;

                if (!(pos == _hexBox._leadingPaddingByteCount && nibblePos == 0))
                {
                    pos = Math.Max(-1, pos - _hexBox._visibleBytesPerLine);
                    // Moving up a row would land on the same column one row up. If that
                    // lands inside the leading padding — including going negative, i.e.
                    // there was no row above at all — there's no valid cell above, so
                    // treat it the same as already being at the very top (do nothing)
                    // rather than jumping sideways to the nearest real column.
                    if (pos == -1 || pos < _hexBox._leadingPaddingByteCount)
                        return true;

                    _hexBox.SetPosition(pos);

                    if (pos < _hexBox._startByte)
                    {
                        _hexBox.PerformScrollLineUp();
                    }

                    _hexBox.UpdateCaret();
                    _hexBox.Invalidate();
                }

                _hexBox.ScrollByteIntoView();
                _hexBox.ReleaseSelection();

                return true;
            }

            protected virtual bool PreProcessWmKeyDownRight(ref Message m)
            {
                return PerformPosMoveRightByNibble();
            }

            protected virtual bool PreProcessWmKeyDownDown(ref Message m)
            {
                long pos = _hexBox._bytePos;
                int nibblePos = _hexBox._byteCharacterPos;

                if (pos == _hexBox._byteProvider.Length && nibblePos == 0)
                    return true;

                pos = Math.Min(_hexBox._byteProvider.Length, pos + _hexBox._visibleBytesPerLine);

                if (pos == _hexBox._byteProvider.Length)
                    nibblePos = 0;

                _hexBox.SetPosition(pos, nibblePos);

                if (pos > _hexBox._endByte - 1)
                {
                    _hexBox.PerformScrollLineDown();
                }

                _hexBox.UpdateCaret();
                _hexBox.ScrollByteIntoView();
                _hexBox.ReleaseSelection();
                _hexBox.Invalidate();

                return true;
            }

            protected virtual bool PreProcessWmKeyDownPageUp(ref Message m)
            {
                long pos = _hexBox._bytePos;
                int nibblePos = _hexBox._byteCharacterPos;

                if (pos == _hexBox._leadingPaddingByteCount && nibblePos == 0)
                    return true;

                pos = Math.Max(_hexBox._leadingPaddingByteCount, pos - _hexBox._visibleByteCount);
                if (pos == _hexBox._leadingPaddingByteCount)
                    return true;

                _hexBox.SetPosition(pos);

                if (pos < _hexBox._startByte)
                {
                    _hexBox.PerformScrollPageUp();
                }

                _hexBox.ReleaseSelection();
                _hexBox.UpdateCaret();
                _hexBox.Invalidate();
                return true;
            }

            protected virtual bool PreProcessWmKeyDownPageDown(ref Message m)
            {
                long pos = _hexBox._bytePos;
                int nibblePos = _hexBox._byteCharacterPos;

                if (pos == _hexBox._byteProvider.Length && nibblePos == 0)
                    return true;

                pos = Math.Min(_hexBox._byteProvider.Length, pos + _hexBox._visibleByteCount);

                if (pos == _hexBox._byteProvider.Length)
                    nibblePos = 0;

                _hexBox.SetPosition(pos, nibblePos);

                if (pos > _hexBox._endByte - 1)
                {
                    _hexBox.PerformScrollPageDown();
                }

                _hexBox.ReleaseSelection();
                _hexBox.UpdateCaret();
                _hexBox.Invalidate();

                return true;
            }

            protected virtual bool PreProcessWmKeyDownShiftLeft(ref Message m)
            {
                long pos = _hexBox._bytePos;
                long selLen = _hexBox._selectionLength;

                if (pos + selLen < 1)
                    return true;

                if (pos + selLen <= _bpiStart.Index)
                {
                    if (pos == _hexBox._leadingPaddingByteCount)
                        return true;

                    pos--;
                    selLen++;
                }
                else
                {
                    selLen = Math.Max(0, selLen - 1);
                }

                _hexBox.ScrollByteIntoView();
                _hexBox.InternalSelect(pos, selLen);

                return true;
            }

            protected virtual bool PreProcessWmKeyDownShiftUp(ref Message m)
            {
                long pos = _hexBox._bytePos;
                long selLen = _hexBox._selectionLength;

                if (pos - _hexBox._visibleBytesPerLine < 0 && pos <= _bpiStart.Index)
                    return true;

                if (_bpiStart.Index >= pos + selLen)
                {
                    pos = pos - _hexBox._visibleBytesPerLine;
                    selLen += _hexBox._visibleBytesPerLine;
                    _hexBox.InternalSelect(pos, selLen);
                    _hexBox.ScrollByteIntoView();
                }
                else
                {
                    selLen -= _hexBox._visibleBytesPerLine;
                    if (selLen < 0)
                    {
                        pos = _bpiStart.Index + selLen;
                        selLen = -selLen;
                        _hexBox.InternalSelect(pos, selLen);
                        _hexBox.ScrollByteIntoView();
                    }
                    else
                    {
                        selLen -= _hexBox._visibleBytesPerLine;
                        _hexBox.InternalSelect(pos, selLen);
                        _hexBox.ScrollByteIntoView(pos + selLen);
                    }
                }

                return true;
            }

            protected virtual bool PreProcessWmKeyDownShiftRight(ref Message m)
            {
                long pos = _hexBox._bytePos;
                long selLen = _hexBox._selectionLength;

                if (pos + selLen >= _hexBox._byteProvider.Length)
                    return true;

                if (_bpiStart.Index <= pos)
                {
                    selLen++;
                    _hexBox.InternalSelect(pos, selLen);
                    _hexBox.ScrollByteIntoView(pos + selLen);
                }
                else
                {
                    pos++;
                    selLen = Math.Max(0, selLen - 1);
                    _hexBox.InternalSelect(pos, selLen);
                    _hexBox.ScrollByteIntoView();
                }

                return true;
            }

            protected virtual bool PreProcessWmKeyDownShiftDown(ref Message m)
            {
                long pos = _hexBox._bytePos;
                long selLen = _hexBox._selectionLength;

                long max = _hexBox._byteProvider.Length;

                if (pos + selLen + _hexBox._visibleBytesPerLine > max)
                    return true;

                if (_bpiStart.Index <= pos)
                {
                    selLen += _hexBox._visibleBytesPerLine;
                    _hexBox.InternalSelect(pos, selLen);
                    _hexBox.ScrollByteIntoView(pos + selLen);
                }
                else
                {
                    selLen -= _hexBox._visibleBytesPerLine;
                    if (selLen < 0)
                    {
                        pos = _bpiStart.Index;
                        selLen = -selLen;
                    }
                    else
                    {
                        pos += _hexBox._visibleBytesPerLine;
                        //selLen -= _hexBox._visibleBytesPerLine;
                    }

                    _hexBox.InternalSelect(pos, selLen);
                    _hexBox.ScrollByteIntoView();
                }

                return true;
            }

            protected virtual bool PreProcessWmKeyDownTab(ref Message m)
            {
                if (_hexBox._stringViewVisible && _hexBox._keyInterpreter.GetType() == typeof(KeyInterpreter))
                {
                    _hexBox.ActivateStringKeyInterpreter();
                    _hexBox.ScrollByteIntoView();
                    _hexBox.ReleaseSelection();
                    _hexBox.UpdateCaret();
                    _hexBox.Invalidate();
                    return true;
                }

                if (_hexBox.Parent == null) return true;
                _hexBox.Parent.SelectNextControl(_hexBox, true, true, true, true);
                return true;
            }

            protected virtual bool PreProcessWmKeyDownShiftTab(ref Message m)
            {
                if (_hexBox._keyInterpreter is StringKeyInterpreter)
                {
                    _shiftDown = false;
                    _hexBox.ActivateKeyInterpreter();
                    _hexBox.ScrollByteIntoView();
                    _hexBox.ReleaseSelection();
                    _hexBox.UpdateCaret();
                    _hexBox.Invalidate();
                    return true;
                }

                if (_hexBox.Parent == null) return true;
                _hexBox.Parent.SelectNextControl(_hexBox, false, true, true, true);
                return true;
            }

            protected virtual bool PreProcessWmKeyDownBack(ref Message m)
            {
                if (!_hexBox._byteProvider.SupportsDeleteBytes())
                    return true;

                if (_hexBox.ReadOnly)
                    return true;

                long pos = _hexBox._bytePos;
                long selLen = _hexBox._selectionLength;
                int nibblePos = _hexBox._byteCharacterPos;

                long startDelete = (nibblePos == 0 && selLen == 0) ? pos - 1 : pos;
                if (startDelete < _hexBox._leadingPaddingByteCount && selLen < 1)
                    return true;

                long bytesToDelete = (selLen > 0) ? selLen : 1;
                _hexBox._byteProvider.DeleteBytes(Math.Max(_hexBox._leadingPaddingByteCount, startDelete), bytesToDelete);
                _hexBox.UpdateScrollSize();

                if (selLen == 0)
                    PerformPosMoveLeftByByte();

                _hexBox.ReleaseSelection();
                _hexBox.Invalidate();

                return true;
            }

            protected virtual bool PreProcessWmKeyDownDelete(ref Message m)
            {
                if (!_hexBox._byteProvider.SupportsDeleteBytes())
                    return true;

                if (_hexBox.ReadOnly)
                    return true;

                long pos = _hexBox._bytePos;
                long selLen = _hexBox._selectionLength;

                if (pos >= _hexBox._byteProvider.Length)
                    return true;

                long bytesToDelete = (selLen > 0) ? selLen : 1;
                _hexBox._byteProvider.DeleteBytes(pos, bytesToDelete);

                _hexBox.UpdateScrollSize();
                _hexBox.ReleaseSelection();
                _hexBox.Invalidate();

                return true;
            }

            protected virtual bool PreProcessWmKeyDownHome(ref Message m)
            {
                long pos = _hexBox._bytePos;
                int nibblePos = _hexBox._byteCharacterPos;

                if (pos <= _hexBox._leadingPaddingByteCount)
                    return true;

                pos = _hexBox._leadingPaddingByteCount;
                nibblePos = 0;
                _hexBox.SetPosition(pos, nibblePos);

                _hexBox.ScrollByteIntoView();
                _hexBox.UpdateCaret();
                _hexBox.ReleaseSelection();

                return true;
            }

            protected virtual bool PreProcessWmKeyDownEnd(ref Message m)
            {
                long pos = _hexBox._bytePos;
                int nibblePos = _hexBox._byteCharacterPos;

                if (pos >= _hexBox._byteProvider.Length - 1)
                    return true;

                pos = _hexBox._byteProvider.Length;
                nibblePos = 0;
                _hexBox.SetPosition(pos, nibblePos);

                _hexBox.ScrollByteIntoView();
                _hexBox.UpdateCaret();
                _hexBox.ReleaseSelection();

                return true;
            }

            protected virtual bool PreProcessWmKeyDownShiftKeyPressed(ref Message m)
            {
                if (_mouseDown)
                    return true;
                if (_shiftDown)
                    return true;

                _shiftDown = true;

                if (_hexBox._selectionLength > 0)
                    return true;

                _bpiStart = new BytePositionInfo(_hexBox._bytePos, _hexBox._byteCharacterPos);

                return true;
            }

            protected virtual bool PreProcessWmKeyDownControlC(ref Message m)
            {
                _hexBox.CopyHex();
                return true;
            }

            protected virtual bool PreProcessWmKeyDownControlX(ref Message m)
            {
                _hexBox.CutHex();
                return true;
            }

            protected virtual bool PreProcessWmKeyDownControlV(ref Message m)
            {
                _hexBox.PasteHex();
                return true;
            }

            #endregion

            #region PreProcessWmChar methods
            public virtual bool PreProcessWmChar(ref Message m)
            {
                if (Control.ModifierKeys == Keys.Control)
                {
                    return _hexBox.BasePreProcessMessage(ref m);
                }

                bool sWrite = _hexBox._byteProvider.SupportsWriteByte();
                bool sInsert = _hexBox._byteProvider.SupportsInsertBytes();
                bool sDelete = _hexBox._byteProvider.SupportsDeleteBytes();

                long pos = _hexBox._bytePos;
                long selLen = _hexBox._selectionLength;
                int nibblePos = _hexBox._byteCharacterPos;

                if (
                    (!sWrite && pos != _hexBox._byteProvider.Length) ||
                    (!sInsert && pos == _hexBox._byteProvider.Length))
                {
                    return _hexBox.BasePreProcessMessage(ref m);
                }

                char keyChar = (char)m.WParam.ToInt32();

                if (Uri.IsHexDigit(keyChar))
                {
                    if (RaiseKeyPress(keyChar))
                        return true;

                    if (_hexBox.ReadOnly)
                        return true;

                    bool isInsertMode = (pos == _hexBox._byteProvider.Length);

                    // do insert when insertActive = true
                    if (!isInsertMode && sInsert && _hexBox.InsertActive && nibblePos == 0)
                        isInsertMode = true;

                    if (sDelete && sInsert && selLen > 0)
                    {
                        _hexBox._byteProvider.DeleteBytes(pos, selLen);
                        isInsertMode = true;
                        nibblePos = 0;
                        _hexBox.SetPosition(pos, nibblePos);
                    }

                    _hexBox.ReleaseSelection();

                    byte currentByte;
                    if (isInsertMode)
                        currentByte = 0;
                    else
                        currentByte = _hexBox._byteProvider.ReadByte(pos);

                    string sCb = currentByte.ToString("X", System.Threading.Thread.CurrentThread.CurrentCulture);
                    if (sCb.Length == 1)
                        sCb = "0" + sCb;

                    string sNewCb = keyChar.ToString();
                    if (nibblePos == 0)
                        sNewCb += sCb.Substring(1, 1);
                    else
                        sNewCb = sCb.Substring(0, 1) + sNewCb;
                    byte newcb = byte.Parse(sNewCb, System.Globalization.NumberStyles.AllowHexSpecifier, System.Threading.Thread.CurrentThread.CurrentCulture);

                    if (isInsertMode)
                        _hexBox._byteProvider.InsertBytes(pos, new byte[] { newcb });
                    else
                        _hexBox._byteProvider.WriteByte(pos, newcb);

                    PerformPosMoveRightByNibble();

                    _hexBox.Invalidate();
                    return true;
                }
                else
                {
                    return _hexBox.BasePreProcessMessage(ref m);
                }
            }

            protected bool RaiseKeyPress(char keyChar)
            {
                KeyPressEventArgs e = new KeyPressEventArgs(keyChar);
                _hexBox.OnKeyPress(e);
                return e.Handled;
            }
            #endregion

            #region PreProcessWmKeyUp methods
            public virtual bool PreProcessWmKeyUp(ref Message m)
            {
                System.Diagnostics.Debug.WriteLine("PreProcessWmKeyUp(ref Message m)", "KeyInterpreter");

                Keys vc = (Keys)m.WParam.ToInt32();

                Keys keyData = vc | Control.ModifierKeys;

                switch (keyData)
                {
                    case Keys.ShiftKey:
                    case Keys.Insert:
                        if (RaiseKeyUp(keyData))
                            return true;
                        break;
                }

                switch (keyData)
                {
                    case Keys.ShiftKey:
                        _shiftDown = false;
                        return true;
                    case Keys.Insert:
                        return PreProcessWmKeyUp_Insert(ref m);
                    default:
                        return _hexBox.BasePreProcessMessage(ref m);
                }
            }

            protected virtual bool PreProcessWmKeyUp_Insert(ref Message m)
            {
                _hexBox.InsertActive = !_hexBox.InsertActive;
                return true;
            }

            protected bool RaiseKeyUp(Keys keyData)
            {
                KeyEventArgs e = new KeyEventArgs(keyData);
                _hexBox.OnKeyUp(e);
                return e.Handled;
            }
            #endregion

            #region Misc
            Dictionary<Keys, MessageDelegate> MessageHandlers
            {
                get
                {
                    if (_messageHandlers == null)
                    {
                        _messageHandlers = new Dictionary<Keys, MessageDelegate>();
                        _messageHandlers.Add(Keys.Left, new MessageDelegate(PreProcessWmKeyDownLeft)); // move left
                        _messageHandlers.Add(Keys.Up, new MessageDelegate(PreProcessWmKeyDownUp)); // move up
                        _messageHandlers.Add(Keys.Right, new MessageDelegate(PreProcessWmKeyDownRight)); // move right
                        _messageHandlers.Add(Keys.Down, new MessageDelegate(PreProcessWmKeyDownDown)); // move down
                        _messageHandlers.Add(Keys.PageUp, new MessageDelegate(PreProcessWmKeyDownPageUp)); // move pageup
                        _messageHandlers.Add(Keys.PageDown, new MessageDelegate(PreProcessWmKeyDownPageDown)); // move page down
                        _messageHandlers.Add(Keys.Left | Keys.Shift, new MessageDelegate(PreProcessWmKeyDownShiftLeft)); // move left with selection
                        _messageHandlers.Add(Keys.Up | Keys.Shift, new MessageDelegate(PreProcessWmKeyDownShiftUp)); // move up with selection
                        _messageHandlers.Add(Keys.Right | Keys.Shift, new MessageDelegate(PreProcessWmKeyDownShiftRight)); // move right with selection
                        _messageHandlers.Add(Keys.Down | Keys.Shift, new MessageDelegate(PreProcessWmKeyDownShiftDown)); // move down with selection
                        _messageHandlers.Add(Keys.Tab, new MessageDelegate(PreProcessWmKeyDownTab)); // switch to string view
                        _messageHandlers.Add(Keys.Back, new MessageDelegate(PreProcessWmKeyDownBack)); // back
                        _messageHandlers.Add(Keys.Delete, new MessageDelegate(PreProcessWmKeyDownDelete)); // delete
                        _messageHandlers.Add(Keys.Home, new MessageDelegate(PreProcessWmKeyDownHome)); // move to home
                        _messageHandlers.Add(Keys.End, new MessageDelegate(PreProcessWmKeyDownEnd)); // move to end
                        _messageHandlers.Add(Keys.ShiftKey | Keys.Shift, new MessageDelegate(PreProcessWmKeyDownShiftKeyPressed)); // begin selection process
                        _messageHandlers.Add(Keys.C | Keys.Control, new MessageDelegate(PreProcessWmKeyDownControlC)); // copy 
                        _messageHandlers.Add(Keys.X | Keys.Control, new MessageDelegate(PreProcessWmKeyDownControlX)); // cut
                        _messageHandlers.Add(Keys.V | Keys.Control, new MessageDelegate(PreProcessWmKeyDownControlV)); // paste
                    }
                    return _messageHandlers;
                }
            }

            protected virtual bool PerformPosMoveLeftByNibble()
            {
                long pos = _hexBox._bytePos;
                long selLen = _hexBox._selectionLength;
                int nibblePos = _hexBox._byteCharacterPos;

                if (selLen != 0)
                {
                    nibblePos = 0;
                    _hexBox.SetPosition(pos, nibblePos);
                    _hexBox.ReleaseSelection();
                }
                else
                {
                    if (pos == _hexBox._leadingPaddingByteCount && nibblePos == 0)
                        return true;

                    if (nibblePos > 0)
                    {
                        nibblePos--;
                    }
                    else
                    {
                        pos = Math.Max(_hexBox._leadingPaddingByteCount, pos - 1);
                        nibblePos++;
                    }

                    _hexBox.SetPosition(pos, nibblePos);

                    if (pos < _hexBox._startByte)
                    {
                        _hexBox.PerformScrollLineUp();
                    }
                    _hexBox.UpdateCaret();
                    _hexBox.Invalidate();
                }

                _hexBox.ScrollByteIntoView();
                return true;
            }

            protected virtual bool PerformPosMoveRightByNibble()
            {
                long pos = _hexBox._bytePos;
                int nibblePos = _hexBox._byteCharacterPos;
                long selLen = _hexBox._selectionLength;

                if (selLen != 0)
                {
                    pos += selLen;
                    nibblePos = 0;
                    _hexBox.SetPosition(pos, nibblePos);
                    _hexBox.ReleaseSelection();
                }
                else
                {
                    if (!(pos == _hexBox._byteProvider.Length && nibblePos == 0))
                    {

                        if (nibblePos > 0)
                        {
                            pos = Math.Min(_hexBox._byteProvider.Length, pos + 1);
                            nibblePos = 0;
                        }
                        else
                        {
                            nibblePos++;
                        }

                        _hexBox.SetPosition(pos, nibblePos);

                        if (pos > _hexBox._endByte - 1)
                        {
                            _hexBox.PerformScrollLineDown();
                        }
                        _hexBox.UpdateCaret();
                        _hexBox.Invalidate();
                    }
                }

                _hexBox.ScrollByteIntoView();
                return true;
            }

            protected virtual bool PerformPosMoveLeftByByte()
            {
                long pos = _hexBox._bytePos;
                int nibblePos = _hexBox._byteCharacterPos;

                if (pos == _hexBox._leadingPaddingByteCount)
                    return true;

                pos = Math.Max(_hexBox._leadingPaddingByteCount, pos - 1);
                nibblePos = 0;

                _hexBox.SetPosition(pos, nibblePos);

                if (pos < _hexBox._startByte)
                {
                    _hexBox.PerformScrollLineUp();
                }
                _hexBox.UpdateCaret();
                _hexBox.ScrollByteIntoView();
                _hexBox.Invalidate();

                return true;
            }

            protected virtual bool PerformPosMoveRightByByte()
            {
                long pos = _hexBox._bytePos;
                int nibblePos = _hexBox._byteCharacterPos;

                if (pos == _hexBox._byteProvider.Length) //Current position reached the end of byteProvider
                    return true;

                pos = Math.Min(_hexBox._byteProvider.Length, pos + 1);
                nibblePos = 0;

                _hexBox.SetPosition(pos, nibblePos);

                if (pos > _hexBox._endByte - 1)
                {
                    _hexBox.PerformScrollLineDown();
                }
                _hexBox.UpdateCaret();
                _hexBox.ScrollByteIntoView();
                _hexBox.Invalidate();

                return true;
            }

            public virtual PointF GetCaretPointF(long byteIndex)
            {
                System.Diagnostics.Debug.WriteLine("GetCaretPointF()", "KeyInterpreter");

                return _hexBox.GetBytePointF(byteIndex);
            }

            protected virtual BytePositionInfo GetBytePositionInfo(Point p)
            {
                return _hexBox.GetHexBytePositionInfo(p);
            }
            #endregion
        }
        #endregion

        #region StringKeyInterpreter class
        /// <summary>
        /// Handles user input such as mouse and keyboard input during string view edit
        /// </summary>
        class StringKeyInterpreter : KeyInterpreter
        {
            #region Ctors
            public StringKeyInterpreter(HexBox hexBox)
                : base(hexBox)
            {
                _hexBox._byteCharacterPos = 0;
            }
            #endregion

            #region PreProcessWmKeyDown methods
            public override bool PreProcessWmKeyDown(ref Message m)
            {
                Keys vc = (Keys)m.WParam.ToInt32();

                Keys keyData = vc | Control.ModifierKeys;

                switch (keyData)
                {
                    case Keys.Tab | Keys.Shift:
                    case Keys.Tab:
                        if (RaiseKeyDown(keyData))
                            return true;
                        break;
                }

                switch (keyData)
                {
                    case Keys.Tab | Keys.Shift:
                        return PreProcessWmKeyDownShiftTab(ref m);
                    case Keys.Tab:
                        return PreProcessWmKeyDownTab(ref m);
                    default:
                        return base.PreProcessWmKeyDown(ref m);
                }
            }

            protected override bool PreProcessWmKeyDownLeft(ref Message m)
            {
                return PerformPosMoveLeftByByte();
            }

            protected override bool PreProcessWmKeyDownRight(ref Message m)
            {
                return PerformPosMoveRightByByte();
            }

            #endregion

            #region PreProcessWmChar methods
            public override bool PreProcessWmChar(ref Message m)
            {
                if (Control.ModifierKeys == Keys.Control)
                {
                    return _hexBox.BasePreProcessMessage(ref m);
                }

                bool sWrite = _hexBox._byteProvider.SupportsWriteByte();
                bool sInsert = _hexBox._byteProvider.SupportsInsertBytes();
                bool sDelete = _hexBox._byteProvider.SupportsDeleteBytes();

                long pos = _hexBox._bytePos;
                long selLen = _hexBox._selectionLength;
                int nibblePos = _hexBox._byteCharacterPos;

                if (
                    (!sWrite && pos != _hexBox._byteProvider.Length) ||
                    (!sInsert && pos == _hexBox._byteProvider.Length))
                {
                    return _hexBox.BasePreProcessMessage(ref m);
                }

                char keyChar = (char)m.WParam.ToInt32();

                if (RaiseKeyPress(keyChar))
                    return true;

                if (_hexBox.ReadOnly)
                    return true;

                bool isInsertMode = (pos == _hexBox._byteProvider.Length);

                // do insert when insertActive = true
                if (!isInsertMode && sInsert && _hexBox.InsertActive)
                    isInsertMode = true;

                if (sDelete && sInsert && selLen > 0)
                {
                    _hexBox._byteProvider.DeleteBytes(pos, selLen);
                    isInsertMode = true;
                    nibblePos = 0;
                    _hexBox.SetPosition(pos, nibblePos);
                }

                _hexBox.ReleaseSelection();

                byte b = _hexBox.ByteCharConverter.ToByte(keyChar);
                if (isInsertMode)
                    _hexBox._byteProvider.InsertBytes(pos, new byte[] { b });
                else
                    _hexBox._byteProvider.WriteByte(pos, b);

                PerformPosMoveRightByByte();
                _hexBox.Invalidate();

                return true;
            }
            #endregion

            #region Misc
            public override PointF GetCaretPointF(long byteIndex)
            {
                System.Diagnostics.Debug.WriteLine("GetCaretPointF()", "StringKeyInterpreter");

                Point gp = _hexBox.GetGridBytePoint(byteIndex);
                return _hexBox.GetByteStringPointF(gp);
            }

            protected override BytePositionInfo GetBytePositionInfo(Point p)
            {
                return _hexBox.GetStringBytePositionInfo(p);
            }
            #endregion
        }
        #endregion

        #region Highlighted Region
        /// <summary>
		/// 
		/// </summary>
        public class HighlightedRegion
        {
            /// <summary>
			/// Start index of highlighted region
			/// </summary>
			public int Start;
            /// <summary>
			/// Length of highlighted region
			/// </summary>
			public int Length;
            /// <summary>
			/// End index of highlighted region
			/// </summary>
			public int End { get { return Start + Length - 1; } }
            /// <summary>
			/// Color of highlighted region
			/// </summary>
			public Color Color;

            /// <summary>
			/// Highlighted region
			/// </summary>
			public HighlightedRegion()
            {

            }

            /// <summary>
			/// Highlighted region
			/// </summary>
			/// <param name="start"></param>
			/// <param name="length"></param>
			/// <param name="color"></param>
			public HighlightedRegion(int start, int length, Color color)
            {
                Start = start;
                Length = length;
                Color = color;
            }

            /// <summary>
			/// Is byte position selected
			/// </summary>
			/// <param name="bytePos"></param>
			/// <returns></returns>
			public bool IsByteSelected(long bytePos)
            {
                return bytePos >= Start && bytePos <= End;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// Contains the hole content bounds of all text
        /// </summary>
        Rectangle _recContent;
        /// <summary>
        /// Contains the line info bounds
        /// </summary>
        Rectangle _recLineInfo;
        /// <summary>
        /// Contains the column info header rectangle bounds
        /// </summary>
        Rectangle _recColumnInfo;
        /// <summary>
        /// Contains the hex data bounds
        /// </summary>
        Rectangle _recHex;
        /// <summary>
        /// Contains the string view bounds
        /// </summary>
        Rectangle _recStringView;

        /// <summary>
        /// Contains string format information for text drawing
        /// </summary>
        StringFormat _stringFormat;
        /// <summary>
        /// Contains the maximum of visible horizontal bytes
        /// </summary>
        int _visibleBytesPerLine;
        /// <summary>
        /// Contains the maximum of visible vertical bytes
        /// </summary>
        int _visibleLineCount;
        /// <summary>
        /// Contains the maximum of visible bytes.
        /// </summary>
        int _visibleByteCount;

        /// <summary>
        /// Contains the scroll bars minimum value
        /// </summary>
        long _scrollVmin;
        /// <summary>
        /// Contains the scroll bars maximum value
        /// </summary>
        long _scrollVmax;
        /// <summary>
        /// Contains the scroll bars current position
        /// </summary>
        long _scrollVpos;
        /// <summary>
        /// Contains a vertical scroll
        /// </summary>
        VScrollBar _vScrollBar;
        /// <summary>
        /// Contains a timer for thumbtrack scrolling
        /// </summary>
        Timer _thumbTrackTimer = new Timer();
        /// <summary>
        /// Contains the thumbtrack scrolling position
        /// </summary>
        long _thumbTrackPosition;
        /// <summary>
        /// Contains the thumbtrack delay for scrolling in milliseconds.
        /// </summary>
        const int ThumbTrackDelayMs = 50;
        /// <summary>
        /// Contains the Enviroment.TickCount of the last refresh
        /// </summary>
        int _lastThumbtrack;
        /// <summary>
        /// Contains the border's left shift
        /// </summary>
        int _recBorderLeft = SystemInformation.Border3DSize.Width;
        /// <summary>
        /// Contains the border's right shift
        /// </summary>
        int _recBorderRight = SystemInformation.Border3DSize.Width;
        /// <summary>
        /// Contains the border's top shift
        /// </summary>
        int _recBorderTop = SystemInformation.Border3DSize.Height;
        /// <summary>
        /// Contains the border bottom shift
        /// </summary>
        int _recBorderBottom = SystemInformation.Border3DSize.Height;

        /// <summary>
        /// Contains the index of the first visible byte
        /// </summary>
        long _startByte;
        /// <summary>
        /// Contains the index of the last visible byte
        /// </summary>
        long _endByte;

        /// <summary>
        /// Contains the current byte position
        /// </summary>
        long _bytePos = -1;
        /// <summary>
        /// Contains the current char position in one byte
        /// </summary>
        /// <example>
        /// "1A"
        /// "1" = char position of 0
        /// "A" = char position of 1
        /// </example>
        int _byteCharacterPos;

        /// <summary>
        /// Contains string format information for hex values
        /// </summary>
        string _hexStringFormat = "X";


        /// <summary>
        /// Contains the current key interpreter
        /// </summary>
        IKeyInterpreter _keyInterpreter;
        /// <summary>
        /// Contains an empty key interpreter without functionality
        /// </summary>
        EmptyKeyInterpreter _emptyKeyInterpreterInstance;
        /// <summary>
        /// Contains the default key interpreter
        /// </summary>
        KeyInterpreter _keyInterpreterInstance;
        /// <summary>
        /// Contains the string key interpreter
        /// </summary>
        StringKeyInterpreter _stringKeyInterpreterInstance;

        /// <summary>
        /// Contains True if caret is visible
        /// </summary>
        bool _caretVisible;

        /// <summary>
        /// Contains a state value about Insert or Write mode. When this value is true and the ByteProvider SupportsInsert is true bytes are inserted instead of overridden.
        /// </summary>
        bool _insertActive;

        /// <summary>
		/// Highlighted regions
		/// </summary>
		public readonly List<HighlightedRegion> HighlightedRegions = new List<HighlightedRegion>();
        #endregion

        #region Events
        /// <summary>
        /// Occurs, when the value of SelectionStart property has changed.
        /// </summary>
        [Description("Occurs, when the value of SelectionStart property has changed.")]
        public event EventHandler SelectionStartChanged;
        /// <summary>
        /// Occurs, when the value of SelectionLength property has changed.
        /// </summary>
        [Description("Occurs, when the value of SelectionLength property has changed.")]
        public event EventHandler SelectionLengthChanged;
        #endregion

        #region Ctors

        /// <summary>
        /// Initializes a new instance of a HexBox class.
        /// </summary>
        public HexBox()
        {
            _vScrollBar = new VScrollBar();
            _vScrollBar.Scroll += new ScrollEventHandler(_vScrollBar_Scroll);

            BackColor = Color.White;
            Font = new Font(new FontFamily("Consolas"), 10, FontStyle.Regular); //SystemFonts.MessageBoxFont;
            _stringFormat = new StringFormat(StringFormat.GenericTypographic);
            _stringFormat.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;

            ActivateEmptyKeyInterpreter();

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            _thumbTrackTimer.Interval = 50;
            _thumbTrackTimer.Tick += new EventHandler(PerformScrollThumbTrack);
        }

        #endregion

        #region Scroll methods
        void _vScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            switch (e.Type)
            {
                case ScrollEventType.Last:
                    break;
                case ScrollEventType.EndScroll:
                    break;
                case ScrollEventType.SmallIncrement:
                    PerformScrollLineDown();
                    break;
                case ScrollEventType.SmallDecrement:
                    PerformScrollLineUp();
                    break;
                case ScrollEventType.LargeIncrement:
                    PerformScrollPageDown();
                    break;
                case ScrollEventType.LargeDecrement:
                    PerformScrollPageUp();
                    break;
                case ScrollEventType.ThumbPosition:
                    long lPos = FromScrollPos(e.NewValue);
                    PerformScrollThumbPosition(lPos);
                    break;
                case ScrollEventType.ThumbTrack:
                    // to avoid performance problems use a refresh delay implemented with a timer
                    if (_thumbTrackTimer.Enabled) // stop old timer
                        _thumbTrackTimer.Enabled = false;

                    // perform scroll immediately only if last refresh is very old
                    int currentThumbTrack = System.Environment.TickCount;
                    if (currentThumbTrack - _lastThumbtrack > ThumbTrackDelayMs)
                    {
                        PerformScrollThumbTrack(null, null);
                        _lastThumbtrack = currentThumbTrack;
                        break;
                    }

                    // start thumbtrack timer 
                    _thumbTrackPosition = FromScrollPos(e.NewValue);
                    _thumbTrackTimer.Enabled = true;
                    break;
                case ScrollEventType.First:
                    break;
                default:
                    break;
            }

            e.NewValue = ToScrollPos(_scrollVpos);
        }

        /// <summary>
        /// Performs the thumbtrack scrolling after an delay.
        /// </summary>
        void PerformScrollThumbTrack(object sender, EventArgs e)
        {
            _thumbTrackTimer.Enabled = false;
            PerformScrollThumbPosition(_thumbTrackPosition);
            _lastThumbtrack = Environment.TickCount;
        }

        void UpdateScrollSize()
        {
            if (!Util.DesignMode)
                System.Diagnostics.Debug.WriteLine("UpdateScrollSize()", "HexBox");

            // calc scroll bar info
            if (VScrollBarVisible && _byteProvider != null && _byteProvider.Length > 0 && _visibleBytesPerLine != 0)
            {
                long scrollmax = (long)Math.Ceiling((double)(_byteProvider.Length + 1) / (double)_visibleBytesPerLine - (double)_visibleLineCount);
                scrollmax = Math.Max(0, scrollmax);

                long scrollpos = _startByte / _visibleBytesPerLine;

                if (scrollmax < _scrollVmax)
                {
                    /* Data size has been decreased. */
                    if (_scrollVpos == _scrollVmax)
                        /* Scroll one line up if we at bottom. */
                        PerformScrollLineUp();
                }

                if (scrollmax == _scrollVmax && scrollpos == _scrollVpos)
                    return;

                _scrollVmin = 0;
                _scrollVmax = scrollmax;
                _scrollVpos = Math.Min(scrollpos, scrollmax);
                UpdateVScroll();
            }
            else if (VScrollBarVisible)
            {
                // disable scroll bar
                _scrollVmin = 0;
                _scrollVmax = 0;
                _scrollVpos = 0;
                UpdateVScroll();
            }
        }

        void UpdateVScroll()
        {
            if (!Util.DesignMode)
                System.Diagnostics.Debug.WriteLine("UpdateVScroll()", "HexBox");

            int max = ToScrollMax(_scrollVmax);

            if (max > 0)
            {
                _vScrollBar.Minimum = 0;
                _vScrollBar.Maximum = max;
                _vScrollBar.Value = ToScrollPos(_scrollVpos);
                _vScrollBar.Visible = true;
            }
            else
            {
                _vScrollBar.Visible = false;
            }
        }

        int ToScrollPos(long value)
        {
            int max = 65535;

            if (_scrollVmax < max)
                return (int)value;
            else
            {
                double valperc = (double)value / (double)_scrollVmax * (double)100;
                int res = (int)Math.Floor((double)max / (double)100 * valperc);
                res = (int)Math.Max(_scrollVmin, res);
                res = (int)Math.Min(_scrollVmax, res);
                return res;
            }
        }

        long FromScrollPos(int value)
        {
            int max = 65535;
            if (_scrollVmax < max)
            {
                return (long)value;
            }
            else
            {
                double valperc = (double)value / (double)max * (double)100;
                long res = (long)Math.Floor((double)_scrollVmax / (double)100 * valperc);
                return res;
            }
        }

        int ToScrollMax(long value)
        {
            long max = 65535;
            if (value > max)
                return (int)max;
            else
                return (int)value;
        }

        void PerformScrollToLine(long pos)
        {
            if (pos < _scrollVmin || pos > _scrollVmax || pos == _scrollVpos)
                return;

            _scrollVpos = pos;

            UpdateVScroll();
            UpdateVisibilityBytes();
            UpdateCaret();
            Invalidate();
        }

        void PerformScrollLines(int lines)
        {
            long pos;
            if (lines > 0)
            {
                pos = Math.Min(_scrollVmax, _scrollVpos + lines);
            }
            else if (lines < 0)
            {
                pos = Math.Max(_scrollVmin, _scrollVpos + lines);
            }
            else
            {
                return;
            }

            PerformScrollToLine(pos);
        }

        void PerformScrollLineDown()
        {
            PerformScrollLines(1);
        }

        void PerformScrollLineUp()
        {
            PerformScrollLines(-1);
        }

        void PerformScrollPageDown()
        {
            PerformScrollLines(_visibleLineCount);
        }

        void PerformScrollPageUp()
        {
            PerformScrollLines(-_visibleLineCount);
        }

        void PerformScrollThumbPosition(long pos)
        {
            // Bug fix: Scroll to end, do not scroll to end
            int difference = (_scrollVmax > 65535) ? 10 : 9;

            if (ToScrollPos(pos) == ToScrollMax(_scrollVmax) - difference)
                pos = _scrollVmax;
            // End Bug fix


            PerformScrollToLine(pos);
        }

        /// <summary>
        /// Scrolls the selection start byte into view
        /// </summary>
        public void ScrollByteIntoView()
        {
            if (!Util.DesignMode)
                System.Diagnostics.Debug.WriteLine("ScrollByteIntoView()", "HexBox");

            ScrollByteIntoView(_bytePos);
        }

        /// <summary>
        /// Scrolls the specific byte into view
        /// </summary>
        /// <param name="index">the index of the byte</param>
        public void ScrollByteIntoView(long index)
        {
            if (!Util.DesignMode)
                System.Diagnostics.Debug.WriteLine("ScrollByteIntoView(long index)", "HexBox");

            if (_byteProvider == null || _keyInterpreter == null)
                return;

            if (index < _startByte)
            {
                long line = (long)Math.Floor((double)index / (double)_visibleBytesPerLine);
                PerformScrollThumbPosition(line);
            }
            else if (index > _endByte)
            {
                long line = (long)Math.Floor((double)index / (double)_visibleBytesPerLine);
                line -= _visibleLineCount - 1;
                PerformScrollThumbPosition(line);
            }
        }
        #endregion

        #region Selection methods
        void ReleaseSelection()
        {
            System.Diagnostics.Debug.WriteLine("ReleaseSelection()", "HexBox");

            if (_selectionLength == 0)
                return;
            _selectionLength = 0;
            OnSelectionLengthChanged(EventArgs.Empty);

            if (!_caretVisible)
                CreateCaret();
            else
                UpdateCaret();

            Invalidate();
        }

        /// <summary>
        /// Returns true if Select method could be invoked.
        /// </summary>
        public bool CanSelectAll()
        {
            if (!Enabled)
                return false;
            if (_byteProvider == null)
                return false;

            return true;
        }

        /// <summary>
        /// Selects all bytes.
        /// </summary>
        public void SelectAll()
        {
            if (ByteProvider == null)
                return;
            Select(0, ByteProvider.Length);
        }

        /// <summary>
        /// Selects the hex box.
        /// </summary>
        /// <param name="start">the start index of the selection</param>
        /// <param name="length">the length of the selection</param>
        public void Select(long start, long length)
        {
            if (ByteProvider == null)
                return;
            if (!Enabled)
                return;

            InternalSelect(start, length);
            ScrollByteIntoView();
        }

        void InternalSelect(long start, long length)
        {
            // Every selection path (mouse drag, Shift+arrow keys, and the public Select())
            // funnels through here, so this is the single place a selection that starts
            // inside the leading padding gets pulled forward past it — shrinking the
            // selection from the left rather than letting it start on a non-real byte.
            if (start < _leadingPaddingByteCount)
            {
                long clampedAmount = _leadingPaddingByteCount - start;
                start = _leadingPaddingByteCount;
                length = Math.Max(0, length - clampedAmount);
            }

            long pos = start;
            long selLen = length;
            int nibblePos = 0;

            if (selLen > 0 && _caretVisible)
                DestroyCaret();
            else if (selLen == 0 && !_caretVisible)
                CreateCaret();

            SetPosition(pos, nibblePos);
            SetSelectionLength(selLen);

            UpdateCaret();
            Invalidate();
        }
        #endregion

        #region Key interpreter methods
        void ActivateEmptyKeyInterpreter()
        {
            if (_emptyKeyInterpreterInstance == null)
                _emptyKeyInterpreterInstance = new EmptyKeyInterpreter(this);

            if (_emptyKeyInterpreterInstance == _keyInterpreter)
                return;

            if (_keyInterpreter != null)
                _keyInterpreter.Deactivate();

            _keyInterpreter = _emptyKeyInterpreterInstance;
            _keyInterpreter.Activate();
        }

        void ActivateKeyInterpreter()
        {
            if (_keyInterpreterInstance == null)
                _keyInterpreterInstance = new KeyInterpreter(this);

            if (_keyInterpreterInstance == _keyInterpreter)
                return;

            if (_keyInterpreter != null)
                _keyInterpreter.Deactivate();

            _keyInterpreter = _keyInterpreterInstance;
            _keyInterpreter.Activate();
        }

        void ActivateStringKeyInterpreter()
        {
            if (_stringKeyInterpreterInstance == null)
                _stringKeyInterpreterInstance = new StringKeyInterpreter(this);

            if (_stringKeyInterpreterInstance == _keyInterpreter)
                return;

            if (_keyInterpreter != null)
                _keyInterpreter.Deactivate();

            _keyInterpreter = _stringKeyInterpreterInstance;
            _keyInterpreter.Activate();
        }
        #endregion

        #region Caret methods
        void CreateCaret()
        {
            // IsHandleCreated matters here specifically: Handle (below) implicitly creates
            // the window handle on first access, which is the wrong thing to do from a
            // caret call on a control that isn't realized yet (e.g. a resize/focus event
            // racing with handle (re)creation) — skip until a handle already exists instead.
            if (_byteProvider == null || _keyInterpreter == null || _caretVisible || !Focused || !IsHandleCreated)
                return;

            System.Diagnostics.Debug.WriteLine("CreateCaret()", "HexBox");

            // define the caret width depending on InsertActive mode
            int caretWidth = (InsertActive) ? 1 : (int)_charSize.Width;
            int caretHeight = (int)_charSize.Height;
            Caret.Create(Handle, IntPtr.Zero, caretWidth, caretHeight);

            UpdateCaret();

            Caret.Show(Handle);

            _caretVisible = true;
        }

        void UpdateCaret()
        {
            if (_byteProvider == null || _keyInterpreter == null || !IsHandleCreated)
                return;

            System.Diagnostics.Debug.WriteLine("UpdateCaret()", "HexBox");

            long byteIndex = _bytePos - _startByte;
            PointF p = _keyInterpreter.GetCaretPointF(byteIndex);
            p.X += _byteCharacterPos * _charSize.Width;
            Caret.SetPos((int)p.X, (int)p.Y);
        }

        void DestroyCaret()
        {
            if (!_caretVisible)
                return;

            System.Diagnostics.Debug.WriteLine("DestroyCaret()", "HexBox");

            Caret.Destroy();
            _caretVisible = false;
        }

        void SetCaretPosition(Point p)
        {
            System.Diagnostics.Debug.WriteLine("SetCaretPosition()", "HexBox");

            if (_byteProvider == null || _keyInterpreter == null)
                return;

            long pos = _bytePos;
            int nibblePos = _byteCharacterPos;

            if (_recHex.Contains(p))
            {
                BytePositionInfo bpi = GetHexBytePositionInfo(p);
                pos = bpi.Index;
                nibblePos = bpi.CharacterPosition;

                SetPosition(pos, nibblePos);

                ActivateKeyInterpreter();
                UpdateCaret();
                Invalidate();
            }
            else if (_recStringView.Contains(p))
            {
                BytePositionInfo bpi = GetStringBytePositionInfo(p);
                pos = bpi.Index;
                nibblePos = bpi.CharacterPosition;

                SetPosition(pos, nibblePos);

                ActivateStringKeyInterpreter();
                UpdateCaret();
                Invalidate();
            }
        }

        BytePositionInfo GetHexBytePositionInfo(Point p)
        {
            System.Diagnostics.Debug.WriteLine("GetHexBytePositionInfo()", "HexBox");

            long bytePos;
            int byteCharacterPos;

            float x = ((float)(p.X - _recHex.X) / _charSize.Width);
            float y = ((float)(p.Y - _recHex.Y) / _charSize.Height);
            int iX = (int)x;
            int iY = (int)y;

            int hPos = (iX / 3 + 1);

            bytePos = Math.Min(_byteProvider.Length,
                _startByte + (_visibleBytesPerLine * (iY + 1) - _visibleBytesPerLine) + hPos - 1);
            byteCharacterPos = (iX % 3);
            if (byteCharacterPos > 1)
                byteCharacterPos = 1;

            if (bytePos == _byteProvider.Length)
                byteCharacterPos = 0;

            if (bytePos < _leadingPaddingByteCount)
                return new BytePositionInfo(_leadingPaddingByteCount, 0);
            return new BytePositionInfo(bytePos, byteCharacterPos);
        }

        BytePositionInfo GetStringBytePositionInfo(Point p)
        {
            System.Diagnostics.Debug.WriteLine("GetStringBytePositionInfo()", "HexBox");

            long bytePos;
            int byteCharacterPos;

            float x = ((float)(p.X - _recStringView.X) / _charSize.Width);
            float y = ((float)(p.Y - _recStringView.Y) / _charSize.Height);
            int iX = (int)x;
            int iY = (int)y;

            int hPos = iX + 1;

            bytePos = Math.Min(_byteProvider.Length,
                _startByte + (_visibleBytesPerLine * (iY + 1) - _visibleBytesPerLine) + hPos - 1);
            byteCharacterPos = 0;

            if (bytePos < _leadingPaddingByteCount)
                return new BytePositionInfo(_leadingPaddingByteCount, 0);
            return new BytePositionInfo(bytePos, byteCharacterPos);
        }
        #endregion

        #region PreProcessMessage methods
        /// <summary>
        /// Preprocesses windows messages.
        /// </summary>
        /// <param name="m">the message to process.</param>
        /// <returns>true, if the message was processed</returns>
        public override bool PreProcessMessage(ref Message m)
        {
            switch (m.Msg)
            {
                case (int)PInvoke.WM_KEYDOWN:
                    return _keyInterpreter.PreProcessWmKeyDown(ref m);
                case (int)PInvoke.WM_CHAR:
                    return _keyInterpreter.PreProcessWmChar(ref m);
                case (int)PInvoke.WM_KEYUP:
                    return _keyInterpreter.PreProcessWmKeyUp(ref m);
                default:
                    return base.PreProcessMessage(ref m);
            }
        }

        bool BasePreProcessMessage(ref Message m)
        {
            return base.PreProcessMessage(ref m);
        }
        #endregion

        #region Copy, Cut and Paste methods
        byte[] GetCopyData()
        {
            if (!CanCopy()) return new byte[0];

            // put bytes into buffer
            byte[] buffer = new byte[_selectionLength];
            int id = -1;
            for (long i = _bytePos; i < _bytePos + _selectionLength; i++)
            {
                id++;

                buffer[id] = _byteProvider.ReadByte(i);
            }
            return buffer;
        }
        /// <summary>
        /// Return true if Copy method could be invoked.
        /// </summary>
        public bool CanCopy()
        {
            if (_selectionLength < 1 || _byteProvider == null)
                return false;

            return true;
        }

        /// <summary>
        /// Return true if Cut method could be invoked.
        /// </summary>
        public bool CanCut()
        {
            if (ReadOnly || !Enabled)
                return false;
            if (_byteProvider == null)
                return false;
            if (_selectionLength < 1 || !_byteProvider.SupportsDeleteBytes())
                return false;

            return true;
        }

        /// <summary>
        /// Moves the current selection in the hex box to the Clipboard in hex format.
        /// </summary>
        public void CutHex()
        {
            if (!CanCut()) return;

            CopyHex();

            _byteProvider.DeleteBytes(_bytePos, _selectionLength);
            _byteCharacterPos = 0;
            UpdateCaret();
            ScrollByteIntoView();
            ReleaseSelection();
            Invalidate();
            Refresh();
        }

        /// <summary>
        /// Returns true if PasteHex() could write clipboard data at the current position or
        /// into the current selection — either by inserting (growing the buffer), or, when the
        /// provider doesn't support insert, by overwriting existing bytes in place instead.
        /// </summary>
        bool CanPasteHexInto()
        {
            if (_byteProvider == null || ReadOnly || !Enabled)
                return false;

            if (_byteProvider.SupportsInsertBytes())
                return _byteProvider.SupportsDeleteBytes() || _selectionLength == 0;

            return _byteProvider.SupportsWriteByte();
        }

        /// <summary>
        /// Replaces the current selection in the hex box with the hex string data of the
        /// Clipboard. If the provider doesn't support insert (e.g. a size-locked provider that
        /// only allows overwriting existing bytes), the clipboard bytes overwrite in place
        /// instead of growing the buffer: with an active selection, they're repeated/truncated
        /// to exactly fill it; with no selection, they're written as-is from the current
        /// position, truncated if they'd run past the end of the buffer. Either way this never
        /// changes the buffer's length — silently, since there is no confirmation for how much
        /// of the clipboard actually got used.
        /// </summary>
        public void PasteHex()
        {
            if (!CanPasteHexInto()) return;

            byte[] buffer = null;
            IDataObject da = Clipboard.GetDataObject();
            if (da.GetDataPresent(typeof(string)))
            {
                string hexString = (string)da.GetData(typeof(string));
                buffer = ConvertHexToBytes(hexString);
                if (buffer == null || buffer.Length == 0)
                    return;
            }
            else
            {
                return;
            }

            if (_byteProvider.SupportsInsertBytes())
            {
                if (_selectionLength > 0)
                    _byteProvider.DeleteBytes(_bytePos, _selectionLength);

                _byteProvider.InsertBytes(_bytePos, buffer);

                SetPosition(_bytePos + buffer.Length, 0);
            }
            else
            {
                long destLength = _selectionLength > 0
                    ? Math.Min(_selectionLength, _byteProvider.Length - _bytePos)
                    : Math.Min(buffer.Length, _byteProvider.Length - _bytePos);

                for (long i = 0; i < destLength; i++)
                    _byteProvider.WriteByte(_bytePos + i, buffer[i % buffer.Length]);

                SetPosition(_bytePos + destLength, 0);
            }

            ReleaseSelection();
            ScrollByteIntoView();
            UpdateCaret();
            Invalidate();
        }

        /// <summary>
        /// Copies the current selection in the hex box to the Clipboard in hex format.
        /// </summary>
        public void CopyHex()
        {
            if (!CanCopy()) return;

            // put bytes into buffer
            byte[] buffer = GetCopyData();

            DataObject da = new DataObject();

            // set string buffer clipbard data
            string hexString = ConvertBytesToHex(buffer); ;
            da.SetData(typeof(string), hexString);

            //set memorystream (BinaryData) clipboard data
            System.IO.MemoryStream ms = new System.IO.MemoryStream(buffer, 0, buffer.Length, false, true);
            da.SetData("BinaryData", ms);

            Clipboard.SetDataObject(da, true);
            UpdateCaret();
            ScrollByteIntoView();
            Invalidate();
        }

        #endregion

        #region Paint methods
        /// <summary>
        /// Paints the background.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            switch (_borderStyle)
            {
                case BorderStyle.Fixed3D:
                    {
                        if (TextBoxRenderer.IsSupported)
                        {
                            VisualStyleElement state = VisualStyleElement.TextBox.TextEdit.Normal;
                            Color backColor = BackColor;

                            if (Enabled)
                            {
                                if (ReadOnly)
                                    state = VisualStyleElement.TextBox.TextEdit.ReadOnly;
                                else if (Focused)
                                    state = VisualStyleElement.TextBox.TextEdit.Focused;
                            }
                            else
                            {
                                state = VisualStyleElement.TextBox.TextEdit.Disabled;
                                backColor = _backColorDisabled;
                            }

                            VisualStyleRenderer vsr = new VisualStyleRenderer(state);
                            vsr.DrawBackground(e.Graphics, ClientRectangle);

                            Rectangle rectContent = vsr.GetBackgroundContentRectangle(e.Graphics, ClientRectangle);
                            using (Brush brush = new SolidBrush(backColor))
                                e.Graphics.FillRectangle(brush, rectContent);
                        }
                        else
                        {
                            // draw background
                            using (Brush brush = new SolidBrush(BackColor))
                                e.Graphics.FillRectangle(brush, ClientRectangle);

                            // draw default border
                            ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, Border3DStyle.Sunken);
                        }

                        break;
                    }
                case BorderStyle.FixedSingle:
                    {
                        // draw background
                        using (Brush brush = new SolidBrush(BackColor))
                            e.Graphics.FillRectangle(brush, ClientRectangle);

                        // draw fixed single border
                        ControlPaint.DrawBorder(e.Graphics, ClientRectangle, Color.Black, ButtonBorderStyle.Solid);
                        break;
                    }
                default:
                    {
                        // draw background
                        using (Brush brush = new SolidBrush(BackColor))
                            e.Graphics.FillRectangle(brush, ClientRectangle);
                        break;
                    }
            }
        }


        /// <summary>
        /// Paints the hex box.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_byteProvider == null)
                return;

            System.Diagnostics.Debug.WriteLine("OnPaint " + DateTime.Now.ToString(), "HexBox");

            // draw only in the content rectangle, so exclude the border and the scrollbar.
            using Region r = new Region(ClientRectangle);
            r.Exclude(_recContent);
            e.Graphics.ExcludeClip(r);

            UpdateVisibilityBytes();


            if (_lineInfoVisible)
                PaintLineInfo(e.Graphics, _startByte, _endByte);

            if (!_stringViewVisible)
            {
                PaintHex(e.Graphics, _startByte, _endByte);
            }
            else
            {
                PaintHexAndStringView(e.Graphics, _startByte, _endByte);
                if (_shadowSelectionVisible)
                    PaintCurrentBytesSign(e.Graphics);
            }
            if (_columnInfoVisible)
                PaintHeaderRow(e.Graphics);
            if (_groupSeparatorVisible)
                PaintColumnSeparator(e.Graphics);
        }


        void PaintLineInfo(Graphics g, long startByte, long endByte)
        {
            // Ensure endByte isn't > length of array.
            endByte = Math.Min(_byteProvider.Length - 1, endByte);

            Color lineInfoColor = (InfoForeColor != Color.Empty) ? InfoForeColor : ForeColor;
            using (Brush brush = new SolidBrush(lineInfoColor))
            {
                int maxLine = GetGridBytePoint(endByte - startByte).Y + 1;

                for (int i = 0; i < maxLine; i++)
                {
                    long firstLineByte = (startByte + (_visibleBytesPerLine) * i) + _lineInfoOffset;

                    PointF bytePointF = GetBytePointF(new Point(0, 0 + i));
                    string info = firstLineByte.ToString(_hexStringFormat, System.Threading.Thread.CurrentThread.CurrentCulture);
                    int nulls = 8 - info.Length;
                    string formattedInfo;
                    if (nulls > -1)
                    {
                        formattedInfo = new string('0', 8 - info.Length) + info;
                    }
                    else
                    {
                        formattedInfo = new string('~', 8);
                    }

                    g.DrawString(formattedInfo, Font, brush, new PointF(_recLineInfo.X, bytePointF.Y), _stringFormat);
                }
            }
        }

        void PaintHeaderRow(Graphics g)
        {
            using (Brush brush = new SolidBrush(InfoForeColor))
            {
                for (int col = 0; col < _visibleBytesPerLine; col++)
                {
                    PaintColumnInfo(g, (byte)col, brush, col);
                }
            }
        }

        void PaintColumnSeparator(Graphics g)
        {
            for (int col = GroupSize; col < _visibleBytesPerLine; col += GroupSize)
            {
                using Brush brush = new SolidBrush(InfoForeColor);
                using var pen = new Pen(brush, 1);
                PointF headerPointF = GetColumnInfoPointF(col);
                headerPointF.X -= _charSize.Width / 2;
                g.DrawLine(pen, headerPointF, new PointF(headerPointF.X, headerPointF.Y + _recColumnInfo.Height + _recHex.Height));
                if (StringViewVisible)
                {
                    PointF byteStringPointF = GetByteStringPointF(new Point(col, 0));
                    headerPointF.X -= 2;
                    g.DrawLine(pen, new PointF(byteStringPointF.X, byteStringPointF.Y), new PointF(byteStringPointF.X, byteStringPointF.Y + _recHex.Height));
                }
            }
        }

        void PaintHex(Graphics g, long startByte, long endByte)
        {
            using Brush brush = new SolidBrush(GetDefaultForeColor());
            using Brush selBrush = new SolidBrush(_selectionForeColor);
            using Brush selBrushBack = new SolidBrush(_selectionBackColor);

            int counter = -1;
            long intern_endByte = Math.Min(_byteProvider.Length - 1, endByte + _visibleBytesPerLine);

            bool isKeyInterpreterActive = _keyInterpreter == null || _keyInterpreter.GetType() == typeof(KeyInterpreter);

            for (long i = startByte; i < intern_endByte + 1; i++)
            {
                counter++;

                // Leading padding bytes aren't real data — leave the cell blank rather
                // than painting a value, but still advance counter so column alignment
                // of the real bytes that follow is unaffected.
                if (i < _leadingPaddingByteCount)
                    continue;

                Point gridPoint = GetGridBytePoint(counter);
                byte b = _byteProvider.ReadByte(i);

                bool isSelectedByte = i >= _bytePos && i <= (_bytePos + _selectionLength - 1) && _selectionLength != 0;

                if (isSelectedByte && isKeyInterpreterActive)
                {
                    PaintHexStringSelected(g, b, selBrush, selBrushBack, gridPoint);
                }
                else
                {
                    PaintHexString(g, b, brush, gridPoint);
                }
            }
        }

        void PaintHexString(Graphics g, byte b, Brush brush, Point gridPoint, Brush brushBack = null)
        {
            PointF bytePointF = GetBytePointF(gridPoint);

            string sB = ConvertByteToHex(b);

            // Color the backgound over ere
            if (brushBack != null)
            {
                bool isLastLineChar = (gridPoint.X + 1 == _visibleBytesPerLine);
                float bcWidth = (isLastLineChar) ? _charSize.Width * 2 : _charSize.Width * 3;
                g.FillRectangle(brushBack, bytePointF.X, bytePointF.Y, bcWidth, _charSize.Height);
            }

            g.DrawString(sB.Substring(0, 1), Font, brush, bytePointF, _stringFormat);
            bytePointF.X += _charSize.Width;
            g.DrawString(sB.Substring(1, 1), Font, brush, bytePointF, _stringFormat);
        }

        void PaintColumnInfo(Graphics g, byte b, Brush brush, int col)
        {
            PointF headerPointF = GetColumnInfoPointF(col);

            string sB = ConvertByteToHex(b);

            g.DrawString(sB.Substring(0, 1), Font, brush, headerPointF, _stringFormat);
            headerPointF.X += _charSize.Width;
            g.DrawString(sB.Substring(1, 1), Font, brush, headerPointF, _stringFormat);
        }

        void PaintHexStringSelected(Graphics g, byte b, Brush brush, Brush brushBack, Point gridPoint)
        {
            string sB = b.ToString(_hexStringFormat, System.Threading.Thread.CurrentThread.CurrentCulture);
            if (sB.Length == 1)
                sB = "0" + sB;

            PointF bytePointF = GetBytePointF(gridPoint);

            bool isLastLineChar = (gridPoint.X + 1 == _visibleBytesPerLine);
            float bcWidth = (isLastLineChar) ? _charSize.Width * 2 : _charSize.Width * 3;

            g.FillRectangle(brushBack, bytePointF.X, bytePointF.Y, bcWidth, _charSize.Height);
            g.DrawString(sB.Substring(0, 1), Font, brush, bytePointF, _stringFormat);
            bytePointF.X += _charSize.Width;
            g.DrawString(sB.Substring(1, 1), Font, brush, bytePointF, _stringFormat);
        }

        void PaintHexAndStringView(Graphics g, long startByte, long endByte)
        {
            using Brush brush = new SolidBrush(GetDefaultForeColor());
            using Brush selBrush = new SolidBrush(_selectionForeColor);
            using Brush selBrushBack = new SolidBrush(_selectionBackColor);

            int counter = -1;
            long intern_endByte = Math.Min(_byteProvider.Length - 1, endByte + _visibleBytesPerLine);

            bool isKeyInterpreterActive = _keyInterpreter == null || _keyInterpreter.GetType() == typeof(KeyInterpreter);
            bool isStringKeyInterpreterActive = _keyInterpreter != null && _keyInterpreter.GetType() == typeof(StringKeyInterpreter);

            for (long i = startByte; i < intern_endByte + 1; i++)
            {
                counter++;

                // Leading padding bytes aren't real data — leave both the hex and ASCII
                // cells blank rather than painting a value, but still advance counter so
                // column alignment of the real bytes that follow is unaffected.
                if (i < _leadingPaddingByteCount)
                    continue;

                Point gridPoint = GetGridBytePoint(counter);
                PointF byteStringPointF = GetByteStringPointF(gridPoint);
                byte b = _byteProvider.ReadByte(i);

                bool isSelectedByte = i >= _bytePos && i <= (_bytePos + _selectionLength - 1) && _selectionLength != 0;

                if (isSelectedByte && isKeyInterpreterActive)
                {
                    PaintHexStringSelected(g, b, selBrush, selBrushBack, gridPoint);
                }
                else
                {
                    // Check if its in a higlighted region
                    bool paintedByte = false;
                    foreach (var highlightedRegion in HighlightedRegions)
                    {
                        if (highlightedRegion.IsByteSelected(i))
                        {
                            var colorBrush = new SolidBrush(highlightedRegion.Color);
                            PaintHexString(g, b, brush, gridPoint, colorBrush);
                            paintedByte = true;
                            break;
                        }
                    }

                    if (!paintedByte)
                    {
                        PaintHexString(g, b, brush, gridPoint);
                    }
                }

                string s = new String(ByteCharConverter.ToChar(b), 1);

                if (isSelectedByte && isStringKeyInterpreterActive)
                {
                    g.FillRectangle(selBrushBack, byteStringPointF.X, byteStringPointF.Y, _charSize.Width, _charSize.Height);
                    g.DrawString(s, Font, selBrush, byteStringPointF, _stringFormat);
                }
                else
                {
                    g.DrawString(s, Font, brush, byteStringPointF, _stringFormat);
                }
            }
        }

        void PaintCurrentBytesSign(Graphics g)
        {
            if (_keyInterpreter != null && _bytePos != -1 && Enabled)
            {
                if (_keyInterpreter.GetType() == typeof(KeyInterpreter))
                {
                    if (_selectionLength == 0)
                    {
                        Point gp = GetGridBytePoint(_bytePos - _startByte);
                        PointF pf = GetByteStringPointF(gp);
                        Size s = new Size((int)_charSize.Width, (int)_charSize.Height);
                        Rectangle r = new Rectangle((int)pf.X, (int)pf.Y, s.Width, s.Height);
                        if (r.IntersectsWith(_recStringView))
                        {
                            r.Intersect(_recStringView);
                            PaintCurrentByteSign(g, r);
                        }
                    }
                    else
                    {
                        int lineWidth = (int)(_recStringView.Width - _charSize.Width);

                        Point startSelGridPoint = GetGridBytePoint(_bytePos - _startByte);
                        PointF startSelPointF = GetByteStringPointF(startSelGridPoint);

                        Point endSelGridPoint = GetGridBytePoint(_bytePos - _startByte + _selectionLength - 1);
                        PointF endSelPointF = GetByteStringPointF(endSelGridPoint);

                        int multiLine = endSelGridPoint.Y - startSelGridPoint.Y;
                        if (multiLine == 0)
                        {

                            Rectangle singleLine = new Rectangle(
                                (int)startSelPointF.X,
                                (int)startSelPointF.Y,
                                (int)(endSelPointF.X - startSelPointF.X + _charSize.Width),
                                (int)_charSize.Height);
                            if (singleLine.IntersectsWith(_recStringView))
                            {
                                singleLine.Intersect(_recStringView);
                                PaintCurrentByteSign(g, singleLine);
                            }
                        }
                        else
                        {
                            Rectangle firstLine = new Rectangle(
                                (int)startSelPointF.X,
                                (int)startSelPointF.Y,
                                (int)(_recStringView.X + lineWidth - startSelPointF.X + _charSize.Width),
                                (int)_charSize.Height);
                            if (firstLine.IntersectsWith(_recStringView))
                            {
                                firstLine.Intersect(_recStringView);
                                PaintCurrentByteSign(g, firstLine);
                            }

                            if (multiLine > 1)
                            {
                                Rectangle betweenLines = new Rectangle(
                                    _recStringView.X,
                                    (int)(startSelPointF.Y + _charSize.Height),
                                    (int)(_recStringView.Width),
                                    (int)(_charSize.Height * (multiLine - 1)));
                                if (betweenLines.IntersectsWith(_recStringView))
                                {
                                    betweenLines.Intersect(_recStringView);
                                    PaintCurrentByteSign(g, betweenLines);
                                }

                            }

                            Rectangle lastLine = new Rectangle(
                                _recStringView.X,
                                (int)endSelPointF.Y,
                                (int)(endSelPointF.X - _recStringView.X + _charSize.Width),
                                (int)_charSize.Height);
                            if (lastLine.IntersectsWith(_recStringView))
                            {
                                lastLine.Intersect(_recStringView);
                                PaintCurrentByteSign(g, lastLine);
                            }
                        }
                    }
                }
                else
                {
                    if (_selectionLength == 0)
                    {
                        Point gp = GetGridBytePoint(_bytePos - _startByte);
                        PointF pf = GetBytePointF(gp);
                        Size s = new Size((int)_charSize.Width * 2, (int)_charSize.Height);
                        Rectangle r = new Rectangle((int)pf.X, (int)pf.Y, s.Width, s.Height);
                        PaintCurrentByteSign(g, r);
                    }
                    else
                    {
                        int lineWidth = (int)(_recHex.Width - _charSize.Width * 5);

                        Point startSelGridPoint = GetGridBytePoint(_bytePos - _startByte);
                        PointF startSelPointF = GetBytePointF(startSelGridPoint);

                        Point endSelGridPoint = GetGridBytePoint(_bytePos - _startByte + _selectionLength - 1);
                        PointF endSelPointF = GetBytePointF(endSelGridPoint);

                        int multiLine = endSelGridPoint.Y - startSelGridPoint.Y;
                        if (multiLine == 0)
                        {
                            Rectangle singleLine = new Rectangle(
                                (int)startSelPointF.X,
                                (int)startSelPointF.Y,
                                (int)(endSelPointF.X - startSelPointF.X + _charSize.Width * 2),
                                (int)_charSize.Height);
                            if (singleLine.IntersectsWith(_recHex))
                            {
                                singleLine.Intersect(_recHex);
                                PaintCurrentByteSign(g, singleLine);
                            }
                        }
                        else
                        {
                            Rectangle firstLine = new Rectangle(
                                (int)startSelPointF.X,
                                (int)startSelPointF.Y,
                                (int)(_recHex.X + lineWidth - startSelPointF.X + _charSize.Width * 2),
                                (int)_charSize.Height);
                            if (firstLine.IntersectsWith(_recHex))
                            {
                                firstLine.Intersect(_recHex);
                                PaintCurrentByteSign(g, firstLine);
                            }

                            if (multiLine > 1)
                            {
                                Rectangle betweenLines = new Rectangle(
                                    _recHex.X,
                                    (int)(startSelPointF.Y + _charSize.Height),
                                    (int)(lineWidth + _charSize.Width * 2),
                                    (int)(_charSize.Height * (multiLine - 1)));
                                if (betweenLines.IntersectsWith(_recHex))
                                {
                                    betweenLines.Intersect(_recHex);
                                    PaintCurrentByteSign(g, betweenLines);
                                }

                            }

                            Rectangle lastLine = new Rectangle(
                                _recHex.X,
                                (int)endSelPointF.Y,
                                (int)(endSelPointF.X - _recHex.X + _charSize.Width * 2),
                                (int)_charSize.Height);
                            if (lastLine.IntersectsWith(_recHex))
                            {
                                lastLine.Intersect(_recHex);
                                PaintCurrentByteSign(g, lastLine);
                            }
                        }
                    }
                }
            }
        }

        void PaintCurrentByteSign(Graphics g, Rectangle rec)
        {
            // stack overflowexception on big files - workaround
            if (rec.Top < 0 || rec.Left < 0 || rec.Width <= 0 || rec.Height <= 0)
                return;

            using Bitmap myBitmap = new Bitmap(rec.Width, rec.Height);
            using Graphics bitmapGraphics = Graphics.FromImage(myBitmap);

            using SolidBrush shadowSelectionBrush = new SolidBrush(_shadowSelectionColor);

            bitmapGraphics.FillRectangle(shadowSelectionBrush, 0,
                0, rec.Width, rec.Height);

            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.GammaCorrected;

            g.DrawImage(myBitmap, rec.Left, rec.Top);
        }

        Color GetDefaultForeColor()
        {
            if (Enabled)
                return ForeColor;
            else
                return Color.Gray;
        }
        void UpdateVisibilityBytes()
        {
            if (_byteProvider == null || _byteProvider.Length == 0)
                return;

            _startByte = (_scrollVpos + 1) * _visibleBytesPerLine - _visibleBytesPerLine;
            _endByte = (long)Math.Min(_byteProvider.Length - 1, _startByte + _visibleByteCount);
        }
        #endregion

        #region Positioning methods
        void UpdateRectanglePositioning()
        {
            // calc char size
            // Measured via a throwaway bitmap rather than CreateGraphics(): CreateGraphics()
            // implicitly creates (or, mid-teardown, can throw against) this control's own
            // window handle, which this layout pass has no business forcing — a resize or
            // property change can call UpdateRectanglePositioning while the handle is being
            // (re)created. A Bitmap's Graphics context measures text identically without
            // touching the control's handle at all.
            SizeF charSize;
            using (var measureBitmap = new Bitmap(1, 1))
            using (var graphics = Graphics.FromImage(measureBitmap))
            {
                charSize = graphics.MeasureString("A", Font, 100, _stringFormat);
            }
            CharSize = new SizeF((float)Math.Ceiling(charSize.Width), (float)Math.Ceiling(charSize.Height));

            int requiredWidth = 0;

            // calc content bounds
            _recContent = ClientRectangle;
            _recContent.X += _recBorderLeft;
            _recContent.Y += _recBorderTop;
            _recContent.Width -= _recBorderRight + _recBorderLeft;
            _recContent.Height -= _recBorderBottom + _recBorderTop;

            if (_vScrollBarVisible)
            {
                _recContent.Width -= _vScrollBar.Width;
                _vScrollBar.Left = _recContent.X + _recContent.Width;
                _vScrollBar.Top = _recContent.Y;
                _vScrollBar.Height = _recContent.Height;
                requiredWidth += _vScrollBar.Width;
            }

            int marginLeft = 4;

            // calc line info bounds
            if (_lineInfoVisible)
            {
                _recLineInfo = new Rectangle(_recContent.X + marginLeft,
                    _recContent.Y,
                    (int)(_charSize.Width * 10),
                    _recContent.Height);
                requiredWidth += _recLineInfo.Width;
            }
            else
            {
                _recLineInfo = Rectangle.Empty;
                _recLineInfo.X = marginLeft;
                requiredWidth += marginLeft;
            }

            // calc line info bounds
            _recColumnInfo = new Rectangle(_recLineInfo.X + _recLineInfo.Width, _recContent.Y, _recContent.Width - _recLineInfo.Width, (int)charSize.Height + 4);
            if (_columnInfoVisible)
            {
                _recLineInfo.Y += (int)charSize.Height + 4;
                _recLineInfo.Height -= (int)charSize.Height + 4;
            }
            else
            {
                _recColumnInfo.Height = 0;
            }

            // calc hex bounds and grid
            _recHex = new Rectangle(_recLineInfo.X + _recLineInfo.Width,
                _recLineInfo.Y,
                _recContent.Width - _recLineInfo.Width,
                _recContent.Height - _recColumnInfo.Height);

            // A window shrunk smaller than the reserved line-info/column-header/scrollbar
            // space would otherwise drive these negative, which then corrupts every
            // byte-per-line/line-count calculation derived from them below.
            if (_recHex.Width < 0) _recHex.Width = 0;
            if (_recHex.Height < 0) _recHex.Height = 0;

            if (UseFixedBytesPerLine)
            {
                SetHorizontalByteCount(_bytesPerLine);
                _recHex.Width = (int)Math.Floor(((double)_visibleBytesPerLine) * _charSize.Width * 3 + (2 * _charSize.Width));
                requiredWidth += _recHex.Width;
            }
            else
            {
                int hmax = (int)Math.Floor((double)_recHex.Width / (double)_charSize.Width);
                if (_stringViewVisible)
                {
                    hmax -= 2;
                    if (hmax > 1)
                        SetHorizontalByteCount((int)Math.Floor((double)hmax / 4));
                    else
                        SetHorizontalByteCount(1);
                }
                else
                {
                    if (hmax > 1)
                        SetHorizontalByteCount((int)Math.Floor((double)hmax / 3));
                    else
                        SetHorizontalByteCount(1);
                }
                _recHex.Width = (int)Math.Floor(((double)_visibleBytesPerLine) * _charSize.Width * 3 + (2 * _charSize.Width));
                requiredWidth += _recHex.Width;
            }

            if (_stringViewVisible)
            {
                _recStringView = new Rectangle(_recHex.X + _recHex.Width,
                    _recHex.Y,
                    (int)(_charSize.Width * _visibleBytesPerLine),
                    _recHex.Height);
                requiredWidth += _recStringView.Width;
            }
            else
            {
                _recStringView = Rectangle.Empty;
            }

            RequiredWidth = requiredWidth;

            // Unlike the horizontal byte count above, this had no floor: a short window
            // could drive vmax to 0 (or negative), and everything downstream that divides
            // by _visibleLineCount/_visibleByteCount assumes at least one visible line.
            int vmax = Math.Max(1, (int)Math.Floor((double)_recHex.Height / (double)_charSize.Height));
            SetVerticalByteCount(vmax);

            _visibleByteCount = _visibleBytesPerLine * _visibleLineCount;

            UpdateScrollSize();
        }

        PointF GetBytePointF(long byteIndex)
        {
            Point gp = GetGridBytePoint(byteIndex);

            return GetBytePointF(gp);
        }

        PointF GetBytePointF(Point gp)
        {
            float x = (3 * _charSize.Width) * gp.X + _recHex.X;
            float y = (gp.Y + 1) * _charSize.Height - _charSize.Height + _recHex.Y;

            return new PointF(x, y);
        }
        PointF GetColumnInfoPointF(int col)
        {
            Point gp = GetGridBytePoint(col);
            float x = (3 * _charSize.Width) * gp.X + _recColumnInfo.X;
            float y = _recColumnInfo.Y;

            return new PointF(x, y);
        }

        PointF GetByteStringPointF(Point gp)
        {
            float x = (_charSize.Width) * gp.X + _recStringView.X;
            float y = (gp.Y + 1) * _charSize.Height - _charSize.Height + _recStringView.Y;

            return new PointF(x, y);
        }

        Point GetGridBytePoint(long byteIndex)
        {
            int row = (int)Math.Floor((double)byteIndex / (double)_visibleBytesPerLine);
            int column = (int)(byteIndex + _visibleBytesPerLine - _visibleBytesPerLine * (row + 1));

            Point res = new Point(column, row);
            return res;
        }
        #endregion

        #region Overridden properties
        /// <summary>
        /// Gets or sets the background color for the control.
        /// </summary>
        [DefaultValue(typeof(Color), "White")]
        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                base.BackColor = value;
            }
        }

        /// <summary>
        /// The font used to display text in the hexbox.
        /// </summary>
        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                if (value == null)
                    return;

                base.Font = value;
                UpdateRectanglePositioning();
                Invalidate();
            }
        }

        /// <summary>
        /// Not used.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Bindable(false)]
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
            }
        }

        /// <summary>
        /// Not used.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never), Bindable(false)]
        public override RightToLeft RightToLeft
        {
            get
            {
                return base.RightToLeft;
            }
            set
            {
                base.RightToLeft = value;
            }
        }
        #endregion

        #region Properties

        // Background color when disabled (see OnPaintBackground()), and bytes-per-line when
        // UseFixedBytesPerLine is true (see UpdateRectanglePositioning()). No longer exposed
        // as public properties — the host never changes either from its default.
        Color _backColorDisabled = Color.FromName("WhiteSmoke");
        int _bytesPerLine = 16;

        /// <summary>
        /// Gets or sets if the user can change the byte data.
        /// </summary>
        /// <remarks>
        /// When set to True, all editing (typing, insert, delete, cut, paste) is disabled.
        /// </remarks>
        [DefaultValue(false), Category("HexBehavior"), Description("Gets or sets if the user can change the byte data.")]
        public bool ReadOnly
        {
            get { return _readOnly; }
            set
            {
                if (_readOnly == value)
                    return;

                _readOnly = value;
                Invalidate();
            }
        }
        bool _readOnly;

        /// <summary>
        /// Gets or sets the number of bytes in a group. Used to show the group separator line (if GroupSeparatorVisible is true)
        /// </summary>
        /// <remarks>
        /// GroupSeparatorVisible property must set to true
        /// </remarks>
        [DefaultValue(4), Category("HexBehavior"), Description("Gets or sets the byte-count between group separators (if visible).")]
        public int GroupSize
        {
            get { return _groupSize; }
            set
            {
                if (_groupSize == value)
                    return;

                _groupSize = value;

                UpdateRectanglePositioning();
                Invalidate();
            }
        }
        int _groupSize = 4;
        /// <summary>
        /// Gets or sets if the count of bytes in one line is fix.
        /// </summary>
        /// <remarks>
        /// When set to True, BytesPerLine property determine the maximum count of bytes in one line.
        /// </remarks>
        [DefaultValue(false), Category("HexBehavior"), Description("Gets or sets if the count of bytes in one line is fix.")]
        public bool UseFixedBytesPerLine
        {
            get { return _useFixedBytesPerLine; }
            set
            {
                if (_useFixedBytesPerLine == value)
                    return;

                _useFixedBytesPerLine = value;

                UpdateRectanglePositioning();
                Invalidate();
            }
        }
        bool _useFixedBytesPerLine;

        /// <summary>
        /// Gets or sets the visibility of a vertical scroll bar.
        /// </summary>
        [DefaultValue(false), Category("HexBehavior"), Description("Gets or sets the visibility of a vertical scroll bar.")]
        public bool VScrollBarVisible
        {
            get { return _vScrollBarVisible; }
            set
            {
                if (_vScrollBarVisible == value)
                    return;

                _vScrollBarVisible = value;

                if (_vScrollBarVisible)
                    Controls.Add(_vScrollBar);
                else
                    Controls.Remove(_vScrollBar);

                UpdateRectanglePositioning();
                UpdateScrollSize();
            }
        }
        bool _vScrollBarVisible;

        /// <summary>
        /// Gets or sets the ByteProvider.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IByteProvider ByteProvider
        {
            get { return _byteProvider; }
            set
            {
                if (_byteProvider == value)
                    return;

                if (value == null)
                    ActivateEmptyKeyInterpreter();
                else
                    ActivateKeyInterpreter();

                if (_byteProvider != null)
                    _byteProvider.LengthChanged -= new EventHandler(_byteProvider_LengthChanged);

                _byteProvider = value;
                if (_byteProvider != null)
                    _byteProvider.LengthChanged += new EventHandler(_byteProvider_LengthChanged);

                if (value == null) // do not raise events if value is null
                {
                    _bytePos = -1;
                    _byteCharacterPos = 0;
                    _selectionLength = 0;

                    DestroyCaret();
                }
                else
                {
                    SetPosition(_leadingPaddingByteCount, 0);
                    SetSelectionLength(0);

                    if (_caretVisible && Focused)
                        UpdateCaret();
                    else
                        CreateCaret();
                }

                CheckCurrentLineChanged();
                CheckCurrentPositionInLineChanged();

                _scrollVpos = 0;

                UpdateVisibilityBytes();
                UpdateRectanglePositioning();

                Invalidate();
            }
        }

        IByteProvider _byteProvider;
        bool _groupSeparatorVisible = false;

        /// <summary>
        /// Gets or sets the visibility of the column info
        /// </summary>
        [DefaultValue(false), Category("HexBehavior"), Description("Gets or sets the visibility of header row.")]
        public bool ColumnInfoVisible
        {
            get { return _columnInfoVisible; }
            set
            {
                if (_columnInfoVisible == value)
                    return;

                _columnInfoVisible = value;

                UpdateRectanglePositioning();
                Invalidate();
            }
        }
        bool _columnInfoVisible = false;

        /// <summary>
        /// Gets or sets the visibility of a line info.
        /// </summary>
        [DefaultValue(false), Category("HexBehavior"), Description("Gets or sets the visibility of a line info.")]
        public bool LineInfoVisible
        {
            get { return _lineInfoVisible; }
            set
            {
                if (_lineInfoVisible == value)
                    return;

                _lineInfoVisible = value;

                UpdateRectanglePositioning();
                Invalidate();
            }
        }
        bool _lineInfoVisible = false;

        /// <summary>
        /// Gets or sets the offset of a line info.
        /// </summary>
        [DefaultValue((long)0), Category("HexBehavior"), Description("Gets or sets the offset of the line info.")]
        public long LineInfoOffset
        {
            get { return _lineInfoOffset; }
            set
            {
                if (_lineInfoOffset == value)
                    return;

                _lineInfoOffset = value;

                Invalidate();
            }
        }
        long _lineInfoOffset = 0;

        /// <summary>
        /// Gets or sets how many bytes at the start of the ByteProvider are placeholder
        /// padding rather than real data (e.g. inserted by a host app so a non-aligned
        /// start address still lines up on a row boundary). These byte indices are
        /// painted blank instead of their actual value, and are excluded from keyboard
        /// navigation, mouse selection, and click hit-testing, which all treat this value
        /// as the first reachable byte position instead of 0.
        /// </summary>
        [DefaultValue((long)0), Category("HexBehavior"), Description("Gets or sets how many leading bytes are placeholder padding, painted blank and excluded from selection/navigation.")]
        public long LeadingPaddingByteCount
        {
            get { return _leadingPaddingByteCount; }
            set
            {
                if (_leadingPaddingByteCount == value)
                    return;

                _leadingPaddingByteCount = value;

                // The current caret position was only ever floor-clamped against whatever
                // this value was at the time (e.g. the previous file/page). If it now falls
                // inside the padding this value just grew to cover, pull it forward — this
                // is what makes the property self-correcting regardless of whether a host
                // sets it before or after attaching a new ByteProvider.
                if (_byteProvider != null && _bytePos < value)
                {
                    SetPosition(value, 0);
                    SetSelectionLength(0);
                }

                Invalidate();
            }
        }
        long _leadingPaddingByteCount = 0;

        // Only ever set to its default (Fixed3D) — the fields it would otherwise recompute
        // (_recBorderLeft etc.) already default to the equivalent values, see below.
        BorderStyle _borderStyle = BorderStyle.Fixed3D;

        /// <summary>
        /// Gets or sets the visibility of the string view.
        /// </summary>
        [DefaultValue(false), Category("HexBehavior"), Description("Gets or sets the visibility of the string view.")]
        public bool StringViewVisible
        {
            get { return _stringViewVisible; }
            set
            {
                if (_stringViewVisible == value)
                    return;

                _stringViewVisible = value;

                UpdateRectanglePositioning();
                Invalidate();
            }
        }
        bool _stringViewVisible;

        /// <summary>
        /// Gets and sets the starting point of the bytes selected in the hex box.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long SelectionStart
        {
            get { return _bytePos; }
            set
            {
                SetPosition(value, 0);
                ScrollByteIntoView();
                Invalidate();
            }
        }

        /// <summary>
        /// Gets and sets the number of bytes selected in the hex box.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long SelectionLength
        {
            get { return _selectionLength; }
            set
            {
                SetSelectionLength(value);
                ScrollByteIntoView();
                Invalidate();
            }
        }
        long _selectionLength;


        /// <summary>
        /// Gets or sets the info color used for column info and line info. When this property is null, then ForeColor property is used.
        /// </summary>
        [DefaultValue(typeof(Color), "Gray"), Category("HexAppearance"), Description("Gets or sets the line info color. When this property is null, then ForeColor property is used.")]
        public Color InfoForeColor
        {
            get { return _infoForeColor; }
            set { _infoForeColor = value; Invalidate(); }
        }
        Color _infoForeColor = Color.Gray;

        Color _selectionBackColor = Color.Blue;
        Color _selectionForeColor = Color.White;
        bool _shadowSelectionVisible = true;

        /// <summary>
        /// Gets or sets the color of the shadow selection. 
        /// </summary>
        /// <remarks>
        /// A alpha component must be given! 
        /// Default alpha = 100
        /// </remarks>
        [DefaultValue(typeof(Color), "100, 60, 188, 255"), Category("HexAppearance"), Description("Gets or sets the color of the shadow selection.")]
        public Color ShadowSelectionColor
        {
            get { return _shadowSelectionColor; }
            set { _shadowSelectionColor = value; Invalidate(); }
        }
        Color _shadowSelectionColor = Color.FromArgb(100, 60, 188, 255);

        /// <summary>
        /// Contains the size of a single character in pixel
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SizeF CharSize
        {
            get { return _charSize; }
            private set
            {
                if (_charSize == value)
                    return;
                _charSize = value;
            }
        }
        SizeF _charSize;

        /// <summary>
        /// Gets the width required for the content
        /// </summary>
        [DefaultValue(0), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int RequiredWidth
        {
            get { return _requiredWidth; }
            private set
            {
                if (_requiredWidth == value)
                    return;
                _requiredWidth = value;
            }
        }
        int _requiredWidth;

        long _currentLine;
        int _currentPositionInLine;

        /// <summary>
        /// Gets the a value if insertion mode is active or not.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool InsertActive
        {
            get { return _insertActive; }
            set
            {
                if (_insertActive == value)
                    return;

                _insertActive = value;

                // recreate caret
                DestroyCaret();
                CreateCaret();
            }
        }

        /// <summary>
        /// Gets or sets the converter that will translate between byte and character values.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IByteCharConverter ByteCharConverter
        {
            get
            {
                if (_byteCharConverter == null)
                    _byteCharConverter = new DefaultByteCharConverter();
                return _byteCharConverter;
            }
            set
            {
                if (value != null && value != _byteCharConverter)
                {
                    _byteCharConverter = value;
                    Invalidate();
                }
            }
        }
        IByteCharConverter _byteCharConverter;

        #endregion

        #region Misc

        /// <summary>
        /// Converts a byte array to a hex string. For example: {10,11} = "0A 0B"
        /// </summary>
        /// <param name="data">the byte array</param>
        /// <returns>the hex string</returns>
        string ConvertBytesToHex(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
            {
                string hex = ConvertByteToHex(b);
                sb.Append(hex);
                sb.Append(" ");
            }
            if (sb.Length > 0)
                sb.Remove(sb.Length - 1, 1);
            string result = sb.ToString();
            return result;
        }
        /// <summary>
        /// Converts the byte to a hex string. For example: "10" = "0A";
        /// </summary>
        /// <param name="b">the byte to format</param>
        /// <returns>the hex string</returns>
        string ConvertByteToHex(byte b)
        {
            string sB = b.ToString(_hexStringFormat, System.Threading.Thread.CurrentThread.CurrentCulture);
            if (sB.Length == 1)
                sB = "0" + sB;
            return sB;
        }
        /// <summary>
        /// Converts the hex string to an byte array. The hex string must be separated by a space char ' '. If there is any invalid hex information in the string the result will be null.
        /// </summary>
        /// <param name="hex">the hex string separated by ' '. For example: "0A 0B 0C"</param>
        /// <returns>the byte array. null if hex is invalid or empty</returns>
        byte[] ConvertHexToBytes(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return null;
            hex = hex.Trim();
            var hexArray = hex.Split(' ');
            var byteArray = new byte[hexArray.Length];

            for (int i = 0; i < hexArray.Length; i++)
            {
                var hexValue = hexArray[i];

                byte b;
                var isByte = ConvertHexToByte(hexValue, out b);
                if (!isByte)
                    return null;
                byteArray[i] = b;
            }

            return byteArray;
        }

        bool ConvertHexToByte(string hex, out byte b)
        {
            bool isByte = byte.TryParse(hex, System.Globalization.NumberStyles.HexNumber, System.Threading.Thread.CurrentThread.CurrentCulture, out b);
            return isByte;
        }

        void SetPosition(long bytePos)
        {
            SetPosition(bytePos, _byteCharacterPos);
        }

        void SetPosition(long bytePos, int byteCharacterPos)
        {
            // The sole writer of _bytePos (aside from the ByteProvider-detach reset to -1),
            // so this is the single choke point to keep it inside the buffer regardless of
            // what any caller computed — callers have gotten this wrong before (e.g. a
            // caret left stale at the old end-of-buffer position after a delete shrinks the
            // provider), and an out-of-range _bytePos crashes the next ReadByte/WriteByte.
            if (_byteProvider != null)
            {
                if (bytePos < _leadingPaddingByteCount)
                    bytePos = _leadingPaddingByteCount;
                else if (bytePos > _byteProvider.Length)
                    bytePos = _byteProvider.Length;
            }

            if (_byteCharacterPos != byteCharacterPos)
            {
                _byteCharacterPos = byteCharacterPos;
            }

            if (bytePos != _bytePos)
            {
                _bytePos = bytePos;
                CheckCurrentLineChanged();
                CheckCurrentPositionInLineChanged();

                OnSelectionStartChanged(EventArgs.Empty);
            }
        }

        void SetSelectionLength(long selectionLength)
        {
            // Mirrors the clamp in SetPosition: keeps _bytePos + _selectionLength from
            // running past the buffer, which GetCopyData() and friends trust unchecked.
            if (_byteProvider != null)
            {
                long maxLength = Math.Max(0, _byteProvider.Length - _bytePos);
                if (selectionLength > maxLength)
                    selectionLength = maxLength;
            }
            if (selectionLength < 0)
                selectionLength = 0;

            if (selectionLength != _selectionLength)
            {
                _selectionLength = selectionLength;
                OnSelectionLengthChanged(EventArgs.Empty);
            }
        }

        void SetHorizontalByteCount(int value)
        {
            if (_visibleBytesPerLine == value)
                return;

            _visibleBytesPerLine = value;
        }

        void SetVerticalByteCount(int value)
        {
            if (_visibleLineCount == value)
                return;

            _visibleLineCount = value;
        }

        void CheckCurrentLineChanged()
        {
            long currentLine = (long)Math.Floor((double)_bytePos / (double)_visibleBytesPerLine) + 1;

            if (_byteProvider == null && _currentLine != 0)
            {
                _currentLine = 0;
            }
            else if (currentLine != _currentLine)
            {
                _currentLine = currentLine;
            }
        }

        void CheckCurrentPositionInLineChanged()
        {
            Point gb = GetGridBytePoint(_bytePos);
            int currentPositionInLine = gb.X + 1;

            if (_byteProvider == null && _currentPositionInLine != 0)
            {
                _currentPositionInLine = 0;
            }
            else if (currentPositionInLine != _currentPositionInLine)
            {
                _currentPositionInLine = currentPositionInLine;
            }
        }

        /// <summary>
        /// Raises the SelectionStartChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnSelectionStartChanged(EventArgs e)
        {
            if (SelectionStartChanged != null)
                SelectionStartChanged(this, e);
        }

        /// <summary>
        /// Raises the SelectionLengthChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnSelectionLengthChanged(EventArgs e)
        {
            if (SelectionLengthChanged != null)
                SelectionLengthChanged(this, e);
        }

        /// <summary>
        /// Raises the MouseDown event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("OnMouseDown()", "HexBox");

            if (!Focused)
                Focus();

            if (e.Button == MouseButtons.Left)
                SetCaretPosition(new Point(e.X, e.Y));

            base.OnMouseDown(e);
        }

        /// <summary>
        /// Raises the MouseWhell event
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int linesToScroll = -(e.Delta * SystemInformation.MouseWheelScrollLines / 120);
            PerformScrollLines(linesToScroll);

            base.OnMouseWheel(e);
        }


        /// <summary>
        /// Raises the Resize event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRectanglePositioning();
        }

        /// <summary>
        /// Raises the GotFocus event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnGotFocus(EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("OnGotFocus()", "HexBox");

            base.OnGotFocus(e);

            CreateCaret();
        }

        /// <summary>
        /// Raises the LostFocus event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLostFocus(EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("OnLostFocus()", "HexBox");

            base.OnLostFocus(e);

            DestroyCaret();
        }

        void _byteProvider_LengthChanged(object sender, EventArgs e)
        {
            UpdateScrollSize();
        }

        /// <summary>
        /// Dispose pattern
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (IsDisposed) return;
            _stringFormat.Dispose();
            _vScrollBar.Dispose();
            _thumbTrackTimer.Dispose();
        }
        #endregion

        #region Scaling Support for High DPI resolution screens
        /// <summary>
        /// For high resolution screen support
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <param name="specified">bounds</param>
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);

            BeginInvoke(new MethodInvoker(() =>
                {
                    UpdateRectanglePositioning();
                    if (_caretVisible)
                    {
                        DestroyCaret();
                        CreateCaret();
                    }
                    Invalidate();
                }));
        }
        #endregion
    }
}
