using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.backup
{
	using Description = org.hamcrest.Description;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;


	public class ExceptionMatchers
	{
		 public static TypeSafeMatcher<Exception> ExceptionContainsSuppressedThrowable( Exception expectedSuppressed )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass( expectedSuppressed );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<Exception>
		 {
			 private Exception _expectedSuppressed;

			 public TypeSafeMatcherAnonymousInnerClass( Exception expectedSuppressed )
			 {
				 this._expectedSuppressed = expectedSuppressed;
			 }

			 protected internal override bool matchesSafely( Exception item )
			 {
				  IList<Exception> suppress = Arrays.asList( item.Suppressed );
				  return suppress.Contains( _expectedSuppressed );
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "contains suppressed exception " ).appendValue( _expectedSuppressed );
			 }

			 protected internal override void describeMismatchSafely( Exception item, Description mismatchDescription )
			 {
				  IList<string> suppressedExceptionStrings = Stream.of( item.Suppressed ).map( ExceptionMatchers.exceptionWithMessageToString ).collect( Collectors.toList() );
				  mismatchDescription.appendText( "exception " ).appendValue( item ).appendText( " with suppressed " ).appendValueList( "[", ", ", "]", suppressedExceptionStrings ).appendText( " does not contain " ).appendValue( ExceptionWithMessageToString( _expectedSuppressed ) );
			 }
		 }

		 private static string ExceptionWithMessageToString( Exception throwable )
		 {
			  return format( "<%s:%s>", throwable.GetType(), throwable.Message );
		 }
	}

}