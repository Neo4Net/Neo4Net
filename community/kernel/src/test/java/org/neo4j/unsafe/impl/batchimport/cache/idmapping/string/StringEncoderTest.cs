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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.cache.idmapping.@string
{
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class StringEncoderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEncodeStringWithZeroLength()
		 public virtual void ShouldEncodeStringWithZeroLength()
		 {
			  // GIVEN
			  Encoder encoder = new StringEncoder();

			  // WHEN
			  long eId = encoder.Encode( "" );

			  // THEN
			  assertTrue( eId != 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEncodeStringWithAnyLength()
		 public virtual void ShouldEncodeStringWithAnyLength()
		 {
			  // GIVEN
			  Encoder encoder = new StringEncoder();

			  // WHEN
			  MutableLongSet encoded = new LongHashSet();
			  int total = 1_000;
			  int duplicates = 0;
			  for ( int i = 0; i < total; i++ )
			  {
					// THEN
					long encode = encoder.Encode( AbcStringOfLength( i ) );
					assertTrue( encode != 0 );
					if ( !encoded.add( encode ) )
					{
						 duplicates++;
					}
			  }
			  assertTrue( ( ( float ) duplicates / ( float ) total ) < 0.01f );
		 }

		 private string AbcStringOfLength( int length )
		 {
			  char[] chars = new char[length];
			  for ( int i = 0; i < length; i++ )
			  {
					int ch = 'a' + ( i % 20 );
					chars[i] = ( char ) ch;
			  }
			  return new string( chars );
		 }
	}

}