using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.@unsafe.Impl.Batchimport
{

	using Neo4Net.Collections.Helpers;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using ByteArray = Neo4Net.@unsafe.Impl.Batchimport.cache.ByteArray;
	using LongArray = Neo4Net.@unsafe.Impl.Batchimport.cache.LongArray;
	using MemoryStatsVisitor = Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor;
	using NumberArrayFactory = Neo4Net.@unsafe.Impl.Batchimport.cache.NumberArrayFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Format.bytes;

	/// <summary>
	/// Holds information vital for making <seealso cref="RelationshipGroupDefragmenter"/> work the way it does.
	/// 
	/// The defragmenter goes potentially multiple rounds through the relationship group store and each round
	/// selects groups from a range of node ids. This cache can cache the groups for the nodes in this range.
	/// 
	/// First all group counts per node are updated (<seealso cref="incrementGroupCount(long)"/>).
	/// Then <seealso cref="prepare(long)"/> is called from lowest node id (0) and given the maximum configured memory
	/// given to this cache in its constructor the highest node id to cache is returned. Then groups are
	/// <seealso cref="put(RelationshipGroupRecord)"/> and cached in here to later be <seealso cref="iterator() retrieved"/>
	/// where they are now ordered by node and type.
	/// This will go on until the entire node range have been visited.
	/// </summary>
	/// <seealso cref= RelationshipGroupDefragmenter </seealso>
	public class RelationshipGroupCache : IEnumerable<RelationshipGroupRecord>, IDisposable, Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor_Visitable
	{
		 public const int GROUP_ENTRY_SIZE = 1 + 3 + 6 * 3;

		 private readonly ByteArray _groupCountCache;
		 private readonly ByteArray _cache;
		 private readonly long _highNodeId;
		 private readonly LongArray _offsets;
		 private readonly sbyte[] _scratch = new sbyte[GROUP_ENTRY_SIZE];
		 private long _fromNodeId;
		 private long _toNodeId;
		 private long _highCacheId;
		 private readonly long _maxCacheLength;

		 public RelationshipGroupCache( NumberArrayFactory arrayFactory, long maxMemory, long highNodeId )
		 {
			  this._offsets = arrayFactory.NewDynamicLongArray( 100_000, 0 );
			  this._groupCountCache = arrayFactory.NewByteArray( highNodeId, new sbyte[2] );
			  this._highNodeId = highNodeId;

			  long memoryDedicatedToCounting = 2 * highNodeId;
			  long memoryLeftForGroupCache = maxMemory - memoryDedicatedToCounting;
			  if ( memoryLeftForGroupCache < 0 )
			  {
					throw new System.ArgumentException( "Too little memory to cache any groups, provided " + bytes( maxMemory ) + " where " + bytes( memoryDedicatedToCounting ) + " was dedicated to group counting" );
			  }
			  _maxCacheLength = memoryLeftForGroupCache / GROUP_ENTRY_SIZE;
			  this._cache = arrayFactory.NewDynamicByteArray( max( 1_000, _maxCacheLength / 100 ), new sbyte[GROUP_ENTRY_SIZE] );
		 }

		 /// <summary>
		 /// Before caching any relationship groups all group counts for all nodes are incremented by calling
		 /// this method once for every encountered group (its node id).
		 /// </summary>
		 /// <param name="nodeId"> node to increment group count for. </param>
		 public virtual void IncrementGroupCount( long nodeId )
		 {
			  int count = GroupCount( nodeId );
			  count++;
			  if ( ( count & ~0xFFFF ) != 0 )
			  {
					throw new System.InvalidOperationException( "Invalid number of relationship groups for node " + nodeId + " " + count );
			  }
			  _groupCountCache.setShort( nodeId, 0, ( short ) count );
		 }

		 internal virtual int GroupCount( long nodeId )
		 {
			  return _groupCountCache.getShort( nodeId, 0 ) & 0xFFFF;
		 }

		 /// <summary>
		 /// Getter here because we can use this already allocated data structure for other things in and
		 /// around places where this group cache is used.
		 /// </summary>
		 internal virtual ByteArray GroupCountCache
		 {
			 get
			 {
				  return _groupCountCache;
			 }
		 }

		 /// <summary>
		 /// Looks at max amount of configured memory (in constructor) and figures out for how many nodes their groups
		 /// can be cached. Before the first call to this method all <seealso cref="incrementGroupCount(long)"/> calls
		 /// must have been made. After a call to this there should be a sequence of <seealso cref="put(RelationshipGroupRecord)"/>
		 /// calls to cache the groups. If this call returns a node id which is lower than the highest node id in the
		 /// store then more rounds of caching should be performed after completing this round.
		 /// </summary>
		 /// <param name="fromNodeId"> inclusive </param>
		 /// <returns> toNodeId exclusive </returns>
		 public virtual long Prepare( long fromNodeId )
		 {
			  _cache.clear(); // this will have all the "first" bytes set to 0, which means !inUse
			  this._fromNodeId = fromNodeId; // keep for use in put later on

			  _highCacheId = 0;
			  for ( long nodeId = fromNodeId; nodeId < _highNodeId; nodeId++ )
			  {
					int count = GroupCount( nodeId );
					if ( _highCacheId + count > _maxCacheLength )
					{
						 // Cannot include this one, so up until the previous is good
						 return this._toNodeId = nodeId;
					}
					_offsets.set( Rebase( nodeId ), _highCacheId );
					_highCacheId += count;
			  }
			  return this._toNodeId = _highNodeId;
		 }

		 private long Rebase( long toNodeId )
		 {
			  return toNodeId - _fromNodeId;
		 }

		 /// <summary>
		 /// Caches a relationship group into this cache, it will be cached if the
		 /// <seealso cref="RelationshipGroupRecord.getOwningNode() owner"/> is within the <seealso cref="prepare(long) prepared"/> range,
		 /// where {@code true} will be returned, otherwise {@code false}.
		 /// </summary>
		 /// <param name="groupRecord"> <seealso cref="RelationshipGroupRecord"/> to cache. </param>
		 /// <returns> whether or not the group was cached, i.e. whether or not it was within the prepared range. </returns>
		 public virtual bool Put( RelationshipGroupRecord groupRecord )
		 {
			  long nodeId = groupRecord.OwningNode;
			  Debug.Assert( nodeId < _highNodeId );
			  if ( nodeId < _fromNodeId || nodeId >= _toNodeId )
			  {
					return false;
			  }

			  long baseIndex = _offsets.get( Rebase( nodeId ) );
			  // grouCount is extra validation, really
			  int groupCount = groupCount( nodeId );
			  long index = ScanForFreeFrom( baseIndex, groupCount, groupRecord.Type, nodeId );

			  // Put the group at this index
			  _cache.setByte( index, 0, ( sbyte ) 1 );
			  _cache.set3ByteInt( index, 1, groupRecord.Type );
			  _cache.set6ByteLong( index, 1 + 3, groupRecord.FirstOut );
			  _cache.set6ByteLong( index, 1 + 3 + 6, groupRecord.FirstIn );
			  _cache.set6ByteLong( index, 1 + 3 + 6 + 6, groupRecord.FirstLoop );
			  return true;
		 }

		 private long ScanForFreeFrom( long startIndex, int groupCount, int type, long owningNodeId )
		 {
			  long desiredIndex = -1;
			  long freeIndex = -1;
			  for ( int i = 0; i < groupCount; i++ )
			  {
					long candidateIndex = startIndex + i;
					bool free = _cache.getByte( candidateIndex, 0 ) == 0;
					if ( free )
					{
						 freeIndex = candidateIndex;
						 break;
					}

					if ( desiredIndex == -1 )
					{
						 int existingType = _cache.get3ByteInt( candidateIndex, 1 );
						 if ( existingType == type )
						 {
							  throw new System.InvalidOperationException( "Tried to put multiple groups with same type " + type + " for node " + owningNodeId );
						 }

						 if ( type < existingType )
						 {
							  // This means that the groups have arrived here out of order, please put this group
							  // in the correct place, not at the end
							  desiredIndex = candidateIndex;
						 }
					}
			  }

			  if ( freeIndex == -1 )
			  {
					throw new System.InvalidOperationException( "There's no room for me for startIndex:" + startIndex + " with a group count of " + groupCount + ". This means that there's an asymmetry between calls " + "to incrementGroupCount and actual contents sent into put" );
			  }

			  // For the future: Instead of doing the sorting here right away be doing the relatively expensive move
			  // of potentially multiple items one step to the right in the array, then an idea is to simply mark
			  // this group as in need of sorting and then there may be a step later which can use all CPUs
			  // on the machine, jumping from group to group and see if the "needs sorting" flag has been raised
			  // and if so sort that group. This is fine as it is right now because the groups put into this cache
			  // will be almost entirely sorted, since we come here straight after import. Although if this thing
			  // is to be used as a generic relationship group defragmenter this sorting will have to be fixed
			  // to something like what is described above in this comment.
			  if ( desiredIndex != -1 )
			  {
					MoveRight( desiredIndex, freeIndex );
					return desiredIndex;
			  }
			  return freeIndex;
		 }

		 private void MoveRight( long fromIndex, long toIndex )
		 {
			  for ( long index = toIndex; index > fromIndex; index-- )
			  {
					_cache.get( index - 1, _scratch );
					_cache.set( index, _scratch );
			  }
		 }

		 /// <returns> cached <seealso cref="RelationshipGroupRecord"/> sorted by node id and then type id. </returns>
		 public override IEnumerator<RelationshipGroupRecord> Iterator()
		 {
			  return new PrefetchingIteratorAnonymousInnerClass( this );
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<RelationshipGroupRecord>
		 {
			 private readonly RelationshipGroupCache _outerInstance;

			 public PrefetchingIteratorAnonymousInnerClass( RelationshipGroupCache outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 nodeId = outerInstance.fromNodeId;
				 countLeftForThisNode = outerInstance.GroupCount( nodeId );

				 findNextNodeWithGroupsIfNeeded();
			 }

			 private long cursor;
			 private long nodeId;
			 private int countLeftForThisNode;

			 protected internal override RelationshipGroupRecord fetchNextOrNull()
			 {
				  while ( cursor < _outerInstance.highCacheId )
				  {
						RelationshipGroupRecord group = null;
						if ( _outerInstance.cache.getByte( cursor, 0 ) == 1 )
						{
							 // Here we have an alive group
							 group = ( new RelationshipGroupRecord( -1 ) ).initialize( true, _outerInstance.cache.get3ByteInt( cursor, 1 ), _outerInstance.cache.get6ByteLong( cursor, 1 + 3 ), _outerInstance.cache.get6ByteLong( cursor, 1 + 3 + 6 ), _outerInstance.cache.get6ByteLong( cursor, 1 + 3 + 6 + 6 ), nodeId, countLeftForThisNode - 1 );
						}

						cursor++;
						countLeftForThisNode--;
						findNextNodeWithGroupsIfNeeded();

						if ( group != null )
						{
							 return group;
						}
				  }
				  return null;
			 }

			 private void findNextNodeWithGroupsIfNeeded()
			 {
				  if ( countLeftForThisNode == 0 )
				  {
						do
						{
							 nodeId++;
							 countLeftForThisNode = nodeId >= _outerInstance.groupCountCache.length() ? 0 : _outerInstance.groupCount(nodeId);
						} while ( countLeftForThisNode == 0 && nodeId < _outerInstance.groupCountCache.length() );
				  }
			 }
		 }

		 public override void AcceptMemoryStatsVisitor( MemoryStatsVisitor visitor )
		 {
			  _groupCountCache.acceptMemoryStatsVisitor( visitor );
			  _cache.acceptMemoryStatsVisitor( visitor );
			  _offsets.acceptMemoryStatsVisitor( visitor );
		 }

		 public override void Close()
		 {
			  _cache.close();
			  _offsets.close();
		 }
	}

}