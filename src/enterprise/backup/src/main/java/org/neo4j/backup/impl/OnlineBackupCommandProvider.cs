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
namespace Neo4Net.backup.impl
{

	using AdminCommand = Neo4Net.CommandLine.Admin.AdminCommand;
	using AdminCommandSection = Neo4Net.CommandLine.Admin.AdminCommandSection;
	using OutsideWorld = Neo4Net.CommandLine.Admin.OutsideWorld;
	using Arguments = Neo4Net.CommandLine.Args.Arguments;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using Level = Neo4Net.Logging.Level;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.backup.impl.BackupSupportingClassesFactoryProvider.getProvidersByPriority;

	public class OnlineBackupCommandProvider : Neo4Net.CommandLine.Admin.AdminCommand_Provider
	{
		 public OnlineBackupCommandProvider() : base("backup")
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public Neo4Net.commandline.arguments.Arguments allArguments()
		 public override Arguments AllArguments()
		 {
			  return OnlineBackupContextFactory.Arguments();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public String description()
		 public override string Description()
		 {
			  return format( "Perform an online backup from a running Neo4Net enterprise server. Neo4Net's backup service must " + "have been configured on the server beforehand.%n" + "%n" + "All consistency checks except 'cc-graph' can be quite expensive so it may be useful to turn them off" + " for very large databases. Increasing the heap size can also be a good idea." + " See 'Neo4Net-admin help' for details.%n" + "%n" + "For more information see: https://Neo4Net.com/docs/operations-manual/current/backup/" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public String summary()
		 public override string Summary()
		 {
			  return "Perform an online backup from a running Neo4Net enterprise server.";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public Neo4Net.commandline.admin.AdminCommandSection commandSection()
		 public override AdminCommandSection CommandSection()
		 {
			  return OnlineBackupCommandSection.instance();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public Neo4Net.commandline.admin.AdminCommand create(java.nio.file.Path homeDir, java.nio.file.Path configDir, Neo4Net.commandline.admin.OutsideWorld outsideWorld)
		 public override AdminCommand Create( Path homeDir, Path configDir, OutsideWorld outsideWorld )
		 {
			  bool debug = System.getenv().get("Neo4Net_DEBUG") != null;
			  LogProvider logProvider = FormattedLogProvider.withDefaultLogLevel( debug ? Level.DEBUG : Level.NONE ).toOutputStream( outsideWorld.OutStream() );
			  Monitors monitors = new Monitors();

			  OnlineBackupContextFactory contextBuilder = new OnlineBackupContextFactory( homeDir, configDir );
			  BackupModule backupModule = new BackupModule( outsideWorld, logProvider, monitors );

			  BackupSupportingClassesFactoryProvider classesFactoryProvider = ProvidersByPriority.findFirst().orElseThrow(NoProviderException());
			  BackupSupportingClassesFactory supportingClassesFactory = classesFactoryProvider.GetFactory( backupModule );
			  BackupStrategyCoordinatorFactory coordinatorFactory = new BackupStrategyCoordinatorFactory( backupModule );

			  return new OnlineBackupCommand( outsideWorld, contextBuilder, supportingClassesFactory, coordinatorFactory );
		 }

		 private static System.Func<System.InvalidOperationException> NoProviderException()
		 {
			  return () => new System.InvalidOperationException("Unable to find a suitable backup supporting classes provider in the classpath");
		 }
	}

}