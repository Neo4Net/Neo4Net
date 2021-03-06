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
namespace Org.Neo4j.causalclustering.management
{


	using RaftMachine = Org.Neo4j.causalclustering.core.consensus.RaftMachine;
	using ClusterStateDirectory = Org.Neo4j.causalclustering.core.state.ClusterStateDirectory;
	using Service = Org.Neo4j.Helpers.Service;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using FileUtils = Org.Neo4j.Io.fs.FileUtils;
	using ManagementBeanProvider = Org.Neo4j.Jmx.impl.ManagementBeanProvider;
	using ManagementData = Org.Neo4j.Jmx.impl.ManagementData;
	using Neo4jMBean = Org.Neo4j.Jmx.impl.Neo4jMBean;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;
	using CausalClustering = Org.Neo4j.management.CausalClustering;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.log.RaftLog_Fields.RAFT_LOG_DIRECTORY_NAME;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(ManagementBeanProvider.class) public class CausalClusteringBean extends org.neo4j.jmx.impl.ManagementBeanProvider
	public class CausalClusteringBean : ManagementBeanProvider
	{
		 private static readonly EnumSet<OperationalMode> _clusteringModes = EnumSet.of( OperationalMode.core, OperationalMode.read_replica );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public CausalClusteringBean()
		 public CausalClusteringBean() : base(typeof(CausalClustering))
		 {
		 }

		 protected internal override Neo4jMBean CreateMBean( ManagementData management )
		 {
			  if ( IsCausalClustering( management ) )
			  {
					return new CausalClusteringBeanImpl( management, false );
			  }
			  return null;
		 }

		 protected internal override Neo4jMBean CreateMXBean( ManagementData management )
		 {
			  if ( IsCausalClustering( management ) )
			  {
					return new CausalClusteringBeanImpl( management, true );
			  }
			  return null;
		 }

		 private static bool IsCausalClustering( ManagementData management )
		 {
			  DatabaseInfo databaseInfo = management.ResolveDependency( typeof( DatabaseInfo ) );
			  return _clusteringModes.contains( databaseInfo.OperationalMode );
		 }

		 private class CausalClusteringBeanImpl : Neo4jMBean, CausalClustering
		 {
			  internal readonly ClusterStateDirectory ClusterStateDirectory;
			  internal readonly RaftMachine RaftMachine;
			  internal readonly FileSystemAbstraction Fs;

			  internal CausalClusteringBeanImpl( ManagementData management, bool isMXBean ) : base( management, isMXBean )
			  {
					ClusterStateDirectory = management.ResolveDependency( typeof( ClusterStateDirectory ) );
					RaftMachine = management.ResolveDependency( typeof( RaftMachine ) );

					Fs = management.KernelData.FilesystemAbstraction;
			  }

			  public virtual string Role
			  {
				  get
				  {
						return RaftMachine.currentRole().ToString();
				  }
			  }

			  public virtual long RaftLogSize
			  {
				  get
				  {
						File raftLogDirectory = new File( ClusterStateDirectory.get(), RAFT_LOG_DIRECTORY_NAME );
						return FileUtils.size( Fs, raftLogDirectory );
				  }
			  }

			  public virtual long ReplicatedStateSize
			  {
				  get
				  {
						File replicatedStateDirectory = ClusterStateDirectory.get();
   
						File[] files = Fs.listFiles( replicatedStateDirectory );
						if ( files == null )
						{
							 return 0L;
						}
   
						long size = 0L;
						foreach ( File file in files )
						{
							 // Exclude raft log that resides in same directory
							 if ( Fs.isDirectory( file ) && file.Name.Equals( RAFT_LOG_DIRECTORY_NAME ) )
							 {
								  continue;
							 }
   
							 size += FileUtils.size( Fs, file );
						}
   
						return size;
				  }
			  }
		 }
	}

}