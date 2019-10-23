﻿/*
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
namespace Neo4Net.Kernel.Impl.Index.Schema.tracking
{

	using PrimitiveLongResourceIterator = Neo4Net.Collections.PrimitiveLongResourceIterator;
	using IndexOrder = Neo4Net.Kernel.Api.Internal.IndexOrder;
	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using IndexNotApplicableKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotApplicableKernelException;
	using NodePropertyAccessor = Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor;
	using IndexProgressor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexProgressor;
	using IndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader;
	using IndexSampler = Neo4Net.Kernel.Api.StorageEngine.schema.IndexSampler;
	using Value = Neo4Net.Values.Storable.Value;

	public class TrackingIndexReader : IndexReader
	{
		 private readonly IndexReader @delegate;
		 private readonly AtomicLong _closeReadersCounter;

		 internal TrackingIndexReader( IndexReader @delegate, AtomicLong closeReadersCounter )
		 {
			  this.@delegate = @delegate;
			  this._closeReadersCounter = closeReadersCounter;
		 }

		 public override long CountIndexedNodes( long nodeId, int[] propertyKeyIds, params Value[] propertyValues )
		 {
			  return @delegate.CountIndexedNodes( nodeId, propertyKeyIds, propertyValues );
		 }

		 public override IndexSampler CreateSampler()
		 {
			  return @delegate.CreateSampler();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.collection.PrimitiveLongResourceIterator query(org.Neo4Net.Kernel.Api.Internal.IndexQuery... predicates) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotApplicableKernelException
		 public override PrimitiveLongResourceIterator Query( params IndexQuery[] predicates )
		 {
			  return @delegate.Query( predicates );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void query(org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexProgressor_NodeValueClient client, org.Neo4Net.Kernel.Api.Internal.IndexOrder indexOrder, boolean needsValues, org.Neo4Net.Kernel.Api.Internal.IndexQuery... query) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotApplicableKernelException
		 public override void Query( Neo4Net.Kernel.Api.StorageEngine.schema.IndexProgressor_NodeValueClient client, IndexOrder indexOrder, bool needsValues, params IndexQuery[] query )
		 {
			  @delegate.Query( client, indexOrder, needsValues, query );
		 }

		 public override bool HasFullValuePrecision( params IndexQuery[] predicates )
		 {
			  return @delegate.HasFullValuePrecision( predicates );
		 }

		 public override void DistinctValues( Neo4Net.Kernel.Api.StorageEngine.schema.IndexProgressor_NodeValueClient client, NodePropertyAccessor propertyAccessor, bool needsValues )
		 {
			  @delegate.DistinctValues( client, propertyAccessor, needsValues );
		 }

		 public override void Close()
		 {
			  @delegate.Close();
			  _closeReadersCounter.incrementAndGet();
		 }
	}

}