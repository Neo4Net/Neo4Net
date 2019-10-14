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
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

	/// <summary>
	/// Includes value and entity id (to be able to handle non-unique values). A value can be any <seealso cref="DurationValue"/>.
	/// 
	/// Durations are tricky, because exactly how long a duration is depends on the start date. We therefore sort them by
	/// average total time in seconds, but keep the original months and days so we can reconstruct the value.
	/// </summary>
	internal class DurationIndexKey : NativeIndexSingleValueKey<DurationIndexKey>
	{
		 /// <summary>
		 /// An average month is 30 days, 10 hours and 30 minutes.
		 /// In seconds this is (((30 * 24) + 10) * 60 + 30) * 60 = 2629800
		 /// </summary>
		 internal const long AVG_MONTH_SECONDS = 2_629_800;
		 internal const long AVG_DAY_SECONDS = 86_400;

		 internal static readonly int Size = Long.BYTES + Integer.BYTES + Long.BYTES + Long.BYTES + ENTITY_ID_SIZE; // entityId

		 internal long TotalAvgSeconds;
		 internal int NanosOfSecond;
		 internal long Months;
		 internal long Days;

		 public override Value AsValue()
		 {
			  long seconds = TotalAvgSeconds - Months * AVG_MONTH_SECONDS - Days * AVG_DAY_SECONDS;
			  return DurationValue.duration( Months, Days, seconds, NanosOfSecond );
		 }

		 public override void InitValueAsLowest( ValueGroup valueGroups )
		 {
			  TotalAvgSeconds = long.MinValue;
			  NanosOfSecond = int.MinValue;
			  Months = long.MinValue;
			  Days = long.MinValue;
		 }

		 public override void InitValueAsHighest( ValueGroup valueGroups )
		 {
			  TotalAvgSeconds = long.MaxValue;
			  NanosOfSecond = int.MaxValue;
			  Months = long.MaxValue;
			  Days = long.MaxValue;
		 }

		 public override int CompareValueTo( DurationIndexKey other )
		 {
			  int comparison = Long.compare( TotalAvgSeconds, other.TotalAvgSeconds );
			  if ( comparison == 0 )
			  {
					comparison = Integer.compare( NanosOfSecond, other.NanosOfSecond );
					if ( comparison == 0 )
					{
						 comparison = Long.compare( Months, other.Months );
						 if ( comparison == 0 )
						 {
							  comparison = Long.compare( Days, other.Days );
						 }
					}
			  }
			  return comparison;
		 }

		 public override string ToString()
		 {
			  return format( "value=%s,entityId=%d,totalAvgSeconds=%d,nanosOfSecond=%d,months=%d,days=%d", AsValue(), EntityId, TotalAvgSeconds, NanosOfSecond, Months, Days );
		 }

		 public override void WriteDuration( long months, long days, long seconds, int nanos )
		 { // no-op
			  this.TotalAvgSeconds = months * AVG_MONTH_SECONDS + days * AVG_DAY_SECONDS + seconds;
			  this.NanosOfSecond = nanos;
			  this.Months = months;
			  this.Days = days;
		 }

		 protected internal override Value AssertCorrectType( Value value )
		 {
			  if ( !( value is DurationValue ) )
			  {
					throw new System.ArgumentException( "Key layout does only support DurationValue, tried to create key from " + value );
			  }
			  return value;
		 }
	}

}