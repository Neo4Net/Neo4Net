using System.Collections.Generic;
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
namespace Org.Dummy.Web.Service
{

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Path("/") public class DummyThirdPartyWebService
	public class DummyThirdPartyWebService
	{

		 public const string DUMMY_WEB_SERVICE_MOUNT_POINT = "/dummy";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Produces(javax.ws.rs.core.MediaType.TEXT_PLAIN) public javax.ws.rs.core.Response sayHello()
		 public virtual Response SayHello()
		 {
			  return Response.ok().entity("hello").build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path("/{something}/{somethingElse}") @Produces(javax.ws.rs.core.MediaType.TEXT_PLAIN) public javax.ws.rs.core.Response forSecurityTesting()
		 public virtual Response ForSecurityTesting()
		 {
			  return Response.ok().entity("you've reached a dummy service").build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path("inject-test") @Produces(javax.ws.rs.core.MediaType.TEXT_PLAIN) public javax.ws.rs.core.Response countNodes(@Context GraphDatabaseService db)
		 public virtual Response CountNodes( GraphDatabaseService db )
		 {
			  using ( Transaction transaction = Db.beginTx() )
			  {
					return Response.ok().entity(CountNodesIn(db).ToString()).build();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path("needs-auth-header") @Produces(javax.ws.rs.core.MediaType.APPLICATION_JSON) public javax.ws.rs.core.Response authHeader(@Context HttpHeaders headers)
		 public virtual Response AuthHeader( HttpHeaders headers )
		 {
			  StringBuilder theEntity = new StringBuilder( "{" );
			  IEnumerator<KeyValuePair<string, IList<string>>> headerIt = headers.RequestHeaders.entrySet().GetEnumerator();
			  while ( headerIt.MoveNext() )
			  {
					KeyValuePair<string, IList<string>> header = headerIt.Current;
					if ( header.Value.size() != 1 )
					{
						 throw new System.ArgumentException( "Multivalued header: " + header.Key );
					}
					theEntity.Append( "\"" ).Append( header.Key ).Append( "\":\"" ).Append( header.Value.get( 0 ) ).Append( "\"" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( headerIt.hasNext() )
					{
						 theEntity.Append( ", " );
					}
			  }
			  theEntity.Append( "}" );
			  return Response.ok().entity(theEntity.ToString()).build();
		 }

		 private int CountNodesIn( GraphDatabaseService db )
		 {
			  int count = 0;
			  foreach ( Node ignore in Db.AllNodes )
			  {
					count++;
			  }
			  return count;
		 }
	}

}