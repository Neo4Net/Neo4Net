using System;
using System.Collections;
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
namespace Neo4Net.Index.@internal.gbptree
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using Neo4Net.Cursors;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.rules.RuleChain.outerRule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.PageCacheRule.config;

	public abstract class GBPTreeITBase<KEY, VALUE>
	{
		private bool InstanceFieldsInitialized = false;

		public GBPTreeITBase()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_directory = TestDirectory.testDirectory( this.GetType(), _fs.get() );
			Rules = outerRule( _fs ).around( _directory ).around( _pageCacheRule ).around( Random );
		}

		 private readonly DefaultFileSystemRule _fs = new DefaultFileSystemRule();
		 private TestDirectory _directory;
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 internal readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain rules = outerRule(fs).around(directory).around(pageCacheRule).around(random);
		 public RuleChain Rules;

		 private double _ratioToKeepInLeftOnSplit;
		 private TestLayout<KEY, VALUE> _layout;
		 private GBPTree<KEY, VALUE> _index;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _ratioToKeepInLeftOnSplit = Random.nextBoolean() ? InternalTreeLogic.DEFAULT_SPLIT_RATIO : Random.NextDouble();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private GBPTree<KEY,VALUE> createIndex() throws java.io.IOException
		 private GBPTree<KEY, VALUE> CreateIndex()
		 {
			  // some random padding
			  _layout = GetLayout( Random );
			  PageCache pageCache = _pageCacheRule.getPageCache( _fs.get(), config().withPageSize(512).withAccessChecks(true) );
			  return _index = ( new GBPTreeBuilder<KEY, VALUE>( pageCache, _directory.file( "index" ), _layout ) ).build();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Writer<KEY,VALUE> createWriter(GBPTree<KEY,VALUE> index) throws java.io.IOException
		 private Writer<KEY, VALUE> CreateWriter( GBPTree<KEY, VALUE> index )
		 {
			  return index.Writer( _ratioToKeepInLeftOnSplit );
		 }

		 internal abstract TestLayout<KEY, VALUE> GetLayout( RandomRule random );

		 internal abstract Type<KEY> KeyClass { get; }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStayCorrectAfterRandomModifications() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStayCorrectAfterRandomModifications()
		 {
			  // GIVEN
			  using ( GBPTree<KEY, VALUE> index = CreateIndex() )
			  {
					IComparer<KEY> keyComparator = _layout;
					IDictionary<KEY, VALUE> data = new SortedDictionary<KEY, VALUE>( keyComparator );
					int count = 100;
					int totalNumberOfRounds = 10;
					for ( int i = 0; i < count; i++ )
					{
						 data[RandomKey( Random.random() )] = RandomValue(Random.random());
					}

					// WHEN
					using ( Writer<KEY, VALUE> writer = CreateWriter( index ) )
					{
						 foreach ( KeyValuePair<KEY, VALUE> entry in data.SetOfKeyValuePairs() )
						 {
							  writer.Put( entry.Key, entry.Value );
						 }
					}

					for ( int round = 0; round < totalNumberOfRounds; round++ )
					{
						 // THEN
						 for ( int i = 0; i < count; i++ )
						 {
							  KEY first = RandomKey( Random.random() );
							  KEY second = RandomKey( Random.random() );
							  KEY from;
							  KEY to;
							  if ( _layout.keySeed( first ) < _layout.keySeed( second ) )
							  {
									from = first;
									to = second;
							  }
							  else
							  {
									from = second;
									to = first;
							  }
							  IDictionary<KEY, VALUE> expectedHits = expectedHits( data, from, to, keyComparator );
							  using ( RawCursor<Hit<KEY, VALUE>, IOException> result = index.Seek( from, to ) )
							  {
									while ( result.Next() )
									{
										 KEY key = result.get().key();
										 if ( expectedHits.Remove( key ) == null )
										 {
											  fail( "Unexpected hit " + key + " when searching for " + from + " - " + to );
										 }

										 assertTrue( keyComparator.Compare( key, from ) >= 0 );
										 if ( keyComparator.Compare( from, to ) != 0 )
										 {
											  assertTrue( keyComparator.Compare( key, to ) < 0 );
										 }
									}
									if ( expectedHits.Count > 0 )
									{
										 fail( "There were results which were expected to be returned, but weren't:" + expectedHits + " when searching range " + from + " - " + to );
									}
							  }
						 }

						 index.Checkpoint( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
						 RandomlyModifyIndex( index, data, Random.random(), (double) round / totalNumberOfRounds );
					}

					// and finally
					index.ConsistencyCheck();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleRemoveEntireTree() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleRemoveEntireTree()
		 {
			  // given
			  using ( GBPTree<KEY, VALUE> index = CreateIndex() )
			  {
					int numberOfNodes = 200_000;
					using ( Writer<KEY, VALUE> writer = CreateWriter( index ) )
					{
						 for ( int i = 0; i < numberOfNodes; i++ )
						 {
							  writer.Put( Key( i ), Value( i ) );
						 }
					}

					// when
					BitArray removed = new BitArray();
					using ( Writer<KEY, VALUE> writer = CreateWriter( index ) )
					{
						 for ( int i = 0; i < numberOfNodes - numberOfNodes / 10; i++ )
						 {
							  int candidate;
							  do
							  {
									candidate = Random.Next( max( 1, Random.Next( numberOfNodes ) ) );
							  } while ( removed.Get( candidate ) );
							  removed.Set( candidate, true );

							  writer.Remove( Key( candidate ) );
						 }
					}

					int next = 0;
					using ( Writer<KEY, VALUE> writer = CreateWriter( index ) )
					{
						 for ( int i = 0; i < numberOfNodes / 10; i++ )
						 {
							  next = removed.nextClearBit( next );
							  removed.Set( next, true );
							  writer.Remove( Key( next ) );
						 }
					}

					// then
					using ( RawCursor<Hit<KEY, VALUE>, IOException> seek = index.Seek( Key( 0 ), Key( numberOfNodes ) ) )
					{
						 assertFalse( seek.Next() );
					}

					// and finally
					index.ConsistencyCheck();
			  }
		 }

		 // Timeout because test verify no infinite loop
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 10_000L) public void shouldHandleDescendingWithEmptyRange() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleDescendingWithEmptyRange()
		 {
			  long[] seeds = new long[]{ 0, 1, 4 };
			  using ( GBPTree<KEY, VALUE> index = CreateIndex() )
			  {
					// Write
					using ( Writer<KEY, VALUE> writer = CreateWriter( index ) )
					{
						 foreach ( long seed in seeds )
						 {
							  KEY key = _layout.key( seed );
							  VALUE value = _layout.value( 0 );
							  writer.Put( key, value );
						 }
					}

					KEY from = _layout.key( 3 );
					KEY to = _layout.key( 1 );
					using ( RawCursor<Hit<KEY, VALUE>, IOException> seek = index.Seek( from, to ) )
					{
						 assertFalse( seek.Next() );
					}
					index.Checkpoint( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void randomlyModifyIndex(GBPTree<KEY,VALUE> index, java.util.Map<KEY,VALUE> data, java.util.Random random, double removeProbability) throws java.io.IOException
		 private void RandomlyModifyIndex( GBPTree<KEY, VALUE> index, IDictionary<KEY, VALUE> data, Random random, double removeProbability )
		 {
			  int changeCount = random.Next( 10 ) + 10;
			  using ( Writer<KEY, VALUE> writer = CreateWriter( index ) )
			  {
					for ( int i = 0; i < changeCount; i++ )
					{
						 if ( random.NextDouble() < removeProbability && data.Count > 0 )
						 { // remove
							  KEY key = RandomKey( data, random );
							  VALUE value = data.Remove( key );
							  VALUE removedValue = writer.Remove( key );
							  AssertEqualsValue( value, removedValue );
						 }
						 else
						 { // put
							  KEY key = RandomKey( random );
							  VALUE value = RandomValue( random );
							  writer.Put( key, value );
							  data[key] = value;
						 }
					}
			  }
		 }

		 private IDictionary<KEY, VALUE> ExpectedHits( IDictionary<KEY, VALUE> data, KEY from, KEY to, IComparer<KEY> comparator )
		 {
			  IDictionary<KEY, VALUE> hits = new SortedDictionary<KEY, VALUE>( comparator );
			  foreach ( KeyValuePair<KEY, VALUE> candidate in data.SetOfKeyValuePairs() )
			  {
					if ( comparator.Compare( from, to ) == 0 && comparator.Compare( candidate.Key, from ) == 0 )
					{
						 hits[candidate.Key] = candidate.Value;
					}
					else if ( comparator.Compare( candidate.Key, from ) >= 0 && comparator.Compare( candidate.Key, to ) < 0 )
					{
						 hits[candidate.Key] = candidate.Value;
					}
			  }
			  return hits;
		 }

		 private KEY RandomKey( IDictionary<KEY, VALUE> data, Random random )
		 {
			  //noinspection unchecked
			  KEY[] keys = data.Keys.toArray( ( KEY[] ) Array.CreateInstance( KeyClass, data.Count ) );
			  return keys[random.Next( keys.Length )];
		 }

		 private KEY RandomKey( Random random )
		 {
			  return Key( random.Next( 1_000 ) );
		 }

		 private VALUE RandomValue( Random random )
		 {
			  return Value( random.Next( 1_000 ) );
		 }

		 private VALUE Value( long seed )
		 {
			  return _layout.value( seed );
		 }

		 private KEY Key( long seed )
		 {
			  return _layout.key( seed );
		 }

		 private void AssertEqualsValue( VALUE expected, VALUE actual )
		 {
			  assertEquals( string.Format( "expected equal, expected={0}, actual={1}", expected.ToString(), actual.ToString() ), 0, _layout.compareValue(expected, actual) );
		 }

		 // KEEP even if unused
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private void printTree() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private void PrintTree()
		 {
			  _index.printTree( false, false, false, false, false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private void printNode(@SuppressWarnings("SameParameterValue") int id) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private void PrintNode( int id )
		 {
			  _index.printNode( id );
		 }
	}

}