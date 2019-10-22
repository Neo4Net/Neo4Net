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
namespace Neo4Net.Server
{
	using Test = org.junit.Test;


	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.ArrayUtil.array;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.stringMap;

	public class ServerCommandLineArgsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickUpSpecifiedConfigFile()
		 public virtual void ShouldPickUpSpecifiedConfigFile()
		 {
			  File dir = ( new File( "/some-dir" ) ).AbsoluteFile;
			  Optional<File> expectedFile = new File( dir, Config.DEFAULT_CONFIG_FILE_NAME );
			  assertEquals( expectedFile, Parse( "--config-dir", dir.ToString() ).configFile() );
			  assertEquals( expectedFile, Parse( "--config-dir=" + dir ).configFile() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResolveConfigFileRelativeToWorkingDirectory()
		 public virtual void ShouldResolveConfigFileRelativeToWorkingDirectory()
		 {
			  Optional<File> expectedFile = new File( "some-dir", Config.DEFAULT_CONFIG_FILE_NAME );
			  assertEquals( expectedFile, Parse( "--config-dir", "some-dir" ).configFile() );
			  assertEquals( expectedFile, Parse( "--config-dir=some-dir" ).configFile() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNullIfConfigDirIsNotSpecified()
		 public virtual void ShouldReturnNullIfConfigDirIsNotSpecified()
		 {
			  assertEquals( null, Parse().configFile() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickUpSpecifiedHomeDir()
		 public virtual void ShouldPickUpSpecifiedHomeDir()
		 {
			  File homeDir = ( new File( "/some/absolute/homedir" ) ).AbsoluteFile;

			  assertEquals( homeDir, Parse( "--home-dir", homeDir.ToString() ).homeDir() );
			  assertEquals( homeDir, Parse( "--home-dir=" + homeDir.ToString() ).homeDir() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNullIfHomeDirIsNotSpecified()
		 public virtual void ShouldReturnNullIfHomeDirIsNotSpecified()
		 {
			  assertNull( Parse().homeDir() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickUpOverriddenConfigurationParameters()
		 public virtual void ShouldPickUpOverriddenConfigurationParameters()
		 {
			  // GIVEN
			  string[] args = array( "-c", "myoption=myvalue" );

			  // WHEN
			  ServerCommandLineArgs parsed = ServerCommandLineArgs.Parse( args );

			  // THEN
			  assertEquals( stringMap( "myoption", "myvalue" ), parsed.ConfigOverrides() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickUpOverriddenBooleanConfigurationParameters()
		 public virtual void ShouldPickUpOverriddenBooleanConfigurationParameters()
		 {
			  // GIVEN
			  string[] args = array( "-c", "myoptionenabled" );

			  // WHEN
			  ServerCommandLineArgs parsed = ServerCommandLineArgs.Parse( args );

			  // THEN
			  assertEquals( stringMap( "myoptionenabled", true.ToString() ), parsed.ConfigOverrides() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickUpMultipleOverriddenConfigurationParameters()
		 public virtual void ShouldPickUpMultipleOverriddenConfigurationParameters()
		 {
			  // GIVEN
			  string[] args = array( "-c", "my_first_option=first", "-c", "myoptionenabled", "-c", "my_second_option=second" );

			  // WHEN
			  ServerCommandLineArgs parsed = ServerCommandLineArgs.Parse( args );

			  // THEN
			  assertEquals( stringMap( "my_first_option", "first", "myoptionenabled", true.ToString(), "my_second_option", "second" ), parsed.ConfigOverrides() );
		 }

		 private ServerCommandLineArgs Parse( params string[] args )
		 {
			  return ServerCommandLineArgs.Parse( args );
		 }
	}

}