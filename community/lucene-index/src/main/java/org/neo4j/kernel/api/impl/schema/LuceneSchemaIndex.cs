using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Api.Impl.Schema
{

	using TaskCoordinator = Org.Neo4j.Helpers.TaskCoordinator;
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Org.Neo4j.Kernel.Api.Impl.Index;
	using AbstractIndexPartition = Org.Neo4j.Kernel.Api.Impl.Index.partition.AbstractIndexPartition;
	using IndexPartitionFactory = Org.Neo4j.Kernel.Api.Impl.Index.partition.IndexPartitionFactory;
	using PartitionSearcher = Org.Neo4j.Kernel.Api.Impl.Index.partition.PartitionSearcher;
	using PartitionedIndexStorage = Org.Neo4j.Kernel.Api.Impl.Index.storage.PartitionedIndexStorage;
	using PartitionedIndexReader = Org.Neo4j.Kernel.Api.Impl.Schema.reader.PartitionedIndexReader;
	using SimpleIndexReader = Org.Neo4j.Kernel.Api.Impl.Schema.reader.SimpleIndexReader;
	using PartitionedUniquenessVerifier = Org.Neo4j.Kernel.Api.Impl.Schema.verification.PartitionedUniquenessVerifier;
	using SimpleUniquenessVerifier = Org.Neo4j.Kernel.Api.Impl.Schema.verification.SimpleUniquenessVerifier;
	using UniquenessVerifier = Org.Neo4j.Kernel.Api.Impl.Schema.verification.UniquenessVerifier;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using IndexSamplingConfig = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using Value = Org.Neo4j.Values.Storable.Value;

	/// <summary>
	/// Implementation of Lucene schema index that support multiple partitions.
	/// </summary>
	internal class LuceneSchemaIndex : AbstractLuceneIndex<IndexReader>
	{

		 private readonly IndexSamplingConfig _samplingConfig;

		 private readonly TaskCoordinator _taskCoordinator = new TaskCoordinator( 10, TimeUnit.MILLISECONDS );

		 internal LuceneSchemaIndex( PartitionedIndexStorage indexStorage, IndexDescriptor descriptor, IndexSamplingConfig samplingConfig, IndexPartitionFactory partitionFactory ) : base( indexStorage, partitionFactory, descriptor )
		 {
			  this._samplingConfig = samplingConfig;
		 }

		 /// <summary>
		 /// Verifies uniqueness of property values present in this index.
		 /// </summary>
		 /// <param name="accessor"> the accessor to retrieve actual property values from the store. </param>
		 /// <param name="propertyKeyIds"> the ids of the properties to verify. </param>
		 /// <exception cref="IndexEntryConflictException"> if there are duplicates. </exception>
		 /// <exception cref="IOException"> </exception>
		 /// <seealso cref= UniquenessVerifier#verify(NodePropertyAccessor, int[]) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyUniqueness(org.neo4j.storageengine.api.NodePropertyAccessor accessor, int[] propertyKeyIds) throws java.io.IOException, org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public virtual void VerifyUniqueness( NodePropertyAccessor accessor, int[] propertyKeyIds )
		 {
			  Flush( true );
			  using ( UniquenessVerifier verifier = CreateUniquenessVerifier() )
			  {
					verifier.Verify( accessor, propertyKeyIds );
			  }
		 }

		 /// <summary>
		 /// Verifies uniqueness of updated property values.
		 /// </summary>
		 /// <param name="accessor"> the accessor to retrieve actual property values from the store. </param>
		 /// <param name="propertyKeyIds"> the ids of the properties to verify. </param>
		 /// <param name="updatedValueTuples"> the values to check uniqueness for. </param>
		 /// <exception cref="IndexEntryConflictException"> if there are duplicates. </exception>
		 /// <exception cref="IOException"> </exception>
		 /// <seealso cref= UniquenessVerifier#verify(NodePropertyAccessor, int[], List) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyUniqueness(org.neo4j.storageengine.api.NodePropertyAccessor accessor, int[] propertyKeyIds, java.util.List<org.neo4j.values.storable.Value[]> updatedValueTuples) throws java.io.IOException, org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public virtual void VerifyUniqueness( NodePropertyAccessor accessor, int[] propertyKeyIds, IList<Value[]> updatedValueTuples )
		 {
			  using ( UniquenessVerifier verifier = CreateUniquenessVerifier() )
			  {
					verifier.Verify( accessor, propertyKeyIds, updatedValueTuples );
			  }
		 }

		 public override void Drop()
		 {
			  _taskCoordinator.cancel();
			  try
			  {
					_taskCoordinator.awaitCompletion();
			  }
			  catch ( InterruptedException e )
			  {
					throw new Exception( "Interrupted while waiting for concurrent tasks to complete.", e );
			  }
			  base.Drop();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.api.impl.schema.verification.UniquenessVerifier createUniquenessVerifier() throws java.io.IOException
		 private UniquenessVerifier CreateUniquenessVerifier()
		 {
			  EnsureOpen();
			  MaybeRefreshBlocking();
			  IList<AbstractIndexPartition> partitions = Partitions;
			  return HasSinglePartition( partitions ) ? CreateSimpleUniquenessVerifier( partitions ) : CreatePartitionedUniquenessVerifier( partitions );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.api.impl.schema.verification.UniquenessVerifier createSimpleUniquenessVerifier(java.util.List<org.neo4j.kernel.api.impl.index.partition.AbstractIndexPartition> partitions) throws java.io.IOException
		 private UniquenessVerifier CreateSimpleUniquenessVerifier( IList<AbstractIndexPartition> partitions )
		 {
			  AbstractIndexPartition singlePartition = GetFirstPartition( partitions );
			  PartitionSearcher partitionSearcher = singlePartition.AcquireSearcher();
			  return new SimpleUniquenessVerifier( partitionSearcher );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.api.impl.schema.verification.UniquenessVerifier createPartitionedUniquenessVerifier(java.util.List<org.neo4j.kernel.api.impl.index.partition.AbstractIndexPartition> partitions) throws java.io.IOException
		 private UniquenessVerifier CreatePartitionedUniquenessVerifier( IList<AbstractIndexPartition> partitions )
		 {
			  IList<PartitionSearcher> searchers = AcquireSearchers( partitions );
			  return new PartitionedUniquenessVerifier( searchers );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.kernel.api.impl.schema.reader.SimpleIndexReader createSimpleReader(java.util.List<org.neo4j.kernel.api.impl.index.partition.AbstractIndexPartition> partitions) throws java.io.IOException
		 protected internal override SimpleIndexReader CreateSimpleReader( IList<AbstractIndexPartition> partitions )
		 {
			  AbstractIndexPartition singlePartition = GetFirstPartition( partitions );
			  return new SimpleIndexReader( singlePartition.AcquireSearcher(), DescriptorConflict, _samplingConfig, _taskCoordinator );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.kernel.api.impl.schema.reader.PartitionedIndexReader createPartitionedReader(java.util.List<org.neo4j.kernel.api.impl.index.partition.AbstractIndexPartition> partitions) throws java.io.IOException
		 protected internal override PartitionedIndexReader CreatePartitionedReader( IList<AbstractIndexPartition> partitions )
		 {
			  IList<PartitionSearcher> searchers = AcquireSearchers( partitions );
			  return new PartitionedIndexReader( searchers, DescriptorConflict, _samplingConfig, _taskCoordinator );
		 }

	}

}