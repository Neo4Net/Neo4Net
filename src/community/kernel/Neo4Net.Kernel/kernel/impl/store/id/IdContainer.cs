/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.impl.store.id
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OffsetChannel = Neo4Net.Io.fs.OffsetChannel;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;

	/// <summary>
	/// This class handles the persisting of a highest id in use. A sticky byte is present in the header to indicate
	/// whether the file was closed properly. It also handel delegation of reusable ids to the <seealso cref="FreeIdKeeper"/>
	/// class.
	/// 
	/// This class is <b>not thread-safe</b> and synchronization need to be handed by the caller.
	/// </summary>
	public class IdContainer
	{
		 public const long NO_RESULT = -1;

		 // header format: sticky(byte), nextFreeId(long)
		 internal static readonly int HeaderSize = Byte.BYTES + Long.BYTES;

		 // if sticky the id generator wasn't closed properly so it has to be
		 // rebuilt (go through the node, relationship, property, rel type etc files)
		 private static readonly sbyte _cleanGenerator = ( sbyte ) 0;
		 private static readonly sbyte _stickyGenerator = ( sbyte ) 1;

		 private readonly File _file;
		 private readonly FileSystemAbstraction _fs;
		 private StoreChannel _fileChannel;
		 private bool _closed = true;

		 private readonly int _grabSize;
		 private readonly bool _aggressiveReuse;
		 private FreeIdKeeper _freeIdKeeper;

		 private long _initialHighId;

		 public IdContainer( FileSystemAbstraction fs, File file, int grabSize, bool aggressiveReuse )
		 {
			  if ( grabSize < 1 )
			  {
					throw new System.ArgumentException( "Illegal grabSize: " + grabSize );
			  }

			  this._file = file;
			  this._fs = fs;
			  this._grabSize = grabSize;
			  this._aggressiveReuse = aggressiveReuse;
		 }

		 /// <summary>
		 /// Initializes the id generator and performs a simple validation. Returns true if the initialization restored
		 /// properly on disk state, false otherwise (such as creating an id file from scratch).
		 /// Will throw <seealso cref="InvalidIdGeneratorException"/> if the id file is found to be damaged or unclean.
		 /// </summary>
		 public virtual bool Init()
		 {
			  bool result = true;
			  try
			  {
					if ( !_fs.fileExists( _file ) )
					{
						 CreateEmptyIdFile( _fs, _file, 0, false );
						 result = false;
					}

					_fileChannel = _fs.open( _file, OpenMode.READ_WRITE );
					_initialHighId = ReadAndValidateHeader();
					MarkAsSticky();

					this._freeIdKeeper = new FreeIdKeeper( new OffsetChannel( _fileChannel, HeaderSize ), _grabSize, _aggressiveReuse );
					_closed = false;
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( "Unable to init id file " + _file, e );
			  }
			  return result;
		 }

		 public virtual bool Closed
		 {
			 get
			 {
				  return _closed;
			 }
		 }

		 internal virtual long InitialHighId
		 {
			 get
			 {
				  return _initialHighId;
			 }
		 }

		 internal virtual void AssertStillOpen()
		 {
			  if ( _closed )
			  {
					throw new System.InvalidOperationException( "Closed id file " + _file );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long readAndValidateHeader() throws java.io.IOException
		 private long ReadAndValidateHeader()
		 {
			  try
			  {
					return ReadAndValidate( _fileChannel, _file );
			  }
			  catch ( InvalidIdGeneratorException e )
			  {
					_fileChannel.close();
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static long readAndValidate(org.neo4j.io.fs.StoreChannel channel, java.io.File fileName) throws java.io.IOException
		 private static long ReadAndValidate( StoreChannel channel, File fileName )
		 {
			  ByteBuffer buffer = ByteBuffer.allocate( HeaderSize );
			  ReadHeader( fileName, channel, buffer );
			  buffer.flip();
			  sbyte storageStatus = buffer.get();
			  if ( storageStatus != _cleanGenerator )
			  {
					throw new InvalidIdGeneratorException( "Id file not properly shutdown [ " + fileName + " ], delete this id file and build a new one" );
			  }
			  return buffer.Long;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static long readHighId(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File file) throws java.io.IOException
		 internal static long ReadHighId( FileSystemAbstraction fileSystem, File file )
		 {
			  using ( StoreChannel channel = fileSystem.Open( file, OpenMode.READ ) )
			  {
					return ReadAndValidate( channel, file );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static long readDefragCount(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File file) throws java.io.IOException
		 internal static long ReadDefragCount( FileSystemAbstraction fileSystem, File file )
		 {
			  using ( StoreChannel channel = fileSystem.Open( file, OpenMode.READ ) )
			  {
					return FreeIdKeeper.CountFreeIds( new OffsetChannel( channel, HeaderSize ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void markAsSticky() throws java.io.IOException
		 private void MarkAsSticky()
		 {
			  ByteBuffer buffer = ByteBuffer.allocate( Byte.BYTES );
			  buffer.put( _stickyGenerator ).flip();
			  _fileChannel.position( 0 );
			  _fileChannel.writeAll( buffer );
			  _fileChannel.force( false );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void markAsCleanlyClosed() throws java.io.IOException
		 private void MarkAsCleanlyClosed()
		 {
			  // remove sticky
			  ByteBuffer buffer = ByteBuffer.allocate( Byte.BYTES );
			  buffer.put( _cleanGenerator ).flip();
			  _fileChannel.position( 0 );
			  _fileChannel.writeAll( buffer );
		 }

		 public virtual void Close( long highId )
		 {
			  if ( !_closed )
			  {
					try
					{
						 _freeIdKeeper.Dispose();
						 WriteHeader( highId );
						 MarkAsCleanlyClosed();
						 CloseChannel();
					}
					catch ( IOException e )
					{
						 throw new UnderlyingStorageException( "Unable to close id file " + _file, e );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void closeChannel() throws java.io.IOException
		 private void CloseChannel()
		 {
			  _fileChannel.force( false );
			  _fileChannel.close();
			  _fileChannel = null;
			  _closed = true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeHeader(long highId) throws java.io.IOException
		 private void WriteHeader( long highId )
		 {
			  ByteBuffer buffer = ByteBuffer.allocate( HeaderSize );
			  buffer.put( _stickyGenerator ).putLong( highId ).flip();
			  _fileChannel.position( 0 );
			  _fileChannel.writeAll( buffer );
		 }

		 public virtual void Delete()
		 {
			  if ( !_closed )
			  {
					try
					{
						 CloseChannel();
					}
					catch ( IOException e )
					{
						 throw new UnderlyingStorageException( "Unable to close id file " + _file, e );
					}
			  }

			  if ( !_fs.deleteFile( _file ) )
			  {
					throw new UnderlyingStorageException( "Unable to delete id file " + _file );
			  }
		 }

		 /// <returns> next free id or <seealso cref="IdContainer.NO_RESULT"/> if not available </returns>
		 public virtual long ReusableId
		 {
			 get
			 {
				  return _freeIdKeeper.Id;
			 }
		 }

		 public virtual long[] GetReusableIds( int numberOfIds )
		 {
			  return _freeIdKeeper.getIds( numberOfIds );
		 }

		 public virtual IdRange GetReusableIdBatch( int maxSize )
		 {
			  long[] tmpIdArr = new long[maxSize];
			  int count = 0;
			  while ( count < maxSize )
			  {
					long id = _freeIdKeeper.Id;
					if ( id == NO_RESULT )
					{
						 break;
					}
					tmpIdArr[count++] = id;
			  }

			  long[] defragIdArr = count == maxSize ? tmpIdArr : Arrays.copyOfRange( tmpIdArr, 0, count );
			  return new IdRange( defragIdArr, 0, 0 );
		 }

		 public virtual void FreeId( long id )
		 {
			  _freeIdKeeper.freeId( id );
		 }

		 public virtual long FreeIdCount
		 {
			 get
			 {
				  return _freeIdKeeper.Count;
			 }
		 }

		 /// <summary>
		 /// Creates a new id file.
		 /// </summary>
		 /// <param name="file"> The name of the id generator </param>
		 /// <param name="throwIfFileExists"> if {@code true} will cause an <seealso cref="System.InvalidOperationException"/> to be thrown if
		 /// the file already exists. if {@code false} will truncate the file writing the header in it. </param>
		 public static void CreateEmptyIdFile( FileSystemAbstraction fs, File file, long highId, bool throwIfFileExists )
		 {
			  // sanity checks
			  if ( fs == null )
			  {
					throw new System.ArgumentException( "Null filesystem" );
			  }
			  if ( file == null )
			  {
					throw new System.ArgumentException( "Null filename" );
			  }
			  if ( throwIfFileExists && fs.FileExists( file ) )
			  {
					throw new System.InvalidOperationException( "Can't create id file [" + file + "], file already exists" );
			  }
			  try
			  {
					  using ( StoreChannel channel = fs.Create( file ) )
					  {
						// write the header
						channel.Truncate( 0 );
						ByteBuffer buffer = ByteBuffer.allocate( HeaderSize );
						buffer.put( _cleanGenerator ).putLong( highId ).flip();
						channel.WriteAll( buffer );
						channel.Force( false );
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( "Unable to create id file " + file, e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void readHeader(java.io.File file, org.neo4j.io.fs.StoreChannel channel, ByteBuffer buffer) throws java.io.IOException
		 private static void ReadHeader( File file, StoreChannel channel, ByteBuffer buffer )
		 {
			  try
			  {
					channel.ReadAll( buffer );
			  }
			  catch ( System.InvalidOperationException )
			  {
					ByteBuffer exceptionBuffer = buffer.duplicate();
					exceptionBuffer.flip();
					throw new InvalidIdGeneratorException( "Unable to read header of id file [" + file + "], bytes read: " + Arrays.ToString( GetBufferBytes( exceptionBuffer ) ) );
			  }
		 }

		 public override string ToString()
		 {
			  return "IdContainer{" + "file=" + _file + ", fs=" + _fs + ", fileChannel=" + _fileChannel + ", defragCount=" + _freeIdKeeper.Count + ", grabSize=" + _grabSize + ", aggressiveReuse=" + _aggressiveReuse + ", closed=" + _closed + '}';
		 }

		 private static sbyte[] GetBufferBytes( ByteBuffer buffer )
		 {
			  sbyte[] bytes = new sbyte[buffer.limit()];
			  buffer.get( bytes );
			  return bytes;
		 }
	}

}