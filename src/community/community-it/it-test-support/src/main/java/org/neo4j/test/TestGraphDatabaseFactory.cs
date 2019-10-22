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
namespace Neo4Net.Test
{

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Neo4Net.GraphDb.config;
	using GraphDatabaseDependencies = Neo4Net.GraphDb.facade.GraphDatabaseDependencies;
	using GraphDatabaseFacadeFactory = Neo4Net.GraphDb.facade.GraphDatabaseFacadeFactory;
	using GraphDatabaseBuilder = Neo4Net.GraphDb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactory = Neo4Net.GraphDb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using PlatformModule = Neo4Net.GraphDb.factory.module.PlatformModule;
	using AbstractEditionModule = Neo4Net.GraphDb.factory.module.edition.AbstractEditionModule;
	using CommunityEditionModule = Neo4Net.GraphDb.factory.module.edition.CommunityEditionModule;
	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using UncloseableDelegatingFileSystemAbstraction = Neo4Net.GraphDb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using URLAccessRule = Neo4Net.GraphDb.security.URLAccessRule;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using Neo4Net.Kernel.extension;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using Neo4Net.Kernel.Impl.Index.Schema;
	using StoreLocker = Neo4Net.Kernel.Internal.locker.StoreLocker;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using SimpleLogService = Neo4Net.Logging.Internal.SimpleLogService;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.ephemeral;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Connector.ConnectorType.BOLT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.TRUE;


	/// <summary>
	/// Test factory for graph databases.
	/// Please be aware that since it's a database it will close filesystem as part of its lifecycle.
	/// If you expect your file system to be open after database is closed, use <seealso cref="UncloseableDelegatingFileSystemAbstraction"/>
	/// </summary>
	public class TestGraphDatabaseFactory : GraphDatabaseFactory
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static final System.Predicate<org.Neo4Net.kernel.extension.KernelExtensionFactory<?>> INDEX_PROVIDERS_FILTER = extension -> extension instanceof org.Neo4Net.kernel.impl.index.schema.AbstractIndexProviderFactory;
		 public static readonly System.Predicate<KernelExtensionFactory<object>> IndexProvidersFilter = extension => extension is AbstractIndexProviderFactory;

		 public TestGraphDatabaseFactory() : this(NullLogProvider.Instance)
		 {
		 }

		 public TestGraphDatabaseFactory( LogProvider logProvider ) : base( new TestGraphDatabaseFactoryState() )
		 {
			  UserLogProvider = logProvider;
		 }

		 public virtual IGraphDatabaseService NewImpermanentDatabase()
		 {
			  return NewImpermanentDatabaseBuilder().newGraphDatabase();
		 }

		 public virtual IGraphDatabaseService NewImpermanentDatabase( File storeDir )
		 {
			  File absoluteDirectory = storeDir.AbsoluteFile;
			  GraphDatabaseBuilder databaseBuilder = NewImpermanentDatabaseBuilder( absoluteDirectory );
			  databaseBuilder.setConfig( GraphDatabaseSettings.active_database, absoluteDirectory.Name );
			  databaseBuilder.setConfig( GraphDatabaseSettings.databases_root_path, absoluteDirectory.ParentFile.AbsolutePath );
			  return databaseBuilder.NewGraphDatabase();
		 }

		 public virtual IGraphDatabaseService NewImpermanentDatabase<T1>( IDictionary<T1> config )
		 {
			  GraphDatabaseBuilder builder = NewImpermanentDatabaseBuilder();
			  SetConfig( config, builder );
			  return builder.NewGraphDatabase();
		 }

		 public virtual IGraphDatabaseService NewImpermanentDatabase<T1>( File storeDir, IDictionary<T1> config )
		 {
			  GraphDatabaseBuilder builder = NewImpermanentDatabaseBuilder( storeDir );
			  SetConfig( config, builder );
			  return builder.NewGraphDatabase();
		 }

		 private void SetConfig<T1>( IDictionary<T1> config, GraphDatabaseBuilder builder )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Map.Entry<org.Neo4Net.graphdb.config.Setting<?>,String> entry : config.entrySet())
			  foreach ( KeyValuePair<Setting<object>, string> entry in config.SetOfKeyValuePairs() )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.graphdb.config.Setting<?> key = entry.getKey();
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
//ORIGINAL LINE: public org.Neo4Net.graphdb.factory.GraphDatabaseBuilder newImpermanentDatabaseBuilder(final java.io.File storeDir)
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

		 protected internal override IGraphDatabaseService NewEmbeddedDatabase( File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies )
		 {
			  return ( new TestGraphDatabaseFacadeFactory( CurrentState ) ).newFacade( storeDir, config, GraphDatabaseDependencies.newDependencies( dependencies ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected org.Neo4Net.graphdb.factory.GraphDatabaseBuilder.DatabaseCreator createImpermanentDatabaseCreator(final java.io.File storeDir, final TestGraphDatabaseFactoryState state)
		 protected internal virtual GraphDatabaseBuilder.DatabaseCreator CreateImpermanentDatabaseCreator( File storeDir, TestGraphDatabaseFactoryState state )
		 {
			  return new DatabaseCreatorAnonymousInnerClass( this, storeDir, state );
		 }

		 private class DatabaseCreatorAnonymousInnerClass : GraphDatabaseBuilder.DatabaseCreator
		 {
			 private readonly TestGraphDatabaseFactory _outerInstance;

			 private File _storeDir;
			 private Neo4Net.Test.TestGraphDatabaseFactoryState _state;

			 public DatabaseCreatorAnonymousInnerClass( TestGraphDatabaseFactory outerInstance, File storeDir, Neo4Net.Test.TestGraphDatabaseFactoryState state )
			 {
				 this.outerInstance = outerInstance;
				 this._storeDir = storeDir;
				 this._state = state;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public org.Neo4Net.graphdb.GraphDatabaseService newDatabase(@Nonnull Config config)
			 public IGraphDatabaseService newDatabase( Config config )
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