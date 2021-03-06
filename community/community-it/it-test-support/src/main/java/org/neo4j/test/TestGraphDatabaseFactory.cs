﻿using System.Collections.Generic;

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

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Org.Neo4j.Graphdb.config;
	using GraphDatabaseDependencies = Org.Neo4j.Graphdb.facade.GraphDatabaseDependencies;
	using GraphDatabaseFacadeFactory = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory;
	using GraphDatabaseBuilder = Org.Neo4j.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactory = Org.Neo4j.Graphdb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using AbstractEditionModule = Org.Neo4j.Graphdb.factory.module.edition.AbstractEditionModule;
	using CommunityEditionModule = Org.Neo4j.Graphdb.factory.module.edition.CommunityEditionModule;
	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using UncloseableDelegatingFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using URLAccessRule = Org.Neo4j.Graphdb.security.URLAccessRule;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using BoltConnector = Org.Neo4j.Kernel.configuration.BoltConnector;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using Org.Neo4j.Kernel.extension;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using Org.Neo4j.Kernel.Impl.Index.Schema;
	using StoreLocker = Org.Neo4j.Kernel.@internal.locker.StoreLocker;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using SimpleLogService = Org.Neo4j.Logging.@internal.SimpleLogService;
	using SystemNanoClock = Org.Neo4j.Time.SystemNanoClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.ephemeral;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Connector.ConnectorType.BOLT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.TRUE;


	/// <summary>
	/// Test factory for graph databases.
	/// Please be aware that since it's a database it will close filesystem as part of its lifecycle.
	/// If you expect your file system to be open after database is closed, use <seealso cref="UncloseableDelegatingFileSystemAbstraction"/>
	/// </summary>
	public class TestGraphDatabaseFactory : GraphDatabaseFactory
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static final System.Predicate<org.neo4j.kernel.extension.KernelExtensionFactory<?>> INDEX_PROVIDERS_FILTER = extension -> extension instanceof org.neo4j.kernel.impl.index.schema.AbstractIndexProviderFactory;
		 public static readonly System.Predicate<KernelExtensionFactory<object>> IndexProvidersFilter = extension => extension is AbstractIndexProviderFactory;

		 public TestGraphDatabaseFactory() : this(NullLogProvider.Instance)
		 {
		 }

		 public TestGraphDatabaseFactory( LogProvider logProvider ) : base( new TestGraphDatabaseFactoryState() )
		 {
			  UserLogProvider = logProvider;
		 }

		 public virtual GraphDatabaseService NewImpermanentDatabase()
		 {
			  return NewImpermanentDatabaseBuilder().newGraphDatabase();
		 }

		 public virtual GraphDatabaseService NewImpermanentDatabase( File storeDir )
		 {
			  File absoluteDirectory = storeDir.AbsoluteFile;
			  GraphDatabaseBuilder databaseBuilder = NewImpermanentDatabaseBuilder( absoluteDirectory );
			  databaseBuilder.setConfig( GraphDatabaseSettings.active_database, absoluteDirectory.Name );
			  databaseBuilder.setConfig( GraphDatabaseSettings.databases_root_path, absoluteDirectory.ParentFile.AbsolutePath );
			  return databaseBuilder.NewGraphDatabase();
		 }

		 public virtual GraphDatabaseService NewImpermanentDatabase<T1>( IDictionary<T1> config )
		 {
			  GraphDatabaseBuilder builder = NewImpermanentDatabaseBuilder();
			  SetConfig( config, builder );
			  return builder.NewGraphDatabase();
		 }

		 public virtual GraphDatabaseService NewImpermanentDatabase<T1>( File storeDir, IDictionary<T1> config )
		 {
			  GraphDatabaseBuilder builder = NewImpermanentDatabaseBuilder( storeDir );
			  SetConfig( config, builder );
			  return builder.NewGraphDatabase();
		 }

		 private void SetConfig<T1>( IDictionary<T1> config, GraphDatabaseBuilder builder )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Map.Entry<org.neo4j.graphdb.config.Setting<?>,String> entry : config.entrySet())
			  foreach ( KeyValuePair<Setting<object>, string> entry in config.SetOfKeyValuePairs() )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.graphdb.config.Setting<?> key = entry.getKey();
					Setting<object> key = entry.Key;
					string value = entry.Value;
					builder.SetConfig( key, value );
			  }
		 }

		 public virtual GraphDatabaseBuilder NewImpermanentDatabaseBuilder()
		 {
			  return NewImpermanentDatabaseBuilder( ImpermanentGraphDatabase.Path );
		 }

		 protected internal override void Configure( GraphDatabaseBuilder builder )
		 {
			  // Reduce the default page cache memory size to 8 mega-bytes for test databases.
			  builder.SetConfig( GraphDatabaseSettings.pagecache_memory, "8m" );
			  builder.setConfig( ( new BoltConnector( "bolt" ) ).type, BOLT.name() );
			  builder.SetConfig( ( new BoltConnector( "bolt" ) ).enabled, "false" );
		 }

		 private void Configure( GraphDatabaseBuilder builder, File storeDir )
		 {
			  Configure( builder );
			  builder.setConfig( GraphDatabaseSettings.logs_directory, ( new File( storeDir, "logs" ) ).AbsolutePath );
		 }

		 protected internal override TestGraphDatabaseFactoryState CurrentState
		 {
			 get
			 {
				  return ( TestGraphDatabaseFactoryState ) base.CurrentState;
			 }
		 }

		 protected internal override TestGraphDatabaseFactoryState StateCopy
		 {
			 get
			 {
				  return new TestGraphDatabaseFactoryState( CurrentState );
			 }
		 }

		 public virtual FileSystemAbstraction FileSystem
		 {
			 get
			 {
				  return CurrentState.FileSystem;
			 }
		 }

		 public virtual TestGraphDatabaseFactory setFileSystem( FileSystemAbstraction fileSystem )
		 {
			  CurrentState.FileSystem = fileSystem;
			  return this;
		 }

		 public override TestGraphDatabaseFactory setMonitors( Monitors monitors )
		 {
			  CurrentState.Monitors = monitors;
			  return this;
		 }

		 public override TestGraphDatabaseFactory setUserLogProvider( LogProvider logProvider )
		 {
			  return ( TestGraphDatabaseFactory ) base.setUserLogProvider( logProvider );
		 }

		 public virtual TestGraphDatabaseFactory setInternalLogProvider( LogProvider logProvider )
		 {
			  CurrentState.InternalLogProvider = logProvider;
			  return this;
		 }

		 public virtual TestGraphDatabaseFactory setClock( SystemNanoClock clock )
		 {
			  CurrentState.Clock = clock;
			  return this;
		 }

		 public virtual TestGraphDatabaseFactory AddKernelExtensions<T1>( IEnumerable<T1> newKernelExtensions )
		 {
			  CurrentState.addKernelExtensions( newKernelExtensions );
			  return this;
		 }

		 public virtual TestGraphDatabaseFactory AddKernelExtension<T1>( KernelExtensionFactory<T1> newKernelExtension )
		 {
			  return AddKernelExtensions( Collections.singletonList( newKernelExtension ) );
		 }

		 public virtual TestGraphDatabaseFactory setKernelExtensions<T1>( IEnumerable<T1> newKernelExtensions )
		 {
			  CurrentState.KernelExtensions = newKernelExtensions;
			  return this;
		 }

		 public virtual TestGraphDatabaseFactory RemoveKernelExtensions<T1>( System.Predicate<T1> filter )
		 {
			  CurrentState.removeKernelExtensions( filter );
			  return this;
		 }

		 public override TestGraphDatabaseFactory AddURLAccessRule( string protocol, URLAccessRule rule )
		 {
			  return ( TestGraphDatabaseFactory ) base.AddURLAccessRule( protocol, rule );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.graphdb.factory.GraphDatabaseBuilder newImpermanentDatabaseBuilder(final java.io.File storeDir)
		 public virtual GraphDatabaseBuilder NewImpermanentDatabaseBuilder( File storeDir )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final TestGraphDatabaseFactoryState state = getStateCopy();
			  TestGraphDatabaseFactoryState state = StateCopy;
			  GraphDatabaseBuilder.DatabaseCreator creator = CreateImpermanentDatabaseCreator( storeDir, state );
			  TestGraphDatabaseBuilder builder = CreateImpermanentGraphDatabaseBuilder( creator );
			  Configure( builder, storeDir );
			  return builder;
		 }

		 private TestGraphDatabaseBuilder CreateImpermanentGraphDatabaseBuilder( GraphDatabaseBuilder.DatabaseCreator creator )
		 {
			  return new TestGraphDatabaseBuilder( creator );
		 }

		 protected internal override GraphDatabaseService NewEmbeddedDatabase( File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies )
		 {
			  return ( new TestGraphDatabaseFacadeFactory( CurrentState ) ).newFacade( storeDir, config, GraphDatabaseDependencies.newDependencies( dependencies ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected org.neo4j.graphdb.factory.GraphDatabaseBuilder.DatabaseCreator createImpermanentDatabaseCreator(final java.io.File storeDir, final TestGraphDatabaseFactoryState state)
		 protected internal virtual GraphDatabaseBuilder.DatabaseCreator CreateImpermanentDatabaseCreator( File storeDir, TestGraphDatabaseFactoryState state )
		 {
			  return new DatabaseCreatorAnonymousInnerClass( this, storeDir, state );
		 }

		 private class DatabaseCreatorAnonymousInnerClass : GraphDatabaseBuilder.DatabaseCreator
		 {
			 private readonly TestGraphDatabaseFactory _outerInstance;

			 private File _storeDir;
			 private Org.Neo4j.Test.TestGraphDatabaseFactoryState _state;

			 public DatabaseCreatorAnonymousInnerClass( TestGraphDatabaseFactory outerInstance, File storeDir, Org.Neo4j.Test.TestGraphDatabaseFactoryState state )
			 {
				 this.outerInstance = outerInstance;
				 this._storeDir = storeDir;
				 this._state = state;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public org.neo4j.graphdb.GraphDatabaseService newDatabase(@Nonnull Config config)
			 public GraphDatabaseService newDatabase( Config config )
			 {
				  return ( new TestGraphDatabaseFacadeFactory( _state, true ) ).newFacade( _storeDir, config, GraphDatabaseDependencies.newDependencies( _state.databaseDependencies() ) );
			 }
		 }

		 public class TestGraphDatabaseFacadeFactory : GraphDatabaseFacadeFactory
		 {
			  internal readonly TestGraphDatabaseFactoryState State;
			  internal readonly bool Impermanent;

			  protected internal TestGraphDatabaseFacadeFactory( TestGraphDatabaseFactoryState state, bool impermanent ) : this( state, impermanent, DatabaseInfo.COMMUNITY, CommunityEditionModule::new )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  }

			  protected internal TestGraphDatabaseFacadeFactory( TestGraphDatabaseFactoryState state, bool impermanent, DatabaseInfo databaseInfo, System.Func<PlatformModule, AbstractEditionModule> editionFactory ) : base( databaseInfo, editionFactory )
			  {
					this.State = state;
					this.Impermanent = impermanent;
			  }

			  internal TestGraphDatabaseFacadeFactory( TestGraphDatabaseFactoryState state ) : this( state, false )
			  {
			  }

			  protected internal override PlatformModule CreatePlatform( File storeDir, Config config, Dependencies dependencies )
			  {
					File absoluteStoreDir = storeDir.AbsoluteFile;
					File databasesRoot = absoluteStoreDir.ParentFile;
					if ( !config.IsConfigured( GraphDatabaseSettings.shutdown_transaction_end_timeout ) )
					{
						 config.Augment( GraphDatabaseSettings.shutdown_transaction_end_timeout, "0s" );
					}
					config.Augment( GraphDatabaseSettings.ephemeral, Settings.FALSE );
					config.augment( GraphDatabaseSettings.active_database, absoluteStoreDir.Name );
					config.augment( GraphDatabaseSettings.databases_root_path, databasesRoot.AbsolutePath );
					if ( Impermanent )
					{
						 config.augment( ephemeral, TRUE );
						 return new ImpermanentTestDatabasePlatformModule( this, databasesRoot, config, dependencies, this.DatabaseInfo );
					}
					else
					{
						 return new TestDatabasePlatformModule( this, databasesRoot, config, dependencies, this.DatabaseInfo );
					}
			  }

			  internal class TestDatabasePlatformModule : PlatformModule
			  {
				  private readonly TestGraphDatabaseFactory.TestGraphDatabaseFacadeFactory _outerInstance;


					internal TestDatabasePlatformModule( TestGraphDatabaseFactory.TestGraphDatabaseFacadeFactory outerInstance, File storeDir, Config config, Dependencies dependencies, DatabaseInfo databaseInfo ) : base( storeDir, config, databaseInfo, dependencies )
					{
						this._outerInstance = outerInstance;
					}

					protected internal override FileSystemAbstraction CreateFileSystemAbstraction()
					{
						 FileSystemAbstraction fs = outerInstance.State.FileSystem;
						 if ( fs != null )
						 {
							  return fs;
						 }
						 else
						 {
							  return CreateNewFileSystem();
						 }
					}

					protected internal virtual FileSystemAbstraction CreateNewFileSystem()
					{
						 return base.CreateFileSystemAbstraction();
					}

					protected internal override LogService CreateLogService( LogProvider userLogProvider )
					{
						 LogProvider internalLogProvider = outerInstance.State.InternalLogProvider;
						 if ( internalLogProvider == null )
						 {
							  if ( !outerInstance.Impermanent )
							  {
									return base.CreateLogService( userLogProvider );
							  }
							  internalLogProvider = NullLogProvider.Instance;
						 }
						 return new SimpleLogService( userLogProvider, internalLogProvider );
					}

					protected internal override SystemNanoClock CreateClock()
					{
						 SystemNanoClock clock = outerInstance.State.clock();
						 return clock != null ? clock : base.CreateClock();
					}
			  }

			  private class ImpermanentTestDatabasePlatformModule : TestDatabasePlatformModule
			  {
				  private readonly TestGraphDatabaseFactory.TestGraphDatabaseFacadeFactory _outerInstance;


					internal ImpermanentTestDatabasePlatformModule( TestGraphDatabaseFactory.TestGraphDatabaseFacadeFactory outerInstance, File storeDir, Config config, Dependencies dependencies, DatabaseInfo databaseInfo ) : base( outerInstance, storeDir, config, dependencies, databaseInfo )
					{
						this._outerInstance = outerInstance;
					}

					protected internal override FileSystemAbstraction CreateNewFileSystem()
					{
						 return new EphemeralFileSystemAbstraction();
					}

					protected internal override StoreLocker CreateStoreLocker()
					{
						 return new StoreLocker( FileSystem, StoreLayout );
					}
			  }
		 }
	}

}