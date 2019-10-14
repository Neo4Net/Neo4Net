using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

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
namespace Neo4Net.Consistency.checking.full
{
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using MutableInt = org.apache.commons.lang3.mutable.MutableInt;
	using AfterClass = org.junit.AfterClass;
	using BeforeClass = org.junit.BeforeClass;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using Applier = Neo4Net.Consistency.checking.GraphStoreFixture.Applier;
	using IdGenerator = Neo4Net.Consistency.checking.GraphStoreFixture.IdGenerator;
	using TransactionDataBuilder = Neo4Net.Consistency.checking.GraphStoreFixture.TransactionDataBuilder;
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using ConsistencySummaryStatistics = Neo4Net.Consistency.report.ConsistencySummaryStatistics;
	using Neo4Net.Functions;
	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using Direction = Neo4Net.Graphdb.Direction;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using Neo4Net.Helpers.Collections;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using TokenRead = Neo4Net.@internal.Kernel.Api.TokenRead;
	using TokenWrite = Neo4Net.@internal.Kernel.Api.TokenWrite;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using IndexProviderDescriptor = Neo4Net.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Statement = Neo4Net.Kernel.api.Statement;
	using DirectStoreAccess = Neo4Net.Kernel.api.direct.DirectStoreAccess;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using LabelScanStore = Neo4Net.Kernel.api.labelscan.LabelScanStore;
	using LabelScanWriter = Neo4Net.Kernel.api.labelscan.LabelScanWriter;
	using NodeLabelUpdate = Neo4Net.Kernel.api.labelscan.NodeLabelUpdate;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using EntityUpdates = Neo4Net.Kernel.Impl.Api.index.EntityUpdates;
	using IndexUpdateMode = Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using LockService = Neo4Net.Kernel.impl.locking.LockService;
	using AbstractDynamicStore = Neo4Net.Kernel.impl.store.AbstractDynamicStore;
	using DynamicRecordAllocator = Neo4Net.Kernel.impl.store.DynamicRecordAllocator;
	using NodeLabelsField = Neo4Net.Kernel.impl.store.NodeLabelsField;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using PropertyType = Neo4Net.Kernel.impl.store.PropertyType;
	using Neo4Net.Kernel.impl.store;
	using SchemaStorage = Neo4Net.Kernel.impl.store.SchemaStorage;
	using SchemaStore = Neo4Net.Kernel.impl.store.SchemaStore;
	using StoreAccess = Neo4Net.Kernel.impl.store.StoreAccess;
	using ReusableRecordsAllocator = Neo4Net.Kernel.impl.store.allocator.ReusableRecordsAllocator;
	using ReusableRecordsCompositeAllocator = Neo4Net.Kernel.impl.store.allocator.ReusableRecordsCompositeAllocator;
	using ConstraintRule = Neo4Net.Kernel.impl.store.record.ConstraintRule;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.impl.store.record.LabelTokenRecord;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using PropertyBlock = Neo4Net.Kernel.impl.store.record.PropertyBlock;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using Record = Neo4Net.Kernel.impl.store.record.Record;
	using RelationshipGroupRecord = Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.impl.store.record.RelationshipTypeTokenRecord;
	using SchemaRuleSerialization = Neo4Net.Kernel.impl.store.record.SchemaRuleSerialization;
	using NeoStoreIndexStoreView = Neo4Net.Kernel.impl.transaction.state.storeview.NeoStoreIndexStoreView;
	using Bits = Neo4Net.Kernel.impl.util.Bits;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using FormattedLog = Neo4Net.Logging.FormattedLog;
	using SchemaRule = Neo4Net.Storageengine.Api.schema.SchemaRule;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using UTF8 = Neo4Net.Strings.UTF8;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.ConsistencyCheckService.defaultConsistencyCheckThreadsNumber;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.RecordCheckTestBase.inUse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.RecordCheckTestBase.notInUse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.SchemaRuleUtil.constraintIndexRule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.SchemaRuleUtil.indexRule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.SchemaRuleUtil.nodePropertyExistenceConstraintRule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.SchemaRuleUtil.relPropertyExistenceConstraintRule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.SchemaRuleUtil.uniquenessConstraintRule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.FullCheckIntegrationTest.ConsistencySummaryVerifier.on;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asIterable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.StatementConstants.ANY_LABEL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.StatementConstants.ANY_RELATIONSHIP_TYPE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.labelscan.NodeLabelUpdate.labelChanges;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.ByteBufferFactory.heapBufferFactory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.AbstractDynamicStore.readFullByteArrayFromHeavyRecords;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.DynamicArrayStore.allocateFromNumbers;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.DynamicArrayStore.getRightArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.DynamicNodeLabels.dynamicPointer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.LabelIdArray.prependNodeId;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.PropertyType.ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.Record.NO_LABELS_FIELD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.Record.NO_NEXT_PROPERTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.Record.NO_PREV_RELATIONSHIP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.FORCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.Bits.bits;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptorFactory.forSchema;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptorFactory.uniqueForSchema;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.Property.property;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.Property.set;

	public class FullCheckIntegrationTest
	{
		private bool InstanceFieldsInitialized = false;

		public FullCheckIntegrationTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _suppressOutput ).around( fixture );
		}

		 private static readonly IndexProviderDescriptor _descriptor = new IndexProviderDescriptor( "lucene", "1.0" );
		 private const string PROP1 = "key1";
		 private const string PROP2 = "key2";
		 private const object VALUE1 = "value1";
		 private const object VALUE2 = "value2";

		 private int _label1;
		 private int _label2;
		 private int _label3;
		 private int _label4;
		 private int _draconian;
		 private int _key1;
		 private int _key2;
		 private int _mandatory;
		 private int _c;
		 private int _t;
		 private int _m;

		 private readonly IList<long> _indexedNodes = new List<long>();

		 private static readonly IDictionary<Type, ISet<string>> _allReports = new Dictionary<Type, ISet<string>>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void collectAllDifferentInconsistencyTypes()
		 public static void CollectAllDifferentInconsistencyTypes()
		 {
			  Type reportClass = typeof( ConsistencyReport );
			  foreach ( Type cls in reportClass.GetNestedTypes( BindingFlags.Public | BindingFlags.NonPublic ) )
			  {
					foreach ( System.Reflection.MethodInfo method in cls.GetMethods( BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance ) )
					{
						 if ( method.getAnnotation( typeof( Documented ) ) != null )
						 {
							  ISet<string> types = _allReports.computeIfAbsent( cls, k => new HashSet<string>() );
							  types.Add( method.Name );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void verifyThatWeHaveExercisedAllTypesOfInconsistenciesThatWeHave()
		 public static void VerifyThatWeHaveExercisedAllTypesOfInconsistenciesThatWeHave()
		 {
			  if ( _allReports.Count > 0 )
			  {
					StringBuilder builder = new StringBuilder( "There are types of inconsistencies not covered by " + "this integration test, please add tests that tests for:" );
					foreach ( KeyValuePair<Type, ISet<string>> reporter in _allReports.SetOfKeyValuePairs() )
					{
						 builder.Append( format( "%n%s:", reporter.Key.SimpleName ) );
						 foreach ( string type in reporter.Value )
						 {
							  builder.Append( format( "%n  %s", type ) );
						 }
					}
					Console.Error.WriteLine( builder.ToString() );
			  }
		 }

		 private GraphStoreFixture fixture = new GraphStoreFixtureAnonymousInnerClass( RecordFormatName );

		 private class GraphStoreFixtureAnonymousInnerClass : GraphStoreFixture
		 {
			 public GraphStoreFixtureAnonymousInnerClass( string getRecordFormatName ) : base( getRecordFormatName )
			 {
			 }

			 protected internal override void generateInitialData( GraphDatabaseService db )
			 {
				  try
				  {
						  using ( Neo4Net.Graphdb.Transaction tx = Db.beginTx() )
						  {
							Db.schema().indexFor(label("label3")).on(PROP1).create();
							KernelTransaction ktx = TransactionOn( db );
							using ( Statement ignore = ktx.AcquireStatement() )
							{
								 // the Core API for composite index creation is not quite merged yet
								 TokenWrite tokenWrite = ktx.TokenWrite();
								 outerInstance.key1 = tokenWrite.PropertyKeyGetOrCreateForName( PROP1 );
								 outerInstance.key2 = tokenWrite.PropertyKeyGetOrCreateForName( PROP2 );
								 outerInstance.label3 = ktx.TokenRead().nodeLabel("label3");
								 ktx.SchemaWrite().indexCreate(forLabel(outerInstance.label3, outerInstance.key1, outerInstance.key2));
							}

							Db.schema().constraintFor(label("label4")).assertPropertyIsUnique(PROP1).create();
							tx.Success();
						  }
				  }
				  catch ( KernelException e )
				  {
						throw new Exception( e );
				  }

				  using ( Neo4Net.Graphdb.Transaction ignored = Db.beginTx() )
				  {
						Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
				  }

				  try
				  {
						  using ( Neo4Net.Graphdb.Transaction tx = Db.beginTx() )
						  {
							Node node1 = set( Db.createNode( label( "label1" ) ) );
							Node node2 = set( Db.createNode( label( "label2" ) ), property( PROP1, VALUE1 ) );
							node1.CreateRelationshipTo( node2, withName( "C" ) );
							// Just to create one more rel type
							Db.createNode().createRelationshipTo(Db.createNode(), withName("T"));
							outerInstance.indexedNodes.Add( set( Db.createNode( label( "label3" ) ), property( PROP1, VALUE1 ) ).Id );
							outerInstance.indexedNodes.Add( set( Db.createNode( label( "label3" ) ), property( PROP1, VALUE1 ), property( PROP2, VALUE2 ) ).Id );

							set( Db.createNode( label( "label4" ) ), property( PROP1, VALUE1 ) );
							tx.Success();

							KernelTransaction ktx = TransactionOn( db );
							using ( Statement ignore = ktx.AcquireStatement() )
							{
								 TokenRead tokenRead = ktx.TokenRead();
								 TokenWrite tokenWrite = ktx.TokenWrite();
								 outerInstance.label1 = tokenRead.NodeLabel( "label1" );
								 outerInstance.label2 = tokenRead.NodeLabel( "label2" );
								 outerInstance.label3 = tokenRead.NodeLabel( "label3" );
								 outerInstance.label4 = tokenRead.NodeLabel( "label4" );
								 outerInstance.draconian = tokenWrite.LabelGetOrCreateForName( "draconian" );
								 outerInstance.key1 = tokenRead.PropertyKey( PROP1 );
								 outerInstance.mandatory = tokenWrite.PropertyKeyGetOrCreateForName( "mandatory" );
								 outerInstance.C = tokenRead.RelationshipType( "C" );
								 outerInstance.T = tokenRead.RelationshipType( "T" );
								 outerInstance.M = tokenWrite.RelationshipTypeGetOrCreateForName( "M" );
							}
						  }
				  }
				  catch ( KernelException e )
				  {
						throw new Exception( e );
				  }
			 }
		 }

		 private readonly SuppressOutput _suppressOutput = SuppressOutput.suppress( SuppressOutput.System.out );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(suppressOutput).around(fixture);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCheckConsistencyOfAConsistentStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCheckConsistencyOfAConsistentStore()
		 {
			  // when
			  ConsistencySummaryStatistics result = Check();

			  // then
			  assertEquals( result.ToString(), 0, result.TotalInconsistencyCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNodeInconsistencies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportNodeInconsistencies()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.NODE, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  tx.Create( new NodeRecord( next.Node(), false, next.Relationship(), -1 ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportInlineNodeLabelInconsistencies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportInlineNodeLabelInconsistencies()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass2( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.NODE, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass2 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass2( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  NodeRecord nodeRecord = new NodeRecord( next.Node(), false, -1, -1 );
				  NodeLabelsField.parseLabelsField( nodeRecord ).add( 10, null, null );
				  tx.Create( nodeRecord );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNodeDynamicLabelContainingUnknownLabelAsNodeInconsistency() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportNodeDynamicLabelContainingUnknownLabelAsNodeInconsistency()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass3( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.NODE, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass3 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass3( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  NodeRecord nodeRecord = new NodeRecord( next.Node(), false, -1, -1 );
				  DynamicRecord record = inUse( new DynamicRecord( next.NodeLabel() ) );
				  ICollection<DynamicRecord> newRecords = new List<DynamicRecord>();
				  allocateFromNumbers( newRecords, prependNodeId( nodeRecord.Id, new long[]{ 42L } ), new ReusableRecordsAllocator( 60, record ) );
				  nodeRecord.SetLabelField( dynamicPointer( newRecords ), newRecords );

				  tx.Create( nodeRecord );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReportAnythingForNodeWithConsistentChainOfDynamicRecordsWithLabels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotReportAnythingForNodeWithConsistentChainOfDynamicRecordsWithLabels()
		 {
			  // given
			  assertEquals( 3, ChainOfDynamicRecordsWithLabelsForANode( 130 ).first().Count );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  assertTrue( "should be consistent", stats.Consistent );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportLabelScanStoreInconsistencies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportLabelScanStoreInconsistencies()
		 {
			  // given
			  GraphStoreFixture.IdGenerator idGenerator = fixture.idGenerator();
			  long nodeId1 = idGenerator.Node();
			  long labelId = idGenerator.Label() - 1;

			  LabelScanStore labelScanStore = fixture.directStoreAccess().labelScanStore();
			  IEnumerable<NodeLabelUpdate> nodeLabelUpdates = asIterable( labelChanges( nodeId1, new long[]{}, new long[]{labelId} ) );
			  Write( labelScanStore, nodeLabelUpdates );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.LABEL_SCAN_DOCUMENT, 1 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void write(org.neo4j.kernel.api.labelscan.LabelScanStore labelScanStore, Iterable<org.neo4j.kernel.api.labelscan.NodeLabelUpdate> nodeLabelUpdates) throws java.io.IOException
		 private void Write( LabelScanStore labelScanStore, IEnumerable<NodeLabelUpdate> nodeLabelUpdates )
		 {
			  using ( LabelScanWriter writer = labelScanStore.NewWriter() )
			  {
					foreach ( NodeLabelUpdate update in nodeLabelUpdates )
					{
						 writer.Write( update );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportIndexInconsistencies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportIndexInconsistencies()
		 {
			  // given
			  foreach ( long? indexedNodeId in _indexedNodes )
			  {
					fixture.directStoreAccess().nativeStores().NodeStore.updateRecord(notInUse(new NodeRecord(indexedNodeId.Value, false, -1, -1)));
			  }

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.INDEX, 3 ).verify( RecordType.LABEL_SCAN_DOCUMENT, 2 ).verify( RecordType.COUNTS, 3 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReportIndexInconsistenciesIfIndexIsFailed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotReportIndexInconsistenciesIfIndexIsFailed()
		 {
			  // this test fails all indexes, and then destroys a record and makes sure we only get a failure for
			  // the label scan store but not for any index

			  // given
			  DirectStoreAccess storeAccess = fixture.directStoreAccess();

			  // fail all indexes
			  IEnumerator<StoreIndexDescriptor> rules = ( new SchemaStorage( storeAccess.NativeStores().SchemaStore ) ).indexesGetAll();
			  while ( rules.MoveNext() )
			  {
					StoreIndexDescriptor rule = rules.Current;
					IndexSamplingConfig samplingConfig = new IndexSamplingConfig( Config.defaults() );
					IndexPopulator populator = storeAccess.Indexes().lookup(rule.ProviderDescriptor()).getPopulator(rule, samplingConfig, heapBufferFactory(1024));
					populator.MarkAsFailed( "Oh noes! I was a shiny index and then I was failed" );
					populator.Close( false );
			  }

			  foreach ( long? indexedNodeId in _indexedNodes )
			  {
					storeAccess.NativeStores().NodeStore.updateRecord(notInUse(new NodeRecord(indexedNodeId.Value, false, -1, -1)));
			  }

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.LABEL_SCAN_DOCUMENT, 2 ).verify( RecordType.COUNTS, 3 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportMismatchedLabels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportMismatchedLabels()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<int> labels = new java.util.ArrayList<>();
			  IList<int> labels = new List<int>();

			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.helpers.collection.Pair<java.util.List<org.neo4j.kernel.impl.store.record.DynamicRecord>, java.util.List<int>> pair = chainOfDynamicRecordsWithLabelsForANode(3);
			  Pair<IList<DynamicRecord>, IList<int>> pair = ChainOfDynamicRecordsWithLabelsForANode( 3 );
			  fixture.apply( new TransactionAnonymousInnerClass4( this, labels, pair ) );

			  long[] before = AsArray( labels );
			  labels.RemoveAt( 1 );
			  long[] after = AsArray( labels );

			  Write( fixture.directStoreAccess().labelScanStore(), asList(labelChanges(42, before, after)) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.LABEL_SCAN_DOCUMENT, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass4 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 private IList<int> _labels;
			 private Pair<IList<DynamicRecord>, IList<int>> _pair;

			 public TransactionAnonymousInnerClass4( FullCheckIntegrationTest outerInstance, IList<int> labels, Pair<IList<DynamicRecord>, IList<int>> pair )
			 {
				 this.outerInstance = outerInstance;
				 this._labels = labels;
				 this._pair = pair;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  NodeRecord node = new NodeRecord( 42, false, -1, -1 );
				  node.InUse = true;
				  IList<DynamicRecord> dynamicRecords;
				  dynamicRecords = _pair.first();
				  ( ( IList<int> )_labels ).AddRange( _pair.other() );
				  node.SetLabelField( dynamicPointer( dynamicRecords ), dynamicRecords );
				  tx.Create( node );

			 }
		 }

		 private long[] AsArray<T1>( IList<T1> @in ) where T1 : Number
		 {
			  long[] longs = new long[@in.Count];
			  for ( int i = 0; i < @in.Count; i++ )
			  {
					longs[i] = @in[i].longValue();
			  }
			  return longs;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportMismatchedInlinedLabels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportMismatchedInlinedLabels()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass5( this ) );

			  Write( fixture.directStoreAccess().labelScanStore(), asList(labelChanges(42, new long[]{ _label1, _label2 }, new long[]{ _label1 })) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.LABEL_SCAN_DOCUMENT, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass5 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass5( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  NodeRecord node = new NodeRecord( 42, false, -1, -1 );
				  node.InUse = true;
				  node.SetLabelField( _outerInstance.inlinedLabelsLongRepresentation( _outerInstance.label1, _outerInstance.label2 ), Collections.emptySet() );
				  tx.Create( node );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNodesThatAreNotIndexed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportNodesThatAreNotIndexed()
		 {
			  // given
			  IndexSamplingConfig samplingConfig = new IndexSamplingConfig( Config.defaults() );
			  IEnumerator<StoreIndexDescriptor> indexDescriptorIterator = ( new SchemaStorage( fixture.directStoreAccess().nativeStores().SchemaStore ) ).indexesGetAll();
			  NeoStoreIndexStoreView storeView = new NeoStoreIndexStoreView( LockService.NO_LOCK_SERVICE, fixture.directStoreAccess().nativeStores().RawNeoStores );
			  while ( indexDescriptorIterator.MoveNext() )
			  {
					StoreIndexDescriptor indexDescriptor = indexDescriptorIterator.Current;
					IndexAccessor accessor = fixture.directStoreAccess().indexes().lookup(indexDescriptor.ProviderDescriptor()).getOnlineAccessor(indexDescriptor, samplingConfig);
					using ( IndexUpdater updater = accessor.NewUpdater( IndexUpdateMode.ONLINE ) )
					{
						 foreach ( long nodeId in _indexedNodes )
						 {
							  EntityUpdates updates = storeView.NodeAsUpdates( nodeId );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.kernel.api.index.IndexEntryUpdate<?> update : updates.forIndexKeys(asList(indexDescriptor)))
							  foreach ( IndexEntryUpdate<object> update in updates.ForIndexKeys( asList( indexDescriptor ) ) )
							  {
									updater.Process( IndexEntryUpdate.remove( nodeId, indexDescriptor, update.Values() ) );
							  }
						 }
					}
					accessor.Force( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
					accessor.Dispose();
			  }

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.NODE, 3 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNodesWithDuplicatePropertyValueInUniqueIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportNodesWithDuplicatePropertyValueInUniqueIndex()
		 {
			  // given
			  IndexSamplingConfig samplingConfig = new IndexSamplingConfig( Config.defaults() );
			  IEnumerator<StoreIndexDescriptor> indexRuleIterator = ( new SchemaStorage( fixture.directStoreAccess().nativeStores().SchemaStore ) ).indexesGetAll();
			  while ( indexRuleIterator.MoveNext() )
			  {
					StoreIndexDescriptor indexRule = indexRuleIterator.Current;
					IndexAccessor accessor = fixture.directStoreAccess().indexes().lookup(indexRule.ProviderDescriptor()).getOnlineAccessor(indexRule, samplingConfig);
					IndexUpdater updater = accessor.NewUpdater( IndexUpdateMode.ONLINE );
					updater.Process( IndexEntryUpdate.add( 42, indexRule.Schema(), Values(indexRule) ) );
					updater.Close();
					accessor.Force( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
					accessor.Dispose();
			  }

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.NODE, 1 ).verify( RecordType.INDEX, 3 ).andThatsAllFolks();
		 }

		 private Value[] Values( StoreIndexDescriptor indexRule )
		 {
			  switch ( indexRule.Schema().PropertyIds.Length )
			  {
			  case 1:
				  return Iterators.array( Values.of( VALUE1 ) );
			  case 2:
				  return Iterators.array( Values.of( VALUE1 ), Values.of( VALUE2 ) );
			  default:
				  throw new System.NotSupportedException();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportMissingMandatoryNodeProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportMissingMandatoryNodeProperty()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass6( this ) );

			  CreateNodePropertyExistenceConstraint( _draconian, _mandatory );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.NODE, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass6 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass6( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  // structurally correct, but does not have the 'mandatory' property with the 'draconian' label
				  NodeRecord node = new NodeRecord( next.Node(), false, -1, next.Property() );
				  node.InUse = true;
				  node.SetLabelField( _outerInstance.inlinedLabelsLongRepresentation( _outerInstance.draconian ), Collections.emptySet() );
				  PropertyRecord property = new PropertyRecord( node.NextProp, node );
				  property.InUse = true;
				  PropertyBlock block = new PropertyBlock();
				  block.SingleBlock = _outerInstance.key1 | ( ( ( long ) PropertyType.INT.intValue() ) << 24 ) | (1337L << 28);
				  property.AddPropertyBlock( block );
				  tx.Create( node );
				  tx.Create( property );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportMissingMandatoryRelationshipProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportMissingMandatoryRelationshipProperty()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass7( this ) );

			  CreateRelationshipPropertyExistenceConstraint( _m, _mandatory );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.RELATIONSHIP, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass7 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass7( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  long nodeId1 = next.Node();
				  long nodeId2 = next.Node();
				  long relId = next.Relationship();
				  long propId = next.Property();

				  NodeRecord node1 = new NodeRecord( nodeId1, true, false, relId, NO_NEXT_PROPERTY.intValue(), NO_LABELS_FIELD.intValue() );
				  NodeRecord node2 = new NodeRecord( nodeId2, true, false, relId, NO_NEXT_PROPERTY.intValue(), NO_LABELS_FIELD.intValue() );

				  // structurally correct, but does not have the 'mandatory' property with the 'M' rel type
				  RelationshipRecord relationship = new RelationshipRecord( relId, true, nodeId1, nodeId2, _outerInstance.M, NO_PREV_RELATIONSHIP.intValue(), NO_NEXT_RELATIONSHIP.intValue(), NO_PREV_RELATIONSHIP.intValue(), NO_NEXT_RELATIONSHIP.intValue(), true, true );
				  relationship.NextProp = propId;

				  PropertyRecord property = new PropertyRecord( propId, relationship );
				  property.InUse = true;
				  PropertyBlock block = new PropertyBlock();
				  block.SingleBlock = _outerInstance.key1 | ( ( ( long ) PropertyType.INT.intValue() ) << 24 ) | (1337L << 28);
				  property.AddPropertyBlock( block );

				  tx.Create( node1 );
				  tx.Create( node2 );
				  tx.Create( relationship );
				  tx.Create( property );
				  tx.IncrementRelationshipCount( ANY_LABEL, ANY_RELATIONSHIP_TYPE, ANY_LABEL, 1 );
				  tx.IncrementRelationshipCount( ANY_LABEL, _outerInstance.M, ANY_LABEL, 1 );
			 }
		 }

		 private long InlinedLabelsLongRepresentation( params long[] labelIds )
		 {
			  long header = ( long ) labelIds.Length << 36;
			  sbyte bitsPerLabel = ( sbyte )( 36 / labelIds.Length );
			  Bits bits = bits( 5 );
			  foreach ( long labelId in labelIds )
			  {
					bits.Put( labelId, bitsPerLabel );
			  }
			  return header | bits.Longs[0];
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportCyclesInDynamicRecordsWithLabels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportCyclesInDynamicRecordsWithLabels()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.neo4j.kernel.impl.store.record.DynamicRecord> chain = chainOfDynamicRecordsWithLabelsForANode(176).first();
			  IList<DynamicRecord> chain = ChainOfDynamicRecordsWithLabelsForANode( 176 ).first();
			  assertEquals( "number of records in chain", 3, chain.Count );
			  assertEquals( "all records full", chain[0].Length, chain[2].Length );
			  fixture.apply( new TransactionAnonymousInnerClass8( this, chain ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.NODE, 1 ).verify( RecordType.COUNTS, 177 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass8 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 private IList<DynamicRecord> _chain;

			 public TransactionAnonymousInnerClass8( FullCheckIntegrationTest outerInstance, IList<DynamicRecord> chain )
			 {
				 this.outerInstance = outerInstance;
				 this._chain = chain;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  long nodeId = ( ( long[] ) getRightArray( readFullByteArrayFromHeavyRecords( _chain, ARRAY ) ).asObject() )[0];
				  NodeRecord before = inUse( new NodeRecord( nodeId, false, -1, -1 ) );
				  NodeRecord after = inUse( new NodeRecord( nodeId, false, -1, -1 ) );
				  DynamicRecord record1 = _chain[0].clone();
				  DynamicRecord record2 = _chain[1].clone();
				  DynamicRecord record3 = _chain[2].clone();

				  record3.NextBlock = record2.Id;
				  before.SetLabelField( dynamicPointer( _chain ), _chain );
				  after.SetLabelField( dynamicPointer( _chain ), asList( record1, record2, record3 ) );
				  tx.Update( before, after );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.helpers.collection.Pair<java.util.List<org.neo4j.kernel.impl.store.record.DynamicRecord>,java.util.List<int>> chainOfDynamicRecordsWithLabelsForANode(int labelCount) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private Pair<IList<DynamicRecord>, IList<int>> ChainOfDynamicRecordsWithLabelsForANode( int labelCount )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] labels = new long[labelCount + 1];
			  long[] labels = new long[labelCount + 1]; // allocate enough labels to need three records
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<int> createdLabels = new java.util.ArrayList<>();
			  IList<int> createdLabels = new List<int>();
			  using ( GraphStoreFixture.Applier applier = fixture.createApplier() )
			  {
					for ( int i = 1; i < labels.Length; i++ )
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int offset = i;
						 int offset = i;
						 applier.Apply( new TransactionAnonymousInnerClass9( this, labels, createdLabels, offset ) );
					}
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.neo4j.kernel.impl.store.record.DynamicRecord> chain = new java.util.ArrayList<>();
			  IList<DynamicRecord> chain = new List<DynamicRecord>();
			  fixture.apply( new TransactionAnonymousInnerClass10( this, labels, chain ) );
			  return Pair.of( chain, createdLabels );
		 }

		 private class TransactionAnonymousInnerClass9 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 private long[] _labels;
			 private IList<int> _createdLabels;
			 private int _offset;

			 public TransactionAnonymousInnerClass9( FullCheckIntegrationTest outerInstance, long[] labels, IList<int> createdLabels, int offset )
			 {
				 this.outerInstance = outerInstance;
				 this._labels = labels;
				 this._createdLabels = createdLabels;
				 this._offset = offset;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  int? label = next.Label();
				  tx.NodeLabel( ( int )( _labels[_offset] = label ), "label:" + _offset );
				  _createdLabels.Add( label );
			 }
		 }

		 private class TransactionAnonymousInnerClass10 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 private long[] _labels;
			 private IList<DynamicRecord> _chain;

			 public TransactionAnonymousInnerClass10( FullCheckIntegrationTest outerInstance, long[] labels, IList<DynamicRecord> chain )
			 {
				 this.outerInstance = outerInstance;
				 this._labels = labels;
				 this._chain = chain;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  NodeRecord nodeRecord = new NodeRecord( next.Node(), false, -1, -1 );
				  DynamicRecord record1 = inUse( new DynamicRecord( next.NodeLabel() ) );
				  DynamicRecord record2 = inUse( new DynamicRecord( next.NodeLabel() ) );
				  DynamicRecord record3 = inUse( new DynamicRecord( next.NodeLabel() ) );
				  _labels[0] = nodeRecord.Id; // the first id should not be a label id, but the id of the node
				  ReusableRecordsAllocator allocator = new ReusableRecordsAllocator( 60, record1, record2, record3 );
				  allocateFromNumbers( _chain, _labels, allocator );

				  nodeRecord.SetLabelField( dynamicPointer( _chain ), _chain );

				  tx.Create( nodeRecord );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNodeDynamicLabelContainingDuplicateLabelAsNodeInconsistency() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportNodeDynamicLabelContainingDuplicateLabelAsNodeInconsistency()
		 {
			  int nodeId = 1000;
			  ICollection<DynamicRecord> duplicatedLabel = new List<DynamicRecord>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.helpers.collection.Pair<java.util.List<org.neo4j.kernel.impl.store.record.DynamicRecord>, java.util.List<int>> labels = chainOfDynamicRecordsWithLabelsForANode(1);
			  Pair<IList<DynamicRecord>, IList<int>> labels = ChainOfDynamicRecordsWithLabelsForANode( 1 );

			  // given
			  fixture.apply( new TransactionAnonymousInnerClass11( this, nodeId, duplicatedLabel, labels ) );

			  StoreAccess storeAccess = fixture.directStoreAccess().nativeStores();
			  NodeRecord nodeRecord = new NodeRecord( nodeId );
			  storeAccess.NodeStore.getRecord( nodeId, nodeRecord, FORCE );
			  nodeRecord.SetLabelField( dynamicPointer( duplicatedLabel ), duplicatedLabel );
			  nodeRecord.InUse = true;
			  storeAccess.NodeStore.updateRecord( nodeRecord );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.NODE, 1 ).verify( RecordType.COUNTS, 0 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass11 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 private int _nodeId;
			 private ICollection<DynamicRecord> _duplicatedLabel;
			 private Pair<IList<DynamicRecord>, IList<int>> _labels;

			 public TransactionAnonymousInnerClass11( FullCheckIntegrationTest outerInstance, int nodeId, ICollection<DynamicRecord> duplicatedLabel, Pair<IList<DynamicRecord>, IList<int>> labels )
			 {
				 this.outerInstance = outerInstance;
				 this._nodeId = nodeId;
				 this._duplicatedLabel = duplicatedLabel;
				 this._labels = labels;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  NodeRecord node = new NodeRecord( _nodeId, false, -1, -1 );
				  node.InUse = true;
				  IList<DynamicRecord> labelRecords = _labels.first();
				  node.SetLabelField( dynamicPointer( labelRecords ), labelRecords );
				  tx.Create( node );

				  int? labelId = _labels.other()[0];
				  DynamicRecord record = inUse( new DynamicRecord( labelId ) );
				  allocateFromNumbers( _duplicatedLabel, new long[]{ _nodeId, labelId, labelId }, new ReusableRecordsAllocator( 60, record ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportOrphanedNodeDynamicLabelAsNodeInconsistency() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportOrphanedNodeDynamicLabelAsNodeInconsistency()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass12( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.NODE_DYNAMIC_LABEL, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass12 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass12( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  tx.NodeLabel( 42, "Label" );

				  NodeRecord nodeRecord = new NodeRecord( next.Node(), false, -1, -1 );
				  DynamicRecord record = inUse( new DynamicRecord( next.NodeLabel() ) );
				  ICollection<DynamicRecord> newRecords = new List<DynamicRecord>();
				  allocateFromNumbers( newRecords, prependNodeId( next.Node(), new long[]{ 42L } ), new ReusableRecordsAllocator(60, record) );
				  nodeRecord.SetLabelField( dynamicPointer( newRecords ), newRecords );

				  tx.Create( nodeRecord );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportRelationshipInconsistencies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportRelationshipInconsistencies()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass13( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.RELATIONSHIP, 2 ).verify( RecordType.COUNTS, 3 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass13 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass13( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  tx.Create( new RelationshipRecord( next.Relationship(), 1, 2, _outerInstance.C ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportRelationshipOtherNodeInconsistencies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportRelationshipOtherNodeInconsistencies()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass14( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.RELATIONSHIP, 2 ).verify( RecordType.NODE, 2 ).verify( RecordType.COUNTS, 2 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass14 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass14( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  long node1 = next.Node();
				  long node2 = next.Node();
				  long rel = next.Relationship();
				  tx.Create( inUse( new RelationshipRecord( rel, node1, node2, 0 ) ) );
				  tx.Create( inUse( new NodeRecord( node1, false, rel + 1, -1 ) ) );
				  tx.Create( inUse( new NodeRecord( node2, false, rel + 2, -1 ) ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportPropertyInconsistencies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportPropertyInconsistencies()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass15( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.PROPERTY, 2 ).verify( RecordType.NODE, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass15 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass15( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  NodeRecord node = new NodeRecord( next.Node() );
				  PropertyRecord property = new PropertyRecord( next.Property() );
				  node.NextProp = property.Id;

				  // Mess up the prev/next pointers a bit
				  property.NextProp = 1_000;

				  PropertyBlock block = new PropertyBlock();
				  block.SingleBlock = next.PropertyKey() | (((long) PropertyType.INT.intValue()) << 24) | (666L << 28);
				  property.AddPropertyBlock( block );
				  tx.Create( node );
				  tx.Create( property );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportStringPropertyInconsistencies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportStringPropertyInconsistencies()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass16( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.STRING_PROPERTY, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass16 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass16( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  DynamicRecord @string = new DynamicRecord( next.StringProperty() );
				  @string.InUse = true;
				  @string.SetCreated();
				  @string.SetType( PropertyType.STRING.intValue() );
				  @string.NextBlock = next.StringProperty();
				  @string.Data = UTF8.encode( "hello world" );

				  PropertyBlock block = new PropertyBlock();
				  block.SingleBlock = ( ( ( long ) PropertyType.STRING.intValue() ) << 24 ) | (@string.Id << 28);
				  block.AddValueRecord( @string );

				  PropertyRecord property = new PropertyRecord( next.Property() );
				  property.AddPropertyBlock( block );

				  tx.Create( property );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportBrokenSchemaRecordChain() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportBrokenSchemaRecordChain()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass17( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.SCHEMA, 3 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass17 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass17( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  DynamicRecord schema = new DynamicRecord( next.Schema() );
				  DynamicRecord schemaBefore = Schema.clone();

				  Schema.NextBlock = next.Schema(); // Point to a record that isn't in use.
				  StoreIndexDescriptor rule = indexRule( Schema.Id, _outerInstance.label1, _outerInstance.key1, _descriptor );
				  Schema.Data = SchemaRuleSerialization.serialize( rule );

				  tx.CreateSchema( asList( schemaBefore ), asList( schema ), rule );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportDuplicateConstraintReferences() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportDuplicateConstraintReferences()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass18( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.SCHEMA, 4 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass18 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass18( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  int ruleId1 = ( int ) next.Schema();
				  int ruleId2 = ( int ) next.Schema();
				  int labelId = next.Label();
				  int propertyKeyId = next.PropertyKey();

				  DynamicRecord record1 = new DynamicRecord( ruleId1 );
				  DynamicRecord record2 = new DynamicRecord( ruleId2 );
				  DynamicRecord record1Before = record1.Clone();
				  DynamicRecord record2Before = record2.Clone();

				  StoreIndexDescriptor rule1 = constraintIndexRule( ruleId1, labelId, propertyKeyId, _descriptor, ruleId1 );
				  StoreIndexDescriptor rule2 = constraintIndexRule( ruleId2, labelId, propertyKeyId, _descriptor, ruleId1 );

				  ICollection<DynamicRecord> records1 = SerializeRule( rule1, record1 );
				  ICollection<DynamicRecord> records2 = SerializeRule( rule2, record2 );

				  assertEquals( asList( record1 ), records1 );
				  assertEquals( asList( record2 ), records2 );

				  tx.NodeLabel( labelId, "label" );
				  tx.PropertyKey( propertyKeyId, "property" );

				  tx.CreateSchema( asList( record1Before ), records1, rule1 );
				  tx.CreateSchema( asList( record2Before ), records2, rule2 );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportInvalidConstraintBackReferences() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportInvalidConstraintBackReferences()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass19( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.SCHEMA, 2 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass19 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass19( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  int ruleId1 = ( int ) next.Schema();
				  int ruleId2 = ( int ) next.Schema();
				  int labelId = next.Label();
				  int propertyKeyId = next.PropertyKey();

				  DynamicRecord record1 = new DynamicRecord( ruleId1 );
				  DynamicRecord record2 = new DynamicRecord( ruleId2 );
				  DynamicRecord record1Before = record1.Clone();
				  DynamicRecord record2Before = record2.Clone();

				  StoreIndexDescriptor rule1 = constraintIndexRule( ruleId1, labelId, propertyKeyId, _descriptor, ruleId2 );
				  ConstraintRule rule2 = uniquenessConstraintRule( ruleId2, labelId, propertyKeyId, ruleId2 );

				  ICollection<DynamicRecord> records1 = SerializeRule( rule1, record1 );
				  ICollection<DynamicRecord> records2 = SerializeRule( rule2, record2 );

				  assertEquals( asList( record1 ), records1 );
				  assertEquals( asList( record2 ), records2 );

				  tx.NodeLabel( labelId, "label" );
				  tx.PropertyKey( propertyKeyId, "property" );

				  tx.CreateSchema( asList( record1Before ), records1, rule1 );
				  tx.CreateSchema( asList( record2Before ), records2, rule2 );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportArrayPropertyInconsistencies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportArrayPropertyInconsistencies()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass20( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.ARRAY_PROPERTY, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass20 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass20( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  DynamicRecord array = new DynamicRecord( next.ArrayProperty() );
				  array.InUse = true;
				  array.SetCreated();
				  array.SetType( ARRAY.intValue() );
				  array.NextBlock = next.ArrayProperty();
				  array.Data = UTF8.encode( "hello world" );

				  PropertyBlock block = new PropertyBlock();
				  block.SingleBlock = ( ( ( long ) ARRAY.intValue() ) << 24 ) | (array.Id << 28);
				  block.AddValueRecord( array );

				  PropertyRecord property = new PropertyRecord( next.Property() );
				  property.AddPropertyBlock( block );

				  tx.Create( property );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportRelationshipLabelNameInconsistencies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportRelationshipLabelNameInconsistencies()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Reference<int> inconsistentName = new Reference<>();
			  Reference<int> inconsistentName = new Reference<int>();
			  fixture.apply( new TransactionAnonymousInnerClass21( this, inconsistentName ) );
			  StoreAccess access = fixture.directStoreAccess().nativeStores();
			  DynamicRecord record = access.RelationshipTypeNameStore.getRecord( inconsistentName.Get(), access.RelationshipTypeNameStore.newRecord(), FORCE );
			  record.NextBlock = record.Id;
			  access.RelationshipTypeNameStore.updateRecord( record );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.RELATIONSHIP_TYPE_NAME, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass21 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 private Neo4Net.Consistency.checking.full.FullCheckIntegrationTest.Reference<int> _inconsistentName;

			 public TransactionAnonymousInnerClass21( FullCheckIntegrationTest outerInstance, Neo4Net.Consistency.checking.full.FullCheckIntegrationTest.Reference<int> inconsistentName )
			 {
				 this.outerInstance = outerInstance;
				 this._inconsistentName = inconsistentName;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  _inconsistentName.set( next.RelationshipType() );
				  tx.RelationshipType( _inconsistentName.get(), "FOO" );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportPropertyKeyNameInconsistencies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportPropertyKeyNameInconsistencies()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Reference<int> inconsistentName = new Reference<>();
			  Reference<int> inconsistentName = new Reference<int>();
			  fixture.apply( new TransactionAnonymousInnerClass22( this, inconsistentName ) );
			  StoreAccess access = fixture.directStoreAccess().nativeStores();
			  DynamicRecord record = access.PropertyKeyNameStore.getRecord( inconsistentName.Get() + 1, access.PropertyKeyNameStore.newRecord(), FORCE );
			  record.NextBlock = record.Id;
			  access.PropertyKeyNameStore.updateRecord( record );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.PROPERTY_KEY_NAME, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass22 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 private Neo4Net.Consistency.checking.full.FullCheckIntegrationTest.Reference<int> _inconsistentName;

			 public TransactionAnonymousInnerClass22( FullCheckIntegrationTest outerInstance, Neo4Net.Consistency.checking.full.FullCheckIntegrationTest.Reference<int> inconsistentName )
			 {
				 this.outerInstance = outerInstance;
				 this._inconsistentName = inconsistentName;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  _inconsistentName.set( next.PropertyKey() );
				  tx.PropertyKey( _inconsistentName.get(), "FOO" );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportRelationshipTypeInconsistencies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportRelationshipTypeInconsistencies()
		 {
			  // given
			  StoreAccess access = fixture.directStoreAccess().nativeStores();
			  RecordStore<RelationshipTypeTokenRecord> relTypeStore = access.RelationshipTypeTokenStore;
			  RelationshipTypeTokenRecord record = relTypeStore.GetRecord( ( int ) relTypeStore.nextId(), relTypeStore.NewRecord(), FORCE );
			  record.NameId = 20;
			  record.InUse = true;
			  relTypeStore.UpdateRecord( record );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  access.Close();
			  on( stats ).verify( RecordType.RELATIONSHIP_TYPE, 1 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportLabelInconsistencies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportLabelInconsistencies()
		 {
			  // given
			  StoreAccess access = fixture.directStoreAccess().nativeStores();
			  LabelTokenRecord record = access.LabelTokenStore.getRecord( 1, access.LabelTokenStore.newRecord(), FORCE );
			  record.NameId = 20;
			  record.InUse = true;
			  access.LabelTokenStore.updateRecord( record );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.LABEL, 1 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportPropertyKeyInconsistencies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportPropertyKeyInconsistencies()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Reference<int> inconsistentKey = new Reference<>();
			  Reference<int> inconsistentKey = new Reference<int>();
			  fixture.apply( new TransactionAnonymousInnerClass23( this, inconsistentKey ) );
			  StoreAccess access = fixture.directStoreAccess().nativeStores();
			  DynamicRecord record = access.PropertyKeyNameStore.getRecord( inconsistentKey.Get() + 1, access.PropertyKeyNameStore.newRecord(), FORCE );
			  record.InUse = false;
			  access.PropertyKeyNameStore.updateRecord( record );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.PROPERTY_KEY, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass23 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 private Neo4Net.Consistency.checking.full.FullCheckIntegrationTest.Reference<int> _inconsistentKey;

			 public TransactionAnonymousInnerClass23( FullCheckIntegrationTest outerInstance, Neo4Net.Consistency.checking.full.FullCheckIntegrationTest.Reference<int> inconsistentKey )
			 {
				 this.outerInstance = outerInstance;
				 this._inconsistentKey = inconsistentKey;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  _inconsistentKey.set( next.PropertyKey() );
				  tx.PropertyKey( _inconsistentKey.get(), "FOO" );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportRelationshipGroupTypeInconsistencies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportRelationshipGroupTypeInconsistencies()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass24( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.RELATIONSHIP_GROUP, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass24 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass24( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  long node = next.Node();
				  long group = next.RelationshipGroup();
				  int nonExistentType = next.RelationshipType() + 1;
				  tx.Create( inUse( new NodeRecord( node, true, group, NO_NEXT_PROPERTY.intValue() ) ) );
				  tx.Create( WithOwner( inUse( new RelationshipGroupRecord( group, nonExistentType ) ), node ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportRelationshipGroupChainInconsistencies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportRelationshipGroupChainInconsistencies()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass25( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.RELATIONSHIP_GROUP, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass25 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass25( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  long node = next.Node();
				  long group = next.RelationshipGroup();
				  tx.Create( inUse( new NodeRecord( node, true, group, NO_NEXT_PROPERTY.intValue() ) ) );
				  tx.Create( WithOwner( Neo4Net.Consistency.checking.full.FullCheckIntegrationTest.WithNext( inUse( new RelationshipGroupRecord( group, _outerInstance.C ) ), group + 1 ), node ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportRelationshipGroupUnsortedChainInconsistencies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportRelationshipGroupUnsortedChainInconsistencies()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass26( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.RELATIONSHIP_GROUP, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass26 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass26( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  long node = next.Node();
				  long firstGroupId = next.RelationshipGroup();
				  long otherGroupId = next.RelationshipGroup();
				  tx.Create( inUse( new NodeRecord( node, true, firstGroupId, NO_NEXT_PROPERTY.intValue() ) ) );
				  tx.Create( WithOwner( Neo4Net.Consistency.checking.full.FullCheckIntegrationTest.WithNext( inUse( new RelationshipGroupRecord( firstGroupId, _outerInstance.T ) ), otherGroupId ), node ) );
				  tx.Create( WithOwner( inUse( new RelationshipGroupRecord( otherGroupId, _outerInstance.C ) ), node ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportRelationshipGroupRelationshipNotInUseInconsistencies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportRelationshipGroupRelationshipNotInUseInconsistencies()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass27( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.RELATIONSHIP_GROUP, 3 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass27 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass27( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  long node = next.Node();
				  long groupId = next.RelationshipGroup();
				  long rel = next.Relationship();
				  tx.Create( inUse( new NodeRecord( node, true, groupId, NO_NEXT_PROPERTY.intValue() ) ) );
				  tx.Create( WithOwner( WithRelationships( inUse( new RelationshipGroupRecord( groupId, _outerInstance.C ) ), rel, rel, rel ), node ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportRelationshipGroupRelationshipNotFirstInconsistencies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportRelationshipGroupRelationshipNotFirstInconsistencies()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass28( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.RELATIONSHIP_GROUP, 3 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass28 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass28( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  /*
				   *   node ----------------> group
				   *                             |
				   *                             v
				   *   otherNode <--> relA <--> relB
				   */
				  long node = next.Node();
				  long otherNode = next.Node();
				  long group = next.RelationshipGroup();
				  long relA = next.Relationship();
				  long relB = next.Relationship();
				  tx.Create( inUse( new NodeRecord( node, true, group, NO_NEXT_PROPERTY.intValue() ) ) );
				  tx.Create( inUse( new NodeRecord( otherNode, false, relA, NO_NEXT_PROPERTY.intValue() ) ) );
				  tx.Create( Neo4Net.Consistency.checking.full.FullCheckIntegrationTest.WithNext( inUse( new RelationshipRecord( relA, otherNode, otherNode, _outerInstance.C ) ), relB ) );
				  tx.Create( _outerInstance.withPrev( inUse( new RelationshipRecord( relB, otherNode, otherNode, _outerInstance.C ) ), relA ) );
				  tx.Create( WithOwner( WithRelationships( inUse( new RelationshipGroupRecord( group, _outerInstance.C ) ), relB, relB, relB ), node ) );
				  tx.IncrementRelationshipCount( ANY_LABEL, ANY_RELATIONSHIP_TYPE, ANY_LABEL, 2 );
				  tx.IncrementRelationshipCount( ANY_LABEL, _outerInstance.C, ANY_LABEL, 2 );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportFirstRelationshipGroupOwnerInconsistency() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportFirstRelationshipGroupOwnerInconsistency()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass29( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  // - next group has other owner that its previous
			  // - first group has other owner
			  on( stats ).verify( RecordType.NODE, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass29 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass29( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  // node -[first]-> group -[owner]-> otherNode
				  long node = next.Node();
				  long otherNode = next.Node();
				  long group = next.RelationshipGroup();
				  tx.Create( inUse( new NodeRecord( node, true, group, NO_NEXT_PROPERTY.intValue() ) ) );
				  tx.Create( inUse( new NodeRecord( otherNode, false, NO_NEXT_RELATIONSHIP.intValue(), NO_NEXT_PROPERTY.intValue() ) ) );
				  tx.Create( WithOwner( inUse( new RelationshipGroupRecord( group, _outerInstance.C ) ), otherNode ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportChainedRelationshipGroupOwnerInconsistency() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportChainedRelationshipGroupOwnerInconsistency()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass30( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.RELATIONSHIP_GROUP, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass30 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass30( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  /* node -[first]-> groupA -[next]-> groupB
				   *    ^               /                |
				   *     \--[owner]----               [owner]
				   *                                     v
				   *                                  otherNode
				   */
				  long node = next.Node();
				  long otherNode = next.Node();
				  long groupA = next.RelationshipGroup();
				  long groupB = next.RelationshipGroup();
				  tx.Create( inUse( new NodeRecord( node, true, groupA, NO_NEXT_PROPERTY.intValue() ) ) );
				  tx.Create( inUse( new NodeRecord( otherNode, false, NO_NEXT_RELATIONSHIP.intValue(), NO_NEXT_PROPERTY.intValue() ) ) );
				  tx.Create( Neo4Net.Consistency.checking.full.FullCheckIntegrationTest.WithNext( WithOwner( inUse( new RelationshipGroupRecord( groupA, _outerInstance.C ) ), node ), groupB ) );
				  tx.Create( WithOwner( inUse( new RelationshipGroupRecord( groupB, _outerInstance.T ) ), otherNode ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportRelationshipGroupOwnerNotInUse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportRelationshipGroupOwnerNotInUse()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass31( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.RELATIONSHIP_GROUP, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass31 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass31( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  // group -[owner]-> <not-in-use node>
				  long node = next.Node();
				  long group = next.RelationshipGroup();
				  tx.Create( WithOwner( inUse( new RelationshipGroupRecord( group, _outerInstance.C ) ), node ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportRelationshipGroupOwnerInvalidValue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportRelationshipGroupOwnerInvalidValue()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass32( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.RELATIONSHIP_GROUP, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass32 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass32( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  // node -[first]-> group -[owner]-> -1
				  long group = next.RelationshipGroup();
				  tx.Create( WithOwner( inUse( new RelationshipGroupRecord( group, _outerInstance.C ) ), -1 ) );
			 }
		 }

		 private RelationshipRecord WithNext( RelationshipRecord relationship, long next )
		 {
			  relationship.FirstNextRel = next;
			  relationship.SecondNextRel = next;
			  return relationship;
		 }

		 private RelationshipRecord WithPrev( RelationshipRecord relationship, long prev )
		 {
			  relationship.FirstInFirstChain = false;
			  relationship.FirstInSecondChain = false;
			  relationship.FirstPrevRel = prev;
			  relationship.SecondPrevRel = prev;
			  return relationship;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportRelationshipGroupRelationshipOfOtherTypeInconsistencies() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportRelationshipGroupRelationshipOfOtherTypeInconsistencies()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass33( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.RELATIONSHIP_GROUP, 3 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass33 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass33( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  /*
				   *   node -----> groupA
				   *                   |
				   *                   v
				   *   otherNode <--> relB
				   */
				  long node = next.Node();
				  long otherNode = next.Node();
				  long group = next.RelationshipGroup();
				  long rel = next.Relationship();
				  tx.Create( new NodeRecord( node, true, group, NO_NEXT_PROPERTY.intValue() ) );
				  tx.Create( new NodeRecord( otherNode, false, rel, NO_NEXT_PROPERTY.intValue() ) );
				  tx.Create( new RelationshipRecord( rel, otherNode, otherNode, _outerInstance.T ) );
				  tx.Create( WithOwner( WithRelationships( new RelationshipGroupRecord( group, _outerInstance.C ), rel, rel, rel ), node ) );
				  tx.IncrementRelationshipCount( ANY_LABEL, ANY_RELATIONSHIP_TYPE, ANY_LABEL, 1 );
				  tx.IncrementRelationshipCount( ANY_LABEL, _outerInstance.T, ANY_LABEL, 1 );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReportRelationshipGroupInconsistenciesForConsistentRecords() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotReportRelationshipGroupInconsistenciesForConsistentRecords()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass34( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  assertTrue( "should be consistent", stats.Consistent );
		 }

		 private class TransactionAnonymousInnerClass34 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass34( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  /* Create a little mini consistent structure:
				   *
				   *    nodeA --> groupA -[next]-> groupB
				   *      ^          |
				   *       \       [out]
				   *        \        v
				   *       [start]- rel -[end]-> nodeB
				   */

				  long nodeA = next.Node();
				  long nodeB = next.Node();
				  long rel = next.Relationship();
				  long groupA = next.RelationshipGroup();
				  long groupB = next.RelationshipGroup();

				  tx.Create( new NodeRecord( nodeA, true, groupA, NO_NEXT_PROPERTY.intValue() ) );
				  tx.Create( new NodeRecord( nodeB, false, rel, NO_NEXT_PROPERTY.intValue() ) );
				  tx.Create( FirstInChains( new RelationshipRecord( rel, nodeA, nodeB, _outerInstance.C ), 1 ) );
				  tx.IncrementRelationshipCount( ANY_LABEL, ANY_RELATIONSHIP_TYPE, ANY_LABEL, 1 );
				  tx.IncrementRelationshipCount( ANY_LABEL, _outerInstance.C, ANY_LABEL, 1 );

				  tx.Create( WithOwner( WithRelationship( Neo4Net.Consistency.checking.full.FullCheckIntegrationTest.WithNext( new RelationshipGroupRecord( groupA, _outerInstance.C ), groupB ), Direction.OUTGOING, rel ), nodeA ) );
				  tx.Create( WithOwner( new RelationshipGroupRecord( groupB, _outerInstance.T ), nodeA ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportWrongNodeCountsEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportWrongNodeCountsEntries()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass35( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.COUNTS, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass35 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass35( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  tx.IncrementNodeCount( _outerInstance.label3, 1 );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportWrongRelationshipCountsEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportWrongRelationshipCountsEntries()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass36( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.COUNTS, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass36 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass36( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  tx.IncrementRelationshipCount( _outerInstance.label1, _outerInstance.C, ANY_LABEL, 1 );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportIfSomeKeysAreMissing() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportIfSomeKeysAreMissing()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass37( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.COUNTS, 1 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass37 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass37( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  tx.IncrementNodeCount( _outerInstance.label3, -1 );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportIfThereAreExtraKeys() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportIfThereAreExtraKeys()
		 {
			  // given
			  fixture.apply( new TransactionAnonymousInnerClass38( this ) );

			  // when
			  ConsistencySummaryStatistics stats = Check();

			  // then
			  on( stats ).verify( RecordType.COUNTS, 2 ).andThatsAllFolks();
		 }

		 private class TransactionAnonymousInnerClass38 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 public TransactionAnonymousInnerClass38( FullCheckIntegrationTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  tx.IncrementNodeCount( 1024, 1 );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportDuplicatedIndexRules() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportDuplicatedIndexRules()
		 {
			  // Given
			  int labelId = CreateLabel();
			  int propertyKeyId = CreatePropertyKey();
			  CreateIndexRule( labelId, propertyKeyId );
			  CreateIndexRule( labelId, propertyKeyId );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then
			  on( stats ).verify( RecordType.SCHEMA, 1 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportDuplicatedCompositeIndexRules() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportDuplicatedCompositeIndexRules()
		 {
			  // Given
			  int labelId = CreateLabel();
			  int propertyKeyId1 = CreatePropertyKey( "p1" );
			  int propertyKeyId2 = CreatePropertyKey( "p2" );
			  int propertyKeyId3 = CreatePropertyKey( "p3" );
			  CreateIndexRule( labelId, propertyKeyId1, propertyKeyId2, propertyKeyId3 );
			  CreateIndexRule( labelId, propertyKeyId1, propertyKeyId2, propertyKeyId3 );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then
			  on( stats ).verify( RecordType.SCHEMA, 1 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportDuplicatedUniquenessConstraintRules() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportDuplicatedUniquenessConstraintRules()
		 {
			  // Given
			  int labelId = CreateLabel();
			  int propertyKeyId = CreatePropertyKey();
			  CreateUniquenessConstraintRule( labelId, propertyKeyId );
			  CreateUniquenessConstraintRule( labelId, propertyKeyId );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then
			  on( stats ).verify( RecordType.SCHEMA, 2 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportDuplicatedCompositeUniquenessConstraintRules() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportDuplicatedCompositeUniquenessConstraintRules()
		 {
			  // Given
			  int labelId = CreateLabel();
			  int propertyKeyId1 = CreatePropertyKey( "p1" );
			  int propertyKeyId2 = CreatePropertyKey( "p2" );
			  CreateUniquenessConstraintRule( labelId, propertyKeyId1, propertyKeyId2 );
			  CreateUniquenessConstraintRule( labelId, propertyKeyId1, propertyKeyId2 );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then
			  on( stats ).verify( RecordType.SCHEMA, 2 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportDuplicatedNodeKeyConstraintRules() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportDuplicatedNodeKeyConstraintRules()
		 {
			  // Given
			  int labelId = CreateLabel();
			  int propertyKeyId1 = CreatePropertyKey( "p1" );
			  int propertyKeyId2 = CreatePropertyKey( "p2" );
			  CreateNodeKeyConstraintRule( labelId, propertyKeyId1, propertyKeyId2 );
			  CreateNodeKeyConstraintRule( labelId, propertyKeyId1, propertyKeyId2 );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then
			  on( stats ).verify( RecordType.SCHEMA, 2 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportDuplicatedNodePropertyExistenceConstraintRules() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportDuplicatedNodePropertyExistenceConstraintRules()
		 {
			  // Given
			  int labelId = CreateLabel();
			  int propertyKeyId = CreatePropertyKey();
			  CreateNodePropertyExistenceConstraint( labelId, propertyKeyId );
			  CreateNodePropertyExistenceConstraint( labelId, propertyKeyId );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then
			  on( stats ).verify( RecordType.SCHEMA, 1 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportDuplicatedRelationshipPropertyExistenceConstraintRules() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportDuplicatedRelationshipPropertyExistenceConstraintRules()
		 {
			  // Given
			  int relTypeId = CreateRelType();
			  int propertyKeyId = CreatePropertyKey();
			  CreateRelationshipPropertyExistenceConstraint( relTypeId, propertyKeyId );
			  CreateRelationshipPropertyExistenceConstraint( relTypeId, propertyKeyId );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then
			  on( stats ).verify( RecordType.SCHEMA, 1 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportInvalidLabelIdInIndexRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportInvalidLabelIdInIndexRule()
		 {
			  // Given
			  int labelId = fixture.idGenerator().label();
			  int propertyKeyId = CreatePropertyKey();
			  CreateIndexRule( labelId, propertyKeyId );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then
			  on( stats ).verify( RecordType.SCHEMA, 1 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportInvalidLabelIdInUniquenessConstraintRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportInvalidLabelIdInUniquenessConstraintRule()
		 {
			  // Given
			  int badLabelId = fixture.idGenerator().label();
			  int propertyKeyId = CreatePropertyKey();
			  CreateUniquenessConstraintRule( badLabelId, propertyKeyId );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then
			  on( stats ).verify( RecordType.SCHEMA, 2 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportInvalidLabelIdInNodeKeyConstraintRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportInvalidLabelIdInNodeKeyConstraintRule()
		 {
			  // Given
			  int badLabelId = fixture.idGenerator().label();
			  int propertyKeyId = CreatePropertyKey();
			  CreateNodeKeyConstraintRule( badLabelId, propertyKeyId );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then
			  on( stats ).verify( RecordType.SCHEMA, 2 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportInvalidLabelIdInNodePropertyExistenceConstraintRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportInvalidLabelIdInNodePropertyExistenceConstraintRule()
		 {
			  // Given
			  int badLabelId = fixture.idGenerator().label();
			  int propertyKeyId = CreatePropertyKey();
			  CreateNodePropertyExistenceConstraint( badLabelId, propertyKeyId );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then
			  on( stats ).verify( RecordType.SCHEMA, 1 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportInvalidPropertyKeyIdInIndexRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportInvalidPropertyKeyIdInIndexRule()
		 {
			  // Given
			  int labelId = CreateLabel();
			  int badPropertyKeyId = fixture.idGenerator().propertyKey();
			  CreateIndexRule( labelId, badPropertyKeyId );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then
			  on( stats ).verify( RecordType.SCHEMA, 1 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportInvalidSecondPropertyKeyIdInIndexRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportInvalidSecondPropertyKeyIdInIndexRule()
		 {
			  // Given
			  int labelId = CreateLabel();
			  int propertyKeyId = CreatePropertyKey();
			  int badPropertyKeyId = fixture.idGenerator().propertyKey();
			  CreateIndexRule( labelId, propertyKeyId, badPropertyKeyId );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then
			  on( stats ).verify( RecordType.SCHEMA, 1 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportInvalidPropertyKeyIdInUniquenessConstraintRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportInvalidPropertyKeyIdInUniquenessConstraintRule()
		 {
			  // Given
			  int labelId = CreateLabel();
			  int badPropertyKeyId = fixture.idGenerator().propertyKey();
			  CreateUniquenessConstraintRule( labelId, badPropertyKeyId );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then
			  on( stats ).verify( RecordType.SCHEMA, 2 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportInvalidSecondPropertyKeyIdInUniquenessConstraintRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportInvalidSecondPropertyKeyIdInUniquenessConstraintRule()
		 {
			  // Given
			  int labelId = CreateLabel();
			  int propertyKeyId = CreatePropertyKey();
			  int badPropertyKeyId = fixture.idGenerator().propertyKey();
			  CreateUniquenessConstraintRule( labelId, propertyKeyId, badPropertyKeyId );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then
			  on( stats ).verify( RecordType.SCHEMA, 2 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportInvalidSecondPropertyKeyIdInNodeKeyConstraintRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportInvalidSecondPropertyKeyIdInNodeKeyConstraintRule()
		 {
			  // Given
			  int labelId = CreateLabel();
			  int propertyKeyId = CreatePropertyKey();
			  int badPropertyKeyId = fixture.idGenerator().propertyKey();
			  CreateNodeKeyConstraintRule( labelId, propertyKeyId, badPropertyKeyId );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then
			  on( stats ).verify( RecordType.SCHEMA, 2 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportInvalidPropertyKeyIdInNodePropertyExistenceConstraintRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportInvalidPropertyKeyIdInNodePropertyExistenceConstraintRule()
		 {
			  // Given
			  int labelId = CreateLabel();
			  int badPropertyKeyId = fixture.idGenerator().propertyKey();
			  CreateNodePropertyExistenceConstraint( labelId, badPropertyKeyId );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then
			  on( stats ).verify( RecordType.SCHEMA, 1 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportInvalidRelTypeIdInRelationshipPropertyExistenceConstraintRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportInvalidRelTypeIdInRelationshipPropertyExistenceConstraintRule()
		 {
			  // Given
			  int badRelTypeId = fixture.idGenerator().relationshipType();
			  int propertyKeyId = CreatePropertyKey();
			  CreateRelationshipPropertyExistenceConstraint( badRelTypeId, propertyKeyId );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then
			  on( stats ).verify( RecordType.SCHEMA, 1 ).andThatsAllFolks();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNothingForUniquenessAndPropertyExistenceConstraintOnSameLabelAndProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportNothingForUniquenessAndPropertyExistenceConstraintOnSameLabelAndProperty()
		 {
			  // Given
			  int labelId = CreateLabel();
			  int propertyKeyId = CreatePropertyKey();

			  CreateUniquenessConstraintRule( labelId, propertyKeyId );
			  CreateNodePropertyExistenceConstraint( labelId, propertyKeyId );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then
			  assertTrue( stats.Consistent );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNothingForNodeKeyAndPropertyExistenceConstraintOnSameLabelAndProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportNothingForNodeKeyAndPropertyExistenceConstraintOnSameLabelAndProperty()
		 {
			  // Given
			  int labelId = CreateLabel();
			  int propertyKeyId = CreatePropertyKey();

			  CreateNodeKeyConstraintRule( labelId, propertyKeyId );
			  CreateNodePropertyExistenceConstraint( labelId, propertyKeyId );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then
			  assertTrue( stats.Consistent );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldManageUnusedRecordsWithWeirdDataIn() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldManageUnusedRecordsWithWeirdDataIn()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicLong id = new java.util.concurrent.atomic.AtomicLong();
			  AtomicLong id = new AtomicLong();
			  fixture.apply( new TransactionAnonymousInnerClass39( this, id ) );
			  fixture.apply( new TransactionAnonymousInnerClass40( this, id ) );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then
			  assertTrue( stats.Consistent );
		 }

		 private class TransactionAnonymousInnerClass39 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 private AtomicLong _id;

			 public TransactionAnonymousInnerClass39( FullCheckIntegrationTest outerInstance, AtomicLong id )
			 {
				 this.outerInstance = outerInstance;
				 this._id = id;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  _id.set( next.Relationship() );
				  RelationshipRecord relationship = new RelationshipRecord( _id.get() );
				  relationship.FirstNode = -1;
				  relationship.SecondNode = -1;
				  relationship.InUse = true;
				  tx.Create( relationship );
			 }
		 }

		 private class TransactionAnonymousInnerClass40 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 private AtomicLong _id;

			 public TransactionAnonymousInnerClass40( FullCheckIntegrationTest outerInstance, AtomicLong id )
			 {
				 this.outerInstance = outerInstance;
				 this._id = id;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  RelationshipRecord relationship = new RelationshipRecord( _id.get() );
				  tx.Delete( relationship );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportCircularNodePropertyRecordChain() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportCircularNodePropertyRecordChain()
		 {
			  ShouldReportCircularPropertyRecordChain( RecordType.NODE, ( tx, next, propertyRecordId ) => tx.create( ( new NodeRecord( next.node() ) ).initialize(true, propertyRecordId, false, -1, Record.NO_LABELS_FIELD.longValue()) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportCircularRelationshipPropertyRecordChain() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportCircularRelationshipPropertyRecordChain()
		 {
			  int relType = CreateRelType();
			  ShouldReportCircularPropertyRecordChain(RecordType.RELATIONSHIP, (tx, next, propertyRecordId) =>
			  {
				long node = next.node();
				long relationship = next.relationship();
				tx.create( ( new NodeRecord( node ) ).initialize( true, -1, false, relationship, Record.NO_LABELS_FIELD.longValue() ) );
				RelationshipRecord relationshipRecord = new RelationshipRecord( relationship );
				relationshipRecord.FirstNode = node;
				relationshipRecord.SecondNode = node;
				relationshipRecord.Type = relType;
				relationshipRecord.NextProp = propertyRecordId;
				tx.create( relationshipRecord );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportMissingCountsStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportMissingCountsStore()
		 {
			  ShouldReportBadCountsStore( this.corruptFileIfExists );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportBrokenCountsStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportBrokenCountsStore()
		 {
			  ShouldReportBadCountsStore( File.delete );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldReportBadCountsStore(org.neo4j.function.ThrowingFunction<java.io.File,bool,java.io.IOException> fileAction) throws Exception
		 private void ShouldReportBadCountsStore( ThrowingFunction<File, bool, IOException> fileAction )
		 {
			  // given
			  bool aCorrupted = fileAction.Apply( fixture.databaseLayout().countStoreA() );
			  bool bCorrupted = fileAction.Apply( fixture.databaseLayout().countStoreB() );
			  assertTrue( aCorrupted || bCorrupted );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then report will be filed on Node inconsistent with the Property completing the circle
			  on( stats ).verify( RecordType.COUNTS, 1 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean corruptFileIfExists(java.io.File file) throws java.io.IOException
		 private bool CorruptFileIfExists( File file )
		 {
			  if ( file.exists() )
			  {
					using ( RandomAccessFile accessFile = new RandomAccessFile( file, "rw" ) )
					{
						 FileChannel channel = accessFile.Channel;
						 ByteBuffer buffer = ByteBuffer.allocate( 30 );
						 while ( buffer.hasRemaining() )
						 {
							  buffer.put( ( sbyte ) 9 );
						 }
						 buffer.flip();
						 channel.write( buffer );
					}
					return true;
			  }
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldReportCircularPropertyRecordChain(org.neo4j.consistency.RecordType expectedInconsistentRecordType, EntityCreator entityCreator) throws Exception
		 private void ShouldReportCircularPropertyRecordChain( RecordType expectedInconsistentRecordType, EntityCreator entityCreator )
		 {
			  // Given
			  fixture.apply( new TransactionAnonymousInnerClass41( this, entityCreator ) );

			  // When
			  ConsistencySummaryStatistics stats = Check();

			  // Then report will be filed on Node inconsistent with the Property completing the circle
			  on( stats ).verify( expectedInconsistentRecordType, 1 );
		 }

		 private class TransactionAnonymousInnerClass41 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 private Neo4Net.Consistency.checking.full.FullCheckIntegrationTest.EntityCreator _entityCreator;

			 public TransactionAnonymousInnerClass41( FullCheckIntegrationTest outerInstance, Neo4Net.Consistency.checking.full.FullCheckIntegrationTest.EntityCreator entityCreator )
			 {
				 this.outerInstance = outerInstance;
				 this._entityCreator = entityCreator;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  // Create property chain A --> B --> C --> D
				  //                             ↑           │
				  //                             └───────────┘
				  long a = next.Property();
				  long b = next.Property();
				  long c = next.Property();
				  long d = next.Property();
				  tx.Create( propertyRecordWithSingleIntProperty( a, next.PropertyKey(), -1, b ) );
				  tx.Create( propertyRecordWithSingleIntProperty( b, next.PropertyKey(), a, c ) );
				  tx.Create( propertyRecordWithSingleIntProperty( c, next.PropertyKey(), b, d ) );
				  tx.Create( propertyRecordWithSingleIntProperty( d, next.PropertyKey(), c, b ) );
				  _entityCreator.create( tx, next, a );
			 }

			 private PropertyRecord propertyRecordWithSingleIntProperty( long id, int propertyKeyId, long prev, long next )
			 {
				  PropertyRecord record = ( new PropertyRecord( id ) ).initialize( true, prev, next );
				  PropertyBlock block = new PropertyBlock();
				  PropertyStore.encodeValue( block, propertyKeyId, Values.intValue( 10 ), null, null, false );
				  record.AddPropertyBlock( block );
				  return record;
			 }
		 }

		 private interface EntityCreator
		 {
			  void Create( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next, long propertyRecordId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.consistency.report.ConsistencySummaryStatistics check() throws ConsistencyCheckIncompleteException
		 private ConsistencySummaryStatistics Check()
		 {
			  return Check( fixture.readOnlyDirectStoreAccess() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.consistency.report.ConsistencySummaryStatistics check(org.neo4j.kernel.api.direct.DirectStoreAccess stores) throws ConsistencyCheckIncompleteException
		 private ConsistencySummaryStatistics Check( DirectStoreAccess stores )
		 {
			  Config config = config();
			  FullCheck checker = new FullCheck( config, ProgressMonitorFactory.NONE, fixture.AccessStatistics, defaultConsistencyCheckThreadsNumber(), true );
			  return checker.Execute(stores, FormattedLog.toOutputStream(System.out), (report, method, message) =>
			  {
						  ISet<string> types = _allReports[report];
						  Debug.Assert( types != null );
						  types.remove( method );
			  });
		 }

		 private Config Config()
		 {
			  IDictionary<string, string> @params = stringMap( ConsistencyCheckSettings.consistency_check_property_owners.name(), "true", GraphDatabaseSettings.record_format.name(), RecordFormatName );
			  return Config.defaults( @params );
		 }

		 protected internal static RelationshipGroupRecord WithRelationships( RelationshipGroupRecord group, long @out, long @in, long loop )
		 {
			  group.FirstOut = @out;
			  group.FirstIn = @in;
			  group.FirstLoop = loop;
			  return group;
		 }

		 protected internal static RelationshipGroupRecord WithRelationship( RelationshipGroupRecord group, Direction direction, long rel )
		 {
			  switch ( direction.innerEnumValue )
			  {
			  case Direction.InnerEnum.OUTGOING:
					group.FirstOut = rel;
					break;
			  case Direction.InnerEnum.INCOMING:
					group.FirstIn = rel;
					break;
			  case Direction.InnerEnum.BOTH:
					group.FirstLoop = rel;
					break;
			  default:
					throw new System.ArgumentException( direction.name() );
			  }
			  return group;
		 }

		 protected internal static RelationshipRecord FirstInChains( RelationshipRecord relationship, int count )
		 {
			  relationship.FirstInFirstChain = true;
			  relationship.FirstPrevRel = count;
			  relationship.FirstInSecondChain = true;
			  relationship.SecondPrevRel = count;
			  return relationship;
		 }

		 protected internal static RelationshipGroupRecord WithNext( RelationshipGroupRecord group, long next )
		 {
			  group.Next = next;
			  return group;
		 }

		 protected internal static RelationshipGroupRecord WithOwner( RelationshipGroupRecord record, long owner )
		 {
			  record.OwningNode = owner;
			  return record;
		 }

		 protected internal virtual string RecordFormatName
		 {
			 get
			 {
				  return StringUtils.EMPTY;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int createLabel() throws Exception
		 private int CreateLabel()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.commons.lang3.mutable.MutableInt id = new org.apache.commons.lang3.mutable.MutableInt(-1);
			  MutableInt id = new MutableInt( -1 );

			  fixture.apply( new TransactionAnonymousInnerClass42( this, id ) );

			  return id.intValue();
		 }

		 private class TransactionAnonymousInnerClass42 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 private MutableInt _id;

			 public TransactionAnonymousInnerClass42( FullCheckIntegrationTest outerInstance, MutableInt id )
			 {
				 this.outerInstance = outerInstance;
				 this._id = id;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  int labelId = next.Label();
				  tx.NodeLabel( labelId, "label" );
				  _id.Value = labelId;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int createPropertyKey() throws Exception
		 private int CreatePropertyKey()
		 {
			  return CreatePropertyKey( "property" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int createPropertyKey(String propertyKey) throws Exception
		 private int CreatePropertyKey( string propertyKey )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.commons.lang3.mutable.MutableInt id = new org.apache.commons.lang3.mutable.MutableInt(-1);
			  MutableInt id = new MutableInt( -1 );

			  fixture.apply( new TransactionAnonymousInnerClass43( this, propertyKey, id ) );

			  return id.intValue();
		 }

		 private class TransactionAnonymousInnerClass43 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 private string _propertyKey;
			 private MutableInt _id;

			 public TransactionAnonymousInnerClass43( FullCheckIntegrationTest outerInstance, string propertyKey, MutableInt id )
			 {
				 this.outerInstance = outerInstance;
				 this._propertyKey = propertyKey;
				 this._id = id;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  int propertyKeyId = next.PropertyKey();
				  tx.PropertyKey( propertyKeyId, _propertyKey );
				  _id.Value = propertyKeyId;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int createRelType() throws Exception
		 private int CreateRelType()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.commons.lang3.mutable.MutableInt id = new org.apache.commons.lang3.mutable.MutableInt(-1);
			  MutableInt id = new MutableInt( -1 );

			  fixture.apply( new TransactionAnonymousInnerClass44( this, id ) );

			  return id.intValue();
		 }

		 private class TransactionAnonymousInnerClass44 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 private MutableInt _id;

			 public TransactionAnonymousInnerClass44( FullCheckIntegrationTest outerInstance, MutableInt id )
			 {
				 this.outerInstance = outerInstance;
				 this._id = id;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  int relTypeId = next.RelationshipType();
				  tx.RelationshipType( relTypeId, "relType" );
				  _id.Value = relTypeId;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createIndexRule(final int labelId, final int... propertyKeyIds) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private void CreateIndexRule( int labelId, params int[] propertyKeyIds )
		 {
			  fixture.apply( new TransactionAnonymousInnerClass45( this, labelId, propertyKeyIds ) );
		 }

		 private class TransactionAnonymousInnerClass45 : GraphStoreFixture.Transaction
		 {
			 private readonly FullCheckIntegrationTest _outerInstance;

			 private int _labelId;
			 private int[] _propertyKeyIds;

			 public TransactionAnonymousInnerClass45( FullCheckIntegrationTest outerInstance, int labelId, int[] propertyKeyIds )
			 {
				 this.outerInstance = outerInstance;
				 this._labelId = labelId;
				 this._propertyKeyIds = propertyKeyIds;
			 }

			 protected internal override void transactionData( GraphStoreFixture.TransactionDataBuilder tx, GraphStoreFixture.IdGenerator next )
			 {
				  int id = ( int ) next.Schema();

				  DynamicRecord recordBefore = new DynamicRecord( id );
				  DynamicRecord recordAfter = recordBefore.Clone();

				  StoreIndexDescriptor index = forSchema( forLabel( _labelId, _propertyKeyIds ), _descriptor ).withId( id );
				  ICollection<DynamicRecord> records = SerializeRule( index, recordAfter );

				  tx.CreateSchema( singleton( recordBefore ), records, index );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void createUniquenessConstraintRule(final int labelId, final int... propertyKeyIds)
		 private void CreateUniquenessConstraintRule( int labelId, params int[] propertyKeyIds )
		 {
			  SchemaStore schemaStore = ( SchemaStore ) fixture.directStoreAccess().nativeStores().SchemaStore;

			  long ruleId1 = schemaStore.NextId();
			  long ruleId2 = schemaStore.NextId();

			  StoreIndexDescriptor indexRule = uniqueForSchema( forLabel( labelId, propertyKeyIds ), _descriptor ).withIds( ruleId1, ruleId2 );
			  ConstraintRule uniqueRule = ConstraintRule.constraintRule( ruleId2, ConstraintDescriptorFactory.uniqueForLabel( labelId, propertyKeyIds ), ruleId1 );

			  WriteToSchemaStore( schemaStore, indexRule );
			  WriteToSchemaStore( schemaStore, uniqueRule );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void createNodeKeyConstraintRule(final int labelId, final int... propertyKeyIds)
		 private void CreateNodeKeyConstraintRule( int labelId, params int[] propertyKeyIds )
		 {
			  SchemaStore schemaStore = ( SchemaStore ) fixture.directStoreAccess().nativeStores().SchemaStore;

			  long ruleId1 = schemaStore.NextId();
			  long ruleId2 = schemaStore.NextId();

			  StoreIndexDescriptor indexRule = uniqueForSchema( forLabel( labelId, propertyKeyIds ), _descriptor ).withIds( ruleId1, ruleId2 );
			  ConstraintRule nodeKeyRule = ConstraintRule.constraintRule( ruleId2, ConstraintDescriptorFactory.nodeKeyForLabel( labelId, propertyKeyIds ), ruleId1 );

			  WriteToSchemaStore( schemaStore, indexRule );
			  WriteToSchemaStore( schemaStore, nodeKeyRule );
		 }

		 private void CreateNodePropertyExistenceConstraint( int labelId, int propertyKeyId )
		 {
			  SchemaStore schemaStore = ( SchemaStore ) fixture.directStoreAccess().nativeStores().SchemaStore;
			  ConstraintRule rule = nodePropertyExistenceConstraintRule( schemaStore.NextId(), labelId, propertyKeyId );
			  WriteToSchemaStore( schemaStore, rule );
		 }

		 private void CreateRelationshipPropertyExistenceConstraint( int relTypeId, int propertyKeyId )
		 {
			  SchemaStore schemaStore = ( SchemaStore ) fixture.directStoreAccess().nativeStores().SchemaStore;
			  ConstraintRule rule = relPropertyExistenceConstraintRule( schemaStore.NextId(), relTypeId, propertyKeyId );
			  WriteToSchemaStore( schemaStore, rule );
		 }

		 private void WriteToSchemaStore( SchemaStore schemaStore, SchemaRule rule )
		 {
			  ICollection<DynamicRecord> records = schemaStore.AllocateFrom( rule );
			  foreach ( DynamicRecord record in records )
			  {
					schemaStore.UpdateRecord( record );
			  }
		 }

		 private static KernelTransaction TransactionOn( GraphDatabaseService db )
		 {
			  DependencyResolver resolver = ( ( GraphDatabaseAPI ) db ).DependencyResolver;
			  ThreadToStatementContextBridge bridge = resolver.ResolveDependency( typeof( ThreadToStatementContextBridge ) );
			  return bridge.GetKernelTransactionBoundToThisThread( true );
		 }

		 private class Reference<T>
		 {
			  internal T Value;

			  internal virtual void Set( T value )
			  {
					this.Value = value;
			  }

			  internal virtual T Get()
			  {
					return Value;
			  }

			  public override string ToString()
			  {
					return Value.ToString();
			  }
		 }

		 public sealed class ConsistencySummaryVerifier
		 {
			  internal readonly ConsistencySummaryStatistics Stats;
			  internal readonly ISet<RecordType> Types = new HashSet<RecordType>();
			  internal long Total;

			  public static ConsistencySummaryVerifier On( ConsistencySummaryStatistics stats )
			  {
					return new ConsistencySummaryVerifier( stats );
			  }

			  internal ConsistencySummaryVerifier( ConsistencySummaryStatistics stats )
			  {
					this.Stats = stats;
			  }

			  public ConsistencySummaryVerifier Verify( RecordType type, int inconsistencies )
			  {
					if ( !Types.Add( type ) )
					{
						 throw new System.InvalidOperationException( "Tried to verify the same type twice: " + type );
					}
					assertEquals( "Inconsistencies of type: " + type, inconsistencies, Stats.getInconsistencyCountForRecordType( type ) );
					Total += inconsistencies;
					return this;
			  }

			  public void AndThatsAllFolks()
			  {
					assertEquals( "Total number of inconsistencies: " + Stats, Total, Stats.TotalInconsistencyCount );
			  }
		 }

		 private static ICollection<DynamicRecord> SerializeRule( SchemaRule rule, params DynamicRecord[] records )
		 {
			  sbyte[] data = SchemaRuleSerialization.serialize( rule );
			  DynamicRecordAllocator dynamicRecordAllocator = new ReusableRecordsCompositeAllocator( asList( records ), schemaAllocator );
			  ICollection<DynamicRecord> result = new List<DynamicRecord>();
			  AbstractDynamicStore.allocateRecordsFromBytes( result, data, dynamicRecordAllocator );
			  return result;
		 }

		 private static DynamicRecordAllocator schemaAllocator = new DynamicRecordAllocatorAnonymousInnerClass();

		 private class DynamicRecordAllocatorAnonymousInnerClass : DynamicRecordAllocator
		 {
			 private int next = 10000; // we start high to not conflict with real ids

			 public int RecordDataSize
			 {
				 get
				 {
					  return SchemaStore.BLOCK_SIZE;
				 }
			 }

			 public DynamicRecord nextRecord()
			 {
				  return new DynamicRecord( next++ );
			 }
		 }
	}

}