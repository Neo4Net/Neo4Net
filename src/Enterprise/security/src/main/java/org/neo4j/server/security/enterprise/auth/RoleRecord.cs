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

	/// <summary>
	/// Controls authorization and authentication for a set of users.
	/// </summary>
	public class RoleRecord
	{
		 /*
		   Design note: These instances are shared across threads doing disparate things with them, and there are no access
		   locks. Correctness depends on write-time assertions and this class remaining immutable. Please do not introduce
		   mutable fields here.
		  */
		 /// <summary>
		 /// Role name </summary>
		 private readonly string _name;

		 /// <summary>
		 /// Member users </summary>
		 private readonly SortedSet<string> _users;

		 public RoleRecord( string name, SortedSet<string> users )
		 {
			  this._name = name;
			  this._users = users;
		 }

		 public RoleRecord( string name, params string[] users )
		 {
			  this._name = name;
			  this._users = new SortedSet<string>();

			  this._users.addAll( Arrays.asList( users ) );
		 }

		 public virtual string Name()
		 {
			  return _name;
		 }

		 public virtual SortedSet<string> Users()
		 {
			  return _users;
		 }

		 /// <summary>
		 /// Use this role as a base for a new role object </summary>
		 public virtual Builder Augment()
		 {
			  return new Builder( this );
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  RoleRecord role = ( RoleRecord ) o;

			  if ( !string.ReferenceEquals( _name, null ) ?!_name.Equals( role._name ) :!string.ReferenceEquals( role._name, null ) )
			  {
					return false;
			  }
			  return _users != null ? _users.Equals( role._users ) : role._users == null;
		 }

		 public override int GetHashCode()
		 {
			  int result = !string.ReferenceEquals( _name, null ) ? _name.GetHashCode() : 0;
			  result = 31 * result + ( _users != null ? _users.GetHashCode() : 0 );
			  return result;
		 }

		 public override string ToString()
		 {
			  return "Role{" +
						"name='" + _name + '\'' +
						", users='" + _users + '\'' +
						'}';
		 }

		 public class Builder
		 {
			  internal string Name;
			  internal SortedSet<string> Users = new SortedSet<string>();

			  public Builder()
			  {
			  }

			  public Builder( RoleRecord @base )
			  {
					Name = @base.name;
					Users = new SortedSet<string>( @base.users );
			  }

			  public virtual Builder WithName( string name )
			  {
					this.Name = name;
					return this;
			  }

			  public virtual Builder WithUsers( SortedSet<string> users )
			  {
					this.Users = users;
					return this;
			  }

			  public virtual Builder WithUser( string user )
			  {
					this.Users.Add( user );
					return this;
			  }

			  public virtual Builder WithoutUser( string user )
			  {
					this.Users.remove( user );
					return this;
			  }

			  public virtual RoleRecord Build()
			  {
					return new RoleRecord( Name, Users );
			  }
		 }
	}

}