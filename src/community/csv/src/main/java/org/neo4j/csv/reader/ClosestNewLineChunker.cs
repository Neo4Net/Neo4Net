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
namespace Neo4Net.Csv.Reader
{

	/// <summary>
	/// In a scenario where there's one reader reading chunks of data, handing those chunks to one or
	/// more processors (parsers) of that data, this class comes in handy. This pattern allows for
	/// multiple <seealso cref="BufferedCharSeeker seeker instances"/>, each operating over one chunk, not transitioning itself
	/// into the next.
	/// </summary>
	public class ClosestNewLineChunker : CharReadableChunker
	{
		 public ClosestNewLineChunker( CharReadable reader, int chunkSize ) : base( reader, chunkSize )
		 {
		 }

		 /// <summary>
		 /// Fills the given chunk with data from the underlying <seealso cref="CharReadable"/>, up to a good cut-off point
		 /// in the vicinity of the buffer size.
		 /// </summary>
		 /// <param name="chunk"> <seealso cref="Chunk"/> to read data into. </param>
		 /// <returns> the next <seealso cref="Chunk"/> of data, ending with a new-line or not for the last chunk. </returns>
		 /// <exception cref="IOException"> on reading error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized boolean nextChunk(org.Neo4Net.csv.reader.Source_Chunk chunk) throws java.io.IOException
		 public override bool NextChunk( Source_Chunk chunk )
		 {
			 lock ( this )
			 {
				  ChunkImpl into = ( ChunkImpl ) chunk;
				  int offset = FillFromBackBuffer( into.Buffer );
				  int leftToRead = ChunkSize - offset;
				  int read = Reader.read( into.Buffer, offset, leftToRead );
				  if ( read == leftToRead )
				  { // Read from reader. We read data into the whole buffer and there seems to be more data left in reader.
						// This means we're most likely not at the end so seek backwards to the last newline character and
						// put the characters after the newline character(s) into the back buffer.
						int newlineOffset = OffsetOfLastNewline( into.Buffer );
						if ( newlineOffset > -1 )
						{ // We found a newline character some characters back
							 read -= StoreInBackBuffer( into.Data(), newlineOffset + 1, ChunkSize - (newlineOffset + 1) );
						}
						else
						{ // There was no newline character, isn't that weird?
							 throw new System.InvalidOperationException( "Weird input data, no newline character in the whole buffer " + ChunkSize + ", not supported a.t.m." );
						}
				  }
				  // else we couldn't completely fill the buffer, this means that we're at the end of a data source, we're good.
      
				  if ( read > 0 )
				  {
						offset += read;
						PositionConflict += read;
						into.Initialize( offset, Reader.sourceDescription() );
						return true;
				  }
				  return false;
			 }
		 }

		 private static int OffsetOfLastNewline( char[] buffer )
		 {
			  for ( int i = buffer.Length - 1; i >= 0; i-- )
			  {
					if ( buffer[i] == '\n' )
					{
						 return i;
					}
			  }
			  return -1;
		 }
	}

}