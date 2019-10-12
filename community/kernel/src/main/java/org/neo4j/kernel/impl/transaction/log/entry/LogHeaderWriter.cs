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
namespace Org.Neo4j.Kernel.impl.transaction.log.entry
{

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogVersions.CURRENT_FORMAT_VERSION;

	public class LogHeaderWriter
	{
		 private LogHeaderWriter()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void writeLogHeader(org.neo4j.kernel.impl.transaction.log.FlushableChannel channel, long logVersion, long previousCommittedTxId) throws java.io.IOException
		 public static void WriteLogHeader( FlushableChannel channel, long logVersion, long previousCommittedTxId )
		 {
			  channel.PutLong( EncodeLogVersion( logVersion ) );
			  channel.PutLong( previousCommittedTxId );
		 }

		 public static ByteBuffer WriteLogHeader( ByteBuffer buffer, long logVersion, long previousCommittedTxId )
		 {
			  buffer.clear();
			  buffer.putLong( EncodeLogVersion( logVersion ) );
			  buffer.putLong( previousCommittedTxId );
			  buffer.flip();
			  return buffer;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void writeLogHeader(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File file, long logVersion, long previousLastCommittedTxId) throws java.io.IOException
		 public static void WriteLogHeader( FileSystemAbstraction fileSystem, File file, long logVersion, long previousLastCommittedTxId )
		 {
			  using ( StoreChannel channel = fileSystem.Open( file, OpenMode.READ_WRITE ) )
			  {
					WriteLogHeader( channel, logVersion, previousLastCommittedTxId );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void writeLogHeader(org.neo4j.io.fs.StoreChannel channel, long logVersion, long previousLastCommittedTxId) throws java.io.IOException
		 public static void WriteLogHeader( StoreChannel channel, long logVersion, long previousLastCommittedTxId )
		 {
			  ByteBuffer buffer = ByteBuffer.allocate( LOG_HEADER_SIZE );
			  WriteLogHeader( buffer, logVersion, previousLastCommittedTxId );
			  channel.WriteAll( buffer );
		 }

		 public static long EncodeLogVersion( long logVersion )
		 {
			  return logVersion | ( ( ( long ) CURRENT_FORMAT_VERSION ) << 56 );
		 }
	}

}