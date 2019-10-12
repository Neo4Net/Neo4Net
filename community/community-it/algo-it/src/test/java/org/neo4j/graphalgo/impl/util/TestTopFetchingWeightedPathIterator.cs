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
	using Test = org.junit.Test;


	using Org.Neo4j.Graphalgo;
	using Path = Org.Neo4j.Graphdb.Path;
	using NoneStrictMath = Org.Neo4j.Kernel.impl.util.NoneStrictMath;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	/// <summary>
	/// @author Anton Persson
	/// </summary>
	public class TestTopFetchingWeightedPathIterator : Neo4jAlgoTestCase
	{
		private bool InstanceFieldsInitialized = false;

		public TestTopFetchingWeightedPathIterator()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_evaluator = CommonEvaluators.doubleCostEvaluator( _length );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleEmptySource()
		 public virtual void ShouldHandleEmptySource()
		 {
			  _topFetcher = new TopFetchingWeightedPathIterator( Collections.emptyIterator(), _evaluator );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( "Expected iterator to be empty", _topFetcher.hasNext() );
			  assertNull( "Expected null after report has no next", _topFetcher.fetchNextOrNull() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleSinglePath()
		 public virtual void ShouldHandleSinglePath()
		 {
			  Path a = Graph.makePathWithRelProperty( _length, "a1-1-a2" );
			  IList<Path> list = new List<Path>();
			  list.Add( a );

			  _topFetcher = new TopFetchingWeightedPathIterator( list.GetEnumerator(), _evaluator, _epsilon );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( "Expected at least one element", _topFetcher.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertPathDef( a, _topFetcher.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( "Expected no more elements", _topFetcher.hasNext() );
			  assertNull( "Expected null after report has no next", _topFetcher.fetchNextOrNull() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMultipleShortest()
		 public virtual void ShouldHandleMultipleShortest()
		 {
			  Path a = Graph.makePathWithRelProperty( _length, "a1-1-a2" );
			  Path b = Graph.makePathWithRelProperty( _length, "b1-0-b2-1-b3-0-b4" );
			  IList<Path> list = new List<Path>();
			  list.Add( a );
			  list.Add( b );

			  _topFetcher = new TopFetchingWeightedPathIterator( list.GetEnumerator(), _evaluator, _epsilon );
			  IList<Path> result = new List<Path>();
			  while ( _topFetcher.MoveNext() )
			  {
					result.Add( _topFetcher.Current );
			  }

			  AssertPathsWithPaths( result, a, b );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleUnsortedSource()
		 public virtual void ShouldHandleUnsortedSource()
		 {
			  Path a = Graph.makePathWithRelProperty( _length, "a1-1-a2-2-a3" ); // 3
			  Path b = Graph.makePathWithRelProperty( _length, "b1-3-b2-3-b3" ); // 6
			  Path c = Graph.makePathWithRelProperty( _length, "c1-0-c2-1-c3" ); // 1
			  Path d = Graph.makePathWithRelProperty( _length, "d1-3-d2-0-d3" ); // 3
			  Path e = Graph.makePathWithRelProperty( _length, "e1-0-e2-0-e3-0-e4-1-e5" ); // 1

			  IList<Path> list = Arrays.asList( a,b,c,d,e );
			  _topFetcher = new TopFetchingWeightedPathIterator( list.GetEnumerator(), _evaluator, _epsilon );

			  IList<Path> result = new List<Path>();
			  while ( _topFetcher.MoveNext() )
			  {
					result.Add( _topFetcher.Current );
			  }

			  AssertPathsWithPaths( result, c, e );
		 }

		 private readonly double _epsilon = NoneStrictMath.EPSILON;
		 private readonly string _length = "length";
		 private CostEvaluator _evaluator;
		 private TopFetchingWeightedPathIterator _topFetcher;
	}

}