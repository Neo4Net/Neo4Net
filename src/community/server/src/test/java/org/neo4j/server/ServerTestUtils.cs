using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Neo4Net.Graphdb.config;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using LegacySslPolicyConfig = Neo4Net.Kernel.configuration.ssl.LegacySslPolicyConfig;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ServerTestUtils
	{
		 private ServerTestUtils()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.io.File createTempDir() throws java.io.IOException
		 public static File CreateTempDir()
		 {
			  return Files.createTempDirectory( "neo4j-test" ).toFile();
		 }

		 public static File SharedTestTemporaryFolder
		 {
			 get
			 {
				  try
				  {
						return CreateTempConfigFile().ParentFile;
				  }
				  catch ( Exception e )
				  {
						throw new Exception( e );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.io.File createTempConfigFile() throws java.io.IOException
		 public static File CreateTempConfigFile()
		 {
			  File file = File.createTempFile( "neo4j", "conf" );
			  file.delete();
			  return file;
		 }

		 public static string GetRelativePath( File folder, Setting<File> setting )
		 {
			  return folder.toPath().resolve(setting.DefaultValue).ToString();
		 }

		 public static IDictionary<string, string> DefaultRelativeProperties
		 {
			 get
			 {
				  File testFolder = SharedTestTemporaryFolder;
				  IDictionary<string, string> settings = new Dictionary<string, string>();
				  AddDefaultRelativeProperties( settings, testFolder );
				  return settings;
			 }
		 }

		 public static void AddDefaultRelativeProperties( IDictionary<string, string> properties, File temporaryFolder )
		 {
			  AddRelativeProperty( temporaryFolder, properties, GraphDatabaseSettings.data_directory );
			  AddRelativeProperty( temporaryFolder, properties, GraphDatabaseSettings.logs_directory );
			  AddRelativeProperty( temporaryFolder, properties, LegacySslPolicyConfig.certificates_directory );
			  properties[GraphDatabaseSettings.pagecache_memory.name()] = "8m";
		 }

		 private static void AddRelativeProperty( File temporaryFolder, IDictionary<string, string> properties, Setting<File> setting )
		 {
			  properties[setting.Name()] = GetRelativePath(temporaryFolder, setting);
		 }

		 public static void WriteConfigToFile( IDictionary<string, string> properties, File file )
		 {
			  Properties props = LoadProperties( file );
			  foreach ( KeyValuePair<string, string> entry in properties.SetOfKeyValuePairs() )
			  {
					props.setProperty( entry.Key, entry.Value );
			  }
			  StoreProperties( file, props );
		 }

		 public static string AsOneLine( IDictionary<string, string> properties )
		 {
			  StringBuilder builder = new StringBuilder();
			  foreach ( KeyValuePair<string, string> property in properties.SetOfKeyValuePairs() )
			  {
					builder.Append( builder.Length > 0 ? "," : "" );
					builder.Append( property.Key ).Append( "=" ).Append( property.Value );
			  }
			  return builder.ToString();
		 }

		 private static void StoreProperties( File file, Properties properties )
		 {
			  Stream @out = null;
			  try
			  {
					@out = new FileStream( file, FileMode.Create, FileAccess.Write );
					properties.store( @out, "" );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
			  finally
			  {
					SafeClose( @out );
			  }
		 }

		 private static Properties LoadProperties( File file )
		 {
			  Properties properties = new Properties();
			  if ( file.exists() )
			  {
					Stream @in = null;
					try
					{
						 @in = new FileStream( file, FileMode.Open, FileAccess.Read );
						 properties.load( @in );
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}
					finally
					{
						 SafeClose( @in );
					}
			  }
			  return properties;
		 }

		 private static void SafeClose( System.IDisposable closeable )
		 {
			  if ( closeable != null )
			  {
					try
					{
						 closeable.Dispose();
					}
					catch ( IOException e )
					{
						 Console.WriteLine( e.ToString() );
						 Console.Write( e.StackTrace );
					}
			  }
		 }

		 public static File CreateTempConfigFile( File parentDir )
		 {
			  File file = new File( parentDir, "test-" + ( new Random() ).Next() + ".properties" );
			  file.deleteOnExit();
			  return file;
		 }

		 public interface BlockWithCSVFileURL
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void execute(String url) throws Exception;
			  void Execute( string url );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void withCSVFile(int rowCount, BlockWithCSVFileURL block) throws Exception
		 public static void WithCSVFile( int rowCount, BlockWithCSVFileURL block )
		 {
			  File file = File.createTempFile( "file", ".csv", null );
			  try
			  {
					using ( PrintWriter writer = new PrintWriter( file ) )
					{
						 for ( int i = 0; i < rowCount; ++i )
						 {
							  writer.println( "1,2,3" );
						 }
					}

					string url = file.toURI().toURL().ToString().Replace("\\", "\\\\");
					block.Execute( url );
			  }
			  finally
			  {
					file.delete();
			  }
		 }

		 public static void VerifyConnector( GraphDatabaseService db, string name, bool enabled )
		 {
			  HostnamePort address = ConnectorAddress( db, name );
			  if ( enabled )
			  {
					assertNotNull( address );
					assertTrue( CanConnectToSocket( address.Host, address.Port ) );
			  }
			  else
			  {
					assertNull( address );
			  }
		 }

		 public static HostnamePort ConnectorAddress( GraphDatabaseService db, string name )
		 {
			  ConnectorPortRegister portRegister = ( ( GraphDatabaseAPI ) db ).DependencyResolver.resolveDependency( typeof( ConnectorPortRegister ) );
			  return portRegister.GetLocalAddress( name );
		 }

		 private static bool CanConnectToSocket( string host, int port )
		 {
			  try
			  {
					( new Socket( host, port ) ).close();
					return true;
			  }
			  catch ( Exception )
			  {
					return false;
			  }
		 }
	}

}