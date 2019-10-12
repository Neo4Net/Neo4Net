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
namespace Neo4Net.Index.@internal.gbptree
{
	using MutableLong = org.apache.commons.lang3.mutable.MutableLong;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using Neo4Net.Cursors;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using FormatCompatibilityVerifier = Neo4Net.Test.FormatCompatibilityVerifier;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.SimpleLongLayout.longLayout;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class GBPTreeFormatTest extends org.neo4j.test.FormatCompatibilityVerifier
	public class GBPTreeFormatTest : FormatCompatibilityVerifier
	{
		private bool InstanceFieldsInitialized = false;

		public GBPTreeFormatTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_initialKeys = _initialKeys();
			_keysToAdd = _keysToAdd();
			Chain = RuleChain.outerRule( _random ).around( _pageCacheRule );
		}

		 private const string STORE = "store";
		 private const int INITIAL_KEY_COUNT = 10_000;
		 private const string CURRENT_FIXED_SIZE_FORMAT_ZIP = "current-format.zip";
		 private const string CURRENT_DYNAMIC_SIZE_FORMAT_ZIP = "current-dynamic-format.zip";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters public static java.util.List<Object[]> data()
		 public static IList<object[]> Data()
		 {
			  return new IList<object[]>
			  {
				  new object[] { longLayout().withFixedSize(true).build(), CURRENT_FIXED_SIZE_FORMAT_ZIP },
				  new object[] { longLayout().withFixedSize(false).build(), CURRENT_DYNAMIC_SIZE_FORMAT_ZIP }
			  };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameter public SimpleLongLayout layout;
		 public SimpleLongLayout Layout;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 [Parameter(1)]
		 public string ZipNameConflict;

		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private readonly RandomRule _random = new RandomRule();
		 private IList<long> _initialKeys;
		 private IList<long> _keysToAdd;
		 private IList<long> _allKeys;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _allKeys = new List<long>();
			  ( ( IList<long> )_allKeys ).AddRange( _initialKeys );
			  ( ( IList<long> )_allKeys ).AddRange( _keysToAdd );
			  _allKeys.sort( long?.compare );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain chain = org.junit.rules.RuleChain.outerRule(random).around(pageCacheRule);
		 public RuleChain Chain;

		 protected internal override string ZipName()
		 {
			  return ZipNameConflict;
		 }

		 protected internal override string StoreFileName()
		 {
			  return STORE;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void createStoreFile(java.io.File storeFile) throws java.io.IOException
		 protected internal override void CreateStoreFile( File storeFile )
		 {
			  IList<long> initialKeys = initialKeys();
			  PageCache pageCache = _pageCacheRule.getPageCache( GlobalFs.get() );
			  using ( GBPTree<MutableLong, MutableLong> tree = ( new GBPTreeBuilder<MutableLong, MutableLong>( pageCache, storeFile, Layout ) ).build() )
			  {
					using ( Writer<MutableLong, MutableLong> writer = tree.Writer() )
					{
						 foreach ( long? key in initialKeys )
						 {
							  Put( writer, key.Value );
						 }
					}
					tree.Checkpoint( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
			  }
		 }

		 /// <summary>
		 /// Throws <seealso cref="FormatViolationException"/> if format has changed.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("EmptyTryBlock") @Override protected void verifyFormat(java.io.File storeFile) throws java.io.IOException, FormatViolationException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 protected internal override void VerifyFormat( File storeFile )
		 {
			  PageCache pageCache = _pageCacheRule.getPageCache( GlobalFs.get() );
			  try
			  {
					  using ( GBPTree<MutableLong, MutableLong> ignored = ( new GBPTreeBuilder<MutableLong, MutableLong>( pageCache, storeFile, Layout ) ).build() )
					  {
					  }
			  }
			  catch ( MetadataMismatchException e )
			  {
					throw new FormatViolationException( this, e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyContent(java.io.File storeFile) throws java.io.IOException
		 public override void VerifyContent( File storeFile )
		 {
			  PageCache pageCache = _pageCacheRule.getPageCache( GlobalFs.get() );
			  using ( GBPTree<MutableLong, MutableLong> tree = ( new GBPTreeBuilder<MutableLong, MutableLong>( pageCache, storeFile, Layout ) ).build() )
			  {
					{
						 // WHEN reading from the tree
						 // THEN initial keys should be there
						 tree.ConsistencyCheck();
						 using ( RawCursor<Hit<MutableLong, MutableLong>, IOException> cursor = tree.Seek( Layout.key( 0 ), Layout.key( long.MaxValue ) ) )
						 {
							  foreach ( long? expectedKey in _initialKeys )
							  {
									AssertHit( cursor, expectedKey );
							  }
							  assertFalse( cursor.Next() );
						 }
					}

					{
						 // WHEN writing more to the tree
						 // THEN we should not see any format conflicts
						 using ( Writer<MutableLong, MutableLong> writer = tree.Writer() )
						 {
							  while ( _keysToAdd.Count > 0 )
							  {
									int next = _random.Next( _keysToAdd.Count );
									Put( writer, _keysToAdd[next] );
									_keysToAdd.RemoveAt( next );
							  }
						 }
					}

					{
						 // WHEN reading from the tree again
						 // THEN all keys including newly added should be there
						 tree.ConsistencyCheck();
						 using ( RawCursor<Hit<MutableLong, MutableLong>, IOException> cursor = tree.Seek( Layout.key( 0 ), Layout.key( 2 * INITIAL_KEY_COUNT ) ) )
						 {
							  foreach ( long? expectedKey in _allKeys )
							  {
									AssertHit( cursor, expectedKey );
							  }
							  assertFalse( cursor.Next() );
						 }
					}

					{
						 // WHEN randomly removing half of tree content
						 // THEN we should not see any format conflicts
						 using ( Writer<MutableLong, MutableLong> writer = tree.Writer() )
						 {
							  int size = _allKeys.Count;
							  while ( _allKeys.Count > size / 2 )
							  {
									int next = _random.Next( _allKeys.Count );
									MutableLong key = Layout.key( _allKeys[next] );
									writer.Remove( key );
									_allKeys.RemoveAt( next );
							  }
						 }
					}

					{
						 // WHEN reading from the tree after remove
						 // THEN we should see everything that is left in the tree
						 tree.ConsistencyCheck();
						 using ( RawCursor<Hit<MutableLong, MutableLong>, IOException> cursor = tree.Seek( Layout.key( 0 ), Layout.key( 2 * INITIAL_KEY_COUNT ) ) )
						 {
							  foreach ( long? expectedKey in _allKeys )
							  {
									AssertHit( cursor, expectedKey );
							  }
							  assertFalse( cursor.Next() );
						 }
					}
			  }
		 }

		 private static long Value( long key )
		 {
			  return key * 2;
		 }

		 private static IList<long> InitialKeys()
		 {
			  IList<long> initialKeys = new List<long>();
			  for ( long i = 0, key = 0; i < INITIAL_KEY_COUNT; i++, key += 2 )
			  {
					initialKeys.Add( key );
			  }
			  return initialKeys;
		 }

		 private static IList<long> KeysToAdd()
		 {
			  IList<long> keysToAdd = new List<long>();
			  for ( long i = 0, key = 1; i < INITIAL_KEY_COUNT; i++, key += 2 )
			  {
					keysToAdd.Add( key );
			  }
			  return keysToAdd;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertHit(org.neo4j.cursor.RawCursor<Hit<org.apache.commons.lang3.mutable.MutableLong,org.apache.commons.lang3.mutable.MutableLong>,java.io.IOException> cursor, System.Nullable<long> expectedKey) throws java.io.IOException
		 private static void AssertHit( RawCursor<Hit<MutableLong, MutableLong>, IOException> cursor, long? expectedKey )
		 {
			  assertTrue( "Had no next when expecting key " + expectedKey, cursor.Next() );
			  Hit<MutableLong, MutableLong> hit = cursor.get();
			  assertEquals( expectedKey.Value, hit.Key().longValue() );
			  assertEquals( Value( expectedKey.Value ), hit.Value().longValue() );
		 }

		 private void Put( Writer<MutableLong, MutableLong> writer, long key )
		 {
			  MutableLong insertKey = Layout.key( key );
			  MutableLong insertValue = Layout.value( Value( key ) );
			  writer.Put( insertKey, insertValue );
		 }
	}

}