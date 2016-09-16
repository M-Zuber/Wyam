﻿using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using IFileProvider = Microsoft.Extensions.FileProviders.IFileProvider;

namespace Wyam.Razor.FileProviders
{

    /// <summary>
    /// A IFileProvider that provides files based on their stream for use with arbitrary documents.
    /// </summary>
    internal class StreamFileProvider : IFileProvider
    {
        private readonly IFileProvider _rootFileProvider;
        private readonly Stream _stream;

        public StreamFileProvider(IFileProvider rootFileProvider, Stream stream)
        {
            _rootFileProvider = rootFileProvider;
            _stream = stream;
        }

        public IFileInfo GetFileInfo(string subpath) => 
            new StreamFileInfo(_rootFileProvider.GetFileInfo(subpath), _stream);

        public IDirectoryContents GetDirectoryContents(string subpath) => 
            _rootFileProvider.GetDirectoryContents(subpath);

        IChangeToken IFileProvider.Watch(string filter) => new EmptyChangeToken();

        private class StreamFileInfo : IFileInfo
        {
            private readonly IFileInfo _info;
            private readonly Stream _stream;

            public StreamFileInfo(IFileInfo info, Stream stream)
            {
                _info = info;
                _stream = stream;
            }

            public bool Exists => true;

            public long Length => _stream.Length;

            public string PhysicalPath => _info.PhysicalPath;

            public string Name => _info.Name;

            public DateTimeOffset LastModified => DateTimeOffset.Now;

            public bool IsDirectory => false;

            public Stream CreateReadStream() => new NonDisposableStreamWrapper(_stream);
        }

        // This prevents the underlying stream from being disposed when the wrapper is
        private class NonDisposableStreamWrapper : Stream
        {
            private readonly Stream _stream;

            public NonDisposableStreamWrapper(Stream stream)
            {
                _stream = stream;
            }

            public override void Flush() => 
                _stream.Flush();

            public override long Seek(long offset, SeekOrigin origin) => 
                _stream.Seek(offset, origin);

            public override void SetLength(long value) =>
                _stream.SetLength(value);

            public override int Read(byte[] buffer, int offset, int count) => 
                _stream.Read(buffer, offset, count);

            public override void Write(byte[] buffer, int offset, int count) => 
                _stream.Write(buffer, offset, count);

            public override bool CanRead => _stream.CanRead;
            public override bool CanSeek => _stream.CanSeek;
            public override bool CanWrite => _stream.CanWrite;
            public override long Length => _stream.Length;

            public override long Position
            {
                get { return _stream.Position; }
                set { _stream.Position = value; }
            }
        }
    }
}
