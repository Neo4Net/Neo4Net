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
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

	/// <summary>
	/// Includes value and entity id (to be able to handle non-unique values). A value can be any <seealso cref="LocalDateTimeValue"/>.
	/// </summary>
	internal class LocalDateTimeIndexKey : NativeIndexSingleValueKey<LocalDateTimeIndexKey>
	{
		 internal static readonly int Size = Long.BYTES + Integer.BYTES + ENTITY_ID_SIZE; // entityId

		 internal int NanoOfSecond;
		 internal long EpochSecond;

		 public override Value AsValue()
		 {
			  return LocalDateTimeValue.localDateTime( EpochSecond, NanoOfSecond );
		 }

		 public override void InitValueAsLowest( ValueGroup valueGroups )
		 {
			  EpochSecond = long.MinValue;
			  NanoOfSecond = int.MinValue;
		 }

		 public override void InitValueAsHighest( ValueGroup valueGroups )
		 {
			  EpochSecond = long.MaxValue;
			  NanoOfSecond = int.MaxValue;
		 }

		 public override int CompareValueTo( LocalDateTimeIndexKey other )
		 {
			  int compare = Long.compare( EpochSecond, other.EpochSecond );
			  if ( compare == 0 )
			  {
					compare = Integer.compare( NanoOfSecond, other.NanoOfSecond );
			  }
			  return compare;
		 }

		 public override string ToString()
		 {
			  return format( "value=%s,entityId=%d,epochSecond=%d,nanoOfSecond=%d", AsValue(), EntityId, EpochSecond, NanoOfSecond );
		 }

		 public override void WriteLocalDateTime( long epochSecond, int nano )
		 {
			  this.NanoOfSecond = nano;
			  this.EpochSecond = epochSecond;
		 }

		 protected internal override Value AssertCorrectType( Value value )
		 {
			  if ( !( value is LocalDateTimeValue ) )
			  {
					throw new System.ArgumentException( "Key layout does only support LocalDateTimeValue, tried to create key from " + value );
			  }
			  return value;
		 }
	}

}