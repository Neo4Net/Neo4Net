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
namespace Neo4Net.Server.rest.web
{

	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;
	using Log = Neo4Net.Logging.Log;
	using AuthorizedRequestWrapper = Neo4Net.Server.rest.dbms.AuthorizedRequestWrapper;
	using ExecutionResultSerializer = Neo4Net.Server.rest.transactional.ExecutionResultSerializer;
	using TransactionFacade = Neo4Net.Server.rest.transactional.TransactionFacade;
	using TransactionHandle = Neo4Net.Server.rest.transactional.TransactionHandle;
	using TransactionTerminationHandle = Neo4Net.Server.rest.transactional.TransactionTerminationHandle;
	using Neo4NetError = Neo4Net.Server.rest.transactional.error.Neo4NetError;
	using TransactionLifecycleException = Neo4Net.Server.rest.transactional.error.TransactionLifecycleException;
	using HttpHeaderUtils = Neo4Net.Server.web.HttpHeaderUtils;
	using UsageData = Neo4Net.Udc.UsageData;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.udc.UsageDataKeys.Features_Fields.http_tx_endpoint;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.udc.UsageDataKeys.features;

	/// <summary>
	/// This does basic mapping from HTTP to <seealso cref="org.Neo4Net.server.rest.transactional.TransactionFacade"/>, and should not
	/// do anything more complicated than that.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Path("/transaction") public class TransactionalService
	public class TransactionalService
	{
		 private readonly TransactionFacade _facade;
		 private readonly UsageData _usage;
		 private readonly TransactionUriScheme _uriScheme;
		 private Log _log;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public TransactionalService(@Context TransactionFacade facade, @Context UriInfo uriInfo, @Context UsageData usage, @Context Log log)
		 public TransactionalService( TransactionFacade facade, UriInfo uriInfo, UsageData usage, Log log )
		 {
			  this._facade = facade;
			  this._usage = usage;
			  this._uriScheme = new TransactionUriBuilder( uriInfo );
			  this._log = log;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @Consumes({javax.ws.rs.core.MediaType.APPLICATION_JSON}) @Produces({javax.ws.rs.core.MediaType.APPLICATION_JSON}) public javax.ws.rs.core.Response executeStatementsInNewTransaction(final java.io.InputStream input, @Context final javax.ws.rs.core.UriInfo uriInfo, @Context final javax.servlet.http.HttpServletRequest request)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public virtual Response ExecuteStatementsInNewTransaction( Stream input, UriInfo uriInfo, HttpServletRequest request )
		 {
			  _usage.get( features ).flag( http_tx_endpoint );
			  LoginContext loginContext = AuthorizedRequestWrapper.getLoginContextFromHttpServletRequest( request );
			  long customTransactionTimeout = HttpHeaderUtils.getTransactionTimeout( request, _log );
			  TransactionHandle transactionHandle = _facade.newTransactionHandle( _uriScheme, false, loginContext, customTransactionTimeout );
			  return CreatedResponse( transactionHandle, executeStatements( input, transactionHandle, uriInfo.BaseUri, request ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @Path("/{id}") @Consumes({javax.ws.rs.core.MediaType.APPLICATION_JSON}) @Produces({javax.ws.rs.core.MediaType.APPLICATION_JSON}) public javax.ws.rs.core.Response executeStatements(@PathParam("id") final long id, final java.io.InputStream input, @Context final javax.ws.rs.core.UriInfo uriInfo, @Context final javax.servlet.http.HttpServletRequest request)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public virtual Response ExecuteStatements( long id, Stream input, UriInfo uriInfo, HttpServletRequest request )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.server.rest.transactional.TransactionHandle transactionHandle;
			  TransactionHandle transactionHandle;
			  try
			  {
					transactionHandle = _facade.findTransactionHandle( id );
			  }
			  catch ( TransactionLifecycleException e )
			  {
					return InvalidTransaction( e, uriInfo.BaseUri );
			  }
			  return OkResponse( executeStatements( input, transactionHandle, uriInfo.BaseUri, request ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @Path("/{id}/commit") @Consumes({javax.ws.rs.core.MediaType.APPLICATION_JSON}) @Produces({javax.ws.rs.core.MediaType.APPLICATION_JSON}) public javax.ws.rs.core.Response commitTransaction(@PathParam("id") final long id, final java.io.InputStream input, @Context final javax.ws.rs.core.UriInfo uriInfo, @Context final javax.servlet.http.HttpServletRequest request)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public virtual Response CommitTransaction( long id, Stream input, UriInfo uriInfo, HttpServletRequest request )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.server.rest.transactional.TransactionHandle transactionHandle;
			  TransactionHandle transactionHandle;
			  try
			  {
					transactionHandle = _facade.findTransactionHandle( id );
			  }
			  catch ( TransactionLifecycleException e )
			  {
					return InvalidTransaction( e, uriInfo.BaseUri );
			  }
			  return OkResponse( ExecuteStatementsAndCommit( input, transactionHandle, uriInfo.BaseUri, request ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @Path("/commit") @Consumes({javax.ws.rs.core.MediaType.APPLICATION_JSON}) @Produces({javax.ws.rs.core.MediaType.APPLICATION_JSON}) public javax.ws.rs.core.Response commitNewTransaction(final java.io.InputStream input, @Context final javax.ws.rs.core.UriInfo uriInfo, @Context final javax.servlet.http.HttpServletRequest request)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public virtual Response CommitNewTransaction( Stream input, UriInfo uriInfo, HttpServletRequest request )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.server.rest.transactional.TransactionHandle transactionHandle;
			  TransactionHandle transactionHandle;
			  LoginContext loginContext = AuthorizedRequestWrapper.getLoginContextFromHttpServletRequest( request );
			  long customTransactionTimeout = HttpHeaderUtils.getTransactionTimeout( request, _log );
			  transactionHandle = _facade.newTransactionHandle( _uriScheme, true, loginContext, customTransactionTimeout );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.ws.rs.core.StreamingOutput streamingResults = executeStatementsAndCommit(input, transactionHandle, uriInfo.getBaseUri(), request);
			  StreamingOutput streamingResults = ExecuteStatementsAndCommit( input, transactionHandle, uriInfo.BaseUri, request );
			  return OkResponse( streamingResults );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DELETE @Path("/{id}") @Consumes({javax.ws.rs.core.MediaType.APPLICATION_JSON}) public javax.ws.rs.core.Response rollbackTransaction(@PathParam("id") final long id, @Context UriInfo uriInfo)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public virtual Response RollbackTransaction( long id, UriInfo uriInfo )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.server.rest.transactional.TransactionHandle transactionHandle;
			  TransactionHandle transactionHandle;
			  try
			  {
					transactionHandle = _facade.terminate( id );
			  }
			  catch ( TransactionLifecycleException e )
			  {
					return InvalidTransaction( e, uriInfo.BaseUri );
			  }
			  return OkResponse( Rollback( transactionHandle, uriInfo.BaseUri ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private javax.ws.rs.core.Response invalidTransaction(final org.Neo4Net.server.rest.transactional.error.TransactionLifecycleException e, final java.net.URI baseUri)
		 private Response InvalidTransaction( TransactionLifecycleException e, URI baseUri )
		 {
			  return Response.status( Response.Status.NOT_FOUND ).entity( SerializeError( e.ToNeo4NetError(), baseUri ) ).build();
		 }

		 private Response CreatedResponse( TransactionHandle transactionHandle, StreamingOutput streamingResults )
		 {
			  return Response.created( transactionHandle.Uri() ).entity(streamingResults).build();
		 }

		 private Response OkResponse( StreamingOutput streamingResults )
		 {
			  return Response.ok().entity(streamingResults).build();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private javax.ws.rs.core.StreamingOutput executeStatements(final java.io.InputStream input, final org.Neo4Net.server.rest.transactional.TransactionHandle transactionHandle, final java.net.URI baseUri, final javax.servlet.http.HttpServletRequest request)
		 private StreamingOutput ExecuteStatements( Stream input, TransactionHandle transactionHandle, URI baseUri, HttpServletRequest request )
		 {
			  return output => transactionHandle.execute( _facade.deserializer( input ), _facade.serializer( output, baseUri ), request );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private javax.ws.rs.core.StreamingOutput executeStatementsAndCommit(final java.io.InputStream input, final org.Neo4Net.server.rest.transactional.TransactionHandle transactionHandle, final java.net.URI baseUri, final javax.servlet.http.HttpServletRequest request)
		 private StreamingOutput ExecuteStatementsAndCommit( Stream input, TransactionHandle transactionHandle, URI baseUri, HttpServletRequest request )
		 {
			  return output =>
			  {
				Stream wrappedOutput = transactionHandle.Implicit ? new InterruptingOutputStream( this, output, transactionHandle ) : output;
				transactionHandle.Commit( _facade.deserializer( input ), _facade.serializer( wrappedOutput, baseUri ), request );
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private javax.ws.rs.core.StreamingOutput rollback(final org.Neo4Net.server.rest.transactional.TransactionHandle transactionHandle, final java.net.URI baseUri)
		 private StreamingOutput Rollback( TransactionHandle transactionHandle, URI baseUri )
		 {
			  return output =>
			  {
				if ( transactionHandle != null )
				{
					 transactionHandle.Rollback( _facade.serializer( output, baseUri ) );
				}
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private javax.ws.rs.core.StreamingOutput serializeError(final org.Neo4Net.server.rest.transactional.error.Neo4NetError Neo4NetError, final java.net.URI baseUri)
		 private StreamingOutput SerializeError( Neo4NetError Neo4NetError, URI baseUri )
		 {
			  return output =>
			  {
				ExecutionResultSerializer serializer = _facade.serializer( output, baseUri );
				serializer.errors( Collections.singletonList( Neo4NetError ) );
				serializer.finish();
			  };
		 }

		 public class TransactionUriBuilder : TransactionUriScheme
		 {
			  internal readonly UriInfo UriInfo;

			  public TransactionUriBuilder( UriInfo uriInfo )
			  {
					this.UriInfo = uriInfo;
			  }

			  public override URI TxUri( long id )
			  {
					return Builder( id ).build();
			  }

			  public override URI TxCommitUri( long id )
			  {
					return Builder( id ).path( "/commit" ).build();
			  }

			  internal virtual UriBuilder Builder( long id )
			  {
					return UriInfo.BaseUriBuilder.path( typeof( TransactionalService ) ).path( "/" + id );
			  }
		 }

		 private class InterruptingOutputStream : Stream
		 {
			 private readonly TransactionalService _outerInstance;

			  internal readonly Stream Delegate;
			  internal readonly TransactionTerminationHandle TerminationHandle;

			  internal InterruptingOutputStream( TransactionalService outerInstance, Stream @delegate, TransactionTerminationHandle terminationHandle )
			  {
				  this._outerInstance = outerInstance;
					this.Delegate = @delegate;
					this.TerminationHandle = terminationHandle;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(byte[] b) throws java.io.IOException
			  public override void Write( sbyte[] b )
			  {
					try
					{
						 Delegate.Write( b, 0, b.Length );
					}
					catch ( IOException e )
					{
						 Terminate();
						 throw e;
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(byte[] b, int off, int len) throws java.io.IOException
			  public override void Write( sbyte[] b, int off, int len )
			  {
					try
					{
						 Delegate.Write( b, off, len );
					}
					catch ( IOException e )
					{
						 Terminate();
						 throw e;
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flush() throws java.io.IOException
			  public override void Flush()
			  {
					try
					{
						 Delegate.Flush();
					}
					catch ( IOException e )
					{
						 Terminate();
						 throw e;
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
			  public override void Close()
			  {
					try
					{
						 Delegate.Close();
					}
					catch ( IOException e )
					{
						 Terminate();
						 throw e;
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(int b) throws java.io.IOException
			  public override void Write( int b )
			  {
					try
					{
						 Delegate.WriteByte( b );
					}
					catch ( IOException e )
					{
						 Terminate();
						 throw e;
					}
			  }

			  internal virtual void Terminate()
			  {
					TerminationHandle.terminate();
			  }
		 }
	}

}