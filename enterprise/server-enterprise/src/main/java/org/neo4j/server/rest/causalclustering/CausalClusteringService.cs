/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.Server.rest.causalclustering
{

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using AdvertisableService = Org.Neo4j.Server.rest.management.AdvertisableService;
	using OutputFormat = Org.Neo4j.Server.rest.repr.OutputFormat;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Path(CausalClusteringService.BASE_PATH) public class CausalClusteringService implements org.neo4j.server.rest.management.AdvertisableService
	public class CausalClusteringService : AdvertisableService
	{
		 internal const string BASE_PATH = "server/causalclustering/";

		 internal const string AVAILABLE = "available";
		 internal const string WRITABLE = "writable";
		 internal const string READ_ONLY = "read-only";
		 internal const string DESCRIPTION = "status";

		 private readonly CausalClusteringStatus _status;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public CausalClusteringService(@Context OutputFormat output, @Context GraphDatabaseService db)
		 public CausalClusteringService( OutputFormat output, GraphDatabaseService db )
		 {
			  this._status = CausalClusteringStatusFactory.Build( output, db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET public javax.ws.rs.core.Response discover()
		 public virtual Response Discover()
		 {
			  return _status.discover();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(WRITABLE) public javax.ws.rs.core.Response isWritable()
		 public virtual Response Writable
		 {
			 get
			 {
				  return _status.writable();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(READ_ONLY) public javax.ws.rs.core.Response isReadOnly()
		 public virtual Response ReadOnly
		 {
			 get
			 {
				  return _status.@readonly();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(AVAILABLE) public javax.ws.rs.core.Response isAvailable()
		 public virtual Response Available
		 {
			 get
			 {
				  return _status.available();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(DESCRIPTION) public javax.ws.rs.core.Response status()
		 public virtual Response Status()
		 {
			  return _status.description();
		 }

		 public virtual string Name
		 {
			 get
			 {
				  return "causalclustering";
			 }
		 }

		 public virtual string ServerPath
		 {
			 get
			 {
				  return BASE_PATH;
			 }
		 }
	}

}