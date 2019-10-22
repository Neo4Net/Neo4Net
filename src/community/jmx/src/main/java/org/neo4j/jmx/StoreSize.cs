﻿using System;

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
namespace Neo4Net.Jmx
{
	[ManagementInterface(name : StoreSize_Fields.NAME), Description("Information about the disk space used by different parts of the Neo4Net graph store"), Obsolete]
	public interface StoreSize
	{

		 [Description("Disk space used by the transaction logs, in bytes.")]
		 long TransactionLogsSize { get; }

		 [Description("Disk space used to store nodes, in bytes.")]
		 long NodeStoreSize { get; }

		 [Description("Disk space used to store relationships, in bytes.")]
		 long RelationshipStoreSize { get; }

		 [Description("Disk space used to store properties (excluding string values and array values), in bytes.")]
		 long PropertyStoreSize { get; }

		 [Description("Disk space used to store string properties, in bytes.")]
		 long StringStoreSize { get; }

		 [Description("Disk space used to store array properties, in bytes.")]
		 long ArrayStoreSize { get; }

		 [Description("Disk space used to store labels, in bytes")]
		 long LabelStoreSize { get; }

		 [Description("Disk space used to store counters, in bytes")]
		 long CountStoreSize { get; }

		 [Description("Disk space used to store schemas (index and constrain declarations), in bytes")]
		 long SchemaStoreSize { get; }

		 [Description("Disk space used to store all indices, in bytes")]
		 long IndexStoreSize { get; }

		 [Description("Disk space used by whole store, in bytes.")]
		 long TotalStoreSize { get; }
	}

	public static class StoreSize_Fields
	{
		 public const string NAME = "Store sizes";
	}

}