﻿using System;

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
namespace Org.Neo4j.Kernel.impl.store
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using RecordFormatPropertyConfigurator = Org.Neo4j.Kernel.impl.store.format.RecordFormatPropertyConfigurator;
	using RecordFormats = Org.Neo4j.Kernel.impl.store.format.RecordFormats;
	using Standard = Org.Neo4j.Kernel.impl.store.format.standard.Standard;
	using DefaultIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using PageCacheAndDependenciesRule = Org.Neo4j.Test.rule.PageCacheAndDependenciesRule;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeTrue;

	public class NeoStoreOpenFailureTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.PageCacheAndDependenciesRule rules = new org.neo4j.test.rule.PageCacheAndDependenciesRule().with(new org.neo4j.test.rule.fs.DefaultFileSystemRule());
		 public PageCacheAndDependenciesRule Rules = new PageCacheAndDependenciesRule().with(new DefaultFileSystemRule());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustCloseAllStoresIfNeoStoresFailToOpen()
		 public virtual void MustCloseAllStoresIfNeoStoresFailToOpen()
		 {
			  PageCache pageCache = Rules.pageCache();
			  DatabaseLayout databaseLayout = Rules.directory().databaseLayout();
			  Config config = Config.defaults();
			  FileSystemAbstraction fs = Rules.fileSystem();
			  IdGeneratorFactory idGenFactory = new DefaultIdGeneratorFactory( fs );
			  LogProvider logProvider = NullLogProvider.Instance;
			  VersionContextSupplier versions = EmptyVersionContextSupplier.EMPTY;
			  RecordFormats formats = Standard.LATEST_RECORD_FORMATS;
			  ( new RecordFormatPropertyConfigurator( formats, config ) ).configure();
			  bool create = true;
			  StoreType[] storeTypes = StoreType.values();
			  OpenOption[] openOptions = new OpenOption[0];
			  NeoStores neoStores = new NeoStores( databaseLayout, config, idGenFactory, pageCache, logProvider, fs, versions, formats, create, storeTypes, openOptions );
			  File schemaStore = neoStores.SchemaStore.StorageFile;
			  neoStores.Close();

			  // Make the schema store inaccessible, to sabotage the next initialisation we'll do.
			  assumeTrue( schemaStore.setReadable( false ) );
			  assumeTrue( schemaStore.setWritable( false ) );

			  try
			  {
					// This should fail due to the permissions we changed above.
					// And when it fails, the already-opened stores should be closed.
					new NeoStores( databaseLayout, config, idGenFactory, pageCache, logProvider, fs, versions, formats, create, storeTypes, openOptions );
					fail( "Opening NeoStores should have thrown." );
			  }
			  catch ( Exception )
			  {
			  }

			  // We verify that the successfully opened stores were closed again by the failed NeoStores open,
			  // by closing the page cache, which will throw if not all files have been unmapped.
			  pageCache.Close();
		 }
	}

}