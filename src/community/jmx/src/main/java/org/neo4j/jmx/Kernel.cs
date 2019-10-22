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
namespace Neo4Net.Jmx
{

	[ManagementInterface(name : Kernel_Fields.NAME), Description("Information about the Neo4Net kernel"), Obsolete]
	public interface Kernel
	{

		 [Description("An ObjectName that can be used as a query for getting all management " + "beans for this Neo4Net instance.")]
		 ObjectName MBeanQuery { get; }

		 [Description("The name of the mounted database")]
		 string DatabaseName { get; }

		 [Description("The version of Neo4Net")]
		 string KernelVersion { get; }

		 [Description("The time from which this Neo4Net instance was in operational mode.")]
		 DateTime KernelStartTime { get; }

		 [Description("The time when this Neo4Net graph store was created.")]
		 DateTime StoreCreationDate { get; }

		 [Description("An identifier that, together with store creation time, uniquely identifies this Neo4Net graph store.")]
		 string StoreId { get; }

		 [Description("The current version of the Neo4Net store logical log.")]
		 long StoreLogVersion { get; }

		 [Description("Whether this is a read only instance")]
		 bool ReadOnly { get; }
	}

	public static class Kernel_Fields
	{
		 public const string NAME = "Kernel";
	}

}