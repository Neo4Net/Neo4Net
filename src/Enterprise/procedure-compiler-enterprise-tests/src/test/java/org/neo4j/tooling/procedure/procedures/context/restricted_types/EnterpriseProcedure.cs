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
namespace Neo4Net.Tooling.procedure.procedures.context.restricted_types
{

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using EnterpriseAuthManager = Neo4Net.Kernel.enterprise.api.security.EnterpriseAuthManager;
	using Context = Neo4Net.Procedure.Context;
	using Procedure = Neo4Net.Procedure.Procedure;
	using SecurityLog = Neo4Net.Server.security.enterprise.log.SecurityLog;

	public class EnterpriseProcedure
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.graphdb.GraphDatabaseService graphDatabaseService;
		 public GraphDatabaseService GraphDatabaseService;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.kernel.enterprise.api.security.EnterpriseAuthManager enterpriseAuthManager;
		 public EnterpriseAuthManager EnterpriseAuthManager;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.server.security.enterprise.log.SecurityLog securityLog;
		 public SecurityLog SecurityLog;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyRecord> procedure()
		 public virtual Stream<MyRecord> Procedure()
		 {
			  return Stream.empty();
		 }

		 public class MyRecord
		 {
			  public string Property;
		 }
	}

}