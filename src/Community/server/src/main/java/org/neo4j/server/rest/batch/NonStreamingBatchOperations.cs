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
namespace Neo4Net.Server.rest.batch
{

	using BatchOperationFailedException = Neo4Net.Server.rest.domain.BatchOperationFailedException;
	using InternalJettyServletRequest = Neo4Net.Server.rest.web.InternalJettyServletRequest;
	using InternalJettyServletResponse = Neo4Net.Server.rest.web.InternalJettyServletResponse;
	using WebServer = Neo4Net.Server.web.WebServer;

	public class NonStreamingBatchOperations : BatchOperations
	{

		 private BatchOperationResults _results;

		 public NonStreamingBatchOperations( WebServer webServer ) : base( webServer )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public BatchOperationResults performBatchJobs(javax.ws.rs.core.UriInfo uriInfo, javax.ws.rs.core.HttpHeaders httpHeaders, javax.servlet.http.HttpServletRequest req, java.io.InputStream body) throws java.io.IOException, javax.servlet.ServletException
		 public virtual BatchOperationResults PerformBatchJobs( UriInfo uriInfo, HttpHeaders httpHeaders, HttpServletRequest req, Stream body )
		 {
			  _results = new BatchOperationResults();
			  ParseAndPerform( uriInfo, httpHeaders, req, body, _results.Locations );
			  return _results;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void invoke(String method, String path, String body, System.Nullable<int> id, java.net.URI targetUri, org.neo4j.server.rest.web.InternalJettyServletRequest req, org.neo4j.server.rest.web.InternalJettyServletResponse res) throws java.io.IOException, javax.servlet.ServletException
		 protected internal override void Invoke( string method, string path, string body, int? id, URI targetUri, InternalJettyServletRequest req, InternalJettyServletResponse res )
		 {
			  WebServer.invokeDirectly( targetUri.Path, req, res );

			  string resultBody = res.OutputStream.ToString();
			  if ( Is2XXStatusCode( res.Status ) )
			  {
					_results.addOperationResult( path, id, resultBody, res.GetHeader( "Location" ) );
			  }
			  else
			  {
					throw new BatchOperationFailedException( res.Status, resultBody, null );
			  }
		 }

	}

}