using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.causalclustering.discovery
{

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using CoreGraphDatabase = Neo4Net.causalclustering.core.CoreGraphDatabase;
	using LeaderCanWrite = Neo4Net.causalclustering.core.LeaderCanWrite;
	using NoLeaderFoundException = Neo4Net.causalclustering.core.consensus.NoLeaderFoundException;
	using Role = Neo4Net.causalclustering.core.consensus.roles.Role;
	using IdGenerationException = Neo4Net.causalclustering.core.state.machines.id.IdGenerationException;
	using LeaderOnlyLockManager = Neo4Net.causalclustering.core.state.machines.locks.LeaderOnlyLockManager;
	using ErrorHandler = Neo4Net.causalclustering.helper.ErrorHandler;
	using ReadReplicaGraphDatabase = Neo4Net.causalclustering.readreplica.ReadReplicaGraphDatabase;
	using Neo4Net.Function;
	using DatabaseShutdownException = Neo4Net.Graphdb.DatabaseShutdownException;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using TransactionFailureException = Neo4Net.Graphdb.TransactionFailureException;
	using TransientTransactionFailureException = Neo4Net.Graphdb.TransientTransactionFailureException;
	using WriteOperationsNotAllowedException = Neo4Net.Graphdb.security.WriteOperationsNotAllowedException;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using DatabaseHealth = Neo4Net.Kernel.@internal.DatabaseHealth;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using AcquireLockTimeoutException = Neo4Net.Storageengine.Api.@lock.AcquireLockTimeoutException;
	using DbRepresentation = Neo4Net.Test.DbRepresentation;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Predicates.await;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Predicates.awaitEx;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Predicates.notNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.firstOrNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_Transaction.LockSessionExpired;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.concurrent.Futures.combine;

	public abstract class Cluster<T> where T : DiscoveryServiceFactory
	{
		 private const int DEFAULT_TIMEOUT_MS = 120_000;
		 private const int DEFAULT_CLUSTER_SIZE = 3;

		 protected internal readonly File ParentDir;
		 private readonly IDictionary<string, string> _coreParams;
		 private readonly IDictionary<string, System.Func<int, string>> _instanceCoreParams;
		 private readonly IDictionary<string, string> _readReplicaParams;
		 private readonly IDictionary<string, System.Func<int, string>> _instanceReadReplicaParams;
		 private readonly string _recordFormat;
		 protected internal readonly T DiscoveryServiceFactory;
		 protected internal readonly string ListenAddress;
		 protected internal readonly string AdvertisedAddress;
		 private readonly ISet<string> _dbNames;

		 private IDictionary<int, CoreClusterMember> _coreMembers = new ConcurrentDictionary<int, CoreClusterMember>();
		 private IDictionary<int, ReadReplica> _readReplicas = new ConcurrentDictionary<int, ReadReplica>();
		 private int _highestCoreServerId;
		 private int _highestReplicaServerId;

		 public Cluster( File parentDir, int noOfCoreMembers, int noOfReadReplicas, T discoveryServiceFactory, IDictionary<string, string> coreParams, IDictionary<string, System.Func<int, string>> instanceCoreParams, IDictionary<string, string> readReplicaParams, IDictionary<string, System.Func<int, string>> instanceReadReplicaParams, string recordFormat, IpFamily ipFamily, bool useWildcard ) : this( parentDir, noOfCoreMembers, noOfReadReplicas, discoveryServiceFactory, coreParams, instanceCoreParams, readReplicaParams, instanceReadReplicaParams, recordFormat, ipFamily, useWildcard, Collections.singleton( CausalClusteringSettings.database.DefaultValue ) )
		 {
		 }

		 public Cluster( File parentDir, int noOfCoreMembers, int noOfReadReplicas, T discoveryServiceFactory, IDictionary<string, string> coreParams, IDictionary<string, System.Func<int, string>> instanceCoreParams, IDictionary<string, string> readReplicaParams, IDictionary<string, System.Func<int, string>> instanceReadReplicaParams, string recordFormat, IpFamily ipFamily, bool useWildcard, ISet<string> dbNames )
		 {
			  this.DiscoveryServiceFactory = discoveryServiceFactory;
			  this.ParentDir = parentDir;
			  this._coreParams = coreParams;
			  this._instanceCoreParams = instanceCoreParams;
			  this._readReplicaParams = readReplicaParams;
			  this._instanceReadReplicaParams = instanceReadReplicaParams;
			  this._recordFormat = recordFormat;
			  ListenAddress = useWildcard ? ipFamily.wildcardAddress() : ipFamily.localhostAddress();
			  AdvertisedAddress = ipFamily.localhostName();
			  IList<AdvertisedSocketAddress> initialHosts = initialHosts( noOfCoreMembers );
			  CreateCoreMembers( noOfCoreMembers, initialHosts, coreParams, instanceCoreParams, recordFormat );
			  CreateReadReplicas( noOfReadReplicas, initialHosts, readReplicaParams, instanceReadReplicaParams, recordFormat );
			  this._dbNames = dbNames;
		 }

		 private IList<AdvertisedSocketAddress> InitialHosts( int noOfCoreMembers )
		 {
			  return IntStream.range( 0, noOfCoreMembers ).mapToObj( ignored => PortAuthority.allocatePort() ).map(port => new AdvertisedSocketAddress(AdvertisedAddress, port)).collect(toList());
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws InterruptedException, java.util.concurrent.ExecutionException
		 public virtual void Start()
		 {
			  StartCoreMembers();
			  StartReadReplicas();
		 }

		 public virtual ISet<CoreClusterMember> HealthyCoreMembers()
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return _coreMembers.Values.Where( db => Db.database().DependencyResolver.resolveDependency(typeof(DatabaseHealth)).Healthy ).collect(Collectors.toSet());
		 }

		 public virtual CoreClusterMember GetCoreMemberById( int memberId )
		 {
			  return _coreMembers[memberId];
		 }

		 public virtual ReadReplica GetReadReplicaById( int memberId )
		 {
			  return _readReplicas[memberId];
		 }

		 public virtual CoreClusterMember AddCoreMemberWithId( int memberId )
		 {
			  return AddCoreMemberWithId( memberId, _coreParams, _instanceCoreParams, _recordFormat );
		 }

		 public virtual CoreClusterMember NewCoreMember()
		 {
			  int newCoreServerId = ++_highestCoreServerId;
			  return AddCoreMemberWithId( newCoreServerId );
		 }

		 public virtual ReadReplica NewReadReplica()
		 {
			  int newReplicaServerId = ++_highestReplicaServerId;
			  return AddReadReplicaWithId( newReplicaServerId );
		 }

		 private CoreClusterMember AddCoreMemberWithId( int memberId, IDictionary<string, string> extraParams, IDictionary<string, System.Func<int, string>> instanceExtraParams, string recordFormat )
		 {
			  IList<AdvertisedSocketAddress> initialHosts = ExtractInitialHosts( _coreMembers );
			  CoreClusterMember coreClusterMember = CreateCoreClusterMember( memberId, PortAuthority.allocatePort(), DEFAULT_CLUSTER_SIZE, initialHosts, recordFormat, extraParams, instanceExtraParams );

			  _coreMembers[memberId] = coreClusterMember;
			  return coreClusterMember;
		 }

		 public virtual ReadReplica AddReadReplicaWithIdAndRecordFormat( int memberId, string recordFormat )
		 {
			  return AddReadReplica( memberId, recordFormat, new Monitors() );
		 }

		 public virtual ReadReplica AddReadReplicaWithId( int memberId )
		 {
			  return AddReadReplicaWithIdAndRecordFormat( memberId, _recordFormat );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public ReadReplica addReadReplicaWithIdAndMonitors(@SuppressWarnings("SameParameterValue") int memberId, org.neo4j.kernel.monitoring.Monitors monitors)
		 public virtual ReadReplica AddReadReplicaWithIdAndMonitors( int memberId, Monitors monitors )
		 {
			  return AddReadReplica( memberId, _recordFormat, monitors );
		 }

		 private ReadReplica AddReadReplica( int memberId, string recordFormat, Monitors monitors )
		 {
			  IList<AdvertisedSocketAddress> initialHosts = ExtractInitialHosts( _coreMembers );
			  ReadReplica member = CreateReadReplica( memberId, initialHosts, _readReplicaParams, _instanceReadReplicaParams, recordFormat, monitors );

			  _readReplicas[memberId] = member;
			  return member;
		 }

		 public virtual void Shutdown()
		 {
			  using ( ErrorHandler errorHandler = new ErrorHandler( "Error when trying to shutdown cluster" ) )
			  {
					ShutdownCoreMembers( CoreMembers(), errorHandler );
					ShutdownReadReplicas( errorHandler );
			  }
		 }

		 private static void ShutdownCoreMembers( ICollection<CoreClusterMember> members, ErrorHandler errorHandler )
		 {
			  ShutdownMembers( members, errorHandler );
		 }

		 public virtual void ShutdownCoreMembers()
		 {
			  ShutdownCoreMembers( CoreMembers() );
		 }

		 public static void ShutdownCoreMember( CoreClusterMember member )
		 {
			  ShutdownCoreMembers( Collections.singleton( member ) );
		 }

		 public static void ShutdownCoreMembers( ICollection<CoreClusterMember> members )
		 {
			  using ( ErrorHandler errorHandler = new ErrorHandler( "Error when trying to shutdown core members" ) )
			  {
					ShutdownCoreMembers( members, errorHandler );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private static void shutdownMembers(java.util.Collection<? extends ClusterMember> clusterMembers, org.neo4j.causalclustering.helper.ErrorHandler errorHandler)
		 private static void ShutdownMembers<T1>( ICollection<T1> clusterMembers, ErrorHandler errorHandler ) where T1 : ClusterMember
		 {
			  errorHandler.Execute(() => combine(InvokeAll("cluster-shutdown", clusterMembers, cm =>
			  {
				cm.shutdown();
				return null;
			  })).get());
		 }

		 private static IList<Future<R>> InvokeAll<X, T, R>( string threadName, ICollection<T> members, System.Func<T, R> call ) where X : Neo4Net.Kernel.@internal.GraphDatabaseAPI where T : ClusterMember<X>
		 {
			  IList<Future<R>> list = new List<Future<R>>( members.Count );
			  int threadNumber = 0;
			  foreach ( T member in members )
			  {
					FutureTask<R> task = new FutureTask<R>( () => call(member) );
					ThreadGroup threadGroup = member.threadGroup();
					Thread thread = new Thread( threadGroup, task, threadName + "-" + threadNumber );
					thread.Start();
					threadNumber++;
					list.Add( task );
			  }
			  return list;
		 }

		 public virtual void RemoveCoreMemberWithServerId( int serverId )
		 {
			  CoreClusterMember memberToRemove = GetCoreMemberById( serverId );

			  if ( memberToRemove != null )
			  {
					memberToRemove.Shutdown();
					RemoveCoreMember( memberToRemove );
			  }
			  else
			  {
					throw new Exception( "Could not remove core member with id " + serverId );
			  }
		 }

		 public virtual void RemoveCoreMember( CoreClusterMember memberToRemove )
		 {
			  memberToRemove.Shutdown();
			  _coreMembers.Values.remove( memberToRemove );
		 }

		 public virtual void RemoveReadReplicaWithMemberId( int memberId )
		 {
			  ReadReplica memberToRemove = GetReadReplicaById( memberId );

			  if ( memberToRemove != null )
			  {
					RemoveReadReplica( memberToRemove );
			  }
			  else
			  {
					throw new Exception( "Could not remove core member with member id " + memberId );
			  }
		 }

		 private void RemoveReadReplica( ReadReplica memberToRemove )
		 {
			  memberToRemove.Shutdown();
			  _readReplicas.Values.remove( memberToRemove );
		 }

		 public virtual ICollection<CoreClusterMember> CoreMembers()
		 {
			  return _coreMembers.Values;
		 }

		 public virtual ICollection<ReadReplica> ReadReplicas()
		 {
			  return _readReplicas.Values;
		 }

		 public virtual ReadReplica FindAnyReadReplica()
		 {
			  return firstOrNull( _readReplicas.Values );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensureDBName(String dbName) throws IllegalArgumentException
		 private void EnsureDBName( string dbName )
		 {
			  if ( !_dbNames.Contains( dbName ) )
			  {
					throw new System.ArgumentException( "Database name " + dbName + " does not exist in this cluster." );
			  }
		 }

		 public virtual CoreClusterMember GetMemberWithRole( Role role )
		 {
			  return GetMemberWithAnyRole( role );
		 }

		 public virtual IList<CoreClusterMember> GetAllMembersWithRole( Role role )
		 {
			  return GetAllMembersWithAnyRole( role );
		 }

		 public virtual CoreClusterMember GetMemberWithRole( string dbName, Role role )
		 {
			  return GetMemberWithAnyRole( dbName, role );
		 }

		 public virtual IList<CoreClusterMember> GetAllMembersWithRole( string dbName, Role role )
		 {
			  return GetAllMembersWithAnyRole( dbName, role );
		 }

		 public virtual CoreClusterMember GetMemberWithAnyRole( params Role[] roles )
		 {
			  string dbName = CausalClusteringSettings.database.DefaultValue;
			  return GetMemberWithAnyRole( dbName, roles );
		 }

		 public virtual IList<CoreClusterMember> GetAllMembersWithAnyRole( params Role[] roles )
		 {
			  string dbName = CausalClusteringSettings.database.DefaultValue;
			  return GetAllMembersWithAnyRole( dbName, roles );
		 }

		 public virtual CoreClusterMember GetMemberWithAnyRole( string dbName, params Role[] roles )
		 {
			  return GetAllMembersWithAnyRole( dbName, roles ).First().orElse(null);
		 }

		 public virtual IList<CoreClusterMember> GetAllMembersWithAnyRole( string dbName, params Role[] roles )
		 {
			  EnsureDBName( dbName );
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  ISet<Role> roleSet = java.util.roles.collect( toSet() );

			  IList<CoreClusterMember> list = new List<CoreClusterMember>();
			  foreach ( CoreClusterMember m in _coreMembers.Values )
			  {
					CoreGraphDatabase database = m.Database();
					if ( database == null )
					{
						 continue;
					}

					if ( m.DbName().Equals(dbName) )
					{
						 if ( roleSet.Contains( database.Role ) )
						 {
							  list.Add( m );
						 }
					}
			  }
			  return list;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public CoreClusterMember awaitLeader() throws java.util.concurrent.TimeoutException
		 public virtual CoreClusterMember AwaitLeader()
		 {
			  return AwaitCoreMemberWithRole( Role.LEADER, DEFAULT_TIMEOUT_MS, TimeUnit.MILLISECONDS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public CoreClusterMember awaitLeader(String dbName) throws java.util.concurrent.TimeoutException
		 public virtual CoreClusterMember AwaitLeader( string dbName )
		 {
			  return AwaitCoreMemberWithRole( dbName, Role.LEADER, DEFAULT_TIMEOUT_MS, TimeUnit.MILLISECONDS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public CoreClusterMember awaitLeader(String dbName, long timeout, java.util.concurrent.TimeUnit timeUnit) throws java.util.concurrent.TimeoutException
		 public virtual CoreClusterMember AwaitLeader( string dbName, long timeout, TimeUnit timeUnit )
		 {
			  return AwaitCoreMemberWithRole( dbName, Role.LEADER, timeout, timeUnit );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public CoreClusterMember awaitLeader(long timeout, java.util.concurrent.TimeUnit timeUnit) throws java.util.concurrent.TimeoutException
		 public virtual CoreClusterMember AwaitLeader( long timeout, TimeUnit timeUnit )
		 {
			  return AwaitCoreMemberWithRole( Role.LEADER, timeout, timeUnit );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public CoreClusterMember awaitCoreMemberWithRole(org.neo4j.causalclustering.core.consensus.roles.Role role, long timeout, java.util.concurrent.TimeUnit timeUnit) throws java.util.concurrent.TimeoutException
		 public virtual CoreClusterMember AwaitCoreMemberWithRole( Role role, long timeout, TimeUnit timeUnit )
		 {
			  return await( () => GetMemberWithRole(role), notNull(), timeout, timeUnit );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public CoreClusterMember awaitCoreMemberWithRole(String dbName, org.neo4j.causalclustering.core.consensus.roles.Role role, long timeout, java.util.concurrent.TimeUnit timeUnit) throws java.util.concurrent.TimeoutException
		 public virtual CoreClusterMember AwaitCoreMemberWithRole( string dbName, Role role, long timeout, TimeUnit timeUnit )
		 {
			  return await( () => GetMemberWithRole(dbName, role), notNull(), timeout, timeUnit );
		 }

		 public virtual int NumberOfCoreMembersReportedByTopology()
		 {

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  CoreClusterMember aCoreGraphDb = _coreMembers.Values.Where( member => member.database() != null ).First().orElseThrow(System.ArgumentException::new);
			  CoreTopologyService coreTopologyService = aCoreGraphDb.Database().DependencyResolver.resolveDependency(typeof(CoreTopologyService));
			  return coreTopologyService.LocalCoreServers().members().Count;
		 }

		 /// <summary>
		 /// Perform a transaction against the core cluster, selecting the target and retrying as necessary.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public CoreClusterMember coreTx(System.Action<org.neo4j.causalclustering.core.CoreGraphDatabase,org.neo4j.graphdb.Transaction> op) throws Exception
		 public virtual CoreClusterMember CoreTx( System.Action<CoreGraphDatabase, Transaction> op )
		 {
			  string dbName = CausalClusteringSettings.database.DefaultValue;
			  return CoreTx( dbName, op );
		 }

		 /// <summary>
		 /// Perform a transaction against the core cluster, selecting the target and retrying as necessary.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public CoreClusterMember coreTx(String dbName, System.Action<org.neo4j.causalclustering.core.CoreGraphDatabase,org.neo4j.graphdb.Transaction> op) throws Exception
		 public virtual CoreClusterMember CoreTx( string dbName, System.Action<CoreGraphDatabase, Transaction> op )
		 {
			  EnsureDBName( dbName );
			  return LeaderTx( dbName, op, DEFAULT_TIMEOUT_MS, TimeUnit.MILLISECONDS );
		 }

		 /// <summary>
		 /// Perform a transaction against the leader of the core cluster, retrying as necessary.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private CoreClusterMember leaderTx(String dbName, System.Action<org.neo4j.causalclustering.core.CoreGraphDatabase,org.neo4j.graphdb.Transaction> op, int timeout, java.util.concurrent.TimeUnit timeUnit) throws Exception
		 private CoreClusterMember LeaderTx( string dbName, System.Action<CoreGraphDatabase, Transaction> op, int timeout, TimeUnit timeUnit )
		 {
			  ThrowingSupplier<CoreClusterMember, Exception> supplier = () =>
			  {
				CoreClusterMember member = AwaitLeader( dbName, timeout, timeUnit );
				CoreGraphDatabase db = member.Database();
				if ( db == null )
				{
					 throw new DatabaseShutdownException();
				}

				try
				{
					using ( Transaction tx = Db.beginTx() )
					{
						 op( db, tx );
						 return member;
					}
				}
				catch ( Exception e )
				{
					 if ( IsTransientFailure( e ) )
					 {
						  return null;
					 }
					 else
					 {
						  throw e;
					 }
				}
			  };
			  return awaitEx( supplier, notNull().test, timeout, timeUnit );
		 }

		 private static bool IsTransientFailure( Exception e )
		 {
			  System.Predicate<Exception> throwablePredicate = e1 => IsLockExpired( e1 ) || IsLockOnFollower( e1 ) || IsWriteNotOnLeader( e1 ) || e1 is TransientTransactionFailureException || e1 is IdGenerationException;
			  return Exceptions.contains( e, throwablePredicate );
		 }

		 private static bool IsWriteNotOnLeader( Exception e )
		 {
			  return e is WriteOperationsNotAllowedException && e.Message.StartsWith( string.format( LeaderCanWrite.NOT_LEADER_ERROR_MSG, "" ) );
		 }

		 private static bool IsLockOnFollower( Exception e )
		 {
			  return e is AcquireLockTimeoutException && ( e.Message.Equals( LeaderOnlyLockManager.LOCK_NOT_ON_LEADER_ERROR_MESSAGE ) || e.InnerException is NoLeaderFoundException );
		 }

		 private static bool IsLockExpired( Exception e )
		 {
			  return e is TransactionFailureException && e.InnerException is Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException && ( ( Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException ) e.InnerException ).Status() == LockSessionExpired;
		 }

		 private IList<AdvertisedSocketAddress> ExtractInitialHosts( IDictionary<int, CoreClusterMember> coreMembers )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return coreMembers.Values.Select( CoreClusterMember::discoveryPort ).Select( port => new AdvertisedSocketAddress( AdvertisedAddress, port ) ).ToList();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void createCoreMembers(final int noOfCoreMembers, java.util.List<org.neo4j.helpers.AdvertisedSocketAddress> initialHosts, java.util.Map<String,String> extraParams, java.util.Map<String,System.Func<int, String>> instanceExtraParams, String recordFormat)
		 private void CreateCoreMembers( int noOfCoreMembers, IList<AdvertisedSocketAddress> initialHosts, IDictionary<string, string> extraParams, IDictionary<string, System.Func<int, string>> instanceExtraParams, string recordFormat )
		 {
			  for ( int i = 0; i < initialHosts.Count; i++ )
			  {
					int discoveryListenAddress = initialHosts[i].Port;
					CoreClusterMember coreClusterMember = CreateCoreClusterMember( i, discoveryListenAddress, noOfCoreMembers, initialHosts, recordFormat, extraParams, instanceExtraParams );
					_coreMembers[i] = coreClusterMember;
			  }
			  _highestCoreServerId = noOfCoreMembers - 1;
		 }

		 protected internal abstract CoreClusterMember CreateCoreClusterMember( int serverId, int discoveryPort, int clusterSize, IList<AdvertisedSocketAddress> initialHosts, string recordFormat, IDictionary<string, string> extraParams, IDictionary<string, System.Func<int, string>> instanceExtraParams );

		 protected internal abstract ReadReplica CreateReadReplica( int serverId, IList<AdvertisedSocketAddress> initialHosts, IDictionary<string, string> extraParams, IDictionary<string, System.Func<int, string>> instanceExtraParams, string recordFormat, Monitors monitors );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void startCoreMembers() throws InterruptedException, java.util.concurrent.ExecutionException
		 public virtual void StartCoreMembers()
		 {
			  StartCoreMembers( _coreMembers.Values );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void startCoreMember(CoreClusterMember member) throws InterruptedException, java.util.concurrent.ExecutionException
		 public static void StartCoreMember( CoreClusterMember member )
		 {
			  StartCoreMembers( Collections.singleton( member ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void startCoreMembers(java.util.Collection<CoreClusterMember> members) throws InterruptedException, java.util.concurrent.ExecutionException
		 public static void StartCoreMembers( ICollection<CoreClusterMember> members )
		 {
			  IList<Future<CoreGraphDatabase>> futures = InvokeAll("cluster-starter", members, cm =>
			  {
				cm.start();
				return cm.database();
			  });
			  foreach ( Future<CoreGraphDatabase> future in futures )
			  {
					future.get();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void startReadReplicas() throws InterruptedException, java.util.concurrent.ExecutionException
		 private void StartReadReplicas()
		 {
			  IDictionary<int, ReadReplica>.ValueCollection members = _readReplicas.Values;
			  IList<Future<ReadReplicaGraphDatabase>> futures = InvokeAll("cluster-starter", members, cm =>
			  {
				cm.start();
				return cm.database();
			  });
			  foreach ( Future<ReadReplicaGraphDatabase> future in futures )
			  {
					future.get();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void createReadReplicas(int noOfReadReplicas, final java.util.List<org.neo4j.helpers.AdvertisedSocketAddress> initialHosts, java.util.Map<String,String> extraParams, java.util.Map<String,System.Func<int, String>> instanceExtraParams, String recordFormat)
		 private void CreateReadReplicas( int noOfReadReplicas, IList<AdvertisedSocketAddress> initialHosts, IDictionary<string, string> extraParams, IDictionary<string, System.Func<int, string>> instanceExtraParams, string recordFormat )
		 {
			  for ( int i = 0; i < noOfReadReplicas; i++ )
			  {
					ReadReplica readReplica = createReadReplica(i, initialHosts, extraParams, instanceExtraParams, recordFormat, new Monitors()
				  );

					_readReplicas[i] = readReplica;
			  }
			  _highestReplicaServerId = noOfReadReplicas - 1;
		 }

		 private void ShutdownReadReplicas( ErrorHandler errorHandler )
		 {
			  ShutdownMembers( ReadReplicas(), errorHandler );
		 }

		 /// <summary>
		 /// Waits for <seealso cref="DEFAULT_TIMEOUT_MS"/> for the <code>memberThatChanges</code> to match the contents of
		 /// <code>memberToLookLike</code>. After calling this method, changes both in <code>memberThatChanges</code> and
		 /// <code>memberToLookLike</code> are picked up.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void dataOnMemberEventuallyLooksLike(CoreClusterMember memberThatChanges, CoreClusterMember memberToLookLike) throws java.util.concurrent.TimeoutException
		 public static void DataOnMemberEventuallyLooksLike( CoreClusterMember memberThatChanges, CoreClusterMember memberToLookLike )
		 {
			  await(() =>
			  {
						  try
						  {
								// We recalculate the DbRepresentation of both source and target, so changes can be picked up
								DbRepresentation representationToLookLike = DbRepresentation.of( memberToLookLike.Database() );
								DbRepresentation representationThatChanges = DbRepresentation.of( memberThatChanges.Database() );
								return representationToLookLike.Equals( representationThatChanges );
						  }
						  catch ( DatabaseShutdownException )
						  {
						  /*
						   * This can happen if the database is still in the process of starting. Yes, the naming
						   * of the exception is unfortunate, since it is thrown when the database lifecycle is not
						   * in RUNNING state and therefore signals general unavailability (e.g still starting) and not
						   * necessarily a database that is shutting down.
						   */
						  }
						  return false;
			  }, DEFAULT_TIMEOUT_MS, TimeUnit.MILLISECONDS);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <T extends ClusterMember> void dataMatchesEventually(ClusterMember source, java.util.Collection<T> targets) throws java.util.concurrent.TimeoutException
		 public static void DataMatchesEventually<T>( ClusterMember source, ICollection<T> targets ) where T : ClusterMember
		 {
			  DataMatchesEventually( DbRepresentation.of( source.database() ), targets );
		 }

		 /// <summary>
		 /// Waits for <seealso cref="DEFAULT_TIMEOUT_MS"/> for the <code>targetDBs</code> to have the same content as the
		 /// <code>member</code>. Changes in the <code>member</code> database contents after this method is called do not get
		 /// picked up and are not part of the comparison.
		 /// </summary>
		 /// <param name="source">  The database to check against </param>
		 /// <param name="targets"> The databases expected to match the contents of <code>member</code> </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <T extends ClusterMember> void dataMatchesEventually(org.neo4j.test.DbRepresentation source, java.util.Collection<T> targets) throws java.util.concurrent.TimeoutException
		 public static void DataMatchesEventually<T>( DbRepresentation source, ICollection<T> targets ) where T : ClusterMember
		 {
			  foreach ( ClusterMember targetDB in targets )
			  {
					await(() =>
					{
					 DbRepresentation representation = DbRepresentation.of( targetDB.database() );
					 return source.Equals( representation );
					}, DEFAULT_TIMEOUT_MS, TimeUnit.MILLISECONDS);
			  }
		 }

		 public virtual ClusterMember GetMemberByBoltAddress( AdvertisedSocketAddress advertisedSocketAddress )
		 {
			  foreach ( CoreClusterMember member in _coreMembers.Values )
			  {
					if ( member.BoltAdvertisedAddress().Equals(advertisedSocketAddress.ToString()) )
					{
						 return member;
					}
			  }

			  foreach ( ReadReplica member in _readReplicas.Values )
			  {
					if ( member.BoltAdvertisedAddress().Equals(advertisedSocketAddress.ToString()) )
					{
						 return member;
					}
			  }

			  throw new Exception( "Could not find a member for bolt address " + advertisedSocketAddress );
		 }

		 public virtual Optional<ClusterMember> RandomMember( bool mustBeStarted )
		 {
			  Stream<ClusterMember> members = Stream.concat( CoreMembers().stream(), ReadReplicas().stream() );

			  if ( mustBeStarted )
			  {
					members = members.filter( m => !m.Shutdown );
			  }

			  IList<ClusterMember> eligible = members.collect( Collectors.toList() );
			  return Random( eligible );
		 }

		 public virtual Optional<CoreClusterMember> RandomCoreMember( bool mustBeStarted )
		 {
			  Stream<CoreClusterMember> members = CoreMembers().stream();

			  if ( mustBeStarted )
			  {
					members = members.filter( m => !m.Shutdown );
			  }

			  IList<CoreClusterMember> eligible = members.collect( Collectors.toList() );
			  return Random( eligible );
		 }

		 private static Optional<T> Random<T>( IList<T> list )
		 {
			  if ( list.Count == 0 )
			  {
					return null;
			  }
			  int ordinal = ThreadLocalRandom.current().Next(list.Count);
			  return list[ordinal];
		 }
	}

}