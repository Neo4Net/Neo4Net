﻿using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.Kernel.impl.transaction.command
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using Org.Neo4j.Helpers.Collection;
	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using NativeLuceneFusionIndexProviderFactory20 = Org.Neo4j.Kernel.Api.Impl.Schema.NativeLuceneFusionIndexProviderFactory20;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using TransactionState = Org.Neo4j.Kernel.api.txstate.TransactionState;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using TransactionQueue = Org.Neo4j.Kernel.Impl.Api.TransactionQueue;
	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using IndexProxy = Org.Neo4j.Kernel.Impl.Api.index.IndexProxy;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using TxState = Org.Neo4j.Kernel.Impl.Api.state.TxState;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;
	using FusionIndexProvider = Org.Neo4j.Kernel.Impl.Index.Schema.fusion.FusionIndexProvider;
	using RecordStorageEngine = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using NodeStore = Org.Neo4j.Kernel.impl.store.NodeStore;
	using NodeCommand = Org.Neo4j.Kernel.impl.transaction.command.Command.NodeCommand;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using CommandCreationContext = Org.Neo4j.Storageengine.Api.CommandCreationContext;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;
	using StorageReader = Org.Neo4j.Storageengine.Api.StorageReader;
	using TransactionApplicationMode = Org.Neo4j.Storageengine.Api.TransactionApplicationMode;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using RecordStorageEngineRule = Org.Neo4j.Test.rule.RecordStorageEngineRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;
	using Org.Neo4j.@unsafe.Impl.Batchimport.cache.idmapping.@string;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.TimeUtil.parseTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.schema.NativeLuceneFusionIndexProviderFactory20.DESCRIPTOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.command.Commands.createIndexRule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.command.Commands.transactionRepresentation;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.Commitment.NO_COMMITMENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.txstate.TxStateVisitor_Fields.NO_DECORATION;

	public class IndexWorkSyncTransactionApplicationStressIT
	{
		private bool InstanceFieldsInitialized = false;

		public IndexWorkSyncTransactionApplicationStressIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _directory ).around( _fileSystemRule ).around( _pageCacheRule ).around( _storageEngineRule );
		}

		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private readonly RecordStorageEngineRule _storageEngineRule = new RecordStorageEngineRule();
		 private readonly TestDirectory _directory = TestDirectory.testDirectory();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(directory).around(fileSystemRule).around(pageCacheRule).around(storageEngineRule);
		 public RuleChain RuleChain;

		 private readonly LabelSchemaDescriptor _descriptor = SchemaDescriptorFactory.forLabel( 0, 0 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyIndexUpdatesInWorkSyncedBatches() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyIndexUpdatesInWorkSyncedBatches()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  long duration = parseTimeMillis.apply( System.getProperty( this.GetType().FullName + ".duration", "2s" ) );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  int numThreads = Integer.getInteger( this.GetType().FullName + ".numThreads", Runtime.Runtime.availableProcessors() );
			  DefaultFileSystemAbstraction fs = _fileSystemRule.get();
			  PageCache pageCache = _pageCacheRule.getPageCache( fs );
			  FusionIndexProvider indexProvider = NativeLuceneFusionIndexProviderFactory20.create( pageCache, _directory.databaseDir(), fs, IndexProvider.Monitor_Fields.EMPTY, Config.defaults(), OperationalMode.single, RecoveryCleanupWorkCollector.immediate() );
			  RecordStorageEngine storageEngine = _storageEngineRule.getWith( fs, pageCache, _directory.databaseLayout() ).indexProvider(indexProvider).build();
			  storageEngine.Apply( Tx( singletonList( createIndexRule( DESCRIPTOR, 1, _descriptor ) ) ), TransactionApplicationMode.EXTERNAL );
			  Dependencies dependencies = new Dependencies();
			  storageEngine.SatisfyDependencies( dependencies );
			  IndexProxy index = dependencies.ResolveDependency( typeof( IndexingService ) ).getIndexProxy( _descriptor );
			  AwaitOnline( index );

			  // WHEN
			  Workers<Worker> workers = new Workers<Worker>( this.GetType().Name );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean end = new java.util.concurrent.atomic.AtomicBoolean();
			  AtomicBoolean end = new AtomicBoolean();
			  for ( int i = 0; i < numThreads; i++ )
			  {
					workers.Start( new Worker( this, i, end, storageEngine, 10, index ) );
			  }

			  // let the threads hammer the storage engine for some time
			  Thread.Sleep( duration );
			  end.set( true );

			  // THEN (assertions as part of the workers applying transactions)
			  workers.AwaitAndThrowOnError();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void awaitOnline(org.neo4j.kernel.impl.api.index.IndexProxy index) throws InterruptedException
		 private void AwaitOnline( IndexProxy index )
		 {
			  while ( index.State == InternalIndexState.POPULATING )
			  {
					Thread.Sleep( 10 );
			  }
		 }

		 private static Value PropertyValue( int id, int progress )
		 {
			  return Values.of( id + "_" + progress );
		 }

		 private static TransactionToApply Tx( ICollection<StorageCommand> commands )
		 {
			  TransactionToApply tx = new TransactionToApply( transactionRepresentation( commands ) );
			  tx.Commitment( NO_COMMITMENT, 0 );
			  return tx;
		 }

		 private class Worker : ThreadStart
		 {
			 private readonly IndexWorkSyncTransactionApplicationStressIT _outerInstance;

			  internal readonly int Id;
			  internal readonly AtomicBoolean End;
			  internal readonly RecordStorageEngine StorageEngine;
			  internal readonly NodeStore NodeIds;
			  internal readonly int BatchSize;
			  internal readonly IndexProxy Index;
			  internal readonly CommandCreationContext CommandCreationContext;
			  internal int I;
			  internal int Base;

			  internal Worker( IndexWorkSyncTransactionApplicationStressIT outerInstance, int id, AtomicBoolean end, RecordStorageEngine storageEngine, int batchSize, IndexProxy index )
			  {
				  this._outerInstance = outerInstance;
					this.Id = id;
					this.End = end;
					this.StorageEngine = storageEngine;
					this.BatchSize = batchSize;
					this.Index = index;
					NeoStores neoStores = this.StorageEngine.testAccessNeoStores();
					this.NodeIds = neoStores.NodeStore;
					this.CommandCreationContext = storageEngine.AllocateCommandCreationContext();
			  }

			  public override void Run()
			  {
					try
					{
						 TransactionQueue queue = new TransactionQueue(BatchSize, (tx, last) =>
						 {
						  // Apply
						  StorageEngine.apply( tx, TransactionApplicationMode.EXTERNAL );

						  // And verify that all nodes are in the index
						  VerifyIndex( tx );
						  Base += BatchSize;
						 });
						 for ( ; !End.get(); I++ )
						 {
							  queue.Queue( CreateNodeAndProperty( I ) );
						 }
						 queue.Empty();
					}
					catch ( Exception e )
					{
						 throw new Exception( e );
					}
					finally
					{
						 CommandCreationContext.close();
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.api.TransactionToApply createNodeAndProperty(int progress) throws Exception
			  internal virtual TransactionToApply CreateNodeAndProperty( int progress )
			  {
					TransactionState txState = new TxState();
					long nodeId = NodeIds.nextId();
					txState.NodeDoCreate( nodeId );
					txState.NodeDoAddLabel( outerInstance.descriptor.LabelId, nodeId );
					txState.NodeDoAddProperty( nodeId, outerInstance.descriptor.PropertyId, PropertyValue( Id, progress ) );
					ICollection<StorageCommand> commands = new List<StorageCommand>();
					using ( StorageReader statement = StorageEngine.newReader() )
					{
						 StorageEngine.createCommands( commands, txState, statement, null, 0, NO_DECORATION );
					}
					return Tx( commands );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyIndex(org.neo4j.kernel.impl.api.TransactionToApply tx) throws Exception
			  internal virtual void VerifyIndex( TransactionToApply tx )
			  {
					using ( IndexReader reader = Index.newReader() )
					{
						 NodeVisitor visitor = new NodeVisitor();
						 for ( int i = 0; tx != null; i++ )
						 {
							  tx.TransactionRepresentation().accept(visitor.Clear());

							  Value propertyValue = propertyValue( Id, Base + i );
							  IndexQuery.ExactPredicate query = IndexQuery.exact( outerInstance.descriptor.PropertyId, propertyValue );
							  LongIterator hits = reader.Query( query );
							  assertEquals( "Index doesn't contain " + visitor.NodeId + " " + propertyValue, visitor.NodeId, hits.next() );
							  assertFalse( hits.hasNext() );
							  tx = tx.Next();
						 }
					}
			  }
		 }

		 private class NodeVisitor : Visitor<StorageCommand, IOException>
		 {
			  internal long NodeId;

			  public override bool Visit( StorageCommand element )
			  {
					if ( element is NodeCommand )
					{
						 NodeId = ( ( NodeCommand )element ).Key;
					}
					return false;
			  }

			  public virtual NodeVisitor Clear()
			  {
					NodeId = -1;
					return this;
			  }
		 }
	}

}