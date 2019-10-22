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
	using RandomStringUtils = org.apache.commons.lang3.RandomStringUtils;
	using Document = org.apache.lucene.document.Document;
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using BooleanQuery = org.apache.lucene.search.BooleanQuery;
	using ConstantScoreQuery = org.apache.lucene.search.ConstantScoreQuery;
	using MultiTermQuery = org.apache.lucene.search.MultiTermQuery;
	using NumericRangeQuery = org.apache.lucene.search.NumericRangeQuery;
	using TermQuery = org.apache.lucene.search.TermQuery;
	using TermRangeQuery = org.apache.lucene.search.TermRangeQuery;
	using WildcardQuery = org.apache.lucene.search.WildcardQuery;
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.impl.LuceneTestUtil.documentRepresentingProperties;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.impl.LuceneTestUtil.newSeekQuery;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.impl.schema.LuceneDocumentStructure.NODE_ID_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.impl.schema.LuceneDocumentStructure.useFieldForUniquenessVerification;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.impl.schema.ValueEncoding.Array;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.impl.schema.ValueEncoding.Bool;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.impl.schema.ValueEncoding.Number;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.impl.schema.ValueEncoding.String;

	internal class LuceneDocumentStructureTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringWithMaximumLengthShouldBeAllowed()
		 internal virtual void StringWithMaximumLengthShouldBeAllowed()
		 {
			  string longestString = RandomStringUtils.randomAscii( IndexWriter.MAX_TERM_LENGTH );
			  Document document = documentRepresentingProperties( ( long ) 123, longestString );
			  assertEquals( longestString, document.getField( string.key( 0 ) ).stringValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBuildDocumentRepresentingStringProperty()
		 internal virtual void ShouldBuildDocumentRepresentingStringProperty()
		 {
			  // given
			  Document document = documentRepresentingProperties( ( long ) 123, "hello" );

			  // then
			  assertEquals( "123", document.get( NODE_ID_KEY ) );
			  assertEquals( "hello", document.get( string.key( 0 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBuildDocumentRepresentingMultipleStringProperties()
		 internal virtual void ShouldBuildDocumentRepresentingMultipleStringProperties()
		 {
			  // given
			  string[] values = new string[]{ "hello", "world" };
			  Document document = documentRepresentingProperties( 123, values );

			  // then
			  assertEquals( "123", document.get( NODE_ID_KEY ) );
			  assertThat( document.get( string.key( 0 ) ), equalTo( values[0] ) );
			  assertThat( document.get( string.key( 1 ) ), equalTo( values[1] ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBuildDocumentRepresentingMultiplePropertiesOfDifferentTypes()
		 internal virtual void ShouldBuildDocumentRepresentingMultiplePropertiesOfDifferentTypes()
		 {
			  // given
			  object[] values = new object[]{ "hello", 789 };
			  Document document = documentRepresentingProperties( 123, values );

			  // then
			  assertEquals( "123", document.get( NODE_ID_KEY ) );
			  assertThat( document.get( string.key( 0 ) ), equalTo( "hello" ) );
			  assertThat( document.get( Number.key( 1 ) ), equalTo( "789.0" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBuildDocumentRepresentingBoolProperty()
		 internal virtual void ShouldBuildDocumentRepresentingBoolProperty()
		 {
			  // given
			  Document document = documentRepresentingProperties( ( long ) 123, true );

			  // then
			  assertEquals( "123", document.get( NODE_ID_KEY ) );
			  assertEquals( "true", document.get( Bool.key( 0 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBuildDocumentRepresentingNumberProperty()
		 internal virtual void ShouldBuildDocumentRepresentingNumberProperty()
		 {
			  // given
			  Document document = documentRepresentingProperties( ( long ) 123, 12 );

			  // then
			  assertEquals( "123", document.get( NODE_ID_KEY ) );
			  assertEquals( 12.0, document.getField( Number.key( 0 ) ).numericValue().doubleValue(), 0.001 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBuildDocumentRepresentingArrayProperty()
		 internal virtual void ShouldBuildDocumentRepresentingArrayProperty()
		 {
			  // given
			  Document document = documentRepresentingProperties((long) 123, new object[]
			  {
				  new int?[]{ 1, 2, 3 }
			  });

			  // then
			  assertEquals( "123", document.get( NODE_ID_KEY ) );
			  assertEquals( "D1.0|2.0|3.0|", document.get( Array.key( 0 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBuildQueryRepresentingBoolProperty()
		 internal virtual void ShouldBuildQueryRepresentingBoolProperty()
		 {
			  // given
			  BooleanQuery booleanQuery = ( BooleanQuery ) newSeekQuery( true );
			  ConstantScoreQuery constantScoreQuery = ( ConstantScoreQuery ) booleanQuery.clauses().get(0).Query;
			  TermQuery query = ( TermQuery ) constantScoreQuery.Query;

			  // then
			  assertEquals( "true", query.Term.text() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBuildQueryRepresentingStringProperty()
		 internal virtual void ShouldBuildQueryRepresentingStringProperty()
		 {
			  // given
			  BooleanQuery booleanQuery = ( BooleanQuery ) newSeekQuery( "Characters" );
			  ConstantScoreQuery query = ( ConstantScoreQuery ) booleanQuery.clauses().get(0).Query;

			  // then
			  assertEquals( "Characters", ( ( TermQuery ) query.Query ).Term.text() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test void shouldBuildQueryRepresentingNumberProperty()
		 internal virtual void ShouldBuildQueryRepresentingNumberProperty()
		 {
			  // given
			  BooleanQuery booleanQuery = ( BooleanQuery ) newSeekQuery( 12 );
			  ConstantScoreQuery constantScoreQuery = ( ConstantScoreQuery ) booleanQuery.clauses().get(0).Query;
			  NumericRangeQuery<double> query = ( NumericRangeQuery<double> ) constantScoreQuery.Query;

			  // then
			  assertEquals( 12.0, query.Min, 0.001 );
			  assertEquals( 12.0, query.Max,0.001 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBuildQueryRepresentingArrayProperty()
		 internal virtual void ShouldBuildQueryRepresentingArrayProperty()
		 {
			  // given
			  BooleanQuery booleanQuery = ( BooleanQuery ) newSeekQuery(new object[]
			  {
				  new int?[]{ 1, 2, 3 }
			  });
			  ConstantScoreQuery constantScoreQuery = ( ConstantScoreQuery ) booleanQuery.clauses().get(0).Query;
			  TermQuery query = ( TermQuery ) constantScoreQuery.Query;

			  // then
			  assertEquals( "D1.0|2.0|3.0|", query.Term.text() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBuildQueryRepresentingMultipleProperties()
		 internal virtual void ShouldBuildQueryRepresentingMultipleProperties()
		 {
			  // given
			  BooleanQuery booleanQuery = ( BooleanQuery ) newSeekQuery( true, "Characters", 12, new int?[]{ 1, 2, 3 } );

			  ConstantScoreQuery boolScoreQuery = ( ConstantScoreQuery ) booleanQuery.clauses().get(0).Query;
			  TermQuery boolTermQuery = ( TermQuery ) boolScoreQuery.Query;

			  ConstantScoreQuery stringScoreQuery = ( ConstantScoreQuery ) booleanQuery.clauses().get(1).Query;
			  TermQuery stringTermQuery = ( TermQuery ) stringScoreQuery.Query;

			  ConstantScoreQuery numberScoreQuery = ( ConstantScoreQuery ) booleanQuery.clauses().get(2).Query;
			  NumericRangeQuery<double> numericRangeQuery = ( NumericRangeQuery<double> ) numberScoreQuery.Query;

			  ConstantScoreQuery arrayScoreQuery = ( ConstantScoreQuery ) booleanQuery.clauses().get(3).Query;
			  TermQuery arrayTermQuery = ( TermQuery ) arrayScoreQuery.Query;

			  // then
			  assertEquals( "true", boolTermQuery.Term.text() );
			  assertEquals( "Characters", stringTermQuery.Term.text() );
			  assertEquals( 12.0, numericRangeQuery.Min, 0.001 );
			  assertEquals( 12.0, numericRangeQuery.Max, 0.001 );
			  assertEquals( "D1.0|2.0|3.0|", arrayTermQuery.Term.text() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBuildRangeSeekByNumberQueryForStrings()
		 internal virtual void ShouldBuildRangeSeekByNumberQueryForStrings()
		 {
			  // given
			  NumericRangeQuery<double> query = LuceneDocumentStructure.NewInclusiveNumericRangeSeekQuery( 12.0d, null );

			  // then
			  assertEquals( "number", query.Field );
			  assertEquals( 12.0, query.Min, 0.001 );
			  assertTrue( query.includesMin() );
			  assertNull( query.Max );
			  assertTrue( query.includesMax() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBuildRangeSeekByStringQueryForStrings()
		 internal virtual void ShouldBuildRangeSeekByStringQueryForStrings()
		 {
			  // given
			  TermRangeQuery query = ( TermRangeQuery ) LuceneDocumentStructure.NewRangeSeekByStringQuery( "foo", false, null, true );

			  // then
			  assertEquals( "string", query.Field );
			  assertEquals( "foo", query.LowerTerm.utf8ToString() );
			  assertFalse( query.includesLower() );
			  assertNull( query.UpperTerm );
			  assertTrue( query.includesUpper() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBuildWildcardQueries()
		 internal virtual void ShouldBuildWildcardQueries()
		 {
			  // given
			  WildcardQuery query = ( WildcardQuery ) LuceneDocumentStructure.NewWildCardStringQuery( "foo" );

			  // then
			  assertEquals( "string", query.Field );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBuildRangeSeekByPrefixQueryForStrings()
		 internal virtual void ShouldBuildRangeSeekByPrefixQueryForStrings()
		 {
			  // given
			  MultiTermQuery prefixQuery = ( MultiTermQuery ) LuceneDocumentStructure.NewRangeSeekByPrefixQuery( "Prefix" );

			  // then
			  assertThat( "Should contain term value", prefixQuery.ToString(), containsString("Prefix") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkFieldUsageForUniquenessVerification()
		 internal virtual void CheckFieldUsageForUniquenessVerification()
		 {
			  assertFalse( useFieldForUniquenessVerification( "id" ) );
			  assertFalse( useFieldForUniquenessVerification( "1number" ) );
			  assertTrue( useFieldForUniquenessVerification( "number" ) );
			  assertFalse( useFieldForUniquenessVerification( "1string" ) );
			  assertFalse( useFieldForUniquenessVerification( "10string" ) );
			  assertTrue( useFieldForUniquenessVerification( "string" ) );
		 }
	}

}