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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using EntityType = Neo4Net.Storageengine.Api.EntityType;

	/// <summary>
	/// Factory that is able to create either <seealso cref="MultipleIndexPopulator"/> or <seealso cref="BatchingMultipleIndexPopulator"/>
	/// depending on the given config.
	/// </summary>
	/// <seealso cref= GraphDatabaseSettings#multi_threaded_schema_index_population_enabled </seealso>
	public abstract class MultiPopulatorFactory
	{
		 private MultiPopulatorFactory()
		 {
		 }

		 public abstract MultipleIndexPopulator Create( IndexStoreView storeView, LogProvider logProvider, EntityType type, SchemaState schemaState );

		 public static MultiPopulatorFactory ForConfig( Config config )
		 {
			  bool multiThreaded = config.Get( GraphDatabaseSettings.multi_threaded_schema_index_population_enabled );
			  return multiThreaded ? new MultiThreadedPopulatorFactory() : new SingleThreadedPopulatorFactory();
		 }

		 private class SingleThreadedPopulatorFactory : MultiPopulatorFactory
		 {
			  public override MultipleIndexPopulator Create( IndexStoreView storeView, LogProvider logProvider, EntityType type, SchemaState schemaState )
			  {
					return new MultipleIndexPopulator( storeView, logProvider, type, schemaState );
			  }
		 }

		 private class MultiThreadedPopulatorFactory : MultiPopulatorFactory
		 {
			  public override MultipleIndexPopulator Create( IndexStoreView storeView, LogProvider logProvider, EntityType type, SchemaState schemaState )
			  {
					return new BatchingMultipleIndexPopulator( storeView, logProvider, type, schemaState );
			  }
		 }
	}

}