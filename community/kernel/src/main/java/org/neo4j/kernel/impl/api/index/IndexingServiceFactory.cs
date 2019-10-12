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
namespace Org.Neo4j.Kernel.Impl.Api.index
{
	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using IndexSamplingConfig = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using IndexSamplingController = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingController;
	using IndexSamplingControllerFactory = Org.Neo4j.Kernel.Impl.Api.index.sampling.IndexSamplingControllerFactory;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using SchemaRule = Org.Neo4j.Storageengine.Api.schema.SchemaRule;

	/// <summary>
	/// Factory to create <seealso cref="IndexingService"/>
	/// </summary>
	public class IndexingServiceFactory
	{
		 private IndexingServiceFactory()
		 {
		 }

		 public static IndexingService CreateIndexingService( Config config, JobScheduler scheduler, IndexProviderMap providerMap, IndexStoreView storeView, TokenNameLookup tokenNameLookup, IEnumerable<SchemaRule> schemaRules, LogProvider internalLogProvider, LogProvider userLogProvider, IndexingService.Monitor monitor, SchemaState schemaState, bool readOnly )
		 {
			  IndexSamplingConfig samplingConfig = new IndexSamplingConfig( config );
			  MultiPopulatorFactory multiPopulatorFactory = MultiPopulatorFactory.ForConfig( config );
			  IndexMapReference indexMapRef = new IndexMapReference();
			  IndexSamplingControllerFactory factory = new IndexSamplingControllerFactory( samplingConfig, storeView, scheduler, tokenNameLookup, internalLogProvider );
			  IndexSamplingController indexSamplingController = factory.Create( indexMapRef );
			  IndexProxyCreator proxySetup = new IndexProxyCreator( samplingConfig, storeView, providerMap, tokenNameLookup, internalLogProvider );

			  return new IndexingService( proxySetup, providerMap, indexMapRef, storeView, schemaRules, indexSamplingController, tokenNameLookup, scheduler, schemaState, multiPopulatorFactory, internalLogProvider, userLogProvider, monitor, readOnly );
		 }
	}

}