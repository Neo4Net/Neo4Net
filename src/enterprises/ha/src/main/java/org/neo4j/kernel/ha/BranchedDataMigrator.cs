using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Kernel.ha
{

	using StoreUtil = Neo4Net.com.storecopy.StoreUtil;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

	public class BranchedDataMigrator : LifecycleAdapter
	{
		 private readonly File _storeDir;

		 public BranchedDataMigrator( File storeDir )
		 {
			  this._storeDir = storeDir;
		 }

		 public override void Start()
		 {
			  MigrateBranchedDataDirectoriesToRootDirectory();
		 }

		 private void MigrateBranchedDataDirectoriesToRootDirectory()
		 {
			  File branchedDir = StoreUtil.getBranchedDataRootDirectory( _storeDir );
			  branchedDir.mkdirs();
			  foreach ( File oldBranchedDir in _storeDir.listFiles() )
			  {
					if ( !oldBranchedDir.Directory || !oldBranchedDir.Name.StartsWith( "branched-" ) )
					{
						 continue;
					}

					long timestamp = 0;
					try
					{
						 timestamp = long.Parse( oldBranchedDir.Name.substring( oldBranchedDir.Name.IndexOf( '-' ) + 1 ) );
					}
					catch ( System.FormatException )
					{ // OK, it wasn't a branched directory after all.
						 continue;
					}

					File targetDir = StoreUtil.getBranchedDataDirectory( _storeDir, timestamp );
					try
					{
						 FileUtils.moveFile( oldBranchedDir, targetDir );
					}
					catch ( IOException e )
					{
						 throw new Exception( "Couldn't move branched directories to " + branchedDir, e );
					}
			  }
		 }
	}

}