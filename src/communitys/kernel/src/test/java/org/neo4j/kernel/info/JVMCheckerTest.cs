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
namespace Neo4Net.Kernel.info
{
	using Test = org.junit.Test;

	using BufferingLog = Neo4Net.Logging.BufferingLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.info.JvmChecker.INCOMPATIBLE_JVM_VERSION_WARNING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.info.JvmChecker.INCOMPATIBLE_JVM_WARNING;

	public class JVMCheckerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIssueWarningWhenUsingHotspotServerVmVersion7()
		 public virtual void ShouldIssueWarningWhenUsingHotspotServerVmVersion7()
		 {
			  BufferingLog bufferingLogger = new BufferingLog();

			  ( new JvmChecker( bufferingLogger, new CannedJvmMetadataRepository( "Java HotSpot(TM) 64-Bit Server VM", "1.7.0-b147" ) ) ).CheckJvmCompatibilityAndIssueWarning();

			  assertThat( bufferingLogger.ToString().Trim(), @is(INCOMPATIBLE_JVM_VERSION_WARNING) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotIssueWarningWhenUsingHotspotServerVmVersion8()
		 public virtual void ShouldNotIssueWarningWhenUsingHotspotServerVmVersion8()
		 {
			  BufferingLog bufferingLogger = new BufferingLog();

			  ( new JvmChecker( bufferingLogger, new CannedJvmMetadataRepository( "Java HotSpot(TM) 64-Bit Server VM", "1.8.0_45" ) ) ).CheckJvmCompatibilityAndIssueWarning();

			  assertTrue( bufferingLogger.ToString().Length == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotIssueWarningWhenUsingIbmJ9Vm()
		 public virtual void ShouldNotIssueWarningWhenUsingIbmJ9Vm()
		 {
			  BufferingLog bufferingLogger = new BufferingLog();

			  ( new JvmChecker( bufferingLogger, new CannedJvmMetadataRepository( "IBM J9 VM", "1.8" ) ) ).CheckJvmCompatibilityAndIssueWarning();

			  assertTrue( bufferingLogger.ToString().Length == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIssueWarningWhenUsingHotspotServerVmVersion7InThe32BitVersion()
		 public virtual void ShouldIssueWarningWhenUsingHotspotServerVmVersion7InThe32BitVersion()
		 {
			  BufferingLog bufferingLogger = new BufferingLog();

			  ( new JvmChecker( bufferingLogger, new CannedJvmMetadataRepository( "Java HotSpot(TM) Server VM", "1.7.0_25-b15" ) ) ).CheckJvmCompatibilityAndIssueWarning();

			  assertThat( bufferingLogger.ToString().Trim(), @is(INCOMPATIBLE_JVM_VERSION_WARNING) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIssueWarningWhenUsingOpenJDKServerVmVersion7()
		 public virtual void ShouldIssueWarningWhenUsingOpenJDKServerVmVersion7()
		 {
			  BufferingLog bufferingLogger = new BufferingLog();

			  ( new JvmChecker( bufferingLogger, new CannedJvmMetadataRepository( "OpenJDK 64-Bit Server VM", "1.7.0-b147" ) ) ).CheckJvmCompatibilityAndIssueWarning();

			  assertThat( bufferingLogger.ToString().Trim(), @is(INCOMPATIBLE_JVM_VERSION_WARNING) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIssueWarningWhenUsingOpenJDKClientVmVersion7()
		 public virtual void ShouldIssueWarningWhenUsingOpenJDKClientVmVersion7()
		 {
			  BufferingLog bufferingLogger = new BufferingLog();

			  ( new JvmChecker( bufferingLogger, new CannedJvmMetadataRepository( "OpenJDK Client VM", "1.7.0-b147" ) ) ).CheckJvmCompatibilityAndIssueWarning();

			  assertThat( bufferingLogger.ToString().Trim(), @is(INCOMPATIBLE_JVM_VERSION_WARNING) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIssueWarningWhenUsingUnsupportedJvm()
		 public virtual void ShouldIssueWarningWhenUsingUnsupportedJvm()
		 {
			  BufferingLog bufferingLogger = new BufferingLog();

			  ( new JvmChecker( bufferingLogger, new CannedJvmMetadataRepository( "MyOwnJDK 64-Bit Awesome VM", "1.7" ) ) ).CheckJvmCompatibilityAndIssueWarning();

			  assertThat( bufferingLogger.ToString().Trim(), @is(INCOMPATIBLE_JVM_WARNING) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIssueWarningWhenUsingUnsupportedJvmVersion()
		 public virtual void ShouldIssueWarningWhenUsingUnsupportedJvmVersion()
		 {
			  BufferingLog bufferingLogger = new BufferingLog();

			  ( new JvmChecker( bufferingLogger, new CannedJvmMetadataRepository( "Java HotSpot(TM) 64-Bit Server VM", "1.6.42_87" ) ) ).CheckJvmCompatibilityAndIssueWarning();

			  assertThat( bufferingLogger.ToString().Trim(), @is(INCOMPATIBLE_JVM_VERSION_WARNING) );
		 }
	}

}