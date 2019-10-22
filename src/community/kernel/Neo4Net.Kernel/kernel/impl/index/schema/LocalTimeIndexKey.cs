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
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

	/// <summary>
	/// Includes value and IEntity id (to be able to handle non-unique values). A value can be any <seealso cref="LocalTimeValue"/>.
	/// </summary>
	internal class LocalTimeIndexKey : NativeIndexSingleValueKey<LocalTimeIndexKey>
	{
		 internal static readonly int Size = Long.BYTES + IEntity_ID_SIZE; // IEntityId

		 internal long NanoOfDay;

		 public override Value AsValue()
		 {
			  return LocalTimeValue.localTime( NanoOfDay );
		 }

		 public override void InitValueAsLowest( ValueGroup valueGroups )
		 {
			  NanoOfDay = long.MinValue;
		 }

		 public override void InitValueAsHighest( ValueGroup valueGroups )
		 {
			  NanoOfDay = long.MaxValue;
		 }

		 public override int CompareValueTo( LocalTimeIndexKey other )
		 {
			  return Long.compare( NanoOfDay, other.NanoOfDay );
		 }

		 public override string ToString()
		 {
			  return format( "value=%s,entityId=%d,nanoOfDay=%d", AsValue(), IEntityId, NanoOfDay );
		 }

		 public override void WriteLocalTime( long nanoOfDay )
		 {
			  this.NanoOfDay = nanoOfDay;
		 }

		 protected internal override Value AssertCorrectType( Value value )
		 {
			  if ( !( value is LocalTimeValue ) )
			  {
					throw new System.ArgumentException( "Key layout does only support LocalTimeValue, tried to create key from " + value );
			  }
			  return value;
		 }
	}

}