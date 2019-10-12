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
namespace Org.Neo4j.Kernel.Api.Impl.Fulltext
{
	using ParseException = org.apache.lucene.queryparser.classic.ParseException;

	using PrimitiveLongResourceIterator = Org.Neo4j.Collection.PrimitiveLongResourceIterator;
	using IndexOrder = Org.Neo4j.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using IndexNotApplicableKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotApplicableKernelException;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using IndexProgressor = Org.Neo4j.Storageengine.Api.schema.IndexProgressor;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using IndexSampler = Org.Neo4j.Storageengine.Api.schema.IndexSampler;

	public abstract class FulltextIndexReader : IndexReader
	{
		public abstract void Close();
		public abstract long CountIndexedNodes( long nodeId, int[] propertyKeyIds, params Org.Neo4j.Values.Storable.Value[] propertyValues );
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
			  return Org.Neo4j.Storageengine.Api.schema.IndexSampler_Fields.Empty;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.PrimitiveLongResourceIterator query(org.neo4j.internal.kernel.api.IndexQuery... predicates) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 public override PrimitiveLongResourceIterator Query( params IndexQuery[] predicates )
		 {
			  throw new IndexNotApplicableKernelException( "Fulltext indexes does not support IndexQuery queries" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void query(org.neo4j.storageengine.api.schema.IndexProgressor_NodeValueClient client, org.neo4j.internal.kernel.api.IndexOrder indexOrder, boolean needsValues, org.neo4j.internal.kernel.api.IndexQuery... query) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 public override void Query( Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, IndexOrder indexOrder, bool needsValues, params IndexQuery[] query )
		 {
			  throw new IndexNotApplicableKernelException( "Fulltext indexes does not support IndexQuery queries" );
		 }

		 public override void DistinctValues( Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, NodePropertyAccessor propertyAccessor, bool needsValues )
		 {
			  throw new System.NotSupportedException( "Fulltext indexes does not support distinctValues queries" );
		 }

		 public override bool HasFullValuePrecision( params IndexQuery[] predicates )
		 {
			  return false;
		 }
	}

}