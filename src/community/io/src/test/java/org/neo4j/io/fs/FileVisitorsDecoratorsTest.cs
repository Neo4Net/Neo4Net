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
namespace Neo4Net.Io.fs
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Predicates = Neo4Net.Functions.Predicates;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.function.ThrowingConsumer.noop;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class FileVisitorsDecoratorsTest
	public class FileVisitorsDecoratorsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public String name;
		 public string Name;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public System.Func<java.nio.file.FileVisitor<java.nio.file.Path>, java.nio.file.FileVisitor<java.nio.file.Path>> decoratorConstructor;
		 public System.Func<FileVisitor<Path>, FileVisitor<Path>> DecoratorConstructor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(2) public boolean throwsExceptions;
		 public bool ThrowsExceptions;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.List<Object[]> formats()
		 public static IList<object[]> Formats()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return new IList<object[]>
			  {
				  new object[]{ "decorator", ( System.Func<FileVisitor<Path>, FileVisitor<Path>> ) FileVisitors.Decorator::new, false },
				  new object[]{ "onFile", ( System.Func<FileVisitor<Path>, FileVisitor<Path>> ) Wrapped => FileVisitors.OnFile( noop(), Wrapped ), false },
				  new object[]{ "onDirectory", ( System.Func<FileVisitor<Path>, FileVisitor<Path>> ) Wrapped => FileVisitors.OnDirectory( noop(), Wrapped ), false },
				  new object[]{ "throwExceptions", ( System.Func<FileVisitor<Path>, FileVisitor<Path>> ) FileVisitors.throwExceptions, true },
				  new object[]{ "onlyMatching", ( System.Func<FileVisitor<Path>, FileVisitor<Path>> ) Wrapped => FileVisitors.OnlyMatching( Predicates.alwaysTrue(), Wrapped ), false }
			  };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public java.nio.file.FileVisitor<java.nio.file.Path> wrapped = mock(java.nio.file.FileVisitor.class);
		 public FileVisitor<Path> Wrapped = mock( typeof( FileVisitor ) );
		 public FileVisitor<Path> Decorator;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  Decorator = DecoratorConstructor.apply( Wrapped );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDelegatePreVisitDirectory() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDelegatePreVisitDirectory()
		 {
			  Path dir = Paths.get( "some-dir" );
			  BasicFileAttributes attrs = mock( typeof( BasicFileAttributes ) );
			  Decorator.preVisitDirectory( dir, attrs );
			  verify( Wrapped ).preVisitDirectory( dir, attrs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPropagateReturnValueFromPreVisitDirectory() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPropagateReturnValueFromPreVisitDirectory()
		 {
			  foreach ( FileVisitResult result in FileVisitResult.values() )
			  {
					when( Wrapped.preVisitDirectory( any(), any() ) ).thenReturn(result);
					assertThat( Decorator.preVisitDirectory( null, null ), @is( result ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPropagateExceptionsFromPreVisitDirectory() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPropagateExceptionsFromPreVisitDirectory()
		 {
			  when( Wrapped.preVisitDirectory( any(), any() ) ).thenThrow(new IOException());

			  try
			  {
					Decorator.preVisitDirectory( null, null );
					fail( "expected exception" );
			  }
			  catch ( IOException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDelegatePostVisitDirectory() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDelegatePostVisitDirectory()
		 {
			  Path dir = Paths.get( "some-dir" );
			  IOException e = ThrowsExceptions ? null : new IOException();
			  Decorator.postVisitDirectory( dir, e );
			  verify( Wrapped ).postVisitDirectory( dir, e );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPropagateReturnValueFromPostVisitDirectory() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPropagateReturnValueFromPostVisitDirectory()
		 {
			  foreach ( FileVisitResult result in FileVisitResult.values() )
			  {
					when( Wrapped.postVisitDirectory( any(), any() ) ).thenReturn(result);
					assertThat( Decorator.postVisitDirectory( null, null ), @is( result ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPropagateExceptionsFromPostVisitDirectory() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPropagateExceptionsFromPostVisitDirectory()
		 {
			  when( Wrapped.postVisitDirectory( any(), any() ) ).thenThrow(new IOException());

			  try
			  {
					Decorator.postVisitDirectory( null, null );
					fail( "expected exception" );
			  }
			  catch ( IOException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDelegateVisitFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDelegateVisitFile()
		 {
			  Path dir = Paths.get( "some-dir" );
			  BasicFileAttributes attrs = mock( typeof( BasicFileAttributes ) );
			  Decorator.visitFile( dir, attrs );
			  verify( Wrapped ).visitFile( dir, attrs );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPropagateReturnValueFromVisitFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPropagateReturnValueFromVisitFile()
		 {
			  foreach ( FileVisitResult result in FileVisitResult.values() )
			  {
					when( Wrapped.visitFile( any(), any() ) ).thenReturn(result);
					assertThat( Decorator.visitFile( null, null ), @is( result ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPropagateExceptionsFromVisitFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPropagateExceptionsFromVisitFile()
		 {
			  when( Wrapped.visitFile( any(), any() ) ).thenThrow(new IOException());

			  try
			  {
					Decorator.visitFile( null, null );
					fail( "expected exception" );
			  }
			  catch ( IOException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDelegateVisitFileFailed() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDelegateVisitFileFailed()
		 {
			  Path dir = Paths.get( "some-dir" );
			  IOException e = ThrowsExceptions ? null : new IOException();
			  Decorator.visitFileFailed( dir, e );
			  verify( Wrapped ).visitFileFailed( dir, e );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPropagateReturnValueFromVisitFileFailed() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPropagateReturnValueFromVisitFileFailed()
		 {
			  foreach ( FileVisitResult result in FileVisitResult.values() )
			  {
					when( Wrapped.visitFileFailed( any(), any() ) ).thenReturn(result);
					assertThat( Decorator.visitFileFailed( null, null ), @is( result ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPropagateExceptionsFromVisitFileFailed() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPropagateExceptionsFromVisitFileFailed()
		 {
			  when( Wrapped.visitFileFailed( any(), any() ) ).thenThrow(new IOException());

			  try
			  {
					Decorator.visitFileFailed( null, null );
					fail( "expected exception" );
			  }
			  catch ( IOException )
			  {
			  }
		 }
	}

}