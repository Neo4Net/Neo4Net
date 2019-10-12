using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Test.matchers
{
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;


	using Exceptions = Org.Neo4j.Helpers.Exceptions;

	public sealed class CommonMatchers
	{
		 private CommonMatchers()
		 {
		 }

		 /// <summary>
		 /// Checks that an iterable of T contains items that each match one and only one of the provided matchers and
		 /// and that each matchers matches exactly one and only one of the items from the iterable.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static <T> org.hamcrest.Matcher<? super Iterable<T>> matchesOneToOneInAnyOrder(org.hamcrest.Matcher<? super T>... expectedMatchers)
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<object> MatchesOneToOneInAnyOrder<T>( params Matcher<object>[] expectedMatchers )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return new MatchesOneToOneInAnyOrder<>(expectedMatchers);
			  return new MatchesOneToOneInAnyOrder<object>( expectedMatchers );
		 }

		 /// <summary>
		 /// Checks that an exception message matches given matcher
		 /// </summary>
		 /// <param name="matcher">
		 /// @return </param>
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static org.hamcrest.Matcher<Throwable> matchesExceptionMessage(org.hamcrest.Matcher<? super String> matcher)
		 public static Matcher<Exception> MatchesExceptionMessage<T1>( Matcher<T1> matcher )
		 {
			  return new ExceptionMessageMatcher( matcher );
		 }

		 /// <summary>
		 /// Checks that exception has expected array or suppressed exceptions.
		 /// </summary>
		 /// <param name="expectedSuppressedErrors"> expected suppressed exceptions. </param>
		 /// <returns> new matcher. </returns>
		 public static Matcher<Exception> HasSuppressed( params Exception[] expectedSuppressedErrors )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass( expectedSuppressedErrors );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<Exception>
		 {
			 private Exception[] _expectedSuppressedErrors;

			 public TypeSafeMatcherAnonymousInnerClass( Exception[] expectedSuppressedErrors )
			 {
				 this._expectedSuppressedErrors = expectedSuppressedErrors;
			 }

			 protected internal override bool matchesSafely( Exception item )
			 {
				  return Arrays.Equals( item.Suppressed, _expectedSuppressedErrors );
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "a throwable with suppressed:\n" );

				  if ( _expectedSuppressedErrors.Length == 0 )
				  {
						description.appendText( "a throwable without suppressed" );
				  }
				  else
				  {
						description.appendText( "a throwable with suppressed:\n" );

//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
						string expectedSuppressedAsString = java.util.expectedSuppressedErrors.Select( Exceptions.stringify ).collect( joining( "\n", "[\n", "]" ) );

						description.appendText( expectedSuppressedAsString );
				  }
			 }
		 }

		 private class MatchesOneToOneInAnyOrder<T> : TypeSafeMatcher<IEnumerable<T>>
		 {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private final org.hamcrest.Matcher<? super T>[] expectedMatchers;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  internal readonly Matcher<object>[] ExpectedMatchers;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs MatchesOneToOneInAnyOrder(org.hamcrest.Matcher<? super T>... expectedMatchers)
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  internal MatchesOneToOneInAnyOrder( params Matcher<object>[] expectedMatchers )
			  {
					this.ExpectedMatchers = expectedMatchers;
			  }

			  protected internal override bool MatchesSafely( IEnumerable<T> items )
			  {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: java.util.Set<org.hamcrest.Matcher<? super T>> matchers = uniqueMatchers();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
					ISet<Matcher> matchers = UniqueMatchers();

					foreach ( T item in items )
					{
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: org.hamcrest.Matcher<? super T> matcherFound = null;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
						 Matcher<object> matcherFound = null;
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: for (org.hamcrest.Matcher<? super T> matcherConsidered : matchers)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
						 foreach ( Matcher<object> matcherConsidered in matchers )
						 {
							  if ( matcherConsidered.matches( item ) )
							  {
									if ( matcherFound == null )
									{
										 matcherFound = matcherConsidered;
									}
									else
									{
										 return false;
									}
							  }
						 }
						 if ( matcherFound == null )
						 {
							  return false;
						 }
						 else
						 {
							  matchers.remove( matcherFound );
						 }
					}

					return matchers.Count == 0;
			  }

			  public override void DescribeTo( Description description )
			  {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: java.util.Set<org.hamcrest.Matcher<? super T>> matchers = uniqueMatchers();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
					ISet<Matcher> matchers = UniqueMatchers();

					description.appendText( "items that each match exactly one of " );
					description.appendList( "{ ", ", ", " }", matchers );
					description.appendText( " and exactly as many items as matchers" );
			  }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private java.util.Set<org.hamcrest.Matcher<? super T>> uniqueMatchers()
			  internal virtual ISet<Matcher> UniqueMatchers()
			  {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: java.util.Set<org.hamcrest.Matcher<? super T>> matchers = new java.util.HashSet<>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
					ISet<Matcher> matchers = new HashSet<Matcher>();
					Collections.addAll( matchers, ExpectedMatchers );
					return matchers;
			  }
		 }
	}

}