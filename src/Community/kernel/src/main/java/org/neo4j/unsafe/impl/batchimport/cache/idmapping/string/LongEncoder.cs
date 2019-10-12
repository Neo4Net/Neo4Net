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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.log10;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;

	/// <summary>
	/// <seealso cref="Encoder"/> that assumes that the entered strings can be parsed to <seealso cref="Long"/> directly.
	/// </summary>
	public class LongEncoder : Encoder
	{
		 public override long Encode( object value )
		 {
			  long longValue = ( ( Number )value ).longValue();
			  long length = NumberOfDigits( longValue );
			  length = length << 57;
			  long returnVal = length | longValue;
			  return returnVal;
		 }

		 private static int NumberOfDigits( long value )
		 {
			  return max( 1, ( int )( log10( value ) + 1 ) );
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name;
		 }
	}

}