using System;

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
namespace Neo4Net.causalclustering.core
{

	using Neo4Net.causalclustering.core.state.storage;
	using Neo4Net.causalclustering.core.state.storage;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using PlatformModule = Neo4Net.GraphDb.factory.module.PlatformModule;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public class IdentityModule
	{
		 public const string CORE_MEMBER_ID_NAME = "core-member-id";

		 private MemberId _myself;

		 internal IdentityModule( PlatformModule platformModule, File clusterStateDirectory )
		 {
			  FileSystemAbstraction fileSystem = platformModule.FileSystem;
			  LogProvider logProvider = platformModule.Logging.InternalLogProvider;

			  Log log = logProvider.getLog( this.GetType() );

			  SimpleStorage<MemberId> memberIdStorage = new SimpleFileStorage<MemberId>( fileSystem, clusterStateDirectory, CORE_MEMBER_ID_NAME, new MemberId.Marshal(), logProvider );

			  try
			  {
					if ( memberIdStorage.Exists() )
					{
						 _myself = memberIdStorage.ReadState();
						 if ( _myself == null )
						 {
							  throw new Exception( "I was null" );
						 }
					}
					else
					{
						 System.Guid uuid = System.Guid.randomUUID();
						 _myself = new MemberId( uuid );
						 memberIdStorage.WriteState( _myself );

						 log.Info( string.Format( "Generated new id: {0} ({1})", _myself, uuid ) );
					}
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }

			  platformModule.JobScheduler.TopLevelGroupName = "Core " + _myself;
		 }

		 public virtual MemberId Myself()
		 {
			  return _myself;
		 }
	}

}