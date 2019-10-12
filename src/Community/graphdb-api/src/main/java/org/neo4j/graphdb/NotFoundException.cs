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
namespace Neo4Net.Graphdb
{
	/// <summary>
	/// This exception will be thrown if a request is made to a node, relationship or
	/// property that does not exist. As an example, using
	/// <seealso cref="GraphDatabaseService.getNodeById"/> passing in an id that does not exist
	/// will cause this exception to be thrown.
	/// <seealso cref="PropertyContainer.getProperty(string)"/> will also throw this exception
	/// if the given key does not exist.
	/// <para>
	/// Another scenario involves multiple concurrent transactions which obtain a reference to the same node or
	/// relationship, which is then deleted by one of the transactions. If the deleting transaction commits, then invoking
	/// any node or relationship methods within any of the remaining open transactions will cause this exception to be
	/// thrown.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= GraphDatabaseService </seealso>
	public class NotFoundException : Exception
	{
		 public NotFoundException() : base()
		 {
		 }

		 public NotFoundException( string message ) : base( message )
		 {
		 }

		 public NotFoundException( string message, Exception cause ) : base( message, cause )
		 {
		 }

		 public NotFoundException( Exception cause ) : base( cause )
		 {
		 }
	}

}