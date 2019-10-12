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
namespace Org.Neo4j.Adversaries.pagecache
{
	using Test = org.junit.jupiter.api.Test;

	using ByteArrayPageCursor = Org.Neo4j.Io.pagecache.ByteArrayPageCursor;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class AdversarialReadPageCursorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotMessUpUnrelatedSegmentOnReadBytes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotMessUpUnrelatedSegmentOnReadBytes()
		 {
			  // Given
			  sbyte[] buf = new sbyte[4];
			  sbyte[] page = new sbyte[]{ 7 };
			  AdversarialReadPageCursor cursor = new AdversarialReadPageCursor( new ByteArrayPageCursor( page ), new PageCacheRule.AtomicBooleanInconsistentReadAdversary( new AtomicBoolean( true ) ) );

			  // When
			  cursor.Next( 0 );
			  cursor.GetBytes( buf, buf.Length - 1, 1 );
			  cursor.ShouldRetry();
			  cursor.GetBytes( buf, buf.Length - 1, 1 );

			  // Then the range outside of buf.length-1, buf.length should be pristine
			  assertEquals( 0, buf[0] );
			  assertEquals( 0, buf[1] );
			  assertEquals( 0, buf[2] );
		 }
	}

}