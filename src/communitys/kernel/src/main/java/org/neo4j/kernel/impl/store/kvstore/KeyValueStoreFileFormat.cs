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
namespace Neo4Net.Kernel.impl.store.kvstore
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;

	/// <summary>
	/// Defines the format of a <seealso cref="KeyValueStoreFile"/>.
	/// </summary>
	public abstract class KeyValueStoreFileFormat
	{
		 private readonly int _maxSize;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final HeaderField<?>[] headerFields;
		 private readonly HeaderField<object>[] _headerFields;

		 /// <param name="maxSize">      the largest possible size of a key or value that conforms to this format. </param>
		 /// <param name="headerFields"> identifiers for the entries to write from the metadata to the store. </param>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public KeyValueStoreFileFormat(int maxSize, HeaderField<?>... headerFields)
		 public KeyValueStoreFileFormat( int maxSize, params HeaderField<object>[] headerFields )
		 {
			  if ( maxSize < 0 )
			  {
					throw new System.ArgumentException( "Negative maxSize: " + maxSize );
			  }
			  this._maxSize = maxSize;
			  this._headerFields = headerFields.Clone();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final KeyValueStoreFile createStore(org.neo4j.io.fs.FileSystemAbstraction fs, org.neo4j.io.pagecache.PageCache pages, java.io.File path, int keySize, int valueSize, Headers headers, DataProvider data) throws java.io.IOException
		 public KeyValueStoreFile CreateStore( FileSystemAbstraction fs, PageCache pages, File path, int keySize, int valueSize, Headers headers, DataProvider data )
		 {
			  return Create( requireNonNull( fs, typeof( FileSystemAbstraction ).Name ), requireNonNull( path, "path" ), requireNonNull( pages, typeof( PageCache ).Name ), keySize, valueSize, requireNonNull( headers, "headers" ), requireNonNull( data, "data" ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final void createEmptyStore(org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File path, int keySize, int valueSize, Headers headers) throws java.io.IOException
		 public void CreateEmptyStore( FileSystemAbstraction fs, File path, int keySize, int valueSize, Headers headers )
		 {
			  Create( requireNonNull( fs, typeof( FileSystemAbstraction ).Name ), requireNonNull( path, "path" ), null, keySize, valueSize, requireNonNull( headers, "headers" ), null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final KeyValueStoreFile openStore(org.neo4j.io.fs.FileSystemAbstraction fs, org.neo4j.io.pagecache.PageCache pages, java.io.File path) throws java.io.IOException
		 public KeyValueStoreFile OpenStore( FileSystemAbstraction fs, PageCache pages, File path )
		 {
			  return Open( requireNonNull( fs, typeof( FileSystemAbstraction ).Name ), requireNonNull( path, "path" ), requireNonNull( pages, typeof( PageCache ).Name ) );
		 }

		 protected internal abstract void WriteFormatSpecifier( WritableBuffer formatSpecifier );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: protected HeaderField<?>[] headerFieldsForFormat(ReadableBuffer formatSpecifier)
		 protected internal virtual HeaderField<object>[] HeaderFieldsForFormat( ReadableBuffer formatSpecifier )
		 {
			  return _headerFields.Clone();
		 }

		 // IMPLEMENTATION

		 /// <summary>
		 /// Create a collector for interpreting metadata from a file. </summary>
		 private MetadataCollector Metadata( ReadableBuffer formatSpecifier, int pageSize, int keySize, int valueSize )
		 {
			  sbyte[] format = new sbyte[formatSpecifier.Size()];
			  for ( int i = 0; i < format.Length; i++ )
			  {
					format[i] = formatSpecifier.GetByte( i );
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final BigEndianByteArrayBuffer specifier = new BigEndianByteArrayBuffer(format);
			  BigEndianByteArrayBuffer specifier = new BigEndianByteArrayBuffer( format );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: HeaderField<?>[] headerFields = headerFieldsForFormat(formatSpecifier);
			  HeaderField<object>[] headerFields = HeaderFieldsForFormat( formatSpecifier );
			  return new MetadataCollectorAnonymousInnerClass( this, headerFields, specifier );
		 }

		 private class MetadataCollectorAnonymousInnerClass : MetadataCollector
		 {
			 private readonly KeyValueStoreFileFormat _outerInstance;

			 public MetadataCollectorAnonymousInnerClass<T1>( KeyValueStoreFileFormat outerInstance, Neo4Net.Kernel.impl.store.kvstore.HeaderField<T1>[] headerFields, Neo4Net.Kernel.impl.store.kvstore.BigEndianByteArrayBuffer specifier ) : base( pageSize / ( keySize + valueSize ), headerFields, specifier )
			 {
				 this.outerInstance = outerInstance;
			 }

			 internal override bool verifyFormatSpecifier( ReadableBuffer value )
			 {
				  int size = value.Size();
				  ReadableBuffer expectedFormat = expectedFormat();
				  if ( size == expectedFormat.Size() )
				  {
						for ( int i = 0; i < size; i++ )
						{
							 if ( value.GetByte( i ) != expectedFormat.GetByte( i ) )
							 {
								  return false;
							 }
						}
						return true;
				  }
				  return false;
			 }
		 }

		 /// <summary>
		 /// Create a new store file.
		 /// </summary>
		 /// <param name="fs">           the file system that should hold the store file. </param>
		 /// <param name="path">         the location in the file system where the store file resides. </param>
		 /// <param name="pages">        if {@code null} the newly created store fill will not be opened. </param>
		 /// <param name="keySize">      the size of the keys in the new store. </param>
		 /// <param name="valueSize">    the size of the values in the new store. </param>
		 /// <param name="headers">      the headers to write to the store. </param>
		 /// <param name="dataProvider"> the data to write into the store, {@code null} is accepted to mean no data. </param>
		 /// <returns> an opened version of the newly created store file - iff a <seealso cref="PageCache"/> was provided. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private KeyValueStoreFile create(org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File path, org.neo4j.io.pagecache.PageCache pages, int keySize, int valueSize, Headers headers, DataProvider dataProvider) throws java.io.IOException
		 private KeyValueStoreFile Create( FileSystemAbstraction fs, File path, PageCache pages, int keySize, int valueSize, Headers headers, DataProvider dataProvider )
		 {
			  if ( keySize <= 0 || keySize > _maxSize || valueSize <= 0 || valueSize > _maxSize )
			  {
					throw new System.ArgumentException( string.Format( "Invalid sizes: keySize={0:D}, valueSize={1:D}, format maxSize={2:D}", keySize, valueSize, _maxSize ) );
			  }

			  if ( fs.FileExists( path ) )
			  {
					fs.Truncate( path, 0 );
			  }

			  BigEndianByteArrayBuffer key = new BigEndianByteArrayBuffer( new sbyte[keySize] );
			  BigEndianByteArrayBuffer value = new BigEndianByteArrayBuffer( new sbyte[valueSize] );

			  // format specifier
			  WriteFormatSpecifier( value );
			  if ( !ValidFormatSpecifier( value.Buffer, keySize ) )
			  {
					throw new System.ArgumentException( "Invalid Format specifier: " + BigEndianByteArrayBuffer.ToString( value.Buffer ) );
			  }

			  int pageSize = pageSize( pages, keySize, valueSize );
			  using ( KeyValueWriter writer = NewWriter( fs, path, value, pages, pageSize, keySize, valueSize ), DataProvider data = dataProvider )
			  {
					// header
					if ( !writer.WriteHeader( key, value ) )
					{
						 throw new System.InvalidOperationException( "The format specifier should be a valid header value" );
					}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (HeaderField<?> header : headerFields)
					foreach ( HeaderField<object> header in _headerFields )
					{
						 headers.Write( header, value );
						 if ( !writer.WriteHeader( key, value ) )
						 {
							  throw new System.ArgumentException( "Invalid header value. " + header + ": " + value );
						 }
					}
					if ( _headerFields.Length == 0 )
					{
						 if ( !writer.WriteHeader( key, value ) )
						 {
							  throw new System.InvalidOperationException( "A padding header should be valid." );
						 }
					}

					// data
					long dataEntries = 0;
					for ( ; data != null && data.Visit( key, value ); dataEntries++ )
					{
						 writer.WriteData( key, value );
					}
					// 'data' is allowed to write into the buffers even if it returns false, so we need to clear them
					key.Clear();
					value.Clear();

					// trailer
					value.PutIntegerAtEnd( dataEntries == 0 ? -1 : dataEntries );
					if ( !writer.WriteHeader( key, value ) )
					{
						 throw new System.InvalidOperationException( "The trailing size header should be valid" );
					}

					return writer.OpenStoreFile();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private KeyValueWriter newWriter(org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File path, ReadableBuffer formatSpecifier, org.neo4j.io.pagecache.PageCache pages, int pageSize, int keySize, int valueSize) throws java.io.IOException
		 private KeyValueWriter NewWriter( FileSystemAbstraction fs, File path, ReadableBuffer formatSpecifier, PageCache pages, int pageSize, int keySize, int valueSize )
		 {
			  return KeyValueWriter.Create( Metadata( formatSpecifier, pageSize, keySize, valueSize ), fs, pages, path, pageSize );
		 }

		 /// <summary>
		 /// Opens an existing store file.
		 /// </summary>
		 /// <param name="fs">    the file system which holds the store file. </param>
		 /// <param name="path">  the location in the file system where the store file resides. </param>
		 /// <param name="pages"> the page cache to use for opening the store file. </param>
		 /// <returns> the opened store file. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private KeyValueStoreFile open(org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File path, org.neo4j.io.pagecache.PageCache pages) throws java.io.IOException
		 private KeyValueStoreFile Open( FileSystemAbstraction fs, File path, PageCache pages )
		 {
			  ByteBuffer buffer = ByteBuffer.wrap( new sbyte[_maxSize * 4] );
			  using ( StoreChannel channel = fs.Open( path, OpenMode.READ ) )
			  {
					while ( buffer.hasRemaining() )
					{
						 int bytes = channel.read( buffer );
						 if ( bytes == -1 )
						 {
							  break;
						 }
					}
			  }
			  buffer.flip();
			  // compute the key sizes
			  int keySize = 0;
			  while ( buffer.hasRemaining() && buffer.get() == 0 )
			  {
					if ( ++keySize > _maxSize )
					{
						 throw new IOException( "Invalid header, key size too large." );
					}
			  }
			  // compute the value size
			  int valueSize = 1; // start at 1, since we've seen the first non-zero byte
			  for ( int zeros = 0; zeros <= keySize; zeros++ )
			  {
					if ( !buffer.hasRemaining() )
					{
						 throw new IOException( "Invalid value size: " + valueSize );
					}
					if ( buffer.get() != 0 )
					{
						 zeros = 0;
					}
					if ( ++valueSize - keySize > _maxSize )
					{
						 throw new IOException( "Invalid header, value size too large." );
					}
			  }
			  valueSize -= keySize; // we read in the next zero-key
			  // compute a page size that aligns with the <key,value>-tuple size
			  int pageSize = pageSize( pages, keySize, valueSize );
			  // read the store metadata
			  {
					BigEndianByteArrayBuffer formatSpecifier = new BigEndianByteArrayBuffer( new sbyte[valueSize] );
					WriteFormatSpecifier( formatSpecifier );

					PagedFile file = pages.Map( path, pageSize );
					try
					{
						 BigEndianByteArrayBuffer key = new BigEndianByteArrayBuffer( new sbyte[keySize] );
						 BigEndianByteArrayBuffer value = new BigEndianByteArrayBuffer( new sbyte[valueSize] );
						 // the first value is the format identifier, pass it along
						 buffer.position( keySize );
						 buffer.limit( keySize + valueSize );
						 value.DataFrom( buffer );

						 MetadataCollector metadata = metadata( formatSpecifier, pageSize, keySize, valueSize );
						 // scan and catalogue all entries in the file
						 KeyValueStoreFile.ScanAll( file, 0, metadata, key, value );
						 KeyValueStoreFile storeFile = new KeyValueStoreFile( file, keySize, valueSize, metadata );
						 file = null;
						 return storeFile;
					}
					finally
					{
						 if ( file != null )
						 {
							  file.Close();
						 }
					}
			  }
		 }

		 private static int PageSize( PageCache pages, int keySize, int valueSize )
		 {
			  int pageSize = pages == null ? Neo4Net.Io.pagecache.PageCache_Fields.PAGE_SIZE : pages.PageSize();
			  pageSize -= pageSize % ( keySize + valueSize );
			  return pageSize;
		 }

		 internal static bool ValidFormatSpecifier( sbyte[] buffer, int keySize )
		 {
			  for ( int i = 0, key = 0; i < buffer.Length; i++ )
			  {
					if ( buffer[i] == 0 )
					{
						 if ( i == 0 || ++key == keySize || i == buffer.Length - 1 )
						 {
							  return false;
						 }
					}
					else
					{
						 key = 0;
					}
			  }
			  return true;
		 }
	}

}