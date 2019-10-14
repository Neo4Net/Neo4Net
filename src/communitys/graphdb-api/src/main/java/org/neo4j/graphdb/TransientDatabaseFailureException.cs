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
namespace Neo4Net.Graphdb
{
	/// <summary>
	/// Indicates that the database is in, or meanwhile a unit of work was executing, got into an intermediate state
	/// where the unit of work, and potentially other units of work as well, couldn't complete successfully.
	/// 
	/// A proper response to a caught exception of this type is to cancel the unit of work that produced
	/// this exception, check for database availability and retry the unit of work again, as a whole.
	/// </summary>
	public class TransientDatabaseFailureException : TransientFailureException
	{
		 public TransientDatabaseFailureException( string message, Exception cause ) : base( message, cause )
		 {
		 }

		 public TransientDatabaseFailureException( string message ) : base( message )
		 {
		 }
	}

}