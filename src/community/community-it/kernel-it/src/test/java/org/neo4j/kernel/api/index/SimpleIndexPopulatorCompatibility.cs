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
namespace Neo4Net.Kernel.Api.Index
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using Ignore = org.junit.Ignore;
	using Test = org.junit.Test;


	using PrimitiveLongCollections = Neo4Net.Collections.PrimitiveLongCollections;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using IndexOrder = Neo4Net.Kernel.Api.Internal.IndexOrder;
	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using IndexNotApplicableKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotApplicableKernelException;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.Api.schema.index.TestIndexDescriptorFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IndexUpdateMode = Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode;
	using PhaseTracker = Neo4Net.Kernel.Impl.Api.index.PhaseTracker;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using NodeValueIterator = Neo4Net.Kernel.Impl.Index.Schema.NodeValueIterator;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader;
	using QueryResultComparingIndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.QueryResultComparingIndexReader;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueTuple = Neo4Net.Values.Storable.ValueTuple;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.InternalIndexState.FAILED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.index.IndexEntryUpdate.add;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.schema.ByteBufferFactory.heapBufferFactory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.stringValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite that provides test cases for verifying" + " IndexProvider implementations. Each index provider that is to be tested by this suite" + " must create their own test class extending IndexProviderCompatibilityTestSuite." + " The @Ignore annotation doesn't prevent these tests to run, it rather removes some annoying" + " errors or warnings in some IDEs about test classes needing a public zero-arg constructor.") public class SimpleIndexPopulatorCompatibility extends IndexProviderCompatibilityTestSuite.Compatibility
	public class SimpleIndexPopulatorCompatibility : IndexProviderCompatibilityTestSuite.Compatibility
	{
		 public SimpleIndexPopulatorCompatibility( IndexProviderCompatibilityTestSuite testSuite, IndexDescriptor descriptor ) : base( testSuite, descriptor )
		 {
		 }

		 internal readonly IndexSamplingConfig IndexSamplingConfig = new IndexSamplingConfig( Config.defaults() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStorePopulationFailedForRetrievalFromProviderLater() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStorePopulationFailedForRetrievalFromProviderLater()
		 {
			  // GIVEN
			  string failure = "The contrived failure";
			  IndexSamplingConfig indexSamplingConfig = new IndexSamplingConfig( Config.defaults() );
			  // WHEN (this will attempt to call close)
			  WithPopulator( IndexProvider.getPopulator( Descriptor, indexSamplingConfig, heapBufferFactory( 1024 ) ), p => p.markAsFailed( failure ), false );
			  // THEN
			  assertThat( IndexProvider.getPopulationFailure( Descriptor ), containsString( failure ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportInitialStateAsFailedIfPopulationFailed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportInitialStateAsFailedIfPopulationFailed()
		 {
			  // GIVEN
			  IndexSamplingConfig indexSamplingConfig = new IndexSamplingConfig( Config.defaults() );
			  WithPopulator(IndexProvider.getPopulator(Descriptor, indexSamplingConfig, heapBufferFactory(1024)), p =>
			  {
				string failure = "The contrived failure";

				// WHEN
				p.markAsFailed( failure );
				p.close( false );

				// THEN
				assertEquals( FAILED, IndexProvider.getInitialState( Descriptor ) );
			  }, false);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToDropAClosedIndexPopulator() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToDropAClosedIndexPopulator()
		 {
			  // GIVEN
			  IndexSamplingConfig indexSamplingConfig = new IndexSamplingConfig( Config.defaults() );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexPopulator p = indexProvider.getPopulator(descriptor, indexSamplingConfig, heapBufferFactory(1024));
			  IndexPopulator p = IndexProvider.getPopulator( Descriptor, indexSamplingConfig, heapBufferFactory( 1024 ) );
			  p.Close( false );

			  // WHEN
			  p.Drop();

			  // THEN - no exception should be thrown (it's been known to!)
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyUpdatesIdempotently() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyUpdatesIdempotently()
		 {
			  // GIVEN
			  IndexSamplingConfig indexSamplingConfig = new IndexSamplingConfig( Config.defaults() );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.values.storable.Value propertyValue = Neo4Net.values.storable.Values.of("value1");
			  Value propertyValue = Values.of( "value1" );
			  WithPopulator(IndexProvider.getPopulator(Descriptor, indexSamplingConfig, heapBufferFactory(1024)), p =>
			  {
				long nodeId = 1;

				// update using populator...
				IndexEntryUpdate<SchemaDescriptor> update = add( nodeId, Descriptor.schema(), propertyValue );
				p.add( singletonList( update ) );
				// ...is the same as update using updater
				using ( IndexUpdater updater = p.newPopulatingUpdater( ( node, propertyId ) => propertyValue ) )
				{
					 updater.Process( update );
				}
			  });

			  // THEN
			  using ( IndexAccessor accessor = IndexProvider.getOnlineAccessor( Descriptor, indexSamplingConfig ) )
			  {
					using ( IndexReader reader = new QueryResultComparingIndexReader( accessor.NewReader() ) )
					{
						 int propertyKeyId = Descriptor.schema().PropertyId;
						 LongIterator nodes = reader.Query( IndexQuery.exact( propertyKeyId, propertyValue ) );
						 assertEquals( asSet( 1L ), PrimitiveLongCollections.toSet( nodes ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPopulateWithAllValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPopulateWithAllValues()
		 {
			  // GIVEN
			  WithPopulator( IndexProvider.getPopulator( Descriptor, IndexSamplingConfig, heapBufferFactory( 1024 ) ), p => p.add( Updates( ValueSet1 ) ) );

			  // THEN
			  AssertHasAllValues( ValueSet1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateWithAllValuesDuringPopulation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateWithAllValuesDuringPopulation()
		 {
			  // GIVEN
			  WithPopulator(IndexProvider.getPopulator(Descriptor, IndexSamplingConfig, heapBufferFactory(1024)), p =>
			  {
				using ( IndexUpdater updater = p.newPopulatingUpdater( this.valueSet1Lookup ) )
				{
					 foreach ( NodeAndValue entry in ValueSet1 )
					 {
						  updater.Process( add( entry.NodeId, Descriptor.schema(), entry.Value ) );
					 }
				}
			  });

			  // THEN
			  AssertHasAllValues( ValueSet1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPopulateAndUpdate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPopulateAndUpdate()
		 {
			  // GIVEN
			  WithPopulator( IndexProvider.getPopulator( Descriptor, IndexSamplingConfig, heapBufferFactory( 1024 ) ), p => p.add( Updates( ValueSet1 ) ) );

			  using ( IndexAccessor accessor = IndexProvider.getOnlineAccessor( Descriptor, IndexSamplingConfig ) )
			  {
					// WHEN
					using ( IndexUpdater updater = accessor.NewUpdater( IndexUpdateMode.ONLINE ) )
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<IndexEntryUpdate<?>> updates = updates(valueSet2);
						 IList<IndexEntryUpdate<object>> updates = updates( ValueSet2 );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (IndexEntryUpdate<?> update : updates)
						 foreach ( IndexEntryUpdate<object> update in updates )
						 {
							  updater.Process( update );
						 }
					}

					// THEN
					using ( IndexReader reader = new QueryResultComparingIndexReader( accessor.NewReader() ) )
					{
						 int propertyKeyId = Descriptor.schema().PropertyId;
						 foreach ( NodeAndValue entry in Iterables.concat( ValueSet1, ValueSet2 ) )
						 {
							  NodeValueIterator nodes = new NodeValueIterator();
							  reader.Query( nodes, IndexOrder.NONE, false, IndexQuery.exact( propertyKeyId, entry.Value ) );
							  assertEquals( entry.NodeId, nodes.Next() );
							  assertFalse( nodes.HasNext() );
						 }
					}
			  }
		 }

		 /// <summary>
		 /// This test target a bug around minimal splitter in gbpTree and unique index populator. It goes like this:
		 /// Given a set of updates (value,entityId):
		 /// - ("A01",1), ("A90",3), ("A9",2)
		 /// If ("A01",1) and ("A90",3) would cause a split to occur they would produce a minimal splitter ("A9",3).
		 /// Note that the value in this minimal splitter is equal to our last update ("A9",2).
		 /// When making insertions with the unique populator we don't compare IEntityId which would means ("A9",2)
		 /// ends up to the right of ("A9",3), even though it belongs to the left because of IEntityId being smaller.
		 /// At this point the tree is in an inconsistent (key on wrong side of splitter).
		 /// 
		 /// To work around this problem the IEntityId is only kept in minimal splitter if strictly necessary to divide
		 /// left from right. This means the minimal splitter between ("A01",1) and ("A90",3) is ("A9",-1) and ("A9",2)
		 /// will correctly be placed on the right side of this splitter.
		 /// 
		 /// To trigger this scenario this test first insert a bunch of values that are all unique and that will cause a
		 /// split to happen. This is the firstBatch.
		 /// The second batch are constructed so that at least one of them will have a value equal to the splitter key
		 /// constructed during the firstBatch.
		 /// It's important that the secondBatch has ids that are lower than the first batch to align with example described above.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPopulateAndRemoveEntriesWithSimilarMinimalSplitter() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPopulateAndRemoveEntriesWithSimilarMinimalSplitter()
		 {
			  string prefix = "Work out your own salvation. Do not depend on others. ";
			  int nbrOfNodes = 200;
			  long nodeId = 0;

			  // Second batch has lower ids
			  IList<NodeAndValue> secondBatch = new List<NodeAndValue>();
			  for ( int i = 0; i < nbrOfNodes; i++ )
			  {
					secondBatch.Add( new NodeAndValue( nodeId++, stringValue( prefix + i ) ) );
			  }

			  // First batch has higher ids and minimal splitter among values in first batch will be found among second batch
			  IList<NodeAndValue> firstBatch = new List<NodeAndValue>();
			  for ( int i = 0; i < nbrOfNodes; i++ )
			  {
					firstBatch.Add( new NodeAndValue( nodeId++, stringValue( prefix + i + " " + i ) ) );
			  }

			  WithPopulator(IndexProvider.getPopulator(Descriptor, IndexSamplingConfig, heapBufferFactory(1024)), p =>
			  {
				p.add( Updates( firstBatch ) );
				p.add( Updates( secondBatch ) );

				// Index should be consistent
			  });

			  IList<NodeAndValue> toRemove = new List<NodeAndValue>();
			  ( ( IList<NodeAndValue> )toRemove ).AddRange( firstBatch );
			  ( ( IList<NodeAndValue> )toRemove ).AddRange( secondBatch );
			  Collections.shuffle( toRemove );

			  // And we should be able to remove the entries in any order
			  using ( IndexAccessor accessor = IndexProvider.getOnlineAccessor( Descriptor, IndexSamplingConfig ) )
			  {
					// WHEN
					using ( IndexUpdater updater = accessor.NewUpdater( IndexUpdateMode.ONLINE ) )
					{
						 foreach ( NodeAndValue nodeAndValue in toRemove )
						 {
							  updater.Process( IndexEntryUpdate.Remove( nodeAndValue.NodeId, Descriptor, nodeAndValue.Value ) );
						 }
					}

					// THEN
					using ( IndexReader reader = new QueryResultComparingIndexReader( accessor.NewReader() ) )
					{
						 int propertyKeyId = Descriptor.schema().PropertyId;
						 foreach ( NodeAndValue nodeAndValue in toRemove )
						 {
							  NodeValueIterator nodes = new NodeValueIterator();
							  reader.Query( nodes, IndexOrder.NONE, false, IndexQuery.exact( propertyKeyId, nodeAndValue.Value ) );
							  bool anyHits = false;

							  StringJoiner nodesStillLeft = new StringJoiner( ", ", "[", "]" );
							  while ( nodes.HasNext() )
							  {
									anyHits = true;
									nodesStillLeft.add( Convert.ToString( nodes.Next() ) );
							  }
							  assertFalse( "Expected this query to have zero hits but found " + nodesStillLeft.ToString(), anyHits );
						 }
					}
			  }
		 }

		 private Value ValueSet1Lookup( long nodeId, int propertyId )
		 {
			  foreach ( NodeAndValue x in ValueSet1 )
			  {
					if ( x.NodeId == nodeId )
					{
						 return x.Value;
					}
			  }
			  return Values.NO_VALUE;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertHasAllValues(java.util.List<NodeAndValue> values) throws java.io.IOException, Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotApplicableKernelException
		 private void AssertHasAllValues( IList<NodeAndValue> values )
		 {
			  using ( IndexAccessor accessor = IndexProvider.getOnlineAccessor( Descriptor, IndexSamplingConfig ) )
			  {
					using ( IndexReader reader = new QueryResultComparingIndexReader( accessor.NewReader() ) )
					{
						 int propertyKeyId = Descriptor.schema().PropertyId;
						 foreach ( NodeAndValue entry in values )
						 {
							  NodeValueIterator nodes = new NodeValueIterator();
							  reader.Query( nodes, IndexOrder.NONE, false, IndexQuery.exact( propertyKeyId, entry.Value ) );
							  assertEquals( entry.NodeId, nodes.Next() );
							  assertFalse( nodes.HasNext() );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite") public static class General extends SimpleIndexPopulatorCompatibility
		 public class General : SimpleIndexPopulatorCompatibility
		 {
			  public General( IndexProviderCompatibilityTestSuite testSuite ) : base( testSuite, TestIndexDescriptorFactory.forLabel( 1000, 100 ) )
			  {
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvidePopulatorThatAcceptsDuplicateEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldProvidePopulatorThatAcceptsDuplicateEntries()
			  {
					// when
					long offset = ValueSet1.Count;
					WithPopulator(IndexProvider.getPopulator(Descriptor, IndexSamplingConfig, heapBufferFactory(1024)), p =>
					{
					 p.add( Updates( ValueSet1, 0 ) );
					 p.add( Updates( ValueSet1, offset ) );
					});

					// then
					using ( IndexAccessor accessor = IndexProvider.getOnlineAccessor( Descriptor, IndexSamplingConfig ) )
					{
						 using ( IndexReader reader = new QueryResultComparingIndexReader( accessor.NewReader() ) )
						 {
							  int propertyKeyId = Descriptor.schema().PropertyId;
							  foreach ( NodeAndValue entry in ValueSet1 )
							  {
									NodeValueIterator nodes = new NodeValueIterator();
									reader.Query( nodes, IndexOrder.NONE, false, IndexQuery.exact( propertyKeyId, entry.Value ) );
									assertEquals( entry.Value.ToString(), asSet(entry.NodeId, entry.NodeId + offset), PrimitiveLongCollections.toSet(nodes) );
							  }
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite") public static class Unique extends SimpleIndexPopulatorCompatibility
		 public class Unique : SimpleIndexPopulatorCompatibility
		 {
			  public Unique( IndexProviderCompatibilityTestSuite testSuite ) : base( testSuite, TestIndexDescriptorFactory.uniqueForLabel( 1000, 100 ) )
			  {
			  }

			  /// <summary>
			  /// This is also checked by the UniqueConstraintCompatibility test, only not on this abstraction level.
			  /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvidePopulatorThatEnforcesUniqueConstraints() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldProvidePopulatorThatEnforcesUniqueConstraints()
			  {
					// when
					Value value = Values.of( "value1" );
					int nodeId1 = 1;
					int nodeId2 = 2;

					WithPopulator(IndexProvider.getPopulator(Descriptor, IndexSamplingConfig, heapBufferFactory(1024)), p =>
					{
					 try
					 {
						  p.add( Arrays.asList( add( nodeId1, Descriptor.schema(), value ), add(nodeId2, Descriptor.schema(), value) ) );
						  TestNodePropertyAccessor propertyAccessor = new TestNodePropertyAccessor( nodeId1, Descriptor.schema(), value );
						  propertyAccessor.AddNode( nodeId2, Descriptor.schema(), value );
						  p.scanCompleted( PhaseTracker.nullInstance );
						  p.verifyDeferredConstraints( propertyAccessor );

						  fail( "expected exception" );
					 }
					 // then
					 catch ( Exception e )
					 {
						  Exception root = Exceptions.rootCause( e );
						  if ( root is IndexEntryConflictException )
						  {
								IndexEntryConflictException conflict = ( IndexEntryConflictException )root;
								assertEquals( nodeId1, conflict.ExistingNodeId );
								assertEquals( ValueTuple.of( value ), conflict.PropertyValues );
								assertEquals( nodeId2, conflict.AddedNodeId );
						  }
						  else
						  {
								throw e;
						  }
					 }
					}, false);
			  }
		 }
	}

}