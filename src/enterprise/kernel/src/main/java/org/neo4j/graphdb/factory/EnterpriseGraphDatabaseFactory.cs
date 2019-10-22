/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.GraphDb.factory
{

	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using EnterpriseGraphDatabase = Neo4Net.Kernel.enterprise.EnterpriseGraphDatabase;
	using Edition = Neo4Net.Kernel.impl.factory.Edition;

	/// <summary>
	/// Factory for Neo4Net database instances with Enterprise Edition features.
	/// </summary>
	/// <seealso cref= GraphDatabaseFactory </seealso>
	public class EnterpriseGraphDatabaseFactory : GraphDatabaseFactory
	{
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected GraphDatabaseBuilder.DatabaseCreator createDatabaseCreator(final java.io.File storeDir, final GraphDatabaseFactoryState state)
		 protected internal override GraphDatabaseBuilder.DatabaseCreator CreateDatabaseCreator( File storeDir, GraphDatabaseFactoryState state )
		 {
			  return new DatabaseCreatorAnonymousInnerClass( this, storeDir, state );
		 }

		 private class DatabaseCreatorAnonymousInnerClass : GraphDatabaseBuilder.DatabaseCreator
		 {
			 private readonly EnterpriseGraphDatabaseFactory _outerInstance;

			 private File _storeDir;
			 private Neo4Net.GraphDb.factory.GraphDatabaseFactoryState _state;

			 public DatabaseCreatorAnonymousInnerClass( EnterpriseGraphDatabaseFactory outerInstance, File storeDir, Neo4Net.GraphDb.factory.GraphDatabaseFactoryState state )
			 {
				 this.outerInstance = outerInstance;
				 this._storeDir = storeDir;
				 this._state = state;
			 }

			 public IGraphDatabaseService newDatabase( Config config )
			 {
				  File absoluteStoreDir = _storeDir.AbsoluteFile;
				  File databasesRoot = absoluteStoreDir.ParentFile;
				  config.Augment( GraphDatabaseSettings.Ephemeral, Settings.FALSE );
				  config.augment( GraphDatabaseSettings.ActiveDatabase, absoluteStoreDir.Name );
				  config.augment( GraphDatabaseSettings.DatabasesRootPath, databasesRoot.AbsolutePath );
				  return new EnterpriseGraphDatabase( databasesRoot, config, _state.databaseDependencies() );
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