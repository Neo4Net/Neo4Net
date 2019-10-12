using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Api.Impl.Schema.verification
{
	using Fields = Org.Apache.Lucene.Index.Fields;
	using LeafReaderContext = Org.Apache.Lucene.Index.LeafReaderContext;
	using Term = Org.Apache.Lucene.Index.Term;
	using TermsEnum = Org.Apache.Lucene.Index.TermsEnum;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using Query = org.apache.lucene.search.Query;
	using TermQuery = org.apache.lucene.search.TermQuery;
	using BytesRef = org.apache.lucene.util.BytesRef;


	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using PartitionSearcher = Org.Neo4j.Kernel.Api.Impl.Index.partition.PartitionSearcher;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using Value = Org.Neo4j.Values.Storable.Value;

	/// <summary>
	/// A <seealso cref="UniquenessVerifier"/> that is able to verify value uniqueness inside a single index partition using
	/// it's <seealso cref="PartitionSearcher"/>.
	/// <para>
	/// This verifier reads all terms, checks document frequency for each term and verifies uniqueness of values from the
	/// property store if document frequency is greater than 1.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= PartitionSearcher </seealso>
	/// <seealso cref= DuplicateCheckingCollector </seealso>
	public class SimpleUniquenessVerifier : UniquenessVerifier
	{
		 private readonly PartitionSearcher _partitionSearcher;

		 public SimpleUniquenessVerifier( PartitionSearcher partitionSearcher )
		 {
			  this._partitionSearcher = partitionSearcher;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verify(org.neo4j.storageengine.api.NodePropertyAccessor accessor, int[] propKeyIds) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, java.io.IOException
		 public override void Verify( NodePropertyAccessor accessor, int[] propKeyIds )
		 {
			  try
			  {
					DuplicateCheckingCollector collector = DuplicateCheckingCollector.ForProperties( accessor, propKeyIds );
					IndexSearcher searcher = IndexSearcher();
					foreach ( LeafReaderContext leafReaderContext in searcher.IndexReader.leaves() )
					{
						 Fields fields = leafReaderContext.reader().fields();
						 foreach ( string field in fields )
						 {
							  if ( LuceneDocumentStructure.useFieldForUniquenessVerification( field ) )
							  {
									TermsEnum terms = LuceneDocumentStructure.originalTerms( fields.terms( field ), field );
									BytesRef termsRef;
									while ( ( termsRef = terms.next() ) != null )
									{
										 if ( terms.docFreq() > 1 )
										 {
											  collector.Init( terms.docFreq() );
											  searcher.search( new TermQuery( new Term( field, termsRef ) ), collector );
										 }
									}
							  }
						 }
					}
			  }
			  catch ( IOException e )
			  {
					Exception cause = e.InnerException;
					if ( cause is IndexEntryConflictException )
					{
						 throw ( IndexEntryConflictException ) cause;
					}
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verify(org.neo4j.storageengine.api.NodePropertyAccessor accessor, int[] propKeyIds, java.util.List<org.neo4j.values.storable.Value[]> updatedValueTuples) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, java.io.IOException
		 public override void Verify( NodePropertyAccessor accessor, int[] propKeyIds, IList<Value[]> updatedValueTuples )
		 {
			  try
			  {
					DuplicateCheckingCollector collector = DuplicateCheckingCollector.ForProperties( accessor, propKeyIds );
					foreach ( Value[] valueTuple in updatedValueTuples )
					{
						 collector.Init();
						 Query query = LuceneDocumentStructure.newSeekQuery( valueTuple );
						 IndexSearcher().search(query, collector);
					}
			  }
			  catch ( IOException e )
			  {
					Exception cause = e.InnerException;
					if ( cause is IndexEntryConflictException )
					{
						 throw ( IndexEntryConflictException ) cause;
					}
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _partitionSearcher.Dispose();
		 }

		 private IndexSearcher IndexSearcher()
		 {
			  return _partitionSearcher.IndexSearcher;
		 }
	}

}