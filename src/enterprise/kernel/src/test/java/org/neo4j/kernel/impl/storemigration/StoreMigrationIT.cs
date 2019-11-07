﻿using System;
using System.Collections.Generic;
using System.IO;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.impl.storemigration
{
	using ClassRule = org.junit.ClassRule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using ConsistencyCheckService = Neo4Net.Consistency.ConsistencyCheckService;
	using ConsistencyCheckIncompleteException = Neo4Net.Consistency.checking.full.ConsistencyCheckIncompleteException;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using EnterpriseGraphDatabaseFactory = Neo4Net.GraphDb.factory.EnterpriseGraphDatabaseFactory;
	using GraphDatabaseFactory = Neo4Net.GraphDb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Service = Neo4Net.Helpers.Service;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using RecordFormatSelector = Neo4Net.Kernel.impl.store.format.RecordFormatSelector;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using StandardV2_3 = Neo4Net.Kernel.impl.store.format.standard.StandardV2_3;
	using StandardV3_0 = Neo4Net.Kernel.impl.store.format.standard.StandardV3_0;
	using StandardV3_2 = Neo4Net.Kernel.impl.store.format.standard.StandardV3_2;
	using StandardV3_4 = Neo4Net.Kernel.impl.store.format.standard.StandardV3_4;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogTailScanner = Neo4Net.Kernel.recovery.LogTailScanner;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class StoreMigrationIT
	public class StoreMigrationIT
	{
		 private static readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private static readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private static readonly TestDirectory _testDir = TestDirectory.testDirectory( _fileSystemRule );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fileSystemRule).around(pageCacheRule).around(testDir);
		 public static readonly RuleChain RuleChain = RuleChain.outerRule( _fileSystemRule ).around( _pageCacheRule ).around( _testDir );

		 public static readonly string CreateQuery = ReadQuery();

		 private static string ReadQuery()
		 {
			  Stream @in = typeof( StoreMigrationIT ).ClassLoader.getResourceAsStream( "store-migration-data.txt" );
			  string result = ( new StreamReader( @in ) ).lines().collect(Collectors.joining("\n"));
			  try
			  {
					@in.Close();
			  }
			  catch ( IOException e )
			  {
					Console.WriteLine( e.ToString() );
					Console.Write( e.StackTrace );
			  }
			  return result;
		 }

		 protected internal readonly RecordFormats From;
		 protected internal readonly RecordFormats To;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "Migrate: {0}->{1}") public static Iterable<Object[]> data() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static IEnumerable<object[]> Data()
		 {
			  FileSystemAbstraction fs = _fileSystemRule.get();
			  PageCache pageCache = _pageCacheRule.getPageCache( fs );
			  TestDirectory testDirectory = TestDirectory.testDirectory();
			  testDirectory.PrepareDirectory( typeof( StoreMigrationIT ), "migration" );
			  DatabaseLayout databaseLayout = testDirectory.DatabaseLayout();
			  StoreVersionCheck storeVersionCheck = new StoreVersionCheck( pageCache );
			  VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();
			  LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( databaseLayout.DatabaseDirectory(), fs ).withLogEntryReader(logEntryReader).build();
			  LogTailScanner tailScanner = new LogTailScanner( logFiles, logEntryReader, new Monitors() );
			  IList<object[]> data = new List<object[]>();
			  List<RecordFormats> recordFormats = new List<RecordFormats>();
			  RecordFormatSelector.allFormats().forEach(f => addIfNotThere(f, recordFormats));
			  foreach ( RecordFormats toFormat in recordFormats )
			  {
					UpgradableDatabase upgradableDatabase = new UpgradableDatabase( storeVersionCheck, toFormat, tailScanner );
					foreach ( RecordFormats fromFormat in recordFormats )
					{
						 try
						 {
							  CreateDb( fromFormat, databaseLayout.DatabaseDirectory() );
							  if ( !upgradableDatabase.HasCurrentVersion( databaseLayout ) )
							  {
									upgradableDatabase.CheckUpgradable( databaseLayout );
									data.Add( new object[]{ fromFormat, toFormat } );
							  }
						 }
						 catch ( Exception )
						 {
							  //This means that the combination is not migratable.
						 }
						 fs.DeleteRecursively( databaseLayout.DatabaseDirectory() );
					}
			  }

			  return data;
		 }

		 private static string BaseDirName( RecordFormats toFormat, RecordFormats fromFormat )
		 {
			  return fromFormat.StoreVersion() + toFormat.StoreVersion();
		 }

		 private static void AddIfNotThere( RecordFormats f, List<RecordFormats> recordFormats )
		 {
			  foreach ( RecordFormats format in recordFormats )
			  {
					if ( format.StoreVersion().Equals(f.StoreVersion()) )
					{
						 return;
					}
			  }
			  recordFormats.Add( f );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(RecordFormats.Factory.class) public static class Standard23Factory extends Neo4Net.kernel.impl.store.format.RecordFormats_Factory
		 public class Standard23Factory : Neo4Net.Kernel.impl.store.format.RecordFormats_Factory
		 {
			  public Standard23Factory() : base(StandardV2_3.STORE_VERSION)
			  {
			  }

			  public override RecordFormats NewInstance()
			  {
					return StandardV2_3.RECORD_FORMATS;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(RecordFormats.Factory.class) public static class Standard30Factory extends Neo4Net.kernel.impl.store.format.RecordFormats_Factory
		 public class Standard30Factory : Neo4Net.Kernel.impl.store.format.RecordFormats_Factory
		 {
			  public Standard30Factory() : base(StandardV3_0.STORE_VERSION)
			  {
			  }

			  public override RecordFormats NewInstance()
			  {
					return StandardV3_0.RECORD_FORMATS;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(RecordFormats.Factory.class) public static class Standard32Factory extends Neo4Net.kernel.impl.store.format.RecordFormats_Factory
		 public class Standard32Factory : Neo4Net.Kernel.impl.store.format.RecordFormats_Factory
		 {
			  public Standard32Factory() : base(StandardV3_2.STORE_VERSION)
			  {
			  }

			  public override RecordFormats NewInstance()
			  {
					return StandardV3_2.RECORD_FORMATS;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(RecordFormats.Factory.class) public static class Standard34Factory extends Neo4Net.kernel.impl.store.format.RecordFormats_Factory
		 public class Standard34Factory : Neo4Net.Kernel.impl.store.format.RecordFormats_Factory
		 {
			  public Standard34Factory() : base(StandardV3_4.STORE_VERSION)
			  {
			  }

			  public override RecordFormats NewInstance()
			  {
					return StandardV3_4.RECORD_FORMATS;
			  }
		 }

		 private static void CreateDb( RecordFormats recordFormat, File storeDir )
		 {
			  IGraphDatabaseService database = ( new GraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(storeDir).setConfig(GraphDatabaseSettings.allow_upgrade, Settings.TRUE).setConfig(GraphDatabaseSettings.record_format, recordFormat.StoreVersion()).newGraphDatabase();
			  database.Shutdown();
		 }

		 public StoreMigrationIT( RecordFormats from, RecordFormats to )
		 {
			  this.From = from;
			  this.To = to;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMigrate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMigrate()
		 {
			  DatabaseLayout databaseLayout = _testDir.databaseLayout( BaseDirName( To, From ) );
			  IGraphDatabaseService database = GetGraphDatabaseService( databaseLayout.DatabaseDirectory(), From.storeVersion() );

			  database.Execute( "CREATE INDEX ON :Person(name)" );
			  database.Execute( "CREATE INDEX ON :Person(born)" );
			  database.Execute( "CREATE CONSTRAINT ON (person:Person) ASSERT exists(person.name)" );
			  database.Execute( CreateQuery );
			  long beforeNodes;
			  long beforeLabels;
			  long beforeKeys;
			  long beforeRels;
			  long beforeRelTypes;
			  long beforeIndexes;
			  long beforeConstraints;
			  using ( Transaction ignore = database.BeginTx() )
			  {
					beforeNodes = database.AllNodes.Count();
					beforeLabels = database.AllLabels.Count();
					beforeKeys = database.AllPropertyKeys.Count();
					beforeRels = database.AllRelationships.Count();
					beforeRelTypes = database.AllRelationshipTypes.Count();
					beforeIndexes = Stream( database.Schema().Indexes ).count();
					beforeConstraints = Stream( database.Schema().Constraints ).count();
			  }
			  database.Shutdown();

			  database = GetGraphDatabaseService( databaseLayout.DatabaseDirectory(), To.storeVersion() );
			  long afterNodes;
			  long afterLabels;
			  long afterKeys;
			  long afterRels;
			  long afterRelTypes;
			  long afterIndexes;
			  long afterConstraints;
			  using ( Transaction ignore = database.BeginTx() )
			  {
					afterNodes = database.AllNodes.Count();
					afterLabels = database.AllLabels.Count();
					afterKeys = database.AllPropertyKeys.Count();
					afterRels = database.AllRelationships.Count();
					afterRelTypes = database.AllRelationshipTypes.Count();
					afterIndexes = Stream( database.Schema().Indexes ).count();
					afterConstraints = Stream( database.Schema().Constraints ).count();
			  }
			  database.Shutdown();

			  assertEquals( beforeNodes, afterNodes ); //171
			  assertEquals( beforeLabels, afterLabels ); //2
			  assertEquals( beforeKeys, afterKeys ); //8
			  assertEquals( beforeRels, afterRels ); //253
			  assertEquals( beforeRelTypes, afterRelTypes ); //6
			  assertEquals( beforeIndexes, afterIndexes ); //2
			  assertEquals( beforeConstraints, afterConstraints ); //1
			  ConsistencyCheckService consistencyCheckService = new ConsistencyCheckService();
			  ConsistencyCheckService.Result result = RunConsistencyChecker( databaseLayout, _fileSystemRule.get(), consistencyCheckService, To.storeVersion() );
			  if ( !result.Successful )
			  {
					fail( "Database is inconsistent after migration." );
			  }
		 }

		 protected internal virtual Stream<T> Stream<T>( IEnumerable<T> iterable )
		 {
			  return StreamSupport.stream( iterable.spliterator(), false );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected static Neo4Net.consistency.ConsistencyCheckService.Result runConsistencyChecker(Neo4Net.io.layout.DatabaseLayout databaseLayout, Neo4Net.io.fs.FileSystemAbstraction fs, Neo4Net.consistency.ConsistencyCheckService consistencyCheckService, String storeVersion) throws Neo4Net.consistency.checking.full.ConsistencyCheckIncompleteException
		 protected internal static ConsistencyCheckService.Result RunConsistencyChecker( DatabaseLayout databaseLayout, FileSystemAbstraction fs, ConsistencyCheckService consistencyCheckService, string storeVersion )
		 {
			  Config config = Config.defaults( GraphDatabaseSettings.record_format, storeVersion );
			  return consistencyCheckService.runFullConsistencyCheck( databaseLayout, config, ProgressMonitorFactory.NONE, NullLogProvider.Instance, fs, false );
		 }

		 protected internal static IGraphDatabaseService GetGraphDatabaseService( File db, string storeVersion )
		 {
			  return ( new EnterpriseGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(db).setConfig(GraphDatabaseSettings.allow_upgrade, Settings.TRUE).setConfig(GraphDatabaseSettings.record_format, storeVersion).newGraphDatabase();
		 }
	}

}