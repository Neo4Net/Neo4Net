﻿using System.Threading;

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
namespace Org.Neo4j.Server.security.enterprise.log
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Level = Org.Neo4j.Logging.Level;
	using LogTimeZone = Org.Neo4j.Logging.LogTimeZone;
	using SecuritySettings = Org.Neo4j.Server.security.enterprise.configuration.SecuritySettings;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.array;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	public class SecurityLogTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.EphemeralFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public EphemeralFileSystemRule FileSystemRule = new EphemeralFileSystemRule();

		 private Config _config = Config.defaults( stringMap( SecuritySettings.store_security_log_rotation_threshold.name(), "5", SecuritySettings.store_security_log_rotation_delay.name(), "1ms" ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRotateLog() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRotateLog()
		 {
			  SecurityLog securityLog = new SecurityLog( _config, FileSystemRule.get(), ThreadStart.run );
			  securityLog.Info( "line 1" );
			  securityLog.Info( "line 2" );

			  FileSystemAbstraction fs = FileSystemRule.get();

			  File activeLogFile = _config.get( SecuritySettings.security_log_filename );
			  assertThat( fs.FileExists( activeLogFile ), equalTo( true ) );
			  assertThat( fs.FileExists( Archive( 1 ) ), equalTo( true ) );
			  assertThat( fs.FileExists( Archive( 2 ) ), equalTo( false ) );

			  string[] activeLines = ReadLogFile( fs, activeLogFile );
			  assertThat( activeLines, array( containsString( "line 2" ) ) );

			  string[] archiveLines = ReadLogFile( fs, Archive( 1 ) );
			  assertThat( archiveLines, array( containsString( "line 1" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void logUseSystemTimeZoneIfConfigured() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LogUseSystemTimeZoneIfConfigured()
		 {
			  TimeZone defaultTimeZone = TimeZone.Default;
			  try
			  {
					CheckLogTimeZone( 4, "+0400" );
					CheckLogTimeZone( -8, "-0800" );
			  }
			  finally
			  {
					TimeZone.Default = defaultTimeZone;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkLogTimeZone(int hoursShift, String timeZoneSuffix) throws Exception
		 private void CheckLogTimeZone( int hoursShift, string timeZoneSuffix )
		 {
			  TimeZone.Default = TimeZone.getTimeZone( ZoneOffset.ofHours( hoursShift ) );
			  Config timeZoneConfig = Config.defaults( GraphDatabaseSettings.db_timezone, LogTimeZone.SYSTEM.name() );
			  SecurityLog securityLog = new SecurityLog( timeZoneConfig, FileSystemRule.get(), ThreadStart.run );
			  securityLog.Info( "line 1" );

			  FileSystemAbstraction fs = FileSystemRule.get();
			  File activeLogFile = timeZoneConfig.Get( SecuritySettings.security_log_filename );
			  string[] activeLines = ReadLogFile( fs, activeLogFile );
			  assertThat( activeLines, array( containsString( timeZoneSuffix ) ) );
			  FileSystemRule.clear();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHonorLogLevel() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHonorLogLevel()
		 {
			  WriteAllLevelsAndShutdown( WithLogLevel( Level.DEBUG ), "debug" );
			  WriteAllLevelsAndShutdown( WithLogLevel( Level.INFO ), "info" );
			  WriteAllLevelsAndShutdown( WithLogLevel( Level.WARN ), "warn" );
			  WriteAllLevelsAndShutdown( WithLogLevel( Level.ERROR ), "error" );

			  FileSystemAbstraction fs = FileSystemRule.get();
			  File activeLogFile = _config.get( SecuritySettings.security_log_filename );
			  string[] activeLines = ReadLogFile( fs, activeLogFile );
			  assertThat( activeLines, array( containsString( "debug: debug line" ), containsString( "debug: info line" ), containsString( "debug: warn line" ), containsString( "debug: error line" ), containsString( "info: info line" ), containsString( "info: warn line" ), containsString( "info: error line" ), containsString( "warn: warn line" ), containsString( "warn: error line" ), containsString( "error: error line" ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeAllLevelsAndShutdown(SecurityLog securityLog, String tag) throws Throwable
		 private void WriteAllLevelsAndShutdown( SecurityLog securityLog, string tag )
		 {
			  securityLog.Debug( format( "%s: debug line", tag ) );
			  securityLog.Info( format( "%s: info line", tag ) );
			  securityLog.Warn( format( "%s: warn line", tag ) );
			  securityLog.Error( format( "%s: error line", tag ) );
			  securityLog.Shutdown();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private SecurityLog withLogLevel(org.neo4j.logging.Level debug) throws java.io.IOException
		 private SecurityLog WithLogLevel( Level debug )
		 {
			  return new SecurityLog( Config.defaults( SecuritySettings.security_log_level, debug.name() ), FileSystemRule.get(), ThreadStart.run );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String[] readLogFile(org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File activeLogFile) throws java.io.IOException
		 private string[] ReadLogFile( FileSystemAbstraction fs, File activeLogFile )
		 {
			  Scanner scan = new Scanner( fs.OpenAsInputStream( activeLogFile ) );
			  scan.useDelimiter( "\\Z" );
			  string allLines = scan.next();
			  scan.close();
			  return allLines.Split( "\\n", true );
		 }

		 private File Archive( int archiveNumber )
		 {
			  return new File( format( "%s.%d", _config.get( SecuritySettings.security_log_filename ), archiveNumber ) );
		 }
	}

}