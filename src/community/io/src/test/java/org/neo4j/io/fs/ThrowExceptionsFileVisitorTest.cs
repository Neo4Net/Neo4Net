using System;

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
namespace Neo4Net.Io.fs
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Mock = org.mockito.Mock;
	using MockitoJUnitRunner = org.mockito.junit.MockitoJUnitRunner;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.fs.FileVisitors.throwExceptions;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(MockitoJUnitRunner.class) public class ThrowExceptionsFileVisitorTest
	public class ThrowExceptionsFileVisitorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock public java.nio.file.FileVisitor<java.nio.file.Path> wrapped;
		 public FileVisitor<Path> Wrapped;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionFromVisitFileFailed()
		 public virtual void ShouldThrowExceptionFromVisitFileFailed()
		 {
			  IOException exception = new IOException();
			  try
			  {
					throwExceptions( Wrapped ).visitFileFailed( null, exception );
					fail( "Expected exception" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, @is( exception ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionFromPostVisitDirectory()
		 public virtual void ShouldThrowExceptionFromPostVisitDirectory()
		 {
			  IOException exception = new IOException();
			  try
			  {
					throwExceptions( Wrapped ).postVisitDirectory( null, exception );
					fail( "Expected exception" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, @is( exception ) );
			  }
		 }
	}

}