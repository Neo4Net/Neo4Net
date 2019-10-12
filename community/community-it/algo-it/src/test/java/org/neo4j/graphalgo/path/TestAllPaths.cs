using System.Collections.Generic;

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
namespace Org.Neo4j.Graphalgo.path
{
	using Neo4jAlgoTestCase = Common.Neo4jAlgoTestCase;
	using Test = org.junit.Test;

	using Org.Neo4j.Graphalgo;
	using Path = Org.Neo4j.Graphdb.Path;
	using PathExpanders = Org.Neo4j.Graphdb.PathExpanders;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphalgo.GraphAlgoFactory.allPaths;

	public class TestAllPaths : Neo4jAlgoTestCase
	{
		 protected internal virtual PathFinder<Path> InstantiatePathFinder( int maxDepth )
		 {
			  return allPaths( PathExpanders.allTypesAndDirections(), maxDepth );
		 }

		 private class DijkstraFactoryAnonymousInnerClass : DijkstraFactory
		 {
			 private readonly MissingClass outerInstance;

			 public DijkstraFactoryAnonymousInnerClass( MissingClass outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public PathFinder<WeightedPath> dijkstra( PathExpander expander )
			 {
				  return new Dijkstra( expander, InitialBranchState.NO_STATE, CommonEvaluators.DoubleCostEvaluator( "length" ) );
			 }

			 public PathFinder<WeightedPath> dijkstra( PathExpander expander, CostEvaluator costEvaluator )
			 {
				  return new Dijkstra( expander, InitialBranchState.NO_STATE, costEvaluator );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCircularGraph()
		 public virtual void TestCircularGraph()
		 {
			  /* Layout
			   *
			   * (a)---(b)===(c)---(e)
			   *         \   /
			   *          (d)
			   */
			  Graph.makeEdge( "a", "b" );
			  Graph.makeEdge( "b", "c" );
			  Graph.makeEdge( "b", "c" );
			  Graph.makeEdge( "b", "d" );
			  Graph.makeEdge( "c", "d" );
			  Graph.makeEdge( "c", "e" );

			  PathFinder<Path> finder = InstantiatePathFinder( 10 );
			  IEnumerable<Path> paths = finder.FindAllPaths( Graph.getNode( "a" ), Graph.getNode( "e" ) );
			  AssertPaths( paths, "a,b,c,e", "a,b,c,e", "a,b,d,c,e", "a,b,c,d,b,c,e", "a,b,c,d,b,c,e", "a,b,c,b,d,c,e", "a,b,c,b,d,c,e", "a,b,d,c,b,c,e", "a,b,d,c,b,c,e" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testTripleRelationshipGraph()
		 public virtual void TestTripleRelationshipGraph()
		 {
			  /* Layout
			   *          ___
			   * (a)---(b)===(c)---(d)
			   */
			  Graph.makeEdge( "a", "b" );
			  Graph.makeEdge( "b", "c" );
			  Graph.makeEdge( "b", "c" );
			  Graph.makeEdge( "b", "c" );
			  Graph.makeEdge( "c", "d" );

			  PathFinder<Path> finder = InstantiatePathFinder( 10 );
			  IEnumerable<Path> paths = finder.FindAllPaths( Graph.getNode( "a" ), Graph.getNode( "d" ) );
			  AssertPaths( paths, "a,b,c,d", "a,b,c,d", "a,b,c,d", "a,b,c,b,c,d", "a,b,c,b,c,d", "a,b,c,b,c,d", "a,b,c,b,c,d", "a,b,c,b,c,d", "a,b,c,b,c,d" );
		 }
	}

}