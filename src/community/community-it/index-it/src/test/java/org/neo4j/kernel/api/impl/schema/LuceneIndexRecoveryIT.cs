using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Api.Impl.Schema
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using InternalIndexState = Neo4Net.Kernel.Api.Internal.InternalIndexState;
	using DirectoryFactory = Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using ExtensionType = Neo4Net.Kernel.extension.ExtensionType;
	using Neo4Net.Kernel.extension;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using LogRotation = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asUniqueSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.impl.schema.LuceneIndexProvider.defaultDirectoryStructure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.impl.schema.LuceneIndexProviderFactory.PROVIDER_DESCRIPTOR;

	public class LuceneIndexRecoveryIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.fs.EphemeralFileSystemRule fs = new org.Neo4Net.test.rule.fs.EphemeralFileSystemRule();
		 public EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();

		 private readonly string _numBananasKey = "number_of_bananas_owned";
		 private static readonly Label _myLabel = label( "MyLabel" );
		 private GraphDatabaseAPI _db;
		 private DirectoryFactory _directoryFactory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _directoryFactory = new Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory_InMemoryDirectoryFactory();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void After()
		 {
			  if ( _db != null )
			  {
					_db.shutdown();
			  }
			  _directoryFactory.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addShouldBeIdempotentWhenDoingRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AddShouldBeIdempotentWhenDoingRecovery()
		 {
			  // Given
			  StartDb( CreateLuceneIndexFactory() );

			  IndexDefinition index = CreateIndex( _myLabel );
			  WaitForIndex( index );

			  long nodeId = CreateNode( _myLabel, 12 );
			  using ( Transaction ignored = _db.beginTx() )
			  {
					assertNotNull( _db.getNodeById( nodeId ) );
			  }
			  assertEquals( 1, DoIndexLookup( _myLabel, 12 ).Count );

			  // And Given
			  KillDb();

			  // When
			  StartDb( CreateLuceneIndexFactory() );

			  // Then
			  using ( Transaction ignored = _db.beginTx() )
			  {
					assertNotNull( _db.getNodeById( nodeId ) );
			  }
			  assertEquals( 1, DoIndexLookup( _myLabel, 12 ).Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void changeShouldBeIdempotentWhenDoingRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ChangeShouldBeIdempotentWhenDoingRecovery()
		 {
			  // Given
			  StartDb( CreateLuceneIndexFactory() );

			  IndexDefinition indexDefinition = CreateIndex( _myLabel );
			  WaitForIndex( indexDefinition );

			  long node = CreateNode( _myLabel, 12 );
			  RotateLogsAndCheckPoint();

			  UpdateNode( node, 13 );

			  // And Given
			  KillDb();

			  // When
			  StartDb( CreateLuceneIndexFactory() );

			  // Then
			  assertEquals( 0, DoIndexLookup( _myLabel, 12 ).Count );
			  assertEquals( 1, DoIndexLookup( _myLabel, 13 ).Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removeShouldBeIdempotentWhenDoingRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RemoveShouldBeIdempotentWhenDoingRecovery()
		 {
			  // Given
			  StartDb( CreateLuceneIndexFactory() );

			  IndexDefinition indexDefinition = CreateIndex( _myLabel );
			  WaitForIndex( indexDefinition );

			  long node = CreateNode( _myLabel, 12 );
			  RotateLogsAndCheckPoint();

			  DeleteNode( node );

			  // And Given
			  KillDb();

			  // When
			  StartDb( CreateLuceneIndexFactory() );

			  // Then
			  assertEquals( 0, DoIndexLookup( _myLabel, 12 ).Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAddTwiceDuringRecoveryIfCrashedDuringPopulation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAddTwiceDuringRecoveryIfCrashedDuringPopulation()
		 {
			  // Given
			  StartDb( CreateAlwaysInitiallyPopulatingLuceneIndexFactory() );

			  IndexDefinition indexDefinition = CreateIndex( _myLabel );
			  WaitForIndex( indexDefinition );

			  long nodeId = CreateNode( _myLabel, 12 );
			  assertEquals( 1, DoIndexLookup( _myLabel, 12 ).Count );

			  // And Given
			  KillDb();

			  // When
			  StartDb( CreateAlwaysInitiallyPopulatingLuceneIndexFactory() );

			  using ( Transaction ignored = _db.beginTx() )
			  {
					IndexDefinition index = _db.schema().Indexes.GetEnumerator().next();
					WaitForIndex( index );

					// Then
					assertEquals( 12, _db.getNodeById( nodeId ).getProperty( _numBananasKey ) );
					assertEquals( 1, DoIndexLookup( _myLabel, 12 ).Count );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotUpdateTwiceDuringRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotUpdateTwiceDuringRecovery()
		 {
			  // Given
			  StartDb( CreateLuceneIndexFactory() );

			  IndexDefinition indexDefinition = CreateIndex( _myLabel );
			  WaitForIndex( indexDefinition );

			  long nodeId = CreateNode( _myLabel, 12 );
			  UpdateNode( nodeId, 14 );

			  // And Given
			  KillDb();

			  // When
			  StartDb( CreateLuceneIndexFactory() );

			  // Then
			  assertEquals( 0, DoIndexLookup( _myLabel, 12 ).Count );
			  assertEquals( 1, DoIndexLookup( _myLabel, 14 ).Count );
		 }

		 private void StartDb<T1>( KernelExtensionFactory<T1> indexProviderFactory )
		 {
			  if ( _db != null )
			  {
					_db.shutdown();
			  }

			  TestGraphDatabaseFactory factory = new TestGraphDatabaseFactory();
			  factory.FileSystem = Fs.get();
			  factory.KernelExtensions = Collections.singletonList( indexProviderFactory );
			  _db = ( GraphDatabaseAPI ) factory.NewImpermanentDatabaseBuilder().setConfig(default_schema_provider, PROVIDER_DESCRIPTOR.name()).newGraphDatabase();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void killDb() throws Exception
		 private void KillDb()
		 {
			  if ( _db != null )
			  {
					Fs.snapshot(() =>
					{
					 _db.shutdown();
					 _db = null;
					});
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void rotateLogsAndCheckPoint() throws java.io.IOException
		 private void RotateLogsAndCheckPoint()
		 {
			  _db.DependencyResolver.resolveDependency( typeof( LogRotation ) ).rotateLogFile();
			  _db.DependencyResolver.resolveDependency( typeof( CheckPointer ) ).forceCheckPoint( new SimpleTriggerInfo( "test" ) );
		 }

		 private IndexDefinition CreateIndex( Label label )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					IndexDefinition definition = _db.schema().indexFor(label).on(_numBananasKey).create();
					tx.Success();
					return definition;
			  }
		 }

		 private void WaitForIndex( IndexDefinition definition )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().awaitIndexOnline(definition, 10, TimeUnit.SECONDS);
					tx.Success();
			  }
		 }

		 private ISet<Node> DoIndexLookup( Label myLabel, object value )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					IEnumerator<Node> iter = _db.findNodes( myLabel, _numBananasKey, value );
					ISet<Node> nodes = asUniqueSet( iter );
					tx.Success();
					return nodes;
			  }
		 }

		 private long CreateNode( Label label, int number )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.createNode( label );
					node.SetProperty( _numBananasKey, number );
					tx.Success();
					return node.Id;
			  }
		 }

		 private void UpdateNode( long nodeId, int value )
		 {

			  using ( Transaction tx = _db.beginTx() )
			  {
					Node node = _db.getNodeById( nodeId );
					node.SetProperty( _numBananasKey, value );
					tx.Success();
			  }
		 }

		 private void DeleteNode( long node )
		 {

			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.getNodeById( node ).delete();
					tx.Success();
			  }
		 }

		 private KernelExtensionFactory<LuceneIndexProviderFactory.Dependencies> CreateAlwaysInitiallyPopulatingLuceneIndexFactory()
		 {
			  return new KernelExtensionFactoryAnonymousInnerClass( this, PROVIDER_DESCRIPTOR.Key );
		 }

		 private class KernelExtensionFactoryAnonymousInnerClass : KernelExtensionFactory<LuceneIndexProviderFactory.Dependencies>
		 {
			 private readonly LuceneIndexRecoveryIT _outerInstance;

			 public KernelExtensionFactoryAnonymousInnerClass( LuceneIndexRecoveryIT outerInstance, UnknownType getKey ) : base( ExtensionType.DATABASE, getKey )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override Lifecycle newInstance( KernelContext context, LuceneIndexProviderFactory.Dependencies dependencies )
			 {
				  return new LuceneIndexProviderAnonymousInnerClass( this, _outerInstance.fs.get(), _outerInstance.directoryFactory, defaultDirectoryStructure(context.Directory()), IndexProvider.Monitor_Fields.EMPTY, dependencies.Config );
			 }

			 private class LuceneIndexProviderAnonymousInnerClass : LuceneIndexProvider
			 {
				 private readonly KernelExtensionFactoryAnonymousInnerClass _outerInstance;

				 public LuceneIndexProviderAnonymousInnerClass( KernelExtensionFactoryAnonymousInnerClass outerInstance, UnknownType get, DirectoryFactory directoryFactory, UnknownType defaultDirectoryStructure, IndexProvider.Monitor empty, UnknownType getConfig ) : base( get, directoryFactory, defaultDirectoryStructure, empty, getConfig, context.databaseInfo().operationalMode )
				 {
					 this.outerInstance = outerInstance;
				 }

				 public override InternalIndexState getInitialState( StoreIndexDescriptor descriptor )
				 {
					  return InternalIndexState.POPULATING;
				 }
			 }
		 }

		 // Creates a lucene index factory with the shared in-memory directory
		 private KernelExtensionFactory<LuceneIndexProviderFactory.Dependencies> CreateLuceneIndexFactory()
		 {
			  return new KernelExtensionFactoryAnonymousInnerClass2( this, PROVIDER_DESCRIPTOR.Key );
		 }

		 private class KernelExtensionFactoryAnonymousInnerClass2 : KernelExtensionFactory<LuceneIndexProviderFactory.Dependencies>
		 {
			 private readonly LuceneIndexRecoveryIT _outerInstance;

			 public KernelExtensionFactoryAnonymousInnerClass2( LuceneIndexRecoveryIT outerInstance, UnknownType getKey ) : base( ExtensionType.DATABASE, getKey )
			 {
				 this.outerInstance = outerInstance;
			 }


			 public override Lifecycle newInstance( KernelContext context, LuceneIndexProviderFactory.Dependencies dependencies )
			 {
				  return new LuceneIndexProvider( _outerInstance.fs.get(), _outerInstance.directoryFactory, defaultDirectoryStructure(context.Directory()), IndexProvider.Monitor_Fields.EMPTY, dependencies.Config, context.DatabaseInfo().OperationalMode );
			 }
		 }
	}

}