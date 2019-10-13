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
namespace Neo4Net.Bolt.v1.runtime.bookmarking
{
	using Test = org.junit.jupiter.api.Test;

	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using AnyValue = Neo4Net.Values.AnyValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using MapValueBuilder = Neo4Net.Values.@virtual.MapValueBuilder;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class BookmarkTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFormatAndParseSingleBookmarkContainingTransactionId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldFormatAndParseSingleBookmarkContainingTransactionId()
		 {
			  // given
			  long txId = 1234;
			  MapValue @params = SingletonMap( "bookmark", ( new Bookmark( txId ) ).ToString() );

			  // when
			  Bookmark bookmark = Bookmark.FromParamsOrNull( @params );

			  // then
			  assertEquals( txId, bookmark.TxId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFormatAndParseMultipleBookmarksContainingTransactionId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldFormatAndParseMultipleBookmarksContainingTransactionId()
		 {
			  // given
			  long txId1 = 1234;
			  long txId2 = 12345;
			  MapValue @params = SingletonMap( "bookmarks", asList( ( new Bookmark( txId1 ) ).ToString(), (new Bookmark(txId2)).ToString() ) );

			  // when
			  Bookmark bookmark = Bookmark.FromParamsOrNull( @params );

			  // then
			  assertEquals( txId2, bookmark.TxId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseAndFormatSingleBookmarkContainingTransactionId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldParseAndFormatSingleBookmarkContainingTransactionId()
		 {
			  // given
			  string expected = "neo4j:bookmark:v1:tx1234";
			  MapValue @params = SingletonMap( "bookmark", expected );

			  // when
			  string actual = ( new Bookmark( Bookmark.FromParamsOrNull( @params ).txId() ) ).ToString();

			  // then
			  assertEquals( expected, actual );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldParseAndFormatMultipleBookmarkContainingTransactionId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldParseAndFormatMultipleBookmarkContainingTransactionId()
		 {
			  // given
			  string txId1 = "neo4j:bookmark:v1:tx1234";
			  string txId2 = "neo4j:bookmark:v1:tx12345";
			  MapValue @params = SingletonMap( "bookmarks", asList( txId1, txId2 ) );

			  // when
			  string actual = ( new Bookmark( Bookmark.FromParamsOrNull( @params ).txId() ) ).ToString();

			  // then
			  assertEquals( txId2, actual );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailWhenParsingBadlyFormattedSingleBookmark()
		 internal virtual void ShouldFailWhenParsingBadlyFormattedSingleBookmark()
		 {
			  string bookmarkString = "neo4q:markbook:v9:xt998";

			  BookmarkFormatException e = assertThrows( typeof( BookmarkFormatException ), () => Bookmark.FromParamsOrNull(SingletonMap("bookmark", bookmarkString)) );

			  assertTrue( e.CausesFailureMessage() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailWhenParsingBadlyFormattedMultipleBookmarks()
		 internal virtual void ShouldFailWhenParsingBadlyFormattedMultipleBookmarks()
		 {
			  string bookmarkString = "neo4j:bookmark:v1:tx998";
			  string wrongBookmarkString = "neo4q:markbook:v9:xt998";

			  BookmarkFormatException e = assertThrows( typeof( BookmarkFormatException ), () => Bookmark.FromParamsOrNull(SingletonMap("bookmarks", asList(bookmarkString, wrongBookmarkString))) );

			  assertTrue( e.CausesFailureMessage() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailWhenNoNumberFollowsThePrefixInSingleBookmark()
		 internal virtual void ShouldFailWhenNoNumberFollowsThePrefixInSingleBookmark()
		 {
			  string bookmarkString = "neo4j:bookmark:v1:tx";

			  BookmarkFormatException e = assertThrows( typeof( BookmarkFormatException ), () => Bookmark.FromParamsOrNull(SingletonMap("bookmark", bookmarkString)) );

			  assertTrue( e.CausesFailureMessage() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailWhenNoNumberFollowsThePrefixInMultipleBookmarks()
		 internal virtual void ShouldFailWhenNoNumberFollowsThePrefixInMultipleBookmarks()
		 {
			  string bookmarkString = "neo4j:bookmark:v1:tx10";
			  string wrongBookmarkString = "neo4j:bookmark:v1:tx";

			  BookmarkFormatException e = assertThrows( typeof( BookmarkFormatException ), () => Bookmark.FromParamsOrNull(SingletonMap("bookmarks", asList(bookmarkString, wrongBookmarkString))) );

			  assertTrue( e.CausesFailureMessage() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailWhenSingleBookmarkHasExtraneousTrailingCharacters()
		 internal virtual void ShouldFailWhenSingleBookmarkHasExtraneousTrailingCharacters()
		 {
			  string bookmarkString = "neo4j:bookmark:v1:tx1234supercalifragilisticexpialidocious";

			  BookmarkFormatException e = assertThrows( typeof( BookmarkFormatException ), () => Bookmark.FromParamsOrNull(SingletonMap("bookmark", bookmarkString)) );

			  assertTrue( e.CausesFailureMessage() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailWhenMultipleBookmarksHaveExtraneousTrailingCharacters()
		 internal virtual void ShouldFailWhenMultipleBookmarksHaveExtraneousTrailingCharacters()
		 {
			  string bookmarkString = "neo4j:bookmark:v1:tx1234";
			  string wrongBookmarkString = "neo4j:bookmark:v1:tx1234supercalifragilisticexpialidocious";

			  BookmarkFormatException e = assertThrows( typeof( BookmarkFormatException ), () => Bookmark.FromParamsOrNull(SingletonMap("bookmarks", asList(bookmarkString, wrongBookmarkString))) );

			  assertTrue( e.CausesFailureMessage() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUseMultipleBookmarksWhenGivenBothSingleAndMultiple() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldUseMultipleBookmarksWhenGivenBothSingleAndMultiple()
		 {
			  MapValue @params = @params( "neo4j:bookmark:v1:tx42", asList( "neo4j:bookmark:v1:tx10", "neo4j:bookmark:v1:tx99", "neo4j:bookmark:v1:tx3" ) );

			  Bookmark bookmark = Bookmark.FromParamsOrNull( @params );

			  assertEquals( 99, bookmark.TxId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUseMultipleBookmarksWhenGivenOnlyMultiple() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldUseMultipleBookmarksWhenGivenOnlyMultiple()
		 {
			  MapValue @params = @params( null, asList( "neo4j:bookmark:v1:tx85", "neo4j:bookmark:v1:tx47", "neo4j:bookmark:v1:tx15", "neo4j:bookmark:v1:tx6" ) );

			  Bookmark bookmark = Bookmark.FromParamsOrNull( @params );

			  assertEquals( 85, bookmark.TxId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUseSingleBookmarkWhenGivenOnlySingle() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldUseSingleBookmarkWhenGivenOnlySingle()
		 {
			  MapValue @params = @params( "neo4j:bookmark:v1:tx82", null );

			  Bookmark bookmark = Bookmark.FromParamsOrNull( @params );

			  assertEquals( 82, bookmark.TxId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUseSingleBookmarkWhenGivenBothSingleAndNullAsMultiple() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldUseSingleBookmarkWhenGivenBothSingleAndNullAsMultiple()
		 {
			  MapValue @params = @params( "neo4j:bookmark:v1:tx58", null );

			  Bookmark bookmark = Bookmark.FromParamsOrNull( @params );

			  assertEquals( 58, bookmark.TxId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUseSingleBookmarkWhenGivenBothSingleAndEmptyListAsMultiple() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldUseSingleBookmarkWhenGivenBothSingleAndEmptyListAsMultiple()
		 {
			  MapValue @params = @params( "neo4j:bookmark:v1:tx67", emptyList() );

			  Bookmark bookmark = Bookmark.FromParamsOrNull( @params );

			  assertEquals( 67, bookmark.TxId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowWhenMultipleBookmarksIsNotAList()
		 internal virtual void ShouldThrowWhenMultipleBookmarksIsNotAList()
		 {
			  MapValue @params = @params( "neo4j:bookmark:v1:tx67", new string[]{ "neo4j:bookmark:v1:tx68" } );

			  BookmarkFormatException e = assertThrows( typeof( BookmarkFormatException ), () => Bookmark.FromParamsOrNull(@params) );

			  assertTrue( e.CausesFailureMessage() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowWhenMultipleBookmarksIsNotAListOfStrings()
		 internal virtual void ShouldThrowWhenMultipleBookmarksIsNotAListOfStrings()
		 {
			  MapValue @params = @params( "neo4j:bookmark:v1:tx67", asList( new string[]{ "neo4j:bookmark:v1:tx50" }, new object[]{ "neo4j:bookmark:v1:tx89" } ) );

			  BookmarkFormatException e = assertThrows( typeof( BookmarkFormatException ), () => Bookmark.FromParamsOrNull(@params) );

			  assertTrue( e.CausesFailureMessage() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowWhenOneOfMultipleBookmarksIsMalformed()
		 internal virtual void ShouldThrowWhenOneOfMultipleBookmarksIsMalformed()
		 {
			  MapValue @params = @params( "neo4j:bookmark:v1:tx67", asList( "neo4j:bookmark:v1:tx99", "neo4j:bookmark:v1:tx12", "neo4j:bookmark:www:tx99" ) );

			  BookmarkFormatException e = assertThrows( typeof( BookmarkFormatException ), () => Bookmark.FromParamsOrNull(@params) );

			  assertTrue( e.CausesFailureMessage() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowWhenSingleBookmarkIsMalformed()
		 internal virtual void ShouldThrowWhenSingleBookmarkIsMalformed()
		 {
			  MapValue @params = @params( "neo4j:strange-bookmark:v1:tx6", null );

			  BookmarkFormatException e = assertThrows( typeof( BookmarkFormatException ), () => Bookmark.FromParamsOrNull(@params) );

			  assertTrue( e.CausesFailureMessage() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnNullWhenNoBookmarks() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReturnNullWhenNoBookmarks()
		 {
			  assertNull( Bookmark.FromParamsOrNull( VirtualValues.EMPTY_MAP ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnNullWhenGivenEmptyListForMultipleBookmarks() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReturnNullWhenGivenEmptyListForMultipleBookmarks()
		 {
			  MapValue @params = @params( null, emptyList() );
			  assertNull( Bookmark.FromParamsOrNull( @params ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSkipNullsInMultipleBookmarks() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldSkipNullsInMultipleBookmarks()
		 {
			  MapValue @params = @params( null, asList( "neo4j:bookmark:v1:tx3", "neo4j:bookmark:v1:tx5", null, "neo4j:bookmark:v1:tx17" ) );

			  Bookmark bookmark = Bookmark.FromParamsOrNull( @params );

			  assertEquals( 17, bookmark.TxId() );
		 }

		 private static MapValue Params( string bookmark, object bookmarks )
		 {
			  MapValueBuilder builder = new MapValueBuilder();
			  if ( !string.ReferenceEquals( bookmark, null ) )
			  {
					builder.Add( "bookmark", ValueUtils.of( bookmark ) );
			  }
			  builder.Add( "bookmarks", ValueUtils.of( bookmarks ) );
			  return builder.Build();
		 }

		 private static MapValue SingletonMap( string key, object value )
		 {
			  return VirtualValues.map( new string[]{ key }, new AnyValue[]{ ValueUtils.of( value ) } );
		 }
	}

}