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
namespace Neo4Net.Helpers
{

	/// <summary>
	/// A bind exception that includes which port we failed to bind to.
	/// Whenever possible, catch and rethrow bind exceptions as this, to make it possible to
	/// sort out which address it is that is in use.
	/// </summary>
	public class PortBindException : BindException
	{
		 public PortBindException( ListenSocketAddress address, Exception original ) : this( address, null, original )
		 {
		 }

		 public PortBindException( ListenSocketAddress address1, ListenSocketAddress address2, Exception original ) : base( CreateMessage( address1, address2 ) )
		 {
			  StackTrace = original.StackTrace;
		 }

		 private static string CreateMessage( ListenSocketAddress address1, ListenSocketAddress address2 )
		 {
			  if ( address1 == null && address2 == null )
			  {
					return "Address is already in use, cannot bind to it.";
			  }
			  else if ( address1 != null && address2 != null )
			  {
					return string.Format( "At least one of the addresses {0} or {1} is already in use, cannot bind to it.", address1, address2 );
			  }
			  else
			  {
					return string.Format( "Address {0} is already in use, cannot bind to it.", address1 != null ? address1 : address2 );
			  }
		 }
	}

}