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
namespace Neo4Net.Kernel.configuration
{

	/// <summary>
	/// Used in settings classes to denote that a field contains an <seealso cref="ConfigurationMigrator"/>.
	/// This gets picked up by the configuration, and config migrations are applied whenever configuration
	/// is modified.
	/// <para>
	/// The filed must be declared as {@code static} and have a type that implements <seealso cref="ConfigurationMigrator"/>, otherwise
	/// runtime exceptions will be thrown.
	/// </para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	public class Migrator : System.Attribute
	{

	}

}