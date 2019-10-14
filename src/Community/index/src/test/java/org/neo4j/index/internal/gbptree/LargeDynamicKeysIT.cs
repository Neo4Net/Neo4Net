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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.Cursors;
	using Neo4Net.Helpers.Collections;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using UTF8 = Neo4Net.Strings.UTF8;
	using PageCacheAndDependenciesRule = Neo4Net.Test.rule.PageCacheAndDependenciesRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.TreeNodeDynamicSize.keyValueSizeCapFromPageSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PageCache_Fields.PAGE_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.PageCacheRule.config;

	public class LargeDynamicKeysIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.PageCacheAndDependenciesRule storage = new org.neo4j.test.rule.PageCacheAndDependenciesRule().with(new org.neo4j.test.rule.fs.DefaultFileSystemRule());
		 public readonly PageCacheAndDependenciesRule Storage = new PageCacheAndDependenciesRule().with(new DefaultFileSystemRule());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustStayCorrectWhenInsertingValuesOfIncreasingLength() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustStayCorrectWhenInsertingValuesOfIncreasingLength()
		 {
			  Layout<RawBytes, RawBytes> layout = layout();
			  using ( GBPTree<RawBytes, RawBytes> index = CreateIndex( layout ), Writer<RawBytes, RawBytes> writer = index.Writer() )
			  {
					RawBytes emptyValue = layout.NewValue();
					emptyValue.Bytes = new sbyte[0];
					for ( int keySize = 1; keySize < index.KeyValueSizeCap(); keySize++ )
					{
						 RawBytes key = layout.NewKey();
						 key.Bytes = new sbyte[keySize];
						 writer.Put( key, emptyValue );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteAndReadSmallToSemiLargeEntries() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteAndReadSmallToSemiLargeEntries()
		 {
			  int keyValueSizeCap = keyValueSizeCapFromPageSize( PAGE_SIZE );
			  int minValueSize = 0;
			  int maxValueSize = Random.Next( 200 );
			  int minKeySize = 4;
			  int maxKeySize = keyValueSizeCap / 5;
			  ShouldWriteAndReadEntriesOfRandomSizes( minKeySize, maxKeySize, minValueSize, maxValueSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteAndReadSmallToLargeEntries() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteAndReadSmallToLargeEntries()
		 {
			  int keyValueSizeCap = keyValueSizeCapFromPageSize( PAGE_SIZE );
			  int minValueSize = 0;
			  int maxValueSize = Random.Next( 200 );
			  int minKeySize = 4;
			  int maxKeySize = keyValueSizeCap - maxValueSize;
			  ShouldWriteAndReadEntriesOfRandomSizes( minKeySize, maxKeySize, minValueSize, maxValueSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteAndReadSemiLargeToLargeEntries() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteAndReadSemiLargeToLargeEntries()
		 {
			  int keyValueSizeCap = keyValueSizeCapFromPageSize( PAGE_SIZE );
			  int minValueSize = 0;
			  int maxValueSize = Random.Next( 200 );
			  int minKeySize = keyValueSizeCap / 5;
			  int maxKeySize = keyValueSizeCap - maxValueSize;
			  ShouldWriteAndReadEntriesOfRandomSizes( minKeySize, maxKeySize, minValueSize, maxValueSize );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldWriteAndReadEntriesOfRandomSizes(int minKeySize, int maxKeySize, int minValueSize, int maxValueSize) throws java.io.IOException
		 private void ShouldWriteAndReadEntriesOfRandomSizes( int minKeySize, int maxKeySize, int minValueSize, int maxValueSize )
		 {
			  // given
			  using ( GBPTree<RawBytes, RawBytes> tree = CreateIndex( Layout() ) )
			  {
					// when
					ISet<string> generatedStrings = new HashSet<string>();
					IList<Pair<RawBytes, RawBytes>> entries = new List<Pair<RawBytes, RawBytes>>();
					using ( Writer<RawBytes, RawBytes> writer = tree.Writer() )
					{
						 for ( int i = 0; i < 1_000; i++ )
						 {
							  // value, based on i
							  RawBytes value = new RawBytes();
							  value.Bytes = new sbyte[Random.Next( minValueSize, maxValueSize )];
							  Random.NextBytes( value.Bytes );

							  // key, randomly generated
							  string @string;
							  do
							  {
									@string = Random.nextAlphaNumericString( minKeySize, maxKeySize );
							  } while ( !generatedStrings.Add( @string ) );
							  RawBytes key = new RawBytes();
							  key.Bytes = UTF8.encode( @string );
							  entries.Add( Pair.of( key, value ) );

							  // write
							  writer.Put( key, value );
						 }
					}

					// then
					foreach ( Pair<RawBytes, RawBytes> entry in entries )
					{
						 using ( RawCursor<Hit<RawBytes, RawBytes>, IOException> seek = tree.Seek( entry.First(), entry.First() ) )
						 {
							  assertTrue( seek.Next() );
							  assertArrayEquals( entry.First().Bytes, seek.get().key().bytes );
							  assertArrayEquals( entry.Other().Bytes, seek.get().value().bytes );
							  assertFalse( seek.Next() );
						 }
					}
			  }
		 }

		 private SimpleByteArrayLayout Layout()
		 {
			  return new SimpleByteArrayLayout( false );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private GBPTree<RawBytes,RawBytes> createIndex(Layout<RawBytes,RawBytes> layout) throws java.io.IOException
		 private GBPTree<RawBytes, RawBytes> CreateIndex( Layout<RawBytes, RawBytes> layout )
		 {
			  // some random padding
			  PageCache pageCache = Storage.pageCacheRule().getPageCache(Storage.fileSystem(), config().withAccessChecks(true));
			  return ( new GBPTreeBuilder<RawBytes, RawBytes>( pageCache, Storage.directory().file("index"), layout ) ).build();
		 }
	}

}