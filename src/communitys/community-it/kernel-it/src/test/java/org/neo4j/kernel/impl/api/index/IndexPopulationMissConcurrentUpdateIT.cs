using System;
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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseFactory = Neo4Net.Graphdb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using IndexCapability = Neo4Net.@internal.Kernel.Api.IndexCapability;
	using InternalIndexState = Neo4Net.@internal.Kernel.Api.InternalIndexState;
	using IndexProviderDescriptor = Neo4Net.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using ExtensionType = Neo4Net.Kernel.extension.ExtensionType;
	using Neo4Net.Kernel.extension;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using ByteBufferFactory = Neo4Net.Kernel.Impl.Index.Schema.ByteBufferFactory;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using IdController = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using StoreMigrationParticipant = Neo4Net.Kernel.impl.storemigration.StoreMigrationParticipant;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using IndexSample = Neo4Net.Storageengine.Api.schema.IndexSample;
	using LabelScanReader = Neo4Net.Storageengine.Api.schema.LabelScanReader;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using Barrier = Neo4Net.Test.Barrier;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;
	using FeatureToggles = Neo4Net.Utils.FeatureToggles;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.filter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.InternalIndexState.POPULATING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexDirectoryStructure.directoriesByProvider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.MultipleIndexPopulator.BATCH_SIZE_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.MultipleIndexPopulator.QUEUE_THRESHOLD_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.storemigration.StoreMigrationParticipant_Fields.NOT_PARTICIPATING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.TestLabels.LABEL_ONE;

	public class IndexPopulationMissConcurrentUpdateIT
	{
		 private const string NAME_PROPERTY = "name";
		 private const long INITIAL_CREATION_NODE_ID_THRESHOLD = 30;
		 private const long SCAN_BARRIER_NODE_ID_THRESHOLD = 10;

		 private readonly ControlledSchemaIndexProvider _index = new ControlledSchemaIndexProvider();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule()
		 public final DatabaseRule db = new ImpermanentDatabaseRuleAnonymousInnerClass()
		 .withSetting( GraphDatabaseSettings.multi_threaded_schema_index_population_enabled, Settings.FALSE ).withSetting( GraphDatabaseSettings.default_schema_provider, ControlledSchemaIndexProvider.IndexProvider.name() );
		 // The single-threaded setting makes the test deterministic. The multi-threaded variant has the same problem tested below.

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setFeatureToggle()
		 public void setFeatureToggle()
		 {
			  // let our populator have fine-grained insight into updates coming in
			  FeatureToggles.set( typeof( MultipleIndexPopulator ), BATCH_SIZE_NAME, 1 );
			  FeatureToggles.set( typeof( BatchingMultipleIndexPopulator ), BATCH_SIZE_NAME, 1 );
			  FeatureToggles.set( typeof( MultipleIndexPopulator ), QUEUE_THRESHOLD_NAME, 1 );
			  FeatureToggles.set( typeof( BatchingMultipleIndexPopulator ), QUEUE_THRESHOLD_NAME, 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void resetFeatureToggle()
		 public void resetFeatureToggle()
		 {
			  FeatureToggles.clear( typeof( MultipleIndexPopulator ), BATCH_SIZE_NAME );
			  FeatureToggles.clear( typeof( BatchingMultipleIndexPopulator ), BATCH_SIZE_NAME );
			  FeatureToggles.clear( typeof( MultipleIndexPopulator ), QUEUE_THRESHOLD_NAME );
			  FeatureToggles.clear( typeof( BatchingMultipleIndexPopulator ), QUEUE_THRESHOLD_NAME );
		 }

		 /// <summary>
		 /// Tests an issue where the <seealso cref="MultipleIndexPopulator"/> had a condition when applying external concurrent updates that any given
		 /// update would only be applied if the entity id was lower than the highest entity id the scan had seen (i.e. where the scan was currently at).
		 /// This would be a problem because of how the <seealso cref="LabelScanReader"/> works internally, which is that it reads one bit-set of node ids
		 /// at the time, effectively caching a small range of ids. If a concurrent creation would happen right in front of where the scan was
		 /// after it had read and cached that bit-set it would not apply the update and miss that entity in the scan and would end up with an index
		 /// that was inconsistent with the store.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 60_000) public void shouldNoticeConcurrentUpdatesWithinCurrentLabelIndexEntryRange() throws Exception
		 public void shouldNoticeConcurrentUpdatesWithinCurrentLabelIndexEntryRange() throws Exception
		 {
			  // given nodes [0...30]. Why 30, because this test ties into a bug regarding "caching" of bit-sets in label index reader,
			  // where each bit-set is of size 64.
			  IList<Node> nodes = new List<Node>();
			  int nextId = 0;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node;
					do
					{
						 node = Db.createNode( LABEL_ONE );
						 node.SetProperty( NAME_PROPERTY, "Node " + nextId++ );
						 nodes.Add( node );
					} while ( node.Id < INITIAL_CREATION_NODE_ID_THRESHOLD );
					tx.Success();
			  }
			  assertThat( "At least one node below the scan barrier threshold must have been created, otherwise test assumptions are invalid or outdated", count( filter( n => n.Id <= SCAN_BARRIER_NODE_ID_THRESHOLD, nodes ) ), greaterThan( 0L ) );
			  assertThat( "At least two nodes above the scan barrier threshold and below initial creation threshold must have been created, " + "otherwise test assumptions are invalid or outdated", count( filter( n => n.Id > SCAN_BARRIER_NODE_ID_THRESHOLD, nodes ) ), greaterThan( 1L ) );
			  Db.DependencyResolver.resolveDependency( typeof( IdController ) ).maintenance();

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(LABEL_ONE).on(NAME_PROPERTY).create();
					tx.Success();
			  }

			  _index.barrier.await();
			  // Now the index population has come some way into the first bit-set entry of the label index
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node;
					do
					{
						 node = Db.createNode( LABEL_ONE );
						 node.SetProperty( NAME_PROPERTY, nextId++ );
						 nodes.Add( node );
					} while ( node.Id < _index.populationAtId );
					// here we know that we have created a node in front of the index populator and also inside the cached bit-set of the label index reader
					tx.Success();
			  }
			  _index.barrier.release();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, MINUTES);
					tx.Success();
			  }

			  // then all nodes must be in the index
			  assertEquals( nodes.Count, _index.entitiesByScan.Count + _index.entitiesByUpdater.Count );
			  using ( Transaction tx = Db.beginTx() )
			  {
					foreach ( Node node in Db.AllNodes )
					{
						 assertTrue( _index.entitiesByScan.Contains( node.Id ) || _index.entitiesByUpdater.Contains( node.Id ) );
					}
					tx.Success();
			  }
		 }

		 /// <summary>
		 /// A very specific <seealso cref="IndexProvider"/> which can be paused and continued at juuust the right places.
		 /// </summary>
		 private static class ControlledSchemaIndexProvider extends KernelExtensionFactory<System.Func>
		 {
			  private final Neo4Net.Test.Barrier_Control barrier = new Neo4Net.Test.Barrier_Control();
			  private final ISet<long> entitiesByScan = new ConcurrentSkipListSet<>();
			  private final ISet<long> entitiesByUpdater = new ConcurrentSkipListSet<>();
			  private volatile long populationAtId;
			  static IndexProviderDescriptor INDEX_PROVIDER = new IndexProviderDescriptor( "controlled", "1" );

			  ControlledSchemaIndexProvider()
			  {
					base( ExtensionType.DATABASE, "controlled" );
			  }

			  public Lifecycle newInstance( KernelContext context, System.Func noDependencies )
			  {
					return new IndexProviderAnonymousInnerClass( this, INDEX_PROVIDER, directoriesByProvider( new File( "not-even-persistent" ) ) );
			  }
		 }
	}

}