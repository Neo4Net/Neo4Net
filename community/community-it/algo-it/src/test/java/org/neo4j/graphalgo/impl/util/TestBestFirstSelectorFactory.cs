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
namespace Org.Neo4j.Graphalgo.impl.util
{
	using Neo4jAlgoTestCase = Common.Neo4jAlgoTestCase;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Org.Neo4j.Graphalgo;
	using Direction = Org.Neo4j.Graphdb.Direction;
	using Node = Org.Neo4j.Graphdb.Node;
	using Path = Org.Neo4j.Graphdb.Path;
	using Org.Neo4j.Graphdb;
	using PathExpanders = Org.Neo4j.Graphdb.PathExpanders;
	using Org.Neo4j.Graphdb;
	using TraversalBranch = Org.Neo4j.Graphdb.traversal.TraversalBranch;
	using Traverser = Org.Neo4j.Graphdb.traversal.Traverser;
	using Uniqueness = Org.Neo4j.Graphdb.traversal.Uniqueness;
	using MonoDirectionalTraversalDescription = Org.Neo4j.Kernel.impl.traversal.MonoDirectionalTraversalDescription;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	/// <summary>
	/// @author Anton Persson
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class TestBestFirstSelectorFactory extends common.Neo4jAlgoTestCase
	public class TestBestFirstSelectorFactory : Neo4jAlgoTestCase
	{
		 /*
		  * LAYOUT
		  *
		  *  (a) - 1 -> (b) - 2 -> (d)
		  *   |          ^
		  *   2 -> (c) - 4
		  *
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void buildGraph()
		 public virtual void BuildGraph()
		 {
			  Graph.makePathWithRelProperty( _length, "a-1-b-2-d" );
			  Graph.makePathWithRelProperty( _length, "a-2-c-4-b" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDoWholeTraversalInCorrectOrder()
		 public virtual void ShouldDoWholeTraversalInCorrectOrder()
		 {
			  Node a = Graph.getNode( "a" );

			  Traverser traverser = ( new MonoDirectionalTraversalDescription() ).expand(_expander).order(_factory).uniqueness(_uniqueness).traverse(a);

			  ResourceIterator<Path> iterator = traverser.GetEnumerator();

			  int i = 0;
			  while ( iterator.MoveNext() )
			  {
					assertPath( iterator.Current, _expectedResult[i] );
					i++;
			  }
			  assertEquals( string.Format( "Not all expected paths where traversed. Missing paths are {0}\n", Arrays.ToString( Arrays.copyOfRange( _expectedResult, i, _expectedResult.Length ) ) ), _expectedResult.Length, i );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  return Arrays.asList(new object[][]
			  {
				  new object[]
				  {
					  PathExpanders.allTypesAndDirections(), PathInterestFactory.All(), Uniqueness.NODE_PATH, new string[]{ "a", "a,b", "a,c", "a,b,d", "a,b,c", "a,c,b", "a,c,b,d" }
				  },
				  new object[]
				  {
					  PathExpanders.allTypesAndDirections(), PathInterestFactory.AllShortest(), Uniqueness.NODE_PATH, new string[]{ "a", "a,b", "a,c", "a,b,d" }
				  },
				  new object[]
				  {
					  PathExpanders.forDirection( Direction.OUTGOING ), PathInterestFactory.All(), Uniqueness.NODE_PATH, new string[]{ "a", "a,b", "a,c", "a,b,d", "a,c,b", "a,c,b,d" }
				  },
				  new object[]
				  {
					  PathExpanders.allTypesAndDirections(), PathInterestFactory.All(), Uniqueness.NODE_GLOBAL, new string[]{ "a", "a,b", "a,c", "a,b,d" }
				  },
				  new object[]
				  {
					  PathExpanders.allTypesAndDirections(), PathInterestFactory.All(), Uniqueness.RELATIONSHIP_GLOBAL, new string[]{ "a", "a,b", "a,c", "a,b,d", "a,b,c" }
				  }
			  });
		 }

		 private readonly string _length = "length";
		 private readonly PathExpander _expander;
		 private readonly Uniqueness _uniqueness;
		 private readonly string[] _expectedResult;
		 private readonly BestFirstSelectorFactory<int, int> _factory;

		 public TestBestFirstSelectorFactory( PathExpander expander, PathInterest<int> interest, Uniqueness uniqueness, string[] expectedResult )
		 {

			  this._expander = expander;
			  this._uniqueness = uniqueness;
			  this._expectedResult = expectedResult;
			  _factory = new BestFirstSelectorFactoryAnonymousInnerClass( this, interest );
		 }

		 private class BestFirstSelectorFactoryAnonymousInnerClass : BestFirstSelectorFactory<int, int>
		 {
			 private readonly TestBestFirstSelectorFactory _outerInstance;

			 public BestFirstSelectorFactoryAnonymousInnerClass( TestBestFirstSelectorFactory outerInstance, Org.Neo4j.Graphalgo.impl.util.PathInterest<int> interest ) : base( interest )
			 {
				 this.outerInstance = outerInstance;
				 evaluator = CommonEvaluators.intCostEvaluator( outerInstance.length );
			 }

			 private readonly CostEvaluator<int> evaluator;

			 protected internal override int? StartData
			 {
				 get
				 {
					  return 0;
				 }
			 }

			 protected internal override int? addPriority( TraversalBranch source, int? currentAggregatedValue, int? value )
			 {
				  return value + currentAggregatedValue;
			 }

			 protected internal override int? calculateValue( TraversalBranch next )
			 {
				  return next.Length() == 0 ? 0 : evaluator.getCost(next.LastRelationship(), Direction.BOTH);
			 }
		 }
	}

}