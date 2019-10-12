using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
namespace Org.Neo4j.Server.rest.web
{
	using Log = org.eclipse.jetty.util.log.Log;
	using Logger = org.eclipse.jetty.util.log.Logger;


	using BatchOperations = Org.Neo4j.Server.rest.batch.BatchOperations;
	using StreamingBatchOperationResults = Org.Neo4j.Server.rest.batch.StreamingBatchOperationResults;
	using BatchOperationFailedException = Org.Neo4j.Server.rest.domain.BatchOperationFailedException;
	using StreamingJsonFormat = Org.Neo4j.Server.rest.repr.formats.StreamingJsonFormat;
	using WebServer = Org.Neo4j.Server.web.WebServer;

	public class StreamingBatchOperations : BatchOperations
	{

		 private static readonly Logger _logger = Log.getLogger( typeof( StreamingBatchOperations ) );
		 private StreamingBatchOperationResults _results;

		 public StreamingBatchOperations( WebServer webServer ) : base( webServer )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void readAndExecuteOperations(javax.ws.rs.core.UriInfo uriInfo, javax.ws.rs.core.HttpHeaders httpHeaders, javax.servlet.http.HttpServletRequest req, java.io.InputStream body, javax.servlet.ServletOutputStream output) throws java.io.IOException, javax.servlet.ServletException
		 public virtual void ReadAndExecuteOperations( UriInfo uriInfo, HttpHeaders httpHeaders, HttpServletRequest req, Stream body, ServletOutputStream output )
		 {
			  _results = new StreamingBatchOperationResults( JsonFactory.createJsonGenerator( output ), output );
			  IDictionary<int, string> locations = _results.Locations;
			  ParseAndPerform( uriInfo, httpHeaders, req, body, locations );
			  _results.close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void invoke(String method, String path, String body, System.Nullable<int> id, java.net.URI targetUri, InternalJettyServletRequest req, InternalJettyServletResponse res) throws java.io.IOException
		 protected internal override void Invoke( string method, string path, string body, int? id, URI targetUri, InternalJettyServletRequest req, InternalJettyServletResponse res )
		 {
			  _results.startOperation( path, id );
			  try
			  {
					res = new BatchInternalJettyServletResponse( _results.ServletOutputStream );
					WebServer.invokeDirectly( targetUri.Path, req, res );
			  }
			  catch ( Exception e )
			  {
					_logger.warn( e );
					_results.writeError( 500, e.Message );
					throw new BatchOperationFailedException( 500, e.Message, e );

			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int status = res.getStatus();
			  int status = res.Status;
			  if ( Is2XXStatusCode( status ) )
			  {
					_results.addOperationResult( status, id, res.GetHeader( "Location" ) );
			  }
			  else
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String message = "Error " + status + " executing batch operation: " + ((id != null) ? id + ". " : "") + method + " " + path + " " + body;
					string message = "Error " + status + " executing batch operation: " + ( ( id != null ) ? id + ". " : "" ) + method + " " + path + " " + body;
					_results.writeError( status, res.Reason );
					throw new BatchOperationFailedException( status, message, new Exception( res.Reason ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected void addHeaders(final InternalJettyServletRequest res, final javax.ws.rs.core.HttpHeaders httpHeaders)
		 protected internal override void AddHeaders( InternalJettyServletRequest res, HttpHeaders httpHeaders )
		 {
			  base.AddHeaders( res, httpHeaders );
			  res.AddHeader( StreamingJsonFormat.STREAM_HEADER, "true" );
		 }

		 private class BatchInternalJettyServletResponse : InternalJettyServletResponse
		 {
			  internal readonly ServletOutputStream Output;

			  internal BatchInternalJettyServletResponse( ServletOutputStream output )
			  {
					this.Output = output;
			  }

			  public override ServletOutputStream OutputStream
			  {
				  get
				  {
						return Output;
				  }
			  }

			  public override PrintWriter Writer
			  {
				  get
				  {
						return new PrintWriter( new StreamWriter( Output, Encoding.UTF8 ) );
				  }
			  }
		 }
	}

}