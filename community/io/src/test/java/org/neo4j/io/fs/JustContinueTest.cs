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
namespace Org.Neo4j.Io.fs
{
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;

	internal class JustContinueTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldJustContinue() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldJustContinue()
		 {
			  assertThat( FileVisitors.JustContinue().preVisitDirectory(null, null), @is(FileVisitResult.CONTINUE) );
			  assertThat( FileVisitors.JustContinue().visitFile(null, null), @is(FileVisitResult.CONTINUE) );
			  assertThat( FileVisitors.JustContinue().visitFileFailed(null, null), @is(FileVisitResult.CONTINUE) );
			  assertThat( FileVisitors.JustContinue().postVisitDirectory(null, null), @is(FileVisitResult.CONTINUE) );
		 }
	}

}