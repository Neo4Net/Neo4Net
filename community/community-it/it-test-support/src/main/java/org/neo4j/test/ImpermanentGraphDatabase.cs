﻿using System;
using System.Collections.Concurrent;
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
namespace Org.Neo4j.Test
{

	using GraphDatabaseFacadeFactory = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory;
	using Dependencies = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory.Dependencies;
	using EmbeddedGraphDatabase = Org.Neo4j.Graphdb.facade.embedded.EmbeddedGraphDatabase;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using CommunityEditionModule = Org.Neo4j.Graphdb.factory.module.edition.CommunityEditionModule;
	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using Service = Org.Neo4j.Helpers.Service;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Org.Neo4j.Kernel.extension;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using StoreLocker = Org.Neo4j.Kernel.@internal.locker.StoreLocker;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using SimpleLogService = Org.Neo4j.Logging.@internal.SimpleLogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.facade.GraphDatabaseDependencies.newDependencies;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.ephemeral;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.pagecache_memory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.TRUE;

	/// <summary>
	/// A database meant to be used in unit tests. It will always be empty on start.
	/// </summary>
	public class ImpermanentGraphDatabase : EmbeddedGraphDatabase
	{
		 /// <summary>
		 /// If enabled will track unclosed database instances in tests. The place of instantiation
		 /// will get printed in an exception with the message "Unclosed database instance".
		 /// </summary>
		 private const bool TRACK_UNCLOSED_DATABASE_INSTANCES = false;
		 private static readonly IDictionary<File, Exception> _startedButNotYetClosed = new ConcurrentDictionary<File, Exception>();

		 protected internal static readonly File Path = new File( "target/test-data/impermanent-db" );

		 /// <summary>
		 /// This is deprecated. Use <seealso cref="TestGraphDatabaseFactory"/> instead
		 /// </summary>
		 [Obsolete]
		 public ImpermanentGraphDatabase() : this(new Dictionary<>())
		 {
		 }

		 /*
		  * TODO this shouldn't be here. It so happens however that some tests may use the database
		  * directory as the path to store stuff and in this case we need to define the path explicitly,
		  * otherwise we end up writing outside the workspace and hence leave stuff behind.
		  * The other option is to explicitly remove all files present on startup. Either way,
		  * the fact that this discussion takes place is indication that things are inconsistent,
		  * since an ImpermanentGraphDatabase should not have any mention of a store directory in
		  * any case.
		  */
		 public ImpermanentGraphDatabase( File storeDir ) : this( storeDir, new Dictionary<string, string>() )
		 {
		 }

		 /// <summary>
		 /// This is deprecated. Use <seealso cref="TestGraphDatabaseFactory"/> instead
		 /// </summary>
		 [Obsolete]
		 public ImpermanentGraphDatabase( IDictionary<string, string> @params ) : this( Path, @params )
		 {
		 }

		 /// <summary>
		 /// This is deprecated. Use <seealso cref="TestGraphDatabaseFactory"/> instead
		 /// </summary>
		 [Obsolete]
		 public ImpermanentGraphDatabase( File storeDir, IDictionary<string, string> @params ) : this( storeDir, @params, Iterables.cast( Service.load( typeof( KernelExtensionFactory ) ) ) )
		 {
		 }

		 /// <summary>
		 /// This is deprecated. Use <seealso cref="TestGraphDatabaseFactory"/> instead
		 /// </summary>
		 [Obsolete]
		 public ImpermanentGraphDatabase<T1>( IDictionary<string, string> @params, IEnumerable<T1> kernelExtensions ) : this( Path, @params, kernelExtensions )
		 {
		 }

		 /// <summary>
		 /// This is deprecated. Use <seealso cref="TestGraphDatabaseFactory"/> instead
		 /// </summary>
		 [Obsolete]
		 public ImpermanentGraphDatabase<T1>( File storeDir, IDictionary<string, string> @params, IEnumerable<T1> kernelExtensions ) : this( storeDir, @params, GetDependencies( kernelExtensions ) )
		 {
		 }

		 private static GraphDatabaseFacadeFactory.Dependencies GetDependencies<T1>( IEnumerable<T1> kernelExtensions )
		 {
			  return newDependencies().kernelExtensions(kernelExtensions);
		 }

		 public ImpermanentGraphDatabase( File storeDir, IDictionary<string, string> @params, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( storeDir, @params, dependencies )
		 {
			  TrackUnclosedUse( storeDir );
		 }

		 public ImpermanentGraphDatabase( File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( storeDir, config, dependencies )
		 {
			  TrackUnclosedUse( storeDir );
		 }

		 protected internal override void Create( File storeDir, IDictionary<string, string> @params, GraphDatabaseFacadeFactory.Dependencies dependencies )
		 {
			  new GraphDatabaseFacadeFactoryAnonymousInnerClass( this, DatabaseInfo.COMMUNITY, storeDir, dependencies )
			  .initFacade( storeDir, @params, dependencies, this );
		 }

		 private class GraphDatabaseFacadeFactoryAnonymousInnerClass : GraphDatabaseFacadeFactory
		 {
			 private readonly ImpermanentGraphDatabase _outerInstance;

			 private File _storeDir;
			 private GraphDatabaseFacadeFactory.Dependencies _dependencies;

			 public GraphDatabaseFacadeFactoryAnonymousInnerClass( ImpermanentGraphDatabase outerInstance, DatabaseInfo community, File storeDir, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( community, CommunityEditionModule::new )
			 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
				 this.outerInstance = outerInstance;
				 this._storeDir = storeDir;
				 this._dependencies = dependencies;
			 }

			 protected internal override PlatformModule createPlatform( File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies )
			 {
				  return new ImpermanentPlatformModule( storeDir, config, databaseInfo, dependencies );
			 }
		 }

		 private static void TrackUnclosedUse( File storeDir )
		 {
			  if ( TRACK_UNCLOSED_DATABASE_INSTANCES )
			  {
					Exception testThatDidNotCloseDb = _startedButNotYetClosed[storeDir] = new Exception( "Unclosed database instance" );
					if ( testThatDidNotCloseDb != null )
					{
						 Console.WriteLine( testThatDidNotCloseDb.ToString() );
						 Console.Write( testThatDidNotCloseDb.StackTrace );
					}
			  }
		 }

		 public override void Shutdown()
		 {
			  if ( TRACK_UNCLOSED_DATABASE_INSTANCES )
			  {
					_startedButNotYetClosed.Remove( DatabaseLayout().databaseDirectory() );
			  }

			  base.Shutdown();
		 }

		 private static Config WithForcedInMemoryConfiguration( Config config )
		 {
			  config.augment( ephemeral, TRUE );
			  config.AugmentDefaults( pagecache_memory, "8M" );
			  return config;
		 }

		 protected internal class ImpermanentPlatformModule : PlatformModule
		 {
			  public ImpermanentPlatformModule( File storeDir, Config config, DatabaseInfo databaseInfo, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( storeDir, WithForcedInMemoryConfiguration( config ), databaseInfo, dependencies )
			  {
			  }

			  protected internal override StoreLocker CreateStoreLocker()
			  {
					return new StoreLocker( FileSystem, StoreLayout );
			  }

			  protected internal override FileSystemAbstraction CreateFileSystemAbstraction()
			  {
					return new EphemeralFileSystemAbstraction();
			  }

			  protected internal override LogService CreateLogService( LogProvider userLogProvider )
			  {
					return new SimpleLogService( NullLogProvider.Instance, NullLogProvider.Instance );
			  }
		 }
	}

}