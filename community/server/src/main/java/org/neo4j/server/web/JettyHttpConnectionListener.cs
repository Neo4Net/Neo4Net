﻿/*
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
namespace Org.Neo4j.Server.web
{
	using Connection = org.eclipse.jetty.io.Connection;

	using NetworkConnectionTracker = Org.Neo4j.Kernel.api.net.NetworkConnectionTracker;

	/// <summary>
	/// Connection listener that notifies <seealso cref="NetworkConnectionTracker"/> about open and closed <seealso cref="JettyHttpConnection"/>s.
	/// All other types of connections are ignored.
	/// </summary>
	public class JettyHttpConnectionListener : Connection.Listener
	{
		 private readonly NetworkConnectionTracker _connectionTracker;

		 public JettyHttpConnectionListener( NetworkConnectionTracker connectionTracker )
		 {
			  this._connectionTracker = connectionTracker;
		 }

		 public override void OnOpened( Connection connection )
		 {
			  if ( connection is JettyHttpConnection )
			  {
					_connectionTracker.add( ( JettyHttpConnection ) connection );
			  }
		 }

		 public override void OnClosed( Connection connection )
		 {
			  if ( connection is JettyHttpConnection )
			  {
					_connectionTracker.remove( ( JettyHttpConnection ) connection );
			  }
		 }
	}

}