using System.Collections.Generic;

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
namespace Neo4Net.Server.configuration
{

	using Neo4Net.Graphdb.config;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using MapUtil = Neo4Net.Helpers.Collection.MapUtil;

	public class ConfigFileBuilder
	{
		 private readonly File _directory;
		 private readonly IDictionary<string, string> _config;

		 public static ConfigFileBuilder Builder( File directory )
		 {
			  return new ConfigFileBuilder( directory );
		 }

		 private ConfigFileBuilder( File directory )
		 {
			  this._directory = directory;

			  //initialize config with defaults that doesn't pollute
			  //workspace with generated data
			  this._config = MapUtil.stringMap( GraphDatabaseSettings.data_directory.name(), directory.AbsolutePath + "/data", ServerSettings.ManagementApiPath.name(), "http://localhost:7474/db/manage/", ServerSettings.RestApiPath.name(), "http://localhost:7474/db/data/" );
		 }

		 public virtual File Build()
		 {
			  File file = new File( _directory, "config" );
			  ServerTestUtils.writeConfigToFile( _config, file );
			  return file;
		 }

		 public virtual ConfigFileBuilder WithNameValue( string name, string value )
		 {
			  _config[name] = value;
			  return this;
		 }

		 public virtual ConfigFileBuilder WithSetting( Setting setting, string value )
		 {
			  _config[setting.name()] = value;
			  return this;
		 }

		 public virtual ConfigFileBuilder WithoutSetting( Setting setting )
		 {
			  _config.Remove( setting.name() );
			  return this;
		 }
	}

}