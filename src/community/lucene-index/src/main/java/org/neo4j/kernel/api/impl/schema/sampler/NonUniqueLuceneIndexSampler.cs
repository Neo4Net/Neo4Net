using System;
using System.Collections.Generic;

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
	using Fields = Org.Apache.Lucene.Index.Fields;
	using IndexReader = Org.Apache.Lucene.Index.IndexReader;
	using LeafReaderContext = Org.Apache.Lucene.Index.LeafReaderContext;
	using Terms = Org.Apache.Lucene.Index.Terms;
	using TermsEnum = Org.Apache.Lucene.Index.TermsEnum;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using BytesRef = org.apache.lucene.util.BytesRef;


	using TaskControl = Neo4Net.Helpers.TaskControl;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using DefaultNonUniqueIndexSampler = Neo4Net.Kernel.Impl.Api.index.sampling.DefaultNonUniqueIndexSampler;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using NonUniqueIndexSampler = Neo4Net.Kernel.Impl.Api.index.sampling.NonUniqueIndexSampler;
	using IndexSample = Neo4Net.Storageengine.Api.schema.IndexSample;

	/// <summary>
	/// Sampler for non-unique Lucene schema index.
	/// Internally uses terms and their document frequencies for sampling.
	/// </summary>
	public class NonUniqueLuceneIndexSampler : LuceneIndexSampler
	{
		 private readonly IndexSearcher _indexSearcher;
		 private readonly IndexSamplingConfig _indexSamplingConfig;

		 public NonUniqueLuceneIndexSampler( IndexSearcher indexSearcher, TaskControl taskControl, IndexSamplingConfig indexSamplingConfig ) : base( taskControl )
		 {
			  this._indexSearcher = indexSearcher;
			  this._indexSamplingConfig = indexSamplingConfig;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.schema.IndexSample sampleIndex() throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 public override IndexSample SampleIndex()
		 {
			  NonUniqueIndexSampler sampler = new DefaultNonUniqueIndexSampler( _indexSamplingConfig.sampleSizeLimit() );
			  IndexReader indexReader = _indexSearcher.IndexReader;
			  foreach ( LeafReaderContext readerContext in indexReader.leaves() )
			  {
					try
					{
						 ISet<string> fieldNames = GetFieldNamesToSample( readerContext );
						 foreach ( string fieldName in fieldNames )
						 {
							  Terms terms = readerContext.reader().terms(fieldName);
							  if ( terms != null )
							  {
									TermsEnum termsEnum = LuceneDocumentStructure.originalTerms( terms, fieldName );
									BytesRef termsRef;
									while ( ( termsRef = termsEnum.next() ) != null )
									{
										 sampler.Include( termsRef.utf8ToString(), termsEnum.docFreq() );
										 CheckCancellation();
									}
							  }
						 }
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}
			  }

			  return sampler.Result( indexReader.numDocs() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.util.Set<String> getFieldNamesToSample(org.apache.lucene.index.LeafReaderContext readerContext) throws java.io.IOException
		 private static ISet<string> GetFieldNamesToSample( LeafReaderContext readerContext )
		 {
			  Fields fields = readerContext.reader().fields();
			  ISet<string> fieldNames = new HashSet<string>();
			  foreach ( string field in fields )
			  {
					if ( !LuceneDocumentStructure.NODE_ID_KEY.Equals( field ) )
					{
						 fieldNames.Add( field );
					}
			  }
			  return fieldNames;
		 }
	}

}