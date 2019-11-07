using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.core
{
	using Test = org.junit.Test;


	using Config = Neo4Net.causalclustering.core.BoundedPriorityQueue.Config;
	using Neo4Net.causalclustering.core.BoundedPriorityQueue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.BoundedPriorityQueue.Result.E_COUNT_EXCEEDED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.BoundedPriorityQueue.Result.E_SIZE_EXCEEDED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.BoundedPriorityQueue.Result.OK;

	public class BoundedPriorityQueueTest
	{
		 private readonly Config _baseConfig = new Config( 0, 5, 100 );
		 private readonly IComparer<int> _noPriority = ( a, b ) => 0;

		 private readonly ThreadLocalRandom _tlr = ThreadLocalRandom.current();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportTotalCountAndSize()
		 public virtual void ShouldReportTotalCountAndSize()
		 {
			  BoundedPriorityQueue<int> queue = new BoundedPriorityQueue<int>( _baseConfig, int?.longValue, _noPriority );

			  assertEquals( 0, queue.Bytes() );
			  assertEquals( 0, queue.Count() );

			  queue.Offer( 10 );
			  assertEquals( 1, queue.Count() );
			  assertEquals( 10, queue.Bytes() );

			  queue.Offer( 20 );
			  assertEquals( 2, queue.Count() );
			  assertEquals( 30, queue.Bytes() );

			  queue.Poll();
			  assertEquals( 1, queue.Count() );
			  assertEquals( 20, queue.Bytes() );

			  queue.Poll();
			  assertEquals( 0, queue.Count() );
			  assertEquals( 0, queue.Bytes() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowMoreThanMaxBytes()
		 public virtual void ShouldNotAllowMoreThanMaxBytes()
		 {
			  BoundedPriorityQueue<int> queue = new BoundedPriorityQueue<int>( _baseConfig, int?.longValue, _noPriority );

			  assertEquals( E_SIZE_EXCEEDED, queue.Offer( 101 ) );
			  assertEquals( OK, queue.Offer( 99 ) );
			  assertEquals( OK, queue.Offer( 1 ) );
			  assertEquals( E_SIZE_EXCEEDED, queue.Offer( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowMinCountDespiteSizeLimit()
		 public virtual void ShouldAllowMinCountDespiteSizeLimit()
		 {
			  Config config = new Config( 2, 5, 100 );
			  BoundedPriorityQueue<int> queue = new BoundedPriorityQueue<int>( config, int?.longValue, _noPriority );

			  assertEquals( OK, queue.Offer( 101 ) );
			  assertEquals( OK, queue.Offer( 101 ) );
			  assertEquals( E_SIZE_EXCEEDED, queue.Offer( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowZeroSizedItemsDespiteSizeLimit()
		 public virtual void ShouldAllowZeroSizedItemsDespiteSizeLimit()
		 {
			  BoundedPriorityQueue<int> queue = new BoundedPriorityQueue<int>( _baseConfig, int?.longValue, _noPriority );

			  assertEquals( OK, queue.Offer( 100 ) );
			  assertEquals( E_SIZE_EXCEEDED, queue.Offer( 1 ) );

			  assertEquals( OK, queue.Offer( 0 ) );
			  assertEquals( OK, queue.Offer( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowMoreThanMaxCount()
		 public virtual void ShouldNotAllowMoreThanMaxCount()
		 {
			  BoundedPriorityQueue<int> queue = new BoundedPriorityQueue<int>( _baseConfig, int?.longValue, _noPriority );

			  assertEquals( OK, queue.Offer( 1 ) );
			  assertEquals( OK, queue.Offer( 1 ) );
			  assertEquals( OK, queue.Offer( 1 ) );
			  assertEquals( OK, queue.Offer( 1 ) );
			  assertEquals( OK, queue.Offer( 1 ) );

			  assertEquals( E_COUNT_EXCEEDED, queue.Offer( 1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowMoreThanMaxCountDespiteZeroSize()
		 public virtual void ShouldNotAllowMoreThanMaxCountDespiteZeroSize()
		 {
			  BoundedPriorityQueue<int> queue = new BoundedPriorityQueue<int>( _baseConfig, int?.longValue, _noPriority );

			  assertEquals( OK, queue.Offer( 0 ) );
			  assertEquals( OK, queue.Offer( 0 ) );
			  assertEquals( OK, queue.Offer( 0 ) );
			  assertEquals( OK, queue.Offer( 0 ) );
			  assertEquals( OK, queue.Offer( 0 ) );

			  assertEquals( E_COUNT_EXCEEDED, queue.Offer( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToPeekEntries()
		 public virtual void ShouldBeAbleToPeekEntries()
		 {
			  BoundedPriorityQueue<int> queue = new BoundedPriorityQueue<int>( _baseConfig, int?.longValue, _noPriority );

			  assertEquals( OK, queue.Offer( 1 ) );
			  assertEquals( OK, queue.Offer( 2 ) );
			  assertEquals( OK, queue.Offer( 3 ) );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertEquals( 1, queue.Peek().map(Removable::get) );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertEquals( 1, queue.Peek().map(Removable::get) );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertEquals( 1, queue.Peek().map(Removable::get) );

			  assertEquals( 3, queue.Count() );
			  assertEquals( 6, queue.Bytes() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRemovePeekedEntries()
		 public virtual void ShouldBeAbleToRemovePeekedEntries()
		 {
			  BoundedPriorityQueue<int> queue = new BoundedPriorityQueue<int>( _baseConfig, int?.longValue, _noPriority );

			  assertEquals( OK, queue.Offer( 1 ) );
			  assertEquals( OK, queue.Offer( 2 ) );
			  assertEquals( OK, queue.Offer( 3 ) );
			  assertEquals( 3, queue.Count() );
			  assertEquals( 6, queue.Bytes() );

			  assertTrue( queue.Peek().Present );
			  assertTrue( queue.Peek().get().remove() );
			  assertEquals( 2, queue.Count() );
			  assertEquals( 5, queue.Bytes() );

			  assertTrue( queue.Peek().Present );
			  assertTrue( queue.Peek().get().remove() );
			  assertEquals( 1, queue.Count() );
			  assertEquals( 3, queue.Bytes() );

			  assertTrue( queue.Peek().Present );
			  assertTrue( queue.Peek().get().remove() );
			  assertEquals( 0, queue.Count() );
			  assertEquals( 0, queue.Bytes() );

			  assertFalse( queue.Peek().Present );
			  try
			  {
					queue.Peek().get().remove();
					fail();
			  }
			  catch ( NoSuchElementException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectPriority()
		 public virtual void ShouldRespectPriority()
		 {
			  int count = 100;
			  Config config = new Config( 0, count, 0 );
			  BoundedPriorityQueue<int> queue = new BoundedPriorityQueue<int>( config, i => 0L, int?.compare );

			  IList<int> list = new List<int>( count );
			  for ( int i = 0; i < count; i++ )
			  {
					list.Add( i );
			  }

			  Collections.shuffle( list, _tlr );
			  list.ForEach( queue.offer );

			  for ( int i = 0; i < count; i++ )
			  {
					assertEquals( i, queue.Poll() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveStablePriority()
		 public virtual void ShouldHaveStablePriority()
		 {
			  int count = 100;
			  int priorities = 3;

			  Config config = new Config( 0, count, 0 );
			  BoundedPriorityQueue<Element> queue = new BoundedPriorityQueue<Element>( config, i => 0L, comparingInt( p => p.priority ) );

			  IList<Element> insertionOrder = new List<Element>( count );
			  for ( int i = 0; i < count; i++ )
			  {
					insertionOrder.Add( new Element( this, _tlr.Next( priorities ) ) );
			  }

			  Collections.shuffle( insertionOrder, _tlr );
			  insertionOrder.ForEach( queue.offer );

			  for ( int p = 0; p < priorities; p++ )
			  {
					List<Element> filteredInsertionOrder = new List<Element>();
					foreach ( Element element in insertionOrder )
					{
						 if ( element.Priority == p )
						 {
							  filteredInsertionOrder.Add( element );
						 }
					}

					foreach ( Element element in filteredInsertionOrder )
					{
						 assertEquals( element, queue.Poll() );
					}
			  }
		 }

		 internal class Element
		 {
			 private readonly BoundedPriorityQueueTest _outerInstance;

			  internal int Priority;

			  internal Element( BoundedPriorityQueueTest outerInstance, int priority )
			  {
				  this._outerInstance = outerInstance;
					this.Priority = priority;
			  }

			  public override bool Equals( object o )
			  {
					return this == o;
			  }

			  public override int GetHashCode()
			  {
					return Objects.hash( Priority );
			  }
		 }
	}

}