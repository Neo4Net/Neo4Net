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
namespace Counts
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Org.Neo4j.Graphdb;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using UncloseableDelegatingFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using Kernel = Org.Neo4j.@internal.Kernel.Api.Kernel;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;
	using CheckPointer = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.index_background_sampling_enabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.Transaction_Type.@explicit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.LoginContext.AUTH_DISABLED;
	using static Org.Neo4j.Logging.AssertableLogProvider.LogMatcherBuilder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;

	public class RebuildCountsTest
	{
		 private const int ALIENS = 16;
		 private const int HUMANS = 16;
		 private static readonly Label _alien = label( "Alien" );
		 private static readonly Label _human = label( "Human" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fsRule = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule FsRule = new EphemeralFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
		 private readonly AssertableLogProvider _userLogProvider = new AssertableLogProvider();
		 private readonly AssertableLogProvider _internalLogProvider = new AssertableLogProvider();

		 private GraphDatabaseService _db;
		 private File _storeDir;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Before()
		 {
			  _storeDir = TestDirectory.databaseDir();
			  Restart( FsRule.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  DoCleanShutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRebuildMissingCountsStoreOnStart() throws java.io.IOException, org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRebuildMissingCountsStoreOnStart()
		 {
			  // given
			  CreateAliensAndHumans();

			  // when
			  FileSystemAbstraction fs = Shutdown();
			  DeleteCounts( fs );
			  Restart( fs );

			  // then
			  using ( Org.Neo4j.@internal.Kernel.Api.Transaction tx = ( ( GraphDatabaseAPI )_db ).DependencyResolver.resolveDependency( typeof( Kernel ) ).beginTransaction( @explicit, AUTH_DISABLED ) )
			  {
					assertEquals( ALIENS + HUMANS, tx.DataRead().countsForNode(-1) );
					assertEquals( ALIENS, tx.DataRead().countsForNode(LabelId(_alien)) );
					assertEquals( HUMANS, tx.DataRead().countsForNode(LabelId(_human)) );
			  }

			  // and also
			  LogMatcherBuilder matcherBuilder = inLog( typeof( MetaDataStore ) );
			  _internalLogProvider.assertAtLeastOnce( matcherBuilder.warn( "Missing counts store, rebuilding it." ) );
			  _internalLogProvider.assertAtLeastOnce( matcherBuilder.warn( "Counts store rebuild completed." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRebuildMissingCountsStoreAfterRecovery() throws java.io.IOException, org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRebuildMissingCountsStoreAfterRecovery()
		 {
			  // given
			  CreateAliensAndHumans();

			  // when
			  RotateLog();
			  DeleteHumans();
			  FileSystemAbstraction fs = Crash();
			  DeleteCounts( fs );
			  Restart( fs );

			  // then
			  using ( Org.Neo4j.@internal.Kernel.Api.Transaction tx = ( ( GraphDatabaseAPI )_db ).DependencyResolver.resolveDependency( typeof( Kernel ) ).beginTransaction( @explicit, AUTH_DISABLED ) )
			  {
					assertEquals( ALIENS, tx.DataRead().countsForNode(-1) );
					assertEquals( ALIENS, tx.DataRead().countsForNode(LabelId(_alien)) );
					assertEquals( 0, tx.DataRead().countsForNode(LabelId(_human)) );
			  }

			  // and also
			  LogMatcherBuilder matcherBuilder = inLog( typeof( MetaDataStore ) );
			  _internalLogProvider.assertAtLeastOnce( matcherBuilder.warn( "Missing counts store, rebuilding it." ) );
			  _internalLogProvider.assertAtLeastOnce( matcherBuilder.warn( "Counts store rebuild completed." ) );
		 }

		 private void CreateAliensAndHumans()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					for ( int i = 0; i < ALIENS; i++ )
					{
						 _db.createNode( _alien );
					}
					for ( int i = 0; i < HUMANS; i++ )
					{
						 _db.createNode( _human );
					}
					tx.Success();
			  }
		 }

		 private void DeleteHumans()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					using ( ResourceIterator<Node> humans = _db.findNodes( _human ) )
					{
						 while ( humans.MoveNext() )
						 {
							  humans.Current.delete();
						 }
					}
					tx.Success();
			  }
		 }

		 private int LabelId( Label alien )
		 {
			  ThreadToStatementContextBridge contextBridge = ( ( GraphDatabaseAPI ) _db ).DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) );
			  using ( Transaction tx = _db.beginTx() )
			  {
					return contextBridge.GetKernelTransactionBoundToThisThread( true ).tokenRead().nodeLabel(alien.Name());
			  }
		 }

		 private void DeleteCounts( FileSystemAbstraction snapshot )
		 {
			  DatabaseLayout databaseLayout = TestDirectory.databaseLayout();
			  File alpha = databaseLayout.CountStoreA();
			  File beta = databaseLayout.CountStoreB();
			  assertTrue( snapshot.DeleteFile( alpha ) );
			  assertTrue( snapshot.DeleteFile( beta ) );
		 }

		 private FileSystemAbstraction Shutdown()
		 {
			  DoCleanShutdown();
			  return FsRule.get().snapshot();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void rotateLog() throws java.io.IOException
		 private void RotateLog()
		 {
			  ( ( GraphDatabaseAPI ) _db ).DependencyResolver.resolveDependency( typeof( CheckPointer ) ).forceCheckPoint( new SimpleTriggerInfo( "test" ) );
		 }

		 private FileSystemAbstraction Crash()
		 {
			  return FsRule.get().snapshot();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void restart(org.neo4j.io.fs.FileSystemAbstraction fs) throws java.io.IOException
		 private void Restart( FileSystemAbstraction fs )
		 {
			  if ( _db != null )
			  {
					_db.shutdown();
			  }

			  fs.Mkdirs( _storeDir );
			  TestGraphDatabaseFactory dbFactory = new TestGraphDatabaseFactory();
			  _db = dbFactory.setUserLogProvider( _userLogProvider ).setInternalLogProvider( _internalLogProvider ).setFileSystem( new UncloseableDelegatingFileSystemAbstraction( fs ) ).newImpermanentDatabaseBuilder( _storeDir ).setConfig( index_background_sampling_enabled, "false" ).newGraphDatabase();
		 }

		 private void DoCleanShutdown()
		 {
			  try
			  {
					_db.shutdown();
			  }
			  finally
			  {
					_db = null;
			  }
		 }
	}

}