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
namespace Neo4Net.Io.layout
{
	/// <summary>
	/// List of file names for a database.
	/// </summary>
	internal sealed class DatabaseFileNames
	{
		 internal const string METADATA_STORE = "neostore";

		 internal const string LABEL_SCAN_STORE = "neostore.labelscanstore.db";

		 internal const string COUNTS_STORE_A = "neostore.counts.db.a";
		 internal const string COUNTS_STORE_B = "neostore.counts.db.b";

		 internal const string NODE_STORE = "neostore.nodestore.db";
		 internal const string NODE_LABELS_STORE = "neostore.nodestore.db.labels";

		 internal const string RELATIONSHIP_STORE = "neostore.relationshipstore.db";
		 internal const string RELATIONSHIP_GROUP_STORE = "neostore.relationshipgroupstore.db";
		 internal const string RELATIONSHIP_TYPE_TOKEN_STORE = "neostore.relationshiptypestore.db";
		 internal const string RELATIONSHIP_TYPE_TOKEN_NAMES_STORE = "neostore.relationshiptypestore.db.names";

		 internal const string PROPERTY_STORE = "neostore.propertystore.db";
		 internal const string PROPERTY_ARRAY_STORE = "neostore.propertystore.db.arrays";
		 internal const string PROPERTY_STRING_STORE = "neostore.propertystore.db.strings";
		 internal const string PROPERTY_KEY_TOKEN_STORE = "neostore.propertystore.db.index";
		 internal const string PROPERTY_KEY_TOKEN_NAMES_STORE = "neostore.propertystore.db.index.keys";

		 internal const string LABEL_TOKEN_STORE = "neostore.labeltokenstore.db";
		 internal const string LABEL_TOKEN_NAMES_STORE = "neostore.labeltokenstore.db.names";

		 internal const string SCHEMA_STORE = "neostore.schemastore.db";
	}

}