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
	/// A <seealso cref="Readable"/>, but focused on {@code char[]}, via a <seealso cref="SectionedCharBuffer"/> with one of the main reasons
	/// that <seealso cref="Reader.read(CharBuffer)"/> creates a new {@code char[]} as big as the data it's about to read
	/// every call. However <seealso cref="Reader.read(char[], int, int)"/> doesn't, and so leaves no garbage.
	/// 
	/// The fact that this is a separate interface means that <seealso cref="Readable"/> instances need to be wrapped,
	/// but that's fine since the buffer size should be reasonably big such that <seealso cref="read(SectionedCharBuffer, int)"/>
	/// isn't called too often. Therefore the wrapping overhead should not be noticeable at all.
	/// 
	/// Also took the opportunity to let <seealso cref="CharReadable"/> extends <seealso cref="System.IDisposable"/>, something that
	/// <seealso cref="Readable"/> doesn't.
	/// </summary>
	public interface ICharReadable : System.IDisposable, SourceTraceability
	{
		 /// <summary>
		 /// Reads characters into the <seealso cref="SectionedCharBuffer buffer"/>.
		 /// This method will block until data is available, an I/O error occurs, or the end of the stream is reached.
		 /// The caller is responsible for passing in {@code from} which index existing characters should be saved,
		 /// using <seealso cref="SectionedCharBuffer.compact(SectionedCharBuffer, int) compaction"/>, before reading into the
		 /// front section of the buffer, using <seealso cref="SectionedCharBuffer.readFrom(Reader)"/>.
		 /// The returned <seealso cref="SectionedCharBuffer"/> can be the same as got passed in, or another buffer if f.ex.
		 /// double-buffering is used. If this reader reached eof, i.e. equal state to that of <seealso cref="Reader.read(char[])"/>
		 /// returning {@code -1} then <seealso cref="SectionedCharBuffer.hasAvailable()"/> for the returned instances will
		 /// return {@code false}.
		 /// </summary>
		 /// <param name="buffer"> <seealso cref="SectionedCharBuffer"/> to read new data into. </param>
		 /// <param name="from"> index into the buffer array where characters to save (compact) starts (inclusive). </param>
		 /// <returns> a <seealso cref="SectionedCharBuffer"/> containing new data. </returns>
		 /// <exception cref="IOException"> if an I/O error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: SectionedCharBuffer read(SectionedCharBuffer buffer, int from) throws java.io.IOException;
		 SectionedCharBuffer Read( SectionedCharBuffer buffer, int from );

		 /// <summary>
		 /// Reads characters into the given array starting at {@code offset}, reading {@code length} number of characters.
		 /// 
		 /// Similar to <seealso cref="Reader.read(char[], int, int)"/> </summary>
		 /// <param name="into"> char[] to read the data into. </param>
		 /// <param name="offset"> offset to start reading into the char[]. </param>
		 /// <param name="length"> number of bytes to read maximum. </param>
		 /// <returns> number of bytes read, or 0 if there were no bytes read and end of readable is reached. </returns>
		 /// <exception cref="IOException"> on read error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int read(char[] into, int offset, int length) throws java.io.IOException;
		 int Read( char[] into, int offset, int length );

		 /// <returns> length of this source, in bytes. </returns>
		 long Length();

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 CharReadable EMPTY = new CharReadable()
	//	 {
	//		  @@Override public long position()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public String sourceDescription()
	//		  {
	//				return "EMPTY";
	//		  }
	//
	//		  @@Override public int read(char[] into, int offset, int length)
	//		  {
	//				return -1;
	//		  }
	//
	//		  @@Override public SectionedCharBuffer read(SectionedCharBuffer buffer, int from)
	//		  {
	//				buffer.compact(buffer, from);
	//				return buffer;
	//		  }
	//
	//		  @@Override public long length()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public void close()
	//		  {
	//		  }
	//	 };
	}

	 public abstract class CharReadable_Adapter : SourceTraceability_Adapter, CharReadable
	 {
		 public override abstract string SourceDescription();
		 public abstract long Length();
		 public abstract int Read( char[] into, int offset, int length );
		 public abstract SectionedCharBuffer Read( SectionedCharBuffer buffer, int from );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		  public override void Close()
		  { // Nothing to close
		  }
	 }

}