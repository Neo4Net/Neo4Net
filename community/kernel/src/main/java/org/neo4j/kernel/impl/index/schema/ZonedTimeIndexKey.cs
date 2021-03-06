﻿/*
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

	using TimeValue = Org.Neo4j.Values.Storable.TimeValue;
	using TimeZones = Org.Neo4j.Values.Storable.TimeZones;
	using Value = Org.Neo4j.Values.Storable.Value;
	using ValueGroup = Org.Neo4j.Values.Storable.ValueGroup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;

	/// <summary>
	/// Includes value and entity id (to be able to handle non-unique values). A value can be any <seealso cref="TimeValue"/>.
	/// 
	/// With these keys the TimeValues are sorted by UTC time of day, and then by time zone.
	/// </summary>
	internal class ZonedTimeIndexKey : NativeIndexSingleValueKey<ZonedTimeIndexKey>
	{
		 internal static readonly int Size = Long.BYTES + Integer.BYTES + ENTITY_ID_SIZE; // entityId

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
			  return format( "value=%s,entityId=%d,nanosOfDayUTC=%d,zoneOffsetSeconds=%d", AsValue(), EntityId, NanosOfDayUTC, ZoneOffsetSeconds );
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