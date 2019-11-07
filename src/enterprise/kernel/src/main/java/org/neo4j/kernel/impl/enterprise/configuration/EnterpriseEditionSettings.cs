using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Kernel.impl.enterprise.configuration
{

	using Description = Neo4Net.Configuration.Description;
	using Internal = Neo4Net.Configuration.Internal;
	using LoadableConfig = Neo4Net.Configuration.LoadableConfig;
	using Neo4Net.GraphDb.config;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.configuration.Settings.STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.configuration.Settings.list;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.configuration.Settings.optionsIgnoreCase;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.configuration.Settings.optionsObeyCase;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.configuration.Settings.setting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.id.IdType.NODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.id.IdType.RELATIONSHIP;

	/// <summary>
	/// Enterprise edition specific settings
	/// </summary>
	public class EnterpriseEditionSettings : LoadableConfig
	{
		 public const string ENTERPRISE_SECURITY_MODULE_ID = "enterprise-security-module";

		 [Description("Specified names of id types (comma separated) that should be reused. " + "Currently only 'node' and 'relationship' types are supported. ")]
		 public static readonly Setting<IList<IdType>> IdTypesToReuse = setting( "dbms.ids.reuse.types.override", list( ",", optionsIgnoreCase( NODE, RELATIONSHIP ) ), string.join( ",", IdType.RELATIONSHIP.name(), IdType.NODE.name() ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final Neo4Net.graphdb.config.Setting<String> security_module = setting("unsupported.dbms.security.module", STRING, ENTERPRISE_SECURITY_MODULE_ID);
		 public static readonly Setting<string> SecurityModule = setting( "unsupported.dbms.security.module", STRING, ENTERPRISE_SECURITY_MODULE_ID );

		 [Description("Configure the operating mode of the database -- 'SINGLE' for stand-alone operation, " + "'HA' for operating as a member in an HA cluster, 'ARBITER' for a cluster member with no database in an HA cluster, " + "'CORE' for operating as a core member of a Causal Cluster, " + "or 'READ_REPLICA' for operating as a read replica member of a Causal Cluster.")]
		 public static readonly Setting<Mode> Mode = setting( "dbms.mode", optionsObeyCase( typeof( Mode ) ), Mode.Single.name() );

		 public enum Mode
		 {
			  Single,
			  Ha,
			  Arbiter,
			  Core,
			  ReadReplica
		 }
	}

}