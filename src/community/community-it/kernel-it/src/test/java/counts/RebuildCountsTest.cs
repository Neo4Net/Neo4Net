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
namespace Counts
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Neo4Net.GraphDb;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using UncloseableDelegatingFileSystemAbstraction = Neo4Net.GraphDb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using Kernel = Neo4Net.Internal.Kernel.Api.Kernel;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.index_background_sampling_enabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.Transaction_Type.@explicit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.security.LoginContext.AUTH_DISABLED;
	using static Neo4Net.Logging.AssertableLogProvider.LogMatcherBuilder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.logging.AssertableLogProvider.inLog;

	public class RebuildCountsTest
	{
		 private const int ALIENS = 16;
		 private const int HUMANS = 16;
		 private static readonly Label _alien = label( "Alien" );
		 private static readonly Label _human = label( "Human" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.fs.EphemeralFileSystemRule fsRule = new org.Neo4Net.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule FsRule = new EphemeralFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory testDirectory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
		 private readonly AssertableLogProvider _userLogProvider = new AssertableLogProvider();
		 private readonly AssertableLogProvider _internalLogProvider = new AssertableLogProvider();

		 private IGraphDatabaseService _db;
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
//ORIGINAL LINE: @Test public void shouldRebuildMissingCountsStoreOnStart() throws java.io.IOException, org.Neo4Net.internal.kernel.api.exceptions.TransactionFailureException
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
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = ( ( GraphDatabaseAPI )_db ).DependencyResolver.resolveDependency( typeof( Kernel ) ).beginTransaction( @explicit, AUTH_DISABLED ) )
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
//ORIGINAL LINE: @Test public void shouldRebuildMissingCountsStoreAfterRecovery() throws java.io.IOException, org.Neo4Net.internal.kernel.api.exceptions.TransactionFailureException
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
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = ( ( GraphDatabaseAPI )_db ).DependencyResolver.resolveDependency( typeof( Kernel ) ).beginTransaction( @explicit, AUTH_DISABLED ) )
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
//ORIGINAL LINE: private void restart(org.Neo4Net.io.fs.FileSystemAbstraction fs) throws java.io.IOException
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