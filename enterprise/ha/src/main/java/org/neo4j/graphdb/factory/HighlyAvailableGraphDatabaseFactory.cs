using System;

/*
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
namespace Org.Neo4j.Graphdb.factory
{

	using ClusterSettings = Org.Neo4j.cluster.ClusterSettings;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using HaSettings = Org.Neo4j.Kernel.ha.HaSettings;
	using HighlyAvailableGraphDatabase = Org.Neo4j.Kernel.ha.HighlyAvailableGraphDatabase;
	using Edition = Org.Neo4j.Kernel.impl.factory.Edition;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;

	/// <summary>
	/// Factory for Neo4j database instances with Enterprise Edition and High-Availability features.
	/// </summary>
	/// <seealso cref= org.neo4j.graphdb.factory.GraphDatabaseFactory </seealso>
	/// @deprecated high availability database/edition is deprecated in favour of causal clustering. It will be removed in next major release. 
	[Obsolete("high availability database/edition is deprecated in favour of causal clustering. It will be removed in next major release.")]
	public class HighlyAvailableGraphDatabaseFactory : GraphDatabaseFactory
	{
		 public HighlyAvailableGraphDatabaseFactory() : base(HighlyAvailableFactoryState())
		 {
		 }

		 private static GraphDatabaseFactoryState HighlyAvailableFactoryState()
		 {
			  GraphDatabaseFactoryState state = new GraphDatabaseFactoryState();
			  state.AddSettingsClasses( asList( typeof( ClusterSettings ), typeof( HaSettings ) ) );
			  return state;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected GraphDatabaseBuilder.DatabaseCreator createDatabaseCreator(final java.io.File storeDir, final GraphDatabaseFactoryState state)
		 protected internal override GraphDatabaseBuilder.DatabaseCreator CreateDatabaseCreator( File storeDir, GraphDatabaseFactoryState state )
		 {
			  return new DatabaseCreatorAnonymousInnerClass( this, storeDir, state );
		 }

		 private class DatabaseCreatorAnonymousInnerClass : GraphDatabaseBuilder.DatabaseCreator
		 {
			 private readonly HighlyAvailableGraphDatabaseFactory _outerInstance;

			 private File _storeDir;
			 private Org.Neo4j.Graphdb.factory.GraphDatabaseFactoryState _state;

			 public DatabaseCreatorAnonymousInnerClass( HighlyAvailableGraphDatabaseFactory outerInstance, File storeDir, Org.Neo4j.Graphdb.factory.GraphDatabaseFactoryState state )
			 {
				 this.outerInstance = outerInstance;
				 this._storeDir = storeDir;
				 this._state = state;
			 }

			 public GraphDatabaseService newDatabase( Config config )
			 {
				  File absoluteStoreDir = _storeDir.AbsoluteFile;
				  File databasesRoot = absoluteStoreDir.ParentFile;
				  config.Augment( GraphDatabaseSettings.Ephemeral, Settings.FALSE );
				  config.augment( GraphDatabaseSettings.ActiveDatabase, absoluteStoreDir.Name );
				  config.augment( GraphDatabaseSettings.DatabasesRootPath, databasesRoot.AbsolutePath );
				  return new HighlyAvailableGraphDatabase( databasesRoot, config, _state.databaseDependencies() );
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