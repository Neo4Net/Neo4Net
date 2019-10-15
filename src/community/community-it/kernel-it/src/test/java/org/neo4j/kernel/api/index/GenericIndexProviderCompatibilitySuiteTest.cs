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
namespace Neo4Net.Kernel.Api.Index
{

	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ReporterFactories = Neo4Net.Kernel.Impl.Annotations.ReporterFactories;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;
	using ConsistencyCheckable = Neo4Net.Kernel.Impl.Index.Schema.ConsistencyCheckable;
	using GenericNativeIndexProviderFactory = Neo4Net.Kernel.Impl.Index.Schema.GenericNativeIndexProviderFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	public class GenericIndexProviderCompatibilitySuiteTest : IndexProviderCompatibilityTestSuite
	{
		 protected internal override IndexProvider CreateIndexProvider( PageCache pageCache, FileSystemAbstraction fs, File graphDbDir )
		 {
			  IndexProvider.Monitor monitor = IndexProvider.Monitor_Fields.Empty;
			  Config config = Config.defaults( stringMap( default_schema_provider.name(), NATIVE_BTREE10.providerName() ) );
			  OperationalMode mode = OperationalMode.single;
			  RecoveryCleanupWorkCollector recoveryCleanupWorkCollector = RecoveryCleanupWorkCollector.immediate();
			  return GenericNativeIndexProviderFactory.create( pageCache, graphDbDir, fs, monitor, config, mode, recoveryCleanupWorkCollector );
		 }

		 public override bool SupportsSpatial()
		 {
			  return true;
		 }

		 public override bool SupportsGranularCompositeQueries()
		 {
			  return true;
		 }

		 public override bool SupportsBooleanRangeQueries()
		 {
			  return true;
		 }

		 public override void ConsistencyCheck( IndexPopulator populator )
		 {
			  ( ( ConsistencyCheckable ) populator ).consistencyCheck( ReporterFactories.throwingReporterFactory() );
		 }
	}

}