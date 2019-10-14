using System.Collections.Generic;

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
namespace Neo4Net.Kernel.ha.management
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ClusterMember = Neo4Net.Kernel.ha.cluster.member.ClusterMember;
	using ClusterMembers = Neo4Net.Kernel.ha.cluster.member.ClusterMembers;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using KernelData = Neo4Net.Kernel.@internal.KernelData;
	using Version = Neo4Net.Kernel.@internal.Version;
	using ClusterDatabaseInfo = Neo4Net.management.ClusterDatabaseInfo;
	using ClusterMemberInfo = Neo4Net.management.ClusterMemberInfo;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.map;

	public class HighlyAvailableKernelData : KernelData
	{
		 private readonly ClusterMembers _memberInfo;
		 private readonly ClusterDatabaseInfoProvider _memberInfoProvider;

		 public HighlyAvailableKernelData( DataSourceManager dataSourceManager, ClusterMembers memberInfo, ClusterDatabaseInfoProvider databaseInfo, FileSystemAbstraction fileSystem, PageCache pageCache, File storeDir, Config config ) : base( fileSystem, pageCache, storeDir, config, dataSourceManager )
		 {
			  this._memberInfo = memberInfo;
			  this._memberInfoProvider = databaseInfo;
		 }

		 public override Version Version()
		 {
			  return Version.Kernel;
		 }

		 internal virtual ClusterMemberInfo[] ClusterInfo
		 {
			 get
			 {
				  IList<ClusterMemberInfo> clusterMemberInfos = new List<ClusterMemberInfo>();
				  System.Func<object, string> nullSafeToString = from => from == null ? "" : from.ToString();
				  foreach ( ClusterMember clusterMember in _memberInfo.Members )
				  {
						ClusterMemberInfo clusterMemberInfo = new ClusterMemberInfo( clusterMember.InstanceId.ToString(), clusterMember.HAUri != null, clusterMember.Alive, clusterMember.HARole, asArray(typeof(string), map(nullSafeToString, clusterMember.RoleURIs)), asArray(typeof(string), map(nullSafeToString, clusterMember.Roles)) );
						clusterMemberInfos.Add( clusterMemberInfo );
				  }
   
				  return clusterMemberInfos.ToArray();
			 }
		 }

		 internal virtual ClusterDatabaseInfo MemberInfo
		 {
			 get
			 {
				  return _memberInfoProvider.Info;
			 }
		 }
	}

}