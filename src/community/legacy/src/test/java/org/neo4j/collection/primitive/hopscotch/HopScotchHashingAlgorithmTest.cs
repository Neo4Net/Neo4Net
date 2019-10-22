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
namespace Neo4Net.Collections.primitive.hopscotch
{
	using Test = org.junit.jupiter.api.Test;


	using Monitor = Neo4Net.Collections.primitive.hopscotch.HopScotchHashingAlgorithm.Monitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.collection.primitive.Primitive.VALUE_MARKER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.collection.primitive.hopscotch.HopScotchHashingAlgorithm.DEFAULT_H;

	internal class HopScotchHashingAlgorithmTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSupportIteratingThroughResize()
		 internal virtual void ShouldSupportIteratingThroughResize()
		 {
			  // GIVEN
			  int threshold = FigureOutGrowthThreshold();
			  TableGrowthAwareMonitor monitor = new TableGrowthAwareMonitor();
			  PrimitiveLongSet set = new PrimitiveLongHashSet( new LongKeyTable<object>( DEFAULT_H, VALUE_MARKER ), VALUE_MARKER, monitor );
			  ISet<long> added = new HashSet<long>();
			  for ( long i = 0; i < threshold - 1; i++ )
			  {
					long value = i * 3;
					set.Add( value );
					added.Add( value );
			  }

			  // WHEN
			  PrimitiveLongIterator iterator = set.GetEnumerator();
			  ISet<long> iterated = new HashSet<long>();
			  for ( int i = 0; i < threshold / 2; i++ )
			  {
					iterated.Add( iterator.Next() );
			  }
			  assertFalse( monitor.CheckAndReset() );
			  // will push it over the edge, to grow the table
			  set.Add( ( threshold - 1 ) * 3 );
			  assertTrue( monitor.CheckAndReset() );
			  while ( iterator.HasNext() )
			  {
					iterated.Add( iterator.Next() );
			  }

			  // THEN
			  assertEquals( added, iterated );
		 }

		 private class TableGrowthAwareMonitor : Neo4Net.Collections.primitive.hopscotch.HopScotchHashingAlgorithm.Monitor_Adapter
		 {
			  internal bool Grew;

			  public override bool TableGrew( int fromCapacity, int toCapacity, int currentSize )
			  {
					return Grew = true;
			  }

			  internal virtual bool CheckAndReset()
			  {
					try
					{
						 return Grew;
					}
					finally
					{
						 Grew = false;
					}
			  }
		 }

		 private static int FigureOutGrowthThreshold()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean grew = new java.util.concurrent.atomic.AtomicBoolean();
			  AtomicBoolean grew = new AtomicBoolean();
			  Monitor monitor = new Monitor_AdapterAnonymousInnerClass( grew );
			  using ( PrimitiveLongSet set = new PrimitiveLongHashSet( new LongKeyTable<object>( DEFAULT_H, VALUE_MARKER ), VALUE_MARKER, monitor ) )
			  {
					int i = 0;
					for ( i = 0; !grew.get(); i++ )
					{
						 set.Add( i * 3 );
					}
					return i;
			  }
		 }

		 private class Monitor_AdapterAnonymousInnerClass : Neo4Net.Collections.primitive.hopscotch.HopScotchHashingAlgorithm.Monitor_Adapter
		 {
			 private AtomicBoolean _grew;

			 public Monitor_AdapterAnonymousInnerClass( AtomicBoolean grew )
			 {
				 this._grew = grew;
			 }

			 public override bool tableGrew( int fromCapacity, int toCapacity, int currentSize )
			 {
				  _grew.set( true );
				  return true;
			 }
		 }
	}

}