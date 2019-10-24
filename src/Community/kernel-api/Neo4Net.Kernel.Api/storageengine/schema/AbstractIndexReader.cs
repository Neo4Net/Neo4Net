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
namespace Neo4Net.Kernel.Api.StorageEngine.schema
{
	using IndexOrder = Neo4Net.Kernel.Api.Internal.IndexOrder;
	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using IndexNotApplicableKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotApplicableKernelException;

	public abstract class AbstractIndexReader : IndexReader
	{
		public abstract void Close();
		public abstract void DistinctValues( IndexProgressor_NodeValueClient client, Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor propertyAccessor, bool needsValues );
		public abstract bool HasFullValuePrecision( params IndexQuery[] predicates );
		public abstract Neo4Net.Collections.PrimitiveLongResourceIterator Query( params IndexQuery[] predicates );
		public abstract IndexSampler CreateSampler();
		public abstract long CountIndexedNodes( long nodeId, int[] propertyKeyIds, params Neo4Net.Values.Storable.Value[] propertyValues );
		 protected internal readonly IndexDescriptor Descriptor;

		 protected internal AbstractIndexReader( IndexDescriptor descriptor )
		 {
			  this.Descriptor = descriptor;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void query(IndexProgressor_NodeValueClient client, org.Neo4Net.Kernel.Api.Internal.IndexOrder indexOrder, boolean needsValues, org.Neo4Net.Kernel.Api.Internal.IndexQuery... query) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotApplicableKernelException
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