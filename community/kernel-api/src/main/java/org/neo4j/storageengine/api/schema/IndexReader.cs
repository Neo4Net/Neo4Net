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
	using PrimitiveLongResourceCollections = Org.Neo4j.Collection.PrimitiveLongResourceCollections;
	using PrimitiveLongResourceIterator = Org.Neo4j.Collection.PrimitiveLongResourceIterator;
	using Resource = Org.Neo4j.Graphdb.Resource;
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using IndexNotApplicableKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotApplicableKernelException;
	using Value = Org.Neo4j.Values.Storable.Value;

	/// <summary>
	/// Reader for an index. Must honor repeatable reads, which means that if a lookup is executed multiple times the
	/// same result set must be returned.
	/// </summary>
	public interface IndexReader : Resource
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
		 /// <returns> the matching entity IDs. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.collection.PrimitiveLongResourceIterator query(org.neo4j.internal.kernel.api.IndexQuery... predicates) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException;
		 PrimitiveLongResourceIterator Query( params IndexQuery[] predicates );

		 /// <summary>
		 /// Queries the index for the given <seealso cref="IndexQuery"/> predicates. </summary>
		 ///  <param name="client"> the client which will control the progression though query results. </param>
		 /// <param name="needsValues"> if the index should fetch property values together with node ids for index queries </param>
		 /// <param name="query"> the query so serve. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void query(IndexProgressor_NodeValueClient client, org.neo4j.internal.kernel.api.IndexOrder indexOrder, boolean needsValues, org.neo4j.internal.kernel.api.IndexQuery... query) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException;
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
//ORIGINAL LINE: public org.neo4j.collection.PrimitiveLongResourceIterator query(org.neo4j.internal.kernel.api.IndexQuery... predicates) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		  public override PrimitiveLongResourceIterator Query( params IndexQuery[] predicates )
		  {
				return null;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void query(IndexProgressor_NodeValueClient client, org.neo4j.internal.kernel.api.IndexOrder indexOrder, boolean needsValues, org.neo4j.internal.kernel.api.IndexQuery... query) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
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