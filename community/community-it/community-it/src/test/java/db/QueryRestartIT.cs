﻿/*
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
namespace Db
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using QueryExecutionException = Org.Neo4j.Graphdb.QueryExecutionException;
	using Result = Org.Neo4j.Graphdb.Result;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseDependencies = Org.Neo4j.Graphdb.facade.GraphDatabaseDependencies;
	using GraphDatabaseFacadeFactory = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory;
	using GraphDatabaseBuilder = Org.Neo4j.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactoryState = Org.Neo4j.Graphdb.factory.GraphDatabaseFactoryState;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using CommunityEditionModule = Org.Neo4j.Graphdb.factory.module.edition.CommunityEditionModule;
	using VersionContext = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContext;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using TransactionVersionContext = Org.Neo4j.Kernel.impl.context.TransactionVersionContext;
	using TransactionVersionContextSupplier = Org.Neo4j.Kernel.impl.context.TransactionVersionContextSupplier;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class QueryRestartIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
		 private GraphDatabaseService _database;
		 private TestTransactionVersionContextSupplier _testContextSupplier;
		 private File _storeDir;
		 private TestVersionContext _testCursorContext;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _storeDir = TestDirectory.directory();
			  _testContextSupplier = new TestTransactionVersionContextSupplier( this );
			  _database = StartSnapshotQueryDb();
			  CreateData();

			  _testCursorContext = _testCursorContext();
			  _testContextSupplier.CursorContext = _testCursorContext;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  if ( _database != null )
			  {
					_database.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void executeQueryWithoutRestarts()
		 public virtual void ExecuteQueryWithoutRestarts()
		 {
			  _testCursorContext.WrongLastClosedTxId = false;

			  Result result = _database.execute( "MATCH (n:label) RETURN n.c" );
			  while ( result.MoveNext() )
			  {
					assertEquals( "d", result.Current.get( "n.c" ) );
			  }
			  assertEquals( 0, _testCursorContext.AdditionalAttempts );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void executeQueryWithSingleRetry()
		 public virtual void ExecuteQueryWithSingleRetry()
		 {
			  Result result = _database.execute( "MATCH (n) RETURN n.c" );
			  assertEquals( 1, _testCursorContext.AdditionalAttempts );
			  while ( result.MoveNext() )
			  {
					assertEquals( "d", result.Current.get( "n.c" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void executeCountStoreQueryWithSingleRetry()
		 public virtual void ExecuteCountStoreQueryWithSingleRetry()
		 {
			  Result result = _database.execute( "MATCH (n:toRetry) RETURN count(n)" );
			  assertEquals( 1, _testCursorContext.AdditionalAttempts );
			  while ( result.MoveNext() )
			  {
					assertEquals( 1L, result.Current.get( "count(n)" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void executeLabelScanQueryWithSingleRetry()
		 public virtual void ExecuteLabelScanQueryWithSingleRetry()
		 {
			  Result result = _database.execute( "MATCH (n:toRetry) RETURN n.c" );
			  assertEquals( 1, _testCursorContext.AdditionalAttempts );
			  while ( result.MoveNext() )
			  {
					assertEquals( "d", result.Current.get( "n.c" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void queryThatModifyDataAndSeeUnstableSnapshotThrowException()
		 public virtual void QueryThatModifyDataAndSeeUnstableSnapshotThrowException()
		 {
			  try
			  {
					_database.execute( "MATCH (n:toRetry) CREATE () RETURN n.c" );
			  }
			  catch ( QueryExecutionException e )
			  {
					assertEquals( "Unable to get clean data snapshot for query " + "'MATCH (n:toRetry) CREATE () RETURN n.c' that perform updates.", e.Message );
			  }
		 }

		 private GraphDatabaseService StartSnapshotQueryDb()
		 {
			  return ( new CustomGraphDatabaseFactory( this, new CustomFacadeFactory( this ) ) ).NewEmbeddedDatabaseBuilder( _storeDir ).setConfig( GraphDatabaseSettings.snapshot_query, Settings.TRUE ).newGraphDatabase();
		 }

		 private void CreateData()
		 {
			  Label label = Label.label( "toRetry" );
			  using ( Transaction transaction = _database.beginTx() )
			  {
					Node node = _database.createNode( label );
					node.SetProperty( "c", "d" );
					transaction.Success();
			  }
		 }

		 private TestVersionContext TestCursorContext()
		 {
			  TransactionIdStore transactionIdStore = TransactionIdStore;
			  return new TestVersionContext( this, transactionIdStore.getLastClosedTransactionId );
		 }

		 private TransactionIdStore TransactionIdStore
		 {
			 get
			 {
				  DependencyResolver dependencyResolver = ( ( GraphDatabaseAPI ) _database ).DependencyResolver;
				  return dependencyResolver.ResolveDependency( typeof( TransactionIdStore ) );
			 }
		 }

		 private class CustomGraphDatabaseFactory : TestGraphDatabaseFactory
		 {
			 private readonly QueryRestartIT _outerInstance;

			  internal GraphDatabaseFacadeFactory CustomFacadeFactory;

			  internal CustomGraphDatabaseFactory( QueryRestartIT outerInstance, GraphDatabaseFacadeFactory customFacadeFactory )
			  {
				  this._outerInstance = outerInstance;
					this.CustomFacadeFactory = customFacadeFactory;
			  }

			  protected internal override GraphDatabaseBuilder.DatabaseCreator CreateDatabaseCreator( File storeDir, GraphDatabaseFactoryState state )
			  {
					return new DatabaseCreatorAnonymousInnerClass( this, storeDir, state );
			  }

			  private class DatabaseCreatorAnonymousInnerClass : GraphDatabaseBuilder.DatabaseCreator
			  {
				  private readonly CustomGraphDatabaseFactory _outerInstance;

				  private File _storeDir;
				  private GraphDatabaseFactoryState _state;

				  public DatabaseCreatorAnonymousInnerClass( CustomGraphDatabaseFactory outerInstance, File storeDir, GraphDatabaseFactoryState state )
				  {
					  this.outerInstance = outerInstance;
					  this._storeDir = storeDir;
					  this._state = state;
				  }

				  public GraphDatabaseService newDatabase( Config config )
				  {
						return _outerInstance.customFacadeFactory.newFacade( _storeDir, config, GraphDatabaseDependencies.newDependencies( _state.databaseDependencies() ) );
				  }
			  }
		 }

		 private class CustomFacadeFactory : GraphDatabaseFacadeFactory
		 {
			 private readonly QueryRestartIT _outerInstance;


			  internal CustomFacadeFactory( QueryRestartIT outerInstance ) : base( DatabaseInfo.COMMUNITY, CommunityEditionModule::new )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
				  this._outerInstance = outerInstance;
			  }

			  protected internal override PlatformModule CreatePlatform( File storeDir, Config config, Dependencies dependencies )
			  {
					return new PlatformModuleAnonymousInnerClass( this, storeDir, config, DatabaseInfo, dependencies );
			  }

			  private class PlatformModuleAnonymousInnerClass : PlatformModule
			  {
				  private readonly CustomFacadeFactory _outerInstance;

				  private new Config _config;

				  public PlatformModuleAnonymousInnerClass( CustomFacadeFactory outerInstance, File storeDir, Config config, DatabaseInfo databaseInfo, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( storeDir, config, databaseInfo, dependencies )
				  {
					  this.outerInstance = outerInstance;
					  this._config = config;
				  }

				  protected internal override VersionContextSupplier createCursorContextSupplier( Config config )
				  {
						return outerInstance.outerInstance.testContextSupplier != null ? outerInstance.outerInstance.testContextSupplier : base.createCursorContextSupplier( config );
				  }
			  }
		 }

		 private class TestVersionContext : TransactionVersionContext
		 {
			 private readonly QueryRestartIT _outerInstance;


//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool WrongLastClosedTxIdConflict = true;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int AdditionalAttemptsConflict;

			  internal TestVersionContext( QueryRestartIT outerInstance, System.Func<long> transactionIdSupplier ) : base( transactionIdSupplier )
			  {
				  this._outerInstance = outerInstance;
			  }

			  public override long LastClosedTransactionId()
			  {
					return WrongLastClosedTxIdConflict ? Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID : base.LastClosedTransactionId();
			  }

			  public override void MarkAsDirty()
			  {
					base.MarkAsDirty();
					WrongLastClosedTxIdConflict = false;
			  }

			  internal virtual bool WrongLastClosedTxId
			  {
				  set
				  {
						this.WrongLastClosedTxIdConflict = value;
				  }
			  }

			  public override bool Dirty
			  {
				  get
				  {
						bool dirty = base.Dirty;
						if ( dirty )
						{
							 AdditionalAttemptsConflict++;
						}
						return dirty;
				  }
			  }

			  internal virtual int AdditionalAttempts
			  {
				  get
				  {
						return AdditionalAttemptsConflict;
				  }
			  }
		 }

		 private class TestTransactionVersionContextSupplier : TransactionVersionContextSupplier
		 {
			 private readonly QueryRestartIT _outerInstance;

			 public TestTransactionVersionContextSupplier( QueryRestartIT outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal virtual VersionContext CursorContext
			  {
				  set
				  {
						this.CursorContext.set( value );
				  }
			  }
		 }
	}

}