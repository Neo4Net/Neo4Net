using System;
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
namespace Neo4Net.Server.rest.transactional
{
	using JsonFactory = org.codehaus.jackson.JsonFactory;
	using JsonGenerator = org.codehaus.jackson.JsonGenerator;
	using JsonParseException = org.codehaus.jackson.JsonParseException;
	using JsonParser = org.codehaus.jackson.JsonParser;
	using JsonToken = org.codehaus.jackson.JsonToken;
	using JsonMappingException = org.codehaus.jackson.map.JsonMappingException;


	using Neo4Net.Helpers.Collections;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Neo4NetError = Neo4Net.Server.rest.transactional.error.Neo4NetError;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.codehaus.jackson.JsonToken.END_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.codehaus.jackson.JsonToken.END_OBJECT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.codehaus.jackson.JsonToken.FIELD_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.codehaus.jackson.JsonToken.START_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.codehaus.jackson.JsonToken.START_OBJECT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.map;

	public class StatementDeserializer : PrefetchingIterator<Statement>
	{
		 private static readonly JsonFactory _jsonFactory = new JsonFactory().setCodec(new Neo4NetJsonCodec()).disable(JsonGenerator.Feature.FLUSH_PASSED_TO_STREAM);
		 private static readonly IDictionary<string, object> _noParameters = unmodifiableMap( map() );

		 private readonly JsonParser _input;
		 private State _state;
		 private IList<Neo4NetError> _errors;

		 private enum State
		 {
			  BeforeOuterArray,
			  InBody,
			  Finished
		 }

		 public StatementDeserializer( Stream input )
		 {
			  try
			  {
					this._input = _jsonFactory.createJsonParser( input );
					this._state = State.BeforeOuterArray;
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 public virtual IEnumerator<Neo4NetError> Errors()
		 {
			  return _errors == null ? emptyIterator() : _errors.GetEnumerator();
		 }

		 protected internal override Statement FetchNextOrNull()
		 {
			  try
			  {
					if ( _errors != null )
					{
						 return null;
					}

					switch ( _state )
					{
						 case Neo4Net.Server.rest.transactional.StatementDeserializer.State.BeforeOuterArray:
							  if ( !BeginsWithCorrectTokens() )
							  {
									return null;
							  }
							  _state = State.InBody;
							 goto case IN_BODY;
						 case Neo4Net.Server.rest.transactional.StatementDeserializer.State.InBody:
							  string statement = null;
							  IDictionary<string, object> parameters = null;
							  IList<object> resultsDataContents = null;
							  bool includeStats = false;
							  JsonToken tok;

							  while ( ( tok = _input.nextToken() ) != null && tok != END_OBJECT )
							  {
									if ( tok == END_ARRAY )
									{
										 // No more statements
										 _state = State.Finished;
										 return null;
									}

									_input.nextValue();
									string currentName = _input.CurrentName;
									switch ( currentName )
									{
									case "statement":
										 statement = _input.readValueAs( typeof( string ) );
										 break;
									case "parameters":
										 parameters = ReadMap( _input );
										 break;
									case "resultDataContents":
										 resultsDataContents = ReadArray( _input );
										 break;
									case "includeStats":
										 includeStats = _input.BooleanValue;
										 break;
									default:
										 DiscardValue( _input );
									 break;
									}
							  }

							  if ( string.ReferenceEquals( statement, null ) )
							  {
									AddError( new Neo4NetError( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, new DeserializationException( "No statement provided." ) ) );
									return null;
							  }
							  return new Statement( statement, parameters == null ? _noParameters : parameters, includeStats, ResultDataContent.fromNames( resultsDataContents ) );

						 case Neo4Net.Server.rest.transactional.StatementDeserializer.State.Finished:
							  return null;

						 default:
							  break;
					}
					return null;
			  }
			  catch ( Exception e ) when ( e is JsonParseException || e is JsonMappingException )
			  {
					AddError( new Neo4NetError( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, new DeserializationException( "Unable to deserialize request", e ) ) );
					return null;
			  }
			  catch ( IOException e )
			  {
					AddError( new Neo4NetError( Neo4Net.Kernel.Api.Exceptions.Status_Network.CommunicationError, e ) );
					return null;
			  }
			  catch ( Exception e )
			  {
					AddError( new Neo4NetError( Neo4Net.Kernel.Api.Exceptions.Status_General.UnknownError, e ) );
					return null;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void discardValue(org.codehaus.jackson.JsonParser input) throws java.io.IOException
		 private void DiscardValue( JsonParser input )
		 {
			  // This could be done without building up an object
			  input.readValueAs( typeof( object ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static java.util.Map<String, Object> readMap(org.codehaus.jackson.JsonParser input) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private static IDictionary<string, object> ReadMap( JsonParser input )
		 {
			  return input.readValueAs( typeof( System.Collections.IDictionary ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static java.util.List<Object> readArray(org.codehaus.jackson.JsonParser input) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private static IList<object> ReadArray( JsonParser input )
		 {
			  return input.readValueAs( typeof( System.Collections.IList ) );
		 }

		 private void AddError( Neo4NetError error )
		 {
			  if ( _errors == null )
			  {
					_errors = new LinkedList<Neo4NetError>();
			  }
			  _errors.Add( error );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean beginsWithCorrectTokens() throws java.io.IOException
		 private bool BeginsWithCorrectTokens()
		 {
			  IList<JsonToken> expectedTokens = new IList<JsonToken> { START_OBJECT, FIELD_NAME, START_ARRAY };
			  string expectedField = "statements";

			  IList<JsonToken> foundTokens = new List<JsonToken>();

			  for ( int i = 0; i < expectedTokens.Count; i++ )
			  {
					JsonToken token = _input.nextToken();
					if ( i == 0 && token == null )
					{
						 return false;
					}
					if ( token == FIELD_NAME && !expectedField.Equals( _input.Text ) )
					{
						 AddError( new Neo4NetError( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, new DeserializationException( string.Format( "Unable to deserialize request. " + "Expected first field to be '{0}', but was '{1}'.", expectedField, _input.Text ) ) ) );
						 return false;
					}
					foundTokens.Add( token );
			  }
//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: if (!expectedTokens.equals(foundTokens))
			  if ( !expectedTokens.SequenceEqual( foundTokens ) )
			  {
					AddError( new Neo4NetError( Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat, new DeserializationException( string.Format( "Unable to deserialize request. " + "Expected {0}, found {1}.", expectedTokens, foundTokens ) ) ) );
					return false;
			  }
			  return true;
		 }
	}

}