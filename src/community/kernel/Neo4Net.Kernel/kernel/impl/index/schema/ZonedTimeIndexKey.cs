﻿/*
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

	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using TimeZones = Neo4Net.Values.Storable.TimeZones;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.NO_VALUE;

	/// <summary>
	/// Includes value and IEntity id (to be able to handle non-unique values). A value can be any <seealso cref="TimeValue"/>.
	/// 
	/// With these keys the TimeValues are sorted by UTC time of day, and then by time zone.
	/// </summary>
	internal class ZonedTimeIndexKey : NativeIndexSingleValueKey<ZonedTimeIndexKey>
	{
		 internal static readonly int Size = Long.BYTES + Integer.BYTES + IEntity_ID_SIZE; // IEntityId

		 internal long NanosOfDayUTC;
		 internal int ZoneOffsetSeconds;

		 public override Value AsValue()
		 {
			  // We need to check validity upfront without throwing exceptions, because the PageCursor might give garbage bytes
			  if ( TimeZones.validZoneOffset( ZoneOffsetSeconds ) )
			  {
					return TimeValue.time( NanosOfDayUTC, ZoneOffset.ofTotalSeconds( ZoneOffsetSeconds ) );
			  }
			  return NO_VALUE;
		 }

		 public override void InitValueAsLowest( ValueGroup valueGroups )
		 {
			  NanosOfDayUTC = long.MinValue;
			  ZoneOffsetSeconds = int.MinValue;
		 }

		 public override void InitValueAsHighest( ValueGroup valueGroups )
		 {
			  NanosOfDayUTC = long.MaxValue;
			  ZoneOffsetSeconds = int.MaxValue;
		 }

		 public override int CompareValueTo( ZonedTimeIndexKey other )
		 {
			  int compare = Long.compare( NanosOfDayUTC, other.NanosOfDayUTC );
			  if ( compare == 0 )
			  {
					compare = Integer.compare( ZoneOffsetSeconds, other.ZoneOffsetSeconds );
			  }
			  return compare;
		 }

		 public override string ToString()
		 {
			  return format( "value=%s,entityId=%d,nanosOfDayUTC=%d,zoneOffsetSeconds=%d", AsValue(), IEntityId, NanosOfDayUTC, ZoneOffsetSeconds );
		 }

		 public override void WriteTime( long nanosOfDayUTC, int offsetSeconds )
		 {
			  this.NanosOfDayUTC = nanosOfDayUTC;
			  this.ZoneOffsetSeconds = offsetSeconds;
		 }

		 protected internal override Value AssertCorrectType( Value value )
		 {
			  if ( !( value is TimeValue ) )
			  {
					throw new System.ArgumentException( "Key layout does only support TimeValue, tried to create key from " + value );
			  }
			  return value;
		 }
	}

}