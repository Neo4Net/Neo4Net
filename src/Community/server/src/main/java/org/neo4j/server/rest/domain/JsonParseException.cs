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
namespace Neo4Net.Server.rest.domain
{
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class JsonParseException extends Exception implements org.neo4j.kernel.api.exceptions.Status_HasStatus
	public class JsonParseException : Exception, Neo4Net.Kernel.Api.Exceptions.Status_HasStatus
	{
		 public JsonParseException( string message, Exception cause ) : base( message, cause )
		 {
		 }

		 public JsonParseException( Exception cause ) : base( cause )
		 {
		 }

		 public override Status Status()
		 {
			  return Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat;
		 }
	}

}