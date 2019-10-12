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
namespace Neo4Net.Kernel.Api.Impl.Index
{

	using LuceneIndexImplementation = Neo4Net.Index.impl.lucene.@explicit.LuceneIndexImplementation;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using IndexProviders = Neo4Net.Kernel.spi.explicitindex.IndexProviders;

	public class LuceneKernelExtension : LifecycleAdapter
	{
		 private readonly DatabaseLayout _databaseLayout;
		 private readonly Config _config;
		 private readonly System.Func<IndexConfigStore> _indexStore;
		 private readonly FileSystemAbstraction _fileSystemAbstraction;
		 private readonly IndexProviders _indexProviders;
		 private readonly OperationalMode _operationalMode;

		 public LuceneKernelExtension( File databaseDirectory, Config config, System.Func<IndexConfigStore> indexStore, FileSystemAbstraction fileSystemAbstraction, IndexProviders indexProviders, OperationalMode operationalMode )
		 {
			  this._databaseLayout = DatabaseLayout.of( databaseDirectory );
			  this._config = config;
			  this._indexStore = indexStore;
			  this._fileSystemAbstraction = fileSystemAbstraction;
			  this._indexProviders = indexProviders;
			  this._operationalMode = operationalMode;
		 }

		 public override void Init()
		 {
			  LuceneIndexImplementation indexImplementation = new LuceneIndexImplementation( _databaseLayout, _config, _indexStore, _fileSystemAbstraction, _operationalMode );
			  _indexProviders.registerIndexProvider( LuceneIndexImplementation.SERVICE_NAME, indexImplementation );
		 }

		 public override void Shutdown()
		 {
			  _indexProviders.unregisterIndexProvider( LuceneIndexImplementation.SERVICE_NAME );
		 }
	}

}