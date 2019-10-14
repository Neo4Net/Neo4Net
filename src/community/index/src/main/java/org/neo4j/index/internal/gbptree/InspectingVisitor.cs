using System.Collections.Generic;

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
namespace Neo4Net.Index.Internal.gbptree
{
	using Pair = org.apache.commons.lang3.tuple.Pair;
	using ImmutableLongList = org.eclipse.collections.api.list.primitive.ImmutableLongList;
	using LongList = org.eclipse.collections.api.list.primitive.LongList;
	using MutableLongList = org.eclipse.collections.api.list.primitive.MutableLongList;
	using LongLists = org.eclipse.collections.impl.factory.primitive.LongLists;



	public class InspectingVisitor<KEY, VALUE> : GBPTreeVisitor_Adaptor<KEY, VALUE>
	{
		 private readonly MutableLongList _internalNodes = LongLists.mutable.empty();
		 private readonly MutableLongList _leafNodes = LongLists.mutable.empty();
		 private readonly MutableLongList _allNodes = LongLists.mutable.empty();
		 private readonly IDictionary<long, int> _allKeyCounts = new Dictionary<long, int>();
		 private readonly IList<LongList> _nodesPerLevel = new List<LongList>();
		 private readonly IList<FreelistEntry> _allFreelistEntries = new List<FreelistEntry>();
		 private long _rootNode;
		 private int _lastLevel;
		 private TreeState _treeState;
		 private MutableLongList _currentLevelNodes;
		 private long _currentFreelistPage;

		 public InspectingVisitor()
		 {
			  Clear();
		 }

		 public virtual GBPTreeInspection<KEY, VALUE> Get()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.eclipse.collections.api.list.primitive.ImmutableLongList> immutableNodesPerLevel = nodesPerLevel.stream().map(org.eclipse.collections.impl.factory.primitive.LongLists.immutable::ofAll).collect(java.util.stream.Collectors.toList());
			  IList<ImmutableLongList> immutableNodesPerLevel = _nodesPerLevel.Select( LongLists.immutable.ofAll ).ToList();
			  return new GBPTreeInspection<KEY, VALUE>( LongLists.immutable.ofAll( _internalNodes ), LongLists.immutable.ofAll( _leafNodes ), LongLists.immutable.ofAll( _allNodes ), unmodifiableMap( _allKeyCounts ), immutableNodesPerLevel, unmodifiableList( _allFreelistEntries ), _rootNode, _lastLevel, _treeState );
		 }

		 public override void TreeState( Pair<TreeState, TreeState> statePair )
		 {
			  this._treeState = TreeStatePair.SelectNewestValidState( statePair );
		 }

		 public override void BeginLevel( int level )
		 {
			  _lastLevel = level;
			  _currentLevelNodes = LongLists.mutable.empty();
			  _nodesPerLevel.Add( _currentLevelNodes );
		 }

		 public override void BeginNode( long pageId, bool isLeaf, long generation, int keyCount )
		 {
			  if ( _lastLevel == 0 )
			  {
					if ( _rootNode != -1 )
					{
						 throw new System.InvalidOperationException( "Expected to only have a single node on level 0" );
					}
					_rootNode = pageId;
			  }

			  _currentLevelNodes.add( pageId );
			  _allNodes.add( pageId );
			  _allKeyCounts[pageId] = keyCount;
			  if ( isLeaf )
			  {
					_leafNodes.add( pageId );
			  }
			  else
			  {
					_internalNodes.add( pageId );
			  }
		 }

		 public override void BeginFreelistPage( long pageId )
		 {
			  _currentFreelistPage = pageId;
		 }

		 public override void FreelistEntry( long pageId, long generation, int pos )
		 {
			  _allFreelistEntries.Add( new FreelistEntry( _currentFreelistPage, pos, pageId, generation ) );
		 }

		 private void Clear()
		 {
			  _rootNode = -1;
			  _lastLevel = -1;
		 }

		 internal class FreelistEntry
		 {
			  internal readonly long FreelistPageId;
			  internal readonly int Pos;
			  internal readonly long Id;
			  internal readonly long Generation;

			  internal FreelistEntry( long freelistPageId, int pos, long id, long generation )
			  {
					this.FreelistPageId = freelistPageId;
					this.Pos = pos;
					this.Id = id;
					this.Generation = generation;
			  }
		 }
	}

}