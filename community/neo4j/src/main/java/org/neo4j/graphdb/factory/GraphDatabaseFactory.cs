﻿using System;
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
namespace Org.Neo4j.Graphdb.factory
{

	using GraphDatabaseFacadeFactory = Org.Neo4j.Graphdb.facade.GraphDatabaseFacadeFactory;
	using CommunityEditionModule = Org.Neo4j.Graphdb.factory.module.edition.CommunityEditionModule;
	using URLAccessRule = Org.Neo4j.Graphdb.security.URLAccessRule;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using Edition = Org.Neo4j.Kernel.impl.factory.Edition;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	/// <summary>
	/// Creates a <seealso cref="org.neo4j.graphdb.GraphDatabaseService"/> with Community Edition features.
	/// <para>
	/// Use <seealso cref="newEmbeddedDatabase(File)"/> or
	/// <seealso cref="newEmbeddedDatabaseBuilder(File)"/> to create a database instance.
	/// </para>
	/// <para>
	/// <strong>Note:</strong> If you are using the Enterprise Edition of Neo4j in embedded mode, you have to create your
	/// database with the <a href="EnterpriseGraphDatabaseFactory.html">{@code EnterpriseGraphDatabaseFactory}</a>
	/// to enable the Enterprise Edition features, or the
	/// <a href="HighlyAvailableGraphDatabaseFactory.html">{@code HighlyAvailableGraphDatabaseFactory}</a> for the
	/// Enterprise and High-Availability features. There is no factory for the Causal Clustering features, because it is
	/// currently not possible to run a causal cluster in embedded mode.
	/// </para>
	/// </summary>
	public class GraphDatabaseFactory
	{
		 private readonly GraphDatabaseFactoryState _state;

		 public GraphDatabaseFactory() : this(new GraphDatabaseFactoryState())
		 {
		 }

		 protected internal GraphDatabaseFactory( GraphDatabaseFactoryState state )
		 {
			  this._state = state;
		 }

		 protected internal virtual GraphDatabaseFactoryState CurrentState
		 {
			 get
			 {
				  return _state;
			 }
		 }

		 protected internal virtual GraphDatabaseFactoryState StateCopy
		 {
			 get
			 {
				  return new GraphDatabaseFactoryState( CurrentState );
			 }
		 }

		 /// <param name="storeDir"> desired embedded database store dir </param>
		 public virtual GraphDatabaseService NewEmbeddedDatabase( File storeDir )
		 {
			  return NewEmbeddedDatabaseBuilder( storeDir ).newGraphDatabase();
		 }

		 /// <param name="storeDir"> desired embedded database store dir </param>
		 public virtual GraphDatabaseBuilder NewEmbeddedDatabaseBuilder( File storeDir )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final GraphDatabaseFactoryState state = getStateCopy();
			  GraphDatabaseFactoryState state = StateCopy;
			  GraphDatabaseBuilder.DatabaseCreator creator = CreateDatabaseCreator( storeDir, state );
			  GraphDatabaseBuilder builder = CreateGraphDatabaseBuilder( creator );
			  Configure( builder );
			  return builder;
		 }

		 protected internal virtual GraphDatabaseBuilder CreateGraphDatabaseBuilder( GraphDatabaseBuilder.DatabaseCreator creator )
		 {
			  return new GraphDatabaseBuilder( creator );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected GraphDatabaseBuilder.DatabaseCreator createDatabaseCreator(final java.io.File storeDir, final GraphDatabaseFactoryState state)
		 protected internal virtual GraphDatabaseBuilder.DatabaseCreator CreateDatabaseCreator( File storeDir, GraphDatabaseFactoryState state )
		 {
			  return new EmbeddedDatabaseCreator( this, storeDir, state );
		 }

		 protected internal virtual void Configure( GraphDatabaseBuilder builder )
		 {
			  // Let the default configuration pass through.
		 }

		 /// <summary>
		 /// See <seealso cref="newDatabase(File, Config, GraphDatabaseFacadeFactory.Dependencies)"/> instead.
		 /// </summary>
		 [Obsolete]
		 protected internal virtual GraphDatabaseService NewDatabase( File storeDir, IDictionary<string, string> settings, GraphDatabaseFacadeFactory.Dependencies dependencies )
		 {
			  return NewDatabase( storeDir, Config.defaults( settings ), dependencies );
		 }

		 protected internal virtual GraphDatabaseService NewEmbeddedDatabase( File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies )
		 {
			  return GraphDatabaseFactory.this.NewDatabase( storeDir, config, dependencies );
		 }

		 protected internal virtual GraphDatabaseService NewDatabase( File storeDir, Config config, GraphDatabaseFacadeFactory.Dependencies dependencies )
		 {
			  File absoluteStoreDir = storeDir.AbsoluteFile;
			  File databasesRoot = absoluteStoreDir.ParentFile;
			  config.Augment( GraphDatabaseSettings.Ephemeral, Settings.FALSE );
			  config.augment( GraphDatabaseSettings.ActiveDatabase, absoluteStoreDir.Name );
			  config.augment( GraphDatabaseSettings.DatabasesRootPath, databasesRoot.AbsolutePath );
			  return GraphDatabaseFacadeFactory.newFacade( databasesRoot, config, dependencies );
		 }

		 protected internal virtual GraphDatabaseFacadeFactory GraphDatabaseFacadeFactory
		 {
			 get
			 {
	//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
				  return new GraphDatabaseFacadeFactory( DatabaseInfo.COMMUNITY, CommunityEditionModule::new );
			 }
		 }

		 public virtual GraphDatabaseFactory AddURLAccessRule( string protocol, URLAccessRule rule )
		 {
			  CurrentState.addURLAccessRule( protocol, rule );
			  return this;
		 }

		 public virtual GraphDatabaseFactory setUserLogProvider( LogProvider userLogProvider )
		 {
			  CurrentState.UserLogProvider = userLogProvider;
			  return this;
		 }

		 public virtual GraphDatabaseFactory setMonitors( Monitors monitors )
		 {
			  CurrentState.Monitors = monitors;
			  return this;
		 }

		 public virtual string Edition
		 {
			 get
			 {
				  return Edition.community.ToString();
			 }
		 }

		 private class EmbeddedDatabaseCreator : GraphDatabaseBuilder.DatabaseCreator
		 {
			 private readonly GraphDatabaseFactory _outerInstance;

			  internal readonly File StoreDir;
			  internal readonly GraphDatabaseFactoryState State;

			  internal EmbeddedDatabaseCreator( GraphDatabaseFactory outerInstance, File storeDir, GraphDatabaseFactoryState state )
			  {
				  this._outerInstance = outerInstance;
					this.StoreDir = storeDir;
					this.State = state;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public org.neo4j.graphdb.GraphDatabaseService newDatabase(@Nonnull Config config)
			  public override GraphDatabaseService NewDatabase( Config config )
			  {
					return outerInstance.NewEmbeddedDatabase( StoreDir, config, State.databaseDependencies() );
			  }
		 }
	}

}