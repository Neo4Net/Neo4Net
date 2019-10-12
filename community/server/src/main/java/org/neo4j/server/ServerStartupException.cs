using System;

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
namespace Org.Neo4j.Server
{
	using Log = Org.Neo4j.Logging.Log;

	public class ServerStartupException : Exception
	{
		 public ServerStartupException( string message, Exception t ) : base( message, t )
		 {
		 }

		 public ServerStartupException( string message ) : base( message )
		 {
		 }

		 public virtual void DescribeTo( Log log )
		 {
			  // By default, log the full error. The intention is that sub classes can override this and
			  // specify less extreme logging options.
			  log.Error( format( "Failed to start Neo4j: %s", Message ), this );
		 }
	}

}