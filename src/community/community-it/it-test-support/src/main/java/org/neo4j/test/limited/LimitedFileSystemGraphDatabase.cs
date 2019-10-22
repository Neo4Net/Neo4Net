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
namespace Neo4Net.Test.limited
{

	using GraphDatabaseFacadeFactory = Neo4Net.GraphDb.facade.GraphDatabaseFacadeFactory;
	using PlatformModule = Neo4Net.GraphDb.factory.module.PlatformModule;
	using CommunityEditionModule = Neo4Net.GraphDb.factory.module.edition.CommunityEditionModule;
	using UncloseableDelegatingFileSystemAbstraction = Neo4Net.GraphDb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;

	public class LimitedFileSystemGraphDatabase : ImpermanentGraphDatabase
	{
		 private FileSystemAbstraction _fs;
		 private LimitedFilesystemAbstraction _limitedFs;

		 public LimitedFileSystemGraphDatabase( File storeDir ) : base( storeDir )
		 {
		 }

		 protected internal override void Create( File storeDir, IDictionary<string, string> @params, GraphDatabaseFacadeFactory.Dependencies dependencies )
		 {
			  new GraphDatabaseFacadeFactoryAnonymousInnerClass( this, DatabaseInfo.COMMUNITY, storeDir, dependencies )
			  .initFacade( storeDir, @params, dependencies, this );
		 }

		 private class GraphDatabaseFacadeFactoryAnonymousInnerClass : GraphDatabaseFacadeFactory
		 {
			 private readonly LimitedFileSystemGraphDatabase _outerInstance;

			 private File _storeDir;
			 private GraphDatabaseFacadeFactory.Dependencies _dependencies;

			 public GraphDatabaseFacadeFactoryAnonymousInnerClass( LimitedFileSystemGraphDatabase outerInstance, DatabaseInfo community, File storeDir, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( community, CommunityEditionModule::new )
			 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
				 this.outerInstance = outerInstance;
				 this._storeDir = storeDir;
				 this._dependencies = dependencies;
			 }

			 protected internal override PlatformModule createPlatform( File storeDir, Config config, Dependencies dependencies )
			 {
				  return new ImpermanentPlatformModuleAnonymousInnerClass( this, storeDir, config, databaseInfo, dependencies );
			 }

			 private class ImpermanentPlatformModuleAnonymousInnerClass : ImpermanentPlatformModule
			 {
				 private readonly GraphDatabaseFacadeFactoryAnonymousInnerClass _outerInstance;

				 public ImpermanentPlatformModuleAnonymousInnerClass( GraphDatabaseFacadeFactoryAnonymousInnerClass outerInstance, File storeDir, Config config, UnknownType databaseInfo, GraphDatabaseFacadeFactory.Dependencies dependencies ) : base( storeDir, config, databaseInfo, dependencies )
				 {
					 this.outerInstance = outerInstance;
				 }

				 protected internal override FileSystemAbstraction createFileSystemAbstraction()
				 {
					  _outerInstance.outerInstance.fs = base.createFileSystemAbstraction();
					  _outerInstance.outerInstance.limitedFs = new LimitedFilesystemAbstraction( new UncloseableDelegatingFileSystemAbstraction( _outerInstance.outerInstance.fs ) );
					  return _outerInstance.outerInstance.limitedFs;
				 }
			 }
		 }

		 public virtual void RunOutOfDiskSpaceNao()
		 {
			  this._limitedFs.runOutOfDiskSpace( true );
		 }

		 public virtual void SomehowGainMoreDiskSpace()
		 {
			  this._limitedFs.runOutOfDiskSpace( false );
		 }

		 public virtual FileSystemAbstraction FileSystem
		 {
			 get
			 {
				  return _fs;
			 }
		 }
	}

}