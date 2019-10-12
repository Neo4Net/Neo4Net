using System;
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
namespace Org.Neo4j.Cypher.@internal.javacompat
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using NotFoundException = Org.Neo4j.Graphdb.NotFoundException;
	using QueryExecutionException = Org.Neo4j.Graphdb.QueryExecutionException;
	using QueryExecutionType = Org.Neo4j.Graphdb.QueryExecutionType;
	using Org.Neo4j.Graphdb;
	using Result = Org.Neo4j.Graphdb.Result;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseDependencies = Org.Neo4j.Graphdb.facade.GraphDatabaseDependencies;
	using GraphDatabaseFacadeFactory = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory;
	using GraphDatabaseBuilder = Org.Neo4j.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactoryState = Org.Neo4j.Graphdb.factory.GraphDatabaseFactoryState;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using CommunityEditionModule = Org.Neo4j.Graphdb.factory.module.edition.CommunityEditionModule;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
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
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class EagerResultIT
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
			  _database = StartRestartableDatabase();
			  PrepareData();
			  TransactionIdStore transactionIdStore = TransactionIdStore;
			  _testCursorContext = new TestVersionContext( this, transactionIdStore.getLastClosedTransactionId );
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
//ORIGINAL LINE: @Test public void eagerResultContainsAllData()
		 public virtual void EagerResultContainsAllData()
		 {
			  Result result = _database.execute( "MATCH (n) RETURN n.c" );
			  assertEquals( 1, _testCursorContext.AdditionalAttempts );
			  int rows = 0;
			  while ( result.MoveNext() )
			  {
					result.Current;
					rows++;
			  }
			  assertEquals( 2, rows );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void eagerResultContainsExecutionType()
		 public virtual void EagerResultContainsExecutionType()
		 {
			  Result result = _database.execute( "MATCH (n) RETURN n.c" );
			  assertEquals( 1, _testCursorContext.AdditionalAttempts );
			  assertEquals( QueryExecutionType.query( QueryExecutionType.QueryType.READ_ONLY ), result.QueryExecutionType );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void eagerResultContainsColumns()
		 public virtual void EagerResultContainsColumns()
		 {
			  Result result = _database.execute( "MATCH (n) RETURN n.c as a, count(n) as b" );
			  assertEquals( 1, _testCursorContext.AdditionalAttempts );
			  assertEquals( Arrays.asList( "a", "b" ), result.Columns() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void useColumnAsOnEagerResult()
		 public virtual void UseColumnAsOnEagerResult()
		 {
			  Result result = _database.execute( "MATCH (n) RETURN n.c as c, n.b as b" );
			  assertEquals( 1, _testCursorContext.AdditionalAttempts );
			  ResourceIterator<object> cValues = result.ColumnAs( "c" );
			  int rows = 0;
			  while ( cValues.MoveNext() )
			  {
					cValues.Current;
					rows++;
			  }
			  assertEquals( 2, rows );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void eagerResultHaveQueryStatistic()
		 public virtual void EagerResultHaveQueryStatistic()
		 {
			  Result result = _database.execute( "MATCH (n) RETURN n.c" );
			  assertEquals( 1, _testCursorContext.AdditionalAttempts );
			  assertFalse( result.QueryStatistics.containsUpdates() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void eagerResultHaveExecutionPlan()
		 public virtual void EagerResultHaveExecutionPlan()
		 {
			  Result result = _database.execute( "profile MATCH (n) RETURN n.c" );
			  assertEquals( 1, _testCursorContext.AdditionalAttempts );
			  assertEquals( 2, result.ExecutionPlanDescription.ProfilerStatistics.Rows );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void eagerResultHaveNotifications()
		 public virtual void EagerResultHaveNotifications()
		 {
			  Result result = _database.execute( " CYPHER planner=rule MATCH (n) RETURN n.c" );
			  assertEquals( 1, _testCursorContext.AdditionalAttempts );
			  assertThat( Iterables.count( result.Notifications ), greaterThan( 0L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void eagerResultToString()
		 public virtual void EagerResultToString()
		 {
			  Result result = _database.execute( "MATCH (n) RETURN n.c, n.d" );
			  assertEquals( 1, _testCursorContext.AdditionalAttempts );
			  string resultString = result.ResultAsString();
			  assertTrue( resultString.Contains( "n.c, n.d" ) );
			  assertTrue( resultString.Contains( "d, a" ) );
			  assertTrue( resultString.Contains( "y, k" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void eagerResultWriteAsStringToStream()
		 public virtual void EagerResultWriteAsStringToStream()
		 {
			  Result result = _database.execute( "MATCH (n) RETURN n.c" );
			  assertEquals( 1, _testCursorContext.AdditionalAttempts );
			  assertEquals( result.ResultAsString(), PrintToStream(result) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void eagerResultVisit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void EagerResultVisit()
		 {
			  Result result = _database.execute( "MATCH (n) RETURN n.c" );
			  IList<string> values = new List<string>();
			  result.Accept((Org.Neo4j.Graphdb.Result_ResultVisitor<Exception>) row =>
			  {
				values.Add( row.getString( "n.c" ) );
				return false;
			  });
			  assertThat( values, hasSize( 2 ) );
			  assertThat( values, containsInAnyOrder( "d", "y" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.graphdb.QueryExecutionException.class) public void dirtyContextDuringResultVisitResultInUnstableSnapshotException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DirtyContextDuringResultVisitResultInUnstableSnapshotException()
		 {
			  Result result = _database.execute( "MATCH (n) RETURN n.c" );
			  IList<string> values = new List<string>();
			  result.Accept((Org.Neo4j.Graphdb.Result_ResultVisitor<Exception>) row =>
			  {
				_testCursorContext.markAsDirty();
				values.Add( row.getString( "n.c" ) );
				return false;
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.graphdb.QueryExecutionException.class) public void dirtyContextEntityNotFoundExceptionDuringResultVisitResultInUnstableSnapshotException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DirtyContextEntityNotFoundExceptionDuringResultVisitResultInUnstableSnapshotException()
		 {
			  Result result = _database.execute( "MATCH (n) RETURN n.c" );
			  result.Accept((Org.Neo4j.Graphdb.Result_ResultVisitor<Exception>) row =>
			  {
				_testCursorContext.markAsDirty();
				throw new NotFoundException( new Exception() );
			  });
		 }

		 private string PrintToStream( Result result )
		 {
			  StringWriter stringWriter = new StringWriter();
			  PrintWriter printWriter = new PrintWriter( stringWriter );
			  result.WriteAsStringTo( printWriter );
			  printWriter.flush();
			  return stringWriter.ToString();
		 }

		 private void PrepareData()
		 {
			  Label label = Label.label( "label" );
			  using ( Transaction transaction = _database.beginTx() )
			  {
					Node node = _database.createNode( label );
					node.SetProperty( "c", "d" );
					node.SetProperty( "d", "a" );
					transaction.Success();
			  }
			  using ( Transaction transaction = _database.beginTx() )
			  {
					Node node = _database.createNode( label );
					node.SetProperty( "c", "y" );
					node.SetProperty( "d", "k" );
					transaction.Success();
			  }
		 }

		 private GraphDatabaseService StartRestartableDatabase()
		 {
			  return ( new CustomGraphDatabaseFactory( this, new CustomFacadeFactory( this ) ) ).NewEmbeddedDatabaseBuilder( _storeDir ).setConfig( GraphDatabaseSettings.snapshot_query, Settings.TRUE ).newGraphDatabase();
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
			 private readonly EagerResultIT _outerInstance;


			  internal GraphDatabaseFacadeFactory CustomFacadeFactory;

			  internal CustomGraphDatabaseFactory( EagerResultIT outerInstance, GraphDatabaseFacadeFactory customFacadeFactory )
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
			 private readonly EagerResultIT _outerInstance;


			  internal CustomFacadeFactory( EagerResultIT outerInstance ) : base( DatabaseInfo.COMMUNITY, CommunityEditionModule::new )
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
			 private readonly EagerResultIT _outerInstance;


			  internal bool UseCorrectLastCommittedTxId;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int AdditionalAttemptsConflict;

			  internal TestVersionContext( EagerResultIT outerInstance, System.Func<long> transactionIdSupplier ) : base( transactionIdSupplier )
			  {
				  this._outerInstance = outerInstance;
			  }

			  public override long LastClosedTransactionId()
			  {
					return UseCorrectLastCommittedTxId ? Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID : base.LastClosedTransactionId();
			  }

			  public override void MarkAsDirty()
			  {
					base.MarkAsDirty();
					UseCorrectLastCommittedTxId = true;
			  }

			  public override bool Dirty
			  {
				  get
				  {
						AdditionalAttemptsConflict++;
						return base.Dirty;
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
			 private readonly EagerResultIT _outerInstance;

			 public TestTransactionVersionContextSupplier( EagerResultIT outerInstance )
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