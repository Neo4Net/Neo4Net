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
	using PrimitiveLongResourceCollections = Neo4Net.Collections.PrimitiveLongResourceCollections;
	using PrimitiveLongResourceIterator = Neo4Net.Collections.PrimitiveLongResourceIterator;
	using Resource = Neo4Net.GraphDb.Resource;
	using IndexOrder = Neo4Net.Kernel.Api.Internal.IndexOrder;
	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using IndexNotApplicableKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotApplicableKernelException;
	using Value = Neo4Net.Values.Storable.Value;

	/// <summary>
	/// Reader for an index. Must honor repeatable reads, which means that if a lookup is executed multiple times the
	/// same result set must be returned.
	/// </summary>
	public interface IIndexReader : Resource
	{
		 /// <param name="nodeId"> node id to match. </param>
		 /// <param name="propertyKeyIds"> the property key ids that correspond to each of the property values. </param>
		 /// <param name="propertyValues"> property values to match. </param>
		 /// <returns> number of index entries for the given {@code nodeId} and {@code propertyValues}. </returns>
		 long CountIndexedNodes( long nodeId, int[] propertyKeyIds, params Value[] propertyValues );

		 IndexSampler CreateSampler();

		 /// <summary>
		 /// Queries the index for the given <seealso cref="IndexQuery"/> predicates.
		 /// </summary>
		 /// <param name="predicates"> the predicates to query for. </param>
		 /// <returns> the matching IEntity IDs. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Neo4Net.collection.PrimitiveLongResourceIterator query(Neo4Net.Kernel.Api.Internal.IndexQuery... predicates) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotApplicableKernelException;
		 PrimitiveLongResourceIterator Query( params IndexQuery[] predicates );

		 /// <summary>
		 /// Queries the index for the given <seealso cref="IndexQuery"/> predicates. </summary>
		 ///  <param name="client"> the client which will control the progression though query results. </param>
		 /// <param name="needsValues"> if the index should fetch property values together with node ids for index queries </param>
		 /// <param name="query"> the query so serve. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void query(IndexProgressor_NodeValueClient client, Neo4Net.Kernel.Api.Internal.IndexOrder indexOrder, boolean needsValues, Neo4Net.Kernel.Api.Internal.IndexQuery... query) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotApplicableKernelException;
		 void Query( IndexProgressor_NodeValueClient client, IndexOrder indexOrder, bool needsValues, params IndexQuery[] query );

		 /// <param name="predicates"> query to determine whether or not index has full value precision for. </param>
		 /// <returns> whether or not this reader will only return 100% matching results from <seealso cref="query(IndexQuery...)"/>.
		 /// If {@code false} is returned this means that the caller of <seealso cref="query(IndexQuery...)"/> will have to
		 /// do additional filtering, double-checking of actual property values, externally. </returns>
		 bool HasFullValuePrecision( params IndexQuery[] predicates );

		 /// <summary>
		 /// Initializes {@code client} to be able to progress through all distinct values in this index. <seealso cref="IndexProgressor.NodeValueClient"/>
		 /// is used because it has a perfect method signature, even if the {@code reference} argument will instead be used
		 /// as number of index entries for the specific indexed value.
		 /// 
		 /// {@code needsValues} decides whether or not values will be materialized and given to the client.
		 /// The use-case for setting this to {@code false} is to have a more efficient counting of distinct values in an index,
		 /// regardless of the actual values. </summary>
		 /// <param name="client"> <seealso cref="IndexProgressor.NodeValueClient"/> to get initialized with this progression. </param>
		 /// <param name="propertyAccessor"> used for distinguishing between lossy indexed values. </param>
		 /// <param name="needsValues"> whether or not values should be loaded. </param>
		 void DistinctValues( IndexProgressor_NodeValueClient client, NodePropertyAccessor propertyAccessor, bool needsValues );

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 IndexReader EMPTY = new IndexReader()
	//	 {
	//		  // Used for checking index correctness
	//		  @@Override public long countIndexedNodes(long nodeId, int[] propertyKeyIds, Value... propertyValues)
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public IndexSampler createSampler()
	//		  {
	//				return IndexSampler.EMPTY;
	//		  }
	//
	//		  @@Override public PrimitiveLongResourceIterator query(IndexQuery[] predicates)
	//		  {
	//				return PrimitiveLongResourceCollections.emptyIterator();
	//		  }
	//
	//		  @@Override public void query(IndexProgressor.NodeValueClient client, IndexOrder indexOrder, boolean needsValues, IndexQuery... query)
	//		  {
	//				// do nothing
	//		  }
	//
	//		  @@Override public void close()
	//		  {
	//		  }
	//
	//		  @@Override public boolean hasFullValuePrecision(IndexQuery... predicates)
	//		  {
	//				return true;
	//		  }
	//
	//		  @@Override public void distinctValues(IndexProgressor.NodeValueClient client, NodePropertyAccessor propertyAccessor, boolean needsValues)
	//		  {
	//				// do nothing
	//		  }
	//	 };
	}

	 public class IndexReader_Adaptor : IndexReader
	 {
		  public override long CountIndexedNodes( long nodeId, int[] propertyKeyIds, params Value[] propertyValues )
		  {
				return 0;
		  }

		  public override IndexSampler CreateSampler()
		  {
				return null;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.collection.PrimitiveLongResourceIterator query(Neo4Net.Kernel.Api.Internal.IndexQuery... predicates) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotApplicableKernelException
		  public override PrimitiveLongResourceIterator Query( params IndexQuery[] predicates )
		  {
				return null;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void query(IndexProgressor_NodeValueClient client, Neo4Net.Kernel.Api.Internal.IndexOrder indexOrder, boolean needsValues, Neo4Net.Kernel.Api.Internal.IndexQuery... query) throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotApplicableKernelException
		  public override void Query( IndexProgressor_NodeValueClient client, IndexOrder indexOrder, bool needsValues, params IndexQuery[] query )
		  {
		  }

		  public override bool HasFullValuePrecision( params IndexQuery[] predicates )
		  {
				return false;
		  }

		  public override void DistinctValues( IndexProgressor_NodeValueClient client, NodePropertyAccessor propertyAccessor, bool needsValues )
		  {
		  }

		  public override void Close()
		  {
		  }
	 }

}