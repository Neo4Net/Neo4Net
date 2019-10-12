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
namespace Org.Neo4j.Values.Storable
{
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;


	using Org.Neo4j.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public sealed class AssertingStructureBuilder<Input, Result> : StructureBuilder<Input, Result>
	{
		 public static AssertingStructureBuilder<I, O> Asserting<I, O>( StructureBuilder<I, O> builder )
		 {
			  return new AssertingStructureBuilder<I, O>( builder );
		 }

		 public static Matcher<Exception> Exception( Type type, string message )
		 {
			  return exception( type, equalTo( message ) );
		 }

		 public static Matcher<Exception> Exception( Type type, Matcher<string> message )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass( type, message );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<Exception>
		 {
			 private Type _type;
			 private Matcher<string> _message;

			 public TypeSafeMatcherAnonymousInnerClass( Type type, Matcher<string> message ) : base( type )
			 {
				 this._type = type;
				 this._message = message;
			 }

			 protected internal override bool matchesSafely( Exception item )
			 {
				  return _message.matches( item.Message );
			 }

			 public override void describeTo( Description description )
			 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				  description.appendText( "Exception of type " ).appendValue( _type.FullName ).appendText( " with message " ).appendDescriptionOf( _message );
			 }
		 }

		 private readonly IDictionary<string, Input> _input = new LinkedHashMap<string, Input>();
		 private readonly StructureBuilder<Input, Result> _builder;

		 private AssertingStructureBuilder( StructureBuilder<Input, Result> builder )
		 {
			  this._builder = builder;
		 }

		 public void AssertThrows( Type type, string message )
		 {
			  AssertThrows( Exception( type, message ) );
		 }

		 public void AssertThrows( Matcher<Exception> matches )
		 {
			  try
			  {
					foreach ( KeyValuePair<string, Input> entry in _input.SetOfKeyValuePairs() )
					{
						 _builder.add( entry.Key, entry.Value );
					}
					_builder.build();
			  }
			  catch ( Exception expected )
			  {
					assertThat( expected, matches );
					return;
			  }
			  fail( "expected exception" );
		 }

		 public override AssertingStructureBuilder<Input, Result> Add( string field, Input value )
		 {
			  _input[field] = value;
			  return this;
		 }

		 public override Result Build()
		 {
			  throw new System.NotSupportedException( "do not use this method" );
		 }
	}

}