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
namespace Neo4Net.backup
{
	using Test = org.junit.Test;
	using Enclosed = org.junit.experimental.runners.Enclosed;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using BackupClient = Neo4Net.backup.impl.BackupClient;
	using BackupProtocolService = Neo4Net.backup.impl.BackupProtocolService;
	using BackupServer = Neo4Net.backup.impl.BackupServer;
	using ConsistencyCheck = Neo4Net.backup.impl.ConsistencyCheck;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Enclosed.class) public class BackupToolUrisTest
	public class BackupToolUrisTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public static class ValidUriTests extends UriTests
		 public class ValidUriTests : UriTests
		 {
			  public ValidUriTests( string host, int? port ) : base( host, port )
			  {
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters public static java.util.List<Object[]> data()
			  public static IList<object[]> Data()
			  {
					return new IList<object[]> { Uri( "127.0.0.1" ), Uri( "127.0.0.1", 6362 ), Uri( "localhost" ), Uri( "localhost", 1234 ), Uri( "192.127.10.12" ), Uri( "192.127.10.12", 20 ), Uri( "1.1.1.1" ), Uri( "1.1.1.1", 1 ), Uri( "Neo4Net.company.com" ), Uri( "Neo4Net.company.com", 65200 ), Uri( "single://localhost", 7090 ), Uri( "test-site.with-long.name.com", 55555 ), Uri( "single://Neo4Net-backup.server", 6468 ), Uri( "single://apple.com" ), Uri( "single://255.255.255.0" ), Uri( "single://255.1.255.1", 88 ), Uri( "single://127.0.0.1" ), Uri( "single://localhost" ), Uri( "single://127.0.0.1", 6264 ), Uri( "ha://test.server" ), Uri( "ha://test.server", 1212 ) };
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExecuteBackupWithValidUri() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldExecuteBackupWithValidUri()
			  {
					// Given
					string[] args = new string[]{ "-from", UriConflict, "-to", "/var/backup/graph" };

					// When
					NewBackupTool().run(args);

					// Then
					verify( BackupProtocolService ).doIncrementalBackupOrFallbackToFull( eq( Host ), eq( Port ), eq( DatabaseLayout.of( Paths.get( "/var/backup/graph" ).toFile() ) ), eq(ConsistencyCheck.FULL), any(typeof(Config)), eq(BackupClient.BIG_READ_TIMEOUT), eq(false) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public static class InvalidUriTests extends UriTests
		 public class InvalidUriTests : UriTests
		 {
			  public InvalidUriTests( string host, int? port ) : base( host, port )
			  {
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters public static java.util.List<Object[]> data()
			  public static IList<object[]> Data()
			  {
					return new IList<object[]> { Uri( "foo://127.0.1.1" ), Uri( "single://localhost,ha://not-localhost" ), Uri( "single://127.0.0.1:6361,single://127.0.0.1:6362" ), Uri( "300.400.500.600" ), Uri( "host-name_with*wrong$chars.com" ), Uri( "dir://my" ), Uri( "dir://my", 10 ), Uri( "foo://127.0.1.1" ), Uri( "foo://127.0.1.1", 6567 ), Uri( "cat://localhost" ), Uri( "cat://localhost", 4444 ), Uri( "notHA://instance1:,instance2:,instance3", 5454 ) };
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowForInvalidUri()
			  public virtual void ShouldThrowForInvalidUri()
			  {
					// Given
					string[] args = new string[]{ "-from", UriConflict, "-to", "/var/backup/graph" };

					try
					{
						 // When
						 NewBackupTool().run(args);
						 fail( "Should exit abnormally for '" + UriConflict + "'" );
					}
					catch ( BackupTool.ToolFailureException e )
					{
						 // Then
						 assertThat( e.Message, equalTo( BackupTool.WrongFromAddressSyntax ) );
					}

					verifyZeroInteractions( BackupProtocolService, SystemOut );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public static class IPv6UriTests extends UriTests
		 public class IPv6UriTests : UriTests
		 {
			  public IPv6UriTests( string host, int? port ) : base( host, port )
			  {
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters public static java.util.List<Object[]> data()
			  public static IList<object[]> Data()
			  {
					return new IList<object[]> { Uri( "[2001:cdba:0000:0000:0000:0000:3257:9652]" ), Uri( "[2001:cdba:0000:0000:0000:0000:3257:9652]", 5656 ), Uri( "[2001:cdba:0:0:0:0:3257:9652]" ), Uri( "[2001:cdba:0:0:0:0:3257:9652]", 9091 ), Uri( "[2001:cdba::3257:9652]" ), Uri( "[2001:cdba::3257:9652]", 20 ), Uri( "[2001:db8::1]", 9991 ), Uri( "[2001:db8::1]", 1990 ), Uri( "[::1]" ), Uri( "[::1]", 8989 ), Uri( "[fe80::]" ), Uri( "[fe80::]", 1209 ), Uri( "[::ffff:0:0]" ), Uri( "[::ffff:0:0]", 4545 ), Uri( "[ff02::1:1]" ), Uri( "[ff02::1:1]", 6767 ), Uri( "[2002::]" ), Uri( "[2002::]", 3040 ) };
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExecuteBackupWithValidUri() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
			  public virtual void ShouldExecuteBackupWithValidUri()
			  {
					// Given
					string[] args = new string[]{ "-host", Host, "-port", Port.ToString(), "-to", "/var/backup/graph" };

					// When
					NewBackupTool().run(args);

					// Then
					verify( BackupProtocolService ).doIncrementalBackupOrFallbackToFull( eq( Host ), eq( Port ), eq( DatabaseLayout.of( Paths.get( "/var/backup/graph" ).toFile() ) ), eq(ConsistencyCheck.FULL), any(typeof(Config)), eq(BackupClient.BIG_READ_TIMEOUT), eq(false) );
			  }
		 }

		 private abstract class UriTests
		 {
			  internal readonly string Host;
			  internal readonly int? Port;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string UriConflict;

			  internal readonly BackupProtocolService BackupProtocolService;
			  internal readonly PrintStream SystemOut;

			  internal UriTests( string host, int? port )
			  {
					this.UriConflict = ( port == null ) ? host : host + ":" + port;
					this.Host = host.Replace( "ha://", "" ).Replace( "single://", "" );
					this.Port = ( port == null ) ? BackupServer.DEFAULT_PORT : port;

					this.BackupProtocolService = mock( typeof( BackupProtocolService ) );
					this.SystemOut = mock( typeof( PrintStream ) );
			  }

			  internal static object[] Uri( string host )
			  {
					return Uri( host, null );
			  }

			  internal static object[] Uri( string host, int? port )
			  {
					return new object[]{ host, port };
			  }

			  internal virtual BackupTool NewBackupTool()
			  {
					return spy( new BackupTool( BackupProtocolService, SystemOut ) );
			  }
		 }
	}

}