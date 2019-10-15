﻿/*
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
namespace Neo4Net.Server.rest.web
{
	public interface Surface
	{
	}

	public static class Surface_Fields
	{
		 public const string PATH_NODES = "node";
		 public const string PATH_NODE_INDEX = "index/node";
		 public const string PATH_RELATIONSHIP_INDEX = "index/relationship";
		 public const string PATH_EXTENSIONS = "ext";
		 public const string PATH_RELATIONSHIP_TYPES = "relationship/types";
		 public const string PATH_SCHEMA_INDEX = "schema/index";
		 public const string PATH_SCHEMA_CONSTRAINT = "schema/constraint";
		 public const string PATH_SCHEMA_RELATIONSHIP_CONSTRAINT = "schema/relationship/constraint";
		 public const string PATH_BATCH = "batch";
		 public const string PATH_CYPHER = "cypher";
		 public const string PATH_TRANSACTION = "transaction";
		 public const string PATH_RELATIONSHIPS = "relationship";
		 public const string PATH_LABELS = "labels";
	}

}