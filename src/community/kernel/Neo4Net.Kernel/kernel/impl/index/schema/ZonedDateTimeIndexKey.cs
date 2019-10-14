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

	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using TimeZones = Neo4Net.Values.Storable.TimeZones;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Values = Neo4Net.Values.Storable.Values;

	/// <summary>
	/// Includes value and entity id (to be able to handle non-unique values). A value can be any <seealso cref="DateTimeValue"/>.
	/// <para>
	/// With these keys the DateTimeValues are sorted
	/// 1. by epochSecond
	/// 2. by nanos
	/// 3. by effective Offset west-to-east
	/// 4. non-named TimeZones before named TimeZones. Named Timezones alphabetically.
	/// </para>
	/// </summary>
	internal class ZonedDateTimeIndexKey : NativeIndexSingleValueKey<ZonedDateTimeIndexKey>
	{
		 internal static readonly int Size = Long.BYTES + Integer.BYTES + Integer.BYTES + ENTITY_ID_SIZE; // entityId

		 internal long EpochSecondUTC;
		 internal int NanoOfSecond;
		 internal short ZoneId;
		 internal int ZoneOffsetSeconds;

		 public override Value AsValue()
		 {
			  return TimeZones.validZoneId( ZoneId ) ? DateTimeValue.datetime( EpochSecondUTC, NanoOfSecond, ZoneId.of( TimeZones.map( ZoneId ) ) ) : DateTimeValue.datetime( EpochSecondUTC, NanoOfSecond, ZoneOffset.ofTotalSeconds( ZoneOffsetSeconds ) );
		 }

		 public override void InitValueAsLowest( ValueGroup valueGroups )
		 {
			  EpochSecondUTC = long.MinValue;
			  NanoOfSecond = int.MinValue;
			  ZoneId = short.MinValue;
			  ZoneOffsetSeconds = int.MinValue;
		 }

		 public override void InitValueAsHighest( ValueGroup valueGroups )
		 {
			  EpochSecondUTC = long.MaxValue;
			  NanoOfSecond = int.MaxValue;
			  ZoneId = short.MaxValue;
			  ZoneOffsetSeconds = int.MaxValue;
		 }

		 public override int CompareValueTo( ZonedDateTimeIndexKey other )
		 {
			  int compare = Long.compare( EpochSecondUTC, other.EpochSecondUTC );
			  if ( compare == 0 )
			  {
					compare = Integer.compare( NanoOfSecond, other.NanoOfSecond );
					if ( compare == 0 && TimeZones.validZoneOffset( ZoneOffsetSeconds ) && TimeZones.validZoneOffset( other.ZoneOffsetSeconds ) )
					{
						 if ( ZoneOffsetSeconds != other.ZoneOffsetSeconds || ZoneId != other.ZoneId )
						 {
							  // In the rare case of comparing the same instant in different time zones, we settle for
							  // mapping to values and comparing using the general values comparator.
							  compare = Values.COMPARATOR.Compare( AsValue(), other.AsValue() );
						 }
					}
			  }
			  return compare;
		 }

		 public override string ToString()
		 {
			  return format( "value=%s,entityId=%d,epochSecond=%d,nanoOfSecond=%d,zoneId=%d,zoneOffset=%d", AsValue(), EntityId, EpochSecondUTC, NanoOfSecond, ZoneId, ZoneOffsetSeconds );
		 }

		 public override void WriteDateTime( long epochSecondUTC, int nano, int offsetSeconds )
		 {
			  this.EpochSecondUTC = epochSecondUTC;
			  this.NanoOfSecond = nano;
			  this.ZoneOffsetSeconds = offsetSeconds;
			  this.ZoneId = -1;
		 }

		 public override void WriteDateTime( long epochSecondUTC, int nano, string zoneId )
		 {
			  this.EpochSecondUTC = epochSecondUTC;
			  this.NanoOfSecond = nano;
			  this.ZoneId = TimeZones.map( zoneId );
			  this.ZoneOffsetSeconds = 0;
		 }

		 protected internal override Value AssertCorrectType( Value value )
		 {
			  if ( !( value is DateTimeValue ) )
			  {
					throw new System.ArgumentException( "Key layout does only support DateTimeValue, tried to create key from " + value );
			  }
			  return value;
		 }
	}

}