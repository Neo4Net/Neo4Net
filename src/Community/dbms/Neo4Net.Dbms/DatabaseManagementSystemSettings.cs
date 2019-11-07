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
namespace Neo4Net.Dbms
{

	using Internal = Neo4Net.Configuration.Internal;
	using LoadableConfig = Neo4Net.Configuration.LoadableConfig;
	using Neo4Net.GraphDb.config;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.configuration.Settings.PATH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.configuration.Settings.derivedSetting;

	public class DatabaseManagementSystemSettings : LoadableConfig
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final Neo4Net.graphdb.config.Setting<java.io.File> auth_store_directory = derivedSetting("unsupported.dbms.directories.auth", Neo4Net.graphdb.factory.GraphDatabaseSettings.data_directory, data -> new java.io.File(data, "dbms"), PATH);
		 public static readonly Setting<File> AuthStoreDirectory = derivedSetting( "unsupported.dbms.directories.auth", GraphDatabaseSettings.data_directory, data => new File( data, "dbms" ), PATH );
	}

}