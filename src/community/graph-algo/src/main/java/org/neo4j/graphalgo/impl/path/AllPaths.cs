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
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Neo4Net.GraphDb;
	using TraversalDescription = Neo4Net.GraphDb.traversal.TraversalDescription;
	using Traverser = Neo4Net.GraphDb.traversal.Traverser;
	using Uniqueness = Neo4Net.GraphDb.traversal.Uniqueness;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.traversal.Evaluators.toDepth;

	public class AllPaths : TraversalPathFinder
	{
		 private readonly PathExpander _expander;
		 private readonly int _maxDepth;

		 public AllPaths( int maxDepth, PathExpander expander )
		 {
			  this._maxDepth = maxDepth;
			  this._expander = expander;
		 }

		 protected internal virtual Uniqueness Uniqueness()
		 {
			  return Uniqueness.RELATIONSHIP_PATH;
		 }

		 protected internal override Traverser InstantiateTraverser( Node start, Node end )
		 {
			  // Bidirectional traversal
			  IGraphDatabaseService db = start.GraphDatabase;
			  TraversalDescription @base = Db.traversalDescription().depthFirst().uniqueness(Uniqueness());
			  return Db.bidirectionalTraversalDescription().startSide(@base.Expand(_expander).evaluator(toDepth(_maxDepth / 2))).endSide(@base.Expand(_expander.reverse()).evaluator(toDepth(_maxDepth - _maxDepth / 2))).traverse(start, end);
		 }
	}

}