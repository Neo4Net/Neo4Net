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
namespace Neo4Net.causalclustering.stresstests
{

	using Neo4Net.causalclustering.discovery;
	using EnterpriseCluster = Neo4Net.causalclustering.discovery.EnterpriseCluster;
	using HazelcastDiscoveryServiceFactory = Neo4Net.causalclustering.discovery.HazelcastDiscoveryServiceFactory;
	using IpFamily = Neo4Net.causalclustering.discovery.IpFamily;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helper.StressTestingHelper.ensureExistsAndEmpty;

	internal class Resources
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.neo4j.causalclustering.discovery.Cluster<?> cluster;
		 private readonly Cluster<object> _cluster;
		 private readonly File _clusterDir;
		 private readonly File _backupDir;
		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly PageCache _pageCache;
		 private readonly LogProvider _logProvider;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Resources(org.neo4j.io.fs.FileSystemAbstraction fileSystem, org.neo4j.io.pagecache.PageCache pageCache, Config config) throws java.io.IOException
		 internal Resources( FileSystemAbstraction fileSystem, PageCache pageCache, Config config ) : this( fileSystem, pageCache, FormattedLogProvider.toOutputStream( System.out ), config )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Resources(org.neo4j.io.fs.FileSystemAbstraction fileSystem, org.neo4j.io.pagecache.PageCache pageCache, org.neo4j.logging.LogProvider logProvider, Config config) throws java.io.IOException
		 private Resources( FileSystemAbstraction fileSystem, PageCache pageCache, LogProvider logProvider, Config config )
		 {
			  this._fileSystem = fileSystem;
			  this._pageCache = pageCache;
			  this._logProvider = logProvider;

			  int numberOfCores = config.NumberOfCores();
			  int numberOfEdges = config.NumberOfEdges();
			  string workingDirectory = config.WorkingDir();

			  this._clusterDir = ensureExistsAndEmpty( new File( workingDirectory, "cluster" ) );
			  this._backupDir = ensureExistsAndEmpty( new File( workingDirectory, "backups" ) );

			  IDictionary<string, string> coreParams = new Dictionary<string, string>();
			  IDictionary<string, string> readReplicaParams = new Dictionary<string, string>();

			  config.PopulateCoreParams( coreParams );
			  config.PopulateReadReplicaParams( readReplicaParams );

			  HazelcastDiscoveryServiceFactory discoveryServiceFactory = new HazelcastDiscoveryServiceFactory();
			  _cluster = new EnterpriseCluster( _clusterDir, numberOfCores, numberOfEdges, discoveryServiceFactory, coreParams, emptyMap(), readReplicaParams, emptyMap(), Standard.LATEST_NAME, IpFamily.IPV4, false );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public org.neo4j.causalclustering.discovery.Cluster<?> cluster()
		 public virtual Cluster<object> Cluster()
		 {
			  return _cluster;
		 }

		 public virtual FileSystemAbstraction FileSystem()
		 {
			  return _fileSystem;
		 }

		 public virtual LogProvider LogProvider()
		 {
			  return _logProvider;
		 }

		 public virtual File BackupDir()
		 {
			  return _backupDir;
		 }

		 public virtual PageCache PageCache()
		 {
			  return _pageCache;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Exception
		 public virtual void Start()
		 {
			  _cluster.start();
		 }

		 public virtual void Stop()
		 {
			  _cluster.shutdown();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void cleanup() throws java.io.IOException
		 public virtual void Cleanup()
		 {
			  FileUtils.deleteRecursively( _clusterDir );
			  FileUtils.deleteRecursively( _backupDir );
		 }

		 public virtual Clock Clock()
		 {
			  return Clock.systemUTC();
		 }
	}

}