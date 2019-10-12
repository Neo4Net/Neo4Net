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
namespace Org.Neo4j.Commandline.dbms
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using RealOutsideWorld = Org.Neo4j.Commandline.admin.RealOutsideWorld;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Args = Org.Neo4j.Helpers.Args;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using SuppressOutputExtension = Org.Neo4j.Test.extension.SuppressOutputExtension;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({TestDirectoryExtension.class, SuppressOutputExtension.class}) class CsvImporterTest
	internal class CsvImporterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDir;
		 private TestDirectory _testDir;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writesReportToSpecifiedReportFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void WritesReportToSpecifiedReportFile()
		 {
			  File dbDir = _testDir.directory( "db" );
			  File logDir = _testDir.directory( "logs" );
			  File reportLocation = _testDir.file( "the_report" );

			  File inputFile = _testDir.file( "foobar.csv" );
			  IList<string> lines = Collections.singletonList( "foo\\tbar\\tbaz" );
			  Files.write( inputFile.toPath(), lines, Charset.defaultCharset() );

			  using ( RealOutsideWorld outsideWorld = new RealOutsideWorld( System.out, System.err, new MemoryStream( new sbyte[0] ) ) )
			  {
					Config config = Config.builder().withSettings(AdditionalConfig()).withSetting(GraphDatabaseSettings.database_path, dbDir.AbsolutePath).withSetting(GraphDatabaseSettings.logs_directory, logDir.AbsolutePath).build();

					CsvImporter csvImporter = new CsvImporter( Args.parse( string.Format( "--report-file={0}", reportLocation.AbsolutePath ), string.Format( "--nodes={0}", inputFile.AbsolutePath ), "--delimiter=TAB" ), config, outsideWorld );
					csvImporter.DoImport();
			  }

			  assertTrue( reportLocation.exists() );
		 }

		 private IDictionary<string, string> AdditionalConfig()
		 {
			  return stringMap( GraphDatabaseSettings.database_path.name(), DatabasePath, GraphDatabaseSettings.logs_directory.name(), LogsDirectory );
		 }

		 private string DatabasePath
		 {
			 get
			 {
				  return _testDir.databaseDir().AbsolutePath;
			 }
		 }

		 private string LogsDirectory
		 {
			 get
			 {
				  return _testDir.directory( "logs" ).AbsolutePath;
			 }
		 }
	}

}