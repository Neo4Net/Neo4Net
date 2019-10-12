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
	using LeafReader = Org.Apache.Lucene.Index.LeafReader;
	using LeafReaderContext = Org.Apache.Lucene.Index.LeafReaderContext;
	using MultiTerms = Org.Apache.Lucene.Index.MultiTerms;
	using ReaderSlice = Org.Apache.Lucene.Index.ReaderSlice;
	using Term = Org.Apache.Lucene.Index.Term;
	using Terms = Org.Apache.Lucene.Index.Terms;
	using TermsEnum = Org.Apache.Lucene.Index.TermsEnum;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using Query = org.apache.lucene.search.Query;
	using TermQuery = org.apache.lucene.search.TermQuery;
	using BytesRef = org.apache.lucene.util.BytesRef;


	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using IOUtils = Org.Neo4j.Io.IOUtils;
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using PartitionSearcher = Org.Neo4j.Kernel.Api.Impl.Index.partition.PartitionSearcher;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using Value = Org.Neo4j.Values.Storable.Value;

	/// <summary>
	/// A <seealso cref="UniquenessVerifier"/> that is able to verify value uniqueness across multiple index partitions using
	/// corresponding <seealso cref="PartitionSearcher"/>s.
	/// <para>
	/// This verifier reads all terms from all partitions using <seealso cref="MultiTerms"/>, checks document frequency for each term
	/// and verifies uniqueness of values from the property store if document frequency is greater than 1.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= MultiTerms </seealso>
	/// <seealso cref= PartitionSearcher </seealso>
	/// <seealso cref= DuplicateCheckingCollector </seealso>
	public class PartitionedUniquenessVerifier : UniquenessVerifier
	{
		 private readonly IList<PartitionSearcher> _searchers;

		 public PartitionedUniquenessVerifier( IList<PartitionSearcher> searchers )
		 {
			  this._searchers = searchers;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verify(org.neo4j.storageengine.api.NodePropertyAccessor accessor, int[] propKeyIds) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, java.io.IOException
		 public override void Verify( NodePropertyAccessor accessor, int[] propKeyIds )
		 {
			  foreach ( string field in AllFields() )
			  {
					if ( LuceneDocumentStructure.useFieldForUniquenessVerification( field ) )
					{
						 TermsEnum terms = LuceneDocumentStructure.originalTerms( TermsForField( field ), field );
						 BytesRef termsRef;
						 while ( ( termsRef = terms.next() ) != null )
						 {
							  if ( terms.docFreq() > 1 )
							  {
									TermQuery query = new TermQuery( new Term( field, termsRef ) );
									SearchForDuplicates( query, accessor, propKeyIds, terms.docFreq() );
							  }
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verify(org.neo4j.storageengine.api.NodePropertyAccessor accessor, int[] propKeyIds, java.util.List<org.neo4j.values.storable.Value[]> updatedValueTuples) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, java.io.IOException
		 public override void Verify( NodePropertyAccessor accessor, int[] propKeyIds, IList<Value[]> updatedValueTuples )
		 {
			  foreach ( Value[] valueTuple in updatedValueTuples )
			  {
					Query query = LuceneDocumentStructure.newSeekQuery( valueTuple );
					SearchForDuplicates( query, accessor, propKeyIds );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  IOUtils.closeAll( _searchers );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.apache.lucene.index.Terms termsForField(String fieldName) throws java.io.IOException
		 private Terms TermsForField( string fieldName )
		 {
			  IList<Terms> terms = new List<Terms>();
			  IList<ReaderSlice> readerSlices = new List<ReaderSlice>();

			  foreach ( LeafReader leafReader in AllLeafReaders() )
			  {
					Fields fields = leafReader.fields();

					Terms leafTerms = fields.terms( fieldName );
					if ( leafTerms != null )
					{
						 ReaderSlice readerSlice = new ReaderSlice( 0, Math.toIntExact( leafTerms.size() ), 0 );
						 terms.Add( leafTerms );
						 readerSlices.Add( readerSlice );
					}
			  }

			  Terms[] termsArray = terms.ToArray();
			  ReaderSlice[] readerSlicesArray = readerSlices.ToArray();

			  return new MultiTerms( termsArray, readerSlicesArray );
		 }

		 /// <summary>
		 /// Search for unknown number of duplicates duplicates
		 /// </summary>
		 /// <param name="query"> query to find duplicates in </param>
		 /// <param name="accessor"> accessor to load actual property value from store </param>
		 /// <param name="propertyKeyIds"> property key ids </param>
		 /// <exception cref="IOException"> </exception>
		 /// <exception cref="IndexEntryConflictException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void searchForDuplicates(org.apache.lucene.search.Query query, org.neo4j.storageengine.api.NodePropertyAccessor accessor, int[] propertyKeyIds) throws java.io.IOException, org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 private void SearchForDuplicates( Query query, NodePropertyAccessor accessor, int[] propertyKeyIds )
		 {
			  DuplicateCheckingCollector collector = GetDuplicateCollector( accessor, propertyKeyIds );
			  collector.Init();
			  SearchForDuplicates( query, collector );
		 }

		 /// <summary>
		 /// Search for known number of duplicates duplicates
		 /// </summary>
		 /// <param name="query"> query to find duplicates in </param>
		 /// <param name="accessor"> accessor to load actual property value from store </param>
		 /// <param name="propertyKeyIds"> property key ids </param>
		 /// <param name="expectedNumberOfEntries"> expected number of duplicates in query </param>
		 /// <exception cref="IOException"> </exception>
		 /// <exception cref="IndexEntryConflictException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void searchForDuplicates(org.apache.lucene.search.Query query, org.neo4j.storageengine.api.NodePropertyAccessor accessor, int[] propertyKeyIds, int expectedNumberOfEntries) throws java.io.IOException, org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 private void SearchForDuplicates( Query query, NodePropertyAccessor accessor, int[] propertyKeyIds, int expectedNumberOfEntries )
		 {
			  DuplicateCheckingCollector collector = GetDuplicateCollector( accessor, propertyKeyIds );
			  collector.Init( expectedNumberOfEntries );
			  SearchForDuplicates( query, collector );
		 }

		 private DuplicateCheckingCollector GetDuplicateCollector( NodePropertyAccessor accessor, int[] propertyKeyIds )
		 {
			  return DuplicateCheckingCollector.ForProperties( accessor, propertyKeyIds );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void searchForDuplicates(org.apache.lucene.search.Query query, DuplicateCheckingCollector collector) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, java.io.IOException
		 private void SearchForDuplicates( Query query, DuplicateCheckingCollector collector )
		 {
			  try
			  {
					foreach ( PartitionSearcher searcher in _searchers )
					{
							  /*
							   * Here {@link DuplicateCheckingCollector#init()} is deliberately not called to preserve accumulated
							   * state (knowledge about duplicates) across all {@link IndexSearcher#search(Query, Collector)} calls.
							   */
						 searcher.IndexSearcher.search( query, collector );
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
//ORIGINAL LINE: private java.util.Set<String> allFields() throws java.io.IOException
		 private ISet<string> AllFields()
		 {
			  ISet<string> allFields = new HashSet<string>();
			  foreach ( LeafReader leafReader in AllLeafReaders() )
			  {
					Iterables.addAll( allFields, leafReader.fields() );
			  }
			  return allFields;
		 }

		 private IList<LeafReader> AllLeafReaders()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return _searchers.Select( PartitionSearcher::getIndexSearcher ).Select( IndexSearcher.getIndexReader ).flatMap( indexReader => indexReader.leaves().stream() ).Select(LeafReaderContext.reader).ToList();
		 }
	}

}