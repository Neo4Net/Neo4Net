using System;
using System.Collections;

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
namespace Neo4Net.Kernel.impl.index.labelscan
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using RecoveryCleanupWorkCollector = Neo4Net.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using LabelScanWriter = Neo4Net.Kernel.api.labelscan.LabelScanWriter;
	using NodeLabelUpdate = Neo4Net.Kernel.api.labelscan.NodeLabelUpdate;
	using FullStoreChangeStream = Neo4Net.Kernel.Impl.Api.scan.FullStoreChangeStream;
	using LifeRule = Neo4Net.Kernel.Lifecycle.LifeRule;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LabelScanReader = Neo4Net.Storageengine.Api.schema.LabelScanReader;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.rules.RuleChain.outerRule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.asArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.labelscan.NodeLabelUpdate.labelChanges;

	public class NativeLabelScanStoreIT
	{
		private bool InstanceFieldsInitialized = false;

		public NativeLabelScanStoreIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Rules = outerRule( _fileSystem ).around( _directory ).around( _pageCacheRule ).around( _life ).around( _random );
		}

		 private readonly TestDirectory _directory = TestDirectory.testDirectory();
		 private readonly DefaultFileSystemRule _fileSystem = new DefaultFileSystemRule();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private readonly LifeRule _life = new LifeRule( true );
		 private readonly RandomRule _random = new RandomRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain rules = outerRule(fileSystem).around(directory).around(pageCacheRule).around(life).around(random);
		 public RuleChain Rules;
		 private NativeLabelScanStore _store;

		 private const int NODE_COUNT = 10_000;
		 private const int LABEL_COUNT = 12;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystem );
			  _store = _life.add( new NativeLabelScanStore( pageCache, _directory.databaseLayout(), _fileSystem, Neo4Net.Kernel.Impl.Api.scan.FullStoreChangeStream_Fields.Empty, false, new Monitors(), RecoveryCleanupWorkCollector.immediate(), Math.Min(pageCache.PageSize(), 256 << _random.Next(5)) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRandomlyTestIt() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRandomlyTestIt()
		 {
			  // GIVEN
			  long[] expected = new long[NODE_COUNT];
			  RandomModifications( expected, NODE_COUNT );

			  // WHEN/THEN
			  for ( int i = 0; i < 100; i++ )
			  {
					VerifyReads( expected );
					RandomModifications( expected, NODE_COUNT / 10 );
			  }
		 }

		 private void VerifyReads( long[] expected )
		 {
			  using ( LabelScanReader reader = _store.newReader() )
			  {
					for ( int i = 0; i < LABEL_COUNT; i++ )
					{
						 long[] actualNodes = asArray( reader.NodesWithLabel( i ) );
						 long[] expectedNodes = NodesWithLabel( expected, i );
						 assertArrayEquals( expectedNodes, actualNodes );
					}
			  }
		 }

		 public static long[] NodesWithLabel( long[] expected, int labelId )
		 {
			  int mask = 1 << labelId;
			  int count = 0;
			  foreach ( long labels in expected )
			  {
					if ( ( labels & mask ) != 0 )
					{
						 count++;
					}
			  }

			  long[] result = new long[count];
			  int cursor = 0;
			  for ( int nodeId = 0; nodeId < expected.Length; nodeId++ )
			  {
					long labels = expected[nodeId];
					if ( ( labels & mask ) != 0 )
					{
						 result[cursor++] = nodeId;
					}
			  }
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void randomModifications(long[] expected, int count) throws java.io.IOException
		 private void RandomModifications( long[] expected, int count )
		 {
			  BitArray editedNodes = new BitArray();
			  using ( LabelScanWriter writer = _store.newWriter() )
			  {
					for ( int i = 0; i < count; i++ )
					{
						 int nodeId = _random.Next( NODE_COUNT );
						 if ( editedNodes.Get( nodeId ) )
						 {
							  i--;
							  continue;
						 }

						 int changeSize = _random.Next( 3 ) + 1;
						 long labels = expected[nodeId];
						 long[] labelsBefore = GetLabels( labels );
						 for ( int j = 0; j < changeSize; j++ )
						 {
							  labels = FlipRandom( labels, LABEL_COUNT, _random.random() );
						 }
						 long[] labelsAfter = GetLabels( labels );
						 editedNodes.Set( nodeId, true );

						 NodeLabelUpdate labelChanges = labelChanges( nodeId, labelsBefore, labelsAfter );
						 writer.Write( labelChanges );
						 expected[nodeId] = labels;
					}
			  }
		 }

		 public static long FlipRandom( long existingLabels, int highLabelId, Random random )
		 {
			  return existingLabels ^ ( 1 << random.Next( highLabelId ) );
		 }

		 public static long[] GetLabels( long bits )
		 {
			  long[] result = new long[Long.bitCount( bits )];
			  for ( int labelId = 0, c = 0; labelId < LABEL_COUNT; labelId++ )
			  {
					int mask = 1 << labelId;
					if ( ( bits & mask ) != 0 )
					{
						 result[c++] = labelId;
					}
			  }
			  return result;
		 }
	}

}