﻿using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Server.security.enterprise.auth
{

	using KernelTransaction = Neo4Net.Kernel.Api.KernelTransaction;
	using KernelTransactionHandle = Neo4Net.Kernel.Api.KernelTransactionHandle;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using NetworkConnectionTracker = Neo4Net.Kernel.Api.net.NetworkConnectionTracker;
	using TrackedNetworkConnection = Neo4Net.Kernel.Api.net.TrackedNetworkConnection;
	using EnterpriseSecurityContext = Neo4Net.Kernel.enterprise.api.security.EnterpriseSecurityContext;
	using KernelTransactions = Neo4Net.Kernel.Impl.Api.KernelTransactions;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using User = Neo4Net.Kernel.impl.security.User;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Context = Neo4Net.Procedure.Context;
	using SecurityLog = Neo4Net.Server.security.enterprise.log.SecurityLog;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public class AuthProceduresBase
	public class AuthProceduresBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public Neo4Net.kernel.enterprise.api.security.EnterpriseSecurityContext securityContext;
		 public EnterpriseSecurityContext SecurityContext;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public Neo4Net.kernel.internal.GraphDatabaseAPI graph;
		 public GraphDatabaseAPI Graph;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public Neo4Net.server.security.enterprise.log.SecurityLog securityLog;
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
			  ActiveTransactions.Where( tx => tx.subject().hasUsername(username) && !tx.isUnderlyingTransaction(currentTx) ).ForEach(tx => tx.markForTermination(Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated));
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