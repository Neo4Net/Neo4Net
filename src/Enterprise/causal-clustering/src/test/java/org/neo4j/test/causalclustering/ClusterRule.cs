using System.Collections;
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
namespace Neo4Net.Test.causalclustering
{
	using ExternalResource = org.junit.rules.ExternalResource;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;


	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using Neo4Net.causalclustering.discovery;
	using EnterpriseCluster = Neo4Net.causalclustering.discovery.EnterpriseCluster;
	using IpFamily = Neo4Net.causalclustering.discovery.IpFamily;
	using CausalClusteringTestHelpers = Neo4Net.causalclustering.helpers.CausalClusteringTestHelpers;
	using DiscoveryServiceType = Neo4Net.causalclustering.scenarios.DiscoveryServiceType;
	using EnterpriseDiscoveryServiceType = Neo4Net.causalclustering.scenarios.EnterpriseDiscoveryServiceType;
	using Neo4Net.Graphdb.config;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using VerboseTimeout = Neo4Net.Test.rule.VerboseTimeout;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.IpFamily.IPV4;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	/// <summary>
	/// Includes a <seealso cref="VerboseTimeout"/> rule with a long default timeout. Use <seealso cref="withTimeout(long, TimeUnit)"/> to customise
	/// or <seealso cref="withNoTimeout()"/> to disable.
	/// </summary>
	public class ClusterRule : ExternalResource
	{
		 private readonly TestDirectory _testDirectory = TestDirectory.testDirectory();
		 private File _clusterDirectory;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;

		 private int _noCoreMembers = 3;
		 private int _noReadReplicas = 3;
		 private DiscoveryServiceType _discoveryServiceType = EnterpriseDiscoveryServiceType.SHARED;
		 private IDictionary<string, string> _coreParams = stringMap();
		 private IDictionary<string, System.Func<int, string>> _instanceCoreParams = new Dictionary<string, System.Func<int, string>>();
		 private IDictionary<string, string> _readReplicaParams = stringMap();
		 private IDictionary<string, System.Func<int, string>> _instanceReadReplicaParams = new Dictionary<string, System.Func<int, string>>();
		 private string _recordFormat = Standard.LATEST_NAME;
		 private IpFamily _ipFamily = IPV4;
		 private bool _useWildcard;
		 private VerboseTimeout.VerboseTimeoutBuilder _timeoutBuilder = new VerboseTimeout.VerboseTimeoutBuilder().withTimeout(15, TimeUnit.MINUTES);
		 private ISet<string> _dbNames = Collections.singleton( CausalClusteringSettings.database.DefaultValue );

		 public ClusterRule()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.junit.runners.model.Statement apply(final org.junit.runners.model.Statement super, final org.junit.runner.Description description)
		 public override Statement Apply( Statement @base, Description description )
		 {
			  Statement timeoutStatement;
			  if ( _timeoutBuilder != null )
			  {
					timeoutStatement = _timeoutBuilder.build().apply(@base, description);
			  }
			  else
			  {
					timeoutStatement = @base;
			  }

			  Statement testMethod = new StatementAnonymousInnerClass( this, description, timeoutStatement );

			  Statement testMethodWithBeforeAndAfter = base.Apply( testMethod, description );

			  return _testDirectory.apply( testMethodWithBeforeAndAfter, description );
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly ClusterRule _outerInstance;

			 private Description _description;
			 private Statement _timeoutStatement;

			 public StatementAnonymousInnerClass( ClusterRule outerInstance, Description description, Statement timeoutStatement )
			 {
				 this.outerInstance = outerInstance;
				 this._description = description;
				 this._timeoutStatement = timeoutStatement;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
			 public override void evaluate()
			 {
				  // If this is used as class rule then getMethodName() returns null, so use
				  // getClassName() instead.
				  string name = _description.MethodName != null ? _description.MethodName : _description.ClassName;
				  _outerInstance.clusterDirectory = _outerInstance.testDirectory.directory( name );
				  _timeoutStatement.evaluate();
			 }
		 }

		 protected internal override void After()
		 {
			  if ( _cluster != null )
			  {
					_cluster.shutdown();
			  }
		 }

		 /// <summary>
		 /// Starts cluster with the configuration provided at instantiation time. This method will not return until the
		 /// cluster is up and all members report each other as available.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.discovery.Cluster<?> startCluster() throws Exception
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public virtual Cluster<object> StartCluster()
		 {
			  CreateCluster();
			  _cluster.start();
			  foreach ( string dbName in _dbNames )
			  {
					_cluster.awaitLeader( dbName );
			  }
			  return _cluster;
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public org.neo4j.causalclustering.discovery.Cluster<?> createCluster()
		 public virtual Cluster<object> CreateCluster()
		 {
			  if ( _cluster == null )
			  {
					_cluster = new EnterpriseCluster( _clusterDirectory, _noCoreMembers, _noReadReplicas, _discoveryServiceType.createFactory(), _coreParams, _instanceCoreParams, _readReplicaParams, _instanceReadReplicaParams, _recordFormat, _ipFamily, _useWildcard, _dbNames );
			  }

			  return _cluster;
		 }

		 public virtual TestDirectory TestDirectory()
		 {
			  return _testDirectory;
		 }

		 public virtual File ClusterDirectory()
		 {
			  return _clusterDirectory;
		 }

		 public virtual ClusterRule WithDatabaseNames( ISet<string> dbNames )
		 {
			  this._dbNames = dbNames;
			  IDictionary<int, string> coreDBMap = CausalClusteringTestHelpers.distributeDatabaseNamesToHostNums( _noCoreMembers, dbNames );
			  IDictionary<int, string> rrDBMap = CausalClusteringTestHelpers.distributeDatabaseNamesToHostNums( _noReadReplicas, dbNames );

//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  IDictionary<string, long> minCoresPerDb = coreDBMap.SetOfKeyValuePairs().collect(Collectors.groupingBy(DictionaryEntry.getValue, Collectors.counting()));

			  IDictionary<int, string> minCoresSettingsMap = new Dictionary<int, string>();

			  foreach ( KeyValuePair<int, string> entry in coreDBMap.SetOfKeyValuePairs() )
			  {
					long? minNumCores = Optional.ofNullable( minCoresPerDb[entry.Value] );
					minNumCores.ifPresent( n => minCoresSettingsMap.put( entry.Key, n.ToString() ) );
			  }

			  WithInstanceCoreParam( CausalClusteringSettings.database, coreDBMap.get );
			  WithInstanceCoreParam( CausalClusteringSettings.minimum_core_cluster_size_at_formation, minCoresSettingsMap.get );
			  WithInstanceReadReplicaParam( CausalClusteringSettings.database, rrDBMap.get );
			  return this;
		 }

		 public virtual ClusterRule WithNumberOfCoreMembers( int noCoreMembers )
		 {
			  this._noCoreMembers = noCoreMembers;
			  return this;
		 }

		 public virtual ClusterRule WithNumberOfReadReplicas( int noReadReplicas )
		 {
			  this._noReadReplicas = noReadReplicas;
			  return this;
		 }

		 public virtual ClusterRule WithDiscoveryServiceType( DiscoveryServiceType discoveryServiceType )
		 {
			  this._discoveryServiceType = discoveryServiceType;
			  return this;
		 }

		 public virtual ClusterRule WithSharedCoreParams( IDictionary<string, string> @params )
		 {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  this._coreParams.putAll( @params );
			  return this;
		 }

		 public virtual ClusterRule WithSharedCoreParam<T1>( Setting<T1> key, string value )
		 {
			  this._coreParams[key.Name()] = value;
			  return this;
		 }

		 public virtual ClusterRule WithInstanceCoreParams( IDictionary<string, System.Func<int, string>> @params )
		 {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  this._instanceCoreParams.putAll( @params );
			  return this;
		 }

		 public virtual ClusterRule WithInstanceCoreParam<T1>( Setting<T1> key, System.Func<int, string> valueFunction )
		 {
			  this._instanceCoreParams[key.Name()] = valueFunction;
			  return this;
		 }

		 public virtual ClusterRule WithSharedReadReplicaParams( IDictionary<string, string> @params )
		 {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  this._readReplicaParams.putAll( @params );
			  return this;
		 }

		 public virtual ClusterRule WithSharedReadReplicaParam<T1>( Setting<T1> key, string value )
		 {
			  this._readReplicaParams[key.Name()] = value;
			  return this;
		 }

		 public virtual ClusterRule WithInstanceReadReplicaParams( IDictionary<string, System.Func<int, string>> @params )
		 {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  this._instanceReadReplicaParams.putAll( @params );
			  return this;
		 }

		 public virtual ClusterRule WithInstanceReadReplicaParam<T1>( Setting<T1> key, System.Func<int, string> valueFunction )
		 {
			  this._instanceReadReplicaParams[key.Name()] = valueFunction;
			  return this;
		 }

		 public virtual ClusterRule WithRecordFormat( string recordFormat )
		 {
			  this._recordFormat = recordFormat;
			  return this;
		 }

		 public virtual ClusterRule WithClusterDirectory( File clusterDirectory )
		 {
			  this._clusterDirectory = clusterDirectory;
			  return this;
		 }

		 public virtual ClusterRule WithIpFamily( IpFamily ipFamily )
		 {
			  this._ipFamily = ipFamily;
			  return this;
		 }

		 public virtual ClusterRule UseWildcard( bool useWildcard )
		 {
			  this._useWildcard = useWildcard;
			  return this;
		 }

		 public virtual ClusterRule WithTimeout( long timeout, TimeUnit unit )
		 {
			  this._timeoutBuilder = ( new VerboseTimeout.VerboseTimeoutBuilder() ).withTimeout(timeout, unit);
			  return this;
		 }

		 public virtual ClusterRule WithNoTimeout()
		 {
			  this._timeoutBuilder = null;
			  return this;
		 }
	}

}