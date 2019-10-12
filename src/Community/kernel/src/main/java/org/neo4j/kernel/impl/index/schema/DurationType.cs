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

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.DurationIndexKey.AVG_DAY_SECONDS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.DurationIndexKey.AVG_MONTH_SECONDS;

	internal class DurationType : Type
	{
		 // Affected key state:
		 // long0 (totalAvgSeconds)
		 // long1 (nanosOfSecond)
		 // long2 (months)
		 // long3 (days)

		 internal DurationType( sbyte typeId ) : base( ValueGroup.DURATION, typeId, DurationValue.MIN_VALUE, DurationValue.MAX_VALUE )
		 {
		 }

		 internal override int ValueSize( GenericKey state )
		 {
			  return GenericKey.SizeDuration;
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

		 internal static DurationValue AsValue( long long0, long long1, long long2, long long3 )
		 {
			  // DurationValue has no "raw" variant
			  long seconds = long0 - long2 * AVG_MONTH_SECONDS - long3 * AVG_DAY_SECONDS;
			  return DurationValue.duration( long2, long3, seconds, long1 );
		 }

		 internal static int Compare( long thisLong0, long thisLong1, long thisLong2, long thisLong3, long thatLong0, long thatLong1, long thatLong2, long thatLong3 )
		 {
			  int comparison = Long.compare( thisLong0, thatLong0 );
			  if ( comparison == 0 )
			  {
					comparison = Integer.compare( ( int ) thisLong1, ( int ) thatLong1 );
					if ( comparison == 0 )
					{
						 comparison = Long.compare( thisLong2, thatLong2 );
						 if ( comparison == 0 )
						 {
							  comparison = Long.compare( thisLong3, thatLong3 );
						 }
					}
			  }
			  return comparison;
		 }

		 internal static void Put( PageCursor cursor, long long0, long long1, long long2, long long3 )
		 {
			  cursor.PutLong( long0 );
			  cursor.PutInt( ( int ) long1 );
			  cursor.PutLong( long2 );
			  cursor.PutLong( long3 );
		 }

		 internal static bool Read( PageCursor cursor, GenericKey into )
		 {
			  // TODO unify order of fields
			  long totalAvgSeconds = cursor.Long;
			  int nanosOfSecond = cursor.Int;
			  long months = cursor.Long;
			  long days = cursor.Long;
			  into.WriteDurationWithTotalAvgSeconds( months, days, totalAvgSeconds, nanosOfSecond );
			  return true;
		 }

		 internal virtual void Write( GenericKey state, long months, long days, long totalAvgSeconds, int nanos )
		 {
			  state.Long0 = totalAvgSeconds;
			  state.Long1 = nanos;
			  state.Long2 = months;
			  state.Long3 = days;
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