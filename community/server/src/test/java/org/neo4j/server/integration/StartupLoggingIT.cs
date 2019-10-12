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
namespace Org.Neo4j.Server.integration
{
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using BoltConnector = Org.Neo4j.Kernel.configuration.BoltConnector;
	using HttpConnector = Org.Neo4j.Kernel.configuration.HttpConnector;
	using Encryption = Org.Neo4j.Kernel.configuration.HttpConnector.Encryption;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using LegacySslPolicyConfig = Org.Neo4j.Kernel.configuration.ssl.LegacySslPolicyConfig;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using ExclusiveServerTestBase = Org.Neo4j.Test.server.ExclusiveServerTestBase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.transport.integration.Neo4jWithSocket.DEFAULT_CONNECTOR_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.AbstractNeoServer.NEO4J_IS_STARTING_MESSAGE;

	public class StartupLoggingIT : ExclusiveServerTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public new SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogHelpfulStartupMessages()
		 public virtual void ShouldLogHelpfulStartupMessages()
		 {
			  CommunityBootstrapper boot = new CommunityBootstrapper();
			  IDictionary<string, string> propertyPairs = PropertyPairs;

			  CommunityBootstrapper.start( TestDir.directory(), (new File("nonexistent-file.conf")), propertyPairs );
			  URI uri = boot.Server.baseUri();
			  boot.Stop();

			  IList<string> captured = SuppressOutput.OutputVoice.lines();
			  assertThat( captured, ContainsAtLeastTheseLines( Warn( "Config file \\[nonexistent-file.conf\\] does not exist." ), Info( NEO4J_IS_STARTING_MESSAGE ), Info( "Starting..." ), Info( "Started." ), Info( "Remote interface available at " + uri.ToString() ), Info("Stopping..."), Info("Stopped.") ) );
		 }

		 private IDictionary<string, string> PropertyPairs
		 {
			 get
			 {
				  IDictionary<string, string> properties = new Dictionary<string, string>();
   
				  properties[GraphDatabaseSettings.data_directory.name()] = TestDir.databaseDir().ToString();
				  properties[GraphDatabaseSettings.logs_directory.name()] = TestDir.databaseDir().ToString();
				  properties[LegacySslPolicyConfig.certificates_directory.name()] = TestDir.databaseDir().ToString();
				  properties[GraphDatabaseSettings.allow_upgrade.name()] = Settings.TRUE;
   
				  HttpConnector http = new HttpConnector( "http", HttpConnector.Encryption.NONE );
				  properties[http.Type.name()] = "HTTP";
				  properties[http.ListenAddress.name()] = "localhost:0";
				  properties[http.Enabled.name()] = Settings.TRUE;
   
				  HttpConnector https = new HttpConnector( "https", HttpConnector.Encryption.TLS );
				  properties[https.Type.name()] = "HTTP";
				  properties[https.ListenAddress.name()] = "localhost:0";
				  properties[https.Enabled.name()] = Settings.TRUE;
   
				  BoltConnector bolt = new BoltConnector( DEFAULT_CONNECTOR_KEY );
				  properties[bolt.Type.name()] = "BOLT";
				  properties[bolt.Enabled.name()] = "true";
				  properties[bolt.ListenAddress.name()] = "localhost:0";
   
				  properties[GraphDatabaseSettings.database_path.name()] = TestDir.absolutePath().AbsolutePath;
				  return properties;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private static org.hamcrest.Matcher<java.util.List<String>> containsAtLeastTheseLines(final org.hamcrest.Matcher<String>... expectedLinePatterns)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private static Matcher<IList<string>> ContainsAtLeastTheseLines( params Matcher<string>[] expectedLinePatterns )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass( expectedLinePatterns );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<IList<string>>
		 {
			 private Matcher<string>[] _expectedLinePatterns;

			 public TypeSafeMatcherAnonymousInnerClass( Matcher<string>[] expectedLinePatterns )
			 {
				 this._expectedLinePatterns = expectedLinePatterns;
			 }

			 protected internal override bool matchesSafely( IList<string> lines )
			 {
				  if ( _expectedLinePatterns.Length > lines.Count )
				  {
						return false;
				  }

				  for ( int i = 0, e = 0; i < lines.Count; i++ )
				  {
						string line = lines[i];
						while ( !_expectedLinePatterns[e].matches( line ) )
						{
							 if ( ++i >= lines.Count )
							 {
								  return false;
							 }
							 line = lines[i];
						}
						e++;

				  }
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendList( "", "\n", "", asList( _expectedLinePatterns ) );
			 }
		 }

		 public static Matcher<string> Info( string messagePattern )
		 {
			  return Line( "INFO", messagePattern );
		 }

		 public static Matcher<string> Warn( string messagePattern )
		 {
			  return Line( "WARN", messagePattern );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.hamcrest.Matcher<String> line(final String level, final String messagePattern)
		 public static Matcher<string> Line( string level, string messagePattern )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass2( level, messagePattern );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass2 : TypeSafeMatcher<string>
		 {
			 private string _level;
			 private string _messagePattern;

			 public TypeSafeMatcherAnonymousInnerClass2( string level, string messagePattern )
			 {
				 this._level = level;
				 this._messagePattern = messagePattern;
			 }

			 protected internal override bool matchesSafely( string line )
			 {
				  return line.matches( ".*" + _level + "\\s+" + _messagePattern );
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( _level ).appendText( " " ).appendText( _messagePattern );
			 }
		 }
	}

}