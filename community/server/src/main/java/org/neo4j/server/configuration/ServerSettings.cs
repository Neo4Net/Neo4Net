using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Server.configuration
{

	using Description = Org.Neo4j.Configuration.Description;
	using DocumentedDefaultValue = Org.Neo4j.Configuration.DocumentedDefaultValue;
	using Internal = Org.Neo4j.Configuration.Internal;
	using LoadableConfig = Org.Neo4j.Configuration.LoadableConfig;
	using Org.Neo4j.Graphdb.config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using JettyThreadCalculator = Org.Neo4j.Server.web.JettyThreadCalculator;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.logs_directory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.BYTES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.DURATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.INTEGER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.NORMALIZED_RELATIVE_URI;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.NO_DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.PATH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.STRING_LIST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.TRUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.buildSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.derivedSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.pathSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.range;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.setting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.ssl.LegacySslPolicyConfig.LEGACY_POLICY_NAME;

	[Description("Settings used by the server configuration")]
	public class ServerSettings : LoadableConfig
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Maximum request header size") @Internal public static final org.neo4j.graphdb.config.Setting<int> maximum_request_header_size = setting("unsupported.dbms.max_http_request_header_size", INTEGER, "20480");
		 [Description("Maximum request header size")]
		 public static readonly Setting<int> MaximumRequestHeaderSize = setting( "unsupported.dbms.max_http_request_header_size", INTEGER, "20480" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Maximum response header size") @Internal public static final org.neo4j.graphdb.config.Setting<int> maximum_response_header_size = setting("unsupported.dbms.max_http_response_header_size", INTEGER, "20480");
		 [Description("Maximum response header size")]
		 public static readonly Setting<int> MaximumResponseHeaderSize = setting( "unsupported.dbms.max_http_response_header_size", INTEGER, "20480" );

		 [Description("Comma-separated list of custom security rules for Neo4j to use.")]
		 public static readonly Setting<IList<string>> SecurityRules = setting( "dbms.security.http_authorization_classes", STRING_LIST, EMPTY );

		 [Description("Number of Neo4j worker threads. This setting is only valid for REST, and does not influence bolt-server. " + "It sets the amount of worker threads for the Jetty server used by neo4j-server. " + "This option can be tuned when you plan to execute multiple, concurrent REST requests, " + "with the aim of getting more throughput from the database. " + "Your OS might enforce a lower limit than the maximum value specified here."), DocumentedDefaultValue("Number of available processors, or 500 for machines which have more than 500 processors.")]
		 public static readonly Setting<int> WebserverMaxThreads = buildSetting( "dbms.threads.worker_count", INTEGER, "" + Math.Min( Runtime.Runtime.availableProcessors(), 500 ) ).constraint(range(1, JettyThreadCalculator.MAX_THREADS)).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("If execution time limiting is enabled in the database, this configures the maximum request execution time. " + "Please use dbms.transaction.timeout instead.") @Internal @Deprecated public static final org.neo4j.graphdb.config.Setting<java.time.Duration> webserver_limit_execution_time = setting("unsupported.dbms" + ".executiontime_limit.time", DURATION, NO_DEFAULT);
		 [Description("If execution time limiting is enabled in the database, this configures the maximum request execution time. " + "Please use dbms.transaction.timeout instead."), Obsolete]
		 public static readonly Setting<Duration> WebserverLimitExecutionTime = setting( "unsupported.dbms" + ".executiontime_limit.time", DURATION, NO_DEFAULT );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.neo4j.graphdb.config.Setting<java.util.List<String>> console_module_engines = setting("unsupported.dbms.console_module.engines", STRING_LIST, "SHELL");
		 public static readonly Setting<IList<string>> ConsoleModuleEngines = setting( "unsupported.dbms.console_module.engines", STRING_LIST, "SHELL" );

		 [Description("Comma-separated list of <classname>=<mount point> for unmanaged extensions.")]
		 public static final Setting<IList<ThirdPartyJaxRsPackage>> third_party_packages = setting("dbms.unmanaged_extension_classes", new FuncAnonymousInnerClass()
				  , EMPTY);

		 [Description("Value of the Access-Control-Allow-Origin header sent over any HTTP or HTTPS " + "connector. This defaults to '*', which allows broadest compatibility. Note " + "that any URI provided here limits HTTP/HTTPS access to that URI only.")]
		 public static final Setting<string> HttpAccessControlAllowOrigin = setting( "dbms.security.http_access_control_allow_origin", STRING, "*" );

		 [Description("Enable HTTP request logging.")]
		 public static final Setting<bool> HttpLoggingEnabled = setting( "dbms.logs.http.enabled", BOOLEAN, FALSE );

		 [Description("Path to HTTP request log.")]
		 public static final Setting<File> HttpLogPath = derivedSetting( "dbms.logs.http.path", logs_directory, logs => new File( logs, "http.log" ), PATH );

		 [Description("Number of HTTP logs to keep.")]
		 public static final Setting<int> HttpLoggingRotationKeepNumber = setting( "dbms.logs.http.rotation.keep_number", INTEGER, "5" );

		 [Description("Size of each HTTP log that is kept.")]
		 public static final Setting<long> HttpLoggingRotationSize = buildSetting( "dbms.logs.http.rotation.size", BYTES, "20m" ).constraint( range( 0L, long.MaxValue ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") @Description("Enable GC Logging") public static final org.neo4j.graphdb.config.Setting<bool> gc_logging_enabled = setting("dbms.logs.gc.enabled", BOOLEAN, FALSE);
		 [Description("Enable GC Logging")]
		 public static final Setting<bool> GcLoggingEnabled = setting( "dbms.logs.gc.enabled", BOOLEAN, FALSE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") @Description("GC Logging Options") public static final org.neo4j.graphdb.config.Setting<String> gc_logging_options = setting("dbms.logs.gc.options", STRING, "" + "-XX:+PrintGCDetails -XX:+PrintGCDateStamps -XX:+PrintGCApplicationStoppedTime " + "-XX:+PrintPromotionFailure -XX:+PrintTenuringDistribution");
		 [Description("GC Logging Options")]
		 public static final Setting<string> GcLoggingOptions = setting( "dbms.logs.gc.options", STRING, "" + "-XX:+PrintGCDetails -XX:+PrintGCDateStamps -XX:+PrintGCApplicationStoppedTime " + "-XX:+PrintPromotionFailure -XX:+PrintTenuringDistribution" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") @Description("Number of GC logs to keep.") public static final org.neo4j.graphdb.config.Setting<int> gc_logging_rotation_keep_number = setting("dbms.logs.gc.rotation.keep_number", INTEGER, "5");
		 [Description("Number of GC logs to keep.")]
		 public static final Setting<int> GcLoggingRotationKeepNumber = setting( "dbms.logs.gc.rotation.keep_number", INTEGER, "5" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") @Description("Size of each GC log that is kept.") public static final org.neo4j.graphdb.config.Setting<long> gc_logging_rotation_size = buildSetting("dbms.logs.gc.rotation.size", BYTES, "20m").constraint(range(0L, Long.MAX_VALUE)).build();
		 [Description("Size of each GC log that is kept.")]
		 public static final Setting<long> GcLoggingRotationSize = buildSetting( "dbms.logs.gc.rotation.size", BYTES, "20m" ).constraint( range( 0L, long.MaxValue ) ).build();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") @Description("Path of the run directory. This directory holds Neo4j's runtime state, such as a pidfile when it " + "is running in the background. The pidfile is created when starting neo4j and removed when stopping it." + " It may be placed on an in-memory filesystem such as tmpfs.") public static final org.neo4j.graphdb.config.Setting<java.io.File> run_directory = pathSetting("dbms.directories.run", "run");
		 [Description("Path of the run directory. This directory holds Neo4j's runtime state, such as a pidfile when it " + "is running in the background. The pidfile is created when starting neo4j and removed when stopping it." + " It may be placed on an in-memory filesystem such as tmpfs.")]
		 public static final Setting<File> RunDirectory = pathSetting( "dbms.directories.run", "run" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") @Description("Path of the lib directory") public static final org.neo4j.graphdb.config.Setting<java.io.File> lib_directory = pathSetting("dbms.directories.lib", "lib");
		 [Description("Path of the lib directory")]
		 public static final Setting<File> LibDirectory = pathSetting( "dbms.directories.lib", "lib" );

		 [Description("Timeout for idle transactions in the REST endpoint.")]
		 public static final Setting<Duration> TransactionIdleTimeout = setting( "dbms.rest.transaction.idle_timeout", DURATION, "60s" );

		 [Description("Value of the HTTP Strict-Transport-Security (HSTS) response header. " + "This header tells browsers that a webpage should only be accessed using HTTPS instead of HTTP. It is attached to every HTTPS response. " + "Setting is not set by default so 'Strict-Transport-Security' header is not sent. " + "Value is expected to contain directives like 'max-age', 'includeSubDomains' and 'preload'.")]
		 public static final Setting<string> HttpStrictTransportSecurity = setting( "dbms.security.http_strict_transport_security", STRING, NO_DEFAULT );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal @Description("Publicly discoverable bolt:// URI to use for Neo4j Drivers wanting to access the data in this " + "particular database instance. Normally this is the same as the advertised address configured for the " + "connector, but this allows manually overriding that default.") @DocumentedDefaultValue("Defaults to a bolt://-schemed version of the advertised address " + "of the first found bolt connector.") public static final org.neo4j.graphdb.config.Setting<java.net.URI> bolt_discoverable_address = setting("unsupported.dbms.discoverable_bolt_address", org.neo4j.kernel.configuration.Settings.URI, "");
		 [Description("Publicly discoverable bolt:// URI to use for Neo4j Drivers wanting to access the data in this " + "particular database instance. Normally this is the same as the advertised address configured for the " + "connector, but this allows manually overriding that default."), DocumentedDefaultValue("Defaults to a bolt://-schemed version of the advertised address " + "of the first found bolt connector.")]
		 public static final Setting<URI> BoltDiscoverableAddress = setting( "unsupported.dbms.discoverable_bolt_address", Settings.URI, "" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") @Description("Commands to be run when Neo4j Browser successfully connects to this server. Separate multiple " + "commands with semi-colon.") public static final org.neo4j.graphdb.config.Setting<String> browser_postConnectCmd = setting("browser.post_connect_cmd", STRING, "");
		 [Description("Commands to be run when Neo4j Browser successfully connects to this server. Separate multiple " + "commands with semi-colon.")]
		 public static final Setting<string> BrowserPostConnectCmd = setting( "browser.post_connect_cmd", STRING, "" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") @Description("Whitelist of hosts for the Neo4j Browser to be allowed to fetch content from.") public static final org.neo4j.graphdb.config.Setting<String> browser_remoteContentHostnameWhitelist = setting("browser.remote_content_hostname_whitelist", STRING, "guides.neo4j.com,localhost");
		 [Description("Whitelist of hosts for the Neo4j Browser to be allowed to fetch content from.")]
		 public static final Setting<string> BrowserRemoteContentHostnameWhitelist = setting( "browser.remote_content_hostname_whitelist", STRING, "guides.neo4j.com,localhost" );

		 [Description("SSL policy name.")]
		 public static final Setting<string> SslPolicy = setting( "https.ssl_policy", STRING, LEGACY_POLICY_NAME );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.neo4j.graphdb.config.Setting<java.net.URI> rest_api_path = setting("unsupported.dbms.uris.rest", NORMALIZED_RELATIVE_URI, "/db/data");
		 public static final Setting<URI> RestApiPath = setting( "unsupported.dbms.uris.rest", NORMALIZED_RELATIVE_URI, "/db/data" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.neo4j.graphdb.config.Setting<java.net.URI> management_api_path = setting("unsupported.dbms.uris.management", NORMALIZED_RELATIVE_URI, "/db/manage");
		 public static final Setting<URI> ManagementApiPath = setting( "unsupported.dbms.uris.management", NORMALIZED_RELATIVE_URI, "/db/manage" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.neo4j.graphdb.config.Setting<java.net.URI> browser_path = setting("unsupported.dbms.uris.browser", org.neo4j.kernel.configuration.Settings.URI, "/browser/");
		 public static final Setting<URI> BrowserPath = setting( "unsupported.dbms.uris.browser", Settings.URI, "/browser/" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.neo4j.graphdb.config.Setting<bool> wadl_enabled = setting("unsupported.dbms.wadl_generation_enabled", BOOLEAN, FALSE);
		 public static final Setting<bool> WadlEnabled = setting( "unsupported.dbms.wadl_generation_enabled", BOOLEAN, FALSE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.neo4j.graphdb.config.Setting<bool> console_module_enabled = setting("unsupported.dbms.console_module.enabled", BOOLEAN, TRUE);
		 public static final Setting<bool> ConsoleModuleEnabled = setting( "unsupported.dbms.console_module.enabled", BOOLEAN, TRUE );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.neo4j.graphdb.config.Setting<bool> jmx_module_enabled = setting("unsupported.dbms.jmx_module.enabled", BOOLEAN, TRUE);
		 public static final Setting<bool> JmxModuleEnabled = setting( "unsupported.dbms.jmx_module.enabled", BOOLEAN, TRUE );
	}

}