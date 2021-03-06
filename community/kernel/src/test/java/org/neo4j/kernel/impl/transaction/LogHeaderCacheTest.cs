﻿/*
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
namespace Org.Neo4j.Kernel.impl.transaction
{
	using Test = org.junit.Test;

	using LogHeaderCache = Org.Neo4j.Kernel.impl.transaction.log.LogHeaderCache;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;

	public class LogHeaderCacheTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNullWhenThereIsNoHeaderInTheCache()
		 public virtual void ShouldReturnNullWhenThereIsNoHeaderInTheCache()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.LogHeaderCache cache = new org.neo4j.kernel.impl.transaction.log.LogHeaderCache(2);
			  LogHeaderCache cache = new LogHeaderCache( 2 );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final System.Nullable<long> logHeader = cache.getLogHeader(5);
			  long? logHeader = cache.GetLogHeader( 5 );

			  // then
			  assertNull( logHeader );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnTheHeaderIfInTheCache()
		 public virtual void ShouldReturnTheHeaderIfInTheCache()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.LogHeaderCache cache = new org.neo4j.kernel.impl.transaction.log.LogHeaderCache(2);
			  LogHeaderCache cache = new LogHeaderCache( 2 );

			  // when
			  cache.PutHeader( 5, 3 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long logHeader = cache.getLogHeader(5);
			  long logHeader = cache.GetLogHeader( 5 ).Value;

			  // then
			  assertEquals( 3, logHeader );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldClearTheCache()
		 public virtual void ShouldClearTheCache()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.log.LogHeaderCache cache = new org.neo4j.kernel.impl.transaction.log.LogHeaderCache(2);
			  LogHeaderCache cache = new LogHeaderCache( 2 );

			  // when
			  cache.PutHeader( 5, 3 );
			  cache.Clear();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final System.Nullable<long> logHeader = cache.getLogHeader(5);
			  long? logHeader = cache.GetLogHeader( 5 );

			  // then
			  assertNull( logHeader );
		 }
	}

}