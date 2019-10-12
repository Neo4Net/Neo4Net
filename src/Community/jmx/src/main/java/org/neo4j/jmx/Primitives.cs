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
namespace Neo4Net.Jmx
{
	[ManagementInterface(name : Primitives_Fields.NAME), Description("Estimates of the numbers of different kinds of Neo4j primitives"), Obsolete]
	public interface Primitives
	{

		 [Description("An estimation of the number of nodes used in this Neo4j instance")]
		 long NumberOfNodeIdsInUse { get; }

		 [Description("An estimation of the number of relationships used in this Neo4j instance")]
		 long NumberOfRelationshipIdsInUse { get; }

		 [Description("The number of relationship types used in this Neo4j instance")]
		 long NumberOfRelationshipTypeIdsInUse { get; }

		 [Description("An estimation of the number of properties used in this Neo4j instance")]
		 long NumberOfPropertyIdsInUse { get; }
	}

	public static class Primitives_Fields
	{
		 public const string NAME = "Primitive count";
	}

}