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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using Test = org.junit.Test;

	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class ZonedTimeIndexKeyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void compareToSameAsValue()
		 public virtual void CompareToSameAsValue()
		 {
			  Value[] values = new Value[] { TimeValue.time( 9999, ZoneOffset.of( "+18:00" ) ), TimeValue.time( 10000, ZoneOffset.of( "-18:00" ) ), TimeValue.time( 10000, ZoneOffset.of( "-00:00" ) ), TimeValue.time( 10000, ZoneOffset.of( "+01:00" ) ), TimeValue.time( 10000, ZoneOffset.of( "+03:00" ) ), TimeValue.time( 10000, ZoneOffset.of( "-18:00" ) ) };

			  ZonedTimeIndexKey keyI = new ZonedTimeIndexKey();
			  ZonedTimeIndexKey keyJ = new ZonedTimeIndexKey();

			  foreach ( Value vi in values )
			  {
					foreach ( Value vj in values )
					{
						 vi.WriteTo( keyI );
						 vj.WriteTo( keyJ );

						 int expected = Values.COMPARATOR.Compare( vi, vj );
						 assertEquals( format( "comparing %s and %s", vi, vj ), expected, keyI.CompareValueTo( keyJ ) );
					}
			  }
		 }
	}

}