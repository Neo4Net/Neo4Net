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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using IndexOrder = Neo4Net.Internal.Kernel.Api.IndexOrder;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueTuple = Neo4Net.Values.Storable.ValueTuple;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class SortedMergeJoinTest
	public class SortedMergeJoinTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters() public static Iterable<Object[]> data()
		 public static IEnumerable<object[]> Data()
		 {
			  return Arrays.asList(new object[][]
			  {
				  new object[] { IndexOrder.ASCENDING },
				  new object[] { IndexOrder.DESCENDING }
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public org.neo4j.internal.kernel.api.IndexOrder indexOrder;
		 public IndexOrder IndexOrder;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWithEmptyLists()
		 public virtual void ShouldWorkWithEmptyLists()
		 {
			  AssertThatItWorksOneWay( Collections.emptyList(), Collections.emptyList() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWithAList()
		 public virtual void ShouldWorkWithAList()
		 {
			  AssertThatItWorks( Arrays.asList( Node( 1L, "a" ), Node( 3L, "aa" ), Node( 5L, "c" ), Node( 7L, "g" ) ), Collections.emptyList() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWith2Lists()
		 public virtual void ShouldWorkWith2Lists()
		 {
			  AssertThatItWorks( Arrays.asList( Node( 1L, "a" ), Node( 3L, "aa" ), Node( 5L, "c" ), Node( 7L, "g" ) ), Arrays.asList( Node( 2L, "b" ), Node( 4L, "ba" ), Node( 6L, "ca" ), Node( 8L, "d" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWithSameElements()
		 public virtual void ShouldWorkWithSameElements()
		 {
			  AssertThatItWorks( Arrays.asList( Node( 1L, "a" ), Node( 3L, "b" ), Node( 5L, "c" ) ), Arrays.asList( Node( 2L, "aa" ), Node( 3L, "b" ), Node( 6L, "ca" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWithCompositeValues()
		 public virtual void ShouldWorkWithCompositeValues()
		 {
			  AssertThatItWorks( Arrays.asList( Node( 1L, "a", "a" ), Node( 3L, "b", "a" ), Node( 5L, "b", "b" ), Node( 7L, "c", "d" ) ), Arrays.asList( Node( 2L, "a", "b" ), Node( 5L, "b", "b" ), Node( 6L, "c", "e" ) ) );
		 }

		 private void AssertThatItWorks( IList<NodeWithPropertyValues> listA, IList<NodeWithPropertyValues> listB )
		 {
			  AssertThatItWorksOneWay( listA, listB );
			  AssertThatItWorksOneWay( listB, listA );
		 }

		 private void AssertThatItWorksOneWay( IList<NodeWithPropertyValues> listA, IList<NodeWithPropertyValues> listB )
		 {
			  SortedMergeJoin sortedMergeJoin = new SortedMergeJoin();
			  sortedMergeJoin.Initialize( IndexOrder );

			  IComparer<NodeWithPropertyValues> comparator = IndexOrder == IndexOrder.ASCENDING ? ( a, b ) => ValueTuple.COMPARATOR.Compare( ValueTuple.of( a.Values ), ValueTuple.of( b.Values ) ) : ( a, b ) => ValueTuple.COMPARATOR.Compare( ValueTuple.of( b.Values ), ValueTuple.of( a.Values ) );

			  listA.sort( comparator );
			  listB.sort( comparator );

			  IList<NodeWithPropertyValues> result = Process( sortedMergeJoin, listA.GetEnumerator(), listB.GetEnumerator() );

			  IList<NodeWithPropertyValues> expected = new List<NodeWithPropertyValues>();
			  ( ( IList<NodeWithPropertyValues> )expected ).AddRange( listA );
			  ( ( IList<NodeWithPropertyValues> )expected ).AddRange( listB );
			  expected.sort( comparator );

			  assertThat( result, equalTo( expected ) );
		 }

		 private IList<NodeWithPropertyValues> Process( SortedMergeJoin sortedMergeJoin, IEnumerator<NodeWithPropertyValues> iteratorA, IEnumerator<NodeWithPropertyValues> iteratorB )
		 {
			  Collector collector = new Collector( this );
			  while ( !collector.Done )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( iteratorA.hasNext() && sortedMergeJoin.NeedsA() )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 NodeWithPropertyValues a = iteratorA.next();
						 sortedMergeJoin.SetA( a.NodeId, a.Values );
					}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( iteratorB.hasNext() && sortedMergeJoin.NeedsB() )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 NodeWithPropertyValues b = iteratorB.next();
						 sortedMergeJoin.SetB( b.NodeId, b.Values );
					}

					sortedMergeJoin.Next( collector );
			  }
			  return collector.Result;
		 }

		 private NodeWithPropertyValues Node( long id, params object[] values )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return new NodeWithPropertyValues( id, Stream.of( values ).map( Values.of ).toArray( Value[]::new ) );
		 }

		 internal class Collector : SortedMergeJoin.Sink
		 {
			 private readonly SortedMergeJoinTest _outerInstance;

			 public Collector( SortedMergeJoinTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal readonly IList<NodeWithPropertyValues> Result = new List<NodeWithPropertyValues>();
			  internal bool Done;

			  public override void AcceptSortedMergeJoin( long nodeId, Value[] values )
			  {
					if ( nodeId == -1 )
					{
						 Done = true;
					}
					else
					{
						 Result.Add( new NodeWithPropertyValues( nodeId, values ) );
					}
			  }
		 }
	}

}