﻿using System;

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
namespace Org.Neo4j.Kernel.impl.index.labelscan
{
	using Matcher = org.hamcrest.Matcher;
	using Matchers = org.hamcrest.Matchers;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using LabelScanStoreTest = Org.Neo4j.Kernel.api.impl.labelscan.LabelScanStoreTest;
	using LabelScanStore = Org.Neo4j.Kernel.api.labelscan.LabelScanStore;
	using FullStoreChangeStream = Org.Neo4j.Kernel.Impl.Api.scan.FullStoreChangeStream;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Matchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.scan.FullStoreChangeStream_Fields.EMPTY;

	public class NativeLabelScanStoreTest : LabelScanStoreTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule();
		 public PageCacheRule PageCacheRule = new PageCacheRule();

		 protected internal override LabelScanStore CreateLabelScanStore( FileSystemAbstraction fileSystemAbstraction, DatabaseLayout databaseLayout, FullStoreChangeStream fullStoreChangeStream, bool usePersistentStore, bool readOnly, Org.Neo4j.Kernel.api.labelscan.LabelScanStore_Monitor monitor )
		 {
			  Monitors monitors = new Monitors();
			  monitors.AddMonitorListener( monitor );
			  return GetLabelScanStore( fileSystemAbstraction, databaseLayout, fullStoreChangeStream, readOnly, monitors );
		 }

		 private LabelScanStore GetLabelScanStore( FileSystemAbstraction fileSystemAbstraction, DatabaseLayout databaseLayout, FullStoreChangeStream fullStoreChangeStream, bool readOnly, Monitors monitors )
		 {
			  PageCache pageCache = PageCacheRule.getPageCache( fileSystemAbstraction );
			  return new NativeLabelScanStore( pageCache, databaseLayout, fileSystemAbstraction, fullStoreChangeStream, readOnly, monitors, RecoveryCleanupWorkCollector.immediate() );
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: protected org.hamcrest.Matcher<Iterable<? super java.io.File>> hasLabelScanStore()
		 protected internal override Matcher<System.Collections.IEnumerable> HasLabelScanStore()
		 {
			  return Matchers.hasItem( Matchers.equalTo( TestDirectory.databaseLayout().labelScanStore() ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void corruptIndex(org.neo4j.io.fs.FileSystemAbstraction fileSystem, org.neo4j.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
		 protected internal override void CorruptIndex( FileSystemAbstraction fileSystem, DatabaseLayout databaseLayout )
		 {
			  File lssFile = databaseLayout.LabelScanStore();
			  ScrambleFile( lssFile );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shutdownNonInitialisedNativeScanStoreWithoutException() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShutdownNonInitialisedNativeScanStoreWithoutException()
		 {
			  string expectedMessage = "Expected exception message";
			  Monitors monitors = mock( typeof( Monitors ) );
			  when( monitors.NewMonitor( typeof( Org.Neo4j.Kernel.api.labelscan.LabelScanStore_Monitor ) ) ).thenReturn( Org.Neo4j.Kernel.api.labelscan.LabelScanStore_Monitor_Fields.Empty );
			  doThrow( new Exception( expectedMessage ) ).when( monitors ).addMonitorListener( any() );

			  LabelScanStore scanStore = GetLabelScanStore( FileSystemRule.get(), TestDirectory.databaseLayout(), EMPTY, true, monitors );
			  try
			  {
					scanStore.Init();
					fail( "Initialisation of store should fail." );
			  }
			  catch ( Exception e )
			  {
					assertEquals( expectedMessage, e.Message );
			  }

			  scanStore.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartPopulationAgainIfNotCompletedFirstTime()
		 public virtual void ShouldStartPopulationAgainIfNotCompletedFirstTime()
		 {
			  // given
			  // label scan store init but no start
			  LifeSupport life = new LifeSupport();
			  TrackingMonitor monitor = new TrackingMonitor();
			  life.Add( CreateLabelScanStore( FileSystemRule.get(), TestDirectory.databaseLayout(), EMPTY, true, false, monitor ) );
			  life.Init();
			  assertTrue( monitor.NoIndexCalled );
			  monitor.Reset();
			  life.Shutdown();

			  // when
			  // starting label scan store again
			  life = new LifeSupport();
			  life.Add( CreateLabelScanStore( FileSystemRule.get(), TestDirectory.databaseLayout(), EMPTY, true, false, monitor ) );
			  life.Init();

			  // then
			  // label scan store should recognize it still needs to be rebuilt
			  assertTrue( monitor.CorruptedIndex );
			  life.Start();
			  assertTrue( monitor.RebuildingCalled );
			  assertTrue( monitor.RebuiltCalled );
			  life.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRestartPopulationIfIndexFileWasNeverFullyInitialized() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRestartPopulationIfIndexFileWasNeverFullyInitialized()
		 {
			  // given
			  File labelScanStoreFile = NativeLabelScanStore.GetLabelScanStoreFile( TestDirectory.databaseLayout() );
			  FileSystemRule.create( labelScanStoreFile ).close();
			  TrackingMonitor monitor = new TrackingMonitor();
			  LifeSupport life = new LifeSupport();

			  // when
			  life.Add( CreateLabelScanStore( FileSystemRule.get(), TestDirectory.databaseLayout(), EMPTY, true, false, monitor ) );
			  life.Start();

			  // then
			  assertTrue( monitor.CorruptedIndex );
			  assertTrue( monitor.RebuildingCalled );
			  life.Shutdown();
		 }
	}

}