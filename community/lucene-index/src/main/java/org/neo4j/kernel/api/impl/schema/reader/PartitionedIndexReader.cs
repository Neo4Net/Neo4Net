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
namespace Org.Neo4j.Kernel.Api.Impl.Schema.reader
{

	using PrimitiveLongResourceCollections = Org.Neo4j.Collection.PrimitiveLongResourceCollections;
	using PrimitiveLongResourceIterator = Org.Neo4j.Collection.PrimitiveLongResourceIterator;
	using TaskCoordinator = Org.Neo4j.Helpers.TaskCoordinator;
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using IndexNotApplicableKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotApplicableKernelException;
	using IOUtils = Org.Neo4j.Io.IOUtils;
	using PartitionSearcher = Org.Neo4j.Kernel.Api.Impl.Index.partition.PartitionSearcher;
	using AggregatingIndexSampler = Org.Neo4j.Kernel.Api.Impl.Index.sampler.AggregatingIndexSampler;
	using IndexSamplingConfig = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using BridgingIndexProgressor = Org.Neo4j.Kernel.Impl.Api.schema.BridgingIndexProgressor;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using AbstractIndexReader = Org.Neo4j.Storageengine.Api.schema.AbstractIndexReader;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexProgressor = Org.Neo4j.Storageengine.Api.schema.IndexProgressor;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using IndexSampler = Org.Neo4j.Storageengine.Api.schema.IndexSampler;
	using Value = Org.Neo4j.Values.Storable.Value;

	/// <summary>
	/// Index reader that is able to read/sample multiple partitions of a partitioned Lucene index.
	/// Internally uses multiple <seealso cref="SimpleIndexReader"/>s for individual partitions.
	/// </summary>
	/// <seealso cref= SimpleIndexReader </seealso>
	public class PartitionedIndexReader : AbstractIndexReader
	{
		 private readonly IList<SimpleIndexReader> _indexReaders;

		 public PartitionedIndexReader( IList<PartitionSearcher> partitionSearchers, IndexDescriptor descriptor, IndexSamplingConfig samplingConfig, TaskCoordinator taskCoordinator ) : this( descriptor, partitionSearchers.Select( partitionSearcher -> new SimpleIndexReader( partitionSearcher, descriptor, samplingConfig, taskCoordinator ) ).ToList() )
		 {
		 }

		 internal PartitionedIndexReader( IndexDescriptor descriptor, IList<SimpleIndexReader> readers ) : base( descriptor )
		 {
			  this._indexReaders = readers;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.PrimitiveLongResourceIterator query(org.neo4j.internal.kernel.api.IndexQuery... predicates) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 public override PrimitiveLongResourceIterator Query( params IndexQuery[] predicates )
		 {
			  try
			  {
					return PartitionedOperation( reader => InnerQuery( reader, predicates ) );
			  }
			  catch ( InnerException e )
			  {
					throw e.InnerException;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void query(org.neo4j.storageengine.api.schema.IndexProgressor_NodeValueClient client, org.neo4j.internal.kernel.api.IndexOrder indexOrder, boolean needsValues, org.neo4j.internal.kernel.api.IndexQuery... query) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 public override void Query( Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, IndexOrder indexOrder, bool needsValues, params IndexQuery[] query )
		 {
			  try
			  {
					BridgingIndexProgressor bridgingIndexProgressor = new BridgingIndexProgressor( client, Descriptor.schema().PropertyIds );
					_indexReaders.ForEach(reader =>
					{
					 try
					 {
						  reader.query( bridgingIndexProgressor, indexOrder, needsValues, query );
					 }
					 catch ( IndexNotApplicableKernelException e )
					 {
						  throw new InnerException( e );
					 }
					});
					client.Initialize( Descriptor, bridgingIndexProgressor, query, indexOrder, needsValues );
			  }
			  catch ( InnerException e )
			  {
					throw e.InnerException;
			  }
		 }

		 public override bool HasFullValuePrecision( params IndexQuery[] predicates )
		 {
			  return false;
		 }

		 public override void DistinctValues( Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, NodePropertyAccessor propertyAccessor, bool needsValues )
		 {
			  BridgingIndexProgressor bridgingIndexProgressor = new BridgingIndexProgressor( client, Descriptor.schema().PropertyIds );
			  _indexReaders.ForEach( reader => reader.distinctValues( bridgingIndexProgressor, propertyAccessor, needsValues ) );
			  client.Initialize( Descriptor, bridgingIndexProgressor, new IndexQuery[0], IndexOrder.NONE, needsValues );
		 }

		 private PrimitiveLongResourceIterator InnerQuery( IndexReader reader, IndexQuery[] predicates )
		 {
			  try
			  {
					return reader.Query( predicates );
			  }
			  catch ( IndexNotApplicableKernelException e )
			  {
					throw new InnerException( e );
			  }
		 }

		 private sealed class InnerException : Exception
		 {
			  internal InnerException( IndexNotApplicableKernelException e ) : base( e )
			  {
			  }

			  public override IndexNotApplicableKernelException Cause
			  {
				  get
				  {
					  lock ( this )
					  {
							return ( IndexNotApplicableKernelException ) base.InnerException;
					  }
				  }
			  }
		 }

		 public override long CountIndexedNodes( long nodeId, int[] propertyKeyIds, params Value[] propertyValues )
		 {
			  return _indexReaders.Select( reader => reader.countIndexedNodes( nodeId, propertyKeyIds, propertyValues ) ).Sum();
		 }

		 public override IndexSampler CreateSampler()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<IndexSampler> indexSamplers = _indexReaders.Select( SimpleIndexReader::createSampler ).ToList();
			  return new AggregatingIndexSampler( indexSamplers );
		 }

		 public override void Close()
		 {
			  try
			  {
					IOUtils.closeAll( _indexReaders );
			  }
			  catch ( IOException e )
			  {
					throw new IndexReaderCloseException( e );
			  }
		 }

		 private PrimitiveLongResourceIterator PartitionedOperation( System.Func<SimpleIndexReader, PrimitiveLongResourceIterator> readerFunction )
		 {
			  return PrimitiveLongResourceCollections.concat( _indexReaders.Select( readerFunction ).ToList() );
		 }
	}

}