using System;
using System.Collections.Generic;

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
namespace Neo4Net.Test.ha
{
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using ExternalResource = org.junit.rules.ExternalResource;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;


	using Cluster = Neo4Net.cluster.client.Cluster;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Neo4Net.GraphDb.config;
	using HighlyAvailableGraphDatabaseFactory = Neo4Net.GraphDb.factory.HighlyAvailableGraphDatabaseFactory;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using Builder = Neo4Net.Kernel.impl.ha.ClusterManager.Builder;
	using Neo4Net.Kernel.impl.ha.ClusterManager;
	using ManagedCluster = Neo4Net.Kernel.impl.ha.ClusterManager.ManagedCluster;
	using StoreDirInitializer = Neo4Net.Kernel.impl.ha.ClusterManager.StoreDirInitializer;
	using Neo4Net.Kernel.impl.util;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.cluster.ClusterSettings.default_timeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.cluster.ClusterSettings.join_timeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.pagecache_memory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.store_internal_log_level;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.ha.HaSettings.tx_push_factor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.allSeesAllAsAvailable;

	/// <summary>
	/// Starts, manages and in the end shuts down an HA cluster as a JUnit {@code Rule} or <seealso cref="ClassRule"/>.
	/// Basically this is <seealso cref="ClusterManager"/> in a JUnit <seealso cref="Rule"/> packaging.
	/// </summary>
	public class ClusterRule : ExternalResource, ClusterManager.ClusterBuilder<ClusterRule>
	{
		 private static readonly ClusterManager.StoreDirInitializer _defaultStoreDirInitializer = ( serverId, storeDir ) =>
		 {
					 File[] files = storeDir.listFiles();
					 if ( files != null )
					 {
						  foreach ( File file in files )
						  {
								FileUtils.deleteRecursively( file );
						  }
					 }
		 };

		 private ClusterManager.Builder _clusterManagerBuilder;
		 private ClusterManager _clusterManager;
		 private File _storeDirectory;

		 private readonly TestDirectory _testDirectory;
		 private ClusterManager.ManagedCluster _cluster;

		 public ClusterRule()
		 {
			  this._testDirectory = TestDirectory.testDirectory();
			  this._clusterManagerBuilder = ( new ClusterManager.Builder() ).withSharedSetting(store_internal_log_level, "DEBUG").withSharedSetting(default_timeout, "1s").withSharedSetting(tx_push_factor, "0").withSharedSetting(pagecache_memory, "8m").withSharedSetting(join_timeout, "60s").withAvailabilityChecks(allSeesAllAsAvailable()).withStoreDirInitializer(_defaultStoreDirInitializer);
		 }

		 public override ClusterRule WithRootDirectory( File root )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public ClusterRule withSeedDir(final java.io.File seedDir)
		 public override ClusterRule WithSeedDir( File seedDir )
		 {
			  return Set( _clusterManagerBuilder.withSeedDir( seedDir ) );
		 }

		 public override ClusterRule WithStoreDirInitializer( ClusterManager.StoreDirInitializer initializer )
		 {
			  return Set( _clusterManagerBuilder.withStoreDirInitializer( initializer ) );
		 }

		 public override ClusterRule WithDbFactory( HighlyAvailableGraphDatabaseFactory dbFactory )
		 {
			  return Set( _clusterManagerBuilder.withDbFactory( dbFactory ) );
		 }

		 public override ClusterRule WithCluster( System.Func<Cluster> supplier )
		 {
			  return Set( _clusterManagerBuilder.withCluster( supplier ) );
		 }

		 public override ClusterRule WithInstanceConfig( IDictionary<string, System.Func<int, string>> commonConfig )
		 {
			  return Set( _clusterManagerBuilder.withInstanceConfig( commonConfig ) );
		 }

		 public override ClusterRule WithBoltEnabled()
		 {
			  return Set( _clusterManagerBuilder.withBoltEnabled() );
		 }

		 public override ClusterRule WithInstanceSetting<T1>( Setting<T1> setting, System.Func<int, string> valueFunction )
		 {
			  return Set( _clusterManagerBuilder.withInstanceSetting( setting, valueFunction ) );
		 }

		 public override ClusterRule WithSharedConfig( IDictionary<string, string> commonConfig )
		 {
			  return Set( _clusterManagerBuilder.withSharedConfig( commonConfig ) );
		 }

		 public override ClusterRule WithSharedSetting<T1>( Setting<T1> setting, string value )
		 {
			  return Set( _clusterManagerBuilder.withSharedSetting( setting, value ) );
		 }

		 public override ClusterRule WithInitialDataset( Listener<GraphDatabaseService> transactor )
		 {
			  return Set( _clusterManagerBuilder.withInitialDataset( transactor ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs @Override public final ClusterRule withAvailabilityChecks(System.Predicate<org.Neo4Net.kernel.impl.ha.ClusterManager.ManagedCluster>... checks)
		 public override ClusterRule WithAvailabilityChecks( params System.Predicate<ClusterManager.ManagedCluster>[] checks )
		 {
			  return Set( _clusterManagerBuilder.withAvailabilityChecks( checks ) );
		 }

		 public override ClusterRule WithConsistencyCheckAfterwards()
		 {
			  return Set( _clusterManagerBuilder.withConsistencyCheckAfterwards() );
		 }

		 public override ClusterRule WithFirstInstanceId( int firstInstanceId )
		 {
			  return Set( _clusterManagerBuilder.withFirstInstanceId( firstInstanceId ) );
		 }

		 private ClusterRule Set( ClusterManager.Builder builder )
		 {
			  _clusterManagerBuilder = builder;
			  return this;
		 }

		 public virtual TestDirectory TestDirectory
		 {
			 get
			 {
				  return _testDirectory;
			 }
		 }

		 /// <summary>
		 /// Starts cluster with the configuration provided at instantiation time. This method will not return until the
		 /// cluster is up and all members report each other as available.
		 /// </summary>
		 public virtual ClusterManager.ManagedCluster StartCluster()
		 {
			  if ( _cluster == null )
			  {
					if ( _clusterManager == null )
					{
						 _clusterManager = _clusterManagerBuilder.withRootDirectory( _storeDirectory ).build();
					}

					try
					{
						 _clusterManager.start();
					}
					catch ( Exception throwable )
					{
						 throw new Exception( throwable );
					}
					_cluster = _clusterManager.Cluster;
			  }
			  _cluster.await( allSeesAllAsAvailable() );
			  return _cluster;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.junit.runners.model.Statement apply(final org.junit.runners.model.Statement super, final org.junit.runner.Description description)
		 public override Statement Apply( Statement @base, Description description )
		 {
			  Statement testMethod = new StatementAnonymousInnerClass( this, @base, description );

			  Statement testMethodWithBeforeAndAfter = base.Apply( testMethod, description );

			  return _testDirectory.apply( testMethodWithBeforeAndAfter, description );
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly ClusterRule _outerInstance;

			 private Statement @base;
			 private Description _description;

			 public StatementAnonymousInnerClass( ClusterRule outerInstance, Statement @base, Description description )
			 {
				 this.outerInstance = outerInstance;
				 this.@base = @base;
				 this._description = description;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
			 public override void evaluate()
			 {
				  // If this is used as class rule then getMethodName() returns null, so use
				  // getClassName() instead.
				  string name = _description.MethodName != null ? _description.MethodName : _description.ClassName;
				  _outerInstance.storeDirectory = _outerInstance.testDirectory.directory( name );
				  @base.evaluate();
			 }
		 }

		 protected internal override void After()
		 {
			  ShutdownCluster();
		 }

		 public virtual void ShutdownCluster()
		 {
			  if ( _clusterManager != null )
			  {
					_clusterManager.safeShutdown();
					_cluster = null;
			  }
		 }

		 public virtual File Directory( string name )
		 {
			  return _testDirectory.directory( name );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.io.File cleanDirectory(String name) throws java.io.IOException
		 public virtual File CleanDirectory( string name )
		 {
			  return _testDirectory.cleanDirectory( name );
		 }

		 /// <summary>
		 /// Adapter for providing a static config value into a setting where per-instances dynamic config values
		 /// are supplied.
		 /// </summary>
		 /// <param name="value"> static config value. </param>
		 /// <returns> this <seealso cref="ClusterRule"/> instance, for builder convenience. </returns>
		 public static System.Func<int, string> Constant( string value )
		 {
			  return ClusterManager.constant( value );
		 }

		 /// <summary>
		 /// Dynamic configuration value, of sorts. Can be used as input to <seealso cref="withInstanceConfig(System.Collections.IDictionary)"/>.
		 /// Some configuration values are a function of server id of the cluster member and this is a utility
		 /// for creating such dynamic configuration values.
		 /// </summary>
		 /// <param name="oneBasedServerId"> value onto which one-based server id is added. So for example
		 /// a value of 10 would have cluster member with server id 2 that config value set to 12. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Func<int, String> intBase(final int oneBasedServerId)
		 public static System.Func<int, string> IntBase( int oneBasedServerId )
		 {
			  return serverId => ( oneBasedServerId + serverId ).ToString();
		 }

		 /// <summary>
		 /// Dynamic configuration value, of sorts. Can be used as input to <seealso cref="withInstanceConfig(System.Collections.IDictionary)"/>.
		 /// Some configuration values are a function of server id of the cluster member and this is a utility
		 /// for creating such dynamic configuration values.
		 /// </summary>
		 /// <param name="prefix"> string prefix for these config values. </param>
		 /// <param name="oneBasedServerId"> value onto which one-based server id is added. So for example
		 /// a value of 10 would have cluster member with server id 2 that config value set to 12. </param>
		 /// <returns> a string which has a prefix and an integer part, where the integer part is a function of
		 /// server id of the cluster member. Can be used to set config values like a host, where arguments could look
		 /// something like: {@code prefix: "localhost:" oneBasedServerId: 5000}. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Func<int, String> stringWithIntBase(final String prefix, final int oneBasedServerId)
		 public static System.Func<int, string> StringWithIntBase( string prefix, int oneBasedServerId )
		 {
			  return serverId => prefix + ( oneBasedServerId + serverId );
		 }
	}

}