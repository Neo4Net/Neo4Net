using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Harness.extensionpackage
{
	using HttpStatus = org.eclipse.jetty.http.HttpStatus;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Result = Neo4Net.Graphdb.Result;
	using Transaction = Neo4Net.Graphdb.Transaction;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Path("myExtension") public class MyEnterpriseUnmanagedExtension
	public class MyEnterpriseUnmanagedExtension
	{
		 private readonly GraphDatabaseService _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public MyEnterpriseUnmanagedExtension(@Context GraphDatabaseService db)
		 public MyEnterpriseUnmanagedExtension( GraphDatabaseService db )
		 {
			  this._db = db;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path("doSomething") public javax.ws.rs.core.Response doSomething()
		 public virtual Response DoSomething()
		 {
			  return Response.status( 234 ).build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path("createConstraint") public javax.ws.rs.core.Response createProperty()
		 public virtual Response CreateProperty()
		 {
			  try
			  {
					  using ( Transaction tx = _db.beginTx() )
					  {
						using ( Result result = _db.execute( "CREATE CONSTRAINT ON (user:User) ASSERT exists(user.name)" ) )
						{
							 // nothing to-do
						}
						tx.Success();
						return Response.status( HttpStatus.CREATED_201 ).build();
					  }
			  }
			  catch ( Exception )
			  {
					return Response.status( HttpStatus.NOT_IMPLEMENTED_501 ).build();
			  }
		 }
	}

}