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
namespace Org.Neo4j.@unsafe.Impl.Batchimport
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using CharReadable = Org.Neo4j.Csv.Reader.CharReadable;
	using DataAfterQuoteException = Org.Neo4j.Csv.Reader.DataAfterQuoteException;
	using Readables = Org.Neo4j.Csv.Reader.Readables;
	using NullOutputStream = Org.Neo4j.Io.NullOutputStream;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using StandardV3_0 = Org.Neo4j.Kernel.impl.store.format.standard.StandardV3_0;
	using NullLogService = Org.Neo4j.Logging.@internal.NullLogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Org.Neo4j.Scheduler.ThreadPoolJobScheduler;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using BadCollector = Org.Neo4j.@unsafe.Impl.Batchimport.input.BadCollector;
	using Input = Org.Neo4j.@unsafe.Impl.Batchimport.input.Input;
	using InputException = Org.Neo4j.@unsafe.Impl.Batchimport.input.InputException;
	using CsvInput = Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.CsvInput;
	using DataFactory = Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.DataFactory;
	using IdType = Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.IdType;
	using ExecutionMonitors = Org.Neo4j.@unsafe.Impl.Batchimport.staging.ExecutionMonitors;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.ImportLogic.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.InputEntityDecorators.NO_DECORATOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.csv.Configuration.COMMAS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.csv.DataFactories.data;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.csv.DataFactories.datas;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.csv.DataFactories.defaultFormatNodeFileHeader;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.csv.DataFactories.defaultFormatRelationshipFileHeader;

	public class ImportPanicIT
	{
		private bool InstanceFieldsInitialized = false;

		public ImportPanicIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Rules = RuleChain.outerRule( _random ).around( _directory );
		}

		 private const int BUFFER_SIZE = 1000;

		 private readonly FileSystemAbstraction _fs = new DefaultFileSystemAbstraction();
		 private readonly TestDirectory _directory = TestDirectory.testDirectory();
		 private readonly RandomRule _random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain rules = org.junit.rules.RuleChain.outerRule(random).around(directory);
		 public RuleChain Rules;

		 /// <summary>
		 /// There was this problem where some steps and in particular parallel CSV input parsing that
		 /// paniced would hang the import entirely.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExitAndThrowExceptionOnPanic() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExitAndThrowExceptionOnPanic()
		 {
			  try
			  {
					  using ( JobScheduler jobScheduler = new ThreadPoolJobScheduler() )
					  {
						BatchImporter importer = new ParallelBatchImporter( _directory.databaseLayout(), _fs, null, Configuration.DEFAULT, NullLogService.Instance, ExecutionMonitors.invisible(), AdditionalInitialIds.EMPTY, Config.defaults(), StandardV3_0.RECORD_FORMATS, NO_MONITOR, jobScheduler );
						IEnumerable<DataFactory> nodeData = datas( data( NO_DECORATOR, FileAsCharReadable( NodeCsvFileWithBrokenEntries() ) ) );
						Input brokenCsvInput = new CsvInput( nodeData, defaultFormatNodeFileHeader(), datas(), defaultFormatRelationshipFileHeader(), IdType.ACTUAL, CsvConfigurationWithLowBufferSize(), new BadCollector(NullOutputStream.NULL_OUTPUT_STREAM, 0, 0), CsvInput.NO_MONITOR );
						importer.DoImport( brokenCsvInput );
						fail( "Should have failed properly" );
					  }
			  }
			  catch ( InputException e )
			  {
					// THEN
					assertTrue( e.InnerException is DataAfterQuoteException );
					// and we managed to shut down properly
			  }
		 }

		 private static Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.Configuration CsvConfigurationWithLowBufferSize()
		 {
			  return new Configuration_OverriddenAnonymousInnerClass( COMMAS );
		 }

		 private class Configuration_OverriddenAnonymousInnerClass : Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.Configuration_Overridden
		 {
			 public Configuration_OverriddenAnonymousInnerClass( UnknownType commas ) : base( commas )
			 {
			 }

			 public override int bufferSize()
			 {
				  return BUFFER_SIZE;
			 }
		 }

		 private static System.Func<CharReadable> FileAsCharReadable( File file )
		 {
			  return () =>
			  {
				try
				{
					 return Readables.files( StandardCharsets.UTF_8, file );
				}
				catch ( IOException e )
				{
					 throw new UncheckedIOException( e );
				}
			  };
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File nodeCsvFileWithBrokenEntries() throws java.io.IOException
		 private File NodeCsvFileWithBrokenEntries()
		 {
			  File file = _directory.file( "broken-node-data.csv" );
			  using ( PrintWriter writer = new PrintWriter( _fs.openAsWriter( file, StandardCharsets.UTF_8, false ) ) )
			  {
					writer.println( ":ID,name" );
					int numberOfLines = BUFFER_SIZE * 10;
					int brokenLine = _random.Next( numberOfLines );
					for ( int i = 0; i < numberOfLines; i++ )
					{
						 if ( i == brokenLine )
						 {
							  writer.println( i + ",\"broken\"line" );
						 }
						 else
						 {
							  writer.println( i + ",name" + i );
						 }
					}
			  }
			  return file;
		 }
	}

}