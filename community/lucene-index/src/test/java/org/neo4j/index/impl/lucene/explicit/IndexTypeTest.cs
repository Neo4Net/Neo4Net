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

	using Document = org.apache.lucene.document.Document;
	using IndexableField = Org.Apache.Lucene.Index.IndexableField;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class IndexTypeTest
	public class IndexTypeTest
	{

		 private const string STRING_TEST_FIELD = "testString";
		 private const string STRING_TEST_FIELD2 = "testString2";
		 private const string NUMERIC_TEST_FIELD = "testNumeric";
		 private const string NUMERIC_TEST_FIELD2 = "testNumeric2";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public IndexType indexType;
		 public IndexType IndexType;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public int documentFieldsPerUserField;
		 public int DocumentFieldsPerUserField;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static Iterable<Object> indexTypes()
		 public static IEnumerable<object> IndexTypes()
		 {
			  IDictionary<string, string> customIndexTypeConfig = MapUtil.stringMap( LuceneIndexImplementation.KEY_TYPE, "exact", LuceneIndexImplementation.KEY_TO_LOWER_CASE, "true" );
			  return Arrays.asList( new object[]{ IndexType.EXACT, 2 }, new object[]{ IndexType.GetIndexType( customIndexTypeConfig ), 3 } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removeFromExactIndexedDocumentRetainCorrectNumberOfFields()
		 public virtual void RemoveFromExactIndexedDocumentRetainCorrectNumberOfFields()
		 {
			  Document document = new Document();
			  IndexType.addToDocument( document, STRING_TEST_FIELD, "value" );
			  IndexType.addToDocument( document, STRING_TEST_FIELD2, "value2" );
			  IndexType.addToDocument( document, NUMERIC_TEST_FIELD, 1 );
			  IndexType.addToDocument( document, NUMERIC_TEST_FIELD2, 2 );
			  IndexType.removeFromDocument( document, STRING_TEST_FIELD, null );
			  assertEquals( "Usual fields, doc values fields for user fields and housekeeping fields.", DocumentFieldsPerUserField * 3, document.Fields.size() );
			  assertEquals( "Two string fields with specified name expected.", 2, GetDocumentFields( document, STRING_TEST_FIELD2 ).Length );
			  assertEquals( "Two numeric fields with specified name expected.", 2, GetDocumentFields( document, NUMERIC_TEST_FIELD ).Length );
			  assertEquals( "Two numeric fields with specified name expected.", 2, GetDocumentFields( document, NUMERIC_TEST_FIELD2 ).Length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removeFieldFromExactIndexedDocumentRetainCorrectNumberOfFields()
		 public virtual void RemoveFieldFromExactIndexedDocumentRetainCorrectNumberOfFields()
		 {
			  Document document = new Document();
			  IndexType.addToDocument( document, STRING_TEST_FIELD, "value" );
			  IndexType.addToDocument( document, STRING_TEST_FIELD2, "value2" );
			  IndexType.addToDocument( document, NUMERIC_TEST_FIELD, 1 );
			  IndexType.addToDocument( document, NUMERIC_TEST_FIELD2, 2 );
			  IndexType.removeFieldsFromDocument( document, NUMERIC_TEST_FIELD, null );
			  IndexType.removeFieldsFromDocument( document, STRING_TEST_FIELD2, null );
			  assertEquals( "Usual fields, doc values fields for user fields and housekeeping fields.", DocumentFieldsPerUserField * 2, document.Fields.size() );
			  assertEquals( "Two string fields with specified name expected.", 2, GetDocumentFields( document, STRING_TEST_FIELD ).Length );
			  assertEquals( "Two numeric fields with specified name expected.", 2, GetDocumentFields( document, NUMERIC_TEST_FIELD2 ).Length );
		 }

		 private static IndexableField[] GetDocumentFields( Document document, string name )
		 {
			  return document.getFields( name );
		 }
	}

}