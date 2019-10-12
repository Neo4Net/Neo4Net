using System;

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
namespace Neo4Net.Test.matchers
{
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;

	public class ExceptionMessageMatcher : TypeSafeMatcher<Exception>
	{
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private final org.hamcrest.Matcher<? super String> matcher;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private readonly Matcher<object> _matcher;

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public ExceptionMessageMatcher(org.hamcrest.Matcher<? super String> matcher)
		 public ExceptionMessageMatcher<T1>( Matcher<T1> matcher )
		 {
			  this._matcher = matcher;
		 }

		 protected internal override bool MatchesSafely( Exception throwable )
		 {
			  return _matcher.matches( throwable.Message );
		 }

		 public override void DescribeTo( Description description )
		 {
			  description.appendText( "expect message to be " ).appendDescriptionOf( _matcher );
		 }

	}

}