using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace WebBackend.Dataset
{
    class FileReader
    {
        private readonly static int _pageSize = 4096 * 16;

        private readonly byte[] _buffer = new byte[_pageSize];

        private readonly List<byte> _lineBuffer = new List<byte>();

        /// <summary>
        /// Determine whether data in buffer are valid.
        /// </summary>
        private bool _isBufferValid = false;

        /// <summary>
        /// Position of buffer within a file
        /// </summary>
        private long _bufferPosition = 0;

        /// <summary>
        /// Actual offset within the buffer.
        /// </summary>
        private int _bufferOffset = 0;

        private readonly long _fileLength;

        private readonly FileStream _file;

        internal bool EndOfStream { get { return ActualPosition >= _fileLength; } }

        internal long ActualPosition
        {
            get
            {
                return _bufferPosition + _bufferOffset;
            }
            set
            {
                //reading is aligned to pages
                var newBufferPosition = (value / _pageSize) * _pageSize;
                if (newBufferPosition != _bufferPosition)
                    _isBufferValid = false;

                _bufferPosition = newBufferPosition;
                _bufferOffset = (int)(value % _pageSize);
            }
        }

        internal FileReader(string filePath)
        {
            _file = new FileStream(filePath, FileMode.Open);
            _fileLength = _file.Length;
        }

        internal string ReadLine()
        {
            _lineBuffer.Clear();

            byte b;
            do
            {
                if (_bufferOffset >= _buffer.Length || !_isBufferValid)
                    fetchPage();

                b = _buffer[_bufferOffset++];
                _lineBuffer.Add(b);

            } while (b != '\n' && _bufferPosition + _bufferPosition < _fileLength);

            _lineBuffer.RemoveAt(_lineBuffer.Count - 1);
            return Encoding.UTF8.GetString(_lineBuffer.ToArray());
        }

        private void fetchPage()
        {
            ActualPosition = _bufferPosition + _bufferOffset;
            _file.Position = _bufferPosition;

            _file.Read(_buffer, 0, _pageSize);
            _isBufferValid = true;
        }
    }
}
