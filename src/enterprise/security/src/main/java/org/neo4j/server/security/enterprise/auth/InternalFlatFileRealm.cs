using System;
using System.Collections.Generic;
using System.Threading;

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
	using AuthenticationException = org.apache.shiro.authc.AuthenticationException;
	using AuthenticationInfo = org.apache.shiro.authc.AuthenticationInfo;
	using AuthenticationToken = org.apache.shiro.authc.AuthenticationToken;
	using DisabledAccountException = org.apache.shiro.authc.DisabledAccountException;
	using ExcessiveAttemptsException = org.apache.shiro.authc.ExcessiveAttemptsException;
	using IncorrectCredentialsException = org.apache.shiro.authc.IncorrectCredentialsException;
	using UnknownAccountException = org.apache.shiro.authc.UnknownAccountException;
	using AllowAllCredentialsMatcher = org.apache.shiro.authc.credential.AllowAllCredentialsMatcher;
	using UnsupportedTokenException = org.apache.shiro.authc.pam.UnsupportedTokenException;
	using AuthorizationInfo = org.apache.shiro.authz.AuthorizationInfo;
	using SimpleAuthorizationInfo = org.apache.shiro.authz.SimpleAuthorizationInfo;
	using AuthorizingRealm = org.apache.shiro.realm.AuthorizingRealm;
	using PrincipalCollection = org.apache.shiro.subject.PrincipalCollection;
	using SimplePrincipalCollection = org.apache.shiro.subject.SimplePrincipalCollection;


	using SetDefaultAdminCommand = Neo4Net.CommandLine.Admin.security.SetDefaultAdminCommand;
	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;
	using AuthToken = Neo4Net.Kernel.api.security.AuthToken;
	using AuthenticationResult = Neo4Net.Kernel.Api.Internal.security.AuthenticationResult;
	using PasswordPolicy = Neo4Net.Kernel.api.security.PasswordPolicy;
	using InvalidAuthTokenException = Neo4Net.Kernel.api.security.exception.InvalidAuthTokenException;
	using LegacyCredential = Neo4Net.Server.Security.Auth.LegacyCredential;
	using User = Neo4Net.Kernel.impl.security.User;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using AuthenticationStrategy = Neo4Net.Server.Security.Auth.AuthenticationStrategy;
	using Neo4Net.Server.Security.Auth;
	using UserRepository = Neo4Net.Server.Security.Auth.UserRepository;
	using ConcurrentModificationException = Neo4Net.Server.Security.Auth.exception.ConcurrentModificationException;
	using PredefinedRoles = Neo4Net.Server.security.enterprise.auth.plugin.api.PredefinedRoles;
	using SecuritySettings = Neo4Net.Server.security.enterprise.configuration.SecuritySettings;
	using UTF8 = Neo4Net.Strings.UTF8;


	/// <summary>
	/// Shiro realm wrapping FileUserRepository and FileRoleRepository </summary>
	/// @deprecated This class will be removed in the next major release. Please consider using SystemGraphRealm instead. 
	[Obsolete("This class will be removed in the next major release. Please consider using SystemGraphRealm instead.")]
	public class InternalFlatFileRealm : AuthorizingRealm, RealmLifecycle, EnterpriseUserManager, ShiroAuthorizationInfoProvider
	{
		 /// <summary>
		 /// This flag is used in the same way as User.PASSWORD_CHANGE_REQUIRED, but it's
		 /// placed here because of user suspension not being a part of community edition
		 /// </summary>
		 internal const string IS_SUSPENDED = "is_suspended";

		 private static int _maxReadAttempts = 10;

		 private readonly UserRepository _userRepository;
		 private readonly RoleRepository _roleRepository;
		 private readonly UserRepository _initialUserRepository;
		 private readonly UserRepository _defaultAdminRepository;
		 private readonly PasswordPolicy _passwordPolicy;
		 private readonly AuthenticationStrategy _authenticationStrategy;
		 private readonly bool _authenticationEnabled;
		 private readonly bool _authorizationEnabled;
		 private readonly IJobScheduler _jobScheduler;
		 private volatile JobHandle _reloadJobHandle;

		 [Obsolete]
		 public InternalFlatFileRealm( UserRepository userRepository, RoleRepository roleRepository, PasswordPolicy passwordPolicy, AuthenticationStrategy authenticationStrategy, IJobScheduler jobScheduler, UserRepository initialUserRepository, UserRepository defaultAdminRepository ) : this( userRepository,roleRepository, passwordPolicy, authenticationStrategy, true, true, jobScheduler, initialUserRepository, defaultAdminRepository )
		 {
		 }

		 internal InternalFlatFileRealm( UserRepository userRepository, RoleRepository roleRepository, PasswordPolicy passwordPolicy, AuthenticationStrategy authenticationStrategy, bool authenticationEnabled, bool authorizationEnabled, IJobScheduler jobScheduler, UserRepository initialUserRepository, UserRepository defaultAdminRepository ) : base()
		 {

			  Name = SecuritySettings.NATIVE_REALM_NAME;
			  this._userRepository = userRepository;
			  this._roleRepository = roleRepository;
			  this._initialUserRepository = initialUserRepository;
			  this._defaultAdminRepository = defaultAdminRepository;
			  this._passwordPolicy = passwordPolicy;
			  this._authenticationStrategy = authenticationStrategy;
			  this._authenticationEnabled = authenticationEnabled;
			  this._authorizationEnabled = authorizationEnabled;
			  this._jobScheduler = jobScheduler;
			  AuthenticationCachingEnabled = false; // NOTE: If this is ever changed to true it is not secure to use
																	  // AllowAllCredentialsMatcher anymore
			  AuthorizationCachingEnabled = false;
			  CredentialsMatcher = new AllowAllCredentialsMatcher(); // Since we do not cache authentication info we can
																							 // disable the credentials matcher
			  RolePermissionResolver = PredefinedRolesBuilder.RolePermissionResolver;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void initialize() throws Throwable
		 [Obsolete]
		 public override void Initialize()
		 {
			  _initialUserRepository.init();
			  _defaultAdminRepository.init();
			  _userRepository.init();
			  _roleRepository.init();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 [Obsolete]
		 public override void Start()
		 {
			  _initialUserRepository.start();
			  _defaultAdminRepository.start();
			  _userRepository.start();
			  _roleRepository.start();

			  ISet<string> addedDefaultUsers = EnsureDefaultUsers();
			  EnsureDefaultRoles( addedDefaultUsers );

			  ScheduleNextFileReload();
		 }

		 protected internal virtual void ScheduleNextFileReload()
		 {
			  _reloadJobHandle = _jobScheduler.schedule( Group.NATIVE_SECURITY, this.readFilesFromDisk, 10, TimeUnit.SECONDS );
		 }

		 private void ReadFilesFromDisk()
		 {
			  try
			  {
					ReadFilesFromDisk( _maxReadAttempts, new LinkedList<string>() );
			  }
			  finally
			  {
					ScheduleNextFileReload();
			  }
		 }

		 private void ReadFilesFromDisk( int attemptLeft, IList<string> failures )
		 {
			  if ( attemptLeft < 0 )
			  {
					throw new Exception( "Unable to load valid flat file repositories! Attempts failed with:\n\t" + string.join( "\n\t", failures ) );
			  }

			  try
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean valid;
					bool valid;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean needsUpdate;
					bool needsUpdate;
					lock ( this )
					{
						 ListSnapshot<User> users = _userRepository.PersistedSnapshot;
						 ListSnapshot<RoleRecord> roles = _roleRepository.PersistedSnapshot;

						 needsUpdate = users.FromPersisted() || roles.FromPersisted();
						 valid = needsUpdate && RoleRepository.validate( users.Values(), roles.Values() );

						 if ( valid )
						 {
							  if ( users.FromPersisted() )
							  {
									_userRepository.Users = users;
							  }
							  if ( roles.FromPersisted() )
							  {
									_roleRepository.Roles = roles;
							  }
						 }
					}
					if ( needsUpdate && !valid )
					{
						 failures.Add( "Role-auth file combination not valid." );
						 Thread.Sleep( 10 );
						 ReadFilesFromDisk( attemptLeft - 1, failures );
					}
			  }
			  catch ( Exception e ) when ( e is IOException || e is System.InvalidOperationException || e is InterruptedException || e is InvalidArgumentsException )
			  {
					failures.Add( e.Message );
					ReadFilesFromDisk( attemptLeft - 1, failures );
			  }
		 }

		 /* Adds Neo4Net user if no users exist */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.Set<String> ensureDefaultUsers() throws Throwable
		 private ISet<string> EnsureDefaultUsers()
		 {
			  if ( _authenticationEnabled || _authorizationEnabled )
			  {
					if ( _userRepository.numberOfUsers() == 0 )
					{
						 User Neo4Net = NewUser( Neo4Net.Kernel.api.security.UserManager_Fields.INITIAL_USER_NAME, UTF8.encode( Neo4Net.Kernel.api.security.UserManager_Fields.INITIAL_PASSWORD ), true );
						 if ( _initialUserRepository.numberOfUsers() > 0 )
						 {
							  User initUser = _initialUserRepository.getUserByName( Neo4Net.Kernel.api.security.UserManager_Fields.INITIAL_USER_NAME );
							  if ( initUser != null )
							  {
									_userRepository.update( Neo4Net, initUser );
							  }
						 }
						 return Collections.singleton( Neo4Net.Kernel.api.security.UserManager_Fields.INITIAL_USER_NAME );
					}
			  }
			  return Collections.emptySet();
		 }

		 /* Builds all predefined roles if no roles exist. Adds 'Neo4Net' to admin role if no admin is assigned */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensureDefaultRoles(java.util.Set<String> addedDefaultUsers) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
		 private void EnsureDefaultRoles( ISet<string> addedDefaultUsers )
		 {
			  if ( _authenticationEnabled || _authorizationEnabled )
			  {
					IList<string> newAdmins = new LinkedList<string>( addedDefaultUsers );

					if ( NumberOfRoles() == 0 )
					{
						 if ( newAdmins.Count == 0 )
						 {
							  ISet<string> usernames = _userRepository.AllUsernames;
							  if ( _defaultAdminRepository.numberOfUsers() > 1 )
							  {
									throw new InvalidArgumentsException( "No roles defined, and multiple users defined as default admin user." + " Please use `Neo4Net-admin " + SetDefaultAdminCommand.COMMAND_NAME + "` to select a valid admin." );
							  }
							  else if ( _defaultAdminRepository.numberOfUsers() == 1 )
							  {
									// We currently support only one default admin
									string newAdminUsername = _defaultAdminRepository.AllUsernames.GetEnumerator().next();
									if ( _userRepository.getUserByName( newAdminUsername ) == null )
									{
										 throw new InvalidArgumentsException( "No roles defined, and default admin user '" + newAdminUsername + "' does not exist. Please use `Neo4Net-admin " + SetDefaultAdminCommand.COMMAND_NAME + "` to select a valid admin." );
									}
									newAdmins.Add( newAdminUsername );
							  }
							  else if ( usernames.Count == 1 )
							  {
									newAdmins.Add( usernames.GetEnumerator().next() );
							  }
							  else if ( usernames.Contains( Neo4Net.Kernel.api.security.UserManager_Fields.INITIAL_USER_NAME ) )
							  {
									newAdmins.Add( Neo4Net.Kernel.api.security.UserManager_Fields.INITIAL_USER_NAME );
							  }
							  else
							  {
									throw new InvalidArgumentsException( "No roles defined, and cannot determine which user should be admin. " + "Please use `Neo4Net-admin " + SetDefaultAdminCommand.COMMAND_NAME + "` to select an " + "admin." );
							  }
						 }

						 foreach ( string role in PredefinedRolesBuilder.Roles.Keys )
						 {
							  NewRole( role );
						 }
					}

					foreach ( string username in newAdmins )
					{
						 AddRoleToUser( PredefinedRoles.ADMIN, username );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 [Obsolete]
		 public override void Stop()
		 {
			  _initialUserRepository.stop();
			  _defaultAdminRepository.stop();
			  _userRepository.stop();
			  _roleRepository.stop();

			  if ( _reloadJobHandle != null )
			  {
					_reloadJobHandle.cancel( true );
					_reloadJobHandle = null;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws Throwable
		 [Obsolete]
		 public override void Shutdown()
		 {
			  _initialUserRepository.shutdown();
			  _defaultAdminRepository.shutdown();
			  _userRepository.shutdown();
			  _roleRepository.shutdown();
			  CacheManager = null;
		 }

		 [Obsolete]
		 public override bool Supports( AuthenticationToken token )
		 {
			  try
			  {
					if ( token is ShiroAuthToken )
					{
						 ShiroAuthToken shiroAuthToken = ( ShiroAuthToken ) token;
						 return shiroAuthToken.Scheme.Equals( Neo4Net.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME ) && ( shiroAuthToken.SupportsRealm( Neo4Net.Kernel.api.security.AuthToken_Fields.NATIVE_REALM ) );
					}
					return false;
			  }
			  catch ( InvalidAuthTokenException )
			  {
					return false;
			  }
		 }

		 protected internal override AuthorizationInfo DoGetAuthorizationInfo( PrincipalCollection principals )
		 {
			  if ( !_authorizationEnabled )
			  {
					return null;
			  }

			  string username = ( string ) getAvailablePrincipal( principals );
			  if ( string.ReferenceEquals( username, null ) )
			  {
					return null;
			  }

			  User user = _userRepository.getUserByName( username );
			  if ( user == null )
			  {
					return null;
			  }

			  if ( user.PasswordChangeRequired() || user.HasFlag(IS_SUSPENDED) )
			  {
					return new SimpleAuthorizationInfo();
			  }
			  else
			  {
					ISet<string> roles = _roleRepository.getRoleNamesByUsername( user.Name() );
					return new SimpleAuthorizationInfo( roles );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.apache.shiro.authc.AuthenticationInfo doGetAuthenticationInfo(org.apache.shiro.authc.AuthenticationToken token) throws org.apache.shiro.authc.AuthenticationException
		 protected internal override AuthenticationInfo DoGetAuthenticationInfo( AuthenticationToken token )
		 {
			  if ( !_authenticationEnabled )
			  {
					return null;
			  }

			  ShiroAuthToken shiroAuthToken = ( ShiroAuthToken ) token;

			  string username;
			  sbyte[] password;
			  try
			  {
					username = AuthToken.safeCast( Neo4Net.Kernel.api.security.AuthToken_Fields.PRINCIPAL, shiroAuthToken.AuthTokenMap );
					password = AuthToken.safeCastCredentials( Neo4Net.Kernel.api.security.AuthToken_Fields.CREDENTIALS, shiroAuthToken.AuthTokenMap );
			  }
			  catch ( InvalidAuthTokenException e )
			  {
					throw new UnsupportedTokenException( e );
			  }

			  User user = _userRepository.getUserByName( username );
			  if ( user == null )
			  {
					throw new UnknownAccountException();
			  }

			  AuthenticationResult result = _authenticationStrategy.authenticate( user, password );

			  switch ( result )
			  {
			  case AuthenticationResult.FAILURE:
					throw new IncorrectCredentialsException();
			  case AuthenticationResult.TOO_MANY_ATTEMPTS:
					throw new ExcessiveAttemptsException();
			  default:
					break;
			  }

			  if ( user.HasFlag( InternalFlatFileRealm.IS_SUSPENDED ) )
			  {
					throw new DisabledAccountException( "User '" + user.Name() + "' is suspended." );
			  }

			  if ( user.PasswordChangeRequired() )
			  {
					result = AuthenticationResult.PASSWORD_CHANGE_REQUIRED;
			  }

			  // NOTE: We do not cache the authentication info using the Shiro cache manager,
			  // so all authentication request will go through this method.
			  // Hence the credentials matcher is set to AllowAllCredentialsMatcher,
			  // and we do not need to store hashed credentials in the AuthenticationInfo.
			  return new ShiroAuthenticationInfo( user.Name(), Name, result );
		 }

		 [Obsolete]
		 public override AuthorizationInfo GetAuthorizationInfoSnapshot( PrincipalCollection principalCollection )
		 {
			  return getAuthorizationInfo( principalCollection );
		 }

		 private int NumberOfUsers()
		 {
			  return _userRepository.numberOfUsers();
		 }

		 private int NumberOfRoles()
		 {
			  return _roleRepository.numberOfRoles();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.kernel.impl.security.User newUser(String username, byte[] initialPassword, boolean requirePasswordChange) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
		 [Obsolete]
		 public override User NewUser( string username, sbyte[] initialPassword, bool requirePasswordChange )
		 {
			  try
			  {
					_userRepository.assertValidUsername( username );
					_passwordPolicy.validatePassword( initialPassword );

					User user = ( new User.Builder() ).withName(username).withCredentials(LegacyCredential.forPassword(initialPassword)).withRequiredPasswordChange(requirePasswordChange).build();
					lock ( this )
					{
						 _userRepository.create( user );
					}

					return user;
			  }
			  finally
			  {
					// Clear password
					if ( initialPassword != null )
					{
						 Arrays.fill( initialPassword, ( sbyte ) 0 );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void newRole(String roleName, String... usernames) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
		 [Obsolete]
		 public override void NewRole( string roleName, params string[] usernames )
		 {
			  _roleRepository.assertValidRoleName( roleName );
			  foreach ( string username in usernames )
			  {
					_userRepository.assertValidUsername( username );
			  }

			  SortedSet<string> userSet = new SortedSet<string>( Arrays.asList( usernames ) );
			  RoleRecord role = ( new RoleRecord.Builder() ).WithName(roleName).withUsers(userSet).build();

			  lock ( this )
			  {
					foreach ( string username in usernames )
					{
						 GetUser( username ); // assert that user exists
					}
					_roleRepository.create( role );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean deleteRole(String roleName) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
		 [Obsolete]
		 public override bool DeleteRole( string roleName )
		 {
			  AssertNotPredefinedRoleName( roleName );

			  bool result = false;
			  lock ( this )
			  {
					RoleRecord role = GetRole( roleName ); // asserts role name exists
					if ( _roleRepository.delete( role ) )
					{
						 result = true;
					}
					else
					{
						 // We should not get here, but if we do the assert will fail and give a nice error msg
						 AssertRoleExists( roleName );
					}
			  }
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private RoleRecord getRole(String roleName) throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
		 private RoleRecord GetRole( string roleName )
		 {
			  RoleRecord role = _roleRepository.getRoleByName( roleName );
			  if ( role == null )
			  {
					throw new InvalidArgumentsException( "Role '" + roleName + "' does not exist." );
			  }
			  return role;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void assertRoleExists(String roleName) throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
		 [Obsolete]
		 public override void AssertRoleExists( string roleName )
		 {
			  GetRole( roleName );
		 }

		 private RoleRecord SilentlyGetRole( string roleName )
		 {
			  return _roleRepository.getRoleByName( roleName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void addRoleToUser(String roleName, String username) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
		 [Obsolete]
		 public override void AddRoleToUser( string roleName, string username )
		 {
			  _roleRepository.assertValidRoleName( roleName );
			  _userRepository.assertValidUsername( username );

			  lock ( this )
			  {
					GetUser( username );
					RoleRecord role = GetRole( roleName );
					RoleRecord newRole = role.Augment().withUser(username).build();
					try
					{
						 _roleRepository.update( role, newRole );
					}
					catch ( ConcurrentModificationException )
					{
						 // Try again
						 AddRoleToUser( roleName, username );
					}
			  }
			  ClearCachedAuthorizationInfoForUser( username );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void removeRoleFromUser(String roleName, String username) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
		 [Obsolete]
		 public override void RemoveRoleFromUser( string roleName, string username )
		 {
			  _roleRepository.assertValidRoleName( roleName );
			  _userRepository.assertValidUsername( username );

			  lock ( this )
			  {
					GetUser( username );
					RoleRecord role = GetRole( roleName );

					RoleRecord newRole = role.Augment().withoutUser(username).build();
					try
					{
						 _roleRepository.update( role, newRole );
					}
					catch ( ConcurrentModificationException )
					{
						 // Try again
						 RemoveRoleFromUser( roleName, username );
					}
			  }

			  ClearCachedAuthorizationInfoForUser( username );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean deleteUser(String username) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
		 [Obsolete]
		 public override bool DeleteUser( string username )
		 {
			  lock ( this )
			  {
					User user = GetUser( username ); // throws if user does not exists
					RemoveUserFromAllRoles( username ); // performed first to always maintain auth-roles repo consistency
					_userRepository.delete( user ); // this will not fail as we know the user exists in this lock
																	// assuming no one messes with the user and role repositories
																	// outside this instance
			  }
			  ClearCacheForUser( username );
			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.kernel.impl.security.User getUser(String username) throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
		 [Obsolete]
		 public override User GetUser( string username )
		 {
			  User u = _userRepository.getUserByName( username );
			  if ( u == null )
			  {
					throw new InvalidArgumentsException( "User '" + username + "' does not exist." );
			  }
			  return u;
		 }

		 [Obsolete]
		 public override User SilentlyGetUser( string username )
		 {
			  return _userRepository.getUserByName( username );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setUserPassword(String username, byte[] password, boolean requirePasswordChange) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
		 [Obsolete]
		 public override void SetUserPassword( string username, sbyte[] password, bool requirePasswordChange )
		 {
			  try
			  {
					User existingUser = GetUser( username );
					_passwordPolicy.validatePassword( password );
					if ( existingUser.Credentials().matchesPassword(password) )
					{
						 throw new InvalidArgumentsException( "Old password and new password cannot be the same." );
					}

					try
					{
						 User updatedUser = existingUser.Augment().withCredentials(LegacyCredential.forPassword(password)).withRequiredPasswordChange(requirePasswordChange).build();
						 lock ( this )
						 {
							  _userRepository.update( existingUser, updatedUser );
						 }
					}
					catch ( ConcurrentModificationException )
					{
						 // try again
						 SetUserPassword( username, password, requirePasswordChange );
					}

					ClearCacheForUser( username );
			  }
			  finally
			  {
					// Clear password
					if ( password != null )
					{
						 Arrays.fill( password, ( sbyte ) 0 );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void suspendUser(String username) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
		 [Obsolete]
		 public override void SuspendUser( string username )
		 {
			  User user = GetUser( username );
			  if ( !user.HasFlag( IS_SUSPENDED ) )
			  {
					User suspendedUser = user.Augment().withFlag(IS_SUSPENDED).build();
					try
					{
						 lock ( this )
						 {
							  _userRepository.update( user, suspendedUser );
						 }
					}
					catch ( ConcurrentModificationException )
					{
						 // Try again
						 SuspendUser( username );
					}
			  }
			  ClearCacheForUser( username );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void activateUser(String username, boolean requirePasswordChange) throws java.io.IOException, org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
		 [Obsolete]
		 public override void ActivateUser( string username, bool requirePasswordChange )
		 {
			  User user = GetUser( username );
			  if ( user.HasFlag( IS_SUSPENDED ) )
			  {
					User activatedUser = user.Augment().withoutFlag(IS_SUSPENDED).withRequiredPasswordChange(requirePasswordChange).build();
					try
					{
						 lock ( this )
						 {
							  _userRepository.update( user, activatedUser );
						 }
					}
					catch ( ConcurrentModificationException )
					{
						 // Try again
						 ActivateUser( username, requirePasswordChange );
					}
			  }
			  ClearCacheForUser( username );
		 }

		 [Obsolete]
		 public virtual ISet<string> AllRoleNames
		 {
			 get
			 {
				  return _roleRepository.AllRoleNames;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Set<String> getRoleNamesForUser(String username) throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
		 [Obsolete]
		 public override ISet<string> GetRoleNamesForUser( string username )
		 {
			  GetUser( username );
			  return _roleRepository.getRoleNamesByUsername( username );
		 }

		 [Obsolete]
		 public override ISet<string> SilentlyGetRoleNamesForUser( string username )
		 {
			  return _roleRepository.getRoleNamesByUsername( username );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Set<String> getUsernamesForRole(String roleName) throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
		 [Obsolete]
		 public override ISet<string> GetUsernamesForRole( string roleName )
		 {
			  RoleRecord role = GetRole( roleName );
			  return role.Users();
		 }

		 [Obsolete]
		 public override ISet<string> SilentlyGetUsernamesForRole( string roleName )
		 {
			  RoleRecord role = SilentlyGetRole( roleName );
			  return role == null ? emptySet() : role.Users();
		 }

		 [Obsolete]
		 public virtual ISet<string> AllUsernames
		 {
			 get
			 {
				  return _userRepository.AllUsernames;
			 }
		 }

		 // this is only used from already synchronized code blocks
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void removeUserFromAllRoles(String username) throws java.io.IOException
		 private void RemoveUserFromAllRoles( string username )
		 {
			  try
			  {
					_roleRepository.removeUserFromAllRoles( username );
			  }
			  catch ( ConcurrentModificationException )
			  {
					// Try again
					RemoveUserFromAllRoles( username );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertNotPredefinedRoleName(String roleName) throws org.Neo4Net.kernel.api.exceptions.InvalidArgumentsException
		 private void AssertNotPredefinedRoleName( string roleName )
		 {
			  if ( !string.ReferenceEquals( roleName, null ) && PredefinedRolesBuilder.Roles.Keys.Contains( roleName ) )
			  {
					throw new InvalidArgumentsException( format( "'%s' is a predefined role and can not be deleted.", roleName ) );
			  }
		 }

		 private void ClearCachedAuthorizationInfoForUser( string username )
		 {
			  clearCachedAuthorizationInfo( new SimplePrincipalCollection( username, this.Name ) );
		 }

		 private void ClearCacheForUser( string username )
		 {
			  clearCache( new SimplePrincipalCollection( username, this.Name ) );
		 }
	}

}