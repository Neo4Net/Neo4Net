using System;
using System.Collections.Generic;

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
namespace Neo4Net.Server.security.enterprise.auth
{

	using AuthorizationViolationException = Neo4Net.Graphdb.security.AuthorizationViolationException;
	using AuthSubject = Neo4Net.Internal.Kernel.Api.security.AuthSubject;
	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using User = Neo4Net.Kernel.impl.security.User;
	using PredefinedRoles = Neo4Net.Server.security.enterprise.auth.plugin.api.PredefinedRoles;
	using SecurityLog = Neo4Net.Server.security.enterprise.log.SecurityLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.security.AuthorizationViolationException.PERMISSION_DENIED;

	internal class PersonalUserManager : EnterpriseUserManager
	{
		 private readonly EnterpriseUserManager _userManager;
		 private readonly SecurityLog _securityLog;
		 private readonly AuthSubject _subject;
		 private readonly bool _isUserManager;

		 internal PersonalUserManager( EnterpriseUserManager userManager, AuthSubject subject, SecurityLog securityLog, bool isUserManager )
		 {
			  this._userManager = userManager;
			  this._securityLog = securityLog;
			  this._subject = subject;
			  this._isUserManager = isUserManager;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.impl.security.User newUser(String username, byte[] initialPassword, boolean requirePasswordChange) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException, org.neo4j.graphdb.security.AuthorizationViolationException
		 public override User NewUser( string username, sbyte[] initialPassword, bool requirePasswordChange )
		 {
			  try
			  {
					AssertUserManager();
					User user = _userManager.newUser( username, initialPassword, requirePasswordChange );
					_securityLog.info( _subject, "created user `%s`%s", username, requirePasswordChange ? ", with password change required" : "" );
					return user;
			  }
			  catch ( Exception e ) when ( e is AuthorizationViolationException || e is IOException || e is InvalidArgumentsException )
			  {
					_securityLog.error( _subject, "tried to create user `%s`: %s", username, e.Message );
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void suspendUser(String username) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException, org.neo4j.graphdb.security.AuthorizationViolationException
		 public override void SuspendUser( string username )
		 {
			  try
			  {
					AssertUserManager();
					if ( _subject.hasUsername( username ) )
					{
						 throw new InvalidArgumentsException( "Suspending yourself (user '" + username + "') is not allowed." );
					}
					_userManager.suspendUser( username );
					_securityLog.info( _subject, "suspended user `%s`", username );
			  }
			  catch ( Exception e ) when ( e is AuthorizationViolationException || e is IOException || e is InvalidArgumentsException )
			  {
					_securityLog.error( _subject, "tried to suspend user `%s`: %s", username, e.Message );
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean deleteUser(String username) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException, org.neo4j.graphdb.security.AuthorizationViolationException
		 public override bool DeleteUser( string username )
		 {
			  try
			  {
					AssertUserManager();
					if ( _subject.hasUsername( username ) )
					{
						 throw new InvalidArgumentsException( "Deleting yourself (user '" + username + "') is not allowed." );
					}
					bool wasDeleted = _userManager.deleteUser( username );
					_securityLog.info( _subject, "deleted user `%s`", username );
					return wasDeleted;
			  }
			  catch ( Exception e ) when ( e is AuthorizationViolationException || e is IOException || e is InvalidArgumentsException )
			  {
					_securityLog.error( _subject, "tried to delete user `%s`: %s", username, e.Message );
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void activateUser(String username, boolean requirePasswordChange) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException, org.neo4j.graphdb.security.AuthorizationViolationException
		 public override void ActivateUser( string username, bool requirePasswordChange )
		 {
			  try
			  {
					AssertUserManager();
					if ( _subject.hasUsername( username ) )
					{
						 throw new InvalidArgumentsException( "Activating yourself (user '" + username + "') is not allowed." );
					}
					_userManager.activateUser( username, requirePasswordChange );
					_securityLog.info( _subject, "activated user `%s`", username );
			  }
			  catch ( Exception e ) when ( e is AuthorizationViolationException || e is IOException || e is InvalidArgumentsException )
			  {
					_securityLog.error( _subject, "tried to activate user `%s`: %s", username, e.Message );
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.impl.security.User getUser(String username) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 public override User GetUser( string username )
		 {
			  return _userManager.getUser( username );
		 }

		 public override User SilentlyGetUser( string username )
		 {
			  return _userManager.silentlyGetUser( username );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void newRole(String roleName, String... usernames) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException, org.neo4j.graphdb.security.AuthorizationViolationException
		 public override void NewRole( string roleName, params string[] usernames )
		 {
			  try
			  {
					AssertUserManager();
					_userManager.newRole( roleName, usernames );
					_securityLog.info( _subject, "created role `%s`", roleName );
			  }
			  catch ( Exception e ) when ( e is AuthorizationViolationException || e is IOException || e is InvalidArgumentsException )
			  {
					_securityLog.error( _subject, "tried to create role `%s`: %s", roleName, e.Message );
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean deleteRole(String roleName) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException, org.neo4j.graphdb.security.AuthorizationViolationException
		 public override bool DeleteRole( string roleName )
		 {
			  try
			  {
					AssertUserManager();
					bool wasDeleted = _userManager.deleteRole( roleName );
					_securityLog.info( _subject, "deleted role `%s`", roleName );
					return wasDeleted;
			  }
			  catch ( Exception e ) when ( e is AuthorizationViolationException || e is IOException || e is InvalidArgumentsException )
			  {
					_securityLog.error( _subject, "tried to delete role `%s`: %s", roleName, e.Message );
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setUserPassword(String username, byte[] password, boolean requirePasswordChange) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException, org.neo4j.graphdb.security.AuthorizationViolationException
		 public override void SetUserPassword( string username, sbyte[] password, bool requirePasswordChange )
		 {
			  if ( _subject.hasUsername( username ) )
			  {
					try
					{
						 _userManager.setUserPassword( username, password, requirePasswordChange );
						 _securityLog.info( _subject, "changed password%s", requirePasswordChange ? ", with password change required" : "" );
					}
					catch ( Exception e ) when ( e is AuthorizationViolationException || e is IOException || e is InvalidArgumentsException )
					{
						 _securityLog.error( _subject, "tried to change password: %s", e.Message );
						 throw e;
					}
			  }
			  else
			  {
					try
					{
						 AssertUserManager();
						 _userManager.setUserPassword( username, password, requirePasswordChange );
						 _securityLog.info( _subject, "changed password for user `%s`%s", username, requirePasswordChange ? ", with password change required" : "" );
					}
					catch ( Exception e ) when ( e is AuthorizationViolationException || e is IOException || e is InvalidArgumentsException )
					{
						 _securityLog.error( _subject, "tried to change password for user `%s`: %s", username, e.Message );
						 throw e;
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Set<String> getAllUsernames() throws org.neo4j.graphdb.security.AuthorizationViolationException
		 public virtual ISet<string> AllUsernames
		 {
			 get
			 {
				  try
				  {
						AssertUserManager();
						return _userManager.AllUsernames;
				  }
				  catch ( AuthorizationViolationException e )
				  {
						_securityLog.error( _subject, "tried to list users: %s", e.Message );
						throw e;
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void assertRoleExists(String roleName) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 public override void AssertRoleExists( string roleName )
		 {
			  _userManager.assertRoleExists( roleName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void addRoleToUser(String roleName, String username) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException, org.neo4j.graphdb.security.AuthorizationViolationException
		 public override void AddRoleToUser( string roleName, string username )
		 {
			  try
			  {
					AssertUserManager();
					_userManager.addRoleToUser( roleName, username );
					_securityLog.info( _subject, "added role `%s` to user `%s`", roleName, username );
			  }
			  catch ( Exception e ) when ( e is AuthorizationViolationException || e is IOException || e is InvalidArgumentsException )
			  {
					_securityLog.error( _subject, "tried to add role `%s` to user `%s`: %s", roleName, username, e.Message );
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void removeRoleFromUser(String roleName, String username) throws java.io.IOException, org.neo4j.kernel.api.exceptions.InvalidArgumentsException, org.neo4j.graphdb.security.AuthorizationViolationException
		 public override void RemoveRoleFromUser( string roleName, string username )
		 {
			  try
			  {
					AssertUserManager();
					if ( _subject.hasUsername( username ) && roleName.Equals( PredefinedRoles.ADMIN ) )
					{
						 throw new InvalidArgumentsException( "Removing yourself (user '" + username + "') from the admin role is not allowed." );
					}
					_userManager.removeRoleFromUser( roleName, username );
					_securityLog.info( _subject, "removed role `%s` from user `%s`", roleName, username );
			  }
			  catch ( Exception e ) when ( e is AuthorizationViolationException || e is IOException || e is InvalidArgumentsException )
			  {
					_securityLog.error( _subject, "tried to remove role `%s` from user `%s`: %s", roleName, username, e.Message );
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Set<String> getAllRoleNames() throws org.neo4j.graphdb.security.AuthorizationViolationException
		 public virtual ISet<string> AllRoleNames
		 {
			 get
			 {
				  try
				  {
						AssertUserManager();
						return _userManager.AllRoleNames;
				  }
				  catch ( AuthorizationViolationException e )
				  {
						_securityLog.error( _subject, "tried to list roles: %s", e.Message );
						throw e;
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Set<String> getRoleNamesForUser(String username) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException, org.neo4j.graphdb.security.AuthorizationViolationException
		 public override ISet<string> GetRoleNamesForUser( string username )
		 {
			  try
			  {
					AssertSelfOrUserManager( username );
					return _userManager.getRoleNamesForUser( username );
			  }
			  catch ( Exception e ) when ( e is AuthorizationViolationException || e is InvalidArgumentsException )
			  {
					_securityLog.error( _subject, "tried to list roles for user `%s`: %s", username, e.Message );
					throw e;
			  }
		 }

		 public override ISet<string> SilentlyGetRoleNamesForUser( string username )
		 {
			  return _userManager.silentlyGetRoleNamesForUser( username );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Set<String> getUsernamesForRole(String roleName) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException, org.neo4j.graphdb.security.AuthorizationViolationException
		 public override ISet<string> GetUsernamesForRole( string roleName )
		 {
			  try
			  {
					AssertUserManager();
					return _userManager.getUsernamesForRole( roleName );
			  }
			  catch ( Exception e ) when ( e is AuthorizationViolationException || e is InvalidArgumentsException )
			  {
					_securityLog.error( _subject, "tried to list users for role `%s`: %s", roleName, e.Message );
					throw e;
			  }
		 }

		 public override ISet<string> SilentlyGetUsernamesForRole( string roleName )
		 {
			  return _userManager.silentlyGetUsernamesForRole( roleName );
		 }

		 private void AssertSelfOrUserManager( string username )
		 {
			  if ( !_subject.hasUsername( username ) )
			  {
					AssertUserManager();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertUserManager() throws org.neo4j.graphdb.security.AuthorizationViolationException
		 private void AssertUserManager()
		 {
			  if ( !_isUserManager )
			  {
					throw new AuthorizationViolationException( PERMISSION_DENIED );
			  }
		 }
	}

}