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
namespace Org.Neo4j.Kernel.Impl.Index.Schema.tracking
{

	using PrimitiveLongResourceIterator = Org.Neo4j.Collection.PrimitiveLongResourceIterator;
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using IndexNotApplicableKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotApplicableKernelException;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using IndexProgressor = Org.Neo4j.Storageengine.Api.schema.IndexProgressor;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using IndexSampler = Org.Neo4j.Storageengine.Api.schema.IndexSampler;
	using Value = Org.Neo4j.Values.Storable.Value;

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
//ORIGINAL LINE: public org.neo4j.collection.PrimitiveLongResourceIterator query(org.neo4j.internal.kernel.api.IndexQuery... predicates) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 public override PrimitiveLongResourceIterator Query( params IndexQuery[] predicates )
		 {
			  return @delegate.Query( predicates );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void query(org.neo4j.storageengine.api.schema.IndexProgressor_NodeValueClient client, org.neo4j.internal.kernel.api.IndexOrder indexOrder, boolean needsValues, org.neo4j.internal.kernel.api.IndexQuery... query) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 public override void Query( Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, IndexOrder indexOrder, bool needsValues, params IndexQuery[] query )
		 {
			  @delegate.Query( client, indexOrder, needsValues, query );
		 }

		 public override bool HasFullValuePrecision( params IndexQuery[] predicates )
		 {
			  return @delegate.HasFullValuePrecision( predicates );
		 }

		 public override void DistinctValues( Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, NodePropertyAccessor propertyAccessor, bool needsValues )
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