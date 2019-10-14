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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{
	using ParseException = org.apache.lucene.queryparser.classic.ParseException;

	using PrimitiveLongResourceIterator = Neo4Net.Collections.PrimitiveLongResourceIterator;
	using IndexOrder = Neo4Net.Internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.Internal.Kernel.Api.IndexQuery;
	using IndexNotApplicableKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotApplicableKernelException;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using IndexProgressor = Neo4Net.Storageengine.Api.schema.IndexProgressor;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using IndexSampler = Neo4Net.Storageengine.Api.schema.IndexSampler;

	public abstract class FulltextIndexReader : IndexReader
	{
		public abstract void Close();
		public abstract long CountIndexedNodes( long nodeId, int[] propertyKeyIds, params Neo4Net.Values.Storable.Value[] propertyValues );
		 /// <summary>
		 /// Queires the fulltext index with the given lucene-syntax query
		 /// </summary>
		 /// <param name="query"> the lucene query </param>
		 /// <returns> A <seealso cref="ScoreEntityIterator"/> over the results </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract ScoreEntityIterator query(String query) throws org.apache.lucene.queryparser.classic.ParseException;
		 public abstract ScoreEntityIterator Query( string query );

		 public override IndexSampler CreateSampler()
		 {
			  return Neo4Net.Storageengine.Api.schema.IndexSampler_Fields.Empty;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.PrimitiveLongResourceIterator query(org.neo4j.internal.kernel.api.IndexQuery... predicates) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 public override PrimitiveLongResourceIterator Query( params IndexQuery[] predicates )
		 {
			  throw new IndexNotApplicableKernelException( "Fulltext indexes does not support IndexQuery queries" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void query(org.neo4j.storageengine.api.schema.IndexProgressor_NodeValueClient client, org.neo4j.internal.kernel.api.IndexOrder indexOrder, boolean needsValues, org.neo4j.internal.kernel.api.IndexQuery... query) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 public override void Query( Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, IndexOrder indexOrder, bool needsValues, params IndexQuery[] query )
		 {
			  throw new IndexNotApplicableKernelException( "Fulltext indexes does not support IndexQuery queries" );
		 }

		 public override void DistinctValues( Neo4Net.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, NodePropertyAccessor propertyAccessor, bool needsValues )
		 {
			  throw new System.NotSupportedException( "Fulltext indexes does not support distinctValues queries" );
		 }

		 public override bool HasFullValuePrecision( params IndexQuery[] predicates )
		 {
			  return false;
		 }
	}

}