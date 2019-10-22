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
namespace Neo4Net.Kernel.Api.Impl.Schema.sampler
{
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;

	using TaskControl = Neo4Net.Helpers.TaskControl;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using UniqueIndexSampler = Neo4Net.Kernel.Impl.Api.index.sampling.UniqueIndexSampler;
	using IndexSample = Neo4Net.Storageengine.Api.schema.IndexSample;

	/// <summary>
	/// Sampler for unique Lucene schema index.
	/// Internally uses number of documents in the index for sampling.
	/// </summary>
	public class UniqueLuceneIndexSampler : LuceneIndexSampler
	{
		 private readonly IndexSearcher _indexSearcher;

		 public UniqueLuceneIndexSampler( IndexSearcher indexSearcher, TaskControl taskControl ) : base( taskControl )
		 {
			  this._indexSearcher = indexSearcher;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.storageengine.api.schema.IndexSample sampleIndex() throws org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override IndexSample SampleIndex()
		 {
			  UniqueIndexSampler sampler = new UniqueIndexSampler();
			  sampler.Increment( _indexSearcher.IndexReader.numDocs() );
			  CheckCancellation();
			  return sampler.Result();
		 }
	}

}