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
namespace Neo4Net.Kernel.Api.Impl.Schema.reader
{
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Mock = org.mockito.Mock;
	using MockitoJUnitRunner = org.mockito.junit.MockitoJUnitRunner;


	using PrimitiveLongCollections = Neo4Net.Collections.PrimitiveLongCollections;
	using PrimitiveLongResourceCollections = Neo4Net.Collections.PrimitiveLongResourceCollections;
	using TaskCoordinator = Neo4Net.Helpers.TaskCoordinator;
	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException;
	using PartitionSearcher = Neo4Net.Kernel.Api.Impl.Index.partition.PartitionSearcher;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.Api.schema.index.TestIndexDescriptorFactory;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using IndexSample = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSample;
	using IndexSampler = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSampler;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.stringValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(MockitoJUnitRunner.class) public class PartitionedIndexReaderTest
	public class PartitionedIndexReaderTest
	{
		 private const int PROP_KEY = 1;
		 private const int LABEL_ID = 0;

		 private IndexDescriptor _schemaIndexDescriptor = TestIndexDescriptorFactory.forLabel( LABEL_ID, PROP_KEY );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock private Neo4Net.kernel.impl.api.index.sampling.IndexSamplingConfig samplingConfig;
		 private IndexSamplingConfig _samplingConfig;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock private Neo4Net.helpers.TaskCoordinator taskCoordinator;
		 private TaskCoordinator _taskCoordinator;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock private Neo4Net.kernel.api.impl.index.partition.PartitionSearcher partitionSearcher1;
		 private PartitionSearcher _partitionSearcher1;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock private Neo4Net.kernel.api.impl.index.partition.PartitionSearcher partitionSearcher2;
		 private PartitionSearcher _partitionSearcher2;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock private Neo4Net.kernel.api.impl.index.partition.PartitionSearcher partitionSearcher3;
		 private PartitionSearcher _partitionSearcher3;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock private SimpleIndexReader indexReader1;
		 private SimpleIndexReader _indexReader1;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock private SimpleIndexReader indexReader2;
		 private SimpleIndexReader _indexReader2;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock private SimpleIndexReader indexReader3;
		 private SimpleIndexReader _indexReader3;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void partitionedReaderCloseAllSearchers() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PartitionedReaderCloseAllSearchers()
		 {
			  PartitionedIndexReader partitionedIndexReader = CreatePartitionedReader();

			  partitionedIndexReader.Close();

			  verify( _partitionSearcher1 ).close();
			  verify( _partitionSearcher2 ).close();
			  verify( _partitionSearcher3 ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void seekOverAllPartitions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SeekOverAllPartitions()
		 {
			  PartitionedIndexReader indexReader = CreatePartitionedReaderFromReaders();

			  IndexQuery.ExactPredicate query = IndexQuery.exact( 1, "Test" );
			  when( _indexReader1.query( query ) ).thenReturn( PrimitiveLongResourceCollections.iterator( null, 1 ) );
			  when( _indexReader2.query( query ) ).thenReturn( PrimitiveLongResourceCollections.iterator( null, 2 ) );
			  when( _indexReader3.query( query ) ).thenReturn( PrimitiveLongResourceCollections.iterator( null, 3 ) );

			  LongSet results = PrimitiveLongCollections.asSet( indexReader.Query( query ) );
			  VerifyResult( results );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rangeSeekByNumberOverPartitions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RangeSeekByNumberOverPartitions()
		 {
			  PartitionedIndexReader indexReader = CreatePartitionedReaderFromReaders();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> query = Neo4Net.Kernel.Api.Internal.IndexQuery.range(1, 1, true, 2, true);
			  IndexQuery.RangePredicate<object> query = IndexQuery.range( 1, 1, true, 2, true );
			  when( _indexReader1.query( query ) ).thenReturn( PrimitiveLongResourceCollections.iterator( null, 1 ) );
			  when( _indexReader2.query( query ) ).thenReturn( PrimitiveLongResourceCollections.iterator( null, 2 ) );
			  when( _indexReader3.query( query ) ).thenReturn( PrimitiveLongResourceCollections.iterator( null, 3 ) );

			  LongSet results = PrimitiveLongCollections.asSet( indexReader.Query( query ) );
			  VerifyResult( results );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rangeSeekByStringOverPartitions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RangeSeekByStringOverPartitions()
		 {
			  PartitionedIndexReader indexReader = CreatePartitionedReaderFromReaders();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.Kernel.Api.Internal.IndexQuery.RangePredicate<?> query = Neo4Net.Kernel.Api.Internal.IndexQuery.range(1, "a", false, "b", true);
			  IndexQuery.RangePredicate<object> query = IndexQuery.range( 1, "a", false, "b", true );
			  when( _indexReader1.query( query ) ).thenReturn( PrimitiveLongResourceCollections.iterator( null, 1 ) );
			  when( _indexReader2.query( query ) ).thenReturn( PrimitiveLongResourceCollections.iterator( null, 2 ) );
			  when( _indexReader3.query( query ) ).thenReturn( PrimitiveLongResourceCollections.iterator( null, 3 ) );

			  LongSet results = PrimitiveLongCollections.asSet( indexReader.Query( query ) );
			  VerifyResult( results );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rangeSeekByPrefixOverPartitions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RangeSeekByPrefixOverPartitions()
		 {
			  PartitionedIndexReader indexReader = CreatePartitionedReaderFromReaders();
			  IndexQuery.StringPrefixPredicate query = IndexQuery.stringPrefix( 1, stringValue( "prefix" ) );
			  when( _indexReader1.query( query ) ).thenReturn( PrimitiveLongResourceCollections.iterator( null, 1 ) );
			  when( _indexReader2.query( query ) ).thenReturn( PrimitiveLongResourceCollections.iterator( null, 2 ) );
			  when( _indexReader3.query( query ) ).thenReturn( PrimitiveLongResourceCollections.iterator( null, 3 ) );

			  LongSet results = PrimitiveLongCollections.asSet( indexReader.Query( query ) );
			  VerifyResult( results );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void scanOverPartitions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ScanOverPartitions()
		 {
			  PartitionedIndexReader indexReader = CreatePartitionedReaderFromReaders();
			  IndexQuery.ExistsPredicate query = IndexQuery.exists( 1 );
			  when( _indexReader1.query( query ) ).thenReturn( PrimitiveLongResourceCollections.iterator( null, 1 ) );
			  when( _indexReader2.query( query ) ).thenReturn( PrimitiveLongResourceCollections.iterator( null, 2 ) );
			  when( _indexReader3.query( query ) ).thenReturn( PrimitiveLongResourceCollections.iterator( null, 3 ) );

			  LongSet results = PrimitiveLongCollections.asSet( indexReader.Query( query ) );
			  VerifyResult( results );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void countNodesOverPartitions()
		 public virtual void CountNodesOverPartitions()
		 {
			  PartitionedIndexReader indexReader = CreatePartitionedReaderFromReaders();
			  when( _indexReader1.countIndexedNodes( 1, new int[] { PROP_KEY }, Values.of( "a" ) ) ).thenReturn( 1L );
			  when( _indexReader2.countIndexedNodes( 1, new int[] { PROP_KEY }, Values.of( "a" ) ) ).thenReturn( 2L );
			  when( _indexReader3.countIndexedNodes( 1, new int[] { PROP_KEY }, Values.of( "a" ) ) ).thenReturn( 3L );

			  assertEquals( 6, indexReader.CountIndexedNodes( 1, new int[] { PROP_KEY }, Values.of( "a" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void samplingOverPartitions() throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SamplingOverPartitions()
		 {
			  PartitionedIndexReader indexReader = CreatePartitionedReaderFromReaders();
			  when( _indexReader1.createSampler() ).thenReturn(new SimpleSampler(this, 1));
			  when( _indexReader2.createSampler() ).thenReturn(new SimpleSampler(this, 2));
			  when( _indexReader3.createSampler() ).thenReturn(new SimpleSampler(this, 3));

			  IndexSampler sampler = indexReader.CreateSampler();
			  assertEquals( new IndexSample( 6, 6, 6 ), sampler.SampleIndex() );
		 }

		 private void VerifyResult( LongSet results )
		 {
			  assertEquals( 3, results.size() );
			  assertTrue( results.contains( 1 ) );
			  assertTrue( results.contains( 2 ) );
			  assertTrue( results.contains( 3 ) );
		 }

		 private PartitionedIndexReader CreatePartitionedReaderFromReaders()
		 {
			  return new PartitionedIndexReader( _schemaIndexDescriptor, PartitionReaders );
		 }

		 private IList<SimpleIndexReader> PartitionReaders
		 {
			 get
			 {
				  return Arrays.asList( _indexReader1, _indexReader2, _indexReader3 );
			 }
		 }

		 private PartitionedIndexReader CreatePartitionedReader()
		 {
			  return new PartitionedIndexReader( PartitionSearchers, _schemaIndexDescriptor, _samplingConfig, _taskCoordinator );
		 }

		 private IList<PartitionSearcher> PartitionSearchers
		 {
			 get
			 {
				  return Arrays.asList( _partitionSearcher1, _partitionSearcher2, _partitionSearcher3 );
			 }
		 }

		 private class SimpleSampler : IndexSampler
		 {
			 private readonly PartitionedIndexReaderTest _outerInstance;

			  internal long SampleValue;

			  internal SimpleSampler( PartitionedIndexReaderTest outerInstance, long sampleValue )
			  {
				  this._outerInstance = outerInstance;
					this.SampleValue = sampleValue;
			  }

			  public override IndexSample SampleIndex()
			  {
					return new IndexSample( SampleValue, SampleValue, SampleValue );
			  }
		 }
	}

}