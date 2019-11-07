using System;
using System.Collections.Generic;
using System.Threading;

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

	using IllegalMultilineFieldException = Neo4Net.Csv.Reader.IllegalMultilineFieldException;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Args = Neo4Net.Helpers.Args;
	using Neo4Net.Helpers.Args;
	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Strings = Neo4Net.Helpers.Strings;
	using Neo4Net.Collections.Helpers;
	using IOUtils = Neo4Net.Io.IOUtils;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using OsBeanUtil = Neo4Net.Io.os.OsBeanUtil;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using RecordFormatSelector = Neo4Net.Kernel.impl.store.format.RecordFormatSelector;
	using Converters = Neo4Net.Kernel.impl.util.Converters;
	using Neo4Net.Kernel.impl.util;
	using Validators = Neo4Net.Kernel.impl.util.Validators;
	using Version = Neo4Net.Kernel.Internal.Version;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using StoreLogService = Neo4Net.Logging.Internal.StoreLogService;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using BatchImporter = Neo4Net.@unsafe.Impl.Batchimport.BatchImporter;
	using BatchImporterFactory = Neo4Net.@unsafe.Impl.Batchimport.BatchImporterFactory;
	using DuplicateInputIdException = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string.DuplicateInputIdException;
	using BadCollector = Neo4Net.@unsafe.Impl.Batchimport.input.BadCollector;
	using Collector = Neo4Net.@unsafe.Impl.Batchimport.input.Collector;
	using Input = Neo4Net.@unsafe.Impl.Batchimport.input.Input;
	using InputException = Neo4Net.@unsafe.Impl.Batchimport.input.InputException;
	using MissingRelationshipDataException = Neo4Net.@unsafe.Impl.Batchimport.input.MissingRelationshipDataException;
	using Configuration = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration;
	using CsvInput = Neo4Net.@unsafe.Impl.Batchimport.input.csv.CsvInput;
	using DataFactory = Neo4Net.@unsafe.Impl.Batchimport.input.csv.DataFactory;
	using Decorator = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Decorator;
	using IdType = Neo4Net.@unsafe.Impl.Batchimport.input.csv.IdType;
	using ExecutionMonitor = Neo4Net.@unsafe.Impl.Batchimport.staging.ExecutionMonitor;
	using ExecutionMonitors = Neo4Net.@unsafe.Impl.Batchimport.staging.ExecutionMonitors;
	using SpectrumExecutionMonitor = Neo4Net.@unsafe.Impl.Batchimport.staging.SpectrumExecutionMonitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.store_internal_log_path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Exceptions.throwIfUnchecked;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Format.bytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Strings.TAB;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.TextUtil.tokenizeStringWithQuotes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.ByteUnit.mebiBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.fs.FileUtils.readTextFile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.configuration.Settings.parseLongWithUnit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.scheduler.JobSchedulerFactory.createScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.PropertyType.EMPTY_BYTE_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.util.Converters.withDefault;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.AdditionalInitialIds.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.Configuration_Fields.BAD_FILE_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.Configuration.DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.Configuration_Fields.DEFAULT_MAX_MEMORY_PERCENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.Configuration.calculateMaxMemoryFromPercent;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.Configuration.canDetectFreeMemory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.input.Collectors.badCollector;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.input.Collectors.collect;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.input.Collectors.silentBadCollector;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.input.InputEntityDecorators.NO_DECORATOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.input.InputEntityDecorators.additiveLabels;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.input.InputEntityDecorators.defaultRelationshipType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.input.csv.Configuration.COMMAS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.input.csv.DataFactories.data;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.input.csv.DataFactories.defaultFormatNodeFileHeader;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.input.csv.DataFactories.defaultFormatRelationshipFileHeader;

	/// <summary>
	/// User-facing command line tool around a <seealso cref="BatchImporter"/>.
	/// </summary>
	public class ImportTool
	{
		 private static readonly string _inputFilesDescription = "Multiple files will be logically seen as one big file " +
					"from the perspective of the importer. " +
					"The first line must contain the header. " +
					"Multiple data sources like these can be specified in one import, " +
					"where each data source has its own header. " +
					"Note that file groups must be enclosed in quotation marks. " +
					"Each file can be a regular expression and will then include all matching files. " +
					"The file matching is done with number awareness such that e.g. files:" +
					"'File1Part_001.csv', 'File12Part_003' will be ordered in that order for a pattern like: 'File.*'";

		 private const string UNLIMITED = "true";

		 internal sealed class Options
		 {
			  public static readonly Options File = new Options( "File", InnerEnum.File, "f", null, "<file name>", "File containing all arguments, used as an alternative to supplying all arguments on the command line directly." + "Each argument can be on a separate line or multiple arguments per line separated by space." + "Arguments containing spaces needs to be quoted." + "Supplying other arguments in addition to this file argument is not supported." );
			  public static readonly Options StoreDir = new Options( "StoreDir", InnerEnum.StoreDir, "into", null, "<store-dir>", "Database directory to import into. " + "Must not contain existing database." );
			  public static readonly Options DbName = new Options( "DbName", InnerEnum.DbName, "database", null, "<database-name>", "Database name to import into. " + "Must not contain existing database.", true );
			  public static readonly Options NodeData = new Options( "NodeData", InnerEnum.NodeData, "nodes", null, "[:Label1:Label2] \"<file1>" + MULTI_FILE_DELIMITER + "<file2>" + MULTI_FILE_DELIMITER + "...\"", "Node CSV header and data. " + INPUT_FILES_DESCRIPTION, true, true );
			  public static readonly Options RelationshipData = new Options( "RelationshipData", InnerEnum.RelationshipData, "relationships", null, "[:RELATIONSHIP_TYPE] \"<file1>" + MULTI_FILE_DELIMITER + "<file2>" + MULTI_FILE_DELIMITER + "...\"", "Relationship CSV header and data. " + INPUT_FILES_DESCRIPTION, true, true );
			  public static readonly Options Delimiter = new Options( "Delimiter", InnerEnum.Delimiter, "delimiter", null, "<delimiter-character>", "Delimiter character, or 'TAB', between values in CSV data. The default option is `" + COMMAS.delimiter() + "`." );
			  public static readonly Options ArrayDelimiter = new Options( "ArrayDelimiter", InnerEnum.ArrayDelimiter, "array-delimiter", null, "<array-delimiter-character>", "Delimiter character, or 'TAB', between array elements within a value in CSV data. " + "The default option is `" + COMMAS.arrayDelimiter() + "`." );
			  public static readonly Options Quote = new Options( "Quote", InnerEnum.Quote, "quote", null, "<quotation-character>", "Character to treat as quotation character for values in CSV data. " + "The default option is `" + COMMAS.quotationCharacter() + "`. " + "Quotes inside quotes escaped like `\"\"\"Go away\"\", he said.\"` and " + "`\"\\\"Go away\\\", he said.\"` are supported. " + "If you have set \"`'`\" to be used as the quotation character, " + "you could write the previous example like this instead: " + "`'\"Go away\", he said.'`" );
			  public static readonly Options MultilineFields = new Options( "MultilineFields", InnerEnum.MultilineFields, "multiline-fields", Neo4Net.Csv.Reader.Configuration_Fields.Default.multilineFields(), "<true/false>", "Whether or not fields from input source can span multiple lines, i.e. contain newline characters." );

			  public static readonly Options TrimStrings = new Options( "TrimStrings", InnerEnum.TrimStrings, "trim-strings", Neo4Net.Csv.Reader.Configuration_Fields.Default.trimStrings(), "<true/false>", "Whether or not strings should be trimmed for whitespaces." );

			  public static readonly Options InputEncoding = new Options( "InputEncoding", InnerEnum.InputEncoding, "input-encoding", null, "<character set>", "Character set that input data is encoded in. Provided value must be one out of the available " + "character sets in the JVM, as provided by Charset#availableCharsets(). " + "If no input encoding is provided, the default character set of the JVM will be used.", true );
			  public static readonly Options IgnoreEmptyStrings = new Options( "IgnoreEmptyStrings", InnerEnum.IgnoreEmptyStrings, "ignore-empty-strings", Neo4Net.Csv.Reader.Configuration_Fields.Default.emptyQuotedStringsAsNull(), "<true/false>", "Whether or not empty string fields, i.e. \"\" from input source are ignored, i.e. treated as null." );
			  public static readonly Options IdType = new Options( "IdType", InnerEnum.IdType, "id-type", Neo4Net.@unsafe.Impl.Batchimport.input.csv.IdType.String, "<id-type>", "One out of " + java.util.Arrays.ToString( Neo4Net.@unsafe.Impl.Batchimport.input.csv.IdType.values() ) + " and specifies how ids in node/relationship " + "input files are treated.\n" + Neo4Net.@unsafe.Impl.Batchimport.input.csv.IdType.String + ": arbitrary strings for identifying nodes.\n" + Neo4Net.@unsafe.Impl.Batchimport.input.csv.IdType.Integer + ": arbitrary integer values for identifying nodes.\n" + Neo4Net.@unsafe.Impl.Batchimport.input.csv.IdType.Actual + ": (advanced) actual node ids. The default option is `" + Neo4Net.@unsafe.Impl.Batchimport.input.csv.IdType.String + "`.", true );
			  public static readonly Options Processors = new Options( "Processors", InnerEnum.Processors, "processors", null, "<max processor count>", "(advanced) Max number of processors used by the importer. Defaults to the number of " + "available processors reported by the JVM" + AvailableProcessorsHint() + ". There is a certain amount of minimum threads needed so for that reason there " + "is no lower bound for this value. For optimal performance this value shouldn't be " + "greater than the number of available processors." );
			  public static readonly Options Stacktrace = new Options( "Stacktrace", InnerEnum.Stacktrace, "stacktrace", false, "<true/false>", "Enable printing of error stack traces." );
			  public static readonly Options BadTolerance = new Options( "BadTolerance", InnerEnum.BadTolerance, "bad-tolerance", 1000, "<max number of bad entries, or " + UNLIMITED + " for unlimited>", "Number of bad entries before the import is considered failed. This tolerance threshold is " + "about relationships referring to missing nodes. Format errors in input data are " + "still treated as errors" );
			  public static readonly Options SkipBadEntriesLogging = new Options( "SkipBadEntriesLogging", InnerEnum.SkipBadEntriesLogging, "skip-bad-entries-logging", false, "<true/false>", "Whether or not to skip logging bad entries detected during import." );
			  public static readonly Options SkipBadRelationships = new Options( "SkipBadRelationships", InnerEnum.SkipBadRelationships, "skip-bad-relationships", true, "<true/false>", "Whether or not to skip importing relationships that refers to missing node ids, i.e. either " + "start or end node id/group referring to node that wasn't specified by the " + "node input data. " + "Skipped nodes will be logged" + ", containing at most number of entities specified by " + BAD_TOLERANCE.key() + ", unless " + "otherwise specified by " + SKIP_BAD_ENTRIES_LOGGING.key() + " option." );
			  public static readonly Options SkipDuplicateNodes = new Options( "SkipDuplicateNodes", InnerEnum.SkipDuplicateNodes, "skip-duplicate-nodes", false, "<true/false>", "Whether or not to skip importing nodes that have the same id/group. In the event of multiple " + "nodes within the same group having the same id, the first encountered will be imported " + "whereas consecutive such nodes will be skipped. " + "Skipped nodes will be logged" + ", containing at most number of entities specified by " + BAD_TOLERANCE.key() + ", unless " + "otherwise specified by " + SKIP_BAD_ENTRIES_LOGGING.key() + "option." );
			  public static readonly Options IgnoreExtraColumns = new Options( "IgnoreExtraColumns", InnerEnum.IgnoreExtraColumns, "ignore-extra-columns", false, "<true/false>", "Whether or not to ignore extra columns in the data not specified by the header. " + "Skipped columns will be logged, containing at most number of entities specified by " + BAD_TOLERANCE.key() + ", unless " + "otherwise specified by " + SKIP_BAD_ENTRIES_LOGGING.key() + "option." );
			  public static readonly Options DatabaseConfig = new Options( "DatabaseConfig", InnerEnum.DatabaseConfig, "db-config", null, "<path/to/" + Neo4Net.Kernel.configuration.Config.DEFAULT_CONFIG_FILE_NAME + ">", "(advanced) Option is deprecated and replaced by 'additional-config'. " );
			  public static readonly Options AdditionalConfig = new Options( "AdditionalConfig", InnerEnum.AdditionalConfig, "additional-config", null, "<path/to/" + Neo4Net.Kernel.configuration.Config.DEFAULT_CONFIG_FILE_NAME + ">", "(advanced) File specifying database-specific configuration. For more information consult " + "manual about available configuration options for a Neo4Net configuration file. " + "Only configuration affecting store at time of creation will be read. " + "Examples of supported config are:\n" + Neo4Net.GraphDb.factory.GraphDatabaseSettings.DenseNodeThreshold.name() + "\n" + Neo4Net.GraphDb.factory.GraphDatabaseSettings.StringBlockSize.name() + "\n" + Neo4Net.GraphDb.factory.GraphDatabaseSettings.ArrayBlockSize.name(), true );
			  public static readonly Options LegacyStyleQuoting = new Options( "LegacyStyleQuoting", InnerEnum.LegacyStyleQuoting, "legacy-style-quoting", Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration.DEFAULT_LEGACY_STYLE_QUOTING, "<true/false>", "Whether or not backslash-escaped quote e.g. \\\" is interpreted as inner quote." );
			  public static readonly Options ReadBufferSize = new Options( "ReadBufferSize", InnerEnum.ReadBufferSize, "read-buffer-size", Neo4Net.Csv.Reader.Configuration_Fields.Default.bufferSize(), "<bytes, e.g. 10k, 4M>", "Size of each buffer for reading input data. It has to at least be large enough to hold the " + "biggest single value in the input data." );
			  public static readonly Options MaxMemory = new Options( "MaxMemory", InnerEnum.MaxMemory, "max-memory", null, "<max memory that importer can use>", "(advanced) Maximum memory that importer can use for various data structures and caching " + "to improve performance. If left as unspecified (null) it is set to " + DEFAULT_MAX_MEMORY_PERCENT + "% of (free memory on machine - max JVM memory). " + "Values can be plain numbers, like 10000000 or e.g. 20G for 20 gigabyte, or even e.g. 70%." );
			  public static readonly Options CacheOnHeap = new Options( "CacheOnHeap", InnerEnum.CacheOnHeap, "cache-on-heap", DEFAULT.allowCacheAllocationOnHeap(), "Whether or not to allow allocating memory for the cache on heap", "(advanced) Whether or not to allow allocating memory for the cache on heap. " + "If 'false' then caches will still be allocated off-heap, but the additional free memory " + "inside the JVM will not be allocated for the caches. This to be able to have better control " + "over the heap memory" );
			  public static readonly Options HighIo = new Options( "HighIo", InnerEnum.HighIo, "high-io", null, "Assume a high-throughput storage subsystem", "(advanced) Ignore environment-based heuristics, and assume that the target storage subsystem can " + "support parallel IO with high throughput." );
			  public static readonly Options DetailedProgress = new Options( "DetailedProgress", InnerEnum.DetailedProgress, "detailed-progress", false, "true/false", "Use the old detailed 'spectrum' progress printing" );

			  private static readonly IList<Options> valueList = new List<Options>();

			  static Options()
			  {
				  valueList.Add( File );
				  valueList.Add( StoreDir );
				  valueList.Add( DbName );
				  valueList.Add( NodeData );
				  valueList.Add( RelationshipData );
				  valueList.Add( Delimiter );
				  valueList.Add( ArrayDelimiter );
				  valueList.Add( Quote );
				  valueList.Add( MultilineFields );
				  valueList.Add( TrimStrings );
				  valueList.Add( InputEncoding );
				  valueList.Add( IgnoreEmptyStrings );
				  valueList.Add( IdType );
				  valueList.Add( Processors );
				  valueList.Add( Stacktrace );
				  valueList.Add( BadTolerance );
				  valueList.Add( SkipBadEntriesLogging );
				  valueList.Add( SkipBadRelationships );
				  valueList.Add( SkipDuplicateNodes );
				  valueList.Add( IgnoreExtraColumns );
				  valueList.Add( DatabaseConfig );
				  valueList.Add( AdditionalConfig );
				  valueList.Add( LegacyStyleQuoting );
				  valueList.Add( ReadBufferSize );
				  valueList.Add( MaxMemory );
				  valueList.Add( CacheOnHeap );
				  valueList.Add( HighIo );
				  valueList.Add( DetailedProgress );
			  }

			  public enum InnerEnum
			  {
				  File,
				  StoreDir,
				  DbName,
				  NodeData,
				  RelationshipData,
				  Delimiter,
				  ArrayDelimiter,
				  Quote,
				  MultilineFields,
				  TrimStrings,
				  InputEncoding,
				  IgnoreEmptyStrings,
				  IdType,
				  Processors,
				  Stacktrace,
				  BadTolerance,
				  SkipBadEntriesLogging,
				  SkipBadRelationships,
				  SkipDuplicateNodes,
				  IgnoreExtraColumns,
				  DatabaseConfig,
				  AdditionalConfig,
				  LegacyStyleQuoting,
				  ReadBufferSize,
				  MaxMemory,
				  CacheOnHeap,
				  HighIo,
				  DetailedProgress
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Private readonly;
			  internal Private readonly;
			  internal Private readonly;
			  internal Private readonly;
			  internal Private readonly;
			  internal Private readonly;

			  internal Options( string name, InnerEnum innerEnum, string key, object defaultValue, string usage, string description ) : this( key, defaultValue, usage, description, false, false )
			  {

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal Options( string name, InnerEnum innerEnum, string key, object defaultValue, string usage, string description, bool supported ) : this( key, defaultValue, usage, description, supported, false )
			  {

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal Options( string name, InnerEnum innerEnum, string key, object defaultValue, string usage, string description, bool supported, bool keyAndUsageGoTogether )
			  {
					this._key = key;
					this._defaultValue = defaultValue;
					this._usage = usage;
					this._description = description;
					this._supported = supported;
					this._keyAndUsageGoTogether = keyAndUsageGoTogether;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal string Key()
			  {
					return _key;
			  }

			  internal string Argument()
			  {
					return "--" + Key();
			  }

			  internal void PrintUsage( java.io.PrintStream @out )
			  {
					@out.println( Argument() + SpaceInBetweenArgumentAndUsage() + _usage );
					foreach ( string line in Args.splitLongLine( DescriptionWithDefaultValue().Replace("`", ""), 80 ) )
					{
						 @out.println( "\t" + line );
					}
			  }

			  internal string SpaceInBetweenArgumentAndUsage()
			  {
					return _keyAndUsageGoTogether ? "" : " ";
			  }

			  internal string DescriptionWithDefaultValue()
			  {
					string result = _description;
					if ( _defaultValue != null )
					{
						 if ( !result.EndsWith( ".", StringComparison.Ordinal ) )
						 {
							  result += ".";
						 }
						 result += " Default value: " + _defaultValue;
					}
					return result;
			  }

			  internal string ManPageEntry()
			  {
					string filteredDescription = DescriptionWithDefaultValue().Replace(AvailableProcessorsHint(), "");
					string usageString = ( _usage.Length > 0 ) ? SpaceInBetweenArgumentAndUsage() + _usage : "";
					return "*" + Argument() + usageString + "*::\n" + filteredDescription + "\n\n";
			  }

			  internal string ManualEntry()
			  {
					return "[[import-tool-option-" + Key() + "]]\n" + ManPageEntry() + "//^\n\n";
			  }

			  internal object DefaultValue()
			  {
					return _defaultValue;
			  }

			  internal static string AvailableProcessorsHint()
			  {
					return " (in your case " + Runtime.Runtime.availableProcessors() + ")";
			  }

			  public bool SupportedOption
			  {
				  get
				  {
						return this._supported;
				  }
			  }

			 public static IList<Options> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static Options ValueOf( string name )
			 {
				 foreach ( Options enumInstance in Options.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 /// <summary>
		 /// Delimiter used between files in an input group.
		 /// </summary>
		 internal const string MULTI_FILE_DELIMITER = ",";

		 private ImportTool()
		 {
		 }

		 /// <summary>
		 /// Runs the import tool given the supplied arguments.
		 /// </summary>
		 /// <param name="incomingArguments"> arguments for specifying input and configuration for the import. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] incomingArguments) throws java.io.IOException
		 public static void Main( string[] incomingArguments )
		 {
			  Main( incomingArguments, false );
		 }

		 /// <summary>
		 /// Runs the import tool given the supplied arguments.
		 /// </summary>
		 /// <param name="incomingArguments"> arguments for specifying input and configuration for the import. </param>
		 /// <param name="defaultSettingsSuitableForTests"> default configuration geared towards unit/integration
		 /// test environments, for example lower default buffer sizes. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] incomingArguments, boolean defaultSettingsSuitableForTests) throws java.io.IOException
		 public static void Main( string[] incomingArguments, bool defaultSettingsSuitableForTests )
		 {
			  Console.Error.WriteLine( format( "WARNING: Neo4Net-import is deprecated and support for it will be removed in a future%n" + "version of Neo4Net; please use Neo4Net-admin import instead." ) );

			  PrintStream @out = System.out;
			  PrintStream err = System.err;
			  Args args = Args.parse( incomingArguments );

			  if ( ArrayUtil.isEmpty( incomingArguments ) || AsksForUsage( args ) )
			  {
					PrintUsage( @out );
					return;
			  }

			  File storeDir;
			  ICollection<Args.Option<File[]>> nodesFiles;
			  ICollection<Args.Option<File[]>> relationshipsFiles;
			  bool enableStacktrace;
			  Number processors;
			  Input input;
			  long badTolerance;
			  Charset inputEncoding;
			  bool skipBadRelationships;
			  bool skipDuplicateNodes;
			  bool ignoreExtraColumns;
			  bool skipBadEntriesLogging;
			  Config dbConfig;
			  Stream badOutput = null;
			  IdType idType;
			  Neo4Net.@unsafe.Impl.Batchimport.Configuration configuration;
			  File badFile = null;
			  long? maxMemory;
			  bool? defaultHighIO;
			  Stream @in;

			  bool success = false;
			  try
			  {
					  using ( FileSystemAbstraction fs = new DefaultFileSystemAbstraction() )
					  {
						args = UseArgumentsFromFileArgumentIfPresent( args );
      
						storeDir = args.InterpretOption( Options.StoreDir.key(), Converters.mandatory(), Converters.toFile(), Validators.DIRECTORY_IS_WRITABLE );
      
						skipBadEntriesLogging = args.GetBoolean( Options.SkipBadEntriesLogging.key(), (bool?) Options.SkipBadEntriesLogging.defaultValue(), false ).Value;
						if ( !skipBadEntriesLogging )
						{
							 badFile = new File( storeDir, BAD_FILE_NAME );
							 badOutput = new BufferedOutputStream( fs.OpenAsOutputStream( badFile, false ) );
						}
						nodesFiles = ExtractInputFiles( args, Options.NodeData.key(), err );
						relationshipsFiles = ExtractInputFiles( args, Options.RelationshipData.key(), err );
						string maxMemoryString = args.Get( Options.MaxMemory.key(), null );
						maxMemory = ParseMaxMemory( maxMemoryString );
      
						ValidateInputFiles( nodesFiles, relationshipsFiles );
						enableStacktrace = args.GetBoolean( Options.Stacktrace.key(), false, true ).Value;
						processors = args.GetNumber( Options.Processors.key(), null );
						idType = args.InterpretOption( Options.IdType.key(), withDefault((IdType)Options.IdType.defaultValue()), _toIdType );
						badTolerance = ParseNumberOrUnlimited( args, Options.BadTolerance );
						inputEncoding = Charset.forName( args.Get( Options.InputEncoding.key(), defaultCharset().name() ) );
      
						skipBadRelationships = args.GetBoolean( Options.SkipBadRelationships.key(), (bool?)Options.SkipBadRelationships.defaultValue(), true ).Value;
						skipDuplicateNodes = args.GetBoolean( Options.SkipDuplicateNodes.key(), (bool?)Options.SkipDuplicateNodes.defaultValue(), true ).Value;
						ignoreExtraColumns = args.GetBoolean( Options.IgnoreExtraColumns.key(), (bool?)Options.IgnoreExtraColumns.defaultValue(), true ).Value;
						defaultHighIO = args.GetBoolean( Options.HighIo.key(), (bool?)Options.HighIo.defaultValue(), true );
      
						Collector badCollector = GetBadCollector( badTolerance, skipBadRelationships, skipDuplicateNodes, ignoreExtraColumns, skipBadEntriesLogging, badOutput );
      
						dbConfig = LoadDbConfig( args.InterpretOption( Options.DatabaseConfig.key(), Converters.optional(), Converters.toFile(), Validators.REGEX_FILE_EXISTS ) );
						dbConfig.Augment( LoadDbConfig( args.InterpretOption( Options.AdditionalConfig.key(), Converters.optional(), Converters.toFile(), Validators.REGEX_FILE_EXISTS ) ) );
						dbConfig.augment( GraphDatabaseSettings.Neo4Net_home, storeDir.CanonicalFile.ParentFile.AbsolutePath );
						bool allowCacheOnHeap = args.GetBoolean( Options.CacheOnHeap.key(), (bool?) Options.CacheOnHeap.defaultValue() ).Value;
						configuration = ImportConfiguration( processors, defaultSettingsSuitableForTests, dbConfig, maxMemory, storeDir, allowCacheOnHeap, defaultHighIO );
						input = new CsvInput( NodeData( inputEncoding, nodesFiles ), defaultFormatNodeFileHeader(), RelationshipData(inputEncoding, relationshipsFiles), defaultFormatRelationshipFileHeader(), idType, CsvConfiguration(args, defaultSettingsSuitableForTests), badCollector, new CsvInput.PrintingMonitor(@out) );
						@in = defaultSettingsSuitableForTests ? new MemoryStream( EMPTY_BYTE_ARRAY ) : System.in;
						bool detailedPrinting = args.GetBoolean( Options.DetailedProgress.key(), (bool?) Options.DetailedProgress.defaultValue() ).Value;
      
						DoImport( @out, err, @in, DatabaseLayout.of( storeDir ), badFile, fs, nodesFiles, relationshipsFiles, enableStacktrace, input, dbConfig, badOutput, configuration, detailedPrinting );
      
						success = true;
					  }
			  }
			  catch ( System.ArgumentException e )
			  {
					throw AndPrintError( "Input error", e, false, err );
			  }
			  catch ( Exception e ) when ( e is IOException || e is UncheckedIOException )
			  {
					throw AndPrintError( "File error", e, false, err );
			  }
			  finally
			  {
					if ( !success && badOutput != null )
					{
						 badOutput.Close();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static Neo4Net.helpers.Args useArgumentsFromFileArgumentIfPresent(Neo4Net.helpers.Args args) throws java.io.IOException
		 public static Args UseArgumentsFromFileArgumentIfPresent( Args args )
		 {
			  string fileArgument = args.Get( Options.File.key(), null );
			  if ( !string.ReferenceEquals( fileArgument, null ) )
			  {
					// Are there any other arguments supplied, in addition to this -f argument?
					if ( args.AsMap().Count > 1 )
					{
						 throw new System.ArgumentException( "Supplying arguments in addition to " + Options.File.argument() + " isn't supported." );
					}

					// Read the arguments from the -f file and use those instead
					args = Args.parse( ParseFileArgumentList( new File( fileArgument ) ) );
			  }
			  return args;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static String[] parseFileArgumentList(java.io.File file) throws java.io.IOException
		 public static string[] ParseFileArgumentList( File file )
		 {
			  IList<string> arguments = new List<string>();
			  readTextFile( file, line => ( ( IList<string> )arguments ).AddRange( asList( tokenizeStringWithQuotes( line, true, true, false ) ) ) );
			  return arguments.ToArray();
		 }

		 internal static long? ParseMaxMemory( string maxMemoryString )
		 {
			  if ( !string.ReferenceEquals( maxMemoryString, null ) )
			  {
					maxMemoryString = maxMemoryString.Trim();
					if ( maxMemoryString.EndsWith( "%", StringComparison.Ordinal ) )
					{
						 int percent = int.Parse( maxMemoryString.Substring( 0, maxMemoryString.Length - 1 ) );
						 long result = calculateMaxMemoryFromPercent( percent );
						 if ( !canDetectFreeMemory() )
						 {
							  Console.Error.WriteLine( "WARNING: amount of free memory couldn't be detected so defaults to " + bytes( result ) + ". For optimal performance instead explicitly specify amount of " + "memory that importer is allowed to use using " + Options.MaxMemory.argument() );
						 }
						 return result;
					}
					return Settings.parseLongWithUnit( maxMemoryString );
			  }
			  return null;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void doImport(java.io.PrintStream out, java.io.PrintStream err, java.io.InputStream in, Neo4Net.io.layout.DatabaseLayout databaseLayout, java.io.File badFile, Neo4Net.io.fs.FileSystemAbstraction fs, java.util.Collection<Neo4Net.helpers.Args.Option<java.io.File[]>> nodesFiles, java.util.Collection<Neo4Net.helpers.Args.Option<java.io.File[]>> relationshipsFiles, boolean enableStacktrace, Neo4Net.unsafe.impl.batchimport.input.Input input, Neo4Net.kernel.configuration.Config dbConfig, java.io.OutputStream badOutput, Neo4Net.unsafe.impl.batchimport.Configuration configuration, boolean detailedProgress) throws java.io.IOException
		 public static void DoImport( PrintStream @out, PrintStream err, Stream @in, DatabaseLayout databaseLayout, File badFile, FileSystemAbstraction fs, ICollection<Args.Option<File[]>> nodesFiles, ICollection<Args.Option<File[]>> relationshipsFiles, bool enableStacktrace, Input input, Config dbConfig, Stream badOutput, Neo4Net.@unsafe.Impl.Batchimport.Configuration configuration, bool detailedProgress )
		 {
			  bool success;
			  LifeSupport life = new LifeSupport();

			  File internalLogFile = dbConfig.Get( store_internal_log_path );
			  LogService logService = life.Add( StoreLogService.withInternalLog( internalLogFile ).build( fs ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.scheduler.JobScheduler jobScheduler = life.add(createScheduler());
			  IJobScheduler jobScheduler = life.Add( createScheduler() );

			  life.Start();
			  ExecutionMonitor executionMonitor = detailedProgress ? new SpectrumExecutionMonitor( 2, TimeUnit.SECONDS, @out, SpectrumExecutionMonitor.DEFAULT_WIDTH ) : ExecutionMonitors.defaultVisible( @in, jobScheduler );
			  BatchImporter importer = BatchImporterFactory.withHighestPriority().instantiate(databaseLayout, fs, null, configuration, logService, executionMonitor, EMPTY, dbConfig, RecordFormatSelector.selectForConfig(dbConfig, logService.InternalLogProvider), new PrintingImportLogicMonitor(@out, err), jobScheduler);
			  PrintOverview( databaseLayout.DatabaseDirectory(), nodesFiles, relationshipsFiles, configuration, @out );
			  success = false;
			  try
			  {
					importer.DoImport( input );
					success = true;
			  }
			  catch ( Exception e )
			  {
					throw AndPrintError( "Import error", e, enableStacktrace, err );
			  }
			  finally
			  {
					Collector collector = input.BadCollector();
					long numberOfBadEntries = collector.BadEntries();
					collector.Close();
					IOUtils.closeAll( badOutput );

					if ( badFile != null )
					{
						 if ( numberOfBadEntries > 0 )
						 {
							  Console.WriteLine( "There were bad entries which were skipped and logged into " + badFile.AbsolutePath );
						 }
					}

					life.Shutdown();

					if ( !success )
					{
						 err.println( "WARNING Import failed. The store files in " + databaseLayout.DatabaseDirectory().AbsolutePath + " are left as they are, although they are likely in an unusable state. " + "Starting a database on these store files will likely fail or observe inconsistent records so " + "start at your own risk or delete the store manually" );
					}
			  }
		 }

		 public static ICollection<Args.Option<File[]>> ExtractInputFiles( Args args, string key, PrintStream err )
		 {
			  return args.InterpretOptionsWithMetadata( key, Converters.optional(), Converters.toFiles(MULTI_FILE_DELIMITER, Converters.regexFiles(true)), FilesExist(err), Validators.atLeast("--" + key, 1) );
		 }

		 private static Validator<File[]> FilesExist( PrintStream err )
		 {
			  return files =>
			  {
				foreach ( File file in files )
				{
					 if ( file.Name.StartsWith( ":" ) )
					 {
						  err.println( "It looks like you're trying to specify default label or relationship type (" + file.Name + "). Please put such directly on the key, f.ex. " + Options.NodeData.argument() + ":MyLabel" );
					 }
					 Validators.REGEX_FILE_EXISTS.validate( file );
				}
			  };
		 }

		 private static Collector GetBadCollector( long badTolerance, bool skipBadRelationships, bool skipDuplicateNodes, bool ignoreExtraColumns, bool skipBadEntriesLogging, Stream badOutput )
		 {
			  int collect = collect( skipBadRelationships, skipDuplicateNodes, ignoreExtraColumns );
			  return skipBadEntriesLogging ? silentBadCollector( badTolerance, collect ) : badCollector( badOutput, badTolerance, collect );
		 }

		 private static long ParseNumberOrUnlimited( Args args, Options option )
		 {
			  string value = args.Get( option.key(), option.defaultValue().ToString() );
			  return UNLIMITED.Equals( value ) ? BadCollector.UNLIMITED_TOLERANCE : long.Parse( value );
		 }

		 private static Config LoadDbConfig( File file )
		 {
			  return Config.fromFile( file ).build();
		 }

		 internal static void PrintOverview( File storeDir, ICollection<Args.Option<File[]>> nodesFiles, ICollection<Args.Option<File[]>> relationshipsFiles, Neo4Net.@unsafe.Impl.Batchimport.Configuration configuration, PrintStream @out )
		 {
			  @out.println( "Neo4Net version: " + Version.Neo4NetVersion );
			  @out.println( "Importing the contents of these files into " + storeDir + ":" );
			  PrintInputFiles( "Nodes", nodesFiles, @out );
			  PrintInputFiles( "Relationships", relationshipsFiles, @out );
			  @out.println();
			  @out.println( "Available resources:" );
			  PrintIndented( "Total machine memory: " + bytes( OsBeanUtil.TotalPhysicalMemory ), @out );
			  PrintIndented( "Free machine memory: " + bytes( OsBeanUtil.FreePhysicalMemory ), @out );
			  PrintIndented( "Max heap memory : " + bytes( Runtime.Runtime.maxMemory() ), @out );
			  PrintIndented( "Processors: " + configuration.MaxNumberOfProcessors(), @out );
			  PrintIndented( "Configured max memory: " + bytes( configuration.MaxMemoryUsage() ), @out );
			  PrintIndented( "High-IO: " + configuration.HighIO(), @out );
			  @out.println();
		 }

		 private static void PrintInputFiles( string name, ICollection<Args.Option<File[]>> files, PrintStream @out )
		 {
			  if ( Files.Count == 0 )
			  {
					return;
			  }

			  @out.println( name + ":" );
			  int i = 0;
			  foreach ( Args.Option<File[]> group in files )
			  {
					if ( i++ > 0 )
					{
						 @out.println();
					}
					if ( !string.ReferenceEquals( group.Metadata(), null ) )
					{
						 PrintIndented( ":" + group.Metadata(), @out );
					}
					foreach ( File file in group.Value() )
					{
						 PrintIndented( file, @out );
					}
			  }
		 }

		 private static void PrintIndented( object value, PrintStream @out )
		 {
			  @out.println( "  " + value );
		 }

		 public static void ValidateInputFiles( ICollection<Args.Option<File[]>> nodesFiles, ICollection<Args.Option<File[]>> relationshipsFiles )
		 {
			  if ( nodesFiles.Count == 0 )
			  {
					if ( relationshipsFiles.Count == 0 )
					{
						 throw new System.ArgumentException( "No input specified, nothing to import" );
					}
					throw new System.ArgumentException( "No node input specified, cannot import relationships without nodes" );
			  }
		 }

		 public static Neo4Net.@unsafe.Impl.Batchimport.Configuration ImportConfiguration( Number processors, bool defaultSettingsSuitableForTests, Config dbConfig, File storeDir, bool? defaultHighIO )
		 {
			  return ImportConfiguration( processors, defaultSettingsSuitableForTests, dbConfig, null, storeDir, DEFAULT.allowCacheAllocationOnHeap(), defaultHighIO );
		 }

		 public static Neo4Net.@unsafe.Impl.Batchimport.Configuration ImportConfiguration( Number processors, bool defaultSettingsSuitableForTests, Config dbConfig, long? maxMemory, File storeDir, bool allowCacheOnHeap, bool? defaultHighIO )
		 {
			  return new ConfigurationAnonymousInnerClass( processors, defaultSettingsSuitableForTests, dbConfig, maxMemory, storeDir, allowCacheOnHeap, defaultHighIO );
		 }

		 private class ConfigurationAnonymousInnerClass : Neo4Net.@unsafe.Impl.Batchimport.Configuration
		 {
			 private Number _processors;
			 private bool _defaultSettingsSuitableForTests;
			 private Config _dbConfig;
			 private long? _maxMemory;
			 private File _storeDir;
			 private bool _allowCacheOnHeap;
			 private bool? _defaultHighIO;

			 public ConfigurationAnonymousInnerClass( Number processors, bool defaultSettingsSuitableForTests, Config dbConfig, long? maxMemory, File storeDir, bool allowCacheOnHeap, bool? defaultHighIO )
			 {
				 this._processors = processors;
				 this._defaultSettingsSuitableForTests = defaultSettingsSuitableForTests;
				 this._dbConfig = dbConfig;
				 this._maxMemory = maxMemory;
				 this._storeDir = storeDir;
				 this._allowCacheOnHeap = allowCacheOnHeap;
				 this._defaultHighIO = defaultHighIO;
			 }

			 public long pageCacheMemory()
			 {
				  return _defaultSettingsSuitableForTests ? mebiBytes( 8 ) : DEFAULT.pageCacheMemory();
			 }

			 public int maxNumberOfProcessors()
			 {
				  return _processors != null ? _processors.intValue() : DEFAULT.maxNumberOfProcessors();
			 }

			 public int denseNodeThreshold()
			 {
				  return _dbConfig.get( GraphDatabaseSettings.dense_node_threshold );
			 }

			 public long maxMemoryUsage()
			 {
				  return _maxMemory != null ? _maxMemory.Value : DEFAULT.maxMemoryUsage();
			 }

			 public bool highIO()
			 {
				  return _defaultHighIO != null ? _defaultHighIO.Value : FileUtils.highIODevice( _storeDir.toPath(), false );
			 }

			 public bool allowCacheAllocationOnHeap()
			 {
				  return _allowCacheOnHeap;
			 }
		 }

		 private static string ManualReference( ManualPage page, Anchor anchor )
		 {
			  // Docs are versioned major.minor-suffix, so drop the patch version.
			  string[] versionParts = Version.Neo4NetVersion.Split( "-", true );
			  versionParts[0] = versionParts[0].Substring( 0, 3 );
			  string docsVersion = string.join( "-", versionParts );

			  return " https://Neo4Net.com/docs/operations-manual/" + docsVersion + "/" + page.getReference( anchor );
		 }

		 /// <summary>
		 /// Method name looks strange, but look at how it's used and you'll see why it's named like that. </summary>
		 /// <param name="stackTrace"> whether or not to also print the stack trace of the error. </param>
		 /// <param name="err"> </param>
		 private static Exception AndPrintError( string typeOfError, Exception e, bool stackTrace, PrintStream err )
		 {
			  // List of common errors that can be explained to the user
			  if ( typeof( DuplicateInputIdException ).Equals( e.GetType() ) )
			  {
					PrintErrorMessage( "Duplicate input ids that would otherwise clash can be put into separate id space, " + "read more about how to use id spaces in the manual:" + ManualReference( ManualPage.ImportToolFormat, Anchor.IdSpaces ), e, stackTrace, err );
			  }
			  else if ( typeof( MissingRelationshipDataException ).Equals( e.GetType() ) )
			  {
					PrintErrorMessage( "Relationship missing mandatory field '" + ( ( MissingRelationshipDataException ) e ).FieldType + "', read more about " + "relationship format in the manual: " + ManualReference( ManualPage.ImportToolFormat, Anchor.Relationship ), e, stackTrace, err );
			  }
			  // This type of exception is wrapped since our input code throws InputException consistently,
			  // and so IllegalMultilineFieldException comes from the csv component, which has no access to InputException
			  // therefore it's wrapped.
			  else if ( Exceptions.contains( e, typeof( IllegalMultilineFieldException ) ) )
			  {
					PrintErrorMessage( "Detected field which spanned multiple lines for an import where " + Options.MultilineFields.argument() + "=false. If you know that your input data " + "include fields containing new-line characters then import with this option set to " + "true.", e, stackTrace, err );
			  }
			  else if ( Exceptions.contains( e, typeof( InputException ) ) )
			  {
					PrintErrorMessage( "Error in input data", e, stackTrace, err );
			  }
			  // Fallback to printing generic error and stack trace
			  else
			  {
					PrintErrorMessage( typeOfError + ": " + e.Message, e, true, err );
			  }
			  err.println();

			  // Mute the stack trace that the default exception handler would have liked to print.
			  // Calling System.exit( 1 ) or similar would be convenient on one hand since we can set
			  // a specific exit code. On the other hand It's very inconvenient to have any System.exit
			  // call in code that is tested.
			  Thread.CurrentThread.UncaughtExceptionHandler = ( t, e1 ) =>
			  {
				/* Shhhh */
			  };
			  throwIfUnchecked( e );
			  return new Exception( e ); // throw in order to have process exit with !0
		 }

		 private static void PrintErrorMessage( string @string, Exception e, bool stackTrace, PrintStream err )
		 {
			  err.println( @string );
			  err.println( "Caused by:" + e.Message );
			  if ( stackTrace )
			  {
					e.printStackTrace( err );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Iterable<Neo4Net.unsafe.impl.batchimport.input.csv.DataFactory> relationshipData(final java.nio.charset.Charset encoding, java.util.Collection<Neo4Net.helpers.Args.Option<java.io.File[]>> relationshipsFiles)
		 public static IEnumerable<DataFactory> RelationshipData( Charset encoding, ICollection<Args.Option<File[]>> relationshipsFiles )
		 {
			  return new IterableWrapperAnonymousInnerClass( relationshipsFiles, encoding );
		 }

		 private class IterableWrapperAnonymousInnerClass : IterableWrapper<DataFactory, Args.Option<File[]>>
		 {
			 private Charset _encoding;

			 public IterableWrapperAnonymousInnerClass( ICollection<Args.Option<File[]>> relationshipsFiles, Charset encoding ) : base( relationshipsFiles )
			 {
				 this._encoding = encoding;
			 }

			 protected internal override DataFactory underlyingObjectToObject( Args.Option<File[]> group )
			 {
				  return data( defaultRelationshipType( group.Metadata() ), _encoding, group.Value() );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Iterable<Neo4Net.unsafe.impl.batchimport.input.csv.DataFactory> nodeData(final java.nio.charset.Charset encoding, java.util.Collection<Neo4Net.helpers.Args.Option<java.io.File[]>> nodesFiles)
		 public static IEnumerable<DataFactory> NodeData( Charset encoding, ICollection<Args.Option<File[]>> nodesFiles )
		 {
			  return new IterableWrapperAnonymousInnerClass2( nodesFiles, encoding );
		 }

		 private class IterableWrapperAnonymousInnerClass2 : IterableWrapper<DataFactory, Args.Option<File[]>>
		 {
			 private Charset _encoding;

			 public IterableWrapperAnonymousInnerClass2( ICollection<Args.Option<File[]>> nodesFiles, Charset encoding ) : base( nodesFiles )
			 {
				 this._encoding = encoding;
			 }

			 protected internal override DataFactory underlyingObjectToObject( Args.Option<File[]> input )
			 {
				  Decorator decorator = !string.ReferenceEquals( input.Metadata(), null ) ? additiveLabels(input.Metadata().Split(":", true)) : NO_DECORATOR;
				  return data( decorator, _encoding, input.Value() );
			 }
		 }

		 private static void PrintUsage( PrintStream @out )
		 {
			  @out.println( "Neo4Net Import Tool" );
			  foreach ( string line in Args.splitLongLine( "Neo4Net-import is used to create a new Neo4Net database " + "from data in CSV files. " + "See the chapter \"Import Tool\" in the Neo4Net Manual for details on the CSV file format " + "- a special kind of header is required.", 80 ) )
			  {
					@out.println( "\t" + line );
			  }
			  @out.println( "Usage:" );
			  foreach ( Options option in Options.values() )
			  {
					option.printUsage( @out );
			  }

			  @out.println( "Example:" );
			  @out.print( Strings.joinAsLines( TAB + "bin/Neo4Net-import --into retail.db --id-type string --nodes:Customer customers.csv ", TAB + "--nodes products.csv --nodes orders_header.csv,orders1.csv,orders2.csv ", TAB + "--relationships:CONTAINS order_details.csv ", TAB + "--relationships:ORDERED customer_orders_header.csv,orders1.csv,orders2.csv" ) );
		 }

		 private static bool AsksForUsage( Args args )
		 {
			  foreach ( string orphan in args.Orphans() )
			  {
					if ( IsHelpKey( orphan ) )
					{
						 return true;
					}
			  }

			  foreach ( KeyValuePair<string, string> option in args.AsMap().SetOfKeyValuePairs() )
			  {
					if ( IsHelpKey( option.Key ) )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 private static bool IsHelpKey( string key )
		 {
			  return key.Equals( "?" ) || key.Equals( "help" );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Neo4Net.unsafe.impl.batchimport.input.csv.Configuration csvConfiguration(Neo4Net.helpers.Args args, final boolean defaultSettingsSuitableForTests)
		 public static Configuration CsvConfiguration( Args args, bool defaultSettingsSuitableForTests )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.unsafe.impl.batchimport.input.csv.Configuration defaultConfiguration = COMMAS;
			  Configuration defaultConfiguration = COMMAS;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final System.Nullable<char> specificDelimiter = args.interpretOption(Options.DELIMITER.key(), Neo4Net.kernel.impl.util.Converters.optional(), CHARACTER_CONVERTER);
			  char? specificDelimiter = args.InterpretOption( Options.Delimiter.key(), Converters.optional(), _characterConverter );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final System.Nullable<char> specificArrayDelimiter = args.interpretOption(Options.ARRAY_DELIMITER.key(), Neo4Net.kernel.impl.util.Converters.optional(), CHARACTER_CONVERTER);
			  char? specificArrayDelimiter = args.InterpretOption( Options.ArrayDelimiter.key(), Converters.optional(), _characterConverter );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final System.Nullable<char> specificQuote = args.interpretOption(Options.QUOTE.key(), Neo4Net.kernel.impl.util.Converters.optional(), CHARACTER_CONVERTER);
			  char? specificQuote = args.InterpretOption( Options.Quote.key(), Converters.optional(), _characterConverter );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final System.Nullable<bool> multiLineFields = args.getBoolean(Options.MULTILINE_FIELDS.key(), null);
			  bool? multiLineFields = args.GetBoolean( Options.MultilineFields.key(), null );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final System.Nullable<bool> emptyStringsAsNull = args.getBoolean(Options.IGNORE_EMPTY_STRINGS.key(), null);
			  bool? emptyStringsAsNull = args.GetBoolean( Options.IgnoreEmptyStrings.key(), null );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final System.Nullable<bool> trimStrings = args.getBoolean(Options.TRIM_STRINGS.key(), null);
			  bool? trimStrings = args.GetBoolean( Options.TrimStrings.key(), null );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final System.Nullable<bool> legacyStyleQuoting = args.getBoolean(Options.LEGACY_STYLE_QUOTING.key(), null);
			  bool? legacyStyleQuoting = args.GetBoolean( Options.LegacyStyleQuoting.key(), null );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Number bufferSize = args.has(Options.READ_BUFFER_SIZE.key()) ? parseLongWithUnit(args.get(Options.READ_BUFFER_SIZE.key(), null)) : null;
			  Number bufferSize = args.Has( Options.ReadBufferSize.key() ) ? parseLongWithUnit(args.Get(Options.ReadBufferSize.key(), null)) : null;
			  return new Configuration_DefaultAnonymousInnerClass( defaultSettingsSuitableForTests, defaultConfiguration, specificDelimiter, specificArrayDelimiter, specificQuote, multiLineFields, emptyStringsAsNull, trimStrings, legacyStyleQuoting, bufferSize );
		 }

		 private class Configuration_DefaultAnonymousInnerClass : Neo4Net.@unsafe.Impl.Batchimport.input.csv.Configuration_Default
		 {
			 private bool _defaultSettingsSuitableForTests;
			 private Configuration _defaultConfiguration;
			 private char? _specificDelimiter;
			 private char? _specificArrayDelimiter;
			 private char? _specificQuote;
			 private bool? _multiLineFields;
			 private bool? _emptyStringsAsNull;
			 private bool? _trimStrings;
			 private bool? _legacyStyleQuoting;
			 private Number _bufferSize;

			 public Configuration_DefaultAnonymousInnerClass( bool defaultSettingsSuitableForTests, Configuration defaultConfiguration, char? specificDelimiter, char? specificArrayDelimiter, char? specificQuote, bool? multiLineFields, bool? emptyStringsAsNull, bool? trimStrings, bool? legacyStyleQuoting, Number bufferSize )
			 {
				 this._defaultSettingsSuitableForTests = defaultSettingsSuitableForTests;
				 this._defaultConfiguration = defaultConfiguration;
				 this._specificDelimiter = specificDelimiter;
				 this._specificArrayDelimiter = specificArrayDelimiter;
				 this._specificQuote = specificQuote;
				 this._multiLineFields = multiLineFields;
				 this._emptyStringsAsNull = emptyStringsAsNull;
				 this._trimStrings = trimStrings;
				 this._legacyStyleQuoting = legacyStyleQuoting;
				 this._bufferSize = bufferSize;
			 }

			 public override char delimiter()
			 {
				  return _specificDelimiter != null ? _specificDelimiter.Value : _defaultConfiguration.delimiter();
			 }

			 public override char arrayDelimiter()
			 {
				  return _specificArrayDelimiter != null ? _specificArrayDelimiter.Value : _defaultConfiguration.arrayDelimiter();
			 }

			 public override char quotationCharacter()
			 {
				  return _specificQuote != null ? _specificQuote.Value : _defaultConfiguration.quotationCharacter();
			 }

			 public override bool multilineFields()
			 {
				  return _multiLineFields != null ? _multiLineFields.Value : _defaultConfiguration.multilineFields();
			 }

			 public override bool emptyQuotedStringsAsNull()
			 {
				  return _emptyStringsAsNull != null ? _emptyStringsAsNull.Value : _defaultConfiguration.emptyQuotedStringsAsNull();
			 }

			 public override int bufferSize()
			 {
				  return _bufferSize != null ? _bufferSize.intValue() : _defaultSettingsSuitableForTests ? 10_000 : base.bufferSize();
			 }

			 public override bool trimStrings()
			 {
				  return _trimStrings != null ? _trimStrings.Value : _defaultConfiguration.trimStrings();
			 }

			 public override bool legacyStyleQuoting()
			 {
				  return _legacyStyleQuoting != null ? _legacyStyleQuoting.Value : _defaultConfiguration.legacyStyleQuoting();
			 }
		 }

		 private static readonly System.Func<string, IdType> _toIdType = from => IdType.ValueOf( from.ToUpper() );

		 private static readonly System.Func<string, char> _characterConverter = new CharacterConverter();

		 private sealed class ManualPage
		 {
			  public static readonly ManualPage ImportToolFormat = new ManualPage( "ImportToolFormat", InnerEnum.ImportToolFormat, "tools/import/file-header-format/" );

			  private static readonly IList<ManualPage> valueList = new List<ManualPage>();

			  static ManualPage()
			  {
				  valueList.Add( ImportToolFormat );
			  }

			  public enum InnerEnum
			  {
				  ImportToolFormat
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Private readonly;

			  internal ManualPage( string name, InnerEnum innerEnum, string page )
			  {
					this._page = page;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  public string GetReference( Anchor anchor )
			  {
					// As long as the the operations manual is single-page we only use the anchor.
					return _page + "#" + anchor.anchor;
			  }

			 public static IList<ManualPage> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static ManualPage ValueOf( string name )
			 {
				 foreach ( ManualPage enumInstance in ManualPage.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private sealed class Anchor
		 {
			  public static readonly Anchor IdSpaces = new Anchor( "IdSpaces", InnerEnum.IdSpaces, "import-tool-id-spaces" );
			  public static readonly Anchor Relationship = new Anchor( "Relationship", InnerEnum.Relationship, "import-tool-header-format-rels" );

			  private static readonly IList<Anchor> valueList = new List<Anchor>();

			  static Anchor()
			  {
				  valueList.Add( IdSpaces );
				  valueList.Add( Relationship );
			  }

			  public enum InnerEnum
			  {
				  IdSpaces,
				  Relationship
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Private readonly;

			  internal Anchor( string name, InnerEnum innerEnum, string anchor )
			  {
					this._anchor = anchor;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			 public static IList<Anchor> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static Anchor ValueOf( string name )
			 {
				 foreach ( Anchor enumInstance in Anchor.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }
	}

}