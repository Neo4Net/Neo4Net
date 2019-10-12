using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.com.storecopy
{

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;

	public class ToFileStoreWriter : StoreWriter
	{
		 private readonly File _basePath;
		 private readonly FileSystemAbstraction _fs;
		 private readonly StoreCopyClientMonitor _monitor;

		 public ToFileStoreWriter( File graphDbStoreDir, FileSystemAbstraction fs, StoreCopyClientMonitor storeCopyClientMonitor )
		 {
			  this._basePath = graphDbStoreDir;
			  this._fs = fs;
			  this._monitor = storeCopyClientMonitor;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(String path, java.nio.channels.ReadableByteChannel data, ByteBuffer temporaryBuffer, boolean hasData, int requiredElementAlignment) throws java.io.IOException
		 public override long Write( string path, ReadableByteChannel data, ByteBuffer temporaryBuffer, bool hasData, int requiredElementAlignment )
		 {
			  try
			  {
					temporaryBuffer.clear();
					File file = new File( _basePath, path );
					file.ParentFile.mkdirs();

					string fullFilePath = file.ToString();

					_monitor.startReceivingStoreFile( fullFilePath );
					try
					{
						 // We don't add file move actions for these files. The reason is that we will perform the file moves
						 // *after* we have done recovery on the store, and this may delete some files, and add other files.
						 return WriteDataThroughFileSystem( file, data, temporaryBuffer, hasData );
					}
					finally
					{
						 _monitor.finishReceivingStoreFile( fullFilePath );
					}
			  }
			  catch ( Exception t )
			  {
					throw new IOException( t );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long writeDataThroughFileSystem(java.io.File file, java.nio.channels.ReadableByteChannel data, ByteBuffer temporaryBuffer, boolean hasData) throws java.io.IOException
		 private long WriteDataThroughFileSystem( File file, ReadableByteChannel data, ByteBuffer temporaryBuffer, bool hasData )
		 {
			  using ( StoreChannel channel = _fs.create( file ) )
			  {
					return WriteData( data, temporaryBuffer, hasData, channel );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long writeData(java.nio.channels.ReadableByteChannel data, ByteBuffer temporaryBuffer, boolean hasData, java.nio.channels.WritableByteChannel channel) throws java.io.IOException
		 private long WriteData( ReadableByteChannel data, ByteBuffer temporaryBuffer, bool hasData, WritableByteChannel channel )
		 {
			  long totalToWrite = 0;
			  long totalWritten = 0;
			  if ( hasData )
			  {
					while ( data.read( temporaryBuffer ) >= 0 )
					{
						 temporaryBuffer.flip();
						 totalToWrite += temporaryBuffer.limit();
						 int bytesWritten;
						 while ( ( totalWritten += bytesWritten = channel.write( temporaryBuffer ) ) < totalToWrite )
						 {
							  if ( bytesWritten < 0 )
							  {
									throw new IOException( "Unable to write to disk, reported bytes written was " + bytesWritten );
							  }
						 }
						 temporaryBuffer.clear();
					}
			  }
			  return totalWritten;
		 }

		 public override void Close()
		 {
			  // Do nothing
		 }
	}

}