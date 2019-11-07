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
namespace Neo4Net.Bolt.v1.runtime.spi
{
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;

	using QueryResult = Neo4Net.Cypher.result.QueryResult;
	using AnyValue = Neo4Net.Values.AnyValue;
	using NumberValue = Neo4Net.Values.Storable.NumberValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;

	public class StreamMatchers
	{
		 private StreamMatchers()
		 {
		 }

		 public static Matcher<AnyValue> GreaterThanOrEqualTo( long input )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass( input );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<AnyValue>
		 {
			 private long _input;

			 public TypeSafeMatcherAnonymousInnerClass( long input )
			 {
				 this._input = input;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "Value = " + _input );

			 }

			 protected internal override bool matchesSafely( AnyValue value )
			 {
				  return value is NumberValue && ( ( NumberValue ) value ).longValue() >= _input;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.Matcher<Neo4Net.cypher.result.QueryResult_Record> eqRecord(final org.hamcrest.Matcher<?>... expectedFieldValues)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<Neo4Net.Cypher.result.QueryResult_Record> EqRecord( params Matcher<object>[] expectedFieldValues )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass2( expectedFieldValues );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass2 : TypeSafeMatcher<Neo4Net.Cypher.result.QueryResult_Record>
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.hamcrest.Matcher<JavaToDotNetGenericWildcard>[] expectedFieldValues;
			 private Matcher<object>[] _expectedFieldValues;

			 public TypeSafeMatcherAnonymousInnerClass2<T1>( Matcher<T1>[] expectedFieldValues )
			 {
				 this._expectedFieldValues = expectedFieldValues;
			 }

			 protected internal override bool matchesSafely( Neo4Net.Cypher.result.QueryResult_Record item )
			 {
				  if ( _expectedFieldValues.Length != item.Fields().Length )
				  {
						return false;
				  }

				  for ( int i = 0; i < item.Fields().Length; i++ )
				  {
						if ( !_expectedFieldValues[i].matches( item.Fields()[i] ) )
						{
							 return false;
						}
				  }
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "Record[" ).appendList( ", fields=[", ",", "]", asList( _expectedFieldValues ) );

			 }
		 }

	}

}