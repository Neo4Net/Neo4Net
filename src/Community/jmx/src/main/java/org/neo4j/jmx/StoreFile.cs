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
	[ManagementInterface(name : StoreFile_Fields.NAME), Description("This bean is deprecated, use StoreSize bean instead; " + "Information about the sizes of the different parts of the Neo4j graph store"), Obsolete]
	public interface StoreFile
	{

		 [Description("The amount of disk space used by the current Neo4j logical log, in bytes.")]
		 long LogicalLogSize { get; }

		 [Description("The total disk space used by this Neo4j instance, in bytes.")]
		 long TotalStoreSize { get; }

		 [Description("The amount of disk space used to store nodes, in bytes.")]
		 long NodeStoreSize { get; }

		 [Description("The amount of disk space used to store relationships, in bytes.")]
		 long RelationshipStoreSize { get; }

		 [Description("The amount of disk space used to store properties " + "(excluding string values and array values), in bytes.")]
		 long PropertyStoreSize { get; }

		 [Description("The amount of disk space used to store string properties, in bytes.")]
		 long StringStoreSize { get; }

		 [Description("The amount of disk space used to store array properties, in bytes.")]
		 long ArrayStoreSize { get; }
	}

	public static class StoreFile_Fields
	{
		 public const string NAME = "Store file sizes";
	}

}