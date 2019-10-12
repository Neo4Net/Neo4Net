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
namespace Org.Neo4j.Index.impl.lucene.@explicit
{
	using Analyzer = org.apache.lucene.analysis.Analyzer;
	using Document = org.apache.lucene.document.Document;
	using DoubleField = org.apache.lucene.document.DoubleField;
	using Field = org.apache.lucene.document.Field;
	using Store = org.apache.lucene.document.Field.Store;
	using FieldType = org.apache.lucene.document.FieldType;
	using FloatField = org.apache.lucene.document.FloatField;
	using IntField = org.apache.lucene.document.IntField;
	using LongField = org.apache.lucene.document.LongField;
	using NumericDocValuesField = org.apache.lucene.document.NumericDocValuesField;
	using SortedNumericDocValuesField = org.apache.lucene.document.SortedNumericDocValuesField;
	using SortedSetDocValuesField = org.apache.lucene.document.SortedSetDocValuesField;
	using StringField = org.apache.lucene.document.StringField;
	using TextField = org.apache.lucene.document.TextField;
	using DocValuesType = Org.Apache.Lucene.Index.DocValuesType;
	using IndexableField = Org.Apache.Lucene.Index.IndexableField;
	using Term = Org.Apache.Lucene.Index.Term;
	using ParseException = org.apache.lucene.queryparser.classic.ParseException;
	using QueryParser = org.apache.lucene.queryparser.classic.QueryParser;
	using Query = org.apache.lucene.search.Query;
	using TermQuery = org.apache.lucene.search.TermQuery;
	using Similarity = org.apache.lucene.search.similarities.Similarity;
	using BytesRef = org.apache.lucene.util.BytesRef;
	using NumericUtils = org.apache.lucene.util.NumericUtils;


	using QueryContext = Org.Neo4j.Index.lucene.QueryContext;
	using ValueContext = Org.Neo4j.Index.lucene.ValueContext;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static false;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static true;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.impl.lucene.@explicit.LuceneExplicitIndex.isValidKey;

	public abstract class IndexType
	{
		 public static readonly IndexType EXACT = new IndexTypeAnonymousInnerClass( LuceneDataSource.KeywordAnalyzer );

		 private class IndexTypeAnonymousInnerClass : IndexType
		 {
			 public IndexTypeAnonymousInnerClass( Analyzer keywordAnalyzer ) : base( keywordAnalyzer, false )
			 {
			 }

			 public override Query get( string key, object value )
			 {
				  return outerInstance.queryForGet( key, value );
			 }

			 internal override void removeFieldsFromDocument( Document document, string key, object value )
			 {
				  outerInstance.removeFieldsFromDocument( document, key, key, value );
			 }

			 protected internal override void addNewFieldToDocument( Document document, string key, object value )
			 {
				  document.add( InstantiateField( key, value, StringField.TYPE_STORED ) );
				  document.add( InstantiateSortField( key, value ) );
			 }

			 internal override void removeFieldFromDocument( Document document, string name )
			 {
				  document.removeFields( name );
			 }

			 public override string ToString()
			 {
				  return "EXACT";
			 }
		 }

		 private class CustomType : IndexType
		 {
			  public const string EXACT_FIELD_SUFFIX = "_e";
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Similarity SimilarityConflict;

			  internal CustomType( Analyzer analyzer, bool toLowerCase, Similarity similarity ) : base( analyzer, toLowerCase )
			  {
					this.SimilarityConflict = similarity;
			  }

			  internal override Similarity Similarity
			  {
				  get
				  {
						return this.SimilarityConflict;
				  }
			  }

			  public override Query Get( string key, object value )
			  {
					// TODO we do value.toString() here since initially #addToDocument didn't
					// honor ValueContext, and changing it would mean changing store format.
					return new TermQuery( new Term( ExactKey( key ), value.ToString() ) );
			  }

			  internal virtual string ExactKey( string key )
			  {
					return key + EXACT_FIELD_SUFFIX;
			  }

			  // TODO We should honor ValueContext instead of doing value.toString() here.
			  // if changing it, also change #get to honor ValueContext.
			  protected internal override void AddNewFieldToDocument( Document document, string key, object value )
			  {
					document.add( new StringField( ExactKey( key ), value.ToString(), Field.Store.YES ) );
					document.add( InstantiateField( key, value, TextField.TYPE_STORED ) );
					document.add( InstantiateSortField( key, value ) );
			  }

			  internal override void RemoveFieldFromDocument( Document document, string name )
			  {
					document.removeFields( ExactKey( name ) );
					document.removeFields( name );
			  }

			  internal override void RemoveFieldsFromDocument( Document document, string key, object value )
			  {
					RemoveFieldsFromDocument( document, key, ExactKey( key ), value );
			  }

			  protected internal override bool IsStoredField( IndexableField field )
			  {
					return !field.name().EndsWith(CustomType.EXACT_FIELD_SUFFIX) && base.IsStoredField(field);
			  }

			  public override string ToString()
			  {
					return "FULLTEXT";
			  }
		 }

		 internal readonly Analyzer Analyzer;
		 private readonly bool _toLowerCase;

		 private IndexType( Analyzer analyzer, bool toLowerCase )
		 {
			  this.Analyzer = analyzer;
			  this._toLowerCase = toLowerCase;
		 }

		 internal abstract void RemoveFieldsFromDocument( Document document, string key, object value );

		 internal abstract void RemoveFieldFromDocument( Document document, string name );

		 internal abstract void AddNewFieldToDocument( Document document, string key, object value );

		 internal abstract Query Get( string key, object value );

		 internal static IndexType GetIndexType( IDictionary<string, string> config )
		 {
			  string type = config[LuceneIndexImplementation.KEY_TYPE];
			  IndexType result = null;
			  Similarity similarity = GetCustomSimilarity( config );
			  bool? toLowerCaseUnbiased = !string.ReferenceEquals( config[LuceneIndexImplementation.KEY_TO_LOWER_CASE], null ) ? ParseBoolean( config[LuceneIndexImplementation.KEY_TO_LOWER_CASE], true ) : null;
			  Analyzer customAnalyzer = GetCustomAnalyzer( config );
			  if ( !string.ReferenceEquals( type, null ) )
			  {
					// Use the built in alternatives... "exact" or "fulltext"
					if ( "exact".Equals( type ) )
					{
						 // In the exact case we default to false
						 bool toLowerCase = TRUE.Equals( toLowerCaseUnbiased );

						 result = toLowerCase ? new CustomType( new LowerCaseKeywordAnalyzer(), true, similarity ) : EXACT;
					}
					else if ( "fulltext".Equals( type ) )
					{
						 // In the fulltext case we default to true
						 bool toLowerCase = !FALSE.Equals( toLowerCaseUnbiased );

						 Analyzer analyzer = customAnalyzer;
						 if ( analyzer == null )
						 {
							  analyzer = TRUE.Equals( toLowerCase ) ? LuceneDataSource.LOWER_CASE_WHITESPACE_ANALYZER : LuceneDataSource.WhitespaceAnalyzer;
						 }
						 result = new CustomType( analyzer, toLowerCase, similarity );
					}
					else
					{
						 throw new System.ArgumentException( "The given type was not recognized: " + type + ". Known types are 'fulltext' and 'exact'" );
					}
			  }
			  else
			  {
					// In the custom case we default to true
					bool toLowerCase = !FALSE.Equals( toLowerCaseUnbiased );

					// Use custom analyzer
					if ( customAnalyzer == null )
					{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						 throw new System.ArgumentException( "No 'type' was given (which can point out " + "built-in analyzers, such as 'exact' and 'fulltext')" + " and no 'analyzer' was given either (which can point out a custom " + typeof( Analyzer ).FullName + " to use)" );
					}
					result = new CustomType( customAnalyzer, toLowerCase, similarity );
			  }
			  return result;
		 }

		 public virtual void AddToDocument( Document document, string key, object value )
		 {
			  AddNewFieldToDocument( document, key, value );
			  RestoreSortFields( document );
		 }

		 protected internal virtual bool IsStoredField( IndexableField field )
		 {
			  return isValidKey( field.name() ) && field.fieldType().stored() && !FullTxData.TX_STATE_KEY.Equals(field.name());
		 }

		 private static bool ParseBoolean( string @string, bool valueIfNull )
		 {
			  return string.ReferenceEquals( @string, null ) ? valueIfNull : bool.Parse( @string );
		 }

		 private static Similarity GetCustomSimilarity( IDictionary<string, string> config )
		 {
			  return GetByClassName( config, LuceneIndexImplementation.KEY_SIMILARITY, typeof( Similarity ) );
		 }

		 private static Analyzer GetCustomAnalyzer( IDictionary<string, string> config )
		 {
			  return GetByClassName( config, LuceneIndexImplementation.KEY_ANALYZER, typeof( Analyzer ) );
		 }

		 private static T GetByClassName<T>( IDictionary<string, string> config, string configKey, Type cls )
		 {
				 cls = typeof( T );
			  string className = config[configKey];
			  if ( !string.ReferenceEquals( className, null ) )
			  {
					try
					{
						 return Type.GetType( className ).asSubclass( cls ).newInstance();
					}
					catch ( Exception e )
					{
						 throw new Exception( e );
					}
			  }
			  return default( T );
		 }

		 internal virtual TxData NewTxData( LuceneExplicitIndex index )
		 {
			  return new ExactTxData( index );
		 }

		 internal virtual Query Query( string keyOrNull, object value, QueryContext contextOrNull )
		 {
			  if ( value is Query )
			  {
					return ( Query ) value;
			  }

			  QueryParser parser = new QueryParser( keyOrNull, Analyzer );
			  parser.AllowLeadingWildcard = true;
			  parser.LowercaseExpandedTerms = _toLowerCase;
			  if ( contextOrNull != null && contextOrNull.DefaultOperator != null )
			  {
					parser.DefaultOperator = contextOrNull.DefaultOperator;
			  }
			  try
			  {
					return parser.parse( value.ToString() );
			  }
			  catch ( ParseException e )
			  {
					throw new Exception( e );
			  }
		 }

		 public static IndexableField InstantiateField( string key, object value, FieldType fieldType )
		 {
			  IndexableField field;
			  if ( value is Number )
			  {
					Number number = ( Number ) value;
					if ( value is long? )
					{
						 field = new LongField( key, number.longValue(), Field.Store.YES );
					}
					else if ( value is float? )
					{
						 field = new FloatField( key, number.floatValue(), Field.Store.YES );
					}
					else if ( value is double? )
					{
						 field = new DoubleField( key, number.doubleValue(), Field.Store.YES );
					}
					else
					{
						 field = new IntField( key, number.intValue(), Field.Store.YES );
					}
			  }
			  else
			  {
					field = new Field( key, value.ToString(), fieldType );
			  }
			  return field;
		 }

		 public static IndexableField InstantiateSortField( string key, object value )
		 {
			  IndexableField field;
			  if ( value is Number )
			  {
					Number number = ( Number ) value;
					if ( value is float? )
					{
						 field = new SortedNumericDocValuesField( key, NumericUtils.floatToSortableInt( number.floatValue() ) );
					}
					else if ( value is double? )
					{
						 field = new SortedNumericDocValuesField( key, NumericUtils.doubleToSortableLong( number.doubleValue() ) );
					}
					else
					{
						 field = new SortedNumericDocValuesField( key, number.longValue() );
					}
			  }
			  else
			  {
					if ( LuceneExplicitIndex.KEY_DOC_ID.Equals( key ) )
					{
						 field = new NumericDocValuesField( key, long.Parse( value.ToString() ) );
					}
					else
					{
						 field = new SortedSetDocValuesField( key, new BytesRef( value.ToString() ) );
					}
			  }
			  return field;
		 }

		 internal void RemoveFromDocument( Document document, string key, object value )
		 {
			  if ( string.ReferenceEquals( key, null ) && value == null )
			  {
					ClearDocument( document );
			  }
			  else
			  {
					RemoveFieldsFromDocument( document, key, value );
					RestoreSortFields( document );
			  }
		 }

		 private void ClearDocument( Document document )
		 {
			  ISet<string> names = new HashSet<string>();
			  foreach ( IndexableField field in document.Fields )
			  {
					string name = field.name();
					if ( LuceneExplicitIndex.IsValidKey( name ) )
					{
						 names.Add( name );
					}
			  }
			  foreach ( string name in names )
			  {
					document.removeFields( name );
			  }
		 }

		 // Re-add field since their index info is lost after reading the fields from the index store
		 internal virtual void RestoreSortFields( Document document )
		 {
			  ICollection<IndexableField> notIndexedStoredFields = GetNotIndexedStoredFields( document );
			  foreach ( IndexableField field in notIndexedStoredFields )
			  {
					object fieldValue = GetFieldValue( field );
					string name = field.name();
					RemoveFieldsFromDocument( document, name, fieldValue );
					AddNewFieldToDocument( document, name, fieldValue );
			  }
		 }

		 private ICollection<IndexableField> GetNotIndexedStoredFields( Document document )
		 {
			  IDictionary<string, IndexableField> nameFieldMap = new Dictionary<string, IndexableField>();
			  IList<string> indexedFields = new List<string>();
			  foreach ( IndexableField field in document.Fields )
			  {
					if ( IsStoredField( field ) )
					{
						 nameFieldMap[field.name()] = field;
					}
					else if ( !DocValuesType.NONE.Equals( field.fieldType().docValuesType() ) )
					{
						 indexedFields.Add( field.name() );
					}
			  }
			  indexedFields.ForEach( nameFieldMap.remove );
			  return nameFieldMap.Values;
		 }

		 internal virtual void RemoveFieldsFromDocument( Document document, string key, string exactKey, object value )
		 {
			  ISet<string> values = null;
			  if ( value != null )
			  {
					string stringValue = value.ToString();
					values = new HashSet<string>( Arrays.asList( document.getValues( exactKey ) ) );
					if ( !values.remove( stringValue ) )
					{
						 return;
					}
			  }
			  RemoveFieldFromDocument( document, key );

			  if ( value != null )
			  {
					foreach ( string existingValue in values )
					{
						 AddNewFieldToDocument( document, key, existingValue );
					}
			  }
		 }

		 private object GetFieldValue( IndexableField field )
		 {
			  Number numericFieldValue = field.numericValue();
			  return numericFieldValue != null ? numericFieldValue : field.stringValue();
		 }

		 public static Document NewBaseDocument( long entityId )
		 {
			  Document doc = new Document();
			  doc.add( new StringField( LuceneExplicitIndex.KEY_DOC_ID, "" + entityId, Field.Store.YES ) );
			  doc.add( new NumericDocValuesField( LuceneExplicitIndex.KEY_DOC_ID, entityId ) );
			  return doc;
		 }

		 public static Document NewDocument( EntityId entityId )
		 {
			  Document document = NewBaseDocument( entityId.Id() );
			  entityId.Enhance( document );
			  return document;
		 }

		 public virtual Term IdTerm( long entityId )
		 {
			  return new Term( LuceneExplicitIndex.KEY_DOC_ID, "" + entityId );
		 }

		 internal virtual Query IdTermQuery( long entityId )
		 {
			  return new TermQuery( IdTerm( entityId ) );
		 }

		 internal virtual Similarity Similarity
		 {
			 get
			 {
				  return null;
			 }
		 }

		 internal virtual Query QueryForGet( string key, object value )
		 {
			  if ( value is ValueContext )
			  {
					object realValue = ( ( ValueContext )value ).Value;
					if ( realValue is Number )
					{
						 Number number = ( Number ) realValue;
						 return LuceneUtil.RangeQuery( key, number, number, true, true );
					}
			  }
			  return new TermQuery( new Term( key, value.ToString() ) );
		 }
	}

}