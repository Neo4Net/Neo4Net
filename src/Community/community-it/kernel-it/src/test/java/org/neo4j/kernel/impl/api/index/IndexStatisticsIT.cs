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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using UncloseableDelegatingFileSystemAbstraction = Neo4Net.Graphdb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using IndexDefinition = Neo4Net.Graphdb.schema.IndexDefinition;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using IndexSamplingController = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingController;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using SchemaStorage = Neo4Net.Kernel.impl.store.SchemaStorage;
	using CountsTracker = Neo4Net.Kernel.impl.store.counts.CountsTracker;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using Register_DoubleLongRegister = Neo4Net.Register.Register_DoubleLongRegister;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.index_background_sampling_enabled;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.register.Registers.newDoubleLongRegister;

	public class IndexStatisticsIT
	{
		 private static readonly Label _alien = label( "Alien" );
		 private const string SPECIMEN = "specimen";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fsRule = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule FsRule = new EphemeralFileSystemRule();
		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider();
		 private GraphDatabaseService _db;
		 private EphemeralFileSystemAbstraction _fileSystem;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _fileSystem = FsRule.get();
			  StartDb();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverIndexCountsBySamplingThemOnStartup()
		 public virtual void ShouldRecoverIndexCountsBySamplingThemOnStartup()
		 {
			  // given some aliens in a database
			  CreateAliens();

			  // that have been indexed
			  AwaitIndexOnline( IndexAliensBySpecimen() );

			  // where ALIEN and SPECIMEN are both the first ids of their kind
			  IndexDescriptor index = TestIndexDescriptorFactory.forLabel( LabelId( _alien ), PkId( SPECIMEN ) );
			  SchemaStorage storage = new SchemaStorage( NeoStores().SchemaStore );
			  long indexId = storage.IndexGetForSchema( index ).Id;

			  // for which we don't have index counts
			  ResetIndexCounts( indexId );

			  // when we shutdown the database and restart it
			  Restart();

			  // then we should have re-sampled the index
			  CountsTracker tracker = NeoStores().Counts;
			  AssertEqualRegisters( "Unexpected updates and size for the index", newDoubleLongRegister( 0, 32 ), tracker.IndexUpdatesAndSize( indexId, newDoubleLongRegister() ) );
			  AssertEqualRegisters( "Unexpected sampling result", newDoubleLongRegister( 16, 32 ), tracker.IndexSample( indexId, newDoubleLongRegister() ) );

			  // and also
			  AssertLogExistsForRecoveryOn( ":Alien(specimen)" );
		 }

		 private void AssertEqualRegisters( string message, Register_DoubleLongRegister expected, Register_DoubleLongRegister actual )
		 {
			  assertEquals( message + " (first part of register)", expected.ReadFirst(), actual.ReadFirst() );
			  assertEquals( message + " (second part of register)", expected.ReadSecond(), actual.ReadSecond() );
		 }

		 private void AssertLogExistsForRecoveryOn( string labelAndProperty )
		 {
			  _logProvider.assertAtLeastOnce( inLog( typeof( IndexSamplingController ) ).debug( "Recovering index sampling for index %s", labelAndProperty ) );
		 }

		 private int LabelId( Label alien )
		 {
			  using ( Transaction ignore = _db.beginTx() )
			  {
					return Ktx().tokenRead().nodeLabel(alien.Name());
			  }
		 }

		 private int PkId( string propertyName )
		 {
			  using ( Transaction ignore = _db.beginTx() )
			  {
					return Ktx().tokenRead().propertyKey(propertyName);
			  }
		 }

		 private KernelTransaction Ktx()
		 {
			  return ( ( GraphDatabaseAPI ) _db ).DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( true );
		 }

		 private void CreateAliens()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					for ( int i = 0; i < 32; i++ )
					{
						 Node alien = _db.createNode( _alien );
						 alien.SetProperty( SPECIMEN, i / 2 );
					}
					tx.Success();
			  }
		 }

		 private void AwaitIndexOnline( IndexDefinition definition )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.schema().awaitIndexOnline(definition, 10, TimeUnit.SECONDS);
					tx.Success();
			  }
		 }

		 private IndexDefinition IndexAliensBySpecimen()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					IndexDefinition definition = _db.schema().indexFor(_alien).on(SPECIMEN).create();
					tx.Success();
					return definition;
			  }
		 }

		 private void ResetIndexCounts( long indexId )
		 {
			  using ( Neo4Net.Kernel.Impl.Api.CountsAccessor_IndexStatsUpdater updater = NeoStores().Counts.updateIndexCounts() )
			  {
					updater.ReplaceIndexSample( indexId, 0, 0 );
					updater.ReplaceIndexUpdateAndSize( indexId, 0, 0 );
			  }
		 }

		 private NeoStores NeoStores()
		 {
			  return ( ( GraphDatabaseAPI ) _db ).DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores();
		 }

		 private void StartDb()
		 {
			  _db = ( new TestGraphDatabaseFactory() ).setInternalLogProvider(_logProvider).setFileSystem(new UncloseableDelegatingFileSystemAbstraction(_fileSystem)).newImpermanentDatabaseBuilder().setConfig(index_background_sampling_enabled, "false").newGraphDatabase();
		 }

		 internal virtual void Restart()
		 {
			  _db.shutdown();
			  StartDb();
		 }
	}

}