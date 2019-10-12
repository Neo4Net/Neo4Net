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

	public class LogHeaderReader
	{
		 private LogHeaderReader()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static LogHeader readLogHeader(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File file) throws java.io.IOException
		 public static LogHeader ReadLogHeader( FileSystemAbstraction fileSystem, File file )
		 {
			  return ReadLogHeader( fileSystem, file, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static LogHeader readLogHeader(org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.io.File file, boolean strict) throws java.io.IOException
		 public static LogHeader ReadLogHeader( FileSystemAbstraction fileSystem, File file, bool strict )
		 {
			  using ( StoreChannel channel = fileSystem.Open( file, OpenMode.READ ) )
			  {
					return ReadLogHeader( ByteBuffer.allocate( LOG_HEADER_SIZE ), channel, strict, file );
			  }
		 }

		 /// <summary>
		 /// Reads the header of a log. Data will be read from {@code channel} using supplied {@code buffer}
		 /// as to allow more controlled allocation.
		 /// </summary>
		 /// <param name="buffer"> <seealso cref="ByteBuffer"/> to read into. Passed in to allow control over allocation. </param>
		 /// <param name="channel"> <seealso cref="ReadableByteChannel"/> to read from, typically a channel over a file containing the data. </param>
		 /// <param name="strict"> if {@code true} then will fail with <seealso cref="IncompleteLogHeaderException"/> on incomplete
		 /// header, i.e. if there's not enough data in the channel to even read the header. If {@code false} then
		 /// the return value will instead be {@code null}. </param>
		 /// <param name="fileForAdditionalErrorInformationOrNull"> when in {@code strict} mode the exception can be
		 /// amended with information about which file the channel represents, if any. Purely for better forensics
		 /// ability. </param>
		 /// <returns> <seealso cref="LogHeader"/> containing the log header data from the {@code channel}. </returns>
		 /// <exception cref="IOException"> if unable to read from {@code channel} </exception>
		 /// <exception cref="IncompleteLogHeaderException"> if {@code strict} and not enough data could be read </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static LogHeader readLogHeader(ByteBuffer buffer, java.nio.channels.ReadableByteChannel channel, boolean strict, java.io.File fileForAdditionalErrorInformationOrNull) throws java.io.IOException
		 public static LogHeader ReadLogHeader( ByteBuffer buffer, ReadableByteChannel channel, bool strict, File fileForAdditionalErrorInformationOrNull )
		 {
			  buffer.clear();
			  buffer.limit( LOG_HEADER_SIZE );

			  int read = channel.read( buffer );
			  if ( read != LOG_HEADER_SIZE )
			  {
					if ( strict )
					{
						 if ( fileForAdditionalErrorInformationOrNull != null )
						 {
							  throw new IncompleteLogHeaderException( fileForAdditionalErrorInformationOrNull, read );
						 }
						 throw new IncompleteLogHeaderException( read );
					}
					return null;
			  }
			  buffer.flip();
			  long encodedLogVersions = buffer.Long;
			  sbyte logFormatVersion = DecodeLogFormatVersion( encodedLogVersions );
			  long logVersion = DecodeLogVersion( encodedLogVersions );
			  long previousCommittedTx = buffer.Long;
			  return new LogHeader( logFormatVersion, logVersion, previousCommittedTx );
		 }

		 internal static long DecodeLogVersion( long encLogVersion )
		 {
			  return encLogVersion & 0x00FFFFFFFFFFFFFFL;
		 }

		 internal static sbyte DecodeLogFormatVersion( long encLogVersion )
		 {
			  return unchecked( ( sbyte )( ( encLogVersion >> 56 ) & 0xFF ) );
		 }
	}

}