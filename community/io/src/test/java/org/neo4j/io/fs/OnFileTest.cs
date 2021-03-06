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
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Mock = org.mockito.Mock;
	using MockitoJUnitRunner = org.mockito.junit.MockitoJUnitRunner;


	using Org.Neo4j.Function;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.fs.FileVisitors.onFile;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(MockitoJUnitRunner.class) public class OnFileTest
	public class OnFileTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock public org.neo4j.function.ThrowingConsumer<java.nio.file.Path, java.io.IOException> operation;
		 public ThrowingConsumer<Path, IOException> Operation;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock public java.nio.file.FileVisitor<java.nio.file.Path> wrapped;
		 public FileVisitor<Path> Wrapped;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotOperateOnDirectories() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotOperateOnDirectories()
		 {
			  Path dir = Paths.get( "/some/path" );
			  onFile( Operation, Wrapped ).preVisitDirectory( dir, null );
			  verify( Operation, never() ).accept(dir);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOperateOnFiles() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOperateOnFiles()
		 {
			  Path file = Paths.get( "/some/path" );
			  onFile( Operation, Wrapped ).visitFile( file, null );
			  verify( Operation ).accept( file );
		 }
	}

}