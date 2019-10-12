using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.catchup.storecopy
{

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.IOUtils.closeAll;

	public class StreamToDisk : StoreFileStream
	{
		 private WritableByteChannel _writableByteChannel;
		 private IList<AutoCloseable> _closeables;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static StreamToDisk fromFile(org.neo4j.io.fs.FileSystemAbstraction fsa, java.io.File file) throws java.io.IOException
		 internal static StreamToDisk FromFile( FileSystemAbstraction fsa, File file )
		 {
			  return new StreamToDisk( fsa.Open( file, OpenMode.READ_WRITE ) );
		 }

		 private StreamToDisk( WritableByteChannel writableByteChannel, params AutoCloseable[] closeables )
		 {
			  this._writableByteChannel = writableByteChannel;
			  this._closeables = new List<AutoCloseable>();
			  this._closeables.Add( writableByteChannel );
			  ( ( IList<AutoCloseable> )this._closeables ).AddRange( Arrays.asList( closeables ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(byte[] data) throws java.io.IOException
		 public override void Write( sbyte[] data )
		 {
			  ByteBuffer buffer = ByteBuffer.wrap( data );
			  while ( buffer.hasRemaining() )
			  {
					_writableByteChannel.write( buffer );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  closeAll( _closeables );
		 }
	}

}