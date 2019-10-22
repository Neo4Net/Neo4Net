﻿/*
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
namespace Neo4Net.Graphalgo.impl.path
{
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Neo4Net.GraphDb;
	using TraversalDescription = Neo4Net.GraphDb.traversal.TraversalDescription;
	using Traverser = Neo4Net.GraphDb.traversal.Traverser;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.traversal.Evaluators.toDepth;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.traversal.SideSelectorPolicies.LEVEL_STOP_DESCENT_ON_RESULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.traversal.Uniqueness.NODE_PATH;

	/// <summary>
	/// Implements shortest path algorithm, see <seealso cref="ShortestPath"/>, but using
	/// the traversal framework straight off with the bidirectional traversal feature.
	/// 
	/// It's still experimental and slightly slower than the highly optimized
	/// <seealso cref="ShortestPath"/> implementation.
	/// </summary>
	public class TraversalShortestPath : TraversalPathFinder
	{
		 private readonly PathExpander _expander;
		 private readonly int _maxDepth;
		 private readonly int? _maxResultCount;

		 public TraversalShortestPath( PathExpander expander, int maxDepth )
		 {
			  this._expander = expander;
			  this._maxDepth = maxDepth;
			  this._maxResultCount = null;
		 }

		 public TraversalShortestPath( PathExpander expander, int maxDepth, int maxResultCount )
		 {
			  this._expander = expander;
			  this._maxDepth = maxDepth;
			  this._maxResultCount = maxResultCount;
		 }

		 protected internal override Traverser InstantiateTraverser( Node start, Node end )
		 {
			  IGraphDatabaseService db = start.GraphDatabase;
			  TraversalDescription sideBase = Db.traversalDescription().breadthFirst().uniqueness(NODE_PATH);
			  return Db.bidirectionalTraversalDescription().mirroredSides(sideBase.Expand(_expander)).sideSelector(LEVEL_STOP_DESCENT_ON_RESULT, _maxDepth).collisionEvaluator(toDepth(_maxDepth)).traverse(start, end);
		 }

		 protected internal override int? MaxResultCount()
		 {
			  return _maxResultCount;
		 }
	}

}