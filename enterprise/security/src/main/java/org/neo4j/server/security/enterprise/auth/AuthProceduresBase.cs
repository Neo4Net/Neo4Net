using System;
using System.Collections.Generic;

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

	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using KernelTransactionHandle = Org.Neo4j.Kernel.api.KernelTransactionHandle;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using NetworkConnectionTracker = Org.Neo4j.Kernel.api.net.NetworkConnectionTracker;
	using TrackedNetworkConnection = Org.Neo4j.Kernel.api.net.TrackedNetworkConnection;
	using EnterpriseSecurityContext = Org.Neo4j.Kernel.enterprise.api.security.EnterpriseSecurityContext;
	using KernelTransactions = Org.Neo4j.Kernel.Impl.Api.KernelTransactions;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using User = Org.Neo4j.Kernel.impl.security.User;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using Context = Org.Neo4j.Procedure.Context;
	using SecurityLog = Org.Neo4j.Server.security.enterprise.log.SecurityLog;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public class AuthProceduresBase
	public class AuthProceduresBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.kernel.enterprise.api.security.EnterpriseSecurityContext securityContext;
		 public EnterpriseSecurityContext SecurityContext;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.kernel.internal.GraphDatabaseAPI graph;
		 public GraphDatabaseAPI Graph;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.server.security.enterprise.log.SecurityLog securityLog;
		 public SecurityLog SecurityLog;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public EnterpriseUserManager userManager;
		 public EnterpriseUserManager UserManager;

		 // ----------------- helpers ---------------------

		 protected internal virtual void KickoutUser( string username, string reason )
		 {
			  try
			  {
					TerminateTransactionsForValidUser( username );
					TerminateConnectionsForValidUser( username );
			  }
			  catch ( Exception e )
			  {
					SecurityLog.error( SecurityContext.subject(), "failed to terminate running transaction and bolt connections for " + "user `%s` following %s: %s", username, reason, e.Message );
					throw e;
			  }
		 }

		 protected internal virtual void TerminateTransactionsForValidUser( string username )
		 {
			  KernelTransaction currentTx = CurrentTx;
			  ActiveTransactions.Where( tx => tx.subject().hasUsername(username) && !tx.isUnderlyingTransaction(currentTx) ).ForEach(tx => tx.markForTermination(Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.Terminated));
		 }

		 protected internal virtual void TerminateConnectionsForValidUser( string username )
		 {
			  NetworkConnectionTracker connectionTracker = Graph.DependencyResolver.resolveDependency( typeof( NetworkConnectionTracker ) );
			  connectionTracker.ActiveConnections().Where(connection => Objects.Equals(username, connection.username())).ForEach(TrackedNetworkConnection.close);
		 }

		 private ISet<KernelTransactionHandle> ActiveTransactions
		 {
			 get
			 {
				  return Graph.DependencyResolver.resolveDependency( typeof( KernelTransactions ) ).activeTransactions();
			 }
		 }

		 private KernelTransaction CurrentTx
		 {
			 get
			 {
				  return Graph.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( true );
			 }
		 }

		 public class StringResult
		 {
			  public readonly string Value;

			  internal StringResult( string value )
			  {
					this.Value = value;
			  }
		 }

		 protected internal virtual UserResult UserResultForSubject()
		 {
			  string username = SecurityContext.subject().username();
			  User user = UserManager.silentlyGetUser( username );
			  IEnumerable<string> flags = user == null ? emptyList() : user.Flags;
			  return new UserResult( username, SecurityContext.roles(), flags );
		 }

		 protected internal virtual UserResult UserResultForName( string username )
		 {
			  if ( username.Equals( SecurityContext.subject().username() ) )
			  {
					return UserResultForSubject();
			  }
			  else
			  {
					User user = UserManager.silentlyGetUser( username );
					IEnumerable<string> flags = user == null ? emptyList() : user.Flags;
					ISet<string> roles = UserManager.silentlyGetRoleNamesForUser( username );
					return new UserResult( username, roles, flags );
			  }
		 }

		 public class UserResult
		 {
			  public readonly string Username;
			  public readonly IList<string> Roles;
			  public readonly IList<string> Flags;

			  internal UserResult( string username, ISet<string> roles, IEnumerable<string> flags )
			  {
					this.Username = username;
					this.Roles = new List<string>();
					( ( IList<string> )this.Roles ).AddRange( roles );
					this.Flags = new List<string>();
					foreach ( string f in flags )
					{
						 this.Flags.Add( f );
					}
			  }
		 }

		 protected internal virtual RoleResult RoleResultForName( string roleName )
		 {
			  return new RoleResult( roleName, UserManager.silentlyGetUsernamesForRole( roleName ) );
		 }

		 public class RoleResult
		 {
			  public readonly string Role;
			  public readonly IList<string> Users;

			  internal RoleResult( string role, ISet<string> users )
			  {
					this.Role = role;
					this.Users = new List<string>();
					( ( IList<string> )this.Users ).AddRange( users );
			  }
		 }
	}

}