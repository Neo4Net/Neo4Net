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


	using ExecutionPlanDescription = Neo4Net.Graphdb.ExecutionPlanDescription;
	using InputPosition = Neo4Net.Graphdb.InputPosition;
	using Notification = Neo4Net.Graphdb.Notification;
	using QueryStatistics = Neo4Net.Graphdb.QueryStatistics;
	using Result = Neo4Net.Graphdb.Result;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using RFC1123 = Neo4Net.Server.rest.repr.util.RFC1123;
	using Neo4jError = Neo4Net.Server.rest.transactional.error.Neo4jError;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.domain.JsonHelper.writeValue;

	/// <summary>
	/// Writes directly to an output stream, therefore implicitly stateful. Methods must be invoked in the correct
	/// order, as follows:
	/// <ul>
	/// <li><seealso cref="transactionCommitUri(URI) transactionId"/>{@code ?}</li>
	/// <li><seealso cref="statementResult(org.neo4j.graphdb.Result, bool, ResultDataContent...) statementResult"/>{@code *}</li>
	/// <li><seealso cref="errors(System.Collections.IEnumerable) errors"/>{@code ?}</li>
	/// <li><seealso cref="transactionStatus(long expiryDate)"/>{@code ?}</li>
	/// <li><seealso cref="finish() finish"/></li>
	/// </ul>
	/// <para>
	/// Where {@code ?} means invoke at most once, and {@code *} means invoke zero or more times.
	/// </para>
	/// </summary>
	public class ExecutionResultSerializer
	{
		 public ExecutionResultSerializer( Stream output, URI baseUri, LogProvider logProvider, TransitionalPeriodTransactionMessContainer container )
		 {
			  this._baseUri = baseUri;
			  this._log = logProvider.getLog( this.GetType() );
			  this._container = container;
			  _jsonFactory.Codec = new Neo4jJsonCodec( container );
			  JsonGenerator generator = null;
			  try
			  {
					generator = _jsonFactory.createJsonGenerator( output );
			  }
			  catch ( IOException e )
			  {
					LoggedIOException( e );
			  }
			  this.@out = generator;
		 }

		 /// <summary>
		 /// Will always get called at most once, and is the first method to get called. This method is not allowed
		 /// to throw exceptions. If there are network errors or similar, the handler should take appropriate action,
		 /// but never fail this method.
		 /// </summary>
		 public virtual void TransactionCommitUri( URI commitUri )
		 {
			  try
			  {
					EnsureDocumentOpen();
					@out.writeStringField( "commit", commitUri.ToString() );
			  }
			  catch ( IOException e )
			  {
					LoggedIOException( e );
			  }
		 }

		 /// <summary>
		 /// Will get called at most once per statement. Throws IOException so that upstream executor can decide whether
		 /// to execute further statements.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void statementResult(org.neo4j.graphdb.Result result, boolean includeStats, ResultDataContent... resultDataContents) throws java.io.IOException
		 public virtual void StatementResult( Result result, bool includeStats, params ResultDataContent[] resultDataContents )
		 {
			  try
			  {
					EnsureResultsFieldOpen();
					@out.writeStartObject();
					try
					{
						 IEnumerable<string> columns = result.Columns();
						 WriteColumns( columns );
						 WriteRows( columns, result, ConfigureWriters( resultDataContents ) );
						 if ( includeStats )
						 {
							  WriteStats( result.QueryStatistics );
						 }
						 if ( result.QueryExecutionType.requestedExecutionPlanDescription() )
						 {
							  WriteRootPlanDescription( result.ExecutionPlanDescription );
						 }
					}
					finally
					{
						 @out.writeEndObject(); // </result>
					}
			  }
			  catch ( IOException e )
			  {
					throw LoggedIOException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void notifications(Iterable<org.neo4j.graphdb.Notification> notifications) throws java.io.IOException
		 public virtual void Notifications( IEnumerable<Notification> notifications )
		 {
			  //don't add anything if notifications are empty
			  if ( !notifications.GetEnumerator().hasNext() )
			  {
					return;
			  }

			  try
			  {
					EnsureResultsFieldClosed();

					@out.writeArrayFieldStart( "notifications" );
					try
					{
						 foreach ( Notification notification in notifications )
						 {
							  @out.writeStartObject();
							  try
							  {
									@out.writeStringField( "code", notification.Code );
									@out.writeStringField( "severity", notification.Severity.ToString() );
									@out.writeStringField( "title", notification.Title );
									@out.writeStringField( "description", notification.Description );
									WritePosition( notification.Position );

							  }
							  finally
							  {
									@out.writeEndObject();
							  }
						 }

					}
					finally
					{
						 @out.writeEndArray();
					}
			  }
			  catch ( IOException e )
			  {
					throw LoggedIOException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writePosition(org.neo4j.graphdb.InputPosition position) throws java.io.IOException
		 private void WritePosition( InputPosition position )
		 {
			  //do not add position if empty
			  if ( position == InputPosition.empty )
			  {
					return;
			  }

			  @out.writeObjectFieldStart( "position" );
			  try
			  {
					@out.writeNumberField( "offset", position.Offset );
					@out.writeNumberField( "line", position.Line );
					@out.writeNumberField( "column", position.Column );
			  }
			  finally
			  {
					@out.writeEndObject();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeStats(org.neo4j.graphdb.QueryStatistics stats) throws java.io.IOException
		 private void WriteStats( QueryStatistics stats )
		 {
			  @out.writeObjectFieldStart( "stats" );
			  try
			  {
					@out.writeBooleanField( "contains_updates", stats.ContainsUpdates() );
					@out.writeNumberField( "nodes_created", stats.NodesCreated );
					@out.writeNumberField( "nodes_deleted", stats.NodesDeleted );
					@out.writeNumberField( "properties_set", stats.PropertiesSet );
					@out.writeNumberField( "relationships_created", stats.RelationshipsCreated );
					@out.writeNumberField( "relationship_deleted", stats.RelationshipsDeleted );
					@out.writeNumberField( "labels_added", stats.LabelsAdded );
					@out.writeNumberField( "labels_removed", stats.LabelsRemoved );
					@out.writeNumberField( "indexes_added", stats.IndexesAdded );
					@out.writeNumberField( "indexes_removed", stats.IndexesRemoved );
					@out.writeNumberField( "constraints_added", stats.ConstraintsAdded );
					@out.writeNumberField( "constraints_removed", stats.ConstraintsRemoved );
			  }
			  finally
			  {
					@out.writeEndObject();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeRootPlanDescription(org.neo4j.graphdb.ExecutionPlanDescription planDescription) throws java.io.IOException
		 private void WriteRootPlanDescription( ExecutionPlanDescription planDescription )
		 {
			  @out.writeObjectFieldStart( "plan" );
			  try
			  {
					@out.writeObjectFieldStart( "root" );
					try
					{
						 WritePlanDescriptionObjectBody( planDescription );
					}
					finally
					{
						 @out.writeEndObject();
					}
			  }
			  finally
			  {
					@out.writeEndObject();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writePlanDescriptionObjectBody(org.neo4j.graphdb.ExecutionPlanDescription planDescription) throws java.io.IOException
		 private void WritePlanDescriptionObjectBody( ExecutionPlanDescription planDescription )
		 {
			  @out.writeStringField( "operatorType", planDescription.Name );
			  WritePlanArgs( planDescription );
			  WritePlanIdentifiers( planDescription );

			  IList<ExecutionPlanDescription> children = planDescription.Children;
			  @out.writeArrayFieldStart( "children" );
			  try
			  {
					foreach ( ExecutionPlanDescription child in children )
					{
						 @out.writeStartObject();
						 try
						 {
							  WritePlanDescriptionObjectBody( child );
						 }
						 finally
						 {
							  @out.writeEndObject();
						 }
					}
			  }
			  finally
			  {
					@out.writeEndArray();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writePlanArgs(org.neo4j.graphdb.ExecutionPlanDescription planDescription) throws java.io.IOException
		 private void WritePlanArgs( ExecutionPlanDescription planDescription )
		 {
			  foreach ( KeyValuePair<string, object> entry in planDescription.Arguments.SetOfKeyValuePairs() )
			  {
					string fieldName = entry.Key;
					object fieldValue = entry.Value;

					@out.writeFieldName( fieldName );
					writeValue( @out, fieldValue );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writePlanIdentifiers(org.neo4j.graphdb.ExecutionPlanDescription planDescription) throws java.io.IOException
		 private void WritePlanIdentifiers( ExecutionPlanDescription planDescription )
		 {
			  @out.writeArrayFieldStart( "identifiers" );
			  foreach ( string id in planDescription.Identifiers )
			  {
					@out.writeString( id );
			  }
			  @out.writeEndArray();
		 }

		 /// <summary>
		 /// Will get called once if any errors occurred,
		 /// after <seealso cref="statementResult(org.neo4j.graphdb.Result, bool, ResultDataContent...)"/>  statementResults}
		 /// has been called This method is not allowed to throw exceptions. If there are network errors or similar, the
		 /// handler should take appropriate action, but never fail this method. </summary>
		 /// <param name="errors"> the errors to write </param>
		 public virtual void Errors<T1>( IEnumerable<T1> errors ) where T1 : Neo4Net.Server.rest.transactional.error.Neo4jError
		 {
			  try
			  {
					EnsureDocumentOpen();
					EnsureResultsFieldClosed();
					@out.writeArrayFieldStart( "errors" );
					try
					{
						 foreach ( Neo4jError error in errors )
						 {
							  try
							  {
									@out.writeStartObject();
									@out.writeObjectField( "code", error.Status().code().serialize() );
									@out.writeObjectField( "message", error.Message );
									if ( error.ShouldSerializeStackTrace() )
									{
										 @out.writeObjectField( "stackTrace", error.StackTraceAsString );
									}
							  }
							  finally
							  {
									@out.writeEndObject();
							  }
						 }
					}
					finally
					{
						 @out.writeEndArray();
						 _currentState = State.ErrorsWritten;
					}
			  }
			  catch ( IOException e )
			  {
					LoggedIOException( e );
			  }
		 }

		 public virtual void TransactionStatus( long expiryDate )
		 {
			  try
			  {
					EnsureDocumentOpen();
					EnsureResultsFieldClosed();
					@out.writeObjectFieldStart( "transaction" );
					@out.writeStringField( "expires", RFC1123.formatDate( new DateTime( expiryDate ) ) );
					@out.writeEndObject();
			  }
			  catch ( IOException e )
			  {
					LoggedIOException( e );
			  }
		 }

		 /// <summary>
		 /// This method must be called exactly once, and no method must be called after calling this method.
		 /// This method may not fail.
		 /// </summary>
		 public virtual void Finish()
		 {
			  try
			  {
					EnsureDocumentOpen();
					if ( _currentState != State.ErrorsWritten )
					{
						 Errors( Collections.emptyList() );
					}
					@out.writeEndObject();
					@out.flush();
			  }
			  catch ( IOException e )
			  {
					LoggedIOException( e );
			  }
		 }

		 private ResultDataContentWriter ConfigureWriters( ResultDataContent[] specifiers )
		 {
			  if ( specifiers == null || specifiers.Length == 0 )
			  {
					return ResultDataContent.Row.writer( _baseUri ); // default
			  }
			  if ( specifiers.Length == 1 )
			  {
					return specifiers[0].writer( _baseUri );
			  }
			  ResultDataContentWriter[] writers = new ResultDataContentWriter[specifiers.Length];
			  for ( int i = 0; i < specifiers.Length; i++ )
			  {
					writers[i] = specifiers[i].writer( _baseUri );
			  }
			  return new AggregatingWriter( writers );
		 }

		 private enum State
		 {
			  Empty,
			  DocumentOpen,
			  ResultsOpen,
			  ResultsClosed,
			  ErrorsWritten
		 }

		 private State _currentState = State.Empty;

		 private static readonly JsonFactory _jsonFactory = new JsonFactory().disable(JsonGenerator.Feature.FLUSH_PASSED_TO_STREAM);
		 private readonly JsonGenerator @out;
		 private readonly URI _baseUri;
		 private readonly Log _log;
		 private readonly TransitionalPeriodTransactionMessContainer _container;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensureDocumentOpen() throws java.io.IOException
		 private void EnsureDocumentOpen()
		 {
			  if ( _currentState == State.Empty )
			  {
					@out.writeStartObject();
					_currentState = State.DocumentOpen;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensureResultsFieldOpen() throws java.io.IOException
		 private void EnsureResultsFieldOpen()
		 {
			  EnsureDocumentOpen();
			  if ( _currentState == State.DocumentOpen )
			  {
					@out.writeArrayFieldStart( "results" );
					_currentState = State.ResultsOpen;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensureResultsFieldClosed() throws java.io.IOException
		 private void EnsureResultsFieldClosed()
		 {
			  EnsureResultsFieldOpen();
			  if ( _currentState == State.ResultsOpen )
			  {
					@out.writeEndArray();
					_currentState = State.ResultsClosed;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeRows(final Iterable<String> columns, org.neo4j.graphdb.Result data, final ResultDataContentWriter writer) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private void WriteRows( IEnumerable<string> columns, Result data, ResultDataContentWriter writer )
		 {
			  @out.writeArrayFieldStart( "data" );
			  try
			  {
					data.Accept(row =>
					{
					 @out.writeStartObject();
					 try
					 {
						  using ( TransactionStateChecker txStateChecker = TransactionStateChecker.Create( _container ) )
						  {
								writer.Write( @out, columns, row, txStateChecker );
						  }
					 }
					 finally
					 {
						  @out.writeEndObject();
					 }
					 return true;
					});
			  }
			  finally
			  {
					@out.writeEndArray(); // </data>
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeColumns(Iterable<String> columns) throws java.io.IOException
		 private void WriteColumns( IEnumerable<string> columns )
		 {
			  try
			  {
					@out.writeArrayFieldStart( "columns" );
					foreach ( string key in columns )
					{
						 @out.writeString( key );
					}
			  }
			  finally
			  {
					@out.writeEndArray(); // </columns>
			  }
		 }

		 private IOException LoggedIOException( IOException exception )
		 {
			  if ( Exceptions.contains( exception, "Broken pipe", typeof( IOException ) ) )
			  {
					_log.error( "Unable to reply to request, because the client has closed the connection (Broken pipe)." );
			  }
			  else
			  {
					_log.error( "Failed to generate JSON output.", exception );
			  }
			  return exception;
		 }
	}

}