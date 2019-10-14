using System;

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
	/// Extracts a value from a part of a {@code char[]} into any type of value, f.ex. a <seealso cref="Extractors.string()"/>,
	/// <seealso cref="Extractors.long_() long"/> or <seealso cref="Extractors.intArray()"/>.
	/// 
	/// An <seealso cref="Extractor"/> is mutable for the single purpose of ability to reuse its value instance. Consider extracting
	/// a primitive int -
	/// 
	/// Sub-interfaces and implementations can and should specify specific accessors for the purpose
	/// of performance and less garbage, f.ex. where an IntExtractor could have an accessor method for
	/// getting the extracted value as primitive int, to avoid auto-boxing which would arise from calling <seealso cref="value()"/>.
	/// </summary>
	/// <seealso cref= Extractors for a collection of very common extractors. </seealso>
	public interface Extractor<T> : ICloneable
	{
		 /// <summary>
		 /// Extracts value of type {@code T} from the given character data. </summary>
		 /// <param name="data"> characters in a buffer. </param>
		 /// <param name="offset"> offset into the buffer where the value starts. </param>
		 /// <param name="length"> number of characters from the offset to extract. </param>
		 /// <param name="hadQuotes"> whether or not there were skipped characters, f.ex. quotation. </param>
		 /// <param name="optionalData"> optional data to be used for spatial or temporal values or null if csv header did not use it </param>
		 /// <returns> {@code true} if a value was extracted, otherwise {@code false}. </returns>
		 bool Extract( char[] data, int offset, int length, bool hadQuotes, CSVHeaderInformation optionalData );

		 /// <summary>
		 /// Extracts value of type {@code T} from the given character data. </summary>
		 /// <param name="data"> characters in a buffer. </param>
		 /// <param name="offset"> offset into the buffer where the value starts. </param>
		 /// <param name="length"> number of characters from the offset to extract. </param>
		 /// <param name="hadQuotes"> whether or not there were skipped characters, f.ex. quotation. </param>
		 /// <returns> {@code true} if a value was extracted, otherwise {@code false}. </returns>
		 bool Extract( char[] data, int offset, int length, bool hadQuotes );

		 /// <returns> the most recently extracted value. </returns>
		 T Value();

		 /// <returns> string representation of what type of value of produces. Also used as key in <seealso cref="Extractors"/>. </returns>
		 string Name();

		 Extractor<T> Clone();
	}

}