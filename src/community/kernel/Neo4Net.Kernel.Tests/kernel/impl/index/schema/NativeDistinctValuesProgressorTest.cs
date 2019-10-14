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
	using MutableInt = org.apache.commons.lang3.mutable.MutableInt;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.Cursors;
	using Neo4Net.Index.Internal.gbptree;
	using IndexOrder = Neo4Net.Internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.Internal.Kernel.Api.IndexQuery;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexKey.Inclusion.NEUTRAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexValue.INSTANCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

	public class NativeDistinctValuesProgressorTest
	{
		 private readonly StringLayout _layout = new StringLayout();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCountDistinctValues()
		 public virtual void ShouldCountDistinctValues()
		 {
			  // given
			  Value[] strings = GenerateRandomStrings();
			  DataCursor source = new DataCursor( AsHitData( strings ) );
			  GatheringNodeValueClient client = new GatheringNodeValueClient();

			  // when
			  NativeDistinctValuesProgressor<StringIndexKey, NativeIndexValue> progressor = new NativeDistinctValuesProgressor<StringIndexKey, NativeIndexValue>( source, client, new List<RawCursor<Hit<KEY, VALUE>, IOException>>(), _layout, _layout.compareValue );
			  client.Initialize( null, progressor, new IndexQuery[0], IndexOrder.NONE, true );
			  IDictionary<Value, MutableInt> expectedCounts = AsDistinctCounts( strings );

			  // then
			  int uniqueValues = 0;
			  int nonUniqueValues = 0;
			  while ( progressor.Next() )
			  {
					Value @string = client.Values[0];
					MutableInt expectedCount = expectedCounts.Remove( @string );
					assertNotNull( expectedCount );
					assertEquals( expectedCount.intValue(), client.Reference );

					if ( expectedCount.intValue() > 1 )
					{
						 nonUniqueValues++;
					}
					else
					{
						 uniqueValues++;
					}
			  }
			  assertTrue( expectedCounts.Count == 0 );
			  assertTrue( uniqueValues > 0 );
			  assertTrue( nonUniqueValues > 0 );
		 }

		 private IDictionary<Value, MutableInt> AsDistinctCounts( Value[] strings )
		 {
			  IDictionary<Value, MutableInt> map = new Dictionary<Value, MutableInt>();
			  foreach ( Value @string in strings )
			  {
					map.computeIfAbsent( @string, s => new MutableInt( 0 ) ).increment();
			  }
			  return map;
		 }

		 private Value[] GenerateRandomStrings()
		 {
			  Value[] strings = new Value[1_000];
			  for ( int i = 0; i < strings.Length; i++ )
			  {
					// Potential for a lot of duplicates
					strings[i] = stringValue( Random.Next( 1_000 ).ToString() );
			  }
			  Arrays.sort( strings, Values.COMPARATOR );
			  return strings;
		 }

		 private ICollection<Hit<StringIndexKey, NativeIndexValue>> AsHitData( Value[] strings )
		 {
			  ICollection<Hit<StringIndexKey, NativeIndexValue>> data = new List<Hit<StringIndexKey, NativeIndexValue>>( strings.Length );
			  for ( int i = 0; i < strings.Length; i++ )
			  {
					StringIndexKey key = _layout.newKey();
					key.Initialize( i );
					key.initFromValue( 0, strings[i], NEUTRAL );
					data.Add( new SimpleHit<>( key, INSTANCE ) );
			  }
			  return data;
		 }

		 private class DataCursor : RawCursor<Hit<StringIndexKey, NativeIndexValue>, IOException>
		 {
			  internal readonly IEnumerator<Hit<StringIndexKey, NativeIndexValue>> Iterator;
			  internal Hit<StringIndexKey, NativeIndexValue> Current;

			  internal DataCursor( ICollection<Hit<StringIndexKey, NativeIndexValue>> data )
			  {
					this.Iterator = data.GetEnumerator();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws RuntimeException
			  public override bool Next()
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( !Iterator.hasNext() )
					{
						 return false;
					}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					Current = Iterator.next();
					return true;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws RuntimeException
			  public override void Close()
			  {
					// Nothing to close
			  }

			  public override Hit<StringIndexKey, NativeIndexValue> Get()
			  {
					return Current;
			  }
		 }
	}

}