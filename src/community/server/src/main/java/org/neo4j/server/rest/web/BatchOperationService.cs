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
namespace Neo4Net.Server.rest.web
{
	using Log = org.eclipse.jetty.util.log.Log;
	using Logger = org.eclipse.jetty.util.log.Logger;


	using BatchOperationResults = Neo4Net.Server.rest.batch.BatchOperationResults;
	using NonStreamingBatchOperations = Neo4Net.Server.rest.batch.NonStreamingBatchOperations;
	using OutputFormat = Neo4Net.Server.rest.repr.OutputFormat;
	using RepresentationWriteHandler = Neo4Net.Server.rest.repr.RepresentationWriteHandler;
	using StreamingFormat = Neo4Net.Server.rest.repr.StreamingFormat;
	using HttpHeaderUtils = Neo4Net.Server.web.HttpHeaderUtils;
	using WebServer = Neo4Net.Server.web.WebServer;
	using UsageData = Neo4Net.Udc.UsageData;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.udc.UsageDataKeys.Features_Fields.http_batch_endpoint;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.udc.UsageDataKeys.features;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Path("/batch") public class BatchOperationService
	public class BatchOperationService
	{

		 private static readonly Logger _logger = Log.getLogger( typeof( BatchOperationService ) );

		 private readonly OutputFormat _output;
		 private readonly WebServer _webServer;
		 private readonly UsageData _usage;
		 private RepresentationWriteHandler _representationWriteHandler = RepresentationWriteHandler.DO_NOTHING;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public BatchOperationService(@Context WebServer webServer, @Context OutputFormat output, @Context UsageData usage)
		 public BatchOperationService( WebServer webServer, OutputFormat output, UsageData usage )
		 {
			  this._output = output;
			  this._webServer = webServer;
			  this._usage = usage;
		 }

		 public virtual RepresentationWriteHandler RepresentationWriteHandler
		 {
			 set
			 {
				  this._representationWriteHandler = value;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST public javax.ws.rs.core.Response performBatchOperations(@Context UriInfo uriInfo, @Context HttpHeaders httpHeaders, @Context HttpServletRequest req, java.io.InputStream body)
		 public virtual Response PerformBatchOperations( UriInfo uriInfo, HttpHeaders httpHeaders, HttpServletRequest req, Stream body )
		 {
			  _usage.get( features ).flag( http_batch_endpoint );
			  if ( IsStreaming( httpHeaders ) )
			  {
					return BatchProcessAndStream( uriInfo, httpHeaders, req, body );
			  }
			  return BatchProcess( uriInfo, httpHeaders, req, body );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private javax.ws.rs.core.Response batchProcessAndStream(final javax.ws.rs.core.UriInfo uriInfo, final javax.ws.rs.core.HttpHeaders httpHeaders, final javax.servlet.http.HttpServletRequest req, final java.io.InputStream body)
		 private Response BatchProcessAndStream( UriInfo uriInfo, HttpHeaders httpHeaders, HttpServletRequest req, Stream body )
		 {
			  try
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.ws.rs.core.StreamingOutput stream = output ->
					StreamingOutput stream = _output =>
					{
					 try
					 {
						  ServletOutputStream servletOutputStream = new ServletOutputStreamAnonymousInnerClass( this );
						  ( new StreamingBatchOperations( _webServer ) ).readAndExecuteOperations( uriInfo, httpHeaders, req, body, servletOutputStream );
						  _representationWriteHandler.onRepresentationWritten();
					 }
					 catch ( Exception e )
					 {
						  _logger.warn( "Error executing batch request ", e );
					 }
					 finally
					 {
						  _representationWriteHandler.onRepresentationFinal();
					 }
					};
					return Response.ok( stream ).type( HttpHeaderUtils.mediaTypeWithCharsetUtf8( MediaType.APPLICATION_JSON_TYPE ) ).build();
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
		 }

		 private class ServletOutputStreamAnonymousInnerClass : ServletOutputStream
		 {
			 private readonly BatchOperationService _outerInstance;

			 public ServletOutputStreamAnonymousInnerClass( BatchOperationService outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(int i) throws java.io.IOException
			 public override void write( int i )
			 {
				  _outerInstance.output.write( i );
			 }

			 public override bool Ready
			 {
				 get
				 {
					  return true;
				 }
			 }

			 public override WriteListener WriteListener
			 {
				 set
				 {
					  try
					  {
							value.onWritePossible();
					  }
					  catch ( IOException )
					  {
							// Ignore
					  }
				 }
			 }
		 }

		 private Response BatchProcess( UriInfo uriInfo, HttpHeaders httpHeaders, HttpServletRequest req, Stream body )
		 {
			  try
			  {
					NonStreamingBatchOperations batchOperations = new NonStreamingBatchOperations( _webServer );
					BatchOperationResults results = batchOperations.PerformBatchJobs( uriInfo, httpHeaders, req, body );

					Response res = Response.ok().entity(results.ToJSON()).type(HttpHeaderUtils.mediaTypeWithCharsetUtf8(MediaType.APPLICATION_JSON_TYPE)).build();
					_representationWriteHandler.onRepresentationWritten();
					return res;
			  }
			  catch ( Exception e )
			  {
					return _output.serverError( e );
			  }
			  finally
			  {
					_representationWriteHandler.onRepresentationFinal();
			  }
		 }

		 private bool IsStreaming( HttpHeaders httpHeaders )
		 {
			  if ( "true".Equals( httpHeaders.RequestHeaders.getFirst( Neo4Net.Server.rest.repr.StreamingFormat_Fields.STREAM_HEADER ), StringComparison.OrdinalIgnoreCase ) )
			  {
					return true;
			  }
			  foreach ( MediaType mediaType in httpHeaders.AcceptableMediaTypes )
			  {
					IDictionary<string, string> parameters = mediaType.Parameters;
					if ( parameters.ContainsKey( "stream" ) && "true".Equals( parameters["stream"], StringComparison.OrdinalIgnoreCase ) )
					{
						 return true;
					}
			  }
			  return false;
		 }
	}

}