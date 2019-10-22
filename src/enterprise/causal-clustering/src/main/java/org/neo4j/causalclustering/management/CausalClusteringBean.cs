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
namespace Neo4Net.causalclustering.management
{


	using RaftMachine = Neo4Net.causalclustering.core.consensus.RaftMachine;
	using ClusterStateDirectory = Neo4Net.causalclustering.core.state.ClusterStateDirectory;
	using Service = Neo4Net.Helpers.Service;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using ManagementBeanProvider = Neo4Net.Jmx.impl.ManagementBeanProvider;
	using ManagementData = Neo4Net.Jmx.impl.ManagementData;
	using Neo4NetMBean = Neo4Net.Jmx.impl.Neo4NetMBean;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;
	using CausalClustering = Neo4Net.management.CausalClustering;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.log.RaftLog_Fields.RAFT_LOG_DIRECTORY_NAME;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(ManagementBeanProvider.class) public class CausalClusteringBean extends org.Neo4Net.jmx.impl.ManagementBeanProvider
	public class CausalClusteringBean : ManagementBeanProvider
	{
		 private static readonly EnumSet<OperationalMode> _clusteringModes = EnumSet.of( OperationalMode.core, OperationalMode.read_replica );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public CausalClusteringBean()
		 public CausalClusteringBean() : base(typeof(CausalClustering))
		 {
		 }

		 protected internal override Neo4NetMBean CreateMBean( ManagementData management )
		 {
			  if ( IsCausalClustering( management ) )
			  {
					return new CausalClusteringBeanImpl( management, false );
			  }
			  return null;
		 }

		 protected internal override Neo4NetMBean CreateMXBean( ManagementData management )
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

		 private class CausalClusteringBeanImpl : Neo4NetMBean, CausalClustering
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