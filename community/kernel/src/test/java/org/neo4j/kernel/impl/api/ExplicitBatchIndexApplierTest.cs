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
namespace Org.Neo4j.Kernel.Impl.Api
{
	using MutableObjectIntMap = org.eclipse.collections.api.map.primitive.MutableObjectIntMap;
	using ObjectIntHashMap = org.eclipse.collections.impl.map.mutable.primitive.ObjectIntHashMap;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using IndexManager = Org.Neo4j.Graphdb.index.IndexManager;
	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using AddNodeCommand = Org.Neo4j.Kernel.impl.index.IndexCommand.AddNodeCommand;
	using AddRelationshipCommand = Org.Neo4j.Kernel.impl.index.IndexCommand.AddRelationshipCommand;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using IndexDefineCommand = Org.Neo4j.Kernel.impl.index.IndexDefineCommand;
	using Commitment = Org.Neo4j.Kernel.impl.transaction.log.Commitment;
	using FakeCommitment = Org.Neo4j.Kernel.impl.transaction.log.FakeCommitment;
	using PhysicalTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using SynchronizedArrayIdOrderingQueue = Org.Neo4j.Kernel.impl.util.SynchronizedArrayIdOrderingQueue;
	using LifeRule = Org.Neo4j.Kernel.Lifecycle.LifeRule;
	using Race = Org.Neo4j.Test.Race;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.IdOrderingQueue.BYPASS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.TransactionApplicationMode.INTERNAL;

	public class ExplicitBatchIndexApplierTest
	{
		private bool InstanceFieldsInitialized = false;

		public ExplicitBatchIndexApplierTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.testDirectory( _fs );
			RuleChain = RuleChain.outerRule( _fs ).around( _testDirectory ).around( _life );
		}

		 private readonly LifeRule _life = new LifeRule( true );
		 private readonly EphemeralFileSystemRule _fs = new EphemeralFileSystemRule();
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fs).around(testDirectory).around(life);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyCreateOneApplierPerProvider() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOnlyCreateOneApplierPerProvider()
		 {
			  // GIVEN
			  MutableObjectIntMap<string> names = ObjectIntHashMap.newWithKeysValues( "first", 0, "second", 1 );
			  MutableObjectIntMap<string> keys = ObjectIntHashMap.newWithKeysValues( "key", 0 );
			  string applierName = "test-applier";
			  Commitment commitment = mock( typeof( Commitment ) );
			  when( commitment.HasExplicitIndexChanges() ).thenReturn(true);
			  IndexConfigStore config = NewIndexConfigStore( names, applierName );
			  ExplicitIndexApplierLookup applierLookup = mock( typeof( ExplicitIndexApplierLookup ) );
			  TransactionApplier transactionApplier = mock( typeof( TransactionApplier ) );
			  when( applierLookup.NewApplier( anyString(), anyBoolean() ) ).thenReturn(transactionApplier);
			  using ( ExplicitBatchIndexApplier applier = new ExplicitBatchIndexApplier( config, applierLookup, BYPASS, INTERNAL ) )
			  {
					TransactionToApply tx = new TransactionToApply( null, 2 );
					tx.Commitment( commitment, 2 );
					using ( TransactionApplier txApplier = applier.StartTx( tx ) )
					{
						 // WHEN
						 IndexDefineCommand definitions = definitions( names, keys );
						 txApplier.VisitIndexDefineCommand( definitions );
						 txApplier.VisitIndexAddNodeCommand( AddNodeToIndex( definitions, "first" ) );
						 txApplier.VisitIndexAddNodeCommand( AddNodeToIndex( definitions, "second" ) );
						 txApplier.VisitIndexAddRelationshipCommand( AddRelationshipToIndex( definitions, "second" ) );
					}
			  }

			  // THEN
			  verify( applierLookup, times( 1 ) ).newApplier( eq( applierName ), anyBoolean() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOrderTransactionsMakingExplicitIndexChanges() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOrderTransactionsMakingExplicitIndexChanges()
		 {
			  // GIVEN
			  MutableObjectIntMap<string> names = ObjectIntHashMap.newWithKeysValues( "first", 0, "second", 1 );
			  MutableObjectIntMap<string> keys = ObjectIntHashMap.newWithKeysValues( "key", 0 );
			  string applierName = "test-applier";
			  ExplicitIndexApplierLookup applierLookup = mock( typeof( ExplicitIndexApplierLookup ) );
			  TransactionApplier transactionApplier = mock( typeof( TransactionApplier ) );
			  when( applierLookup.NewApplier( anyString(), anyBoolean() ) ).thenReturn(transactionApplier);
			  IndexConfigStore config = NewIndexConfigStore( names, applierName );

			  // WHEN multiple explicit index transactions are running, they should be done in order
			  SynchronizedArrayIdOrderingQueue queue = new SynchronizedArrayIdOrderingQueue();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicLong lastAppliedTxId = new java.util.concurrent.atomic.AtomicLong(-1);
			  AtomicLong lastAppliedTxId = new AtomicLong( -1 );
			  Race race = new Race();
			  for ( long i = 0; i < 100; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long txId = i;
					long txId = i;
					race.AddContestant(() =>
					{
					 try
					 {
						 using ( ExplicitBatchIndexApplier applier = new ExplicitBatchIndexApplier( config, applierLookup, queue, INTERNAL ) )
						 {
							  TransactionToApply txToApply = new TransactionToApply( new PhysicalTransactionRepresentation( new List<Org.Neo4j.Storageengine.Api.StorageCommand>() ) );
							  FakeCommitment commitment = new FakeCommitment( txId, mock( typeof( TransactionIdStore ) ) );
							  commitment.HasExplicitIndexChanges = true;
							  txToApply.Commitment( commitment, txId );
							  TransactionApplier txApplier = applier.StartTx( txToApply );
   
							  // Make sure threads are unordered
							  Thread.Sleep( ThreadLocalRandom.current().Next(5) );
   
							  // THEN
							  assertTrue( lastAppliedTxId.compareAndSet( txId - 1, txId ) );
   
							  // Closing manually instead of using try-with-resources since we have no additional work to do in
							  // txApplier
							  txApplier.close();
						 }
					 }
					 catch ( Exception e )
					 {
						  throw new Exception( e );
					 }
					});
					queue.Offer( txId );
			  }

			  race.Go();
		 }

		 private static AddRelationshipCommand AddRelationshipToIndex( IndexDefineCommand definitions, string indexName )
		 {
			  AddRelationshipCommand command = new AddRelationshipCommand();
			  command.Init( definitions.GetOrAssignIndexNameId( indexName ), 0L, ( sbyte ) 0, null, 1, 2 );
			  return command;
		 }

		 private static AddNodeCommand AddNodeToIndex( IndexDefineCommand definitions, string indexName )
		 {
			  AddNodeCommand command = new AddNodeCommand();
			  command.Init( definitions.GetOrAssignIndexNameId( indexName ), 0L, ( sbyte ) 0, null );
			  return command;
		 }

		 private static IndexDefineCommand Definitions( MutableObjectIntMap<string> names, MutableObjectIntMap<string> keys )
		 {
			  IndexDefineCommand definitions = new IndexDefineCommand();
			  definitions.Init( names, keys );
			  return definitions;
		 }

		 private IndexConfigStore NewIndexConfigStore( MutableObjectIntMap<string> names, string providerName )
		 {
			  EphemeralFileSystemAbstraction fileSystem = _fs.get();
			  IndexConfigStore store = _life.add( new IndexConfigStore( _testDirectory.databaseLayout(), fileSystem ) );

			  names.forEachKey(name =>
			  {
				store.Set( typeof( Node ), name, stringMap( IndexManager.PROVIDER, providerName ) );
				store.Set( typeof( Relationship ), name, stringMap( IndexManager.PROVIDER, providerName ) );
			  });
			  return store;
		 }
	}

}