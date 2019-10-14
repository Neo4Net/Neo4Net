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
namespace Neo4Net.Consistency
{
	using Description = Neo4Net.Configuration.Description;
	using LoadableConfig = Neo4Net.Configuration.LoadableConfig;
	using Neo4Net.Graphdb.config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.TRUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.setting;

	/// <summary>
	/// Settings for consistency checker
	/// </summary>
	[Description("Consistency check configuration settings")]
	public class ConsistencyCheckSettings : LoadableConfig
	{
		 [Description("This setting is deprecated. See commandline arguments for neoj4-admin check-consistency " + "instead. Perform optional additional checking on property ownership. " + "This can detect a theoretical inconsistency where a property could be owned by multiple entities. " + "However, the check is very expensive in time and memory, so it is skipped by default."), Obsolete]
		 public static readonly Setting<bool> ConsistencyCheckPropertyOwners = setting( "tools.consistency_checker.check_property_owners", BOOLEAN, FALSE );

		 [Description("This setting is deprecated. See commandline arguments for neoj4-admin check-consistency " + "instead. Perform checks on the label scan store. Checking this store is more expensive than " + "checking the native stores, so it may be useful to turn off this check for very large databases."), Obsolete]
		 public static readonly Setting<bool> ConsistencyCheckLabelScanStore = setting( "tools.consistency_checker.check_label_scan_store", BOOLEAN, TRUE );

		 [Description("This setting is deprecated. See commandline arguments for neoj4-admin check-consistency " + "instead. Perform checks on indexes. Checking indexes is more expensive than " + "checking the native stores, so it may be useful to turn off this check for very large databases."), Obsolete]
		 public static readonly Setting<bool> ConsistencyCheckIndexes = setting( "tools.consistency_checker.check_indexes", BOOLEAN, TRUE );

		 [Description("This setting is deprecated. See commandline arguments for neoj4-admin check-consistency " + "instead. Perform structural checks on indexes. This is done in separate step before consistency " + "check on store starts. Checking indexes is more expensive than checking the native stores, so " + "it may be useful to turn off this check for very large databases."), Obsolete]
		 public static readonly Setting<bool> ConsistencyCheckIndexStructure = setting( "tools.consistency_checker.check_index_structure", BOOLEAN, FALSE );

		 [Description("This setting is deprecated. See commandline arguments for neoj4-admin check-consistency " + "instead. Perform checks between nodes, relationships, properties, types and tokens."), Obsolete]
		 public static readonly Setting<bool> ConsistencyCheckGraph = setting( "tools.consistency_checker.check_graph", BOOLEAN, TRUE );
	}

}