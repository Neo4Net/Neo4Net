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
namespace Neo4Net.Test.mockito.matcher
{
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using ExceptionUtils = org.apache.commons.lang3.exception.ExceptionUtils;
	using Description = org.hamcrest.Description;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;

	public class RootCauseMatcher<T> : TypeSafeMatcher<T> where T : Exception
	{
		 private readonly Type<T> _rootCause;
		 private readonly string _message;
		 private Exception _cause;

		 public RootCauseMatcher( Type rootCause )
		 {
				 rootCause = typeof( T );
			  this( rootCause, StringUtils.EMPTY );
		 }

		 public RootCauseMatcher( Type rootCause, string message )
		 {
				 rootCause = typeof( T );
			  this._rootCause = rootCause;
			  this._message = message;
		 }

		 protected internal override bool MatchesSafely( T item )
		 {
			  _cause = ExceptionUtils.getRootCause( item );
			  return _rootCause.IsInstanceOfType( _cause ) && _cause.Message.StartsWith( _message );
		 }

		 public override void DescribeTo( Description description )
		 {
			  description.appendText( "Expected root cause of " ).appendValue( _rootCause ).appendText( " with message: " ).appendValue( _message ).appendText( ", but " );
			  if ( _cause != null )
			  {
					description.appendText( "was: " ).appendValue( _cause.GetType() ).appendText(" with message: ").appendValue(_cause.Message);
			  }
			  else
			  {
					description.appendText( "actual exception was never thrown." );
			  }
		 }
	}

}