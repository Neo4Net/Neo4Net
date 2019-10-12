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
namespace Org.Neo4j.Consistency.checking.full
{
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;

	using ConsistencySummaryStatistics = Org.Neo4j.Consistency.report.ConsistencySummaryStatistics;
	using Statistics = Org.Neo4j.Consistency.statistics.Statistics;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using ProgressMonitorFactory = Org.Neo4j.Helpers.progress.ProgressMonitorFactory;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using DirectStoreAccess = Org.Neo4j.Kernel.api.direct.DirectStoreAccess;
	using LabelScanStore = Org.Neo4j.Kernel.api.labelscan.LabelScanStore;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using MyRelTypes = Org.Neo4j.Kernel.impl.MyRelTypes;
	using IndexProviderMap = Org.Neo4j.Kernel.Impl.Api.index.IndexProviderMap;
	using TokenHolders = Org.Neo4j.Kernel.impl.core.TokenHolders;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using RelationshipStore = Org.Neo4j.Kernel.impl.store.RelationshipStore;
	using StoreAccess = Org.Neo4j.Kernel.impl.store.StoreAccess;
	using StoreFactory = Org.Neo4j.Kernel.impl.store.StoreFactory;
	using StoreType = Org.Neo4j.Kernel.impl.store.StoreType;
	using DefaultIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using RecordLoad = Org.Neo4j.Kernel.impl.store.record.RecordLoad;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	public class DetectAllRelationshipInconsistenciesIT
	{
		private bool InstanceFieldsInitialized = false;

		public DetectAllRelationshipInconsistenciesIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Rules = RuleChain.outerRule( _random ).around( _directory ).around( _fileSystemRule );
		}

		 private readonly TestDirectory _directory = TestDirectory.testDirectory();
		 private readonly RandomRule _random = new RandomRule();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain rules = org.junit.rules.RuleChain.outerRule(random).around(directory).around(fileSystemRule);
		 public RuleChain Rules;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectSabotagedRelationshipWhereEverItIs() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectSabotagedRelationshipWhereEverItIs()
		 {
			  // GIVEN a database which lots of relationships
			  GraphDatabaseAPI db = GraphDatabaseAPI;
			  Sabotage sabotage;
			  try
			  {
					Node[] nodes = new Node[1_000];
					Relationship[] relationships = new Relationship[10_000];
					using ( Transaction tx = Db.beginTx() )
					{
						 for ( int i = 0; i < nodes.Length; i++ )
						 {
							  nodes[i] = Db.createNode( label( "Foo" ) );
						 }
						 for ( int i = 0; i < 10_000; i++ )
						 {
							  relationships[i] = _random.among( nodes ).createRelationshipTo( _random.among( nodes ), MyRelTypes.TEST );
						 }
						 tx.Success();
					}

					// WHEN sabotaging a random relationship
					DependencyResolver resolver = Db.DependencyResolver;
					PageCache pageCache = resolver.ResolveDependency( typeof( PageCache ) );

					StoreFactory storeFactory = NewStoreFactory( pageCache );

					using ( NeoStores neoStores = storeFactory.OpenNeoStores( false, StoreType.RELATIONSHIP ) )
					{
						 RelationshipStore relationshipStore = neoStores.RelationshipStore;
						 Relationship sabotagedRelationships = _random.among( relationships );
						 sabotage = sabotage( relationshipStore, sabotagedRelationships.Id );
					}
			  }
			  finally
			  {
					Db.shutdown();
			  }

			  // THEN the checker should find it, where ever it is in the store
			  db = GraphDatabaseAPI;
			  try
			  {
					DependencyResolver resolver = Db.DependencyResolver;
					PageCache pageCache = resolver.ResolveDependency( typeof( PageCache ) );
					StoreFactory storeFactory = NewStoreFactory( pageCache );

					using ( NeoStores neoStores = storeFactory.OpenAllNeoStores() )
					{
						 StoreAccess storeAccess = ( new StoreAccess( neoStores ) ).initialize();
						 DirectStoreAccess directStoreAccess = new DirectStoreAccess( storeAccess, Db.DependencyResolver.resolveDependency( typeof( LabelScanStore ) ), Db.DependencyResolver.resolveDependency( typeof( IndexProviderMap ) ), Db.DependencyResolver.resolveDependency( typeof( TokenHolders ) ) );

						 int threads = _random.intBetween( 2, 10 );
						 FullCheck checker = new FullCheck( TuningConfiguration, ProgressMonitorFactory.NONE, Statistics.NONE, threads, true );
						 AssertableLogProvider logProvider = new AssertableLogProvider( true );
						 ConsistencySummaryStatistics summary = checker.Execute( directStoreAccess, logProvider.GetLog( typeof( FullCheck ) ) );
						 int relationshipInconsistencies = summary.GetInconsistencyCountForRecordType( RecordType.RELATIONSHIP );

						 assertTrue( "Couldn't detect sabotaged relationship " + sabotage, relationshipInconsistencies > 0 );
						 logProvider.RawMessageMatcher().assertContains(sabotage.After.ToString());
					}
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 private StoreFactory NewStoreFactory( PageCache pageCache )
		 {
			  FileSystemAbstraction fileSystem = _fileSystemRule.get();
			  return new StoreFactory( _directory.databaseLayout(), TuningConfiguration, new DefaultIdGeneratorFactory(fileSystem), pageCache, fileSystem, NullLogProvider.Instance, EmptyVersionContextSupplier.EMPTY );
		 }

		 private Config TuningConfiguration
		 {
			 get
			 {
				  return Config.defaults( stringMap( GraphDatabaseSettings.pagecache_memory.name(), "8m", GraphDatabaseSettings.record_format.name(), RecordFormatName ) );
			 }
		 }

		 private GraphDatabaseAPI GraphDatabaseAPI
		 {
			 get
			 {
				  TestGraphDatabaseFactory factory = new TestGraphDatabaseFactory();
				  GraphDatabaseService database = factory.NewEmbeddedDatabaseBuilder( _directory.databaseDir() ).setConfig(GraphDatabaseSettings.record_format, RecordFormatName).setConfig("dbms.backup.enabled", "false").newGraphDatabase();
				  return ( GraphDatabaseAPI ) database;
			 }
		 }

		 protected internal virtual string RecordFormatName
		 {
			 get
			 {
				  return StringUtils.EMPTY;
			 }
		 }

		 private class Sabotage
		 {
			  internal readonly RelationshipRecord Before;
			  internal readonly RelationshipRecord After;
			  internal readonly RelationshipRecord Other;

			  internal Sabotage( RelationshipRecord before, RelationshipRecord after, RelationshipRecord other )
			  {
					this.Before = before;
					this.After = after;
					this.Other = other;
			  }

			  public override string ToString()
			  {
					return "Sabotaged " + Before + " --> " + After + ", other relationship " + Other;
			  }
		 }

		 private Sabotage Sabotage( RelationshipStore store, long id )
		 {
			  RelationshipRecord before = store.GetRecord( id, store.NewRecord(), RecordLoad.NORMAL );
			  RelationshipRecord after = before.Clone();

			  long otherReference;
			  if ( !after.FirstInFirstChain )
			  {
					after.FirstPrevRel = otherReference = after.FirstPrevRel + 1;
			  }
			  else
			  {
					after.FirstNextRel = otherReference = after.FirstNextRel + 1;
			  }

			  store.PrepareForCommit( after );
			  store.UpdateRecord( after );

			  RelationshipRecord other = store.GetRecord( otherReference, store.NewRecord(), RecordLoad.FORCE );
			  return new Sabotage( before, after, other );
		 }
	}

}