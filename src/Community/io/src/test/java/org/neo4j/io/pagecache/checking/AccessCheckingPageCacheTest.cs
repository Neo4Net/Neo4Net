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
namespace Neo4Net.Io.pagecache.checking
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;



//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class AccessCheckingPageCacheTest
	{
		 private PageCache _pageCache;
		 private PageCursor _cursor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void getPageCursor() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void getPageCursor()
		 {
			  PageCache mockedPageCache = mock( typeof( PageCache ) );
			  PagedFile mockedPagedFile = mock( typeof( PagedFile ) );
			  PageCursor mockedCursor = mock( typeof( PageCursor ) );
			  when( mockedPagedFile.Io( anyLong(), anyInt() ) ).thenReturn(mockedCursor);
			  when( mockedPageCache.Map( any( typeof( File ) ), anyInt(), any() ) ).thenReturn(mockedPagedFile);
			  _pageCache = new AccessCheckingPageCache( mockedPageCache );
			  PagedFile file = _pageCache.map( new File( "some file" ), 512 );
			  _cursor = file.Io( 0, Neo4Net.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGrant_read_shouldRetry_close() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldGrantReadShouldRetryClose()
		 {
			  // GIVEN
			  _cursor.Byte;

			  // WHEN
			  _cursor.shouldRetry();

			  // THEN
			  _cursor.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGrant_read_shouldRetry_next() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldGrantReadShouldRetryNext()
		 {
			  // GIVEN
			  _cursor.getByte( 0 );

			  // WHEN
			  _cursor.shouldRetry();

			  // THEN
			  _cursor.next();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGrant_read_shouldRetry_next_with_id() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldGrantReadShouldRetryNextWithId()
		 {
			  // GIVEN
			  _cursor.Short;

			  // WHEN
			  _cursor.shouldRetry();

			  // THEN
			  _cursor.next( 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGrant_read_shouldRetry_read_shouldRetry_close() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldGrantReadShouldRetryReadShouldRetryClose()
		 {
			  // GIVEN
			  _cursor.getShort( 0 );
			  _cursor.shouldRetry();
			  _cursor.Int;

			  // WHEN
			  _cursor.shouldRetry();

			  // THEN
			  _cursor.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGrant_read_shouldRetry_read_shouldRetry_next() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldGrantReadShouldRetryReadShouldRetryNext()
		 {
			  // GIVEN
			  _cursor.getInt( 0 );
			  _cursor.shouldRetry();
			  _cursor.Long;

			  // WHEN
			  _cursor.shouldRetry();

			  // THEN
			  _cursor.next();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGrant_read_shouldRetry_read_shouldRetry_next_with_id() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldGrantReadShouldRetryReadShouldRetryNextWithId()
		 {
			  // GIVEN
			  _cursor.getLong( 0 );
			  _cursor.shouldRetry();
			  _cursor.getBytes( new sbyte[2] );

			  // WHEN
			  _cursor.shouldRetry();

			  // THEN
			  _cursor.next( 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFail_read_close()
		 internal virtual void ShouldFailReadClose()
		 {
			  // GIVEN
			  _cursor.Byte;

			  try
			  {
					// WHEN
					_cursor.close();
					fail( "Should have failed" );
			  }
			  catch ( AssertionError e )
			  {
					// THEN good
					assertThat( e.Message, containsString( "shouldRetry" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFail_read_next() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldFailReadNext()
		 {
			  // GIVEN
			  _cursor.getByte( 0 );

			  try
			  {
					// WHEN
					_cursor.next();
					fail( "Should have failed" );
			  }
			  catch ( AssertionError e )
			  {
					// THEN good
					assertThat( e.Message, containsString( "shouldRetry" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFail_read_next_with_id() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldFailReadNextWithId()
		 {
			  // GIVEN
			  _cursor.Short;

			  try
			  {
					// WHEN
					_cursor.next( 1 );
					fail( "Should have failed" );
			  }
			  catch ( AssertionError e )
			  {
					// THEN good
					assertThat( e.Message, containsString( "shouldRetry" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFail_read_shouldRetry_read_close() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldFailReadShouldRetryReadClose()
		 {
			  // GIVEN
			  _cursor.getShort( 0 );
			  _cursor.shouldRetry();
			  _cursor.Int;

			  try
			  {
					// WHEN
					_cursor.close();
					fail( "Should have failed" );
			  }
			  catch ( AssertionError e )
			  {
					// THEN good
					assertThat( e.Message, containsString( "shouldRetry" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFail_read_shouldRetry_read_next() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldFailReadShouldRetryReadNext()
		 {
			  // GIVEN
			  _cursor.getInt( 0 );
			  _cursor.shouldRetry();
			  _cursor.Long;

			  try
			  {
					// WHEN
					_cursor.next();
					fail( "Should have failed" );
			  }
			  catch ( AssertionError e )
			  {
					// THEN good
					assertThat( e.Message, containsString( "shouldRetry" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFail_read_shouldRetry_read_next_with_id() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldFailReadShouldRetryReadNextWithId()
		 {
			  // GIVEN
			  _cursor.getLong( 0 );
			  _cursor.shouldRetry();
			  _cursor.getBytes( new sbyte[2] );

			  try
			  {
					// WHEN
					_cursor.next( 1 );
					fail( "Should have failed" );
			  }
			  catch ( AssertionError e )
			  {
					// THEN good
					assertThat( e.Message, containsString( "shouldRetry" ) );
			  }
		 }
	}

}