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
namespace Neo4Net.Graphalgo.impl.path
{
	using LiteDepthFirstSelector = Neo4Net.Graphalgo.impl.util.LiteDepthFirstSelector;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Neo4Net.GraphDb;
	using TraversalDescription = Neo4Net.GraphDb.traversal.TraversalDescription;
	using Traverser = Neo4Net.GraphDb.traversal.Traverser;
	using Uniqueness = Neo4Net.GraphDb.traversal.Uniqueness;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.traversal.Evaluators.atDepth;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.traversal.Evaluators.toDepth;

	/// <summary>
	/// Tries to find paths in a graph from a start node to an end node where the
	/// length of found paths must be of a certain length. It also detects
	/// "super nodes", i.e. nodes which have many relationships and only iterates
	/// over such super nodes' relationships up to a supplied threshold. When that
	/// threshold is reached such nodes are considered super nodes and are put on a
	/// queue for later traversal. This makes it possible to find paths w/o having to
	/// traverse heavy super nodes.
	/// 
	/// @author Mattias Persson
	/// @author Tobias Ivarsson
	/// </summary>
	public class ExactDepthPathFinder : TraversalPathFinder
	{
		 private readonly PathExpander _expander;
		 private readonly int _onDepth;
		 private readonly int _startThreshold;
		 private readonly Uniqueness _uniqueness;

		 public ExactDepthPathFinder( PathExpander expander, int onDepth, int startThreshold, bool allowLoops )
		 {
			  this._expander = expander;
			  this._onDepth = onDepth;
			  this._startThreshold = startThreshold;
			  this._uniqueness = allowLoops ? Uniqueness.RELATIONSHIP_GLOBAL : Uniqueness.NODE_PATH;
		 }

		 protected internal override Traverser InstantiateTraverser( Node start, Node end )
		 {
			  IGraphDatabaseService db = start.GraphDatabase;
			  TraversalDescription side = Db.traversalDescription().breadthFirst().uniqueness(_uniqueness).order((startSource, _expander) => new LiteDepthFirstSelector(startSource, _startThreshold, _expander));
			  return Db.bidirectionalTraversalDescription().startSide(side.Expand(_expander).evaluator(toDepth(_onDepth / 2))).endSide(side.Expand(_expander.reverse()).evaluator(toDepth(_onDepth - _onDepth / 2))).collisionEvaluator(atDepth(_onDepth)).traverse(start, end);
		 }
	}

}