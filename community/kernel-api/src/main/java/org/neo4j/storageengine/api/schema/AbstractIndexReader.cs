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
namespace Org.Neo4j.Storageengine.Api.schema
{
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using IndexNotApplicableKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotApplicableKernelException;

	public abstract class AbstractIndexReader : IndexReader
	{
		public abstract void Close();
		public abstract void DistinctValues( IndexProgressor_NodeValueClient client, Org.Neo4j.Storageengine.Api.NodePropertyAccessor propertyAccessor, bool needsValues );
		public abstract bool HasFullValuePrecision( params IndexQuery[] predicates );
		public abstract Org.Neo4j.Collection.PrimitiveLongResourceIterator Query( params IndexQuery[] predicates );
		public abstract IndexSampler CreateSampler();
		public abstract long CountIndexedNodes( long nodeId, int[] propertyKeyIds, params Org.Neo4j.Values.Storable.Value[] propertyValues );
		 protected internal readonly IndexDescriptor Descriptor;

		 protected internal AbstractIndexReader( IndexDescriptor descriptor )
		 {
			  this.Descriptor = descriptor;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void query(IndexProgressor_NodeValueClient client, org.neo4j.internal.kernel.api.IndexOrder indexOrder, boolean needsValues, org.neo4j.internal.kernel.api.IndexQuery... query) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 public override void Query( IndexProgressor_NodeValueClient client, IndexOrder indexOrder, bool needsValues, params IndexQuery[] query )
		 {
			  if ( indexOrder != IndexOrder.NONE )
			  {
					throw new System.NotSupportedException( string.Format( "This reader only have support for index order {0}. Provided index order was {1}.", IndexOrder.NONE, indexOrder ) );
			  }
			  client.Initialize( Descriptor, new NodeValueIndexProgressor( query( query ), client ), query, indexOrder, needsValues );
		 }

	}

}