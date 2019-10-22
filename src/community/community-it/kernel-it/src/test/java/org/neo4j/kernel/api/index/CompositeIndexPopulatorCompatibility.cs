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
	using IndexQuery = Neo4Net.Internal.Kernel.Api.IndexQuery;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using PhaseTracker = Neo4Net.Kernel.Impl.Api.index.PhaseTracker;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using QueryResultComparingIndexReader = Neo4Net.Storageengine.Api.schema.QueryResultComparingIndexReader;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueTuple = Neo4Net.Values.Storable.ValueTuple;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.index.IndexQueryHelper.add;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.ByteBufferFactory.heapBufferFactory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite that provides test cases for verifying" + " IndexProvider implementations. Each index provider that is to be tested by this suite" + " must create their own test class extending IndexProviderCompatibilityTestSuite." + " The @Ignore annotation doesn't prevent these tests to run, it rather removes some annoying" + " errors or warnings in some IDEs about test classes needing a public zero-arg constructor.") public class CompositeIndexPopulatorCompatibility extends IndexProviderCompatibilityTestSuite.Compatibility
	public class CompositeIndexPopulatorCompatibility : IndexProviderCompatibilityTestSuite.Compatibility
	{
		 public CompositeIndexPopulatorCompatibility( IndexProviderCompatibilityTestSuite testSuite, IndexDescriptor descriptor ) : base( testSuite, descriptor )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite") public static class General extends CompositeIndexPopulatorCompatibility
		 public class General : CompositeIndexPopulatorCompatibility
		 {
			  public General( IndexProviderCompatibilityTestSuite testSuite ) : base( testSuite, TestIndexDescriptorFactory.forLabel( 1000, 100, 200 ) )
			  {
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvidePopulatorThatAcceptsDuplicateEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldProvidePopulatorThatAcceptsDuplicateEntries()
			  {
					// when
					IndexSamplingConfig indexSamplingConfig = new IndexSamplingConfig( Config.defaults() );
					WithPopulator( IndexProvider.getPopulator( Descriptor, indexSamplingConfig, heapBufferFactory( 1024 ) ), p => p.add( Arrays.asList( add( 1, Descriptor.schema(), "v1", "v2" ), add(2, Descriptor.schema(), "v1", "v2") ) ) );

					// then
					using ( IndexAccessor accessor = IndexProvider.getOnlineAccessor( Descriptor, indexSamplingConfig ) )
					{
						 using ( IndexReader reader = new QueryResultComparingIndexReader( accessor.NewReader() ) )
						 {
							  LongIterator nodes = reader.Query( IndexQuery.exact( 1, "v1" ), IndexQuery.exact( 1, "v2" ) );
							  assertEquals( asSet( 1L, 2L ), PrimitiveLongCollections.toSet( nodes ) );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Not a test. This is a compatibility suite") public static class Unique extends CompositeIndexPopulatorCompatibility
		 public class Unique : CompositeIndexPopulatorCompatibility
		 {
			  internal Value Value1 = Values.of( "value1" );
			  internal Value Value2 = Values.of( "value2" );
			  internal Value Value3 = Values.of( "value3" );
			  internal int NodeId1 = 3;
			  internal int NodeId2 = 4;

			  public Unique( IndexProviderCompatibilityTestSuite testSuite ) : base( testSuite, TestIndexDescriptorFactory.uniqueForLabel( 1000, 100, 200 ) )
			  {
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEnforceUniqueConstraintsDirectly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldEnforceUniqueConstraintsDirectly()
			  {
					// when
					IndexSamplingConfig indexSamplingConfig = new IndexSamplingConfig( Config.defaults() );
					WithPopulator(IndexProvider.getPopulator(Descriptor, indexSamplingConfig, heapBufferFactory(1024)), p =>
					{
					 try
					 {
						  p.add( Arrays.asList( IndexEntryUpdate.Add( NodeId1, Descriptor.schema(), Value1, Value2 ), IndexEntryUpdate.Add(NodeId2, Descriptor.schema(), Value1, Value2) ) );
						  TestNodePropertyAccessor propertyAccessor = new TestNodePropertyAccessor( NodeId1, Descriptor.schema(), Value1, Value2 );
						  propertyAccessor.AddNode( NodeId2, Descriptor.schema(), Value1, Value2 );
						  p.scanCompleted( PhaseTracker.nullInstance );
						  p.verifyDeferredConstraints( propertyAccessor );

						  fail( "expected exception" );
					 }
					 // then
					 catch ( IndexEntryConflictException conflict )
					 {
						  assertEquals( NodeId1, conflict.ExistingNodeId );
						  assertEquals( ValueTuple.of( Value1, Value2 ), conflict.PropertyValues );
						  assertEquals( NodeId2, conflict.AddedNodeId );
					 }
					}, false);
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotRestrictUpdatesDifferingOnSecondProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldNotRestrictUpdatesDifferingOnSecondProperty()
			  {
					// given
					IndexSamplingConfig indexSamplingConfig = new IndexSamplingConfig( Config.defaults() );
					WithPopulator(IndexProvider.getPopulator(Descriptor, indexSamplingConfig, heapBufferFactory(1024)), p =>
					{
					 // when
					 p.add( Arrays.asList( IndexEntryUpdate.Add( NodeId1, Descriptor.schema(), Value1, Value2 ), IndexEntryUpdate.Add(NodeId2, Descriptor.schema(), Value1, Value3) ) );

					 TestNodePropertyAccessor propertyAccessor = new TestNodePropertyAccessor( NodeId1, Descriptor.schema(), Value1, Value2 );
					 propertyAccessor.AddNode( NodeId2, Descriptor.schema(), Value1, Value3 );

					 // then this should pass fine
					 p.verifyDeferredConstraints( propertyAccessor );
					});
			  }
		 }
	}

}