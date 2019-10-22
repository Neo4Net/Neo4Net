using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Kernel.impl.transaction
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ClassGuardedAdversary = Neo4Net.Adversaries.ClassGuardedAdversary;
	using CountingAdversary = Neo4Net.Adversaries.CountingAdversary;
	using AdversarialFileSystemAbstraction = Neo4Net.Adversaries.fs.AdversarialFileSystemAbstraction;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseFacadeFactory = Neo4Net.GraphDb.facade.GraphDatabaseFacadeFactory;
	using EmbeddedGraphDatabase = Neo4Net.GraphDb.facade.embedded.EmbeddedGraphDatabase;
	using GraphDatabaseFactoryState = Neo4Net.GraphDb.factory.GraphDatabaseFactoryState;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using PlatformModule = Neo4Net.GraphDb.factory.module.PlatformModule;
	using CommunityEditionModule = Neo4Net.GraphDb.factory.module.edition.CommunityEditionModule;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using LogRotation = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.stringMap;

	/// <summary>
	/// Here we are verifying that even if we get an exception from the storage layer during commit,
	/// we should still be able to recover to a consistent state.
	/// </summary>
	public class PartialTransactionFailureIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory dir = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Dir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void concurrentlyCommittingTransactionsMustNotRotateOutLoggedCommandsOfFailingTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConcurrentlyCommittingTransactionsMustNotRotateOutLoggedCommandsOfFailingTransaction()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.adversaries.ClassGuardedAdversary adversary = new org.Neo4Net.adversaries.ClassGuardedAdversary(new org.Neo4Net.adversaries.CountingAdversary(1, false), org.Neo4Net.kernel.impl.transaction.command.Command.RelationshipCommand.class);
			  ClassGuardedAdversary adversary = new ClassGuardedAdversary( new CountingAdversary( 1, false ), typeof( Command.RelationshipCommand ) );
			  adversary.Disable();

			  File storeDir = Dir.storeDir();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<String,String> params = stringMap(org.Neo4Net.graphdb.factory.GraphDatabaseSettings.pagecache_memory.name(), "8m");
			  IDictionary<string, string> @params = stringMap( GraphDatabaseSettings.pagecache_memory.name(), "8m" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.facade.embedded.EmbeddedGraphDatabase db = new TestEmbeddedGraphDatabase(storeDir, params)
			  EmbeddedGraphDatabase db = new TestEmbeddedGraphDatabaseAnonymousInnerClass( this, storeDir, @params, adversary );

			  Node a;
			  Node b;
			  Node c;
			  Node d;
			  using ( Transaction tx = Db.beginTx() )
			  {
					a = Db.createNode();
					b = Db.createNode();
					c = Db.createNode();
					d = Db.createNode();
					tx.Success();
			  }

			  adversary.Enable();
			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
			  Thread t1 = new Thread( CreateRelationship( db, a, b, latch ), "T1" );
			  Thread t2 = new Thread( CreateRelationship( db, c, d, latch ), "T2" );
			  t1.Start();
			  t2.Start();
			  // Wait for both threads to get going
			  t1.Join( 10 );
			  t2.Join( 10 );
			  latch.Signal();

			  // Wait for the transactions to finish
			  t1.Join( 25000 );
			  t2.Join( 25000 );
			  Db.shutdown();

			  // We should observe the store in a consistent state
			  EmbeddedGraphDatabase db2 = new TestEmbeddedGraphDatabase( storeDir, @params );
			  try
			  {
					  using ( Transaction tx = db2.BeginTx() )
					  {
						Node x = db2.GetNodeById( a.Id );
						Node y = db2.GetNodeById( b.Id );
						Node z = db2.GetNodeById( c.Id );
						Node w = db2.GetNodeById( d.Id );
						IEnumerator<Relationship> itrRelX = x.Relationships.GetEnumerator();
						IEnumerator<Relationship> itrRelY = y.Relationships.GetEnumerator();
						IEnumerator<Relationship> itrRelZ = z.Relationships.GetEnumerator();
						IEnumerator<Relationship> itrRelW = w.Relationships.GetEnumerator();
      
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						if ( itrRelX.hasNext() != itrRelY.hasNext() )
						{
							 fail( "Node x and y have inconsistent relationship counts" );
						}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						else if ( itrRelX.hasNext() )
						{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 Relationship rel = itrRelX.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 assertEquals( rel, itrRelY.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 assertFalse( itrRelX.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 assertFalse( itrRelY.hasNext() );
						}
      
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						if ( itrRelZ.hasNext() != itrRelW.hasNext() )
						{
							 fail( "Node z and w have inconsistent relationship counts" );
						}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						else if ( itrRelZ.hasNext() )
						{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 Relationship rel = itrRelZ.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 assertEquals( rel, itrRelW.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 assertFalse( itrRelZ.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 assertFalse( itrRelW.hasNext() );
						}
					  }
			  }
			  finally
			  {
					db2.Shutdown();
			  }
		 }

		 private class TestEmbeddedGraphDatabaseAnonymousInnerClass : TestEmbeddedGraphDatabase
		 {
			 private readonly PartialTransactionFailureIT _outerInstance;

			 private ClassGuardedAdversary _adversary;
			 private File _storeDir;
			 private IDictionary<string, string> @params;

			 public TestEmbeddedGraphDatabaseAnonymousInnerClass( PartialTransactionFailureIT outerInstance, File storeDir, IDictionary<string, string> @params, ClassGuardedAdversary adversary ) : base( storeDir, @params )
			 {
				 this.outerInstance = outerInstance;
				 this._adversary = adversary;
				 this._storeDir = storeDir;
				 this.@params = @params;
			 }

			 protected internal override void create( File storeDir, IDictionary<string, string> @params, GraphDatabaseFacadeFactory.Dependencies dependencies )
			 {
				  new GraphDatabaseFacadeFactoryAnonymousInnerClass( this, DatabaseInfo.COMMUNITY, storeDir, dependencies )
				  .initFacade( storeDir, @params, dependencies, this );
			 }

			 private class GraphDatabaseFacadeFactoryAnonymousInnerClass : GraphDatabaseFacadeFactory
			 {
				 private readonly TestEmbeddedGraphDatabaseAnonymousInnerClass _outerInstance;

				 private File _storeDir;
				 private GraphDatabaseFacadeFactory.Dependencies _dependencies;

				 public GraphDatabaseFacadeFactoryAnonymousInnerClass( TestEmbeddedGraphDatabaseAnonymousInnerClass outerInstance, DatabaseInfo community, File storeDir, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( community, CommunityEditionModule::new )
				 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					 this.outerInstance = outerInstance;
					 this._storeDir = storeDir;
					 this._dependencies = dependencies;
				 }

				 protected internal override PlatformModule createPlatform( File storeDir, Config config, Dependencies dependencies )
				 {
					  return new PlatformModuleAnonymousInnerClass( this, storeDir, config, databaseInfo, dependencies );
				 }

				 private class PlatformModuleAnonymousInnerClass : PlatformModule
				 {
					 private readonly GraphDatabaseFacadeFactoryAnonymousInnerClass _outerInstance;

					 public PlatformModuleAnonymousInnerClass( GraphDatabaseFacadeFactoryAnonymousInnerClass outerInstance, File storeDir, Config config, UnknownType databaseInfo, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( storeDir, config, databaseInfo, dependencies )
					 {
						 this.outerInstance = outerInstance;
					 }

					 protected internal override FileSystemAbstraction createFileSystemAbstraction()
					 {
						  return new AdversarialFileSystemAbstraction( _outerInstance.outerInstance.adversary );
					 }
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Runnable createRelationship(final org.Neo4Net.graphdb.facade.embedded.EmbeddedGraphDatabase db, final org.Neo4Net.graphdb.Node x, final org.Neo4Net.graphdb.Node y, final java.util.concurrent.CountDownLatch latch)
		 private ThreadStart CreateRelationship( EmbeddedGraphDatabase db, Node x, Node y, System.Threading.CountdownEvent latch )
		 {
			  return () =>
			  {
				try
				{
					using ( Transaction tx = Db.beginTx() )
					{
						 x.CreateRelationshipTo( y, RelationshipType.withName( "r" ) );
						 tx.success();
						 latch.await();
						 Db.DependencyResolver.resolveDependency( typeof( LogRotation ) ).rotateLogFile();
						 Db.DependencyResolver.resolveDependency( typeof( CheckPointer ) ).forceCheckPoint(new SimpleTriggerInfo("test")
						);
					}
				}
				catch ( Exception )
				{
					 // We don't care about our transactions failing, as long as we
					 // can recover our database to a consistent state.
				}
			  };
		 }

		 private class TestEmbeddedGraphDatabase : EmbeddedGraphDatabase
		 {
			  internal TestEmbeddedGraphDatabase( File storeDir, IDictionary<string, string> @params ) : base( storeDir, @params, Dependencies() )
			  {
			  }

			  internal static GraphDatabaseFacadeFactory.Dependencies Dependencies()
			  {
					GraphDatabaseFactoryState state = new GraphDatabaseFactoryState();
					return state.DatabaseDependencies();
			  }
		 }
	}

}