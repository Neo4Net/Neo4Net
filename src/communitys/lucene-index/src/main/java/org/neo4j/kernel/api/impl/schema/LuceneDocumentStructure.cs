using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Kernel.Api.Impl.Schema
{
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using Document = org.apache.lucene.document.Document;
	using Field = org.apache.lucene.document.Field;
	using NumericDocValuesField = org.apache.lucene.document.NumericDocValuesField;
	using StringField = org.apache.lucene.document.StringField;
	using FilteredTermsEnum = Org.Apache.Lucene.Index.FilteredTermsEnum;
	using IndexableField = Org.Apache.Lucene.Index.IndexableField;
	using Term = Org.Apache.Lucene.Index.Term;
	using Terms = Org.Apache.Lucene.Index.Terms;
	using TermsEnum = Org.Apache.Lucene.Index.TermsEnum;
	using QueryParser = org.apache.lucene.queryparser.classic.QueryParser;
	using BooleanClause = org.apache.lucene.search.BooleanClause;
	using BooleanQuery = org.apache.lucene.search.BooleanQuery;
	using ConstantScoreQuery = org.apache.lucene.search.ConstantScoreQuery;
	using MatchAllDocsQuery = org.apache.lucene.search.MatchAllDocsQuery;
	using MultiTermQuery = org.apache.lucene.search.MultiTermQuery;
	using NumericRangeQuery = org.apache.lucene.search.NumericRangeQuery;
	using PrefixQuery = org.apache.lucene.search.PrefixQuery;
	using Query = org.apache.lucene.search.Query;
	using TermQuery = org.apache.lucene.search.TermQuery;
	using TermRangeQuery = org.apache.lucene.search.TermRangeQuery;
	using WildcardQuery = org.apache.lucene.search.WildcardQuery;
	using AttributeSource = org.apache.lucene.util.AttributeSource;
	using BytesRef = org.apache.lucene.util.BytesRef;
	using NumericUtils = org.apache.lucene.util.NumericUtils;
	using StringHelper = org.apache.lucene.util.StringHelper;


	using FeatureToggles = Neo4Net.Utils.FeatureToggles;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.lucene.document.Field.Store.YES;

	public class LuceneDocumentStructure
	{
		 private static readonly bool _useLuceneStandardPrefixQuery = FeatureToggles.flag( typeof( LuceneDocumentStructure ), "lucene.standard.prefix.query", false );

		 public const string NODE_ID_KEY = "id";

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 private static readonly ThreadLocal<DocWithId> _perThreadDocument = ThreadLocal.withInitial( DocWithId::new );
		 public const string DELIMITER = "\u001F";

		 private LuceneDocumentStructure()
		 {
		 }

		 private static DocWithId ReuseDocument( long nodeId )
		 {
			  DocWithId doc = _perThreadDocument.get();
			  doc.Id = nodeId;
			  return doc;
		 }

		 public static Document DocumentRepresentingProperties( long nodeId, params Value[] values )
		 {
			  DocWithId document = ReuseDocument( nodeId );
			  document.Values = values;
			  return document.Document;
		 }

		 public static string EncodedStringValuesForSampling( params Value[] values )
		 {
			  StringBuilder sb = new StringBuilder();
			  string sep = "";
			  foreach ( Value value in values )
			  {
					sb.Append( sep );
					sep = DELIMITER;
					ValueEncoding encoding = ValueEncoding.forValue( value );
					sb.Append( encoding.encodeField( encoding.key(), value ).stringValue() );
			  }
			  return sb.ToString();
		 }

		 public static MatchAllDocsQuery NewScanQuery()
		 {
			  return new MatchAllDocsQuery();
		 }

		 public static Query NewSeekQuery( params Value[] values )
		 {
			  BooleanQuery.Builder builder = new BooleanQuery.Builder();
			  for ( int i = 0; i < values.Length; i++ )
			  {
					ValueEncoding encoding = ValueEncoding.forValue( values[i] );
					builder.add( encoding.encodeQuery( values[i], i ), BooleanClause.Occur.MUST );
			  }
			  return builder.build();
		 }

		 /// <summary>
		 /// Range queries are always inclusive, in order to do exclusive range queries the result must be filtered after the
		 /// fact. The reason we can't do inclusive range queries is that longs are coerced to doubles in the index.
		 /// </summary>
		 public static NumericRangeQuery<double> NewInclusiveNumericRangeSeekQuery( Number lower, Number upper )
		 {
			  double? min = lower != null ? lower.doubleValue() : null;
			  double? max = upper != null ? upper.doubleValue() : null;
			  return NumericRangeQuery.newDoubleRange( ValueEncoding.Number.key( 0 ), min, max, true, true );
		 }

		 public static Query NewRangeSeekByStringQuery( string lower, bool includeLower, string upper, bool includeUpper )
		 {
			  bool includeLowerBoundary = StringUtils.EMPTY.Equals( lower ) || includeLower;
			  bool includeUpperBoundary = StringUtils.EMPTY.Equals( upper ) || includeUpper;
			  TermRangeQuery termRangeQuery = TermRangeQuery.newStringRange( ValueEncoding.String.key( 0 ), lower, upper, includeLowerBoundary, includeUpperBoundary );

			  if ( ( includeLowerBoundary != includeLower ) || ( includeUpperBoundary != includeUpper ) )
			  {
					BooleanQuery.Builder builder = new BooleanQuery.Builder();
					builder.DisableCoord = true;
					if ( includeLowerBoundary != includeLower )
					{
						 builder.add( new TermQuery( new Term( ValueEncoding.String.key( 0 ), lower ) ), BooleanClause.Occur.MUST_NOT );
					}
					if ( includeUpperBoundary != includeUpper )
					{
						 builder.add( new TermQuery( new Term( ValueEncoding.String.key( 0 ), upper ) ), BooleanClause.Occur.MUST_NOT );
					}
					builder.add( termRangeQuery, BooleanClause.Occur.FILTER );
					return new ConstantScoreQuery( builder.build() );
			  }
			  return termRangeQuery;
		 }

		 public static Query NewWildCardStringQuery( string searchFor )
		 {
			  string searchTerm = QueryParser.escape( searchFor );
			  Term term = new Term( ValueEncoding.String.key( 0 ), "*" + searchTerm + "*" );

			  return new WildcardQuery( term );
		 }

		 public static Query NewRangeSeekByPrefixQuery( string prefix )
		 {
			  Term term = new Term( ValueEncoding.String.key( 0 ), prefix );
			  return _useLuceneStandardPrefixQuery ? new PrefixQuery( term ) : new PrefixMultiTermsQuery( term );
		 }

		 public static Query NewSuffixStringQuery( string suffix )
		 {
			  string searchTerm = QueryParser.escape( suffix );
			  Term term = new Term( ValueEncoding.String.key( 0 ), "*" + searchTerm );

			  return new WildcardQuery( term );
		 }

		 public static Term NewTermForChangeOrRemove( long nodeId )
		 {
			  return new Term( NODE_ID_KEY, "" + nodeId );
		 }

		 public static long GetNodeId( Document from )
		 {
			  return long.Parse( from.get( NODE_ID_KEY ) );
		 }

		 /// <summary>
		 /// Filters the given <seealso cref="Terms terms"/> to include only terms that were created using fields from
		 /// <seealso cref="ValueEncoding.encodeField(string, Value)"/>. Internal lucene terms like those created for indexing numeric values
		 /// (see javadoc for <seealso cref="NumericRangeQuery"/> class) are skipped. In other words this method returns
		 /// <seealso cref="TermsEnum"/> over all terms for the given field that were created using <seealso cref="ValueEncoding"/>.
		 /// </summary>
		 /// <param name="terms"> the terms to be filtered </param>
		 /// <param name="fieldKey"> the corresponding <seealso cref="ValueEncoding.key(int) field key"/> </param>
		 /// <returns> terms enum over all inserted terms </returns>
		 /// <exception cref="IOException"> if it is not possible to obtain <seealso cref="TermsEnum"/> </exception>
		 /// <seealso cref= NumericRangeQuery </seealso>
		 /// <seealso cref= org.apache.lucene.analysis.NumericTokenStream </seealso>
		 /// <seealso cref= NumericUtils#PRECISION_STEP_DEFAULT </seealso>
		 /// <seealso cref= NumericUtils#filterPrefixCodedLongs(TermsEnum) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.apache.lucene.index.TermsEnum originalTerms(org.apache.lucene.index.Terms terms, String fieldKey) throws java.io.IOException
		 public static TermsEnum OriginalTerms( Terms terms, string fieldKey )
		 {
			  TermsEnum termsEnum = terms.GetEnumerator();
			  return ValueEncoding.forKey( fieldKey ) == ValueEncoding.Number ? NumericUtils.filterPrefixCodedLongs( termsEnum ) : termsEnum;
		 }

		 /// <summary>
		 /// Simple implementation of prefix query that mimics old lucene way of handling prefix queries.
		 /// According to benchmarks this implementation is faster then
		 /// <seealso cref="org.apache.lucene.search.PrefixQuery"/> because we do not construct automaton  which is
		 /// extremely expensive.
		 /// </summary>
		 private class PrefixMultiTermsQuery : MultiTermQuery
		 {
			  internal Term Term;

			  internal PrefixMultiTermsQuery( Term term ) : base( term.field() )
			  {
					this.Term = term;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.apache.lucene.index.TermsEnum getTermsEnum(org.apache.lucene.index.Terms terms, org.apache.lucene.util.AttributeSource atts) throws java.io.IOException
			  protected internal override TermsEnum GetTermsEnum( Terms terms, AttributeSource atts )
			  {
					return Term.bytes().length == 0 ? terms.GetEnumerator() : new PrefixTermsEnum(terms.GetEnumerator(), Term.bytes());
			  }

			  public override string ToString( string field )
			  {
					return this.GetType().Name + ", term:" + Term + ", field:" + field;
			  }

			  private class PrefixTermsEnum : FilteredTermsEnum
			  {
					internal BytesRef Prefix;

					internal PrefixTermsEnum( TermsEnum termEnum, BytesRef prefix ) : base( termEnum )
					{
						 this.Prefix = prefix;
						 InitialSeekTerm = this.Prefix;
					}

					protected internal override AcceptStatus Accept( BytesRef term )
					{
						 return StringHelper.StartsWith( term, Prefix ) ? AcceptStatus.YES : AcceptStatus.END;
					}
			  }
		 }

		 public static Field EncodeValueField( Value value )
		 {
			  ValueEncoding encoding = ValueEncoding.forValue( value );
			  return encoding.encodeField( encoding.key(), value );
		 }

		 public static bool UseFieldForUniquenessVerification( string fieldName )
		 {
			  return !LuceneDocumentStructure.NODE_ID_KEY.Equals( fieldName ) && ValueEncoding.fieldPropertyNumber( fieldName ) == 0;
		 }

		 private class DocWithId
		 {
			  internal readonly Document Document;

			  internal readonly Field IdField;
			  internal readonly Field IdValueField;

			  internal Field[] ReusableValueFields = new Field[0];

			  internal DocWithId()
			  {
					IdField = new StringField( NODE_ID_KEY, "", YES );
					IdValueField = new NumericDocValuesField( NODE_ID_KEY, 0L );
					Document = new Document();
					Document.add( IdField );
					Document.add( IdValueField );
			  }

			  internal virtual long Id
			  {
				  set
				  {
						IdField.StringValue = Convert.ToString( value );
						IdValueField.LongValue = value;
				  }
			  }

			  internal virtual params Value[] Values
			  {
				  set
				  {
						RemoveAllValueFields();
						int neededLength = value.Length * ValueEncoding.values().length;
						if ( ReusableValueFields.Length < neededLength )
						{
							 ReusableValueFields = new Field[neededLength];
						}
   
						for ( int i = 0; i < value.Length; i++ )
						{
							 Field reusableField = GetFieldWithValue( i, value[i] );
							 Document.add( reusableField );
						}
				  }
			  }

			  internal virtual void RemoveAllValueFields()
			  {
					IEnumerator<IndexableField> it = Document.Fields.GetEnumerator();
					while ( it.MoveNext() )
					{
						 IndexableField field = it.Current;
						 string fieldName = field.name();
						 if ( !fieldName.Equals( NODE_ID_KEY ) )
						 {
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
							  it.remove();
						 }
					}
			  }

			  internal virtual Field GetFieldWithValue( int propertyNumber, Value value )
			  {
					ValueEncoding encoding = ValueEncoding.forValue( value );
					int reuseId = propertyNumber * ValueEncoding.values().length + encoding.ordinal();
					string key = encoding.key( propertyNumber );
					Field reusableField = ReusableValueFields[reuseId];
					if ( reusableField == null )
					{
						 reusableField = encoding.encodeField( key, value );
						 ReusableValueFields[reuseId] = reusableField;
					}
					else
					{
						 encoding.setFieldValue( value, reusableField );
					}
					return reusableField;
			  }
		 }
	}

}