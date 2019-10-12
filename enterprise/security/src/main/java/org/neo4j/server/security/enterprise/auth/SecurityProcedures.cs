using System;

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
namespace Org.Neo4j.Server.security.enterprise.auth
{

	using EnterpriseAuthManager = Org.Neo4j.Kernel.enterprise.api.security.EnterpriseAuthManager;
	using Admin = Org.Neo4j.Procedure.Admin;
	using Context = Org.Neo4j.Procedure.Context;
	using Description = Org.Neo4j.Procedure.Description;
	using Procedure = Org.Neo4j.Procedure.Procedure;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.procedure.Mode.DBMS;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"unused", "WeakerAccess"}) public class SecurityProcedures extends AuthProceduresBase
	public class SecurityProcedures : AuthProceduresBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.kernel.enterprise.api.security.EnterpriseAuthManager authManager;
		 public EnterpriseAuthManager AuthManager;

		 [Obsolete, Description("Show the current user. Deprecated by dbms.showCurrentUser."), Procedure(name : "dbms.security.showCurrentUser", mode : DBMS, deprecatedBy : "dbms.showCurrentUser")]
		 public virtual Stream<UserManagementProcedures.UserResult> ShowCurrentUserDeprecated()
		 {
			  return ShowCurrentUser();
		 }

		 [Description("Show the current user."), Procedure(name : "dbms.showCurrentUser", mode : DBMS)]
		 public virtual Stream<UserManagementProcedures.UserResult> ShowCurrentUser()
		 {
			  return Stream.of( UserResultForSubject() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Admin @Description("Clears authentication and authorization cache.") @Procedure(name = "dbms.security.clearAuthCache", mode = DBMS) public void clearAuthenticationCache()
		 [Description("Clears authentication and authorization cache."), Procedure(name : "dbms.security.clearAuthCache", mode : DBMS)]
		 public virtual void ClearAuthenticationCache()
		 {
			  AuthManager.clearAuthCache();
		 }
	}

}