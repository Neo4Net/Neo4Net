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
namespace Neo4Net.Kernel.impl.index.labelscan
{
	using Matchers = org.hamcrest.Matchers;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using LabelScanStore = Neo4Net.Kernel.api.labelscan.LabelScanStore;
	using NodeLabelUpdate = Neo4Net.Kernel.api.labelscan.NodeLabelUpdate;
	using FullStoreChangeStream = Neo4Net.Kernel.Impl.Api.scan.FullStoreChangeStream;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using Neo4Net.Test.rule.fs;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.RecoveryCleanupWorkCollector.ignore;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.RecoveryCleanupWorkCollector.immediate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.api.scan.FullStoreChangeStream_Fields.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.api.scan.FullStoreChangeStream.asStream;

	public class NativeLabelScanStoreRebuildTest
	{
		private bool InstanceFieldsInitialized = false;

		public NativeLabelScanStoreRebuildTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _fileSystemRule ).around( _pageCacheRule ).around( _testDirectory );
		}

		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private readonly FileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private readonly TestDirectory _testDirectory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fileSystemRule).around(pageCacheRule).around(testDirectory);
		 public RuleChain RuleChain;

		 private static readonly FullStoreChangeStream _throwingStream = writer =>
		 {
		  throw new System.ArgumentException();
		 };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBeDirtyIfFailedDuringRebuild() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustBeDirtyIfFailedDuringRebuild()
		 {
			  // given
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystemRule.get() );
			  CreateDirtyIndex( pageCache );

			  // when
			  RecordingMonitor monitor = new RecordingMonitor();
			  Monitors monitors = new Monitors();
			  monitors.AddMonitorListener( monitor );

			  NativeLabelScanStore nativeLabelScanStore = new NativeLabelScanStore( pageCache, _testDirectory.databaseLayout(), _fileSystemRule.get(), EMPTY, false, monitors, immediate() );
			  nativeLabelScanStore.Init();
			  nativeLabelScanStore.Start();

			  // then
			  assertTrue( monitor.NotValid );
			  assertTrue( monitor.RebuildingConflict );
			  assertTrue( monitor.RebuiltConflict );
			  nativeLabelScanStore.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doNotRebuildIfOpenedInReadOnlyModeAndIndexIsNotClean() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DoNotRebuildIfOpenedInReadOnlyModeAndIndexIsNotClean()
		 {
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystemRule.get() );
			  CreateDirtyIndex( pageCache );

			  Monitors monitors = new Monitors();
			  RecordingMonitor monitor = new RecordingMonitor();
			  monitors.AddMonitorListener( monitor );

			  NativeLabelScanStore nativeLabelScanStore = new NativeLabelScanStore( pageCache, _testDirectory.databaseLayout(), _fileSystemRule.get(), EMPTY, true, monitors, ignore() );
			  nativeLabelScanStore.Init();
			  nativeLabelScanStore.Start();

			  assertTrue( monitor.NotValid );
			  assertFalse( monitor.RebuiltConflict );
			  assertFalse( monitor.RebuildingConflict );
			  nativeLabelScanStore.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void labelScanStoreIsDirtyWhenIndexIsNotClean() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LabelScanStoreIsDirtyWhenIndexIsNotClean()
		 {
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystemRule.get() );
			  CreateDirtyIndex( pageCache );

			  Monitors monitors = new Monitors();
			  RecordingMonitor monitor = new RecordingMonitor();
			  monitors.AddMonitorListener( monitor );

			  NativeLabelScanStore nativeLabelScanStore = new NativeLabelScanStore( pageCache, _testDirectory.databaseLayout(), _fileSystemRule.get(), EMPTY, true, monitors, ignore() );
			  nativeLabelScanStore.Init();
			  nativeLabelScanStore.Start();

			  assertTrue( nativeLabelScanStore.Dirty );
			  nativeLabelScanStore.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnUnsortedLabelsFromFullStoreChangeStream() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailOnUnsortedLabelsFromFullStoreChangeStream()
		 {
			  // given
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystemRule.get() );
			  IList<NodeLabelUpdate> existingData = new List<NodeLabelUpdate>();
			  existingData.Add( NodeLabelUpdate.labelChanges( 1, new long[0], new long[]{ 2, 1 } ) );
			  FullStoreChangeStream changeStream = asStream( existingData );
			  NativeLabelScanStore nativeLabelScanStore = null;
			  try
			  {
					nativeLabelScanStore = new NativeLabelScanStore( pageCache, _testDirectory.databaseLayout(), _fileSystemRule.get(), changeStream, false, new Monitors(), immediate() );
					nativeLabelScanStore.Init();

					// when
					nativeLabelScanStore.Start();
					fail( "Expected native label scan store to fail on " );
			  }
			  catch ( System.ArgumentException e )
			  {
					// then
					assertThat( e.Message, Matchers.containsString( "unsorted label" ) );
					assertThat( e.Message, Matchers.stringContainsInOrder( Iterables.asIterable( "2", "1" ) ) );
			  }
			  finally
			  {
					if ( nativeLabelScanStore != null )
					{
						 nativeLabelScanStore.Shutdown();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createDirtyIndex(org.Neo4Net.io.pagecache.PageCache pageCache) throws java.io.IOException
		 private void CreateDirtyIndex( PageCache pageCache )
		 {
			  NativeLabelScanStore nativeLabelScanStore = null;
			  try
			  {
					nativeLabelScanStore = new NativeLabelScanStore( pageCache, _testDirectory.databaseLayout(), _fileSystemRule.get(), _throwingStream, false, new Monitors(), immediate() );

					nativeLabelScanStore.Init();
					nativeLabelScanStore.Start();
			  }
			  catch ( System.ArgumentException )
			  {
					if ( nativeLabelScanStore != null )
					{
						 nativeLabelScanStore.Shutdown();
					}
			  }
		 }

		 private class RecordingMonitor : Neo4Net.Kernel.api.labelscan.LabelScanStore_Monitor_Adaptor
		 {
			  internal bool NotValid;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool RebuildingConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool RebuiltConflict;

			  public override void NotValidIndex()
			  {
					NotValid = true;
			  }

			  public override void Rebuilding()
			  {
					RebuildingConflict = true;
			  }

			  public override void Rebuilt( long roughNodeCount )
			  {
					RebuiltConflict = true;
			  }
		 }
	}

}