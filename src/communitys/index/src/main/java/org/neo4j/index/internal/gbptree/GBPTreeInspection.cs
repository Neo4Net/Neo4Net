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
namespace Neo4Net.Index.@internal.gbptree
{
	using ImmutableLongList = org.eclipse.collections.api.list.primitive.ImmutableLongList;


	public class GBPTreeInspection<KEY, VALUE>
	{
		 private readonly ImmutableLongList _internalNodes;
		 private readonly ImmutableLongList _leafNodes;
		 private readonly ImmutableLongList _allNodes;
		 private readonly IDictionary<long, int> _keyCounts;
		 private readonly IList<ImmutableLongList> _nodesPerLevel;
		 private readonly IList<InspectingVisitor.FreelistEntry> _allFreelistEntries;
		 private readonly long _rootNode;
		 private readonly int _lastLevel;
		 private readonly TreeState _treeState;

		 internal GBPTreeInspection( ImmutableLongList internalNodes, ImmutableLongList leafNodes, ImmutableLongList allNodes, IDictionary<long, int> keyCounts, IList<ImmutableLongList> nodesPerLevel, IList<InspectingVisitor.FreelistEntry> allFreelistEntries, long rootNode, int lastLevel, TreeState treeState )
		 {
			  this._internalNodes = internalNodes;
			  this._leafNodes = leafNodes;
			  this._allNodes = allNodes;
			  this._keyCounts = keyCounts;
			  this._nodesPerLevel = nodesPerLevel;
			  this._allFreelistEntries = allFreelistEntries;
			  this._rootNode = rootNode;
			  this._lastLevel = lastLevel;
			  this._treeState = treeState;
		 }

		 public virtual ImmutableLongList InternalNodes
		 {
			 get
			 {
				  return _internalNodes;
			 }
		 }

		 public virtual ImmutableLongList LeafNodes
		 {
			 get
			 {
				  return _leafNodes;
			 }
		 }

		 public virtual ImmutableLongList AllNodes
		 {
			 get
			 {
				  return _allNodes;
			 }
		 }

		 public virtual IDictionary<long, int> KeyCounts
		 {
			 get
			 {
				  return _keyCounts;
			 }
		 }

		 public virtual IList<ImmutableLongList> NodesPerLevel
		 {
			 get
			 {
				  return _nodesPerLevel;
			 }
		 }

		 public virtual IList<InspectingVisitor.FreelistEntry> AllFreelistEntries
		 {
			 get
			 {
				  return _allFreelistEntries;
			 }
		 }

		 public virtual long RootNode
		 {
			 get
			 {
				  return _rootNode;
			 }
		 }

		 public virtual int LastLevel
		 {
			 get
			 {
				  return _lastLevel;
			 }
		 }

		 public virtual TreeState TreeState
		 {
			 get
			 {
				  return _treeState;
			 }
		 }
	}

}