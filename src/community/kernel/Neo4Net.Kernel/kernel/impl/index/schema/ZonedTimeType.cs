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

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using TimeZones = Neo4Net.Values.Storable.TimeZones;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.NO_VALUE;

	internal class ZonedTimeType : Type
	{
		 // Affected key state:
		 // long0 (nanosOfDayUTC)
		 // long1 (zoneOffsetSeconds)

		 internal ZonedTimeType( sbyte typeId ) : base( ValueGroup.ZONED_TIME, typeId, TimeValue.MIN_VALUE, TimeValue.MAX_VALUE )
		 {
		 }

		 internal override int ValueSize( GenericKey state )
		 {
			  return GenericKey.SizeZonedTime;
		 }

		 internal override void CopyValue( GenericKey to, GenericKey from )
		 {
			  to.Long0 = from.Long0;
			  to.Long1 = from.Long1;
		 }

		 internal override Value AsValue( GenericKey state )
		 {
			  return AsValue( state.Long0, state.Long1 );
		 }

		 internal override int CompareValue( GenericKey left, GenericKey right )
		 {
			  return Compare( left.Long0, left.Long1, right.Long0, right.Long1 );
		 }

		 internal override void PutValue( PageCursor cursor, GenericKey state )
		 {
			  Put( cursor, state.Long0, state.Long1 );
		 }

		 internal override bool ReadValue( PageCursor cursor, int size, GenericKey into )
		 {
			  return Read( cursor, into );
		 }

		 internal static Value AsValue( long long0, long long1 )
		 {
			  OffsetTime time = AsValueRaw( long0, long1 );
			  return time != null ? TimeValue.time( time ) : NO_VALUE;
		 }

		 internal static OffsetTime AsValueRaw( long long0, long long1 )
		 {
			  if ( TimeZones.validZoneOffset( ( int ) long1 ) )
			  {
					return TimeValue.timeRaw( long0, ZoneOffset.ofTotalSeconds( ( int ) long1 ) );
			  }
			  // TODO Getting here means that after a proper read this value is plain wrong... shouldn't something be thrown instead? Yes and same for TimeZones
			  return null;
		 }

		 internal static void Put( PageCursor cursor, long long0, long long1 )
		 {
			  cursor.PutLong( long0 );
			  cursor.PutInt( ( int ) long1 );
		 }

		 internal static bool Read( PageCursor cursor, GenericKey into )
		 {
			  into.WriteTime( cursor.Long, cursor.Int );
			  return true;
		 }

		 internal static int Compare( long thisLong0, long thisLong1, long thatLong0, long thatLong1 )
		 {
			  int compare = Long.compare( thisLong0, thatLong0 );
			  if ( compare == 0 )
			  {
					compare = Integer.compare( ( int ) thisLong1, ( int ) thatLong1 );
			  }
			  return compare;
		 }

		 internal virtual void Write( GenericKey state, long nanosOfDayUTC, int offsetSeconds )
		 {
			  state.Long0 = nanosOfDayUTC;
			  state.Long1 = offsetSeconds;
		 }

		 protected internal override void AddTypeSpecificDetails( StringJoiner joiner, GenericKey state )
		 {
			  joiner.add( "long0=" + state.Long0 );
			  joiner.add( "long1=" + state.Long1 );
		 }
	}

}