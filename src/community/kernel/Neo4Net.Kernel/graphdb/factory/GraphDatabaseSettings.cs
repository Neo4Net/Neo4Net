using System;
using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.GraphDb.factory
{

	using Description = Neo4Net.Configuration.Description;
	using Dynamic = Neo4Net.Configuration.Dynamic;
	using Internal = Neo4Net.Configuration.Internal;
	using LoadableConfig = Neo4Net.Configuration.LoadableConfig;
	using ReplacedBy = Neo4Net.Configuration.ReplacedBy;
	using Configuration = Neo4Net.Csv.Reader.Configuration;
	using Neo4Net.GraphDb.config;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using ByteUnit = Neo4Net.Io.ByteUnit;
	using BoltConnectorValidator = Neo4Net.Kernel.configuration.BoltConnectorValidator;
	using ConfigurationMigrator = Neo4Net.Kernel.configuration.ConfigurationMigrator;
	using GraphDatabaseConfigurationMigrator = Neo4Net.Kernel.configuration.GraphDatabaseConfigurationMigrator;
	using Group = Neo4Net.Kernel.configuration.Group;
	using GroupSettingSupport = Neo4Net.Kernel.configuration.GroupSettingSupport;
	using HttpConnectorValidator = Neo4Net.Kernel.configuration.HttpConnectorValidator;
	using Migrator = Neo4Net.Kernel.configuration.Migrator;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using Title = Neo4Net.Kernel.configuration.Title;
	using SslPolicyConfigValidator = Neo4Net.Kernel.configuration.ssl.SslPolicyConfigValidator;
	using Edition = Neo4Net.Kernel.impl.factory.Edition;
	using Level = Neo4Net.Logging.Level;
	using LogTimeZone = Neo4Net.Logging.LogTimeZone;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.ByteUnit.kibiBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.BYTES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.DOUBLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.DURATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.INTEGER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.LONG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.NO_DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.PATH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.STRING_LIST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.TIMEZONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.TRUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.advertisedAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.buildSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.derivedSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.except;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.illegalValueMessage;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.legacyFallback;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.list;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.listenAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.matches;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.optionsIgnoreCase;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.optionsObeyCase;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.pathSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.powerOf2;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.range;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.Settings.setting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.ssl.LegacySslPolicyConfig.LEGACY_POLICY_NAME;

	/// <summary>
	/// Settings for Neo4Net.
	/// </summary>
	public class GraphDatabaseSettings : LoadableConfig
	{
		 /// <summary>
		 /// Data block sizes for dynamic array stores.
		 /// </summary>
		 public const int DEFAULT_BLOCK_SIZE = 128;
		 public const int DEFAULT_LABEL_BLOCK_SIZE = 64;
		 public const int MINIMAL_BLOCK_SIZE = 16;

		 // default unspecified transaction timeout
		 public const long UNSPECIFIED_TIMEOUT = 0L;

		 public const string SYSTEM_DATABASE_NAME = "system.db";
		 public const string DEFAULT_DATABASE_NAME = "graph.db";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") @Migrator private static final org.Neo4Net.kernel.configuration.ConfigurationMigrator migrator = new org.Neo4Net.kernel.configuration.GraphDatabaseConfigurationMigrator();
		 private static readonly ConfigurationMigrator _migrator = new GraphDatabaseConfigurationMigrator();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Root relative to which directory settings are resolved. This is set in code and should never be " + "configured explicitly.") public static final org.Neo4Net.graphdb.config.Setting<java.io.File> Neo4Net_home = setting("unsupported.dbms.directories.Neo4Net_home", PATH, NO_DEFAULT);
		 [Description("Root relative to which directory settings are resolved. This is set in code and should never be " + "configured explicitly.")]
		 public static readonly Setting<File> Neo4NetHome = setting( "unsupported.dbms.directories.Neo4Net_home", PATH, NO_DEFAULT );

		 /// @deprecated This setting is deprecated and will be removed in 4.0. 
		 [Description("Name of the database to load"), Obsolete]
		 public static readonly Setting<string> ActiveDatabase = buildSetting( "dbms.active_database", STRING, DEFAULT_DATABASE_NAME ).constraint( except( SYSTEM_DATABASE_NAME ) ).build();

		 [Description("Path of the data directory. You must not configure more than one Neo4Net installation to use the " + "same data directory.")]
		 public static readonly Setting<File> DataDirectory = pathSetting( "dbms.directories.data", "data" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.Neo4Net.graphdb.config.Setting<java.io.File> databases_root_path = derivedSetting("unsupported.dbms.directories.databases.root", data_directory, data -> new java.io.File(data, "databases"), PATH);
		 public static readonly Setting<File> DatabasesRootPath = derivedSetting( "unsupported.dbms.directories.databases.root", DataDirectory, data => new File( data, "databases" ), PATH );

		 /// @deprecated This setting is deprecated and will be removed in 4.0. 
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Deprecated public static final org.Neo4Net.graphdb.config.Setting<java.io.File> database_path = derivedSetting("unsupported.dbms.directories.database", databases_root_path, active_database, (parent, child) -> new java.io.File(parent, child), PATH);
		 [Obsolete]
		 public static readonly Setting<File> DatabasePath = derivedSetting( "unsupported.dbms.directories.database", DatabasesRootPath, ActiveDatabase, ( parent, child ) => new File( parent, child ), PATH );

		 [Title("Read only database"), Description("Only allow read operations from this Neo4Net instance. " + "This mode still requires write access to the directory for lock purposes.")]
		 public static readonly Setting<bool> ReadOnly = setting( "dbms.read_only", BOOLEAN, FALSE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.Neo4Net.graphdb.config.Setting<bool> ephemeral = setting("unsupported.dbms.ephemeral", BOOLEAN, FALSE);
		 public static readonly Setting<bool> Ephemeral = setting( "unsupported.dbms.ephemeral", BOOLEAN, FALSE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.Neo4Net.graphdb.config.Setting<String> lock_manager = setting("unsupported.dbms.lock_manager", STRING, "");
		 public static readonly Setting<string> LockManager = setting( "unsupported.dbms.lock_manager", STRING, "" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.Neo4Net.graphdb.config.Setting<String> tracer = setting("unsupported.dbms.tracer", STRING, NO_DEFAULT);
		 public static readonly Setting<string> Tracer = setting( "unsupported.dbms.tracer", STRING, NO_DEFAULT );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.Neo4Net.graphdb.config.Setting<String> editionName = setting("unsupported.dbms.edition", STRING, org.Neo4Net.kernel.impl.factory.Edition.unknown.toString());
		 public static readonly Setting<string> EditionName = setting( "unsupported.dbms.edition", STRING, Edition.unknown.ToString() );

		 /// @deprecated This setting is deprecated and will be removed in 4.0.
		 /// Please use connector configuration <seealso cref="org.Neo4Net.kernel.configuration.Connector.enabled"/> instead. 
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Title("Disconnected") @Internal @Description("Disable all Bolt protocol connectors. This setting is deprecated and will be removed in 4.0. Please use connector configuration instead.") @Deprecated @ReplacedBy("dbms.connector.X.enabled") public static final org.Neo4Net.graphdb.config.Setting<bool> disconnected = setting("unsupported.dbms.disconnected", BOOLEAN, FALSE);
		 [Title("Disconnected"), Description("Disable all Bolt protocol connectors. This setting is deprecated and will be removed in 4.0. Please use connector configuration instead."), Obsolete, ReplacedBy("dbms.connector.X.enabled")]
		 public static readonly Setting<bool> Disconnected = setting( "unsupported.dbms.disconnected", BOOLEAN, FALSE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Print out the effective Neo4Net configuration after startup.") @Internal public static final org.Neo4Net.graphdb.config.Setting<bool> dump_configuration = setting("unsupported.dbms.report_configuration", BOOLEAN, FALSE);
		 [Description("Print out the effective Neo4Net configuration after startup.")]
		 public static readonly Setting<bool> DumpConfiguration = setting( "unsupported.dbms.report_configuration", BOOLEAN, FALSE );

		 [Description("A strict configuration validation will prevent the database from starting up if unknown " + "configuration options are specified in the Neo4Net settings namespace (such as dbms., ha., cypher., etc). " + "This is currently false by default but will be true by default in 4.0.")]
		 public static readonly Setting<bool> StrictConfigValidation = setting( "dbms.config.strict_validation", BOOLEAN, FALSE );

		 [Description("Whether to allow a store upgrade in case the current version of the database starts against an " + "older store version. " + "Setting this to `true` does not guarantee successful upgrade, it just " + "allows an upgrade to be performed."), Obsolete, ReplacedBy("dbms.allow_upgrade")]
		 public static readonly Setting<bool> AllowStoreUpgrade = setting( "dbms.allow_format_migration", BOOLEAN, FALSE );

		 [Description("Whether to allow an upgrade in case the current version of the database starts against an older version.")]
		 public static readonly Setting<bool> AllowUpgrade = setting( "dbms.allow_upgrade", BOOLEAN, FALSE );

		 [Description("Database record format. Valid values: `standard`, `high_limit`. " + "The `high_limit` format is available for Enterprise Edition only. " + "It is required if you have a graph that is larger than 34 billion nodes, 34 billion relationships, or 68 billion properties. " + "A change of the record format is irreversible. " + "Certain operations may suffer from a performance penalty of up to 10%, which is why this format is not switched on by default.")]
		 public static readonly Setting<string> RecordFormat = setting( "dbms.record_format", Settings.STRING, "" );

		 // Cypher settings
		 // TODO: These should live with cypher
		 [Description("Set this to specify the default parser (language version).")]
		 public static readonly Setting<string> CypherParserVersion = setting( "cypher.default_language_version", optionsObeyCase( "2.3", "3.1", "3.4","3.5", DEFAULT ), DEFAULT );

		 [Description("Set this to specify the default planner for the default language version.")]
		 public static readonly Setting<string> CypherPlanner = setting( "cypher.planner", optionsObeyCase( "COST", "RULE", DEFAULT ), DEFAULT );

		 [Description("Set this to specify the behavior when Cypher planner or runtime hints cannot be fulfilled. " + "If true, then non-conformance will result in an error, otherwise only a warning is generated.")]
		 public static readonly Setting<bool> CypherHintsError = setting( "cypher.hints_error", BOOLEAN, FALSE );

		 [Description("This setting is associated with performance optimization. Set this to `true` in situations where " + "it is preferable to have any queries using the 'shortestPath' function terminate as soon as " + "possible with no answer, rather than potentially running for a long time attempting to find an " + "answer (even if there is no path to be found). " + "For most queries, the 'shortestPath' algorithm will return the correct answer very quickly. However " + "there are some cases where it is possible that the fast bidirectional breadth-first search " + "algorithm will find no results even if they exist. This can happen when the predicates in the " + "`WHERE` clause applied to 'shortestPath' cannot be applied to each step of the traversal, and can " + "only be applied to the entire path. When the query planner detects these special cases, it will " + "plan to perform an exhaustive depth-first search if the fast algorithm finds no paths. However, " + "the exhaustive search may be orders of magnitude slower than the fast algorithm. If it is critical " + "that queries terminate as soon as possible, it is recommended that this option be set to `true`, " + "which means that Neo4Net will never consider using the exhaustive search for shortestPath queries. " + "However, please note that if no paths are found, an error will be thrown at run time, which will " + "need to be handled by the application.")]
		 public static readonly Setting<bool> ForbidExhaustiveShortestpath = setting( "cypher.forbid_exhaustive_shortestpath", BOOLEAN, FALSE );

		 [Description("This setting is associated with performance optimization. The shortest path algorithm does not " + "work when the start and end nodes are the same. With this setting set to `false` no path will " + "be returned when that happens. The default value of `true` will instead throw an exception. " + "This can happen if you perform a shortestPath search after a cartesian product that might have " + "the same start and end nodes for some of the rows passed to shortestPath. If it is preferable " + "to not experience this exception, and acceptable for results to be missing for those rows, then " + "set this to `false`. If you cannot accept missing results, and really want the shortestPath " + "between two common nodes, then re-write the query using a standard Cypher variable length pattern " + "expression followed by ordering by path length and limiting to one result.")]
		 public static readonly Setting<bool> ForbidShortestpathCommonNodes = setting( "cypher.forbid_shortestpath_common_nodes", BOOLEAN, TRUE );

		 [Description("Set this to change the behavior for Cypher create relationship when the start or end node is missing. " + "By default this fails the query and stops execution, but by setting this flag the create operation is " + "simply not performed and execution continues.")]
		 public static readonly Setting<bool> CypherLenientCreateRelationship = setting( "cypher.lenient_create_relationship", BOOLEAN, FALSE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Set this to specify the default runtime for the default language version.") @Internal public static final org.Neo4Net.graphdb.config.Setting<String> cypher_runtime = setting("unsupported.cypher.runtime", optionsIgnoreCase("INTERPRETED", "COMPILED", "SLOTTED", "MORSEL", DEFAULT), DEFAULT);
		 [Description("Set this to specify the default runtime for the default language version.")]
		 public static readonly Setting<string> CypherRuntime = setting( "unsupported.cypher.runtime", optionsIgnoreCase( "INTERPRETED", "COMPILED", "SLOTTED", "MORSEL", DEFAULT ), DEFAULT );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Choose the expression engine. The default is to only compile expressions that are hot, if 'COMPILED' " + "is chosen all expressions will be compiled directly and if 'INTERPRETED' is chosen expressions will " + "never be compiled.") @Internal public static final org.Neo4Net.graphdb.config.Setting<String> cypher_expression_engine = setting("unsupported.cypher.expression_engine", optionsIgnoreCase("INTERPRETED", "COMPILED", "ONLY_WHEN_HOT", DEFAULT), DEFAULT);
		 [Description("Choose the expression engine. The default is to only compile expressions that are hot, if 'COMPILED' " + "is chosen all expressions will be compiled directly and if 'INTERPRETED' is chosen expressions will " + "never be compiled.")]
		 public static readonly Setting<string> CypherExpressionEngine = setting( "unsupported.cypher.expression_engine", optionsIgnoreCase( "INTERPRETED", "COMPILED", "ONLY_WHEN_HOT", DEFAULT ), DEFAULT );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Number of uses before an expression is considered for compilation") @Internal public static final org.Neo4Net.graphdb.config.Setting<int> cypher_expression_recompilation_limit = buildSetting("unsupported.cypher.expression_recompilation_limit", INTEGER, "1").constraint(min(0)).build();
		 [Description("Number of uses before an expression is considered for compilation")]
		 public static readonly Setting<int> CypherExpressionRecompilationLimit = buildSetting( "unsupported.cypher.expression_recompilation_limit", INTEGER, "1" ).constraint( min( 0 ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Enable tracing of compilation in cypher.") @Internal public static final org.Neo4Net.graphdb.config.Setting<bool> cypher_compiler_tracing = setting("unsupported.cypher.compiler_tracing", BOOLEAN, FALSE);
		 [Description("Enable tracing of compilation in cypher.")]
		 public static readonly Setting<bool> CypherCompilerTracing = setting( "unsupported.cypher.compiler_tracing", BOOLEAN, FALSE );

		 [Description("The number of Cypher query execution plans that are cached.")]
		 public static readonly Setting<int> QueryCacheSize = buildSetting( "dbms.query_cache_size", INTEGER, "1000" ).constraint( min( 0 ) ).build();

		 [Description("The threshold when a plan is considered stale. If any of the underlying " + "statistics used to create the plan have changed more than this value, " + "the plan will be considered stale and will be replanned. Change is calculated as " + "abs(a-b)/max(a,b). This means that a value of 0.75 requires the database to approximately " + "quadruple in size. A value of 0 means replan as soon as possible, with the soonest being " + "defined by the cypher.min_replan_interval which defaults to 10s. After this interval the " + "divergence threshold will slowly start to decline, reaching 10% after about 7h. This will " + "ensure that long running databases will still get query replanning on even modest changes, " + "while not replanning frequently unless the changes are very large.")]
		 public static readonly Setting<double> QueryStatisticsDivergenceThreshold = buildSetting( "cypher.statistics_divergence_threshold", DOUBLE, "0.75" ).constraint( range( 0.0, 1.0 ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Large databases might change slowly, and so to prevent queries from never being replanned " + "the divergence threshold set by cypher.statistics_divergence_threshold is configured to " + "shrink over time. " + "The algorithm used to manage this change is set by unsupported.cypher.replan_algorithm " + "and will cause the threshold to reach the value set here once the time since the previous " + "replanning has reached unsupported.cypher.target_replan_interval. " + "Setting this value to higher than the cypher.statistics_divergence_threshold will cause the " + "threshold to not decay over time.") @Internal public static final org.Neo4Net.graphdb.config.Setting<double> query_statistics_divergence_target = buildSetting("unsupported.cypher.statistics_divergence_target", DOUBLE, "0.10").constraint(range(0.0, 1.0)).build();
		 [Description("Large databases might change slowly, and so to prevent queries from never being replanned " + "the divergence threshold set by cypher.statistics_divergence_threshold is configured to " + "shrink over time. " + "The algorithm used to manage this change is set by unsupported.cypher.replan_algorithm " + "and will cause the threshold to reach the value set here once the time since the previous " + "replanning has reached unsupported.cypher.target_replan_interval. " + "Setting this value to higher than the cypher.statistics_divergence_threshold will cause the " + "threshold to not decay over time.")]
		 public static readonly Setting<double> QueryStatisticsDivergenceTarget = buildSetting( "unsupported.cypher.statistics_divergence_target", DOUBLE, "0.10" ).constraint( range( 0.0, 1.0 ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("The threshold when a warning is generated if a label scan is done after a load csv " + "where the label has no index") @Internal public static final org.Neo4Net.graphdb.config.Setting<long> query_non_indexed_label_warning_threshold = setting("unsupported.cypher.non_indexed_label_warning_threshold", LONG, "10000");
		 [Description("The threshold when a warning is generated if a label scan is done after a load csv " + "where the label has no index")]
		 public static readonly Setting<long> QueryNonIndexedLabelWarningThreshold = setting( "unsupported.cypher.non_indexed_label_warning_threshold", LONG, "10000" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("To improve IDP query planning time, we can restrict the internal planning table size, " + "triggering compaction of candidate plans. The smaller the threshold the faster the planning, " + "but the higher the risk of sub-optimal plans.") @Internal public static final org.Neo4Net.graphdb.config.Setting<int> cypher_idp_solver_table_threshold = buildSetting("unsupported.cypher.idp_solver_table_threshold", INTEGER, "128").constraint(min(16)).build();
		 [Description("To improve IDP query planning time, we can restrict the internal planning table size, " + "triggering compaction of candidate plans. The smaller the threshold the faster the planning, " + "but the higher the risk of sub-optimal plans.")]
		 public static readonly Setting<int> CypherIdpSolverTableThreshold = buildSetting( "unsupported.cypher.idp_solver_table_threshold", INTEGER, "128" ).constraint( min( 16 ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("To improve IDP query planning time, we can restrict the internal planning loop duration, " + "triggering more frequent compaction of candidate plans. The smaller the threshold the " + "faster the planning, but the higher the risk of sub-optimal plans.") @Internal public static final org.Neo4Net.graphdb.config.Setting<long> cypher_idp_solver_duration_threshold = buildSetting("unsupported.cypher.idp_solver_duration_threshold", LONG, "1000").constraint(min(10L)).build();
		 [Description("To improve IDP query planning time, we can restrict the internal planning loop duration, " + "triggering more frequent compaction of candidate plans. The smaller the threshold the " + "faster the planning, but the higher the risk of sub-optimal plans.")]
		 public static readonly Setting<long> CypherIdpSolverDurationThreshold = buildSetting( "unsupported.cypher.idp_solver_duration_threshold", LONG, "1000" ).constraint( min( 10L ) ).build();

		 [Description("The minimum time between possible cypher query replanning events. After this time, the graph " + "statistics will be evaluated, and if they have changed by more than the value set by " + "cypher.statistics_divergence_threshold, the query will be replanned. If the statistics have " + "not changed sufficiently, the same interval will need to pass before the statistics will be " + "evaluated again. Each time they are evaluated, the divergence threshold will be reduced slightly " + "until it reaches 10% after 7h, so that even moderately changing databases will see query replanning " + "after a sufficiently long time interval.")]
		 public static readonly Setting<Duration> CypherMinReplanInterval = setting( "cypher.min_replan_interval", DURATION, "10s" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Large databases might change slowly, and to prevent queries from never being replanned " + "the divergence threshold set by cypher.statistics_divergence_threshold is configured to " + "shrink over time. The algorithm used to manage this change is set by " + "unsupported.cypher.replan_algorithm and will cause the threshold to reach " + "the value set by unsupported.cypher.statistics_divergence_target once the time since the " + "previous replanning has reached the value set here. Setting this value to less than the " + "value of cypher.min_replan_interval will cause the threshold to not decay over time.") @Internal public static final org.Neo4Net.graphdb.config.Setting<java.time.Duration> cypher_replan_interval_target = setting("unsupported.cypher.target_replan_interval", DURATION, "7h");
		 [Description("Large databases might change slowly, and to prevent queries from never being replanned " + "the divergence threshold set by cypher.statistics_divergence_threshold is configured to " + "shrink over time. The algorithm used to manage this change is set by " + "unsupported.cypher.replan_algorithm and will cause the threshold to reach " + "the value set by unsupported.cypher.statistics_divergence_target once the time since the " + "previous replanning has reached the value set here. Setting this value to less than the " + "value of cypher.min_replan_interval will cause the threshold to not decay over time.")]
		 public static readonly Setting<Duration> CypherReplanIntervalTarget = setting( "unsupported.cypher.target_replan_interval", DURATION, "7h" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Large databases might change slowly, and to prevent queries from never being replanned " + "the divergence threshold set by cypher.statistics_divergence_threshold is configured to " + "shrink over time using the algorithm set here. This will cause the threshold to reach " + "the value set by unsupported.cypher.statistics_divergence_target once the time since the " + "previous replanning has reached the value set in unsupported.cypher.target_replan_interval. " + "Setting the algorithm to 'none' will cause the threshold to not decay over time.") @Internal public static final org.Neo4Net.graphdb.config.Setting<String> cypher_replan_algorithm = setting("unsupported.cypher.replan_algorithm", optionsObeyCase("inverse", "exponential", "none", DEFAULT), DEFAULT);
		 [Description("Large databases might change slowly, and to prevent queries from never being replanned " + "the divergence threshold set by cypher.statistics_divergence_threshold is configured to " + "shrink over time using the algorithm set here. This will cause the threshold to reach " + "the value set by unsupported.cypher.statistics_divergence_target once the time since the " + "previous replanning has reached the value set in unsupported.cypher.target_replan_interval. " + "Setting the algorithm to 'none' will cause the threshold to not decay over time.")]
		 public static readonly Setting<string> CypherReplanAlgorithm = setting( "unsupported.cypher.replan_algorithm", optionsObeyCase( "inverse", "exponential", "none", DEFAULT ), DEFAULT );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Enable using minimum cardinality estimates in the Cypher cost planner, so that cardinality " + "estimates for logical plan operators are not allowed to go below certain thresholds even when " + "the statistics give smaller numbers. " + "This is especially useful for large import queries that write nodes and relationships " + "into an empty or small database, where the generated query plan needs to be able to scale " + "beyond the initial statistics. Otherwise, when this is disabled, the statistics on an empty " + "or tiny database may lead the cost planner to for example pick a scan over an index seek, even " + "when an index exists, because of a lower estimated cost.") @Internal public static final org.Neo4Net.graphdb.config.Setting<bool> cypher_plan_with_minimum_cardinality_estimates = setting("unsupported.cypher.plan_with_minimum_cardinality_estimates", BOOLEAN, TRUE);
		 [Description("Enable using minimum cardinality estimates in the Cypher cost planner, so that cardinality " + "estimates for logical plan operators are not allowed to go below certain thresholds even when " + "the statistics give smaller numbers. " + "This is especially useful for large import queries that write nodes and relationships " + "into an empty or small database, where the generated query plan needs to be able to scale " + "beyond the initial statistics. Otherwise, when this is disabled, the statistics on an empty " + "or tiny database may lead the cost planner to for example pick a scan over an index seek, even " + "when an index exists, because of a lower estimated cost.")]
		 public static readonly Setting<bool> CypherPlanWithMinimumCardinalityEstimates = setting( "unsupported.cypher.plan_with_minimum_cardinality_estimates", BOOLEAN, TRUE );

		 [Description("Determines if Cypher will allow using file URLs when loading data using `LOAD CSV`. Setting this " + "value to `false` will cause Neo4Net to fail `LOAD CSV` clauses that load data from the file system.")]
		 public static readonly Setting<bool> AllowFileUrls = setting( "dbms.security.allow_csv_import_from_file_urls", BOOLEAN, TRUE );

		 [Description("Sets the root directory for file URLs used with the Cypher `LOAD CSV` clause. This should be set to a " + "directory relative to the Neo4Net installation path, restricting access to only those files within that directory " + "and its subdirectories. For example the value \"import\" will only enable access to files within the 'import' folder. " + "Removing this setting will disable the security feature, allowing all files in the local system to be imported. " + "Setting this to an empty field will allow access to all files within the Neo4Net installation folder.")]
		 public static readonly Setting<File> LoadCsvFileUrlRoot = pathSetting( "dbms.directories.import", NO_DEFAULT );

		 [Description("Selects whether to conform to the standard https://tools.ietf.org/html/rfc4180 for interpreting " + "escaped quotation characters in CSV files loaded using `LOAD CSV`. Setting this to `false` will use" + " the standard, interpreting repeated quotes '\"\"' as a single in-lined quote, while `true` will " + "use the legacy convention originally supported in Neo4Net 3.0 and 3.1, allowing a backslash to " + "include quotes in-lined in fields.")]
		 public static readonly Setting<bool> CsvLegacyQuoteEscaping = setting( "dbms.import.csv.legacy_quote_escaping", BOOLEAN, Convert.ToString( Neo4Net.Csv.Reader.Configuration_Fields.DEFAULT_LEGACY_STYLE_QUOTING ) );

		 [Description("The size of the internal buffer in bytes used by `LOAD CSV`. If the csv file contains huge fields " + "this value may have to be increased.")]
		 public static Setting<int> CsvBufferSize = buildSetting( "dbms.import.csv.buffer_size", INTEGER, Convert.ToString( 2 * Neo4Net.Csv.Reader.Configuration_Fields.Mb ) ).constraint( min( 1 ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Enables or disables tracking of how much time a query spends actively executing on the CPU. " + "Calling `dbms.listQueries` will display the time. " + "This can also be logged in the query log by using `log_queries_detailed_time_logging_enabled`.") @Dynamic public static final org.Neo4Net.graphdb.config.Setting<bool> track_query_cpu_time = setting("dbms.track_query_cpu_time", BOOLEAN, FALSE);
		 [Description("Enables or disables tracking of how much time a query spends actively executing on the CPU. " + "Calling `dbms.listQueries` will display the time. " + "This can also be logged in the query log by using `log_queries_detailed_time_logging_enabled`.")]
		 public static readonly Setting<bool> TrackQueryCpuTime = setting( "dbms.track_query_cpu_time", BOOLEAN, FALSE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Enables or disables tracking of how many bytes are allocated by the execution of a query. " + "Calling `dbms.listQueries` will display the time. " + "This can also be logged in the query log by using `log_queries_allocation_logging_enabled`.") @Dynamic public static final org.Neo4Net.graphdb.config.Setting<bool> track_query_allocation = setting("dbms.track_query_allocation", BOOLEAN, FALSE);
		 [Description("Enables or disables tracking of how many bytes are allocated by the execution of a query. " + "Calling `dbms.listQueries` will display the time. " + "This can also be logged in the query log by using `log_queries_allocation_logging_enabled`.")]
		 public static readonly Setting<bool> TrackQueryAllocation = setting( "dbms.track_query_allocation", BOOLEAN, FALSE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Enable tracing of morsel runtime scheduler.") @Internal public static final org.Neo4Net.graphdb.config.Setting<bool> enable_morsel_runtime_trace = setting("unsupported.cypher.enable_morsel_runtime_trace", BOOLEAN, FALSE);
		 [Description("Enable tracing of morsel runtime scheduler.")]
		 public static readonly Setting<bool> EnableMorselRuntimeTrace = setting( "unsupported.cypher.enable_morsel_runtime_trace", BOOLEAN, FALSE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("The size of the morsels") @Internal public static final org.Neo4Net.graphdb.config.Setting<int> cypher_morsel_size = setting("unsupported.cypher.morsel_size", INTEGER, "10000");
		 [Description("The size of the morsels")]
		 public static readonly Setting<int> CypherMorselSize = setting( "unsupported.cypher.morsel_size", INTEGER, "10000" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Duration in milliseconds that parallel runtime waits on a task before trying another task") @Internal public static final org.Neo4Net.graphdb.config.Setting<int> cypher_task_wait = setting("unsupported.cypher.task_wait", INTEGER, "30000");
		 [Description("Duration in milliseconds that parallel runtime waits on a task before trying another task")]
		 public static readonly Setting<int> CypherTaskWait = setting( "unsupported.cypher.task_wait", INTEGER, "30000" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Number of threads to allocate to Cypher worker threads. If set to 0, two workers will be started" + " for every physical core in the system.") @Internal public static final org.Neo4Net.graphdb.config.Setting<int> cypher_worker_count = setting("unsupported.cypher.number_of_workers", INTEGER, "0");
		 [Description("Number of threads to allocate to Cypher worker threads. If set to 0, two workers will be started" + " for every physical core in the system.")]
		 public static readonly Setting<int> CypherWorkerCount = setting( "unsupported.cypher.number_of_workers", INTEGER, "0" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Max number of recent queries to collect in the data collector module. Will round down to the" + " nearest power of two. The default number (8192 query invocations) " + " was chosen as a trade-off between getting a useful amount of queries, and not" + " wasting too much heap. Even with a buffer full of unique queries, the estimated" + " footprint lies in tens of MBs. If the buffer is full of cached queries, the" + " retained size was measured to 265 kB. Setting this to 0 will disable data collection" + " of queries completely.") @Internal public static final org.Neo4Net.graphdb.config.Setting<int> data_collector_max_recent_query_count = buildSetting("unsupported.datacollector.max_recent_query_count", INTEGER, "8192").constraint(min(0)).build();
		 [Description("Max number of recent queries to collect in the data collector module. Will round down to the" + " nearest power of two. The default number (8192 query invocations) " + " was chosen as a trade-off between getting a useful amount of queries, and not" + " wasting too much heap. Even with a buffer full of unique queries, the estimated" + " footprint lies in tens of MBs. If the buffer is full of cached queries, the" + " retained size was measured to 265 kB. Setting this to 0 will disable data collection" + " of queries completely.")]
		 public static readonly Setting<int> DataCollectorMaxRecentQueryCount = buildSetting( "unsupported.datacollector.max_recent_query_count", INTEGER, "8192" ).constraint( min( 0 ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Sets the upper limit for how much of the query text that will be retained by the query collector." + " For queries longer than the limit, only a prefix of size limit will be retained by the collector." + " Lowering this value will reduce the memory footprint of collected query invocations under loads with" + " many queries with long query texts, which could occur for generated queries. The downside is that" + " on retrieving queries by `db.stats.retrieve`, queries longer than this max size would be returned" + " incomplete. Setting this to 0 will completely drop query texts from the collected queries.") @Internal public static final org.Neo4Net.graphdb.config.Setting<int> data_collector_max_query_text_size = buildSetting("unsupported.datacollector.max_query_text_size", INTEGER, "10000").constraint(min(0)).build();
		 [Description("Sets the upper limit for how much of the query text that will be retained by the query collector." + " For queries longer than the limit, only a prefix of size limit will be retained by the collector." + " Lowering this value will reduce the memory footprint of collected query invocations under loads with" + " many queries with long query texts, which could occur for generated queries. The downside is that" + " on retrieving queries by `db.stats.retrieve`, queries longer than this max size would be returned" + " incomplete. Setting this to 0 will completely drop query texts from the collected queries.")]
		 public static readonly Setting<int> DataCollectorMaxQueryTextSize = buildSetting( "unsupported.datacollector.max_query_text_size", INTEGER, "10000" ).constraint( min( 0 ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("The maximum amount of time to wait for the database to become available, when " + "starting a new transaction.") @Internal public static final org.Neo4Net.graphdb.config.Setting<java.time.Duration> transaction_start_timeout = setting("unsupported.dbms.transaction_start_timeout", DURATION, "1s");
		 [Description("The maximum amount of time to wait for the database to become available, when " + "starting a new transaction.")]
		 public static readonly Setting<Duration> TransactionStartTimeout = setting( "unsupported.dbms.transaction_start_timeout", DURATION, "1s" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Please use dbms.transaction.timeout instead.") @Deprecated @ReplacedBy("dbms.transaction.timeout") public static final org.Neo4Net.graphdb.config.Setting<bool> execution_guard_enabled = setting("unsupported.dbms.executiontime_limit.enabled", BOOLEAN, FALSE);
		 [Description("Please use dbms.transaction.timeout instead."), Obsolete, ReplacedBy("dbms.transaction.timeout")]
		 public static readonly Setting<bool> ExecutionGuardEnabled = setting( "unsupported.dbms.executiontime_limit.enabled", BOOLEAN, FALSE );

		  // @see Status.Transaction#TransactionTimedOut
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("The maximum time interval of a transaction within which it should be completed.") @Dynamic public static final org.Neo4Net.graphdb.config.Setting<java.time.Duration> transaction_timeout = setting("dbms.transaction.timeout", DURATION, String.valueOf(UNSPECIFIED_TIMEOUT));
		 [Description("The maximum time interval of a transaction within which it should be completed.")]
		 public static readonly Setting<Duration> TransactionTimeout = setting( "dbms.transaction.timeout", DURATION, UNSPECIFIED_TIMEOUT.ToString() );

		  // @see Status.Transaction#LockAcquisitionTimeout
		 [Description("The maximum time interval within which lock should be acquired.")]
		 public static readonly Setting<Duration> LockAcquisitionTimeout = setting( "dbms.lock.acquisition.timeout", DURATION, UNSPECIFIED_TIMEOUT.ToString() );

		 [Description("Configures the time interval between transaction monitor checks. Determines how often " + "monitor thread will check transaction for timeout.")]
		 public static readonly Setting<Duration> TransactionMonitorCheckInterval = setting( "dbms.transaction.monitor.check.interval", DURATION, "2s" );

		 [Description("The maximum amount of time to wait for running transactions to complete before allowing " + "initiated database shutdown to continue")]
		 public static readonly Setting<Duration> ShutdownTransactionEndTimeout = setting( "dbms.shutdown_transaction_end_timeout", DURATION, "10s" );

		 [Description("Location of the database plugin directory. Compiled Java JAR files that contain database " + "procedures will be loaded if they are placed in this directory.")]
		 public static readonly Setting<File> PluginDir = pathSetting( "dbms.directories.plugins", "plugins" );

		 [Description("Threshold for rotation of the user log. If set to 0 log rotation is disabled.")]
		 public static readonly Setting<long> StoreUserLogRotationThreshold = buildSetting( "dbms.logs.user.rotation.size", BYTES, "0" ).constraint( range( 0L, long.MaxValue ) ).build();

		 [Description("Threshold for rotation of the debug log.")]
		 public static readonly Setting<long> StoreInternalLogRotationThreshold = buildSetting( "dbms.logs.debug.rotation.size", BYTES, "20m" ).constraint( range( 0L, long.MaxValue ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Debug log contexts that should output debug level logging") @Internal public static final org.Neo4Net.graphdb.config.Setting<java.util.List<String>> store_internal_debug_contexts = setting("unsupported.dbms.logs.debug.debug_loggers", list(",", STRING), "org.Neo4Net.diagnostics,org.Neo4Net.cluster.protocol,org.Neo4Net.kernel.ha");
		 [Description("Debug log contexts that should output debug level logging")]
		 public static readonly Setting<IList<string>> StoreInternalDebugContexts = setting( "unsupported.dbms.logs.debug.debug_loggers", list( ",", STRING ), "org.Neo4Net.diagnostics,org.Neo4Net.cluster.protocol,org.Neo4Net.kernel.ha" );

		 [Description("Debug log level threshold.")]
		 public static readonly Setting<Level> StoreInternalLogLevel = setting( "dbms.logs.debug.level", optionsObeyCase( typeof( Level ) ), "INFO" );

		 [Description("Database timezone. Among other things, this setting influences which timezone the logs and monitoring procedures use.")]
		 public static readonly Setting<LogTimeZone> DbTimezone = setting( "dbms.db.timezone", optionsObeyCase( typeof( LogTimeZone ) ), LogTimeZone.UTC.name() );

		 [Description("Database logs timezone."), Obsolete, ReplacedBy("dbms.db.timezone")]
		 public static readonly Setting<LogTimeZone> LogTimezone = setting( "dbms.logs.timezone", optionsObeyCase( typeof( LogTimeZone ) ), LogTimeZone.UTC.name() );

		 [Description("Database timezone for temporal functions. All Time and DateTime values that are created without " + "an explicit timezone will use this configured default timezone.")]
		 public static readonly Setting<ZoneId> DbTemporalTimezone = setting( "db.temporal.timezone", TIMEZONE, ZoneOffset.UTC.ToString() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Maximum time to wait for active transaction completion when rotating counts store") @Internal public static final org.Neo4Net.graphdb.config.Setting<java.time.Duration> counts_store_rotation_timeout = setting("unsupported.dbms.counts_store_rotation_timeout", DURATION, "10m");
		 [Description("Maximum time to wait for active transaction completion when rotating counts store")]
		 public static readonly Setting<Duration> CountsStoreRotationTimeout = setting( "unsupported.dbms.counts_store_rotation_timeout", DURATION, "10m" );

		 [Description("Minimum time interval after last rotation of the user log before it may be rotated again.")]
		 public static readonly Setting<Duration> StoreUserLogRotationDelay = setting( "dbms.logs.user.rotation.delay", DURATION, "300s" );

		 [Description("Minimum time interval after last rotation of the debug log before it may be rotated again.")]
		 public static readonly Setting<Duration> StoreInternalLogRotationDelay = setting( "dbms.logs.debug.rotation.delay", DURATION, "300s" );

		 [Description("Maximum number of history files for the user log.")]
		 public static readonly Setting<int> StoreUserLogMaxArchives = buildSetting( "dbms.logs.user.rotation.keep_number", INTEGER, "7" ).constraint( min( 1 ) ).build();

		 [Description("Maximum number of history files for the debug log.")]
		 public static readonly Setting<int> StoreInternalLogMaxArchives = buildSetting( "dbms.logs.debug.rotation.keep_number", INTEGER, "7" ).constraint( min( 1 ) ).build();

		 [Description("Configures the general policy for when check-points should occur. The default policy is the " + "'periodic' check-point policy, as specified by the 'dbms.checkpoint.interval.tx' and " + "'dbms.checkpoint.interval.time' settings. " + "The Neo4Net Enterprise Edition provides two alternative policies: " + "The first is the 'continuous' check-point policy, which will ignore those settings and run the " + "check-point process all the time. " + "The second is the 'volumetric' check-point policy, which makes a best-effort at check-pointing " + "often enough so that the database doesn't get too far behind on deleting old transaction logs in " + "accordance with the 'dbms.tx_log.rotation.retention_policy' setting.")]
		 public static readonly Setting<string> CheckPointPolicy = setting( "dbms.checkpoint", STRING, "periodic" );

		 [Description("Configures the transaction interval between check-points. The database will not check-point more " + "often  than this (unless check pointing is triggered by a different event), but might check-point " + "less often than this interval, if performing a check-point takes longer time than the configured " + "interval. A check-point is a point in the transaction logs, from which recovery would start from. " + "Longer check-point intervals typically means that recovery will take longer to complete in case " + "of a crash. On the other hand, a longer check-point interval can also reduce the I/O load that " + "the database places on the system, as each check-point implies a flushing and forcing of all the " + "store files.  The default is '100000' for a check-point every 100000 transactions.")]
		 public static readonly Setting<int> CheckPointIntervalTx = buildSetting( "dbms.checkpoint.interval.tx", INTEGER, "100000" ).constraint( min( 1 ) ).build();

		 [Description("Configures the time interval between check-points. The database will not check-point more often " + "than this (unless check pointing is triggered by a different event), but might check-point less " + "often than this interval, if performing a check-point takes longer time than the configured " + "interval. A check-point is a point in the transaction logs, from which recovery would start from. " + "Longer check-point intervals typically means that recovery will take longer to complete in case " + "of a crash. On the other hand, a longer check-point interval can also reduce the I/O load that " + "the database places on the system, as each check-point implies a flushing and forcing of all the " + "store files.")]
		 public static readonly Setting<Duration> CheckPointIntervalTime = setting( "dbms.checkpoint.interval.time", DURATION, "15m" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Dynamic @Description("Limit the number of IOs the background checkpoint process will consume per second. " + "This setting is advisory, is ignored in Neo4Net Community Edition, and is followed to " + "best effort in Enterprise Edition. " + "An IO is in this case a 8 KiB (mostly sequential) write. Limiting the write IO in " + "this way will leave more bandwidth in the IO subsystem to service random-read IOs, " + "which is important for the response time of queries when the database cannot fit " + "entirely in memory. The only drawback of this setting is that longer checkpoint times " + "may lead to slightly longer recovery times in case of a database or system crash. " + "A lower number means lower IO pressure, and consequently longer checkpoint times. " + "Set this to -1 to disable the IOPS limit and remove the limitation entirely; " + "this will let the checkpointer flush data as fast as the hardware will go. " + "Removing the setting, or commenting it out, will set the default value of 300.") public static final org.Neo4Net.graphdb.config.Setting<int> check_point_iops_limit = setting("dbms.checkpoint.iops.limit", INTEGER, "300");
		 [Description("Limit the number of IOs the background checkpoint process will consume per second. " + "This setting is advisory, is ignored in Neo4Net Community Edition, and is followed to " + "best effort in Enterprise Edition. " + "An IO is in this case a 8 KiB (mostly sequential) write. Limiting the write IO in " + "this way will leave more bandwidth in the IO subsystem to service random-read IOs, " + "which is important for the response time of queries when the database cannot fit " + "entirely in memory. The only drawback of this setting is that longer checkpoint times " + "may lead to slightly longer recovery times in case of a database or system crash. " + "A lower number means lower IO pressure, and consequently longer checkpoint times. " + "Set this to -1 to disable the IOPS limit and remove the limitation entirely; " + "this will let the checkpointer flush data as fast as the hardware will go. " + "Removing the setting, or commenting it out, will set the default value of 300.")]
		 public static readonly Setting<int> CheckPointIopsLimit = setting( "dbms.checkpoint.iops.limit", INTEGER, "300" );

		 // Auto Indexing
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Controls the auto indexing feature for nodes. Setting it to `false` shuts it down, " + "while `true` enables it by default for properties listed in the dbms.auto_index.nodes.keys setting.") @Internal @Deprecated public static final org.Neo4Net.graphdb.config.Setting<bool> node_auto_indexing = setting("dbms.auto_index.nodes.enabled", BOOLEAN, FALSE);
		 [Description("Controls the auto indexing feature for nodes. Setting it to `false` shuts it down, " + "while `true` enables it by default for properties listed in the dbms.auto_index.nodes.keys setting."), Obsolete]
		 public static readonly Setting<bool> NodeAutoIndexing = setting( "dbms.auto_index.nodes.enabled", BOOLEAN, FALSE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("A list of property names (comma separated) that will be indexed by default. This applies to " + "_nodes_ only.") @Internal @Deprecated public static final org.Neo4Net.graphdb.config.Setting<java.util.List<String>> node_keys_indexable = setting("dbms.auto_index.nodes.keys", STRING_LIST, "");
		 [Description("A list of property names (comma separated) that will be indexed by default. This applies to " + "_nodes_ only."), Obsolete]
		 public static readonly Setting<IList<string>> NodeKeysIndexable = setting( "dbms.auto_index.nodes.keys", STRING_LIST, "" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Controls the auto indexing feature for relationships. Setting it to `false` shuts it down, " + "while `true` enables it by default for properties listed in the dbms.auto_index.relationships.keys " + "setting.") @Internal @Deprecated public static final org.Neo4Net.graphdb.config.Setting<bool> relationship_auto_indexing = setting("dbms.auto_index.relationships.enabled", BOOLEAN, FALSE);
		 [Description("Controls the auto indexing feature for relationships. Setting it to `false` shuts it down, " + "while `true` enables it by default for properties listed in the dbms.auto_index.relationships.keys " + "setting."), Obsolete]
		 public static readonly Setting<bool> RelationshipAutoIndexing = setting( "dbms.auto_index.relationships.enabled", BOOLEAN, FALSE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("A list of property names (comma separated) that will be indexed by default. This applies to " + "_relationships_ only.") @Internal @Deprecated public static final org.Neo4Net.graphdb.config.Setting<java.util.List<String>> relationship_keys_indexable = setting("dbms.auto_index.relationships.keys", STRING_LIST, "");
		 [Description("A list of property names (comma separated) that will be indexed by default. This applies to " + "_relationships_ only."), Obsolete]
		 public static readonly Setting<IList<string>> RelationshipKeysIndexable = setting( "dbms.auto_index.relationships.keys", STRING_LIST, "" );

		 // Index sampling
		 [Description("Enable or disable background index sampling")]
		 public static readonly Setting<bool> IndexBackgroundSamplingEnabled = setting( "dbms.index_sampling.background_enabled", BOOLEAN, TRUE );

		 [Description("Size of buffer used by index sampling. " + "This configuration setting is no longer applicable as from Neo4Net 3.0.3. " + "Please use dbms.index_sampling.sample_size_limit instead."), Obsolete, ReplacedBy("dbms.index_sampling.sample_size_limit")]
		 public static readonly Setting<long> IndexSamplingBufferSize = buildSetting( "dbms.index_sampling.buffer_size", BYTES, "64m" ).constraint( range( 1048576L, ( long ) int.MaxValue ) ).build();

		 [Description("Index sampling chunk size limit")]
		 public static readonly Setting<int> IndexSampleSizeLimit = buildSetting( "dbms.index_sampling.sample_size_limit", INTEGER, ByteUnit.mebiBytes( 8 ).ToString() ).constraint(range((int) ByteUnit.mebiBytes(1), int.MaxValue)).build();

		 [Description("Percentage of index updates of total index size required before sampling of a given index is " + "triggered")]
		 public static readonly Setting<int> IndexSamplingUpdatePercentage = buildSetting( "dbms.index_sampling.update_percentage", INTEGER, "5" ).constraint( min( 0 ) ).build();

		 // Lucene settings
		 [Description("The maximum number of open Lucene index searchers.")]
		 public static readonly Setting<int> LuceneSearcherCacheSize = buildSetting( "dbms.index_searcher_cache_size",INTEGER, Convert.ToString( int.MaxValue ) ).constraint( min( 1 ) ).build();

		 // Lucene schema indexes
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.Neo4Net.graphdb.config.Setting<bool> multi_threaded_schema_index_population_enabled = setting("unsupported.dbms.multi_threaded_schema_index_population_enabled", BOOLEAN, TRUE);
		 public static readonly Setting<bool> MultiThreadedSchemaIndexPopulationEnabled = setting( "unsupported.dbms.multi_threaded_schema_index_population_enabled", BOOLEAN, TRUE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @ReplacedBy("dbms.index.default_schema_provider") @Internal public static final org.Neo4Net.graphdb.config.Setting<bool> enable_native_schema_index = setting("unsupported.dbms.enable_native_schema_index", BOOLEAN, TRUE);
		 [Obsolete, ReplacedBy("dbms.index.default_schema_provider")]
		 public static readonly Setting<bool> EnableNativeSchemaIndex = setting( "unsupported.dbms.enable_native_schema_index", BOOLEAN, TRUE );

		 public sealed class SchemaIndex
		 {
			  public static readonly SchemaIndex NativeBtree10 = new SchemaIndex( "NativeBtree10", InnerEnum.NativeBtree10, "native-btree", "1.0", false );
			  public static readonly SchemaIndex Native20 = new SchemaIndex( "Native20", InnerEnum.Native20, "lucene+native", "2.0", true );
			  public static readonly SchemaIndex Native10 = new SchemaIndex( "Native10", InnerEnum.Native10, "lucene+native", "1.0", true );
			  public static readonly SchemaIndex Lucene10 = new SchemaIndex( "Lucene10", InnerEnum.Lucene10, "lucene", "1.0", true );

			  private static readonly IList<SchemaIndex> valueList = new List<SchemaIndex>();

			  static SchemaIndex()
			  {
				  valueList.Add( NativeBtree10 );
				  valueList.Add( Native20 );
				  valueList.Add( Native10 );
				  valueList.Add( Lucene10 );
			  }

			  public enum InnerEnum
			  {
				  NativeBtree10,
				  Native20,
				  Native10,
				  Lucene10
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Private readonly;
			  internal Private readonly;
			  internal Private readonly;
			  internal Private readonly;

			  internal SchemaIndex( string name, InnerEnum innerEnum, string providerKey, string providerVersion, bool deprecated )
			  {
					this._providerKey = providerKey;
					this._providerVersion = providerVersion;
					this._deprecated = deprecated;
					this._providerName = ToProviderName( providerKey, providerVersion );

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  public string ProviderName()
			  {
					return _providerName;
			  }

			  public string ProviderKey()
			  {
					return _providerKey;
			  }

			  public string ProviderVersion()
			  {
					return _providerVersion;
			  }

			  public bool Deprecated()
			  {
					return _deprecated;
			  }

			  internal static string ToProviderName( string providerName, string providerVersion )
			  {
					return providerName + "-" + providerVersion;
			  }

			 public static IList<SchemaIndex> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static SchemaIndex valueOf( string name )
			 {
				 foreach ( SchemaIndex enumInstance in SchemaIndex.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 [Description("Index provider to use for newly created schema indexes. " + "An index provider may store different value types in separate physical indexes. " + "lucene-1.0: Spatial and temporal value types are stored in native indexes, remaining value types in Lucene index. " + "lucene+native-1.0: Spatial, temporal and number value types are stored in native indexes and remaining value types in Lucene index. " + "lucene+native-2.0: Spatial, temporal, number and string value types are stored in native indexes and remaining value types in Lucene index. " + "native-btree-1.0: All value types and arrays of all value types, even composite keys, are stored in one native index. " + "A native index has faster updates, less heap and CPU usage compared to a Lucene index. " + "A native index has these limitations: " + "Index key (be it single or composite) size limit of 4039 bytes - transaction resulting in index key surpassing that will fail. " + "Reduced performance of CONTAINS and ENDS WITH string index queries, compared to a Lucene index.")]
		 public static readonly Setting<string> DefaultSchemaProvider = setting( "dbms.index.default_schema_provider", STRING, NATIVE_BTREE10.providerName() );

		 [Description("Location where Neo4Net keeps the logical transaction logs.")]
		 public static readonly Setting<File> LogicalLogsLocation = pathSetting( "dbms.directories.tx_log", "", DatabasePath );

		 // Store settings
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Make Neo4Net keep the logical transaction logs for being able to backup the database. " + "Can be used for specifying the threshold to prune logical logs after. For example \"10 days\" will " + "prune logical logs that only contains transactions older than 10 days from the current time, " + "or \"100k txs\" will keep the 100k latest transactions and prune any older transactions.") @Dynamic public static final org.Neo4Net.graphdb.config.Setting<String> keep_logical_logs = buildSetting("dbms.tx_log.rotation.retention_policy", STRING, "7 days").constraint(illegalValueMessage("must be `true`, `false` or of format `<number><optional unit> <type>`. " + "Valid units are `k`, `M` and `G`. " + "Valid types are `files`, `size`, `txs`, `entries`, `hours` and `days`. " + "For example, `100M size` will limiting logical log space on disk to 100Mb," + " or `200k txs` will limiting the number of transactions to keep to 200 000", matches("^(true|keep_all|false|keep_none|(\\d+[KkMmGg]?( (files|size|txs|entries|hours|days))))$"))).build();
		 [Description("Make Neo4Net keep the logical transaction logs for being able to backup the database. " + "Can be used for specifying the threshold to prune logical logs after. For example \"10 days\" will " + "prune logical logs that only contains transactions older than 10 days from the current time, " + "or \"100k txs\" will keep the 100k latest transactions and prune any older transactions.")]
		 public static readonly Setting<string> KeepLogicalLogs = buildSetting( "dbms.tx_log.rotation.retention_policy", STRING, "7 days" ).constraint( illegalValueMessage( "must be `true`, `false` or of format `<number><optional unit> <type>`. " + "Valid units are `k`, `M` and `G`. " + "Valid types are `files`, `size`, `txs`, `entries`, `hours` and `days`. " + "For example, `100M size` will limiting logical log space on disk to 100Mb," + " or `200k txs` will limiting the number of transactions to keep to 200 000", matches( "^(true|keep_all|false|keep_none|(\\d+[KkMmGg]?( (files|size|txs|entries|hours|days))))$" ) ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Specifies at which file size the logical log will auto-rotate. Minimum accepted value is 1M. ") @Dynamic public static final org.Neo4Net.graphdb.config.Setting<long> logical_log_rotation_threshold = buildSetting("dbms.tx_log.rotation.size", BYTES, "250M").constraint(min(org.Neo4Net.io.ByteUnit.mebiBytes(1))).build();
		 [Description("Specifies at which file size the logical log will auto-rotate. Minimum accepted value is 1M. ")]
		 public static readonly Setting<long> LogicalLogRotationThreshold = buildSetting( "dbms.tx_log.rotation.size", BYTES, "250M" ).constraint( min( ByteUnit.mebiBytes( 1 ) ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("If `true`, Neo4Net will abort recovery if any errors are encountered in the logical log. Setting " + "this to `false` will allow Neo4Net to restore as much as possible from the corrupted log files and ignore " + "the rest, but, the integrity of the database might be compromised.") @Internal public static final org.Neo4Net.graphdb.config.Setting<bool> fail_on_corrupted_log_files = setting("unsupported.dbms.tx_log.fail_on_corrupted_log_files", BOOLEAN, TRUE);
		 [Description("If `true`, Neo4Net will abort recovery if any errors are encountered in the logical log. Setting " + "this to `false` will allow Neo4Net to restore as much as possible from the corrupted log files and ignore " + "the rest, but, the integrity of the database might be compromised.")]
		 public static readonly Setting<bool> FailOnCorruptedLogFiles = setting( "unsupported.dbms.tx_log.fail_on_corrupted_log_files", BOOLEAN, TRUE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Use a quick approach for rebuilding the ID generators. This give quicker recovery time, " + "but will limit the ability to reuse the space of deleted entities.") @Internal public static final org.Neo4Net.graphdb.config.Setting<bool> rebuild_idgenerators_fast = setting("unsupported.dbms.id_generator_fast_rebuild_enabled", BOOLEAN, TRUE);
		 [Description("Use a quick approach for rebuilding the ID generators. This give quicker recovery time, " + "but will limit the ability to reuse the space of deleted entities.")]
		 public static readonly Setting<bool> RebuildIdgeneratorsFast = setting( "unsupported.dbms.id_generator_fast_rebuild_enabled", BOOLEAN, TRUE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Specifies if engine should run cypher query based on a snapshot of accessed data. " + "Query will be restarted in case if concurrent modification of data will be detected.") @Internal public static final org.Neo4Net.graphdb.config.Setting<bool> snapshot_query = setting("unsupported.dbms.query.snapshot", BOOLEAN, FALSE);
		 [Description("Specifies if engine should run cypher query based on a snapshot of accessed data. " + "Query will be restarted in case if concurrent modification of data will be detected.")]
		 public static readonly Setting<bool> SnapshotQuery = setting( "unsupported.dbms.query.snapshot", BOOLEAN, FALSE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Specifies number or retries that query engine will do to execute query based on " + "stable accessed data snapshot before giving up.") @Internal public static final org.Neo4Net.graphdb.config.Setting<int> snapshot_query_retries = buildSetting("unsupported.dbms.query.snapshot.retries", INTEGER, "5").constraint(range(1, Integer.MAX_VALUE)).build();
		 [Description("Specifies number or retries that query engine will do to execute query based on " + "stable accessed data snapshot before giving up.")]
		 public static readonly Setting<int> SnapshotQueryRetries = buildSetting( "unsupported.dbms.query.snapshot.retries", INTEGER, "5" ).constraint( range( 1, int.MaxValue ) ).build();

		 // Store memory settings
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Target size for pages of mapped memory. If set to 0, then a reasonable default is chosen, " + "depending on the storage device used.") @Internal @Deprecated public static final org.Neo4Net.graphdb.config.Setting<long> mapped_memory_page_size = setting("unsupported.dbms.memory.pagecache.pagesize", BYTES, "0");
		 [Description("Target size for pages of mapped memory. If set to 0, then a reasonable default is chosen, " + "depending on the storage device used."), Obsolete]
		 public static readonly Setting<long> MappedMemoryPageSize = setting( "unsupported.dbms.memory.pagecache.pagesize", BYTES, "0" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Description("The amount of memory to use for mapping the store files, in bytes (or kilobytes with the 'k' " + "suffix, megabytes with 'm' and gigabytes with 'g'). If Neo4Net is running on a dedicated server, " + "then it is generally recommended to leave about 2-4 gigabytes for the operating system, give the " + "JVM enough heap to hold all your transaction state and query context, and then leave the rest for " + "the page cache. If no page cache memory is configured, then a heuristic setting is computed based " + "on available system resources.") public static final org.Neo4Net.graphdb.config.Setting<String> pagecache_memory = buildSetting("dbms.memory.pagecache.size", STRING, null).build();
		 [Description("The amount of memory to use for mapping the store files, in bytes (or kilobytes with the 'k' " + "suffix, megabytes with 'm' and gigabytes with 'g'). If Neo4Net is running on a dedicated server, " + "then it is generally recommended to leave about 2-4 gigabytes for the operating system, give the " + "JVM enough heap to hold all your transaction state and query context, and then leave the rest for " + "the page cache. If no page cache memory is configured, then a heuristic setting is computed based " + "on available system resources.")]
		 public static readonly Setting<string> PagecacheMemory = buildSetting( "dbms.memory.pagecache.size", STRING, null ).build();

		 [Description("Specify which page swapper to use for doing paged IO. " + "This is only used when integrating with proprietary storage technology.")]
		 public static readonly Setting<string> PagecacheSwapper = setting( "dbms.memory.pagecache.swapper", STRING, null );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("The profiling frequency for the page cache. Accurate profiles allow the page cache to do active " + "warmup after a restart, reducing the mean time to performance. " + "This feature available in Neo4Net Enterprise Edition.") public static final org.Neo4Net.graphdb.config.Setting<java.time.Duration> pagecache_warmup_profiling_interval = setting("unsupported.dbms.memory.pagecache.warmup.profile.interval", DURATION, "1m");
		 [Description("The profiling frequency for the page cache. Accurate profiles allow the page cache to do active " + "warmup after a restart, reducing the mean time to performance. " + "This feature available in Neo4Net Enterprise Edition.")]
		 public static readonly Setting<Duration> PagecacheWarmupProfilingInterval = setting( "unsupported.dbms.memory.pagecache.warmup.profile.interval", DURATION, "1m" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Page cache can be configured to perform usage sampling of loaded pages that can be used to construct active load profile. " + "According to that profile pages can be reloaded on the restart, replication, etc. " + "This setting allows disabling that behavior. " + "This feature available in Neo4Net Enterprise Edition.") public static final org.Neo4Net.graphdb.config.Setting<bool> pagecache_warmup_enabled = setting("unsupported.dbms.memory.pagecache.warmup.enable", BOOLEAN, TRUE);
		 [Description("Page cache can be configured to perform usage sampling of loaded pages that can be used to construct active load profile. " + "According to that profile pages can be reloaded on the restart, replication, etc. " + "This setting allows disabling that behavior. " + "This feature available in Neo4Net Enterprise Edition.")]
		 public static readonly Setting<bool> PagecacheWarmupEnabled = setting( "unsupported.dbms.memory.pagecache.warmup.enable", BOOLEAN, TRUE );

		 [Description("Allows the enabling or disabling of the file watcher service." + " This is an auxiliary service but should be left enabled in almost all cases.")]
		 public static readonly Setting<bool> FilewatcherEnabled = setting( "dbms.filewatcher.enabled", BOOLEAN, TRUE );

		 /// <summary>
		 /// Block size properties values depends from selected record format.
		 /// We can't figured out record format until it will be selected by corresponding edition.
		 /// As soon as we will figure it out properties will be re-evaluated and overwritten, except cases of user
		 /// defined value.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Specifies the block size for storing strings. This parameter is only honored when the store is " + "created, otherwise it is ignored. " + "Note that each character in a string occupies two bytes, meaning that e.g a block size of 120 will hold " + "a 60 character long string before overflowing into a second block. " + "Also note that each block carries a ~10B of overhead so record size on disk will be slightly larger " + "than the configured block size") @Internal public static final org.Neo4Net.graphdb.config.Setting<int> string_block_size = buildSetting("unsupported.dbms.block_size.strings", INTEGER, "0").constraint(min(0)).build();
		 [Description("Specifies the block size for storing strings. This parameter is only honored when the store is " + "created, otherwise it is ignored. " + "Note that each character in a string occupies two bytes, meaning that e.g a block size of 120 will hold " + "a 60 character long string before overflowing into a second block. " + "Also note that each block carries a ~10B of overhead so record size on disk will be slightly larger " + "than the configured block size")]
		 public static readonly Setting<int> StringBlockSize = buildSetting( "unsupported.dbms.block_size.strings", INTEGER, "0" ).constraint( min( 0 ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Specifies the block size for storing arrays. This parameter is only honored when the store is " + "created, otherwise it is ignored. " + "Also note that each block carries a ~10B of overhead so record size on disk will be slightly larger " + "than the configured block size") @Internal public static final org.Neo4Net.graphdb.config.Setting<int> array_block_size = buildSetting("unsupported.dbms.block_size.array_properties", INTEGER, "0").constraint(min(0)).build();
		 [Description("Specifies the block size for storing arrays. This parameter is only honored when the store is " + "created, otherwise it is ignored. " + "Also note that each block carries a ~10B of overhead so record size on disk will be slightly larger " + "than the configured block size")]
		 public static readonly Setting<int> ArrayBlockSize = buildSetting( "unsupported.dbms.block_size.array_properties", INTEGER, "0" ).constraint( min( 0 ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Specifies the block size for storing labels exceeding in-lined space in node record. " + "This parameter is only honored when the store is created, otherwise it is ignored. " + "Also note that each block carries a ~10B of overhead so record size on disk will be slightly larger " + "than the configured block size") @Internal public static final org.Neo4Net.graphdb.config.Setting<int> label_block_size = buildSetting("unsupported.dbms.block_size.labels", INTEGER, "0").constraint(min(0)).build();
		 [Description("Specifies the block size for storing labels exceeding in-lined space in node record. " + "This parameter is only honored when the store is created, otherwise it is ignored. " + "Also note that each block carries a ~10B of overhead so record size on disk will be slightly larger " + "than the configured block size")]
		 public static readonly Setting<int> LabelBlockSize = buildSetting( "unsupported.dbms.block_size.labels", INTEGER, "0" ).constraint( min( 0 ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Specifies the size of id batches local to each transaction when committing. " + "Committing a transaction which contains changes most often results in new data records being created. " + "For each record a new id needs to be generated from an id generator. " + "It's more efficient to allocate a batch of ids from the contended id generator, which the transaction " + "holds and generates ids from while creating these new records. " + "This setting specifies how big those batches are. " + "Remaining ids are freed back to id generator on clean shutdown.") @Internal public static final org.Neo4Net.graphdb.config.Setting<int> record_id_batch_size = buildSetting("unsupported.dbms.record_id_batch_size", INTEGER, "20").constraint(range(1, 1_000)).build();
		 [Description("Specifies the size of id batches local to each transaction when committing. " + "Committing a transaction which contains changes most often results in new data records being created. " + "For each record a new id needs to be generated from an id generator. " + "It's more efficient to allocate a batch of ids from the contended id generator, which the transaction " + "holds and generates ids from while creating these new records. " + "This setting specifies how big those batches are. " + "Remaining ids are freed back to id generator on clean shutdown.")]
		 public static readonly Setting<int> RecordIdBatchSize = buildSetting( "unsupported.dbms.record_id_batch_size", INTEGER, "20" ).constraint( range( 1, 1_000 ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("An identifier that uniquely identifies this graph database instance within this JVM. " + "Defaults to an auto-generated number depending on how many instance are started in this JVM.") @Internal public static final org.Neo4Net.graphdb.config.Setting<String> forced_kernel_id = buildSetting("unsupported.dbms.kernel_id", STRING, NO_DEFAULT).constraint(illegalValueMessage("has to be a valid kernel identifier", matches("[a-zA-Z0-9]*"))).build();
		 [Description("An identifier that uniquely identifies this graph database instance within this JVM. " + "Defaults to an auto-generated number depending on how many instance are started in this JVM.")]
		 public static readonly Setting<string> ForcedKernelId = buildSetting( "unsupported.dbms.kernel_id", STRING, NO_DEFAULT ).constraint( illegalValueMessage( "has to be a valid kernel identifier", matches( "[a-zA-Z0-9]*" ) ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.Neo4Net.graphdb.config.Setting<java.time.Duration> vm_pause_monitor_measurement_duration = setting("unsupported.vm_pause_monitor.measurement_duration", DURATION, "100ms");
		 public static readonly Setting<Duration> VmPauseMonitorMeasurementDuration = setting( "unsupported.vm_pause_monitor.measurement_duration", DURATION, "100ms" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.Neo4Net.graphdb.config.Setting<java.time.Duration> vm_pause_monitor_stall_alert_threshold = setting("unsupported.vm_pause_monitor.stall_alert_threshold", DURATION, "100ms");
		 public static readonly Setting<Duration> VmPauseMonitorStallAlertThreshold = setting( "unsupported.vm_pause_monitor.stall_alert_threshold", DURATION, "100ms" );

		 [Description("Relationship count threshold for considering a node to be dense")]
		 public static readonly Setting<int> DenseNodeThreshold = buildSetting( "dbms.relationship_grouping_threshold", INTEGER, "50" ).constraint( min( 1 ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Log executed queries that take longer than the configured threshold, dbms.logs.query.threshold. " + "Log entries are by default written to the file _query.log_ located in the Logs directory. " + "For location of the Logs directory, see <<file-locations>>. " + "This feature is available in the Neo4Net Enterprise Edition.") @Dynamic public static final org.Neo4Net.graphdb.config.Setting<bool> log_queries = setting("dbms.logs.query.enabled", BOOLEAN, FALSE);
		 [Description("Log executed queries that take longer than the configured threshold, dbms.logs.query.threshold. " + "Log entries are by default written to the file _query.log_ located in the Logs directory. " + "For location of the Logs directory, see <<file-locations>>. " + "This feature is available in the Neo4Net Enterprise Edition.")]
		 public static readonly Setting<bool> LogQueries = setting( "dbms.logs.query.enabled", BOOLEAN, FALSE );

		 [Description("Send user logs to the process stdout. " + "If this is disabled then logs will instead be sent to the file _Neo4Net.log_ located in the logs directory. " + "For location of the Logs directory, see <<file-locations>>.")]
		 public static readonly Setting<bool> StoreUserLogToStdout = setting( "dbms.logs.user.stdout_enabled", BOOLEAN, TRUE );

		 [Description("Path of the logs directory.")]
		 public static readonly Setting<File> LogsDirectory = pathSetting( "dbms.directories.logs", "logs" );

		 [Description("Path to the query log file.")]
		 public static readonly Setting<File> LogQueriesFilename = derivedSetting( "dbms.logs.query.path", LogsDirectory, logs => new File( logs, "query.log" ), PATH );

		 [Description("Path to the user log file.")]
		 public static readonly Setting<File> StoreUserLogPath = derivedSetting( "dbms.logs.user.path", LogsDirectory, logs => new File( logs, "Neo4Net.log" ), PATH );

		 [Description("Path to the debug log file.")]
		 public static readonly Setting<File> StoreInternalLogPath = derivedSetting( "dbms.logs.debug.path", LogsDirectory, logs => new File( logs, "debug.log" ), PATH );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Log parameters for the executed queries being logged.") @Dynamic public static final org.Neo4Net.graphdb.config.Setting<bool> log_queries_parameter_logging_enabled = setting("dbms.logs.query.parameter_logging_enabled", BOOLEAN, TRUE);
		 [Description("Log parameters for the executed queries being logged.")]
		 public static readonly Setting<bool> LogQueriesParameterLoggingEnabled = setting( "dbms.logs.query.parameter_logging_enabled", BOOLEAN, TRUE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Log detailed time information for the executed queries being logged. Requires `dbms.track_query_cpu_time=true`") @Dynamic public static final org.Neo4Net.graphdb.config.Setting<bool> log_queries_detailed_time_logging_enabled = setting("dbms.logs.query.time_logging_enabled", BOOLEAN, FALSE);
		 [Description("Log detailed time information for the executed queries being logged. Requires `dbms.track_query_cpu_time=true`")]
		 public static readonly Setting<bool> LogQueriesDetailedTimeLoggingEnabled = setting( "dbms.logs.query.time_logging_enabled", BOOLEAN, FALSE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Log allocated bytes for the executed queries being logged. " + "The logged number is cumulative over the duration of the query, " + "i.e. for memory intense or long-running queries the value may be larger " + "than the current memory allocation. Requires `dbms.track_query_allocation=true`") @Dynamic public static final org.Neo4Net.graphdb.config.Setting<bool> log_queries_allocation_logging_enabled = setting("dbms.logs.query.allocation_logging_enabled", BOOLEAN, FALSE);
		 [Description("Log allocated bytes for the executed queries being logged. " + "The logged number is cumulative over the duration of the query, " + "i.e. for memory intense or long-running queries the value may be larger " + "than the current memory allocation. Requires `dbms.track_query_allocation=true`")]
		 public static readonly Setting<bool> LogQueriesAllocationLoggingEnabled = setting( "dbms.logs.query.allocation_logging_enabled", BOOLEAN, FALSE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Logs which runtime that was used to run the query") @Dynamic public static final org.Neo4Net.graphdb.config.Setting<bool> log_queries_runtime_logging_enabled = setting("dbms.logs.query.runtime_logging_enabled", BOOLEAN, FALSE);
		 [Description("Logs which runtime that was used to run the query")]
		 public static readonly Setting<bool> LogQueriesRuntimeLoggingEnabled = setting( "dbms.logs.query.runtime_logging_enabled", BOOLEAN, FALSE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Log page hits and page faults for the executed queries being logged.") @Dynamic public static final org.Neo4Net.graphdb.config.Setting<bool> log_queries_page_detail_logging_enabled = setting("dbms.logs.query.page_logging_enabled", BOOLEAN, FALSE);
		 [Description("Log page hits and page faults for the executed queries being logged.")]
		 public static readonly Setting<bool> LogQueriesPageDetailLoggingEnabled = setting( "dbms.logs.query.page_logging_enabled", BOOLEAN, FALSE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("If the execution of query takes more time than this threshold, the query is logged - " + "provided query logging is enabled. Defaults to 0 seconds, that is all queries are logged.") @Dynamic public static final org.Neo4Net.graphdb.config.Setting<java.time.Duration> log_queries_threshold = setting("dbms.logs.query.threshold", DURATION, "0s");
		 [Description("If the execution of query takes more time than this threshold, the query is logged - " + "provided query logging is enabled. Defaults to 0 seconds, that is all queries are logged.")]
		 public static readonly Setting<Duration> LogQueriesThreshold = setting( "dbms.logs.query.threshold", DURATION, "0s" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("The file size in bytes at which the query log will auto-rotate. If set to zero then no rotation " + "will occur. Accepts a binary suffix `k`, `m` or `g`.") @Dynamic public static final org.Neo4Net.graphdb.config.Setting<long> log_queries_rotation_threshold = buildSetting("dbms.logs.query.rotation.size", BYTES, "20m").constraint(range(0L, Long.MAX_VALUE)).build();
		 [Description("The file size in bytes at which the query log will auto-rotate. If set to zero then no rotation " + "will occur. Accepts a binary suffix `k`, `m` or `g`.")]
		 public static readonly Setting<long> LogQueriesRotationThreshold = buildSetting( "dbms.logs.query.rotation.size", BYTES, "20m" ).constraint( range( 0L, long.MaxValue ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Maximum number of history files for the query log.") @Dynamic public static final org.Neo4Net.graphdb.config.Setting<int> log_queries_max_archives = buildSetting("dbms.logs.query.rotation.keep_number", INTEGER, "7").constraint(min(1)).build();
		 [Description("Maximum number of history files for the query log.")]
		 public static readonly Setting<int> LogQueriesMaxArchives = buildSetting( "dbms.logs.query.rotation.keep_number", INTEGER, "7" ).constraint( min( 1 ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Specifies number of operations that batch inserter will try to group into one batch before " + "flushing data into underlying storage.") @Internal public static final org.Neo4Net.graphdb.config.Setting<int> batch_inserter_batch_size = setting("unsupported.tools.batch_inserter.batch_size", INTEGER, "10000");
		 [Description("Specifies number of operations that batch inserter will try to group into one batch before " + "flushing data into underlying storage.")]
		 public static readonly Setting<int> BatchInserterBatchSize = setting( "unsupported.tools.batch_inserter.batch_size", INTEGER, "10000" );

		 /// @deprecated - lucene label index has been removed. 
		 [Obsolete("- lucene label index has been removed.")]
		 public enum LabelIndex
		 {
			  /// <summary>
			  /// Native label index. Generally the best option.
			  /// </summary>
			  Native,

			  /// <summary>
			  /// Label index backed by Lucene.
			  /// </summary>
			  Lucene,

			  /// <summary>
			  /// Selects which ever label index is present in a store, or the default (NATIVE) if no label index present.
			  /// </summary>
			  Auto
		 }

		 /// @deprecated - lucene label index has been removed, thus 'native' is only viable option and this setting is not needed. 
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated("- lucene label index has been removed, thus 'native' is only viable option and this setting is not needed.") @Description("Backend to use for label --> nodes index") @Internal public static final org.Neo4Net.graphdb.config.Setting<String> label_index = setting("dbms.label_index", optionsIgnoreCase(LabelIndex.NATIVE.name(), LabelIndex.AUTO.name()), LabelIndex.NATIVE.name());
		 [Obsolete("- lucene label index has been removed, thus 'native' is only viable option and this setting is not needed."), Description("Backend to use for label --> nodes index")]
		 public static readonly Setting<string> LabelIndex = setting( "dbms.label_index", optionsIgnoreCase( LabelIndex.Native.name(), LabelIndex.Auto.name() ), LabelIndex.Native.name() );

		 // Security settings

		 [Description("Enable auth requirement to access Neo4Net.")]
		 public static readonly Setting<bool> AuthEnabled = setting( "dbms.security.auth_enabled", BOOLEAN, FALSE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.Neo4Net.graphdb.config.Setting<java.io.File> auth_store = pathSetting("unsupported.dbms.security.auth_store.location", NO_DEFAULT);
		 public static readonly Setting<File> AuthStore = pathSetting( "unsupported.dbms.security.auth_store.location", NO_DEFAULT );

		 [Description("The maximum number of unsuccessful authentication attempts before imposing a user lock for the configured amount of time." + "The locked out user will not be able to log in until the lock period expires, even if correct credentials are provided. " + "Setting this configuration option to values less than 3 is not recommended because it might make it easier for an attacker " + "to brute force the password.")]
		 public static readonly Setting<int> AuthMaxFailedAttempts = buildSetting( "dbms.security.auth_max_failed_attempts", INTEGER, "3" ).constraint( min( 0 ) ).build();

		 [Description("The amount of time user account should be locked after a configured number of unsuccessful authentication attempts. " + "The locked out user will not be able to log in until the lock period expires, even if correct credentials are provided. " + "Setting this configuration option to a low value is not recommended because it might make it easier for an attacker to " + "brute force the password.")]
		 public static readonly Setting<Duration> AuthLockTime = buildSetting( "dbms.security.auth_lock_time", DURATION, "5s" ).constraint( min( Duration.ofSeconds( 0 ) ) ).build();

		 [Description("A list of procedures and user defined functions (comma separated) that are allowed full access to " + "the database. The list may contain both fully-qualified procedure names, and partial names with the " + "wildcard '*'. Note that this enables these procedures to bypass security. Use with caution.")]
		 public static readonly Setting<string> ProcedureUnrestricted = setting( "dbms.security.procedures.unrestricted", Settings.STRING, "" );

		 [Description("Specifies whether or not dbms.killQueries produces a verbose output, with information about which queries were not found")]
		 public static readonly Setting<bool> KillQueryVerbose = setting( "dbms.procedures.kill_query_verbose", BOOLEAN, TRUE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Whether or not to release the exclusive schema lock is while building uniqueness constraints index") @Internal public static final org.Neo4Net.graphdb.config.Setting<bool> release_schema_lock_while_building_constraint = setting("unsupported.dbms.schema.release_lock_while_building_constraint", BOOLEAN, FALSE);
		 [Obsolete, Description("Whether or not to release the exclusive schema lock is while building uniqueness constraints index")]
		 public static readonly Setting<bool> ReleaseSchemaLockWhileBuildingConstraint = setting( "unsupported.dbms.schema.release_lock_while_building_constraint", BOOLEAN, FALSE );

		 [Description("A list of procedures (comma separated) that are to be loaded. " + "The list may contain both fully-qualified procedure names, and partial names with the wildcard '*'. " + "If this setting is left empty no procedures will be loaded.")]
		 public static readonly Setting<string> ProcedureWhitelist = setting( "dbms.security.procedures.whitelist", Settings.STRING, "*" );
		 // Bolt Settings

		 [Description("Default network interface to listen for incoming connections. " + "To listen for connections on all interfaces, use \"0.0.0.0\". " + "To bind specific connectors to a specific network interfaces, " + "specify the +listen_address+ properties for the specific connector.")]
		 public static readonly Setting<string> DefaultListenAddress = setting( "dbms.connectors.default_listen_address", STRING, "127.0.0.1" );

		 [Description("Default hostname or IP address the server uses to advertise itself to its connectors. " + "To advertise a specific hostname or IP address for a specific connector, " + "specify the +advertised_address+ property for the specific connector.")]
		 public static readonly Setting<string> DefaultAdvertisedAddress = setting( "dbms.connectors.default_advertised_address", STRING, "localhost" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Whether to apply network level outbound network buffer based throttling") @Internal public static final org.Neo4Net.graphdb.config.Setting<bool> bolt_outbound_buffer_throttle = setting("unsupported.dbms.bolt.outbound_buffer_throttle", BOOLEAN, TRUE);
		 [Description("Whether to apply network level outbound network buffer based throttling")]
		 public static readonly Setting<bool> BoltOutboundBufferThrottle = setting( "unsupported.dbms.bolt.outbound_buffer_throttle", BOOLEAN, TRUE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("When the size (in bytes) of outbound network buffers, used by bolt's network layer, " + "grows beyond this value bolt channel will advertise itself as unwritable and will block " + "related processing thread until it becomes writable again.") @Internal public static final org.Neo4Net.graphdb.config.Setting<int> bolt_outbound_buffer_throttle_high_water_mark = buildSetting("unsupported.dbms.bolt.outbound_buffer_throttle.high_watermark", INTEGER, String.valueOf(org.Neo4Net.io.ByteUnit.kibiBytes(512))).constraint(range((int) org.Neo4Net.io.ByteUnit.kibiBytes(64), Integer.MAX_VALUE)).build();
		 [Description("When the size (in bytes) of outbound network buffers, used by bolt's network layer, " + "grows beyond this value bolt channel will advertise itself as unwritable and will block " + "related processing thread until it becomes writable again.")]
		 public static readonly Setting<int> BoltOutboundBufferThrottleHighWaterMark = buildSetting( "unsupported.dbms.bolt.outbound_buffer_throttle.high_watermark", INTEGER, ByteUnit.kibiBytes( 512 ).ToString() ).constraint(range((int) ByteUnit.kibiBytes(64), int.MaxValue)).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("When the size (in bytes) of outbound network buffers, previously advertised as unwritable, " + "gets below this value bolt channel will re-advertise itself as writable and blocked processing " + "thread will resume execution.") @Internal public static final org.Neo4Net.graphdb.config.Setting<int> bolt_outbound_buffer_throttle_low_water_mark = buildSetting("unsupported.dbms.bolt.outbound_buffer_throttle.low_watermark", INTEGER, String.valueOf(org.Neo4Net.io.ByteUnit.kibiBytes(128))).constraint(range((int) org.Neo4Net.io.ByteUnit.kibiBytes(16), Integer.MAX_VALUE)).build();
		 [Description("When the size (in bytes) of outbound network buffers, previously advertised as unwritable, " + "gets below this value bolt channel will re-advertise itself as writable and blocked processing " + "thread will resume execution.")]
		 public static readonly Setting<int> BoltOutboundBufferThrottleLowWaterMark = buildSetting( "unsupported.dbms.bolt.outbound_buffer_throttle.low_watermark", INTEGER, ByteUnit.kibiBytes( 128 ).ToString() ).constraint(range((int) ByteUnit.kibiBytes(16), int.MaxValue)).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("When the total time outbound network buffer based throttle lock is held exceeds this value, " + "the corresponding bolt channel will be aborted. Setting " + "this to 0 will disable this behaviour.") @Internal public static final org.Neo4Net.graphdb.config.Setting<java.time.Duration> bolt_outbound_buffer_throttle_max_duration = buildSetting("unsupported.dbms.bolt.outbound_buffer_throttle.max_duration", DURATION, "15m").constraint(min(java.time.Duration.ofSeconds(30))).build();
		 [Description("When the total time outbound network buffer based throttle lock is held exceeds this value, " + "the corresponding bolt channel will be aborted. Setting " + "this to 0 will disable this behaviour.")]
		 public static readonly Setting<Duration> BoltOutboundBufferThrottleMaxDuration = buildSetting( "unsupported.dbms.bolt.outbound_buffer_throttle.max_duration", DURATION, "15m" ).constraint( min( Duration.ofSeconds( 30 ) ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("When the number of queued inbound messages grows beyond this value, reading from underlying " + "channel will be paused (no more inbound messages will be available) until queued number of " + "messages drops below the configured low watermark value.") @Internal public static final org.Neo4Net.graphdb.config.Setting<int> bolt_inbound_message_throttle_high_water_mark = buildSetting("unsupported.dbms.bolt.inbound_message_throttle.high_watermark", INTEGER, String.valueOf(300)).constraint(range(1, Integer.MAX_VALUE)).build();
		 [Description("When the number of queued inbound messages grows beyond this value, reading from underlying " + "channel will be paused (no more inbound messages will be available) until queued number of " + "messages drops below the configured low watermark value.")]
		 public static readonly Setting<int> BoltInboundMessageThrottleHighWaterMark = buildSetting( "unsupported.dbms.bolt.inbound_message_throttle.high_watermark", INTEGER, 300.ToString() ).constraint(range(1, int.MaxValue)).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("When the number of queued inbound messages, previously reached configured high watermark value, " + "drops below this value, reading from underlying channel will be enabled and any pending messages " + "will start queuing again.") @Internal public static final org.Neo4Net.graphdb.config.Setting<int> bolt_inbound_message_throttle_low_water_mark = buildSetting("unsupported.dbms.bolt.inbound_message_throttle.low_watermark", INTEGER, String.valueOf(100)).constraint(range(1, Integer.MAX_VALUE)).build();
		 [Description("When the number of queued inbound messages, previously reached configured high watermark value, " + "drops below this value, reading from underlying channel will be enabled and any pending messages " + "will start queuing again.")]
		 public static readonly Setting<int> BoltInboundMessageThrottleLowWaterMark = buildSetting( "unsupported.dbms.bolt.inbound_message_throttle.low_watermark", INTEGER, 100.ToString() ).constraint(range(1, int.MaxValue)).build();

		 [Description("Specify the SSL policy to use for the encrypted bolt connections.")]
		 public static readonly Setting<string> BoltSslPolicy = setting( "bolt.ssl_policy", STRING, LEGACY_POLICY_NAME );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Create an archive of an index before re-creating it if failing to load on startup.") @Internal public static final org.Neo4Net.graphdb.config.Setting<bool> archive_failed_index = setting("unsupported.dbms.index.archive_failed", BOOLEAN, FALSE);
		 [Description("Create an archive of an index before re-creating it if failing to load on startup.")]
		 public static readonly Setting<bool> ArchiveFailedIndex = setting( "unsupported.dbms.index.archive_failed", BOOLEAN, FALSE );

		 // Needed to validate config, accessed via reflection
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public static final org.Neo4Net.kernel.configuration.BoltConnectorValidator boltValidator = new org.Neo4Net.kernel.configuration.BoltConnectorValidator();
		 public static readonly BoltConnectorValidator BoltValidator = new BoltConnectorValidator();

		 // Needed to validate config, accessed via reflection
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public static final org.Neo4Net.kernel.configuration.ssl.SslPolicyConfigValidator sslPolicyConfigValidator = new org.Neo4Net.kernel.configuration.ssl.SslPolicyConfigValidator();
		 public static readonly SslPolicyConfigValidator SslPolicyConfigValidator = new SslPolicyConfigValidator();

		 [Description("The maximum amount of time to wait for the database state represented by the bookmark.")]
		 public static readonly Setting<Duration> BookmarkReadyTimeout = buildSetting( "dbms.transaction.bookmark_ready_timeout", DURATION, "30s" ).constraint( min( Duration.ofSeconds( 1 ) ) ).build();

		 public enum TransactionStateMemoryAllocation
		 {
			  OnHeap,
			  OffHeap
		 }

		 [Description("Defines whether memory for transaction state should be allocated on- or off-heap.")]
		 public static readonly Setting<TransactionStateMemoryAllocation> TxStateMemoryAllocation = buildSetting( "dbms.tx_state.memory_allocation", optionsIgnoreCase( typeof( TransactionStateMemoryAllocation ) ), TransactionStateMemoryAllocation.OnHeap.name() ).build();

		 [Description("The maximum amount of off-heap memory that can be used to store transaction state data; it's a total amount of memory " + "shared across all active transactions. Zero means 'unlimited'. Used when dbms.tx_state.memory_allocation is set to 'OFF_HEAP'.")]
		 public static readonly Setting<long> TxStateMaxOffHeapMemory = buildSetting( "dbms.tx_state.max_off_heap_memory", BYTES, "2G" ).constraint( min( 0L ) ).build();

		 [Description("Defines the maximum size of an off-heap memory block that can be cached to speed up allocations for transaction state data. " + "The value must be a power of 2.")]
		 public static readonly Setting<long> TxStateOffHeapMaxCacheableBlockSize = buildSetting( "dbms.tx_state.off_heap.max_cacheable_block_size", BYTES, "512k" ).constraint( min( kibiBytes( 4 ) ) ).constraint( powerOf2() ).build();

		 [Description("Defines the size of the off-heap memory blocks cache. The cache will contain this number of blocks for each block size " + "that is power of two. Thus, maximum amount of memory used by blocks cache can be calculated as " + "2 * dbms.tx_state.off_heap.max_cacheable_block_size * dbms.tx_state.off_heap.block_cache_size")]
		 public static readonly Setting<int> TxStateOffHeapBlockCacheSize = buildSetting( "dbms.tx_state.off_heap.block_cache_size", INTEGER, "128" ).constraint( min( 16 ) ).build();

		 // Needed to validate config, accessed via reflection
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public static final org.Neo4Net.kernel.configuration.HttpConnectorValidator httpValidator = new org.Neo4Net.kernel.configuration.HttpConnectorValidator();
		 public static readonly HttpConnectorValidator HttpValidator = new HttpConnectorValidator();

		 /// <param name="key"> connection identifier. </param>
		 /// <returns> a new connector setting instance. </returns>
		 /// @deprecated use <seealso cref="org.Neo4Net.kernel.configuration.BoltConnector"/> instead. This will be removed in 4.0. 
		 [Obsolete("use <seealso cref=\"org.Neo4Net.kernel.configuration.BoltConnector\"/> instead. This will be removed in 4.0.")]
		 public static BoltConnector BoltConnector( string key )
		 {
			  return new BoltConnector( key );
		 }

		 /// @deprecated see <seealso cref="org.Neo4Net.kernel.configuration.Connector"/> instead. This will be removed in 4.0. 
		 [Group("dbms.connector")]
		 public class Connector
		 {
			  [Description("Enable this connector")]
			  public readonly Setting<bool> Enabled;

			  [Description("Connector type. You should always set this to the connector type you want")]
			  public readonly Setting<ConnectorType> Type;

			  // Note: Be careful about adding things here that does not apply to all connectors,
			  //       consider future options like non-tcp transports, making `address` a bad choice
			  //       as a setting that applies to every connector, for instance.

			  public readonly GroupSettingSupport Group;

			  // Note: We no longer use the typeDefault parameter because it made for confusing behaviour;
			  // connectors with unspecified would override settings of other, unrelated connectors.
			  // However, we cannot remove the parameter at this
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public Connector(String key, @SuppressWarnings("UnusedParameters") String typeDefault)
			  public Connector( string key, string typeDefault )
			  {
					Group = new GroupSettingSupport( typeof( Connector ), key );
					Enabled = Group.scope( setting( "enabled", BOOLEAN, FALSE ) );
					Type = Group.scope( setting( "type", optionsObeyCase( typeof( ConnectorType ) ), NO_DEFAULT ) );
			  }

			  public enum ConnectorType
			  {
					Bolt,
					Http
			  }

			  public virtual string Key()
			  {
					return Group.groupKey;
			  }
		 }

		 /// <summary>
		 /// DEPRECATED: Use <seealso cref="org.Neo4Net.kernel.configuration.BoltConnector"/> instead. This will be removed in 4.0.
		 /// </summary>
		 [Obsolete, Description("Configuration options for Bolt connectors. " + "\"(bolt-connector-key)\" is a placeholder for a unique name for the connector, for instance " + "\"bolt-public\" or some other name that describes what the connector is for.")]
		 public class BoltConnector : Connector
		 {
			  [Description("Encryption level to require this connector to use")]
			  public readonly Setting<EncryptionLevel> EncryptionLevel;

			  [Description("Address the connector should bind to. " + "This setting is deprecated and will be replaced by `+listen_address+`")]
			  public readonly Setting<ListenSocketAddress> Address;

			  [Description("Address the connector should bind to")]
			  public readonly Setting<ListenSocketAddress> ListenAddress;

			  [Description("Advertised address for this connector")]
			  public readonly Setting<AdvertisedSocketAddress> AdvertisedAddress;

			  // Used by config doc generator
			  public BoltConnector() : this("(bolt-connector-key)")
			  {
			  }

			  public BoltConnector( string key ) : base( key, null )
			  {
					EncryptionLevel = Group.scope( setting( "tls_level", optionsObeyCase( typeof( EncryptionLevel ) ), EncryptionLevel.Optional.name() ) );
					Setting<ListenSocketAddress> legacyAddressSetting = listenAddress( "address", 7687 );
					Setting<ListenSocketAddress> listenAddressSetting = legacyFallback( legacyAddressSetting, listenAddress( "listen_address", 7687 ) );

					this.Address = Group.scope( legacyAddressSetting );
					this.ListenAddress = Group.scope( listenAddressSetting );
					this.AdvertisedAddress = Group.scope( advertisedAddress( "advertised_address", listenAddressSetting ) );
			  }

			  public enum EncryptionLevel
			  {
					Required,
					Optional,
					Disabled
			  }
		 }
	}

}