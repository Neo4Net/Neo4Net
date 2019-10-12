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
namespace Org.Neo4j.Kernel.impl.security
{

	/// <summary>
	/// Controls authorization and authentication for an individual user.
	/// </summary>
	public class User
	{
		 /*
		   Design note: These instances are shared across threads doing disparate things with them, and there are no access
		   locks. Correctness depends on write-time assertions and this class remaining immutable. Please do not introduce
		   mutable fields here.
		  */
		 /// <summary>
		 /// User name </summary>
		 private readonly string _name;

		 /// <summary>
		 /// Authentication credentials used by the built in username/password authentication scheme </summary>
		 private readonly Credential _credential;

		 /// <summary>
		 /// Set of flags, eg. password_change_required </summary>
		 private readonly SortedSet<string> _flags;

		 public const string PASSWORD_CHANGE_REQUIRED = "password_change_required";

		 private User( string name, Credential credential, SortedSet<string> flags )
		 {
			  this._name = name;
			  this._credential = credential;
			  this._flags = flags;
		 }

		 public virtual string Name()
		 {
			  return _name;
		 }

		 public virtual Credential Credentials()
		 {
			  return _credential;
		 }

		 public virtual bool HasFlag( string flag )
		 {
			  return _flags.Contains( flag );
		 }

		 public virtual IEnumerable<string> Flags
		 {
			 get
			 {
				  return _flags;
			 }
		 }

		 public virtual bool PasswordChangeRequired()
		 {
			  return _flags.Contains( PASSWORD_CHANGE_REQUIRED );
		 }

		 /// <summary>
		 /// Use this user as a base for a new user object </summary>
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

			  User user = ( User ) o;

			  if ( !_flags.Equals( user._flags ) )
			  {
					return false;
			  }
			  if ( _credential != null ?!_credential.Equals( user._credential ) : user._credential != null )
			  {
					return false;
			  }
			  return !string.ReferenceEquals( _name, null ) ? _name.Equals( user._name ) : string.ReferenceEquals( user._name, null );
		 }

		 public override int GetHashCode()
		 {
			  int result = !string.ReferenceEquals( _name, null ) ? _name.GetHashCode() : 0;
			  result = 31 * result + ( _credential != null ? _credential.GetHashCode() : 0 );
			  result = 31 * result + ( _flags.GetHashCode() );
			  return result;
		 }

		 public override string ToString()
		 {
			  return "User{" +
						 "name='" + _name + '\'' +
						 ", credentials=" + _credential +
						 ", flags=" + _flags +
						 '}';
		 }

		 public class Builder
		 {
			  internal string Name;
			  internal Credential Credential = Credential.INACCESSIBLE;
			  internal SortedSet<string> Flags = new SortedSet<string>();

			  public Builder()
			  {
			  }

			  public Builder( string name, Credential credential )
			  {
					this.Name = name;
					this.Credential = credential;
			  }

			  public Builder( User @base )
			  {
					Name = @base.name;
					Credential = @base.credential;
					Flags.addAll( @base.flags );
			  }

			  public virtual Builder WithName( string name )
			  {
					this.Name = name;
					return this;
			  }

			  public virtual Builder WithCredentials( Credential creds )
			  {
					this.Credential = creds;
					return this;
			  }

			  public virtual Builder WithFlag( string flag )
			  {
					this.Flags.Add( flag );
					return this;
			  }

			  public virtual Builder WithoutFlag( string flag )
			  {
					this.Flags.remove( flag );
					return this;
			  }

			  public virtual Builder WithRequiredPasswordChange( bool change )
			  {
					if ( change )
					{
						 WithFlag( PASSWORD_CHANGE_REQUIRED );
					}
					else
					{
						 WithoutFlag( PASSWORD_CHANGE_REQUIRED );
					}
					return this;
			  }

			  public virtual User Build()
			  {
					return new User( Name, Credential, Flags );
			  }
		 }
	}

}