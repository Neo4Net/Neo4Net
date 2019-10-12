using System.Diagnostics;

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
	using IndexSpecificSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using Preconditions = Neo4Net.Util.Preconditions;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

	/// <summary>
	/// <seealso cref="GenericKey"/> which has an array of <seealso cref="GenericKey"/> inside and can therefore hold composite key state.
	/// For single-keys please instead use the more efficient <seealso cref="GenericKey"/>.
	/// </summary>
	internal class CompositeGenericKey : GenericKey
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({ "unchecked", "rawtypes" }) private GenericKey[] states;
		 private GenericKey[] _states;

		 internal CompositeGenericKey( int slots, IndexSpecificSpaceFillingCurveSettingsCache spatialSettings ) : base( spatialSettings )
		 {
			  _states = new GenericKey[slots];
			  for ( int i = 0; i < slots; i++ )
			  {
					_states[i] = new GenericKey( spatialSettings );
			  }
		 }

		 internal override void WriteValue( int stateSlot, Value value, Inclusion inclusion )
		 {
			  stateSlot( stateSlot ).WriteValue( value, inclusion );
		 }

		 internal override void AssertValidValue( int stateSlot, Value value )
		 {
			  Preconditions.requireBetween( stateSlot, 0, NumberOfStateSlots() );
		 }

		 internal override Value[] AsValues()
		 {
			  Value[] values = new Value[NumberOfStateSlots()];
			  for ( int i = 0; i < values.Length; i++ )
			  {
					values[i] = StateSlot( i ).asValue();
			  }
			  return values;
		 }

		 internal override void InitValueAsLowest( int stateSlot, ValueGroup valueGroup )
		 {
			  stateSlot( stateSlot ).InitValueAsLowest( valueGroup );
		 }

		 internal override void InitValueAsHighest( int stateSlot, ValueGroup valueGroup )
		 {
			  stateSlot( stateSlot ).InitValueAsHighest( valueGroup );
		 }

		 internal override int CompareValueToInternal( GenericKey other )
		 {
			  int slots = NumberOfStateSlots();
			  for ( int i = 0; i < slots; i++ )
			  {
					int comparison = StateSlot( i ).compareValueToInternal( other.StateSlot( i ) );
					if ( comparison != 0 )
					{
						 return comparison;
					}
			  }
			  return 0;
		 }

		 internal override void CopyFromInternal( GenericKey key )
		 {
			  int slots = NumberOfStateSlots();
			  if ( key.NumberOfStateSlots() != slots )
			  {
					throw new System.ArgumentException( "Different state lengths " + key.NumberOfStateSlots() + " vs " + slots );
			  }

			  for ( int i = 0; i < slots; i++ )
			  {
					StateSlot( i ).copyFromInternal( key.StateSlot( i ) );
			  }
		 }

		 internal override int SizeInternal()
		 {
			  int size = 0;
			  int slots = NumberOfStateSlots();
			  for ( int i = 0; i < slots; i++ )
			  {
					size += StateSlot( i ).sizeInternal();
			  }
			  return size;
		 }

		 internal override void PutInternal( PageCursor cursor )
		 {
			  int slots = NumberOfStateSlots();
			  for ( int i = 0; i < slots; i++ )
			  {
					StateSlot( i ).putInternal( cursor );
			  }
		 }

		 internal override bool GetInternal( PageCursor cursor, int keySize )
		 {
			  int slots = NumberOfStateSlots();
			  for ( int i = 0; i < slots; i++ )
			  {
					if ( !StateSlot( i ).getInternal( cursor, keySize ) )
					{
						 // The slot's getInternal has already set cursor exception, if it so desired, with more specific information so don't do it here.
						 return false;
					}
			  }
			  return true;
		 }

		 internal override void InitializeToDummyValueInternal()
		 {
			  int slots = NumberOfStateSlots();
			  for ( int i = 0; i < slots; i++ )
			  {
					StateSlot( i ).initializeToDummyValueInternal();
			  }
		 }

		 internal override int NumberOfStateSlots()
		 {
			  return _states.Length;
		 }

		 public override string ToStringInternal()
		 {
			  StringJoiner joiner = new StringJoiner( "," );
			  foreach ( GenericKey state in _states )
			  {
					joiner.add( state.ToStringInternal() );
			  }
			  return joiner.ToString();
		 }

		 internal override string ToDetailedStringInternal()
		 {
			  StringJoiner joiner = new StringJoiner( "," );
			  foreach ( GenericKey state in _states )
			  {
					joiner.add( state.ToDetailedStringInternal() );
			  }
			  return joiner.ToString();
		 }

		 internal override void MinimalSplitterInternal( GenericKey left, GenericKey right, GenericKey into )
		 {
			  int firstStateToDiffer = 0;
			  int compare = 0;
			  int stateCount = right.NumberOfStateSlots();

			  // It's really quite assumed that all these keys have the same number of state slots.
			  // It's not a practical runtime concern, so merely an assertion here
			  Debug.Assert( right.NumberOfStateSlots() == stateCount );
			  Debug.Assert( into.NumberOfStateSlots() == stateCount );

			  while ( compare == 0 && firstStateToDiffer < stateCount )
			  {
					GenericKey leftState = left.StateSlot( firstStateToDiffer );
					GenericKey rightState = right.StateSlot( firstStateToDiffer );
					firstStateToDiffer++;
					compare = leftState.CompareValueToInternal( rightState );
			  }
			  firstStateToDiffer--; // Rewind last increment
			  for ( int i = 0; i < firstStateToDiffer; i++ )
			  {
					into.StateSlot( i ).copyFromInternal( right.StateSlot( i ) );
			  }
			  for ( int i = firstStateToDiffer; i < stateCount; i++ )
			  {
					GenericKey leftState = left.StateSlot( i );
					GenericKey rightState = right.StateSlot( i );
					rightState.MinimalSplitterInternal( leftState, rightState, into.StateSlot( i ) );
			  }
		 }

		 internal override GenericKey StateSlot( int slot )
		 {
			  return _states[slot];
		 }
	}

}