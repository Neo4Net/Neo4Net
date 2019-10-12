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
namespace Org.Neo4j.Procedure
{


	/// <summary>
	/// This annotation marks a <seealso cref="Procedure"/> as performing updates to the graph.
	/// <para>
	/// This is <i>required</i> if the procedure performs write operations.
	/// </para>
	/// </summary>
	[Obsolete, AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class PerformsWrites : System.Attribute
	{
	}

}