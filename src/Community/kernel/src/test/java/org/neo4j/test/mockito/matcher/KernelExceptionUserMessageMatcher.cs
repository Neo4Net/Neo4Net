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
namespace Neo4Net.Test.mockito.matcher
{
	using BaseMatcher = org.hamcrest.BaseMatcher;
	using Description = org.hamcrest.Description;

	using TokenNameLookup = Neo4Net.@internal.Kernel.Api.TokenNameLookup;
	using SchemaKernelException = Neo4Net.@internal.Kernel.Api.exceptions.schema.SchemaKernelException;

	public class KernelExceptionUserMessageMatcher<T> : BaseMatcher<T> where T : Neo4Net.@internal.Kernel.Api.exceptions.schema.SchemaKernelException
	{
		 private TokenNameLookup _tokenNameLookup;
		 private string _expectedMessage;
		 private string _actualMessage;

		 public KernelExceptionUserMessageMatcher( TokenNameLookup tokenNameLookup, string expectedMessage )
		 {
			  this._tokenNameLookup = tokenNameLookup;
			  this._expectedMessage = expectedMessage;
		 }

		 public override bool Matches( object item )
		 {
			  if ( item is SchemaKernelException )
			  {
					_actualMessage = ( ( SchemaKernelException ) item ).getUserMessage( _tokenNameLookup );
					return _expectedMessage.Equals( _actualMessage );
			  }
			  return false;
		 }

		 public override void DescribeTo( Description description )
		 {
			  description.appendText( " expected message: '" ).appendText( _expectedMessage ).appendText( "', but was: '" ).appendText( _actualMessage ).appendText( "'" );
		 }
	}

}