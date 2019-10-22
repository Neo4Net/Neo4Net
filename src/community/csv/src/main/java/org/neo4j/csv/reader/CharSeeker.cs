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

	using CSVHeaderInformation = Neo4Net.Values.Storable.CSVHeaderInformation;
	/// <summary>
	/// Seeks for specific characters in a stream of characters, e.g. a <seealso cref="CharReadable"/>. Uses a <seealso cref="Mark"/>
	/// as keeper of position. Once a <seealso cref="seek(Mark, int)"/> has succeeded the characters specified by
	/// the mark can be <seealso cref="extract(Mark, Extractor, CSVHeaderInformation) extracted"/> into a value of an arbitrary type.
	/// 
	/// Typical usage is:
	/// 
	/// <pre>
	/// CharSeeker seeker = ...
	/// Mark mark = new Mark();
	/// int[] delimiters = new int[] {'\t',','};
	/// 
	/// while ( seeker.seek( mark, delimiters ) )
	/// {
	///     String value = seeker.extract( mark, Extractors.STRING );
	///     // ... somehow manage the value
	///     if ( mark.isEndOfLine() )
	///     {
	///         // ... end of line, put some logic to handle that here
	///     }
	/// }
	/// </pre>
	/// 
	/// Any <seealso cref="System.IDisposable"/> resource that gets passed in will be closed in <seealso cref="close()"/>.
	/// 
	/// @author Mattias Persson
	/// </summary>
	public interface CharSeeker : System.IDisposable, SourceTraceability
	{
		 /// <summary>
		 /// Seeks the next occurrence of any of the characters in {@code untilOneOfChars}, or if end-of-line,
		 /// or even end-of-file.
		 /// </summary>
		 /// <param name="mark"> the mutable <seealso cref="Mark"/> which will be updated with the findings, if any. </param>
		 /// <param name="untilChar"> array of characters to seek. </param>
		 /// <returns> {@code false} if the end was reached and hence no value found, otherwise {@code true}. </returns>
		 /// <exception cref="IOException"> in case of I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean seek(Mark mark, int untilChar) throws java.io.IOException;
		 bool Seek( Mark mark, int untilChar );

		 /// <summary>
		 /// Extracts the value specified by the <seealso cref="Mark"/>, previously populated by a call to <seealso cref="seek(Mark, int)"/>. </summary>
		 /// <param name="mark"> the <seealso cref="Mark"/> specifying which part of a bigger piece of data contains the found value. </param>
		 /// <param name="extractor"> <seealso cref="Extractor"/> capable of extracting the value. </param>
		 /// <param name="optionalData"> holds additional information for spatial and temporal values or null </param>
		 /// <returns> the supplied <seealso cref="Extractor"/>, which after the call carries the extracted value itself,
		 /// where either <seealso cref="Extractor.value()"/> or a more specific accessor method can be called to access the value. </returns>
		 /// <exception cref="IllegalStateException"> if the <seealso cref="Extractor.extract(char[], int, int, bool, org.Neo4Net.values.storable.CSVHeaderInformation) extraction"/>
		 /// returns {@code false}. </exception>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: <EXTRACTOR extends Extractor<?>> EXTRACTOR extract(Mark mark, EXTRACTOR extractor, org.Neo4Net.values.storable.CSVHeaderInformation optionalData);
		 EXTRACTOR extract<EXTRACTOR>( Mark mark, EXTRACTOR extractor, CSVHeaderInformation optionalData );

		 /// <summary>
		 /// Extracts the value specified by the <seealso cref="Mark"/>, previously populated by a call to <seealso cref="seek(Mark, int)"/>. </summary>
		 /// <param name="mark"> the <seealso cref="Mark"/> specifying which part of a bigger piece of data contains the found value. </param>
		 /// <param name="extractor"> <seealso cref="Extractor"/> capable of extracting the value. </param>
		 /// <returns> the supplied <seealso cref="Extractor"/>, which after the call carries the extracted value itself,
		 /// where either <seealso cref="Extractor.value()"/> or a more specific accessor method can be called to access the value. </returns>
		 /// <exception cref="IllegalStateException"> if the <seealso cref="Extractor.extract(char[], int, int, bool, org.Neo4Net.values.storable.CSVHeaderInformation) extraction"/>
		 /// returns {@code false}. </exception>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: <EXTRACTOR extends Extractor<?>> EXTRACTOR extract(Mark mark, EXTRACTOR extractor);
		 EXTRACTOR extract<EXTRACTOR>( Mark mark, EXTRACTOR extractor );

		 /// <summary>
		 /// Extracts the value specified by the <seealso cref="Mark"/>, previously populated by a call to <seealso cref="seek(Mark, int)"/>. </summary>
		 /// <param name="mark"> the <seealso cref="Mark"/> specifying which part of a bigger piece of data contains the found value. </param>
		 /// <param name="extractor"> <seealso cref="Extractor"/> capable of extracting the value. </param>
		 /// <param name="optionalData"> holds additional information for spatial and temporal values or null </param>
		 /// <returns> {@code true} if a value was extracted, otherwise {@code false}. Probably the only reason for
		 /// returning {@code false} would be if the data to extract was empty. </returns>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: boolean tryExtract(Mark mark, Extractor<?> extractor, org.Neo4Net.values.storable.CSVHeaderInformation optionalData);
		 bool tryExtract<T1>( Mark mark, Extractor<T1> extractor, CSVHeaderInformation optionalData );

		 /// <summary>
		 /// Extracts the value specified by the <seealso cref="Mark"/>, previously populated by a call to <seealso cref="seek(Mark, int)"/>. </summary>
		 /// <param name="mark"> the <seealso cref="Mark"/> specifying which part of a bigger piece of data contains the found value. </param>
		 /// <param name="extractor"> <seealso cref="Extractor"/> capable of extracting the value. </param>
		 /// <returns> {@code true} if a value was extracted, otherwise {@code false}. Probably the only reason for
		 /// returning {@code false} would be if the data to extract was empty. </returns>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: boolean tryExtract(Mark mark, Extractor<?> extractor);
		 bool tryExtract<T1>( Mark mark, Extractor<T1> extractor );
	}

}