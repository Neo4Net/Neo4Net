using System;

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
	using Neo4Net.Index.Internal.gbptree;
	using Neo4Net.Kernel.impl.store;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

	internal abstract class NativeIndexKey<SELF> : TemporalValueWriterAdapter<Exception> where SELF : NativeIndexKey<SELF>
	{
		 internal static readonly int IEntityIdSize = Long.BYTES;

		 internal enum Inclusion
		 {
			  Low,
			  Neutral,
			  High
		 }

		 private const bool DEFAULT_COMPARE_ID = true;

		 private long _entityId;
		 private bool _compareId = DEFAULT_COMPARE_ID;

		 /// <summary>
		 /// Marks that comparisons with this key requires also comparing IEntityId, this allows functionality
		 /// of inclusive/exclusive bounds of range queries.
		 /// This is because <seealso cref="GBPTree"/> only support from inclusive and to exclusive.
		 /// <para>
		 /// Note that {@code compareId} is only an in memory state.
		 /// </para>
		 /// </summary>
		 internal virtual bool CompareId
		 {
			 set
			 {
				  this._compareId = value;
			 }
			 get
			 {
				  return _compareId;
			 }
		 }


		 internal virtual long IEntityId
		 {
			 get
			 {
				  return _entityId;
			 }
			 set
			 {
				  this._entityId = value;
			 }
		 }


		 internal void InitFromValue( int stateSlot, Value value, Inclusion inclusion )
		 {
			  AssertValidValue( stateSlot, value );
			  WriteValue( stateSlot, value, inclusion );
		 }

		 internal abstract void WriteValue( int stateSlot, Value value, Inclusion inclusion );

		 internal abstract void AssertValidValue( int stateSlot, Value value );

		 /// <summary>
		 /// Initializes this key with IEntity id and resets other flags to default values.
		 /// Doesn't touch value data.
		 /// </summary>
		 /// <param name="entityId"> IEntity id to set for this key. </param>
		 internal virtual void Initialize( long IEntityId )
		 {
			  this._compareId = DEFAULT_COMPARE_ID;
			  IEntityId = IEntityId;
		 }

		 internal abstract Value[] AsValues();

		 internal abstract void InitValueAsLowest( int stateSlot, ValueGroup valueGroup );

		 internal abstract void InitValueAsHighest( int stateSlot, ValueGroup valueGroup );

		 internal abstract int NumberOfStateSlots();

		 internal void InitValuesAsLowest()
		 {
			  int slots = NumberOfStateSlots();
			  for ( int i = 0; i < slots; i++ )
			  {
					InitValueAsLowest( i, ValueGroup.UNKNOWN );
			  }
		 }

		 internal void InitValuesAsHighest()
		 {
			  int slots = NumberOfStateSlots();
			  for ( int i = 0; i < slots; i++ )
			  {
					InitValueAsHighest( i, ValueGroup.UNKNOWN );
			  }
		 }

		 /// <summary>
		 /// Compares the value of this key to that of another key.
		 /// This method is expected to be called in scenarios where inconsistent reads may happen (and later retried).
		 /// </summary>
		 /// <param name="other"> the key to compare to. </param>
		 /// <returns> comparison against the {@code other} key. </returns>
		 internal abstract int CompareValueTo( SELF other );
	}

}