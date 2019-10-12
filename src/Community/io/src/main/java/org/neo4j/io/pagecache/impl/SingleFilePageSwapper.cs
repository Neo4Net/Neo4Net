using System;
using System.Diagnostics;
using System.Threading;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Io.pagecache.impl
{
	using SystemUtils = org.apache.commons.lang3.SystemUtils;
	using FileChannelImpl = sun.nio.ch.FileChannelImpl;


	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using StoreFileChannel = Neo4Net.Io.fs.StoreFileChannel;
	using StoreFileChannelUnwrapper = Neo4Net.Io.fs.StoreFileChannelUnwrapper;
	using MuninnPageCache = Neo4Net.Io.pagecache.impl.muninn.MuninnPageCache;
	using UnsafeUtil = Neo4Net.@unsafe.Impl.@internal.Dragons.UnsafeUtil;

	/// <summary>
	/// A simple PageSwapper implementation that directs all page swapping to a
	/// single file on the file system.
	/// 
	/// It additionally tracks the file size precisely, to avoid calling into the
	/// file system whenever the size of the given file is queried.
	/// </summary>
	public class SingleFilePageSwapper : PageSwapper
	{
		 private const int MAX_INTERRUPTED_CHANNEL_REOPEN_ATTEMPTS = 42;

		 private static int DefaultChannelStripePower()
		 {
			  int vcores = Runtime.Runtime.availableProcessors();
			  // Find the lowest 2's exponent that can accommodate 'vcores'
			  int stripePower = 32 - Integer.numberOfLeadingZeros( vcores - 1 );
			  return Math.Min( 64, Math.Max( 1, stripePower ) );
		 }

		 // Exponent of 2 of how many channels we open per file:
		 private static readonly int _globalChannelStripePower = Integer.getInteger( "org.neo4j.io.pagecache.implSingleFilePageSwapper.channelStripePower", DefaultChannelStripePower() );

		 // Exponent of 2 of how many consecutive pages go to the same stripe
		 private static readonly int _channelStripeShift = Integer.getInteger( "org.neo4j.io.pagecache.implSingleFilePageSwapper.channelStripeShift", 4 );

		 private static readonly int _globalChannelStripeCount = 1 << _globalChannelStripePower;
		 private static readonly int _globalChannelStripeMask = StripeMask( _globalChannelStripeCount );

		 private const int TOKEN_CHANNEL_STRIPE = 0;
		 private const long TOKEN_FILE_PAGE_ID = 0;

		 private static readonly long _fileSizeOffset = UnsafeUtil.getFieldOffset( typeof( SingleFilePageSwapper ), "fileSize" );

		 private static readonly ThreadLocal<ByteBuffer> _proxyCache = new ThreadLocal<ByteBuffer>();
		 private static readonly MethodHandle _positionLockGetter = PositionLockGetter;

		 private static int StripeMask( int count )
		 {
			  Debug.Assert( Integer.bitCount( count ) == 1 );
			  return count - 1;
		 }

		 private static MethodHandle PositionLockGetter
		 {
			 get
			 {
				  try
				  {
						MethodHandles.Lookup lookup = MethodHandles.lookup();
						System.Reflection.FieldInfo field = typeof( FileChannelImpl ).getDeclaredField( "positionLock" );
						field.Accessible = true;
						return lookup.unreflectGetter( field );
				  }
				  catch ( Exception )
				  {
						return null;
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static ByteBuffer proxy(long buffer, int bufferLength) throws java.io.IOException
		 private static ByteBuffer Proxy( long buffer, int bufferLength )
		 {
			  ByteBuffer buf = _proxyCache.get();
			  if ( buf != null )
			  {
					UnsafeUtil.initDirectByteBuffer( buf, buffer, bufferLength );
					return buf;
			  }
			  return CreateAndGetNewBuffer( buffer, bufferLength );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static ByteBuffer createAndGetNewBuffer(long buffer, int bufferLength) throws java.io.IOException
		 private static ByteBuffer CreateAndGetNewBuffer( long buffer, int bufferLength )
		 {
			  ByteBuffer buf;
			  try
			  {
					buf = UnsafeUtil.newDirectByteBuffer( buffer, bufferLength );
			  }
			  catch ( Exception e )
			  {
					throw new IOException( e );
			  }
			  _proxyCache.set( buf );
			  return buf;
		 }

		 private readonly FileSystemAbstraction _fs;
		 private readonly File _file;
		 private readonly int _filePageSize;
		 private volatile PageEvictionCallback _onEviction;
		 private readonly StoreChannel[] _channels;
		 private FileLock _fileLock;
		 private readonly bool _hasPositionLock;
		 private readonly int _channelStripeCount;
		 private readonly int _channelStripeMask;

		 // Guarded by synchronized(this). See tryReopen() and close().
		 private bool _closed;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private volatile long fileSize;
		 private volatile long _fileSize;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public SingleFilePageSwapper(java.io.File file, org.neo4j.io.fs.FileSystemAbstraction fs, int filePageSize, org.neo4j.io.pagecache.PageEvictionCallback onEviction, boolean noChannelStriping) throws java.io.IOException
		 public SingleFilePageSwapper( File file, FileSystemAbstraction fs, int filePageSize, PageEvictionCallback onEviction, bool noChannelStriping )
		 {
			  this._fs = fs;
			  this._file = file;
			  if ( noChannelStriping )
			  {
					this._channelStripeCount = 1;
					this._channelStripeMask = StripeMask( _channelStripeCount );
			  }
			  else
			  {
					this._channelStripeCount = _globalChannelStripeCount;
					this._channelStripeMask = _globalChannelStripeMask;
			  }
			  this._channels = new StoreChannel[_channelStripeCount];
			  for ( int i = 0; i < _channelStripeCount; i++ )
			  {
					_channels[i] = fs.Open( file, OpenMode.READ_WRITE );
			  }
			  this._filePageSize = filePageSize;
			  this._onEviction = onEviction;
			  IncreaseFileSizeTo( _channels[TOKEN_CHANNEL_STRIPE].size() );

			  try
			  {
					AcquireLock();
			  }
			  catch ( IOException e )
			  {
					CloseAndCollectExceptions( 0, e );
			  }
			  _hasPositionLock = _channels[0].GetType() == typeof(StoreFileChannel) && StoreFileChannelUnwrapper.unwrap(_channels[0]).GetType() == typeof(FileChannelImpl);
		 }

		 private void IncreaseFileSizeTo( long newFileSize )
		 {
			  long currentFileSize;
			  do
			  {
					currentFileSize = CurrentFileSize;
			  } while ( currentFileSize < newFileSize && !UnsafeUtil.compareAndSwapLong( this, _fileSizeOffset, currentFileSize, newFileSize ) );
		 }

		 private long CurrentFileSize
		 {
			 get
			 {
				  return UnsafeUtil.getLongVolatile( this, _fileSizeOffset );
			 }
			 set
			 {
				  UnsafeUtil.putLongVolatile( this, _fileSizeOffset, value );
			 }
		 }


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void acquireLock() throws java.io.IOException
		 private void AcquireLock()
		 {
			  if ( SystemUtils.IS_OS_WINDOWS )
			  {
					// We don't take file locks on the individual store files on Windows, because once you've taking
					// a file lock on a channel, you can only do IO on that file through that channel. This would
					// mean that we can't stripe our FileChannels on Windows, which is the platform that needs striped
					// channels the most because of lack of pwrite and pread support.
					// This is generally fine, because the StoreLocker and the lock file will protect the store from
					// being opened by multiple instances at the same time anyway.
					return;
			  }

			  try
			  {
					_fileLock = _channels[TOKEN_CHANNEL_STRIPE].tryLock();
					if ( _fileLock == null )
					{
						 throw new FileLockException( _file );
					}
			  }
			  catch ( OverlappingFileLockException e )
			  {
					throw new FileLockException( _file, e );
			  }
		 }

		 private StoreChannel Channel( long filePageId )
		 {
			  int stripe = stripe( filePageId );
			  return _channels[stripe];
		 }

		 private int Stripe( long filePageId )
		 {
			  return ( int )( ( long )( ( ulong )filePageId >> _channelStripeShift ) ) & _channelStripeMask;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int swapIn(org.neo4j.io.fs.StoreChannel channel, long bufferAddress, int bufferSize, long fileOffset, int filePageSize) throws java.io.IOException
		 private int SwapIn( StoreChannel channel, long bufferAddress, int bufferSize, long fileOffset, int filePageSize )
		 {
			  int readTotal = 0;
			  try
			  {
					ByteBuffer bufferProxy = Proxy( bufferAddress, filePageSize );
					int read;
					do
					{
						 read = channel.Read( bufferProxy, fileOffset + readTotal );
					} while ( read != -1 && ( readTotal += read ) < filePageSize );

					// Zero-fill the rest.
					Debug.Assert( readTotal >= 0 && filePageSize <= bufferSize && readTotal <= filePageSize, format( "pointer = %h, readTotal = %s, length = %s, page size = %s", bufferAddress, readTotal, filePageSize, bufferSize ) );
					UnsafeUtil.setMemory( bufferAddress + readTotal, filePageSize - readTotal, MuninnPageCache.ZERO_BYTE );
					return readTotal;
			  }
			  catch ( IOException e )
			  {
					throw e;
			  }
			  catch ( Exception e )
			  {
					string msg = format( "Read failed after %s of %s bytes from fileOffset %s", readTotal, filePageSize, fileOffset );
					throw new IOException( msg, e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int swapOut(long bufferAddress, long fileOffset, org.neo4j.io.fs.StoreChannel channel) throws java.io.IOException
		 private int SwapOut( long bufferAddress, long fileOffset, StoreChannel channel )
		 {
			  try
			  {
					ByteBuffer bufferProxy = Proxy( bufferAddress, _filePageSize );
					channel.WriteAll( bufferProxy, fileOffset );
			  }
			  catch ( IOException e )
			  {
					throw e;
			  }
			  catch ( Exception e )
			  {
					throw new IOException( e );
			  }
			  return _filePageSize;
		 }

		 private void Clear( long bufferAddress, int bufferSize )
		 {
			  UnsafeUtil.setMemory( bufferAddress, bufferSize, MuninnPageCache.ZERO_BYTE );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long read(long filePageId, long bufferAddress, int bufferSize) throws java.io.IOException
		 public override long Read( long filePageId, long bufferAddress, int bufferSize )
		 {
			  return ReadAndRetryIfInterrupted( filePageId, bufferAddress, bufferSize, MAX_INTERRUPTED_CHANNEL_REOPEN_ATTEMPTS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long readAndRetryIfInterrupted(long filePageId, long bufferAddress, int bufferSize, int attemptsLeft) throws java.io.IOException
		 private long ReadAndRetryIfInterrupted( long filePageId, long bufferAddress, int bufferSize, int attemptsLeft )
		 {
			  long fileOffset = PageIdToPosition( filePageId );
			  try
			  {
					if ( fileOffset < CurrentFileSize )
					{
						 return SwapIn( Channel( filePageId ), bufferAddress, bufferSize, fileOffset, _filePageSize );
					}
					else
					{
						 Clear( bufferAddress, bufferSize );
					}
			  }
			  catch ( ClosedChannelException e )
			  {
					TryReopen( filePageId, e );

					if ( attemptsLeft < 1 )
					{
						 throw new IOException( "IO failed due to interruption", e );
					}

					bool interrupted = Thread.interrupted();
					long bytesRead = ReadAndRetryIfInterrupted( filePageId, bufferAddress, bufferSize, attemptsLeft - 1 );
					if ( interrupted )
					{
						 Thread.CurrentThread.Interrupt();
					}
					return bytesRead;
			  }
			  return 0;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long read(long startFilePageId, long[] bufferAddresses, int bufferSize, int arrayOffset, int length) throws java.io.IOException
		 public override long Read( long startFilePageId, long[] bufferAddresses, int bufferSize, int arrayOffset, int length )
		 {
			  if ( _positionLockGetter != null && _hasPositionLock )
			  {
					try
					{
						 return ReadPositionedVectoredToFileChannel( startFilePageId, bufferAddresses, arrayOffset, length );
					}
					catch ( IOException ioe )
					{
						 throw ioe;
					}
					catch ( Exception )
					{
						 // There's a lot of reflection going on in that method. We ignore everything that can go wrong, and
						 // isn't exactly an IOException. Instead, we'll try our fallback code and see what it says.
					}
			  }
			  return ReadPositionedVectoredFallback( startFilePageId, bufferAddresses, bufferSize, arrayOffset, length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long readPositionedVectoredToFileChannel(long startFilePageId, long[] bufferAddresses, int arrayOffset, int length) throws Exception
		 private long ReadPositionedVectoredToFileChannel( long startFilePageId, long[] bufferAddresses, int arrayOffset, int length )
		 {
			  long fileOffset = PageIdToPosition( startFilePageId );
			  FileChannel channel = UnwrappedChannel( startFilePageId );
			  ByteBuffer[] srcs = ConvertToByteBuffers( bufferAddresses, arrayOffset, length );
			  long bytesRead = LockPositionReadVectorAndRetryIfInterrupted( startFilePageId, channel, fileOffset, srcs, MAX_INTERRUPTED_CHANNEL_REOPEN_ATTEMPTS );
			  if ( bytesRead == -1 )
			  {
					foreach ( long address in bufferAddresses )
					{
						 UnsafeUtil.setMemory( address, _filePageSize, MuninnPageCache.ZERO_BYTE );
					}
					return 0;
			  }
			  else if ( bytesRead < ( ( long ) _filePageSize ) * length )
			  {
					int pagesRead = ( int )( bytesRead / _filePageSize );
					int bytesReadIntoLastReadPage = ( int )( bytesRead % _filePageSize );
					int pagesNeedingZeroing = length - pagesRead;
					for ( int i = 0; i < pagesNeedingZeroing; i++ )
					{
						 long address = bufferAddresses[arrayOffset + pagesRead + i];
						 long bytesToZero = _filePageSize;
						 if ( i == 0 )
						 {
							  address += bytesReadIntoLastReadPage;
							  bytesToZero -= bytesReadIntoLastReadPage;
						 }
						 UnsafeUtil.setMemory( address, bytesToZero, MuninnPageCache.ZERO_BYTE );
					}
			  }
			  return bytesRead;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long lockPositionReadVectorAndRetryIfInterrupted(long filePageId, java.nio.channels.FileChannel channel, long fileOffset, ByteBuffer[] srcs, int attemptsLeft) throws java.io.IOException
		 private long LockPositionReadVectorAndRetryIfInterrupted( long filePageId, FileChannel channel, long fileOffset, ByteBuffer[] srcs, int attemptsLeft )
		 {
			  try
			  {
					long toRead = _filePageSize * ( long ) srcs.Length;
					long read;
					long readTotal = 0;
					lock ( PositionLock( channel ) )
					{
						 channel.position( fileOffset );
						 do
						 {
							  read = channel.read( srcs );
						 } while ( read != -1 && ( readTotal += read ) < toRead );
						 return readTotal;
					}
			  }
			  catch ( ClosedChannelException e )
			  {
					TryReopen( filePageId, e );

					if ( attemptsLeft < 1 )
					{
						 throw new IOException( "IO failed due to interruption", e );
					}

					bool interrupted = Thread.interrupted();
					channel = UnwrappedChannel( filePageId );
					long bytesWritten = LockPositionReadVectorAndRetryIfInterrupted( filePageId, channel, fileOffset, srcs, attemptsLeft - 1 );
					if ( interrupted )
					{
						 Thread.CurrentThread.Interrupt();
					}
					return bytesWritten;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int readPositionedVectoredFallback(long startFilePageId, long[] bufferAddresses, int bufferSize, int arrayOffset, int length) throws java.io.IOException
		 private int ReadPositionedVectoredFallback( long startFilePageId, long[] bufferAddresses, int bufferSize, int arrayOffset, int length )
		 {
			  int bytes = 0;
			  for ( int i = 0; i < length; i++ )
			  {
					long address = bufferAddresses[arrayOffset + i];
					bytes += ( int )Read( startFilePageId + i, address, bufferSize );
			  }
			  return bytes;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(long filePageId, long bufferAddress) throws java.io.IOException
		 public override long Write( long filePageId, long bufferAddress )
		 {
			  return WriteAndRetryIfInterrupted( filePageId, bufferAddress, MAX_INTERRUPTED_CHANNEL_REOPEN_ATTEMPTS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long writeAndRetryIfInterrupted(long filePageId, long bufferAddress, int attemptsLeft) throws java.io.IOException
		 private long WriteAndRetryIfInterrupted( long filePageId, long bufferAddress, int attemptsLeft )
		 {
			  long fileOffset = PageIdToPosition( filePageId );
			  IncreaseFileSizeTo( fileOffset + _filePageSize );
			  try
			  {
					StoreChannel channel = channel( filePageId );
					return SwapOut( bufferAddress, fileOffset, channel );
			  }
			  catch ( ClosedChannelException e )
			  {
					TryReopen( filePageId, e );

					if ( attemptsLeft < 1 )
					{
						 throw new IOException( "IO failed due to interruption", e );
					}

					bool interrupted = Thread.interrupted();
					long bytesWritten = WriteAndRetryIfInterrupted( filePageId, bufferAddress, attemptsLeft - 1 );
					if ( interrupted )
					{
						 Thread.CurrentThread.Interrupt();
					}
					return bytesWritten;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(long startFilePageId, long[] bufferAddresses, int arrayOffset, int length) throws java.io.IOException
		 public override long Write( long startFilePageId, long[] bufferAddresses, int arrayOffset, int length )
		 {
			  if ( _positionLockGetter != null && _hasPositionLock )
			  {
					try
					{
						 return WritePositionedVectoredToFileChannel( startFilePageId, bufferAddresses, arrayOffset, length );
					}
					catch ( IOException ioe )
					{
						 throw ioe;
					}
					catch ( Exception )
					{
						 // There's a lot of reflection going on in that method. We ignore everything that can go wrong, and
						 // isn't exactly an IOException. Instead, we'll try our fallback code and see what it says.
					}
			  }
			  return WritePositionVectoredFallback( startFilePageId, bufferAddresses, arrayOffset, length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long writePositionedVectoredToFileChannel(long startFilePageId, long[] bufferAddresses, int arrayOffset, int length) throws Exception
		 private long WritePositionedVectoredToFileChannel( long startFilePageId, long[] bufferAddresses, int arrayOffset, int length )
		 {
			  long fileOffset = PageIdToPosition( startFilePageId );
			  IncreaseFileSizeTo( fileOffset + ( ( ( long ) _filePageSize ) * length ) );
			  FileChannel channel = UnwrappedChannel( startFilePageId );
			  ByteBuffer[] srcs = ConvertToByteBuffers( bufferAddresses, arrayOffset, length );
			  return LockPositionWriteVectorAndRetryIfInterrupted( startFilePageId, channel, fileOffset, srcs, MAX_INTERRUPTED_CHANNEL_REOPEN_ATTEMPTS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private ByteBuffer[] convertToByteBuffers(long[] bufferAddresses, int arrayOffset, int length) throws Exception
		 private ByteBuffer[] ConvertToByteBuffers( long[] bufferAddresses, int arrayOffset, int length )
		 {
			  ByteBuffer[] buffers = new ByteBuffer[length];
			  for ( int i = 0; i < length; i++ )
			  {
					long address = bufferAddresses[arrayOffset + i];
					buffers[i] = UnsafeUtil.newDirectByteBuffer( address, _filePageSize );
			  }
			  return buffers;
		 }

		 private FileChannel UnwrappedChannel( long startFilePageId )
		 {
			  StoreChannel storeChannel = Channel( startFilePageId );
			  return StoreFileChannelUnwrapper.unwrap( storeChannel );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long lockPositionWriteVectorAndRetryIfInterrupted(long filePageId, java.nio.channels.FileChannel channel, long fileOffset, ByteBuffer[] srcs, int attemptsLeft) throws java.io.IOException
		 private long LockPositionWriteVectorAndRetryIfInterrupted( long filePageId, FileChannel channel, long fileOffset, ByteBuffer[] srcs, int attemptsLeft )
		 {
			  try
			  {
					long toWrite = _filePageSize * ( long ) srcs.Length;
					long bytesWritten = 0;
					lock ( PositionLock( channel ) )
					{
						 channel.position( fileOffset );
						 do
						 {
							  bytesWritten += channel.write( srcs );
						 } while ( bytesWritten < toWrite );
						 return bytesWritten;
					}
			  }
			  catch ( ClosedChannelException e )
			  {
					TryReopen( filePageId, e );

					if ( attemptsLeft < 1 )
					{
						 throw new IOException( "IO failed due to interruption", e );
					}

					bool interrupted = Thread.interrupted();
					channel = UnwrappedChannel( filePageId );
					long bytesWritten = LockPositionWriteVectorAndRetryIfInterrupted( filePageId, channel, fileOffset, srcs, attemptsLeft - 1 );
					if ( interrupted )
					{
						 Thread.CurrentThread.Interrupt();
					}
					return bytesWritten;
			  }
		 }

		 private object PositionLock( FileChannel channel )
		 {
			  FileChannelImpl impl = ( FileChannelImpl ) channel;
			  try
			  {
					return ( object ) _positionLockGetter.invokeExact( impl );
			  }
			  catch ( Exception th )
			  {
					throw new LinkageError( "No getter for FileChannel.positionLock", th );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int writePositionVectoredFallback(long startFilePageId, long[] bufferAddresses, int arrayOffset, int length) throws java.io.IOException
		 private int WritePositionVectoredFallback( long startFilePageId, long[] bufferAddresses, int arrayOffset, int length )
		 {
			  int bytes = 0;
			  for ( int i = 0; i < length; i++ )
			  {
					long address = bufferAddresses[arrayOffset + i];
					bytes += ( int )Write( startFilePageId + i, address );
			  }
			  return bytes;
		 }

		 public override void Evicted( long filePageId )
		 {
			  PageEvictionCallback callback = this._onEviction;
			  if ( callback != null )
			  {
					callback.OnEvict( filePageId );
			  }
		 }

		 public override File File()
		 {
			  return _file;
		 }

		 private long PageIdToPosition( long pageId )
		 {
			  return _filePageSize * pageId;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  SingleFilePageSwapper that = ( SingleFilePageSwapper ) o;

			  return _file.Equals( that._file );

		 }

		 public override int GetHashCode()
		 {
			  return _file.GetHashCode();
		 }

		 /// <summary>
		 /// Reopens the channel if it has been closed and the close() method on
		 /// this swapper has not been called. In other words, if the channel has
		 /// been "accidentally" closed by an interrupt or the like.
		 /// 
		 /// If the channel has been explicitly closed with the PageSwapper#close()
		 /// method, then this method will re-throw the passed-in exception.
		 /// 
		 /// If the reopening of the file fails with an exception for some reason,
		 /// then that exception is added as a suppressed exception to the passed in
		 /// ClosedChannelException, and the CCE is then rethrown.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private synchronized void tryReopen(long filePageId, java.nio.channels.ClosedChannelException closedException) throws java.nio.channels.ClosedChannelException
		 private void TryReopen( long filePageId, ClosedChannelException closedException )
		 {
			 lock ( this )
			 {
				  int stripe = stripe( filePageId );
				  StoreChannel channel = _channels[stripe];
				  if ( channel.Open )
				  {
						// Someone got ahead of us, presumably. Nothing to do.
						return;
				  }
      
				  if ( _closed )
				  {
						// We've been explicitly closed, so we shouldn't reopen the
						// channel.
						throw closedException;
				  }
      
				  try
				  {
						_channels[stripe] = _fs.open( _file, OpenMode.READ_WRITE );
						if ( stripe == TOKEN_CHANNEL_STRIPE )
						{
							 // The closing of a FileChannel also releases all associated file locks.
							 AcquireLock();
						}
				  }
				  catch ( IOException e )
				  {
						closedException.addSuppressed( e );
						throw closedException;
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void close() throws java.io.IOException
		 public override void Close()
		 {
			 lock ( this )
			 {
				  _closed = true;
				  try
				  {
						CloseAndCollectExceptions( 0, null );
				  }
				  finally
				  {
						// Eagerly relinquish our reference to the onEviction callback, because even though
						// we've closed the PagedFile at this point, there are likely still pages in the cache that are bound to
						// this swapper, and will stay bound, until the eviction threads eventually gets around to kicking them out.
						// It is especially important to null out the onEviction callback field, because it is in turn holding on to
						// the striped translation table, which can be a rather large structure.
						_onEviction = null;
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void closeAndCollectExceptions(int channelIndex, java.io.IOException exception) throws java.io.IOException
		 private void CloseAndCollectExceptions( int channelIndex, IOException exception )
		 {
			  if ( channelIndex == _channels.Length )
			  {
					if ( exception != null )
					{
						 throw exception;
					}
					return;
			  }

			  try
			  {
					_channels[channelIndex].close();
			  }
			  catch ( IOException e )
			  {
					if ( exception == null )
					{
						 exception = e;
					}
					else
					{
						 exception.addSuppressed( e );
					}
			  }
			  CloseAndCollectExceptions( channelIndex + 1, exception );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void closeAndDelete() throws java.io.IOException
		 public override void CloseAndDelete()
		 {
			 lock ( this )
			 {
				  Close();
				  _fs.deleteFile( _file );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void force() throws java.io.IOException
		 public override void Force()
		 {
			  ForceAndRetryIfInterrupted( MAX_INTERRUPTED_CHANNEL_REOPEN_ATTEMPTS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void forceAndRetryIfInterrupted(int attemptsLeft) throws java.io.IOException
		 private void ForceAndRetryIfInterrupted( int attemptsLeft )
		 {
			  try
			  {
					Channel( TOKEN_FILE_PAGE_ID ).force( false );
			  }
			  catch ( ClosedChannelException e )
			  {
					TryReopen( TOKEN_FILE_PAGE_ID, e );

					if ( attemptsLeft < 1 )
					{
						 throw new IOException( "IO failed due to interruption", e );
					}

					bool interrupted = Thread.interrupted();
					ForceAndRetryIfInterrupted( attemptsLeft - 1 );
					if ( interrupted )
					{
						 Thread.CurrentThread.Interrupt();
					}
			  }
		 }

		 public virtual long LastPageId
		 {
			 get
			 {
				  long channelSize = CurrentFileSize;
				  if ( channelSize == 0 )
				  {
						return PageCursor.UNBOUND_PAGE_ID;
				  }
				  long div = channelSize / _filePageSize;
				  long mod = channelSize % _filePageSize;
				  return mod == 0 ? div - 1 : div;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void truncate() throws java.io.IOException
		 public override void Truncate()
		 {
			  TruncateAndRetryIfInterrupted( MAX_INTERRUPTED_CHANNEL_REOPEN_ATTEMPTS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void truncateAndRetryIfInterrupted(int attemptsLeft) throws java.io.IOException
		 private void TruncateAndRetryIfInterrupted( int attemptsLeft )
		 {
			  CurrentFileSize = 0;
			  try
			  {
					Channel( TOKEN_FILE_PAGE_ID ).truncate( 0 );
			  }
			  catch ( ClosedChannelException e )
			  {
					TryReopen( TOKEN_FILE_PAGE_ID, e );

					if ( attemptsLeft < 1 )
					{
						 throw new IOException( "IO failed due to interruption", e );
					}

					bool interrupted = Thread.interrupted();
					TruncateAndRetryIfInterrupted( attemptsLeft - 1 );
					if ( interrupted )
					{
						 Thread.CurrentThread.Interrupt();
					}
			  }
		 }

		 public override string ToString()
		 {
			  return "SingleFilePageSwapper{" +
						 "filePageSize=" + _filePageSize +
						 ", file=" + _file +
						 '}';
		 }
	}

}