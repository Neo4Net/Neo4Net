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
namespace Neo4Net.Kernel.impl.store.kvstore
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.kvstore.KeyValueMergerTest.Pair.pair;

	public class KeyValueMergerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMergeEmptyProviders() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMergeEmptyProviders()
		 {
			  // given
			  KeyValueMerger merger = new KeyValueMerger( Provider(), Provider(), 4, 4 );

			  // when
			  IList<int> data = Extract( merger );

			  // then
			  assertEquals( Arrays.asList<int>(), data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideUpdatesWhenNoDataProvided() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideUpdatesWhenNoDataProvided()
		 {
			  // given
			  KeyValueMerger merger = new KeyValueMerger( Provider(), Provider(pair(14, 1), pair(19, 2), pair(128, 3)), 4, 4 );

			  // when
			  IList<int> data = Extract( merger );

			  // then
			  assertEquals( asList( 14, 1, 19, 2, 128, 3 ), data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideUpdatesWhenNoChangesProvided() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideUpdatesWhenNoChangesProvided()
		 {
			  // given
			  KeyValueMerger merger = new KeyValueMerger( Provider( pair( 14, 1 ), pair( 19, 2 ), pair( 128, 3 ) ), Provider(), 4, 4 );

			  // when
			  IList<int> data = Extract( merger );

			  // then
			  assertEquals( asList( 14, 1, 19, 2, 128, 3 ), data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMergeDataStreams() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMergeDataStreams()
		 {
			  // given
			  KeyValueMerger merger = new KeyValueMerger( Provider( pair( 1, 1 ), pair( 3, 1 ), pair( 5, 1 ) ), Provider( pair( 2, 2 ), pair( 4, 2 ), pair( 6, 2 ) ), 4, 4 );

			  // when
			  IList<int> data = Extract( merger );

			  // then
			  assertEquals( asList( 1, 1, 2, 2, 3, 1, 4, 2, 5, 1, 6, 2 ), data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReplaceValuesOnEqualKey() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReplaceValuesOnEqualKey()
		 {
			  // given
			  KeyValueMerger merger = new KeyValueMerger( Provider( pair( 1, 1 ), pair( 3, 1 ), pair( 5, 1 ) ), Provider( pair( 2, 2 ), pair( 3, 2 ), pair( 6, 2 ) ), 4, 4 );

			  // when
			  IList<int> data = Extract( merger );

			  // then
			  assertEquals( asList( 1, 1, 2, 2, 3, 2, 5, 1, 6, 2 ), data );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.util.List<int> extract(EntryVisitor<WritableBuffer> producer) throws java.io.IOException
		 private static IList<int> Extract( EntryVisitor<WritableBuffer> producer )
		 {
			  IList<int> result = new List<int>();
			  BigEndianByteArrayBuffer key = new BigEndianByteArrayBuffer( 4 );
			  BigEndianByteArrayBuffer value = new BigEndianByteArrayBuffer( 4 );
			  while ( producer.Visit( key, value ) )
			  {
					result.Add( key.GetInt( 0 ) );
					result.Add( value.GetInt( 0 ) );
			  }
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: static DataProvider provider(final Pair... data)
		 internal static DataProvider Provider( params Pair[] data )
		 {
			  return new DataProviderAnonymousInnerClass( data );
		 }

		 private class DataProviderAnonymousInnerClass : DataProvider
		 {
			 private Neo4Net.Kernel.impl.store.kvstore.KeyValueMergerTest.Pair[] _data;

			 public DataProviderAnonymousInnerClass( Neo4Net.Kernel.impl.store.kvstore.KeyValueMergerTest.Pair[] data )
			 {
				 this._data = data;
			 }

			 internal int i;

			 public bool visit( WritableBuffer key, WritableBuffer value )
			 {
				  if ( i < _data.Length )
				  {
						_data[i++].visit( key, value );
						return true;
				  }
				  return false;
			 }

			 public void close()
			 {
			 }
		 }

		 internal class Pair
		 {
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  internal static Pair PairConflict( int key, int value )
			  {
					return new Pair( key, value );
			  }

			  internal readonly int Key;
			  internal int Value;

			  internal Pair( int key, int value )
			  {
					this.Key = key;
					this.Value = value;
			  }

			  internal virtual void Visit( WritableBuffer key, WritableBuffer value )
			  {
					key.PutInt( 0, this.Key );
					value.PutInt( 0, this.Value );
			  }
		 }
	}

}