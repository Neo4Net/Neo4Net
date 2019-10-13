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
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using RuleChain = org.junit.rules.RuleChain;


	using Neo4Net.Cursors;
	using Neo4Net.Index.@internal.gbptree;
	using Neo4Net.Index.@internal.gbptree;
	using Neo4Net.Index.@internal.gbptree;
	using Neo4Net.Index.@internal.gbptree;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using Neo4Net.Kernel.Api.Index;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.rules.RuleChain.outerRule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexDirectoryStructure.directoriesByProvider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexKey.Inclusion.NEUTRAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.PageCacheRule.config;

	public abstract class NativeIndexTestUtil<KEY, VALUE> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		private bool InstanceFieldsInitialized = false;

		public NativeIndexTestUtil()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_directory = TestDirectory.testDirectory( this.GetType(), Fs.get() );
			Rules = outerRule( Random ).around( Fs ).around( _directory ).around( _pageCacheRule );
		}

		 internal const long NON_EXISTENT_ENTITY_ID = 1_000_000_000;

		 internal readonly DefaultFileSystemRule Fs = new DefaultFileSystemRule();
		 private TestDirectory _directory;
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule( config().withAccessChecks(true) );
		 protected internal readonly RandomRule Random = new RandomRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain rules = outerRule(random).around(fs).around(directory).around(pageCacheRule);
		 public RuleChain Rules;

		 internal StoreIndexDescriptor IndexDescriptor;
		 internal ValueCreatorUtil<KEY, VALUE> ValueCreatorUtil;
		 internal IndexLayout<KEY, VALUE> Layout;
		 internal IndexDirectoryStructure IndexDirectoryStructure;
		 private File _indexFile;
		 internal PageCache PageCache;
		 internal IndexProvider.Monitor Monitor = IndexProvider.Monitor_Fields.EMPTY;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  ValueCreatorUtil = CreateValueCreatorUtil();
			  IndexDescriptor = ValueCreatorUtil.indexDescriptor();
			  Layout = CreateLayout();
			  IndexDirectoryStructure = directoriesByProvider( _directory.directory( "root" ) ).forProvider( IndexDescriptor.providerDescriptor() );
			  _indexFile = IndexDirectoryStructure.directoryForIndex( IndexDescriptor.Id );
			  Fs.mkdirs( _indexFile.ParentFile );
			  PageCache = _pageCacheRule.getPageCache( Fs );
		 }

		 public virtual File IndexFile
		 {
			 get
			 {
				  return _indexFile;
			 }
		 }

		 internal abstract ValueCreatorUtil<KEY, VALUE> CreateValueCreatorUtil();

		 internal abstract IndexLayout<KEY, VALUE> CreateLayout();

		 private void CopyValue( VALUE value, VALUE intoValue )
		 {
			  ValueCreatorUtil.copyValue( value, intoValue );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void verifyUpdates(org.neo4j.kernel.api.index.IndexEntryUpdate<org.neo4j.storageengine.api.schema.IndexDescriptor>[] updates) throws java.io.IOException
		 internal virtual void VerifyUpdates( IndexEntryUpdate<IndexDescriptor>[] updates )
		 {
			  Hit<KEY, VALUE>[] expectedHits = ConvertToHits( updates, Layout );
			  IList<Hit<KEY, VALUE>> actualHits = new List<Hit<KEY, VALUE>>();
			  using ( GBPTree<KEY, VALUE> tree = Tree, RawCursor<Hit<KEY, VALUE>, IOException> scan = scan( tree ) )
			  {
					while ( scan.Next() )
					{
						 actualHits.Add( DeepCopy( scan.get() ) );
					}
			  }

			  IComparer<Hit<KEY, VALUE>> hitComparator = ( h1, h2 ) =>
			  {
				int keyCompare = Layout.compare( h1.key(), h2.key() );
				if ( keyCompare == 0 )
				{
					 return ValueCreatorUtil.compareIndexedPropertyValue( h1.key(), h2.key() );
				}
				else
				{
					 return keyCompare;
				}
			  };
			  AssertSameHits( expectedHits, actualHits.ToArray(), hitComparator );
		 }

		 internal virtual GBPTree<KEY, VALUE> Tree
		 {
			 get
			 {
				  return ( new GBPTreeBuilder<KEY, VALUE>( PageCache, IndexFile, Layout ) ).build();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.cursor.RawCursor<org.neo4j.index.internal.gbptree.Hit<KEY,VALUE>, java.io.IOException> scan(org.neo4j.index.internal.gbptree.GBPTree<KEY,VALUE> tree) throws java.io.IOException
		 private RawCursor<Hit<KEY, VALUE>, IOException> Scan( GBPTree<KEY, VALUE> tree )
		 {
			  KEY lowest = Layout.newKey();
			  lowest.initialize( long.MinValue );
			  lowest.initValueAsLowest( 0, ValueGroup.UNKNOWN );
			  KEY highest = Layout.newKey();
			  highest.initialize( long.MaxValue );
			  highest.initValueAsHighest( 0, ValueGroup.UNKNOWN );
			  return tree.Seek( lowest, highest );
		 }

		 private void AssertSameHits( Hit<KEY, VALUE>[] expectedHits, Hit<KEY, VALUE>[] actualHits, IComparer<Hit<KEY, VALUE>> comparator )
		 {
			  Arrays.sort( expectedHits, comparator );
			  Arrays.sort( actualHits, comparator );
			  assertEquals( format( "Array length differ%nExpected:%d, Actual:%d", expectedHits.Length, actualHits.Length ), expectedHits.Length, actualHits.Length );

			  for ( int i = 0; i < expectedHits.Length; i++ )
			  {
					Hit<KEY, VALUE> expected = expectedHits[i];
					Hit<KEY, VALUE> actual = actualHits[i];
					assertEquals( "Hits differ on item number " + i + ". Expected " + expected + " but was " + actual, 0, comparator.Compare( expected, actual ) );
			  }
		 }

		 private Hit<KEY, VALUE> DeepCopy( Hit<KEY, VALUE> from )
		 {
			  KEY intoKey = Layout.newKey();
			  VALUE intoValue = Layout.newValue();
			  Layout.copyKey( from.Key(), intoKey );
			  CopyValue( from.Value(), intoValue );
			  return new SimpleHit<KEY, VALUE>( intoKey, intoValue );
		 }

		 private Hit<KEY, VALUE>[] ConvertToHits( IndexEntryUpdate<IndexDescriptor>[] updates, Layout<KEY, VALUE> layout )
		 {
			  IList<Hit<KEY, VALUE>> hits = new List<Hit<KEY, VALUE>>( updates.Length );
			  foreach ( IndexEntryUpdate<IndexDescriptor> u in updates )
			  {
					KEY key = layout.NewKey();
					key.initialize( u.EntityId );
					for ( int i = 0; i < u.Values().Length; i++ )
					{
						 key.initFromValue( i, u.Values()[i], NEUTRAL );
					}
					VALUE value = layout.NewValue();
					value.From( u.Values() );
					hits.Add( Hit( key, value ) );
			  }
			  return hits.ToArray();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.index.internal.gbptree.Hit<KEY,VALUE> hit(final KEY key, final VALUE value)
		 private Hit<KEY, VALUE> Hit( KEY key, VALUE value )
		 {
			  return new SimpleHit<KEY, VALUE>( key, value );
		 }

		 internal virtual void AssertFilePresent()
		 {
			  assertTrue( Fs.fileExists( IndexFile ) );
		 }

		 internal virtual void AssertFileNotPresent()
		 {
			  assertFalse( Fs.fileExists( IndexFile ) );
		 }

		 // Useful when debugging
		 internal virtual long Seed
		 {
			 set
			 {
				  Random.Seed = value;
				  Random.reset();
			 }
		 }
	}

}