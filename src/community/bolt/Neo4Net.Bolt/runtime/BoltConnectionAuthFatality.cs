using System;

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
namespace Neo4Net.Bolt.runtime
{
	using AuthenticationException = Neo4Net.Bolt.security.auth.AuthenticationException;

	/// <summary>
	/// Indicates that bolt connection has been fatally misused and therefore the server should close the connection.
	/// </summary>
	public class BoltConnectionAuthFatality : BoltConnectionFatality
	{
		 private readonly bool _isLoggable;

		 public BoltConnectionAuthFatality( string message, Exception cause ) : this( message, cause, false )
		 {
		 }

		 public BoltConnectionAuthFatality( AuthenticationException cause ) : this( cause.Message, cause, true )
		 {
		 }

		 private BoltConnectionAuthFatality( string message, Exception cause, bool isLoggable ) : base( message, cause )
		 {
			  requireNonNull( message );
			  this._isLoggable = isLoggable;
		 }

		 public virtual bool Loggable
		 {
			 get
			 {
				  return this._isLoggable;
			 }
		 }
	}

}