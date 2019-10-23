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
namespace Neo4Net.causalclustering.stresstests
{

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using DiscoveryImplementation = Neo4Net.causalclustering.discovery.DiscoveryServiceFactorySelector.DiscoveryImplementation;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.getProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.getenv;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"SameParameterValue", "unused"}) public class Config
	public class Config
	{
		 private const string ENV_OVERRIDE_PREFIX = "STRESS_TESTING_";

		 /* platform */
		 private LogProvider _logProvider;
		 private string _workingDir;

		 /* general */
		 private int _numberOfCores;
		 private int _numberOfEdges;

		 private bool _raftMessagesLog;

		 private int _workDurationMinutes;
		 private int _shutdownDurationMinutes;

		 private string _txPrune;
		 private string _checkpointPolicy;

		 private string _discoveryImplementation;

		 private ICollection<Preparations> _preparations;
		 private ICollection<Workloads> _workloads;
		 private ICollection<Validations> _validations;

		 /* workload specific */
		 private bool _enableIndexes;
		 private long _reelectIntervalSeconds;

		 public Config()
		 {
			  _logProvider = FormattedLogProvider.toOutputStream( System.out );
			  _workingDir = EnvOrDefault( "WORKING_DIR", ( new File( getProperty( "java.io.tmpdir" ) ) ).Path );

			  _numberOfCores = EnvOrDefault( "NUMBER_OF_CORES", 3 );
			  _numberOfEdges = EnvOrDefault( "NUMBER_OF_EDGES", 1 );

			  _raftMessagesLog = EnvOrDefault( "ENABLE_RAFT_MESSAGES_LOG", false );

			  _workDurationMinutes = EnvOrDefault( "WORK_DURATION_MINUTES", 30 );
			  _shutdownDurationMinutes = EnvOrDefault( "SHUTDOWN_DURATION_MINUTES", 5 );

			  _txPrune = EnvOrDefault( "TX_PRUNE", "50 files" );
			  _checkpointPolicy = EnvOrDefault( "CHECKPOINT_POLICY", GraphDatabaseSettings.check_point_policy.DefaultValue );

			  _discoveryImplementation = EnvOrDefault( "DISCOVERY_IMPLEMENTATION", CausalClusteringSettings.discovery_implementation.DefaultValue );

			  _preparations = EnvOrDefault( typeof( Preparations ), "PREPARATIONS" );
			  _workloads = EnvOrDefault( typeof( Workloads ), "WORKLOADS" );
			  _validations = EnvOrDefault( typeof( Validations ), "VALIDATIONS", Validations.ConsistencyCheck );

			  _enableIndexes = EnvOrDefault( "ENABLE_INDEXES", false );
			  _reelectIntervalSeconds = EnvOrDefault( "REELECT_INTERVAL_SECONDS", 60L );
		 }

		 private static string EnvOrDefault( string name, string defaultValue )
		 {
			  string environmentVariableName = ENV_OVERRIDE_PREFIX + name;
			  return ofNullable( getenv( environmentVariableName ) ).orElse( defaultValue );
		 }

		 private static int EnvOrDefault( string name, int defaultValue )
		 {
			  string environmentVariableName = ENV_OVERRIDE_PREFIX + name;
			  return ofNullable( getenv( environmentVariableName ) ).map( int?.parseInt ).orElse( defaultValue );
		 }

		 private static long EnvOrDefault( string name, long defaultValue )
		 {
			  string environmentVariableName = ENV_OVERRIDE_PREFIX + name;
			  return ofNullable( getenv( environmentVariableName ) ).map( long?.parseLong ).orElse( defaultValue );
		 }

		 private static bool EnvOrDefault( string name, bool defaultValue )
		 {
			  string environmentVariableName = ENV_OVERRIDE_PREFIX + name;
			  return ofNullable( getenv( environmentVariableName ) ).map( bool?.parseBoolean ).orElse( defaultValue );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private static <T extends Enum<T>> java.util.Collection<T> envOrDefault(Class<T> type, String name, T... defaultValue)
		 private static ICollection<T> EnvOrDefault<T>( Type type, string name, params T[] defaultValue ) where T : Enum<T>
		 {
				 type = typeof( T );
			  string environmentVariableName = ENV_OVERRIDE_PREFIX + name;
			  return ofNullable( getenv( environmentVariableName ) ).map( env => ParseEnum( env, type ) ).orElse( asList( defaultValue ) );
		 }

		 private static ICollection<T> ParseEnum<T>( string value, Type type ) where T : Enum<T>
		 {
				 type = typeof( T );
			  if ( string.ReferenceEquals( value, null ) || value.Length == 0 )
			  {
					return emptyList();
			  }

			  List<T> workloads = new List<T>();
			  string[] split = value.Split( ",", true );
			  foreach ( string workloadString in split )
			  {
					workloads.Add( T.ValueOf( type, workloadString ) );
			  }
			  return workloads;
		 }

		 public virtual LogProvider LogProvider()
		 {
			  return _logProvider;
		 }

		 public virtual void LogProvider( LogProvider logProvider )
		 {
			  this._logProvider = logProvider;
		 }

		 public virtual string WorkingDir()
		 {
			  return _workingDir;
		 }

		 public virtual int NumberOfCores()
		 {
			  return _numberOfCores;
		 }

		 public virtual int NumberOfEdges()
		 {
			  return _numberOfEdges;
		 }

		 public virtual void NumberOfEdges( int numberOfEdges )
		 {
			  this._numberOfEdges = numberOfEdges;
		 }

		 public virtual void WorkDurationMinutes( int workDurationMinutes )
		 {
			  this._workDurationMinutes = workDurationMinutes;
		 }

		 public virtual int WorkDurationMinutes()
		 {
			  return _workDurationMinutes;
		 }

		 public virtual void DiscoveryImplementation( DiscoveryImplementation discoveryImplementation )
		 {
			  this._discoveryImplementation = discoveryImplementation.name();
		 }

		 public virtual int ShutdownDurationMinutes()
		 {
			  return _shutdownDurationMinutes;
		 }

		 public virtual void Preparations( params Preparations[] preparations )
		 {
			  this._preparations = asList( preparations );
		 }

		 public virtual ICollection<Preparations> Preparations()
		 {
			  return _preparations;
		 }

		 public virtual void Workloads( params Workloads[] workloads )
		 {
			  this._workloads = asList( workloads );
		 }

		 public virtual ICollection<Workloads> Workloads()
		 {
			  return _workloads;
		 }

		 public virtual void Validations( params Validations[] validations )
		 {
			  this._validations = asList( validations );
		 }

		 public virtual ICollection<Validations> Validations()
		 {
			  return _validations;
		 }

		 public virtual bool EnableIndexes()
		 {
			  return _enableIndexes;
		 }

		 public virtual long ReelectIntervalSeconds()
		 {
			  return _reelectIntervalSeconds;
		 }

		 public virtual void ReelectIntervalSeconds( int reelectIntervalSeconds )
		 {
			  this._reelectIntervalSeconds = reelectIntervalSeconds;
		 }

		 private void PopulateCommonParams( IDictionary<string, string> @params )
		 {
			  @params[GraphDatabaseSettings.keep_logical_logs.name()] = _txPrune;
			  @params[GraphDatabaseSettings.logical_log_rotation_threshold.name()] = "1M";
			  @params[GraphDatabaseSettings.check_point_policy.name()] = _checkpointPolicy;
			  @params[CausalClusteringSettings.discovery_implementation.name()] = _discoveryImplementation;
		 }

		 public virtual void PopulateCoreParams( IDictionary<string, string> @params )
		 {
			  PopulateCommonParams( @params );

			  @params[CausalClusteringSettings.raft_log_rotation_size.name()] = "1K";
			  @params[CausalClusteringSettings.raft_log_pruning_frequency.name()] = "250ms";
			  @params[CausalClusteringSettings.raft_log_pruning_strategy.name()] = "keep_none";
			  // the following will override the test-default in CoreClusterMember
			  @params[CausalClusteringSettings.raft_messages_log_enable.name()] = Convert.ToString(_raftMessagesLog);
		 }

		 public virtual void PopulateReadReplicaParams( IDictionary<string, string> @params )
		 {
			  PopulateCommonParams( @params );
		 }

		 public override string ToString()
		 {
			  return "Config{" + "discoveryImplementation='" + _discoveryImplementation + '\'' + ", workingDir='" + _workingDir + '\'' + ", numberOfCores=" + _numberOfCores + ", numberOfEdges=" + _numberOfEdges + ", raftMessagesLog=" + _raftMessagesLog + ", workDurationMinutes=" + _workDurationMinutes +
						 ", shutdownDurationMinutes=" + _shutdownDurationMinutes + ", txPrune='" + _txPrune + '\'' + ", checkpointPolicy='" + _checkpointPolicy + '\'' +
						 ", preparations=" + _preparations + ", workloads=" + _workloads + ", validations=" + _validations + ", enableIndexes=" + _enableIndexes +
						 ", reelectIntervalSeconds=" + _reelectIntervalSeconds + '}';
		 }
	}

}