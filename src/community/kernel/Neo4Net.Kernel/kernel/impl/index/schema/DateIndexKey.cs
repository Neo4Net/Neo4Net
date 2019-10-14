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
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

	/// <summary>
	/// Includes value and entity id (to be able to handle non-unique values). A value can be any <seealso cref="DateValue"/>.
	/// </summary>
	internal class DateIndexKey : NativeIndexSingleValueKey<DateIndexKey>
	{
		 internal static readonly int Size = Long.BYTES + ENTITY_ID_SIZE; // entityId

		 internal long EpochDay;

		 public override Value AsValue()
		 {
			  return DateValue.epochDate( EpochDay );
		 }

		 internal override void InitValueAsLowest( ValueGroup valueGroups )
		 {
			  EpochDay = long.MinValue;
		 }

		 internal override void InitValueAsHighest( ValueGroup valueGroups )
		 {
			  EpochDay = long.MaxValue;
		 }

		 public override int CompareValueTo( DateIndexKey other )
		 {
			  return Long.compare( EpochDay, other.EpochDay );
		 }

		 public override string ToString()
		 {
			  return format( "value=%s,entityId=%d,epochDay=%d", AsValue(), EntityId, EpochDay );
		 }

		 public override void WriteDate( long epochDay )
		 {
			  this.EpochDay = epochDay;
		 }

		 protected internal override Value AssertCorrectType( Value value )
		 {
			  if ( !( value is DateValue ) )
			  {
					throw new System.ArgumentException( "Key layout does only support DateValue, tried to create key from " + value );
			  }
			  return value;
		 }
	}

}