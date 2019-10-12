using System.Collections.Concurrent;
using System.Collections.Generic;

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
namespace Org.Neo4j.Server.Security.Auth
{

	using InvalidArgumentsException = Org.Neo4j.Kernel.Api.Exceptions.InvalidArgumentsException;
	using User = Org.Neo4j.Kernel.impl.security.User;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using ConcurrentModificationException = Org.Neo4j.Server.Security.Auth.exception.ConcurrentModificationException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.trimToList;

	public abstract class AbstractUserRepository : LifecycleAdapter, UserRepository
	{
		public abstract ListSnapshot<User> PersistedSnapshot { get; }
		 /// <summary>
		 /// Quick lookup of users by name </summary>
		 private readonly IDictionary<string, User> _usersByName = new ConcurrentDictionary<string, User>();

		 /// <summary>
		 /// Master list of users </summary>
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal volatile IList<User> UsersConflict = new List<User>();
		 protected internal AtomicLong LastLoaded = new AtomicLong( 0L );

		 // Allow all ascii from '!' to '~', apart from ',' and ':' which are used as separators in flat file
		 private readonly Pattern _usernamePattern = Pattern.compile( "^[\\x21-\\x2B\\x2D-\\x39\\x3B-\\x7E]+$" );

		 public override void Clear()
		 {
			  UsersConflict.Clear();
			  _usersByName.Clear();
		 }

		 public override User GetUserByName( string username )
		 {
			  return string.ReferenceEquals( username, null ) ? null : _usersByName[username];
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void create(org.neo4j.kernel.impl.security.User user) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException, java.io.IOException
		 public override void Create( User user )
		 {
			  AssertValidUsername( user.Name() );

			  lock ( this )
			  {
					// Check for existing user
					foreach ( User other in UsersConflict )
					{
						 if ( other.Name().Equals(user.Name()) )
						 {
							  throw new InvalidArgumentsException( "The specified user '" + user.Name() + "' already exists." );
						 }
					}

					UsersConflict.Add( user );
					_usersByName[user.Name()] = user;
					PersistUsers();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setUsers(ListSnapshot<org.neo4j.kernel.impl.security.User> usersSnapshot) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 public virtual ListSnapshot<User> Users
		 {
			 set
			 {
				  foreach ( User user in value.Values() )
				  {
						AssertValidUsername( user.Name() );
				  }
   
				  lock ( this )
				  {
						UsersConflict.Clear();
   
						( ( IList<User> )this.UsersConflict ).AddRange( value.Values() );
						this.LastLoaded.set( value.Timestamp() );
   
	//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
						trimToList( _usersByName, UsersConflict, User::name );
   
						foreach ( User user in UsersConflict )
						{
							 _usersByName[user.Name()] = user;
						}
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void update(org.neo4j.kernel.impl.security.User existingUser, org.neo4j.kernel.impl.security.User updatedUser) throws org.neo4j.server.security.auth.exception.ConcurrentModificationException, java.io.IOException
		 public override void Update( User existingUser, User updatedUser )
		 {
			  // Assert input is ok
			  if ( !existingUser.Name().Equals(updatedUser.Name()) )
			  {
					throw new System.ArgumentException( "The attempt to update the role from '" + existingUser.Name() + "' to '" + updatedUser.Name() + "' failed. Changing a roles name is not allowed." );
			  }

			  lock ( this )
			  {
					// Copy-on-write for the users list
					IList<User> newUsers = new List<User>();
					bool foundUser = false;
					foreach ( User other in UsersConflict )
					{
						 if ( other.Equals( existingUser ) )
						 {
							  foundUser = true;
							  newUsers.Add( updatedUser );
						 }
						 else
						 {
							  newUsers.Add( other );
						 }
					}

					if ( !foundUser )
					{
						 throw new ConcurrentModificationException();
					}

					UsersConflict = newUsers;
					_usersByName[updatedUser.Name()] = updatedUser;
					PersistUsers();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized boolean delete(org.neo4j.kernel.impl.security.User user) throws java.io.IOException
		 public override bool Delete( User user )
		 {
			 lock ( this )
			 {
				  bool foundUser = false;
				  // Copy-on-write for the users list
				  IList<User> newUsers = new List<User>();
				  foreach ( User other in UsersConflict )
				  {
						if ( other.Name().Equals(user.Name()) )
						{
							 foundUser = true;
						}
						else
						{
							 newUsers.Add( other );
						}
				  }
      
				  if ( foundUser )
				  {
						UsersConflict = newUsers;
						_usersByName.Remove( user.Name() );
						PersistUsers();
				  }
				  return foundUser;
			 }
		 }

		 public override int NumberOfUsers()
		 {
			 lock ( this )
			 {
				  return UsersConflict.Count;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void assertValidUsername(String username) throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
		 public override void AssertValidUsername( string username )
		 {
			  if ( string.ReferenceEquals( username, null ) || username.Length == 0 )
			  {
					throw new InvalidArgumentsException( "The provided username is empty." );
			  }
			  if ( !_usernamePattern.matcher( username ).matches() )
			  {
					throw new InvalidArgumentsException( "Username '" + username + "' contains illegal characters. Use ascii characters that are not ',', ':' or whitespaces." );
			  }
		 }

		 public virtual ISet<string> AllUsernames
		 {
			 get
			 {
				 lock ( this )
				 {
	//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
	//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					  return UsersConflict.Select( User::name ).collect( Collectors.toSet() );
				 }
			 }
		 }

		 /// <summary>
		 /// Override this in the implementing class to persist users
		 /// </summary>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void persistUsers() throws java.io.IOException;
		 protected internal abstract void PersistUsers();

		 /// <summary>
		 /// Override this in the implementing class to load users
		 /// </summary>
		 /// <returns> a timestamped snapshot of users, or null if the backing file did not exist </returns>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract ListSnapshot<org.neo4j.kernel.impl.security.User> readPersistedUsers() throws java.io.IOException;
		 protected internal abstract ListSnapshot<User> ReadPersistedUsers();

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
	}

}