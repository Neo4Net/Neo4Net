using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.store.format
{

	using Neo4Net.GraphDb.config;
	using MapUtil = Neo4Net.Collections.Helpers.MapUtil;
	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.DEFAULT_BLOCK_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.DEFAULT_LABEL_BLOCK_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.MINIMAL_BLOCK_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.array_block_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.label_block_size;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.string_block_size;

	/// <summary>
	/// There are couple of configuration options that should be adapted for each particular implementation of record format.
	/// In case if user already set value of those properties we will keep them, otherwise format specific value will be
	/// evaluated and configuration will be adapted.
	/// </summary>
	public class RecordFormatPropertyConfigurator
	{
		 private readonly RecordFormats _recordFormats;
		 private readonly Config _config;

		 public RecordFormatPropertyConfigurator( RecordFormats recordFormats, Config config )
		 {
			  this._recordFormats = recordFormats;
			  this._config = config;
		 }

		 private static void ConfigureIntegerSetting( Config config, Setting<int> setting, int fullBlockSize, int headerSize, IDictionary<string, string> formatConfig )
		 {
			  int? defaultValue = int.Parse( setting.DefaultValue );
			  int propertyValue = config.Get( setting );
			  if ( propertyValue == defaultValue.Value )
			  {
					int updatedBlockSize = fullBlockSize - headerSize;
					if ( updatedBlockSize != propertyValue )
					{
						 if ( updatedBlockSize < MINIMAL_BLOCK_SIZE )
						 {
							  throw new System.ArgumentException( "Block size should be bigger then " + MINIMAL_BLOCK_SIZE );
						 }
						 AddFormatSetting( formatConfig, setting, updatedBlockSize );
					}
			  }
		 }

		 private static void AddFormatSetting( IDictionary<string, string> configMap, Setting setting, int value )
		 {
			  configMap[setting.name()] = value.ToString();
		 }

		 public virtual void Configure()
		 {
			  IDictionary<string, string> formatConfig = MapUtil.stringMap();
			  int headerSize = _recordFormats.dynamic().RecordHeaderSize;

			  ConfigureIntegerSetting( _config, string_block_size, DEFAULT_BLOCK_SIZE, headerSize, formatConfig );
			  ConfigureIntegerSetting( _config, array_block_size, DEFAULT_BLOCK_SIZE, headerSize, formatConfig );
			  ConfigureIntegerSetting( _config, label_block_size, DEFAULT_LABEL_BLOCK_SIZE, headerSize, formatConfig );
			  if ( formatConfig.Count > 0 )
			  {
					_config.augment( formatConfig );
			  }
		 }
	}

}