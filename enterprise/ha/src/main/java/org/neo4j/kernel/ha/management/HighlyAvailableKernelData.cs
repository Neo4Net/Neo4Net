using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.ha.management
{

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ClusterMember = Org.Neo4j.Kernel.ha.cluster.member.ClusterMember;
	using ClusterMembers = Org.Neo4j.Kernel.ha.cluster.member.ClusterMembers;
	using DataSourceManager = Org.Neo4j.Kernel.impl.transaction.state.DataSourceManager;
	using KernelData = Org.Neo4j.Kernel.@internal.KernelData;
	using Version = Org.Neo4j.Kernel.@internal.Version;
	using ClusterDatabaseInfo = Org.Neo4j.management.ClusterDatabaseInfo;
	using ClusterMemberInfo = Org.Neo4j.management.ClusterMemberInfo;

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