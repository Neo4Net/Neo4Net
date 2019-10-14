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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using MutableLong = org.apache.commons.lang3.mutable.MutableLong;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using Neo4Net.Cursors;
	using Neo4Net.Index.Internal.gbptree;
	using SimpleLongLayout = Neo4Net.Index.Internal.gbptree.SimpleLongLayout;
	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) class CombinedPartSeekerTest
	internal class CombinedPartSeekerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
		 private static readonly IComparer<Hit<MutableLong, MutableLong>> _hitComparator = System.Collections.IComparer.comparing( Hit::key );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject RandomRule random;
		 internal RandomRule Random;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCombineAllParts() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCombineAllParts()
		 {
			  // given
			  SimpleLongLayout layout = new SimpleLongLayout( 0, "", true, 1, 2, 3 );
			  IList<RawCursor<Hit<MutableLong, MutableLong>, IOException>> parts = new List<RawCursor<Hit<MutableLong, MutableLong>, IOException>>();
			  int partCount = Random.Next( 1, 20 );
			  IList<Hit<MutableLong, MutableLong>> expectedAllData = new List<Hit<MutableLong, MutableLong>>();
			  int maxKey = Random.Next( 100, 10_000 );
			  for ( int i = 0; i < partCount; i++ )
			  {
					int dataSize = Random.Next( 0, 100 );
					IList<Hit<MutableLong, MutableLong>> partData = new List<Hit<MutableLong, MutableLong>>( dataSize );
					for ( int j = 0; j < dataSize; j++ )
					{
						 long key = Random.nextLong( maxKey );
						 partData.Add( new SimpleHit<>( new MutableLong( key ), new MutableLong( key * 2 ) ) );
					}
					partData.sort( _hitComparator );
					parts.Add( new SimpleSeeker( partData ) );
					( ( IList<Hit<MutableLong, MutableLong>> )expectedAllData ).AddRange( partData );
			  }
			  expectedAllData.sort( _hitComparator );

			  // when
			  CombinedPartSeeker<MutableLong, MutableLong> combinedSeeker = new CombinedPartSeeker<MutableLong, MutableLong>( layout, parts );

			  // then
			  foreach ( Hit<MutableLong, MutableLong> expectedHit in expectedAllData )
			  {
					assertTrue( combinedSeeker.Next() );
					Hit<MutableLong, MutableLong> actualHit = combinedSeeker.Get();

					assertEquals( expectedHit.Key().longValue(), actualHit.Key().longValue() );
					assertEquals( expectedHit.Value().longValue(), actualHit.Value().longValue() );
			  }
			  assertFalse( combinedSeeker.Next() );
			  // And just ensure it will return false again after that
			  assertFalse( combinedSeeker.Next() );
		 }

		 private class SimpleSeeker : RawCursor<Hit<MutableLong, MutableLong>, IOException>
		 {
			  internal readonly IEnumerator<Hit<MutableLong, MutableLong>> Data;
			  internal Hit<MutableLong, MutableLong> Current;

			  internal SimpleSeeker( IEnumerable<Hit<MutableLong, MutableLong>> data )
			  {
					this.Data = data.GetEnumerator();
			  }

			  public override bool Next()
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( Data.hasNext() )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 Current = Data.next();
						 return true;
					}
					return false;
			  }

			  public override void Close()
			  {
					// Nothing to close
			  }

			  public override Hit<MutableLong, MutableLong> Get()
			  {
					return Current;
			  }
		 }
	}

}