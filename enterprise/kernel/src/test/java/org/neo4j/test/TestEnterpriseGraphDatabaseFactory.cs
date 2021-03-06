﻿/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Test
{

	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using GraphDatabaseDependencies = Org.Neo4j.Graphdb.facade.GraphDatabaseDependencies;
	using GraphDatabaseFacadeFactory = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory;
	using GraphDatabaseBuilder = Org.Neo4j.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactoryState = Org.Neo4j.Graphdb.factory.GraphDatabaseFactoryState;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using EnterpriseEditionModule = Org.Neo4j.Kernel.impl.enterprise.EnterpriseEditionModule;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using Edition = Org.Neo4j.Kernel.impl.factory.Edition;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using SimpleLogService = Org.Neo4j.Logging.@internal.SimpleLogService;

	/// <summary>
	/// Factory for test graph database.
	/// </summary>
	public class TestEnterpriseGraphDatabaseFactory : TestGraphDatabaseFactory
	{
		 public TestEnterpriseGraphDatabaseFactory() : base()
		 {
		 }

		 public TestEnterpriseGraphDatabaseFactory( LogProvider logProvider ) : base( logProvider )
		 {
		 }

		 protected internal override GraphDatabaseBuilder.DatabaseCreator CreateDatabaseCreator( File storeDir, GraphDatabaseFactoryState state )
		 {
			  return new DatabaseCreatorAnonymousInnerClass( this, storeDir, state );
		 }

		 private class DatabaseCreatorAnonymousInnerClass : GraphDatabaseBuilder.DatabaseCreator
		 {
			 private readonly TestEnterpriseGraphDatabaseFactory _outerInstance;

			 private File _storeDir;
			 private GraphDatabaseFactoryState _state;

			 public DatabaseCreatorAnonymousInnerClass( TestEnterpriseGraphDatabaseFactory outerInstance, File storeDir, GraphDatabaseFactoryState state )
			 {
				 this.outerInstance = outerInstance;
				 this._storeDir = storeDir;
				 this._state = state;
			 }

			 public GraphDatabaseService newDatabase( Config config )
			 {
				  File absoluteStoreDir = _storeDir.AbsoluteFile;
				  File databasesRoot = absoluteStoreDir.ParentFile;
				  if ( !config.IsConfigured( GraphDatabaseSettings.shutdown_transaction_end_timeout ) )
				  {
						config.Augment( GraphDatabaseSettings.shutdown_transaction_end_timeout, "0s" );
				  }
				  config.Augment( GraphDatabaseSettings.ephemeral, Settings.FALSE );
				  config.augment( GraphDatabaseSettings.active_database, absoluteStoreDir.Name );
				  config.augment( GraphDatabaseSettings.databases_root_path, databasesRoot.AbsolutePath );
				  return new GraphDatabaseFacadeFactoryAnonymousInnerClass( this, DatabaseInfo.ENTERPRISE, config )
				  .newFacade( databasesRoot, config, GraphDatabaseDependencies.newDependencies( _state.databaseDependencies() ) );
			 }

			 private class GraphDatabaseFacadeFactoryAnonymousInnerClass : GraphDatabaseFacadeFactory
			 {
				 private readonly DatabaseCreatorAnonymousInnerClass _outerInstance;

				 private Config _config;

				 public GraphDatabaseFacadeFactoryAnonymousInnerClass( DatabaseCreatorAnonymousInnerClass outerInstance, DatabaseInfo enterprise, Config config ) : base( enterprise, EnterpriseEditionModule::new )
				 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					 this.outerInstance = outerInstance;
					 this._config = config;
				 }

				 protected internal override PlatformModule createPlatform( File storeDir, Config config, Dependencies dependencies )
				 {
					  return new PlatformModuleAnonymousInnerClass( this, storeDir, config, databaseInfo, dependencies );
				 }

				 private class PlatformModuleAnonymousInnerClass : PlatformModule
				 {
					 private readonly GraphDatabaseFacadeFactoryAnonymousInnerClass _outerInstance;

					 public PlatformModuleAnonymousInnerClass( GraphDatabaseFacadeFactoryAnonymousInnerClass outerInstance, File storeDir, Config config, UnknownType databaseInfo, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( storeDir, config, databaseInfo, dependencies )
					 {
						 this.outerInstance = outerInstance;
					 }

					 protected internal override LogService createLogService( LogProvider userLogProvider )
					 {
						  if ( _outerInstance.outerInstance.state is TestGraphDatabaseFactoryState )
						  {
								LogProvider logProvider = ( ( TestGraphDatabaseFactoryState ) _outerInstance.outerInstance.state ).InternalLogProvider;
								if ( logProvider != null )
								{
									 return new SimpleLogService( logProvider );
								}
						  }
						  return base.createLogService( userLogProvider );
					 }
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected org.neo4j.graphdb.factory.GraphDatabaseBuilder.DatabaseCreator createImpermanentDatabaseCreator(final java.io.File storeDir, final TestGraphDatabaseFactoryState state)
		 protected internal override GraphDatabaseBuilder.DatabaseCreator CreateImpermanentDatabaseCreator( File storeDir, TestGraphDatabaseFactoryState state )
		 {
			  return new DatabaseCreatorAnonymousInnerClass2( this, storeDir, state );
		 }

		 private class DatabaseCreatorAnonymousInnerClass2 : GraphDatabaseBuilder.DatabaseCreator
		 {
			 private readonly TestEnterpriseGraphDatabaseFactory _outerInstance;

			 private File _storeDir;
			 private Org.Neo4j.Test.TestGraphDatabaseFactoryState _state;

			 public DatabaseCreatorAnonymousInnerClass2( TestEnterpriseGraphDatabaseFactory outerInstance, File storeDir, Org.Neo4j.Test.TestGraphDatabaseFactoryState state )
			 {
				 this.outerInstance = outerInstance;
				 this._storeDir = storeDir;
				 this._state = state;
			 }

			 public GraphDatabaseService newDatabase( Config config )
			 {
				  return ( new TestEnterpriseGraphDatabaseFacadeFactory( _state, true ) ).newFacade( _storeDir, config, GraphDatabaseDependencies.newDependencies( _state.databaseDependencies() ) );
			 }
		 }

		 internal class TestEnterpriseGraphDatabaseFacadeFactory : TestGraphDatabaseFacadeFactory
		 {

			  internal TestEnterpriseGraphDatabaseFacadeFactory( TestGraphDatabaseFactoryState state, bool impermanent ) : base( state, impermanent, DatabaseInfo.ENTERPRISE, EnterpriseEditionModule::new )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  }
		 }

		 public override string Edition
		 {
			 get
			 {
				  return Edition.enterprise.ToString();
			 }
		 }
	}

}