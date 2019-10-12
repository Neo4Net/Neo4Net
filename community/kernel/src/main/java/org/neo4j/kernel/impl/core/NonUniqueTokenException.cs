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
namespace Org.Neo4j.Kernel.impl.core
{
	/// <summary>
	/// This is a <seealso cref="System.Exception"/> since there is no sensible way to handle this exception.
	/// It signals that the database is inconsistent, or trying to perform an inconsistent operations,
	/// and when thrown it should bubble up in order to stop the database.
	/// </summary>
	public class NonUniqueTokenException : Exception
	{
		 public NonUniqueTokenException( string tokenType, string tokenName, int tokenId, int existingId ) : base( string.Format( "The {0} \"{1}\" is not unique, it existed with id={2:D} before being added with id={3:D}.", tokenType, tokenName, existingId, tokenId ) )
		 {
		 }
	}

}