/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Server.rest.dbms
{

	using LoginContext = Neo4Net.@internal.Kernel.Api.security.LoginContext;

	public class DelegatingPrincipal : Principal
	{
		 private string _username;
		 private readonly LoginContext _loginContext;

		 internal DelegatingPrincipal( string username, LoginContext loginContext )
		 {
			  this._username = username;
			  this._loginContext = loginContext;
		 }

		 public override string Name
		 {
			 get
			 {
				  return _username;
			 }
		 }

		 public virtual LoginContext LoginContext
		 {
			 get
			 {
				  return _loginContext;
			 }
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( !( o is DelegatingPrincipal ) )
			  {
					return false;
			  }

			  DelegatingPrincipal that = ( DelegatingPrincipal ) o;
			  return _username.Equals( that._username );
		 }

		 public override int GetHashCode()
		 {
			  return _username.GetHashCode();
		 }

		 public override string ToString()
		 {
			  return "DelegatingPrincipal{" +
						 "username='" + _username + '\'' +
						 '}';
		 }
	}

}