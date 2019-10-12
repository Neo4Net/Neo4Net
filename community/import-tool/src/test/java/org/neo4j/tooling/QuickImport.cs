using System;

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
namespace Org.Neo4j.Tooling
{

	using CharSeeker = Org.Neo4j.Csv.Reader.CharSeeker;
	using CharSeekers = Org.Neo4j.Csv.Reader.CharSeekers;
	using Extractors = Org.Neo4j.Csv.Reader.Extractors;
	using Readables = Org.Neo4j.Csv.Reader.Readables;
	using Args = Org.Neo4j.Helpers.Args;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using RecordFormatSelector = Org.Neo4j.Kernel.impl.store.format.RecordFormatSelector;
	using Lifespan = Org.Neo4j.Kernel.Lifecycle.Lifespan;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using SimpleLogService = Org.Neo4j.Logging.@internal.SimpleLogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using BatchImporter = Org.Neo4j.@unsafe.Impl.Batchimport.BatchImporter;
	using BatchImporterFactory = Org.Neo4j.@unsafe.Impl.Batchimport.BatchImporterFactory;
	using ParallelBatchImporter = Org.Neo4j.@unsafe.Impl.Batchimport.ParallelBatchImporter;
	using Collector = Org.Neo4j.@unsafe.Impl.Batchimport.input.Collector;
	using DataGeneratorInput = Org.Neo4j.@unsafe.Impl.Batchimport.input.DataGeneratorInput;
	using Groups = Org.Neo4j.@unsafe.Impl.Batchimport.input.Groups;
	using Input = Org.Neo4j.@unsafe.Impl.Batchimport.input.Input;
	using Configuration = Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.Configuration;
	using DataFactories = Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.DataFactories;
	using Header = Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.Header;
	using IdType = Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.IdType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.dense_node_threshold;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.AdditionalInitialIds.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.ImportLogic.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.ExecutionMonitors.defaultVisible;

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
			  IdType idType = IdType.valueOf( args.Get( "id-type", IdType.INTEGER.name() ) );

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
			  long pageCacheMemory = args.GetNumber( "pagecache-memory", Org.Neo4j.@unsafe.Impl.Batchimport.Configuration_Fields.MaxPageCacheMemory ).longValue();
			  Org.Neo4j.@unsafe.Impl.Batchimport.Configuration importConfig = new ConfigurationAnonymousInnerClass( args, highIo, pageCacheMemory );

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
//ORIGINAL LINE: final org.neo4j.scheduler.JobScheduler jobScheduler = life.add(createScheduler());
						 JobScheduler jobScheduler = life.Add( createScheduler() );
						 consumer = BatchImporterFactory.withHighestPriority().instantiate(DatabaseLayout.of(dir), fileSystem, null, importConfig, new SimpleLogService(logging, logging), defaultVisible(jobScheduler), EMPTY, dbConfig, RecordFormatSelector.selectForConfig(dbConfig, logging), NO_MONITOR, jobScheduler);
						 ImportTool.PrintOverview( dir, Collections.emptyList(), Collections.emptyList(), importConfig, System.out );
					}
					consumer.DoImport( input );
			  }
		 }

		 private class ConfigurationAnonymousInnerClass : Org.Neo4j.@unsafe.Impl.Batchimport.Configuration
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

		 private class Configuration_OverriddenAnonymousInnerClass : Org.Neo4j.Csv.Reader.Configuration_Overridden
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