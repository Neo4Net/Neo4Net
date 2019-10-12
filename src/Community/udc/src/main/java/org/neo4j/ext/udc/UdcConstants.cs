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
namespace Neo4Net.Ext.Udc
{
	public class UdcConstants
	{
		 public const string ID = "id";
		 public const string TAGS = "tags";
		 public const string CLUSTER_HASH = "cluster";
		 public const string REGISTRATION = "reg";
		 public const string PING = "p";
		 public const string DISTRIBUTION = "dist";
		 public const string DATABASE_MODE = "databasemode";
		 public const string SERVER_ID = "serverid";

		 public const string FEATURES = "features";

		 public const string USER_AGENTS = "ua";
		 public const string VERSION = "v";
		 public const string REVISION = "revision";
		 public const string EDITION = "edition";
		 public const string SOURCE = "source";

		 public const string MAC = "mac";
		 public const string NUM_PROCESSORS = "numprocs";
		 public const string TOTAL_MEMORY = "totalmem";
		 public const string HEAP_SIZE = "heapsize";

		 public const string NODE_IDS_IN_USE = "nodeids";
		 public const string RELATIONSHIP_IDS_IN_USE = "relids";
		 public const string PROPERTY_IDS_IN_USE = "propids";
		 public const string LABEL_IDS_IN_USE = "labelids";

		 public const string UDC_PROPERTY_PREFIX = "unsupported.dbms.udc";
		 public const string OS_PROPERTY_PREFIX = "os";
		 public const string UNKNOWN_DIST = "unknown";

		 public const string STORE_SIZE = "storesize";

		 private UdcConstants()
		 {
		 }
	}

}