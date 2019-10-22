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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{
	using Document = org.apache.lucene.document.Document;
	using Field = org.apache.lucene.document.Field;
	using NumericDocValuesField = org.apache.lucene.document.NumericDocValuesField;
	using StringField = org.apache.lucene.document.StringField;
	using TextField = org.apache.lucene.document.TextField;
	using IndexableField = Org.Apache.Lucene.Index.IndexableField;
	using Term = Org.Apache.Lucene.Index.Term;
	using BooleanClause = org.apache.lucene.search.BooleanClause;
	using BooleanQuery = org.apache.lucene.search.BooleanQuery;
	using ConstantScoreQuery = org.apache.lucene.search.ConstantScoreQuery;
	using Query = org.apache.lucene.search.Query;
	using TermQuery = org.apache.lucene.search.TermQuery;


	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.lucene.document.Field.Store.NO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.lucene.document.Field.Store.YES;

	public class LuceneFulltextDocumentStructure
	{
		 public const string FIELD_ENTITY_ID = "__Neo4Net__lucene__fulltext__index__internal__id__";

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 private static readonly ThreadLocal<DocWithId> _perThreadDocument = ThreadLocal.withInitial( DocWithId::new );

		 private LuceneFulltextDocumentStructure()
		 {
		 }

		 private static DocWithId ReuseDocument( long id )
		 {
			  DocWithId doc = _perThreadDocument.get();
			  doc.Id = id;
			  return doc;
		 }

		 public static Document DocumentRepresentingProperties( long id, ICollection<string> propertyNames, Value[] values )
		 {
			  DocWithId document = ReuseDocument( id );
			  document.SetValues( propertyNames, values );
			  return document.Document;
		 }

		 private static Field EncodeValueField( string propertyKey, Value value )
		 {
			  TextValue textValue = ( TextValue ) value;
			  string stringValue = textValue.StringValue();
			  return new TextField( propertyKey, stringValue, NO );
		 }

		 internal static long GetNodeId( Document from )
		 {
			  string IEntityId = from.get( FIELD_ENTITY_ID );
			  return long.Parse( IEntityId );
		 }

		 internal static Term NewTermForChangeOrRemove( long id )
		 {
			  return new Term( FIELD_ENTITY_ID, "" + id );
		 }

		 internal static Query NewCountNodeEntriesQuery( long nodeId, string[] propertyKeys, params Value[] propertyValues )
		 {
			  BooleanQuery.Builder builder = new BooleanQuery.Builder();
			  builder.add( new TermQuery( NewTermForChangeOrRemove( nodeId ) ), BooleanClause.Occur.MUST );
			  for ( int i = 0; i < propertyKeys.Length; i++ )
			  {
					string propertyKey = propertyKeys[i];
					Value value = propertyValues[i];
					if ( value.ValueGroup() == ValueGroup.TEXT )
					{
						 Query valueQuery = new ConstantScoreQuery( new TermQuery( new Term( propertyKey, value.AsObject().ToString() ) ) );
						 builder.add( valueQuery, BooleanClause.Occur.SHOULD );
					}
			  }
			  return builder.build();
		 }

		 private class DocWithId
		 {
			  internal readonly Document Document;

			  internal readonly Field IdField;
			  internal readonly Field IdValueField;

			  internal DocWithId()
			  {
					IdField = new StringField( FIELD_ENTITY_ID, "", YES );
					IdValueField = new NumericDocValuesField( FIELD_ENTITY_ID, 0L );
					Document = new Document();
					Document.add( IdField );
					Document.add( IdValueField );
			  }

			  internal virtual long Id
			  {
				  set
				  {
						RemoveAllValueFields();
						IdField.StringValue = Convert.ToString( value );
						IdValueField.LongValue = value;
				  }
			  }

			  internal virtual void SetValues( ICollection<string> names, Value[] values )
			  {
					int i = 0;
					foreach ( string name in names )
					{
						 Value value = values[i++];
						 if ( value != null && value.ValueGroup() == ValueGroup.TEXT )
						 {
							  Field field = EncodeValueField( name, value );
							  Document.add( field );
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
						 if ( !fieldName.Equals( FIELD_ENTITY_ID ) )
						 {
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
							  it.remove();
						 }
					}
			  }
		 }
	}

}