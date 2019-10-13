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
namespace Neo4Net.Server.rest.transactional.integration
{
	using JsonNode = org.codehaus.jackson.JsonNode;
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using RFC1123 = Neo4Net.Server.rest.repr.util.RFC1123;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.iterator;

	/// <summary>
	/// Matchers and assertion methods for the transactional endpoint.
	/// </summary>
	public class TransactionMatchers
	{
		 private TransactionMatchers()
		 {
		 }

		 internal static Matcher<string> ValidRFCTimestamp
		 {
			 get
			 {
				  return new TypeSafeMatcherAnonymousInnerClass();
			 }
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<string>
		 {
			 protected internal override bool matchesSafely( string item )
			 {
				  try
				  {
						return RFC1123.parseTimestamp( item ).Ticks > 0;
				  }
				  catch ( ParseException )
				  {
						return false;
				  }
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "valid RFC1134 timestamp" );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: static org.hamcrest.Matcher<String> matches(final String pattern)
		 internal static Matcher<string> Matches( string pattern )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.regex.Pattern regex = java.util.regex.Pattern.compile(pattern);
			  Pattern regex = Pattern.compile( pattern );

			  return new TypeSafeMatcherAnonymousInnerClass2( pattern, regex );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass2 : TypeSafeMatcher<string>
		 {
			 private string _pattern;
			 private Pattern _regex;

			 public TypeSafeMatcherAnonymousInnerClass2( string pattern, Pattern regex )
			 {
				 this._pattern = pattern;
				 this._regex = regex;
			 }

			 protected internal override bool matchesSafely( string item )
			 {
				  return _regex.matcher( item ).matches();
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "matching regex: " ).appendValue( _pattern );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static org.hamcrest.Matcher<? super org.neo4j.test.server.HTTP.Response> containsNoErrors()
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<object> ContainsNoErrors()
		 {
			  return HasErrors();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static org.hamcrest.Matcher<? super org.neo4j.test.server.HTTP.Response> hasErrors(final org.neo4j.kernel.api.exceptions.Status... expectedErrors)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<object> HasErrors( params Status[] expectedErrors )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass3( expectedErrors );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass3 : TypeSafeMatcher<HTTP.Response>
		 {
			 private Status[] _expectedErrors;

			 public TypeSafeMatcherAnonymousInnerClass3( Status[] expectedErrors )
			 {
				 this._expectedErrors = expectedErrors;
			 }

			 protected internal override bool matchesSafely( HTTP.Response response )
			 {
				  try
				  {
						IEnumerator<JsonNode> errors = response.Get( "errors" ).GetEnumerator();
						IEnumerator<Status> expected = iterator( _expectedErrors );

						while ( expected.MoveNext() )
						{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 assertTrue( errors.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 assertThat( errors.next().get("code").asText(), equalTo(expected.Current.code().serialize()) );
						}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						if ( errors.hasNext() )
						{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 JsonNode error = errors.next();
							 fail( "Expected no more errors, but got " + error.get( "code" ) + " - '" + error.get( "message" ) + "'." );
						}
						return true;

				  }
				  catch ( JsonParseException )
				  {
						return false;
				  }
			 }

			 public override void describeTo( Description description )
			 {
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static org.codehaus.jackson.JsonNode getJsonNodeWithName(org.neo4j.test.server.HTTP.Response response, String name) throws org.neo4j.server.rest.domain.JsonParseException
		 internal static JsonNode GetJsonNodeWithName( HTTP.Response response, string name )
		 {
			  return response.Get( "results" ).get( 0 ).get( "data" ).get( 0 ).get( name );
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static org.hamcrest.Matcher<? super org.neo4j.test.server.HTTP.Response> rowContainsDeletedEntities(final int nodes, final int rels)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<object> RowContainsDeletedEntities( int nodes, int rels )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass4( nodes, rels );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass4 : TypeSafeMatcher<HTTP.Response>
		 {
			 private int _nodes;
			 private int _rels;

			 public TypeSafeMatcherAnonymousInnerClass4( int nodes, int rels )
			 {
				 this._nodes = nodes;
				 this._rels = rels;
			 }

			 protected internal override bool matchesSafely( HTTP.Response response )
			 {
				  try
				  {
						IEnumerator<JsonNode> meta = GetJsonNodeWithName( response, "meta" ).GetEnumerator();

						int nodeCounter = 0;
						int relCounter = 0;
						for ( int i = 0; i < _nodes + _rels; ++i )
						{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 assertTrue( meta.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 JsonNode node = meta.next();
							 assertThat( node.get( "deleted" ).asBoolean(), equalTo(true) );
							 string type = node.get( "type" ).TextValue;
							 switch ( type )
							 {
							 case "node":
								  ++nodeCounter;
								  break;
							 case "relationship":
								  ++relCounter;
								  break;
							 default:
								  fail( "Unexpected type: " + type );
								  break;
							 }
						}
						assertEquals( _nodes, nodeCounter );
						assertEquals( _rels, relCounter );
						while ( meta.MoveNext() )
						{
							 JsonNode node = meta.Current;
							 assertThat( node.get( "deleted" ).asBoolean(), equalTo(false) );
						}
						return true;
				  }
				  catch ( JsonParseException )
				  {
						return false;
				  }
			 }

			 public override void describeTo( Description description )
			 {
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static org.hamcrest.Matcher<? super org.neo4j.test.server.HTTP.Response> rowContainsDeletedEntitiesInPath(final int nodes, final int rels)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<object> RowContainsDeletedEntitiesInPath( int nodes, int rels )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass5( nodes, rels );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass5 : TypeSafeMatcher<HTTP.Response>
		 {
			 private int _nodes;
			 private int _rels;

			 public TypeSafeMatcherAnonymousInnerClass5( int nodes, int rels )
			 {
				 this._nodes = nodes;
				 this._rels = rels;
			 }

			 protected internal override bool matchesSafely( HTTP.Response response )
			 {
				  try
				  {
						IEnumerator<JsonNode> meta = GetJsonNodeWithName( response, "meta" ).GetEnumerator();

						int nodeCounter = 0;
						int relCounter = 0;
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						assertTrue( "Expected to find a JSON node, but there was none", meta.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						JsonNode node = meta.next();
						assertTrue( "Expected the node to be a list (for a path)", node.Array );
						foreach ( JsonNode inner in node )
						{
							 string type = inner.get( "type" ).TextValue;
							 switch ( type )
							 {
							 case "node":
								  if ( inner.get( "deleted" ).asBoolean() )
								  {
										++nodeCounter;
								  }
								  break;
							 case "relationship":
								  if ( inner.get( "deleted" ).asBoolean() )
								  {
										++relCounter;
								  }
								  break;
							 default:
								  fail( "Unexpected type: " + type );
								  break;
							 }
						}
						assertEquals( _nodes, nodeCounter );
						assertEquals( _rels, relCounter );
						return true;
				  }
				  catch ( JsonParseException )
				  {
						return false;
				  }
			 }

			 public override void describeTo( Description description )
			 {
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static org.hamcrest.Matcher<? super org.neo4j.test.server.HTTP.Response> rowContainsMetaNodesAtIndex(int... indexes)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<object> RowContainsMetaNodesAtIndex( params int[] indexes )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass6( indexes );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass6 : TypeSafeMatcher<HTTP.Response>
		 {
			 private int[] _indexes;

			 public TypeSafeMatcherAnonymousInnerClass6( int[] indexes )
			 {
				 this._indexes = indexes;
			 }

			 protected internal override bool matchesSafely( HTTP.Response response )
			 {
				  return AssertElementAtMetaIndex( response, _indexes, "node" );
			 }

			 public override void describeTo( Description description )
			 {
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static org.hamcrest.Matcher<? super org.neo4j.test.server.HTTP.Response> rowContainsMetaRelsAtIndex(int... indexes)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<object> RowContainsMetaRelsAtIndex( params int[] indexes )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass7( indexes );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass7 : TypeSafeMatcher<HTTP.Response>
		 {
			 private int[] _indexes;

			 public TypeSafeMatcherAnonymousInnerClass7( int[] indexes )
			 {
				 this._indexes = indexes;
			 }

			 protected internal override bool matchesSafely( HTTP.Response response )
			 {
				  return AssertElementAtMetaIndex( response, _indexes, "relationship" );
			 }

			 public override void describeTo( Description description )
			 {
			 }
		 }

		 private static bool AssertElementAtMetaIndex( HTTP.Response response, int[] indexes, string element )
		 {
			  try
			  {
					IEnumerator<JsonNode> meta = GetJsonNodeWithName( response, "meta" ).GetEnumerator();

					int i = 0;
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					for ( int metaIndex = 0; meta.hasNext() && i < indexes.Length; metaIndex++ )
					{
						 JsonNode node = meta.Current;
						 if ( !node.Null )
						 {
							  string type = node.get( "type" ).TextValue;
							  if ( type.Equals( element ) )
							  {
									assertEquals( "Expected " + element + " to be at indexes " + Arrays.ToString( indexes ) + ", but found it at " + metaIndex, indexes[i], metaIndex );
									++i;
							  }
							  else
							  {
									assertNotEquals( "Expected " + element + " at index " + metaIndex + ", but found " + type, indexes[i], metaIndex );
							  }
						 }
					}
					return i == indexes.Length;
			  }
			  catch ( JsonParseException )
			  {
					return false;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static org.hamcrest.Matcher<? super org.neo4j.test.server.HTTP.Response> rowContainsAMetaListAtIndex(int index)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<object> RowContainsAMetaListAtIndex( int index )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass8( index );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass8 : TypeSafeMatcher<HTTP.Response>
		 {
			 private int _index;

			 public TypeSafeMatcherAnonymousInnerClass8( int index )
			 {
				 this._index = index;
			 }

			 protected internal override bool matchesSafely( HTTP.Response response )
			 {
				  try
				  {
						IEnumerator<JsonNode> meta = GetJsonNodeWithName( response, "meta" ).GetEnumerator();

						for ( int metaIndex = 0; meta.MoveNext(); metaIndex++ )
						{
							 JsonNode node = meta.Current;
							 if ( metaIndex == _index )
							 {
								  assertTrue( node.Array );
							 }
						}
						return true;
				  }
				  catch ( JsonParseException )
				  {
						return false;
				  }
			 }

			 public override void describeTo( Description description )
			 {
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static org.hamcrest.Matcher<? super org.neo4j.test.server.HTTP.Response> restContainsDeletedEntities(final int amount)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<object> RestContainsDeletedEntities( int amount )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass9( amount );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass9 : TypeSafeMatcher<HTTP.Response>
		 {
			 private int _amount;

			 public TypeSafeMatcherAnonymousInnerClass9( int amount )
			 {
				 this._amount = amount;
			 }

			 protected internal override bool matchesSafely( HTTP.Response response )
			 {
				  try
				  {
						IEnumerator<JsonNode> entities = GetJsonNodeWithName( response, "rest" ).GetEnumerator();

						for ( int i = 0; i < _amount; ++i )
						{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 assertTrue( entities.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 JsonNode node = entities.next();
							 assertThat( node.get( "metadata" ).get( "deleted" ).asBoolean(), equalTo(true) );
						}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						if ( entities.hasNext() )
						{
							 fail( "Expected no more entities" );
						}
						return true;
				  }
				  catch ( JsonParseException )
				  {
						return false;
				  }
			 }

			 public override void describeTo( Description description )
			 {
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static org.hamcrest.Matcher<? super org.neo4j.test.server.HTTP.Response> graphContainsDeletedNodes(final int amount)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<object> GraphContainsDeletedNodes( int amount )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass10( amount );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass10 : TypeSafeMatcher<HTTP.Response>
		 {
			 private int _amount;

			 public TypeSafeMatcherAnonymousInnerClass10( int amount )
			 {
				 this._amount = amount;
			 }

			 protected internal override bool matchesSafely( HTTP.Response response )
			 {
				  try
				  {
						IEnumerator<JsonNode> nodes = GetJsonNodeWithName( response, "graph" ).get( "nodes" ).GetEnumerator();
						int deleted = 0;
						while ( nodes.MoveNext() )
						{
							 JsonNode node = nodes.Current;
							 if ( node.get( "deleted" ) != null )
							 {
								  assertTrue( node.get( "deleted" ).asBoolean() );
								  deleted++;
							 }
						}
						assertEquals( format( "Expected to see %d deleted elements but %d was encountered.", _amount, deleted ), _amount, deleted );
						return true;
				  }
				  catch ( JsonParseException )
				  {
						return false;
				  }
			 }

			 public override void describeTo( Description description )
			 {
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static org.hamcrest.Matcher<? super org.neo4j.test.server.HTTP.Response> graphContainsNoDeletedEntities()
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<object> GraphContainsNoDeletedEntities()
		 {
			  return new TypeSafeMatcherAnonymousInnerClass11();
		 }

		 private class TypeSafeMatcherAnonymousInnerClass11 : TypeSafeMatcher<HTTP.Response>
		 {
			 protected internal override bool matchesSafely( HTTP.Response response )
			 {
				  try
				  {
						foreach ( JsonNode node in GetJsonNodeWithName( response, "graph" ).get( "nodes" ) )
						{
							 assertNull( node.get( "deleted" ) );
						}
						foreach ( JsonNode node in GetJsonNodeWithName( response, "graph" ).get( "relationships" ) )
						{
							 assertNull( node.get( "deleted" ) );
						}
						return true;
				  }
				  catch ( JsonParseException )
				  {
						return false;
				  }
			 }

			 public override void describeTo( Description description )
			 {
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static org.hamcrest.Matcher<? super org.neo4j.test.server.HTTP.Response> rowContainsNoDeletedEntities()
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<object> RowContainsNoDeletedEntities()
		 {
			  return new TypeSafeMatcherAnonymousInnerClass12();
		 }

		 private class TypeSafeMatcherAnonymousInnerClass12 : TypeSafeMatcher<HTTP.Response>
		 {
			 protected internal override bool matchesSafely( HTTP.Response response )
			 {
				  try
				  {
						foreach ( JsonNode node in GetJsonNodeWithName( response, "meta" ) )
						{
							 assertFalse( node.get( "deleted" ).asBoolean() );
						}
						return true;
				  }
				  catch ( JsonParseException )
				  {
						return false;
				  }
			 }

			 public override void describeTo( Description description )
			 {
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static org.hamcrest.Matcher<? super org.neo4j.test.server.HTTP.Response> restContainsNoDeletedEntities()
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<object> RestContainsNoDeletedEntities()
		 {
			  return new TypeSafeMatcherAnonymousInnerClass13();
		 }

		 private class TypeSafeMatcherAnonymousInnerClass13 : TypeSafeMatcher<HTTP.Response>
		 {
			 protected internal override bool matchesSafely( HTTP.Response response )
			 {
				  try
				  {
						foreach ( JsonNode node in GetJsonNodeWithName( response, "rest" ) )
						{
							 assertNull( node.get( "metadata" ).get( "deleted" ) );
						}
						return true;
				  }
				  catch ( JsonParseException )
				  {
						return false;
				  }
			 }

			 public override void describeTo( Description description )
			 {
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static org.hamcrest.Matcher<? super org.neo4j.test.server.HTTP.Response> graphContainsDeletedRelationships(final int amount)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<object> GraphContainsDeletedRelationships( int amount )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass14( amount );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass14 : TypeSafeMatcher<HTTP.Response>
		 {
			 private int _amount;

			 public TypeSafeMatcherAnonymousInnerClass14( int amount )
			 {
				 this._amount = amount;
			 }

			 protected internal override bool matchesSafely( HTTP.Response response )
			 {
				  try
				  {
						IEnumerator<JsonNode> relationships = GetJsonNodeWithName( response, "graph" ).get( "relationships" ).GetEnumerator();

						for ( int i = 0; i < _amount; ++i )
						{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 assertTrue( relationships.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 JsonNode node = relationships.next();
							 assertThat( node.get( "deleted" ).asBoolean(), equalTo(true) );
						}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						if ( relationships.hasNext() )
						{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							 JsonNode node = relationships.next();
							 fail( "Expected no more nodes, but got a node with id " + node.get( "id" ) );
						}
						return true;
				  }
				  catch ( JsonParseException )
				  {
						return false;
				  }
			 }

			 public override void describeTo( Description description )
			 {
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WhileLoopReplaceableByForEach") public static long countNodes(org.neo4j.graphdb.GraphDatabaseService graphdb)
		 public static long CountNodes( GraphDatabaseService graphdb )
		 {
			  using ( Transaction ignore = graphdb.BeginTx() )
			  {
					long count = 0;
					IEnumerator<Node> allNodes = graphdb.AllNodes.GetEnumerator();
					while ( allNodes.MoveNext() )
					{
						 allNodes.Current;
						 count++;
					}
					return count;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static org.hamcrest.Matcher<? super org.neo4j.test.server.HTTP.Response> containsNoStackTraces()
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static Matcher<object> ContainsNoStackTraces()
		 {
			  return new TypeSafeMatcherAnonymousInnerClass15();
		 }

		 private class TypeSafeMatcherAnonymousInnerClass15 : TypeSafeMatcher<HTTP.Response>
		 {
			 protected internal override bool matchesSafely( HTTP.Response response )
			 {
				  IDictionary<string, object> content = response.Content();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.List<java.util.Map<String, Object>> errors = (java.util.List<java.util.Map<String, Object>>) content.get("errors");
				  IList<IDictionary<string, object>> errors = ( IList<IDictionary<string, object>> ) content["errors"];

				  foreach ( IDictionary<string, object> error in errors )
				  {
						if ( error.ContainsKey( "stackTrace" ) )
						{
							 return false;
						}
				  }

				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "contains stack traces" );
			 }
		 }
	}

}