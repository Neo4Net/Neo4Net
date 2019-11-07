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
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.function.Predicates.alwaysFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.fs.FileVisitors.onlyMatching;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(MockitoJUnitRunner.class) public class OnlyMatchingFileVisitorTest
	public class OnlyMatchingFileVisitorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock public java.nio.file.FileVisitor<java.nio.file.Path> wrapped;
		 public FileVisitor<Path> Wrapped;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDelegatePreVisitDirectoryIfPredicateDoesntMatch() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDelegatePreVisitDirectoryIfPredicateDoesntMatch()
		 {
			  onlyMatching( alwaysFalse(), Wrapped ).preVisitDirectory(null, null);
			  verify( Wrapped, never() ).preVisitDirectory(any(), any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDelegatePostVisitDirectoryIfPredicateDoesntMatch() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDelegatePostVisitDirectoryIfPredicateDoesntMatch()
		 {
			  onlyMatching( alwaysFalse(), Wrapped ).postVisitDirectory(null, null);
			  verify( Wrapped, never() ).postVisitDirectory(any(), any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDelegateVisitFileIfPredicateDoesntMatch() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDelegateVisitFileIfPredicateDoesntMatch()
		 {
			  onlyMatching( alwaysFalse(), Wrapped ).visitFile(null, null);
			  verify( Wrapped, never() ).visitFile(any(), any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDelegateVisitFileFailedIfPredicateDoesntMatch() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDelegateVisitFileFailedIfPredicateDoesntMatch()
		 {
			  onlyMatching( alwaysFalse(), Wrapped ).visitFileFailed(null, null);
			  verify( Wrapped, never() ).visitFileFailed(any(), any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSkipSubtreeFromPreVisitDirectoryIfPredicateDoesntMatch() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSkipSubtreeFromPreVisitDirectoryIfPredicateDoesntMatch()
		 {
			  assertThat( onlyMatching( alwaysFalse(), Wrapped ).preVisitDirectory(null, null), @is(FileVisitResult.SKIP_SUBTREE) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldContinueAfterPostVisitDirectoryIfPredicateDoesntMatch() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldContinueAfterPostVisitDirectoryIfPredicateDoesntMatch()
		 {
			  assertThat( onlyMatching( alwaysFalse(), Wrapped ).postVisitDirectory(null, null), @is(FileVisitResult.CONTINUE) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldContinueAfterVisitFileIfPredicateDoesntMatch() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldContinueAfterVisitFileIfPredicateDoesntMatch()
		 {
			  assertThat( onlyMatching( alwaysFalse(), Wrapped ).visitFile(null, null), @is(FileVisitResult.CONTINUE) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldContinueAfterVisitFileFailedIfPredicateDoesntMatch() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldContinueAfterVisitFileFailedIfPredicateDoesntMatch()
		 {
			  assertThat( onlyMatching( alwaysFalse(), Wrapped ).visitFileFailed(null, null), @is(FileVisitResult.CONTINUE) );
		 }
	}

}