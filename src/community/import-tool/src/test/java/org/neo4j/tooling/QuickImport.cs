using System;

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
namespace Neo4Net.Tooling
{

	using CharSeeker = Neo4Net.Csv.Reader.CharSeeker;
	using CharSeekers = Neo4Net.Csv.Reader.CharSeekers;
	using Extractors = Neo4Net.Csv.Reader.Extractors;
	using Readables = Neo4Net.Csv.Reader.Readables;
	using Args = Neo4Net.Helpers.Args;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using RecordFormatSelector = Neo4Net.Kernel.impl.store.format.RecordFormatSelector;
	using Lifespan = Neo4Net.Kernel.Lifecycle.Lifespan;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using SimpleLogService = Neo4Net.Logging.Internal.SimpleLogService;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using BatchImporter = Neo4Net.@unsafe.Impl.Batchimport.BatchImporter;
	using BatchImporterFactory = Neo4Net.@unsafe.Impl.Batchimport.BatchImporterFactory;
	using ParallelBatchImporter = Neo4Net.@unsafe.Impl.Batchimport.ParallelBatchImporter;
	using Collector = Neo4Net.@unsafe.Impl.Batchimport.input.Collector;
	using DataGeneratorInput = Neo4Net.@unsafe.Impl.Batchimport.input.DataGeneratorInput;
	using Groups = Neo4Net.@unsafe.Impl.Batchimport.input.Groups;
	using Input = Neo4Net.@unsafe.Impl.Batchimport.input.Input;
	using Configuration = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration;
	using DataFactories = Neo4Net.@unsafe.Impl.Batchimport.input.csv.DataFactories;
	using Header = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Header;
	using IdType = Neo4Net.@unsafe.Impl.Batchimport.input.csv.IdType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.dense_node_threshold;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.scheduler.JobSchedulerFactory.createScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.AdditionalInitialIds.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.ImportLogic.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.staging.ExecutionMonitors.defaultVisible;

	/// <summary>
	/// Uses all available shortcuts to as quickly as possible import as much data as possible. Usage of this
	/// utility is most likely just testing behavior of some components in the face of various dataset sizes,
	/// even quite big ones. Uses the import tool, or rather directly the <seealso cref="ParallelBatchImporter"/>.
	/// 
	/// Quick comes from gaming terminology where you sometimes just want to play a quick game, without
	/// any settings or hazzle, just play.
	/// 
	/// Uses <seealso cref="DataGeneratorInput"/> as random data <seealso cref="Input"/>.
	/// 
	/// For the time being the node/relationship data can't be controlled via command-line arguments,
	/// only through changing the code. The <seealso cref="DataGeneratorInput"/> accepts two <seealso cref="Header headers"/>
	/// describing which sort of data it should generate.
	/// </summary>
	public class QuickImport
	{
		 private QuickImport()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] arguments) throws java.io.IOException
		 public static void Main( string[] arguments )
		 {
			  Args args = Args.parse( arguments );
			  long nodeCount = Settings.parseLongWithUnit( args.Get( "nodes", null ) );
			  long relationshipCount = Settings.parseLongWithUnit( args.Get( "relationships", null ) );
			  int labelCount = args.GetNumber( "labels", 4 ).intValue();
			  int relationshipTypeCount = args.GetNumber( "relationship-types", 4 ).intValue();
			  File dir = new File( args.Get( ImportTool.Options.StoreDir.key() ) );
			  long randomSeed = args.GetNumber( "random-seed", currentTimeMillis() ).longValue();
			  Configuration config = Configuration.COMMAS;

			  Extractors extractors = new Extractors( config.ArrayDelimiter() );
			  IdType idType = IdType.ValueOf( args.Get( "id-type", IdType.INTEGER.name() ) );

			  Groups groups = new Groups();
			  Header nodeHeader = ParseNodeHeader( args, idType, extractors, groups );
			  Header relationshipHeader = ParseRelationshipHeader( args, idType, extractors, groups );

			  Config dbConfig;
			  string dbConfigFileName = args.Get( ImportTool.Options.DatabaseConfig.key(), null );
			  if ( !string.ReferenceEquals( dbConfigFileName, null ) )
			  {
					dbConfig = ( new Config.Builder() ).withFile(new File(dbConfigFileName)).build();
			  }
			  else
			  {
					dbConfig = Config.defaults();
			  }

			  bool highIo = args.GetBoolean( ImportTool.Options.HighIo.key() );

			  LogProvider logging = NullLogProvider.Instance;
			  long pageCacheMemory = args.GetNumber( "pagecache-memory", Neo4Net.@unsafe.Impl.Batchimport.Configuration_Fields.MaxPageCacheMemory ).longValue();
			  Neo4Net.@unsafe.Impl.Batchimport.Configuration importConfig = new ConfigurationAnonymousInnerClass( args, highIo, pageCacheMemory );

			  float factorBadNodeData = args.GetNumber( "factor-bad-node-data", 0 ).floatValue();
			  float factorBadRelationshipData = args.GetNumber( "factor-bad-relationship-data", 0 ).floatValue();

			  Input input = new DataGeneratorInput( nodeCount, relationshipCount, idType, Collector.EMPTY, randomSeed, 0, nodeHeader, relationshipHeader, labelCount, relationshipTypeCount, factorBadNodeData, factorBadRelationshipData );

			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction(), Lifespan life = new Lifespan() )
			  {
					BatchImporter consumer;
					if ( args.GetBoolean( "to-csv" ) )
					{
						 consumer = new CsvOutput( dir, nodeHeader, relationshipHeader, config );
					}
					else
					{
						 Console.WriteLine( "Seed " + randomSeed );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.scheduler.JobScheduler jobScheduler = life.add(createScheduler());
						 IJobScheduler jobScheduler = life.Add( createScheduler() );
						 consumer = BatchImporterFactory.withHighestPriority().instantiate(DatabaseLayout.of(dir), fileSystem, null, importConfig, new SimpleLogService(logging, logging), defaultVisible(jobScheduler), EMPTY, dbConfig, RecordFormatSelector.selectForConfig(dbConfig, logging), NO_MONITOR, jobScheduler);
						 ImportTool.PrintOverview( dir, Collections.emptyList(), Collections.emptyList(), importConfig, System.out );
					}
					consumer.DoImport( input );
			  }
		 }

		 private class ConfigurationAnonymousInnerClass : Neo4Net.@unsafe.Impl.Batchimport.Configuration
		 {
			 private Args _args;
			 private bool _highIo;
			 private long _pageCacheMemory;

			 public ConfigurationAnonymousInnerClass( Args args, bool highIo, long pageCacheMemory )
			 {
				 this._args = args;
				 this._highIo = highIo;
				 this._pageCacheMemory = pageCacheMemory;
			 }

			 public int maxNumberOfProcessors()
			 {
				  return _args.getNumber( ImportTool.Options.Processors.key(), DEFAULT.maxNumberOfProcessors() ).intValue();
			 }

			 public int denseNodeThreshold()
			 {
				  return _args.getNumber( dense_node_threshold.name(), DEFAULT.denseNodeThreshold() ).intValue();
			 }

			 public bool highIO()
			 {
				  return _highIo;
			 }

			 public long pageCacheMemory()
			 {
				  return _pageCacheMemory;
			 }

			 public long maxMemoryUsage()
			 {
				  string custom = _args.get( ImportTool.Options.MaxMemory.key(), (string) ImportTool.Options.MaxMemory.defaultValue() );
				  return !string.ReferenceEquals( custom, null ) ? ImportTool.ParseMaxMemory( custom ).Value : DEFAULT.maxMemoryUsage();
			 }
		 }

		 private static Header ParseNodeHeader( Args args, IdType idType, Extractors extractors, Groups groups )
		 {
			  string definition = args.Get( "node-header", null );
			  if ( string.ReferenceEquals( definition, null ) )
			  {
					return DataGeneratorInput.bareboneNodeHeader( idType, extractors );
			  }

			  Configuration config = Configuration.COMMAS;
			  return DataFactories.defaultFormatNodeFileHeader().create(Seeker(definition, config), config, idType, groups);
		 }

		 private static Header ParseRelationshipHeader( Args args, IdType idType, Extractors extractors, Groups groups )
		 {
			  string definition = args.Get( "relationship-header", null );
			  if ( string.ReferenceEquals( definition, null ) )
			  {
					return DataGeneratorInput.bareboneRelationshipHeader( idType, extractors );
			  }

			  Configuration config = Configuration.COMMAS;
			  return DataFactories.defaultFormatRelationshipFileHeader().create(Seeker(definition, config), config, idType, groups);
		 }

		 private static CharSeeker Seeker( string definition, Configuration config )
		 {
			  return CharSeekers.charSeeker(Readables.wrap(definition), new Configuration_OverriddenAnonymousInnerClass(config)
			 , false);
		 }

		 private class Configuration_OverriddenAnonymousInnerClass : Neo4Net.Csv.Reader.Configuration_Overridden
		 {
			 public Configuration_OverriddenAnonymousInnerClass( Configuration config ) : base( config )
			 {
			 }

			 public override int bufferSize()
			 {
				  return 10_000;
			 }
		 }
	}

}