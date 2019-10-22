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
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using TimeZones = Neo4Net.Values.Storable.TimeZones;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.ZonedDateTimeLayout.ZONE_ID_FLAG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.ZonedDateTimeLayout.ZONE_ID_MASK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.ZonedDateTimeLayout.asZoneId;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.ZonedDateTimeLayout.asZoneOffset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.ZonedDateTimeLayout.isZoneId;

	internal class ZonedDateTimeType : Type
	{
		 // Affected key state:
		 // long0 (epochSecondUTC)
		 // long1 (nanoOfSecond)
		 // long2 (zoneId)
		 // long3 (zoneOffsetSeconds)

		 internal ZonedDateTimeType( sbyte typeId ) : base( ValueGroup.ZONED_DATE_TIME, typeId, DateTimeValue.MIN_VALUE, DateTimeValue.MAX_VALUE )
		 {
		 }

		 internal override int ValueSize( GenericKey state )
		 {
			  return GenericKey.SizeZonedDateTime;
		 }

		 internal override void CopyValue( GenericKey to, GenericKey from )
		 {
			  to.Long0 = from.Long0;
			  to.Long1 = from.Long1;
			  to.Long2 = from.Long2;
			  to.Long3 = from.Long3;
		 }

		 internal override Value AsValue( GenericKey state )
		 {
			  return AsValue( state.Long0, state.Long1, state.Long2, state.Long3 );
		 }

		 internal override int CompareValue( GenericKey left, GenericKey right )
		 {
			  return Compare( left.Long0, left.Long1, left.Long2, left.Long3, right.Long0, right.Long1, right.Long2, right.Long3 );
		 }

		 internal override void PutValue( PageCursor cursor, GenericKey state )
		 {
			  Put( cursor, state.Long0, state.Long1, state.Long2, state.Long3 );
		 }

		 internal override bool ReadValue( PageCursor cursor, int size, GenericKey into )
		 {
			  return Read( cursor, into );
		 }

		 internal static int Compare( long thisLong0, long thisLong1, long thisLong2, long thisLong3, long thatLong0, long thatLong1, long thatLong2, long thatLong3 )
		 {
			  int compare = Long.compare( thisLong0, thatLong0 );
			  if ( compare == 0 )
			  {
					compare = Integer.compare( ( int ) thisLong1, ( int ) thatLong1 );
					if ( compare == 0 && TimeZones.validZoneOffset( ( int ) thisLong3 ) && TimeZones.validZoneOffset( ( int ) thatLong3 ) )
					{
						 // In the rare case of comparing the same instant in different time zones, we settle for
						 // mapping to values and comparing using the general values comparator.
						 compare = Values.COMPARATOR.Compare( AsValue( thisLong0, thisLong1, thisLong2, thisLong3 ), AsValue( thatLong0, thatLong1, thatLong2, thatLong3 ) );
					}
			  }
			  return compare;
		 }

		 internal static void Put( PageCursor cursor, long long0, long long1, long long2, long long3 )
		 {
			  cursor.PutLong( long0 );
			  cursor.PutInt( ( int ) long1 );
			  if ( long2 >= 0 )
			  {
					cursor.PutInt( ( int ) long2 | ZONE_ID_FLAG );
			  }
			  else
			  {
					cursor.PutInt( ( int ) long3 & ZONE_ID_MASK );
			  }
		 }

		 internal static bool Read( PageCursor cursor, GenericKey into )
		 {
			  long epochSecondUTC = cursor.Long;
			  int nanoOfSecond = cursor.Int;
			  int encodedZone = cursor.Int;
			  if ( isZoneId( encodedZone ) )
			  {
					into.writeDateTime( epochSecondUTC, nanoOfSecond, asZoneId( encodedZone ) );
			  }
			  else
			  {
					into.writeDateTime( epochSecondUTC, nanoOfSecond, asZoneOffset( encodedZone ) );
			  }
			  return true;
		 }

		 internal static DateTimeValue AsValue( long long0, long long1, long long2, long long3 )
		 {
			  return DateTimeValue.datetime( AsValueRaw( long0, long1, long2, long3 ) );
		 }

		 internal static ZonedDateTime AsValueRaw( long long0, long long1, long long2, long long3 )
		 {
			  return TimeZones.validZoneId( ( short ) long2 ) ? DateTimeValue.datetimeRaw( long0, long1, ZoneId.of( TimeZones.map( ( short ) long2 ) ) ) : DateTimeValue.datetimeRaw( long0, long1, ZoneOffset.ofTotalSeconds( ( int ) long3 ) );
		 }

		 internal virtual void Write( GenericKey state, long epochSecondUTC, int nano, short zoneId, int offsetSeconds )
		 {
			  state.Long0 = epochSecondUTC;
			  state.Long1 = nano;
			  state.Long2 = zoneId;
			  state.Long3 = offsetSeconds;
		 }

		 protected internal override void AddTypeSpecificDetails( StringJoiner joiner, GenericKey state )
		 {
			  joiner.add( "long0=" + state.Long0 );
			  joiner.add( "long1=" + state.Long1 );
			  joiner.add( "long2=" + state.Long2 );
			  joiner.add( "long3=" + state.Long3 );
		 }
	}

}