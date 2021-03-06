﻿/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.causalclustering.discovery
{

	using Org.Neo4j.Graphdb.config;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;

	public interface ClusterMember<T> where T : Org.Neo4j.Kernel.@internal.GraphDatabaseAPI
	{
		 void Start();

		 void Shutdown();

		 bool Shutdown { get; }

		 T Database();

		 ClientConnectorAddresses ClientConnectorAddresses();

		 string SettingValue( string settingName );

		 Config Config();

		 /// <summary>
		 /// <seealso cref="Cluster"/> will use this <seealso cref="ThreadGroup"/> for the threads that start, and shut down, this cluster member.
		 /// This way, the group will be transitively inherited by all the threads that are in turn started by the member
		 /// during its start up and shut down processes.
		 /// <para>
		 /// This helps with debugging, because it makes it immediately visible (in the debugger) which cluster member any
		 /// given thread belongs to.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> The intended parent thread group for this cluster member. </returns>
		 ThreadGroup ThreadGroup();

		 Monitors Monitors();

		 File DatabaseDirectory();

		 File HomeDir();

		 int ServerId();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default void updateConfig(org.neo4j.graphdb.config.Setting<JavaToDotNetGenericWildcard> setting, String value)
	//	 {
	//		  config().augment(setting, value);
	//	 }
	}

}