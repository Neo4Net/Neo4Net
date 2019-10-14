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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using Test = org.junit.Test;


	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class ZonedDateTimeIndexKeyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void compareToSameAsValue()
		 public virtual void CompareToSameAsValue()
		 {
			  Value[] values = new Value[] { DateTimeValue.datetime( 9999, 100, ZoneId.of( "+18:00" ) ), DateTimeValue.datetime( 10000, 100, ZoneId.of( "-18:00" ) ), DateTimeValue.datetime( 10000, 100, ZoneOffset.of( "-17:59:59" ) ), DateTimeValue.datetime( 10000, 100, ZoneId.of( "UTC" ) ), DateTimeValue.datetime( 10000, 100, ZoneId.of( "+01:00" ) ), DateTimeValue.datetime( 10000, 100, ZoneId.of( "Europe/Stockholm" ) ), DateTimeValue.datetime( 10000, 100, ZoneId.of( "+03:00" ) ), DateTimeValue.datetime( 10000, 101, ZoneId.of( "-18:00" ) ) };

			  ZonedDateTimeIndexKey keyI = new ZonedDateTimeIndexKey();
			  ZonedDateTimeIndexKey keyJ = new ZonedDateTimeIndexKey();

			  int len = values.Length;

			  for ( int i = 0; i < len; i++ )
			  {
					for ( int j = 0; j < len; j++ )
					{
						 Value vi = values[i];
						 Value vj = values[j];
						 vi.WriteTo( keyI );
						 vj.WriteTo( keyJ );

						 int expected = Integer.signum( Values.COMPARATOR.Compare( vi, vj ) );
						 assertEquals( format( "comparing %s and %s", vi, vj ), expected, Integer.signum( i - j ) );
						 assertEquals( format( "comparing %s and %s", vi, vj ), expected, Integer.signum( keyI.CompareValueTo( keyJ ) ) );
					}
			  }
		 }
	}

}