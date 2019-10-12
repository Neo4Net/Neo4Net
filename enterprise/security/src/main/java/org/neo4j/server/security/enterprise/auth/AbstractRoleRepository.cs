using System.Collections.Concurrent;
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

	using InvalidArgumentsException = Org.Neo4j.Kernel.Api.Exceptions.InvalidArgumentsException;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using Org.Neo4j.Server.Security.Auth;
	using ConcurrentModificationException = Org.Neo4j.Server.Security.Auth.exception.ConcurrentModificationException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.trimToFlattenedList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.trimToList;

	public abstract class AbstractRoleRepository : LifecycleAdapter, RoleRepository
	{
		public abstract bool Validate( IList<Org.Neo4j.Kernel.impl.security.User> users, IList<RoleRecord> roles );
		public abstract ListSnapshot<RoleRecord> PersistedSnapshot { get; }
		 // TODO: We could improve concurrency by using a ReadWriteLock

		 /// <summary>
		 /// Quick lookup of roles by name </summary>
		 private readonly IDictionary<string, RoleRecord> _rolesByName = new ConcurrentDictionary<string, RoleRecord>();
		 private readonly IDictionary<string, SortedSet<string>> _rolesByUsername = new ConcurrentDictionary<string, SortedSet<string>>();

		 /// <summary>
		 /// Master list of roles </summary>
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal volatile IList<RoleRecord> RolesConflict = new List<RoleRecord>();
		 protected internal AtomicLong LastLoaded = new AtomicLong( 0L );

		 private readonly Pattern _roleNamePattern = Pattern.compile( "^[a-zA-Z0-9_]+$" );

		 public override void Clear()
		 {
			  RolesConflict.Clear();
			  _rolesByName.Clear();
			  _rolesByUsername.Clear();
		 }

		 public override RoleRecord GetRoleByName( string roleName )
		 {
			  return string.ReferenceEquals( roleName, null ) ? null : _rolesByName[roleName];
		 }

		 public override ISet<string> GetRoleNamesByUsername( string username )
		 {
			  ISet<string> roleNames = _rolesByUsername[username];
			  return roleNames != null ? roleNames : Collections.emptySet();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void create(RoleRecord role) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException, java.io.IOException
		 public override void Create( RoleRecord role )
		 {
			  AssertValidRoleName( role.Name() );

			  lock ( this )
			  {
					// Check for existing role
					foreach ( RoleRecord other in RolesConflict )
					{
						 if ( other.Name().Equals(role.Name()) )
						 {
							  throw new InvalidArgumentsException( "The specified role '" + role.Name() + "' already exists." );
						 }
					}

					RolesConflict.Add( role );

					PersistRoles();

					_rolesByName[role.Name()] = role;

					PopulateUserMap( role );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setRoles(org.neo4j.server.security.auth.ListSnapshot<RoleRecord> rolesSnapshot) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 public virtual ListSnapshot<RoleRecord> Roles
		 {
			 set
			 {
				  foreach ( RoleRecord role in value.Values() )
				  {
						AssertValidRoleName( role.Name() );
				  }
   
				  lock ( this )
				  {
						RolesConflict.Clear();
   
						( ( IList<RoleRecord> )this.RolesConflict ).AddRange( value.Values() );
						this.LastLoaded.set( value.Timestamp() );
   
	//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
						trimToList( _rolesByName, RolesConflict, RoleRecord::name );
						trimToFlattenedList( _rolesByUsername, RolesConflict, r => r.users().stream() );
   
						foreach ( RoleRecord role in RolesConflict )
						{
							 _rolesByName[role.Name()] = role;
							 PopulateUserMap( role );
						}
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void update(RoleRecord existingRole, RoleRecord updatedRole) throws org.neo4j.server.security.auth.exception.ConcurrentModificationException, java.io.IOException
		 public override void Update( RoleRecord existingRole, RoleRecord updatedRole )
		 {
			  // Assert input is ok
			  if ( !existingRole.Name().Equals(updatedRole.Name()) )
			  {
					throw new System.ArgumentException( "The attempt to update the role from '" + existingRole.Name() + "' to '" + updatedRole.Name() + "' failed. Changing a roles name is not allowed." );
			  }

			  lock ( this )
			  {
					// Copy-on-write for the roles list
					IList<RoleRecord> newRoles = new List<RoleRecord>();
					bool foundRole = false;
					foreach ( RoleRecord other in RolesConflict )
					{
						 if ( other.Equals( existingRole ) )
						 {
							  foundRole = true;
							  newRoles.Add( updatedRole );
						 }
						 else
						 {
							  newRoles.Add( other );
						 }
					}

					if ( !foundRole )
					{
						 throw new ConcurrentModificationException();
					}

					RolesConflict = newRoles;

					PersistRoles();

					_rolesByName[updatedRole.Name()] = updatedRole;

					RemoveFromUserMap( existingRole );
					PopulateUserMap( updatedRole );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized boolean delete(RoleRecord role) throws java.io.IOException
		 public override bool Delete( RoleRecord role )
		 {
			 lock ( this )
			 {
				  bool foundRole = false;
				  // Copy-on-write for the roles list
				  IList<RoleRecord> newRoles = new List<RoleRecord>();
				  foreach ( RoleRecord other in RolesConflict )
				  {
						if ( other.Name().Equals(role.Name()) )
						{
							 foundRole = true;
						}
						else
						{
							 newRoles.Add( other );
						}
				  }
      
				  if ( foundRole )
				  {
						RolesConflict = newRoles;
      
						PersistRoles();
      
						_rolesByName.Remove( role.Name() );
				  }
      
				  RemoveFromUserMap( role );
				  return foundRole;
			 }
		 }

		 public override int NumberOfRoles()
		 {
			 lock ( this )
			 {
				  return RolesConflict.Count;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void assertValidRoleName(String name) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 public override void AssertValidRoleName( string name )
		 {
			  if ( string.ReferenceEquals( name, null ) || name.Length == 0 )
			  {
					throw new InvalidArgumentsException( "The provided role name is empty." );
			  }
			  if ( !_roleNamePattern.matcher( name ).matches() )
			  {
					throw new InvalidArgumentsException( "Role name '" + name + "' contains illegal characters. Use simple ascii characters and numbers." );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void removeUserFromAllRoles(String username) throws org.neo4j.server.security.auth.exception.ConcurrentModificationException, java.io.IOException
		 public override void RemoveUserFromAllRoles( string username )
		 {
			 lock ( this )
			 {
				  ISet<string> roles = _rolesByUsername[username];
				  if ( roles != null )
				  {
						// Since update() is modifying the set we create a copy for the iteration
						IList<string> rolesToRemoveFrom = new List<string>( roles );
						foreach ( string roleName in rolesToRemoveFrom )
						{
							 RoleRecord role = _rolesByName[roleName];
							 RoleRecord newRole = role.Augment().withoutUser(username).build();
							 Update( role, newRole );
						}
				  }
			 }
		 }

		 public virtual ISet<string> AllRoleNames
		 {
			 get
			 {
				 lock ( this )
				 {
	//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
	//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					  return RolesConflict.Select( RoleRecord::name ).collect( Collectors.toSet() );
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void purge() throws java.io.IOException
		 public override void Purge()
		 {
			  Clear(); // Clear all cached data
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void markAsMigrated() throws java.io.IOException
		 public override void MarkAsMigrated()
		 {
			  Clear(); // Clear all cached data
		 }

		 /// <summary>
		 /// Override this in the implementing class to persist roles
		 /// </summary>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void persistRoles() throws java.io.IOException;
		 protected internal abstract void PersistRoles();

		 /// <summary>
		 /// Override this in the implementing class to load roles
		 /// </summary>
		 /// <returns> a timestamped snapshot of roles, or null if the backing file did not exist </returns>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract org.neo4j.server.security.auth.ListSnapshot<RoleRecord> readPersistedRoles() throws java.io.IOException;
		 protected internal abstract ListSnapshot<RoleRecord> ReadPersistedRoles();

		 // ------------------ helpers --------------------

		 protected internal virtual void PopulateUserMap( RoleRecord role )
		 {
			  foreach ( string username in role.Users() )
			  {
					SortedSet<string> memberOfRoles = _rolesByUsername.computeIfAbsent( username, k => new ConcurrentSkipListSet<string>() );
					memberOfRoles.Add( role.Name() );
			  }
		 }

		 protected internal virtual void RemoveFromUserMap( RoleRecord role )
		 {
			  foreach ( string username in role.Users() )
			  {
					SortedSet<string> memberOfRoles = _rolesByUsername[username];
					if ( memberOfRoles != null )
					{
						 memberOfRoles.remove( role.Name() );
					}
			  }
		 }
	}

}