﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.core
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;

	using DatabaseManager = Org.Neo4j.Dbms.database.DatabaseManager;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using InwardKernel = Org.Neo4j.Kernel.api.InwardKernel;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using AnonymousContext = Org.Neo4j.Kernel.api.security.AnonymousContext;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using PropertyKeyTokenStore = Org.Neo4j.Kernel.impl.store.PropertyKeyTokenStore;
	using PropertyStore = Org.Neo4j.Kernel.impl.store.PropertyStore;
	using StoreFactory = Org.Neo4j.Kernel.impl.store.StoreFactory;
	using DefaultIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using Org.Neo4j.Test;
	using Org.Neo4j.Test.OtherThreadExecutor;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	/// <summary>
	/// Tests for handling many property keys (even after restart of database)
	/// as well as concurrent creation of property keys.
	/// </summary>
	public class ManyPropertyKeysIT
	{
		private bool InstanceFieldsInitialized = false;

		public ManyPropertyKeysIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _testDirectory ).around( _fileSystemRule ).around( _pageCacheRule );
		}

		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private readonly TestDirectory _testDirectory = TestDirectory.testDirectory();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(testDirectory).around(fileSystemRule).around(pageCacheRule);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void creating_many_property_keys_should_have_all_loaded_the_next_restart() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreatingManyPropertyKeysShouldHaveAllLoadedTheNextRestart()
		 {
			  // GIVEN
			  // The previous limit to load was 2500, so go some above that
			  GraphDatabaseAPI db = DatabaseWithManyPropertyKeys( 3000 );
			  int countBefore = PropertyKeyCount( db );

			  // WHEN
			  Db.shutdown();
			  db = Database();
			  CreateNodeWithProperty( db, Key( 2800 ), true );

			  // THEN
			  assertEquals( countBefore, PropertyKeyCount( db ) );
			  Db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void concurrently_creating_same_property_key_in_different_transactions_should_end_up_with_same_key_id() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConcurrentlyCreatingSamePropertyKeyInDifferentTransactionsShouldEndUpWithSameKeyId()
		 {
			  // GIVEN
			  GraphDatabaseAPI db = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  OtherThreadExecutor<WorkerState> worker1 = new OtherThreadExecutor<WorkerState>( "w1", new WorkerState( db ) );
			  OtherThreadExecutor<WorkerState> worker2 = new OtherThreadExecutor<WorkerState>( "w2", new WorkerState( db ) );
			  worker1.Execute( new BeginTx() );
			  worker2.Execute( new BeginTx() );

			  // WHEN
			  string key = "mykey";
			  worker1.Execute( new CreateNodeAndSetProperty( key ) );
			  worker2.Execute( new CreateNodeAndSetProperty( key ) );
			  worker1.Execute( new FinishTx() );
			  worker2.Execute( new FinishTx() );
			  worker1.Dispose();
			  worker2.Dispose();

			  // THEN
			  assertEquals( 1, PropertyKeyCount( db ) );
			  Db.shutdown();
		 }

		 private GraphDatabaseAPI Database()
		 {
			  return ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(_testDirectory.databaseDir());
		 }

		 private GraphDatabaseAPI DatabaseWithManyPropertyKeys( int propertyKeyCount )
		 {

			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystemRule.get() );
			  StoreFactory storeFactory = new StoreFactory( _testDirectory.databaseLayout(), Config.defaults(), new DefaultIdGeneratorFactory(_fileSystemRule.get()), pageCache, _fileSystemRule.get(), NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
			  NeoStores neoStores = storeFactory.OpenAllNeoStores( true );
			  PropertyKeyTokenStore store = neoStores.PropertyKeyTokenStore;
			  for ( int i = 0; i < propertyKeyCount; i++ )
			  {
					PropertyKeyTokenRecord record = new PropertyKeyTokenRecord( ( int ) store.nextId() );
					record.InUse = true;
					ICollection<DynamicRecord> nameRecords = store.AllocateNameRecords( PropertyStore.encodeString( Key( i ) ) );
					record.AddNameRecords( nameRecords );
					record.NameId = ( int ) Iterables.first( nameRecords ).Id;
					store.UpdateRecord( record );
			  }
			  neoStores.Close();

			  return Database();
		 }

		 private string Key( int i )
		 {
			  return "key" + i;
		 }

		 private static Node CreateNodeWithProperty( GraphDatabaseService db, string key, object value )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					node.SetProperty( key, value );
					tx.Success();
					return node;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static int propertyKeyCount(org.neo4j.kernel.internal.GraphDatabaseAPI db) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private static int PropertyKeyCount( GraphDatabaseAPI db )
		 {
			  InwardKernel kernelAPI = Db.DependencyResolver.resolveDependency( typeof( InwardKernel ) );
			  using ( KernelTransaction tx = kernelAPI.BeginTransaction( KernelTransaction.Type.@implicit, AnonymousContext.read() ) )
			  {
					return tx.TokenRead().propertyKeyCount();
			  }
		 }

		 private class WorkerState
		 {
			  protected internal readonly GraphDatabaseService Db;
			  protected internal Transaction Tx;

			  internal WorkerState( GraphDatabaseService db )
			  {
					this.Db = db;
			  }
		 }

		 private class BeginTx : OtherThreadExecutor.WorkerCommand<WorkerState, Void>
		 {
			  public override Void DoWork( WorkerState state )
			  {
					state.Tx = state.Db.beginTx();
					return null;
			  }
		 }

		 private class CreateNodeAndSetProperty : OtherThreadExecutor.WorkerCommand<WorkerState, Void>
		 {
			  internal readonly string Key;

			  internal CreateNodeAndSetProperty( string key )
			  {
					this.Key = key;
			  }

			  public override Void DoWork( WorkerState state )
			  {
					Node node = state.Db.createNode();
					node.SetProperty( Key, true );
					return null;
			  }
		 }

		 private class FinishTx : OtherThreadExecutor.WorkerCommand<WorkerState, Void>
		 {
			  public override Void DoWork( WorkerState state )
			  {
					state.Tx.success();
					state.Tx.close();
					return null;
			  }
		 }
	}

}