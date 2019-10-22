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
namespace Neo4Net.Kernel.Api.Impl.Schema.verification
{
	using MutableObjectLongMap = org.eclipse.collections.api.map.primitive.MutableObjectLongMap;
	using ObjectLongHashMap = org.eclipse.collections.impl.map.mutable.primitive.ObjectLongHashMap;

	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueTuple = Neo4Net.Values.Storable.ValueTuple;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;

	/// <summary>
	/// Base class for strategy used for duplicate check during verification of value uniqueness during
	/// constraint creation.
	/// 
	/// Each particular strategy determines how uniqueness check is done and how to accumulate and store those values for
	/// to make check time and resource consumption optimal.
	/// </summary>
	internal abstract class DuplicateCheckStrategy
	{
		 /// <summary>
		 /// Check uniqueness of multiple properties that belong to a node with provided node id </summary>
		 /// <param name="values"> property values </param>
		 /// <param name="nodeId"> checked node id </param>
		 /// <exception cref="IndexEntryConflictException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract void checkForDuplicate(org.Neo4Net.values.storable.Value[] values, long nodeId) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException;
		 internal abstract void CheckForDuplicate( Value[] values, long nodeId );

		 /// <summary>
		 /// Check uniqueness of single property that belong to a node with provided node id. </summary>
		 /// <param name="value"> property value </param>
		 /// <param name="nodeId"> checked node id </param>
		 /// <exception cref="IndexEntryConflictException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract void checkForDuplicate(org.Neo4Net.values.storable.Value value, long nodeId) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException;
		 internal abstract void CheckForDuplicate( Value value, long nodeId );

		 private static bool PropertyValuesEqual( Value[] properties, Value[] values )
		 {
			  if ( properties.Length != values.Length )
			  {
					return false;
			  }
			  for ( int i = 0; i < properties.Length; i++ )
			  {
					if ( !properties[i].Equals( values[i] ) )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 /// <summary>
		 /// Duplicate check strategy that uses plain hash map. Should be optimal for small amount of entries.
		 /// </summary>
		 internal class MapDuplicateCheckStrategy : DuplicateCheckStrategy
		 {
			  internal readonly MutableObjectLongMap<object> ValueNodeIdMap;

			  internal MapDuplicateCheckStrategy( int expectedNumberOfEntries )
			  {
					this.ValueNodeIdMap = new ObjectLongHashMap<object>( expectedNumberOfEntries );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void checkForDuplicate(org.Neo4Net.values.storable.Value[] values, long nodeId) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
			  public override void CheckForDuplicate( Value[] values, long nodeId )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.values.storable.ValueTuple key = org.Neo4Net.values.storable.ValueTuple.of(values);
					ValueTuple key = ValueTuple.of( values );
					if ( ValueNodeIdMap.containsKey( key ) )
					{
						 throw new IndexEntryConflictException( ValueNodeIdMap.get( key ), nodeId, key );
					}
					ValueNodeIdMap.put( key, nodeId );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void checkForDuplicate(org.Neo4Net.values.storable.Value value, long nodeId) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
			  internal override void CheckForDuplicate( Value value, long nodeId )
			  {
					if ( ValueNodeIdMap.containsKey( value ) )
					{
						 throw new IndexEntryConflictException( ValueNodeIdMap.get( value ), nodeId, value );
					}
					ValueNodeIdMap.put( value, nodeId );
			  }
		 }

		 /// <summary>
		 /// Strategy that uses arrays to store entries and uses hash codes to split those entries over different buckets.
		 /// Number of buckets and size of entries block are dynamic and evaluated based on expected number of duplicates.
		 /// </summary>
		 internal class BucketsDuplicateCheckStrategy : DuplicateCheckStrategy
		 {
			  internal const int BASE_ENTRY_SIZE = 1000;
			  internal const int DEFAULT_BUCKETS = 10;
			  internal static readonly int BucketStrategyEntriesThreshold = BASE_ENTRY_SIZE * DEFAULT_BUCKETS;

			  internal const int MAX_NUMBER_OF_BUCKETS = 100;
			  internal readonly int NumberOfBuckets;
			  internal BucketEntry[] Buckets;
			  internal readonly int BucketSetSize;

			  internal BucketsDuplicateCheckStrategy() : this(BucketStrategyEntriesThreshold)
			  {
			  }

			  internal BucketsDuplicateCheckStrategy( int expectedNumberOfEntries )
			  {
					NumberOfBuckets = min( MAX_NUMBER_OF_BUCKETS, ( expectedNumberOfEntries / BASE_ENTRY_SIZE ) + 1 );
					Buckets = new BucketEntry[NumberOfBuckets];
					BucketSetSize = max( 100, BucketStrategyEntriesThreshold / NumberOfBuckets );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void checkForDuplicate(org.Neo4Net.values.storable.Value[] values, long nodeId) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
			  public override void CheckForDuplicate( Value[] values, long nodeId )
			  {
					BucketEntry current = BucketEntrySet( Arrays.GetHashCode( values ), BucketSetSize );

					// We either have to find the first conflicting entry set element,
					// or append one for the property we just fetched:
					do
					{
						 for ( int i = 0; i < BucketSetSize; i++ )
						 {
							  Value[] currentValues = ( Value[] ) current.Value[i];

							  if ( current.NodeId[i] == StatementConstants.NO_SUCH_NODE )
							  {
									current.Value[i] = values;
									current.NodeId[i] = nodeId;
									if ( i == BucketSetSize - 1 )
									{
										 current.Next = new BucketEntry( BucketSetSize );
									}
									goto scanBreak;
							  }
							  else if ( PropertyValuesEqual( values, currentValues ) )
							  {
									throw new IndexEntryConflictException( current.NodeId[i], nodeId, currentValues );
							  }
						 }
						 current = current.Next;
					} while ( current != null );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void checkForDuplicate(org.Neo4Net.values.storable.Value propertyValue, long nodeId) throws org.Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
			  internal override void CheckForDuplicate( Value propertyValue, long nodeId )
			  {
					BucketEntry current = BucketEntrySet( propertyValue.GetHashCode(), BucketSetSize );

					// We either have to find the first conflicting entry set element,
					// or append one for the property we just fetched:
					do
					{
						 for ( int i = 0; i < BucketSetSize; i++ )
						 {
							  Value value = ( Value ) current.Value[i];

							  if ( current.NodeId[i] == StatementConstants.NO_SUCH_NODE )
							  {
									current.Value[i] = propertyValue;
									current.NodeId[i] = nodeId;
									if ( i == BucketSetSize - 1 )
									{
										 current.Next = new BucketEntry( BucketSetSize );
									}
									goto scanBreak;
							  }
							  else if ( propertyValue.Equals( value ) )
							  {
									throw new IndexEntryConflictException( current.NodeId[i], nodeId, value );
							  }
						 }
						 current = current.Next;
					} while ( current != null );
			  }

			  internal virtual BucketEntry BucketEntrySet( int hashCode, int entrySetSize )
			  {
					int bucket = Math.Abs( hashCode ) % NumberOfBuckets;
					BucketEntry current = Buckets[bucket];
					if ( current == null )
					{
						 current = new BucketEntry( entrySetSize );
						 Buckets[bucket] = current;
					}
					return current;
			  }

			  /// <summary>
			  /// Each bucket entry contains arrays of nodes and corresponding values and link to next BucketEntry in the
			  /// chain for cases when we have more data then the size of one bucket. So bucket entries will form a
			  /// chain of entries to represent values in particular bucket
			  /// </summary>
			  private class BucketEntry
			  {
					internal readonly object[] Value;
					internal readonly long[] NodeId;
					internal BucketEntry Next;

					internal BucketEntry( int entrySize )
					{
						 Value = new object[entrySize];
						 NodeId = new long[entrySize];
						 Arrays.fill( NodeId, StatementConstants.NO_SUCH_NODE );
					}
			  }
		 }
	}

}