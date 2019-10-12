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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{
	using Test = org.junit.Test;


	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using StubPageCursor = Org.Neo4j.Io.pagecache.StubPageCursor;
	using DateTimeValue = Org.Neo4j.Values.Storable.DateTimeValue;
	using Value = Org.Neo4j.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class ZonedDateTimeLayoutTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadAndWriteConsistentValues()
		 public virtual void ShouldReadAndWriteConsistentValues()
		 {
			  Value[] values = new Value[] { DateTimeValue.datetime( 9999, 100, ZoneId.of( "+18:00" ) ), DateTimeValue.datetime( 10000, 100, ZoneId.of( "-18:00" ) ), DateTimeValue.datetime( 10000, 100, ZoneOffset.of( "-17:59:59" ) ), DateTimeValue.datetime( 10000, 100, ZoneId.of( "UTC" ) ), DateTimeValue.datetime( 10000, 100, ZoneId.of( "+01:00" ) ), DateTimeValue.datetime( 10000, 100, ZoneId.of( "Europe/Stockholm" ) ), DateTimeValue.datetime( 10000, 100, ZoneId.of( "+03:00" ) ), DateTimeValue.datetime( 10000, 101, ZoneId.of( "-18:00" ) ) };

			  ZonedDateTimeLayout layout = new ZonedDateTimeLayout();
			  PageCursor cursor = new StubPageCursor( 0, 8 * 1024 );
			  ZonedDateTimeIndexKey writeKey = layout.NewKey();
			  ZonedDateTimeIndexKey readKey = layout.NewKey();

			  // Write all
			  foreach ( Value value in values )
			  {
					value.WriteTo( writeKey );
					layout.WriteKey( cursor, writeKey );
			  }

			  // Read all
			  cursor.Offset = 0;
			  foreach ( Value value in values )
			  {
					layout.ReadKey( cursor, readKey, ZonedDateTimeIndexKey.Size );
					assertEquals( value, readKey.AsValue() );
			  }
		 }
	}

}