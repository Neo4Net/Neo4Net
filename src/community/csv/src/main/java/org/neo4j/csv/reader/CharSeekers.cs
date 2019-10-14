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

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.csv.reader.Configuration_Fields.DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.csv.reader.ThreadAheadReadable.threadAhead;

	/// <summary>
	/// Factory for common <seealso cref="CharSeeker"/> implementations.
	/// </summary>
	public class CharSeekers
	{
		 private CharSeekers()
		 {
		 }

		 /// <summary>
		 /// Instantiates a <seealso cref="BufferedCharSeeker"/> with optional <seealso cref="ThreadAheadReadable read-ahead"/> capability.
		 /// </summary>
		 /// <param name="reader"> the <seealso cref="CharReadable"/> which is the source of data, f.ex. a <seealso cref="System.IO.StreamReader_FileReader"/>. </param>
		 /// <param name="config"> <seealso cref="Configuration"/> for the resulting <seealso cref="CharSeeker"/>. </param>
		 /// <param name="readAhead"> whether or not to start a <seealso cref="ThreadAheadReadable read-ahead thread"/>
		 /// which strives towards always keeping one buffer worth of data read and available from I/O when it's
		 /// time for the <seealso cref="BufferedCharSeeker"/> to read more data. </param>
		 /// <returns> a <seealso cref="CharSeeker"/> with optional <seealso cref="ThreadAheadReadable read-ahead"/> capability. </returns>
		 public static CharSeeker CharSeeker( CharReadable reader, Configuration config, bool readAhead )
		 {
			  if ( readAhead )
			  { // Thread that always has one buffer read ahead
					reader = threadAhead( reader, config.BufferSize() );
			  }

			  // Give the reader to the char seeker
			  return new BufferedCharSeeker( new AutoReadingSource( reader, config.BufferSize() ), config );
		 }

		 /// <summary>
		 /// Instantiates a <seealso cref="BufferedCharSeeker"/> with optional <seealso cref="ThreadAheadReadable read-ahead"/> capability.
		 /// </summary>
		 /// <param name="reader"> the <seealso cref="CharReadable"/> which is the source of data, f.ex. a <seealso cref="System.IO.StreamReader_FileReader"/>. </param>
		 /// <param name="bufferSize"> buffer size of the seeker and, if enabled, the read-ahead thread. </param>
		 /// <param name="readAhead"> whether or not to start a <seealso cref="ThreadAheadReadable read-ahead thread"/>
		 /// which strives towards always keeping one buffer worth of data read and available from I/O when it's
		 /// time for the <seealso cref="BufferedCharSeeker"/> to read more data. </param>
		 /// <param name="quotationCharacter"> character to interpret quotation character. </param>
		 /// <returns> a <seealso cref="CharSeeker"/> with optional <seealso cref="ThreadAheadReadable read-ahead"/> capability. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static CharSeeker charSeeker(CharReadable reader, final int bufferSize, boolean readAhead, final char quotationCharacter)
		 public static CharSeeker CharSeeker( CharReadable reader, int bufferSize, bool readAhead, char quotationCharacter )
		 {
			  return charSeeker(reader, new Configuration_OverriddenAnonymousInnerClass(DEFAULT, bufferSize, quotationCharacter)
			 , readAhead);
		 }

		 private class Configuration_OverriddenAnonymousInnerClass : Configuration_Overridden
		 {
			 private int _bufferSize;
			 private char _quotationCharacter;

			 public Configuration_OverriddenAnonymousInnerClass( UnknownType configurationFields, int bufferSize, char quotationCharacter ) : base( configurationFields.Default )
			 {
				 this._bufferSize = bufferSize;
				 this._quotationCharacter = quotationCharacter;
			 }

			 public override char quotationCharacter()
			 {
				  return _quotationCharacter;
			 }

			 public override int bufferSize()
			 {
				  return _bufferSize;
			 }
		 }
	}

}