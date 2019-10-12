using System.Collections.Generic;
using System.Diagnostics;
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
namespace Org.Neo4j.Server
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Neo4jRule = Org.Neo4j.Harness.junit.Neo4jRule;
	using BoltConnector = Org.Neo4j.Kernel.configuration.BoltConnector;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using LegacySslPolicyConfig = Org.Neo4j.Kernel.configuration.ssl.LegacySslPolicyConfig;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using ServerSettings = Org.Neo4j.Server.configuration.ServerSettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.ServerTestUtils.createTempDir;

	public class BoltQueryLoggingIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.harness.junit.Neo4jRule neo4j;
		 public readonly Neo4jRule Neo4j;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public BoltQueryLoggingIT() throws java.io.IOException
		 public BoltQueryLoggingIT()
		 {
			  string tmpDir = createTempDir().AbsolutePath;
			  this.Neo4j = ( new Neo4jRule() ).withConfig(ServerSettings.http_logging_enabled, "true").withConfig(LegacySslPolicyConfig.certificates_directory.name(), tmpDir).withConfig(GraphDatabaseSettings.auth_enabled, "false").withConfig(GraphDatabaseSettings.logs_directory, tmpDir).withConfig(GraphDatabaseSettings.log_queries, "true").withConfig((new BoltConnector("bolt")).type, "BOLT").withConfig((new BoltConnector("bolt")).enabled, "true").withConfig((new BoltConnector("bolt")).address, "localhost:0").withConfig((new BoltConnector("bolt")).encryption_level, "DISABLED").withConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogQueriesViaBolt() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogQueriesViaBolt()
		 {
			  // *** GIVEN ***

			  Socket socket = new Socket( "localhost", Neo4j.boltURI().Port );
			  DataInputStream dataIn = new DataInputStream( socket.InputStream );
			  DataOutputStream dataOut = new DataOutputStream( socket.OutputStream );

			  // Bolt handshake
			  send( dataOut, new sbyte[] { ( sbyte ) 0x60, ( sbyte ) 0x60, unchecked( ( sbyte ) 0xB0 ), ( sbyte ) 0x17, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } );
			  receive( dataIn, new sbyte[]{ 0, 0, 0, 1 } );

			  // This has been taken from: http://alpha.neohq.net/docs/server-manual/bolt-examples.html

			  // Send INIT "MyClient/1.0" { "scheme": "basic", "principal": "neo4j", "credentials": "secret"}
			  Send( dataOut, "00 40 B1 01  8C 4D 79 43  6C 69 65 6E  74 2F 31 2E\n" + "30 A3 86 73  63 68 65 6D  65 85 62 61  73 69 63 89\n" + "70 72 69 6E  63 69 70 61  6C 85 6E 65  6F 34 6A 8B\n" + "63 72 65 64  65 6E 74 69  61 6C 73 86  73 65 63 72\n" + "65 74 00 00" );
			  // Receive SUCCESS {}
			  ReceiveSuccess( dataIn );

			  // *** WHEN ***

			  for ( int i = 0; i < 5; i++ )
			  {
					// Send RUN "RETURN 1 AS num" {}
					Send( dataOut, "00 13 b2 10  8f 52 45 54  55 52 4e 20  31 20 41 53 20 6e 75 6d  a0 00 00" );
					// Receive SUCCESS { "fields": ["num"], "result_available_after": X }
					//non-deterministic so just ignore it here
					ReceiveSuccess( dataIn );

					//receive( dataIn, "00 0f b1 70  a1 86 66 69  65 6c 64 73  91 83 6e 75 6d 00 00" );

					// Send PULL_ALL
					Send( dataOut, "00 02 B0 3F  00 00" );
					// Receive RECORD[1]
					Receive( dataIn, "00 04 b1 71  91 01 00 00" );
					// Receive SUCCESS { "type": "r", "result_consumed_after": Y }
					//non-deterministic so just ignore it here
					ReceiveSuccess( dataIn );
			  }

			  // *** THEN ***

			  Path queriesLog = Neo4j.Config.get( GraphDatabaseSettings.log_queries_filename ).toPath();
			  IList<string> lines = Files.readAllLines( queriesLog );
			  assertThat( lines, hasSize( 5 ) );
			  foreach ( string line in lines )
			  {
					assertThat( line, containsString( "INFO" ) );
					assertThat( line, containsString( "bolt-session" ) );
					assertThat( line, containsString( "MyClient/1.0" ) );
					assertThat( line, containsString( "client/127.0.0.1:" ) );
					assertThat( line, containsString( "server/127.0.0.1:" + Neo4j.boltURI().Port ) );
					assertThat( line, containsString( " - RETURN 1 AS num - {}" ) );
			  }

			  // *** CLEAN UP ***

			  // Send RESET
			  Send( dataOut, "00 02 b0 0f 00 00" );
			  // Receive SUCCESS {}
			  Receive( dataIn, "00 03 b1 70  a0 00 00" );

			  socket.close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void send(java.io.DataOutputStream dataOut, String toSend) throws java.io.IOException
		 private static void Send( DataOutputStream dataOut, string toSend )
		 {
			  send( dataOut, HexBytes( toSend ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void send(java.io.DataOutputStream dataOut, byte[] bytesToSend) throws java.io.IOException
		 private static void Send( DataOutputStream dataOut, sbyte[] bytesToSend )
		 {
			  dataOut.write( bytesToSend );
			  dataOut.flush();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void receiveSuccess(java.io.DataInputStream dataIn) throws java.io.IOException
		 private void ReceiveSuccess( DataInputStream dataIn )
		 {
			  short bytes = dataIn.readShort();
			  assertThat( dataIn.readUnsignedByte(), equalTo(0xB1) );
			  assertThat( dataIn.readUnsignedByte(), equalTo(0x70) );
			  dataIn.skipBytes( bytes );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void receive(java.io.DataInputStream dataIn, String expected) throws java.io.IOException
		 private static void Receive( DataInputStream dataIn, string expected )
		 {
			  receive( dataIn, HexBytes( expected ) );
		 }
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void receive(java.io.DataInputStream dataIn, byte[] expectedBytes) throws java.io.IOException
		 private static void Receive( DataInputStream dataIn, sbyte[] expectedBytes )
		 {
			  sbyte[] actualBytes = Read( dataIn, expectedBytes.Length );
			  assertThat( actualBytes, equalTo( expectedBytes ) );
		 }

		 private static sbyte[] HexBytes( string input )
		 {
			  string[] pieces = input.Trim().Split("\\s+", true);
			  sbyte[] result = new sbyte[pieces.Length];
			  for ( int i = 0; i < pieces.Length; i++ )
			  {
					result[i] = HexByte( pieces[i] );
			  }
			  return result;
		 }

		 private static sbyte HexByte( string s )
		 {
			  int hi = Character.digit( s[0], 16 ) << 4;
			  int lo = Character.digit( s[1], 16 );
			  return ( sbyte )( hi | lo );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static byte[] read(java.io.DataInputStream dataIn, int howMany) throws java.io.IOException
		 private static sbyte[] Read( DataInputStream dataIn, int howMany )
		 {
			  Debug.Assert( howMany > 0 );

			  sbyte[] buffer = new sbyte[howMany];
			  int offset = 0;
			  while ( offset < howMany )
			  {
					int read = dataIn.read( buffer, offset, howMany - offset );
					if ( read == 0 )
					{
						 Thread.yield();
					}
					offset += read;
			  }
			  return buffer;
		 }
	}

}