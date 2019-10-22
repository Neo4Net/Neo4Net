using System.Collections.Generic;

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
namespace Neo4Net.CommandLine.dbms
{

	using IncorrectUsage = Neo4Net.CommandLine.Admin.IncorrectUsage;
	using OutsideWorld = Neo4Net.CommandLine.Admin.OutsideWorld;
	using WrappedBatchImporterConfigurationForNeo4NetAdmin = Neo4Net.CommandLine.dbms.config.WrappedBatchImporterConfigurationForNeo4NetAdmin;
	using WrappedCsvInputConfigurationForNeo4NetAdmin = Neo4Net.CommandLine.dbms.config.WrappedCsvInputConfigurationForNeo4NetAdmin;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Args = Neo4Net.Helpers.Args;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ImportTool = Neo4Net.Tooling.ImportTool;
	using Configuration = Neo4Net.@unsafe.Impl.Batchimport.Configuration;
	using BadCollector = Neo4Net.@unsafe.Impl.Batchimport.input.BadCollector;
	using Collector = Neo4Net.@unsafe.Impl.Batchimport.input.Collector;
	using CsvInput = Neo4Net.@unsafe.Impl.Batchimport.input.csv.CsvInput;
	using IdType = Neo4Net.@unsafe.Impl.Batchimport.input.csv.IdType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.util.Converters.withDefault;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.tooling.ImportTool.csvConfiguration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.tooling.ImportTool.extractInputFiles;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.tooling.ImportTool.importConfiguration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.tooling.ImportTool.nodeData;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.tooling.ImportTool.relationshipData;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.tooling.ImportTool.validateInputFiles;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.Collectors.badCollector;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.Collectors.collect;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.csv.DataFactories.defaultFormatNodeFileHeader;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.csv.DataFactories.defaultFormatRelationshipFileHeader;

	internal class CsvImporter : Importer
	{
		 private readonly ICollection<Args.Option<File[]>> _nodesFiles;
		 private readonly ICollection<Args.Option<File[]>> _relationshipsFiles;
		 private readonly IdType _idType;
		 private readonly Charset _inputEncoding;
		 private readonly Config _databaseConfig;
		 private readonly Args _args;
		 private readonly OutsideWorld _outsideWorld;
		 private readonly string _reportFileName;
		 private readonly bool _ignoreBadRelationships;
		 private readonly bool _ignoreDuplicateNodes;
		 private readonly bool _ignoreExtraColumns;
		 private readonly bool? _highIO;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: CsvImporter(org.Neo4Net.helpers.Args args, org.Neo4Net.kernel.configuration.Config databaseConfig, org.Neo4Net.commandline.admin.OutsideWorld outsideWorld) throws org.Neo4Net.commandline.admin.IncorrectUsage
		 internal CsvImporter( Args args, Config databaseConfig, OutsideWorld outsideWorld )
		 {
			  this._args = args;
			  this._outsideWorld = outsideWorld;
			  _nodesFiles = extractInputFiles( args, "nodes", outsideWorld.ErrorStream() );
			  _relationshipsFiles = extractInputFiles( args, "relationships", outsideWorld.ErrorStream() );
			  _reportFileName = args.InterpretOption( "report-file", withDefault( ImportCommand.DEFAULT_REPORT_FILE_NAME ), s => s );
			  _ignoreExtraColumns = args.GetBoolean( "ignore-extra-columns", false ).Value;
			  _ignoreDuplicateNodes = args.GetBoolean( "ignore-duplicate-nodes", false ).Value;
			  _ignoreBadRelationships = args.GetBoolean( "ignore-missing-nodes", false ).Value;
			  try
			  {
					validateInputFiles( _nodesFiles, _relationshipsFiles );
			  }
			  catch ( System.ArgumentException e )
			  {
					throw new IncorrectUsage( e.Message );
			  }

			  _idType = args.InterpretOption( "id-type", withDefault( IdType.STRING ), from => IdType.valueOf( from.ToUpper() ) );
			  _inputEncoding = Charset.forName( args.Get( "input-encoding", defaultCharset().name() ) );
			  _highIO = args.GetBoolean( "high-io", null, true ); // intentionally left as null if not specified
			  this._databaseConfig = databaseConfig;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void doImport() throws java.io.IOException
		 public override void DoImport()
		 {
			  FileSystemAbstraction fs = _outsideWorld.fileSystem();
			  File storeDir = _databaseConfig.get( GraphDatabaseSettings.database_path );
			  File reportFile = new File( _reportFileName );

			  Stream badOutput = new BufferedOutputStream( fs.OpenAsOutputStream( reportFile, false ) );
			  Collector badCollector = badCollector( badOutput, IgnoringSomething ? BadCollector.UNLIMITED_TOLERANCE : 0, collect( _ignoreBadRelationships, _ignoreDuplicateNodes, _ignoreExtraColumns ) );

			  Configuration configuration = new WrappedBatchImporterConfigurationForNeo4NetAdmin( importConfiguration( null, false, _databaseConfig, storeDir, _highIO ) );

			  // Extract the default time zone from the database configuration
			  ZoneId dbTimeZone = _databaseConfig.get( GraphDatabaseSettings.db_temporal_timezone );
			  System.Func<ZoneId> defaultTimeZone = () => dbTimeZone;

			  CsvInput input = new CsvInput( nodeData( _inputEncoding, _nodesFiles ), defaultFormatNodeFileHeader( defaultTimeZone ), relationshipData( _inputEncoding, _relationshipsFiles ), defaultFormatRelationshipFileHeader( defaultTimeZone ), _idType, new WrappedCsvInputConfigurationForNeo4NetAdmin( csvConfiguration( _args, false ) ), badCollector, new CsvInput.PrintingMonitor( _outsideWorld.outStream() ) );

			  ImportTool.doImport( _outsideWorld.errorStream(), _outsideWorld.errorStream(), _outsideWorld.inStream(), DatabaseLayout.of(storeDir), reportFile, fs, _nodesFiles, _relationshipsFiles, false, input, this._databaseConfig, badOutput, configuration, false );
		 }

		 private bool IgnoringSomething
		 {
			 get
			 {
				  return _ignoreBadRelationships || _ignoreDuplicateNodes || _ignoreExtraColumns;
			 }
		 }
	}

}