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
namespace Neo4Net.@unsafe.Impl.Batchimport.input.csv
{

	using Neo4Net.Collections;
	using CharReadable = Neo4Net.Csv.Reader.CharReadable;
	using CharSeeker = Neo4Net.Csv.Reader.CharSeeker;
	using MultiReadable = Neo4Net.Csv.Reader.MultiReadable;
	using NumberArrayFactory = Neo4Net.@unsafe.Impl.Batchimport.cache.NumberArrayFactory;
	using IdMapper = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.IdMapper;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.csv.reader.CharSeekers.charSeeker;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.ByteUnit.mebiBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.Collector.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.Inputs.calculatePropertySize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.Inputs.knownEstimates;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.csv.CsvGroupInputIterator.extractors;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.input.csv.CsvInputIterator.extractHeader;

	/// <summary>
	/// Provides <seealso cref="Input"/> from data contained in tabular/csv form. Expects factories for instantiating
	/// the <seealso cref="CharSeeker"/> objects seeking values in the csv data and header factories for how to
	/// extract meta data about the values.
	/// </summary>
	public class CsvInput : Input
	{
		 private static readonly long _estimateSampleSize = mebiBytes( 1 );

		 private readonly IEnumerable<DataFactory> _nodeDataFactory;
		 private readonly Header.Factory _nodeHeaderFactory;
		 private readonly IEnumerable<DataFactory> _relationshipDataFactory;
		 private readonly Header.Factory _relationshipHeaderFactory;
		 private readonly IdType _idType;
		 private readonly Configuration _config;
		 private readonly Collector _badCollector;
		 private readonly Monitor _monitor;
		 private readonly Groups _groups;

		 /// <param name="nodeDataFactory"> multiple <seealso cref="DataFactory"/> instances providing data, each <seealso cref="DataFactory"/>
		 /// specifies an input group with its own header, extracted by the {@code nodeHeaderFactory}. From the outside
		 /// it looks like one stream of nodes. </param>
		 /// <param name="nodeHeaderFactory"> factory for reading node headers. </param>
		 /// <param name="relationshipDataFactory"> multiple <seealso cref="DataFactory"/> instances providing data, each <seealso cref="DataFactory"/>
		 /// specifies an input group with its own header, extracted by the {@code relationshipHeaderFactory}.
		 /// From the outside it looks like one stream of relationships. </param>
		 /// <param name="relationshipHeaderFactory"> factory for reading relationship headers. </param>
		 /// <param name="idType"> <seealso cref="IdType"/> to expect in id fields of node and relationship input. </param>
		 /// <param name="config"> CSV configuration. </param>
		 /// <param name="badCollector"> Collector getting calls about bad input data. </param>
		 /// <param name="monitor"> <seealso cref="Monitor"/> for internal events. </param>
		 public CsvInput( IEnumerable<DataFactory> nodeDataFactory, Header.Factory nodeHeaderFactory, IEnumerable<DataFactory> relationshipDataFactory, Header.Factory relationshipHeaderFactory, IdType idType, Configuration config, Collector badCollector, Monitor monitor ) : this( nodeDataFactory, nodeHeaderFactory, relationshipDataFactory, relationshipHeaderFactory, idType, config, badCollector, monitor, new Groups() )
		 {
		 }

		 internal CsvInput( IEnumerable<DataFactory> nodeDataFactory, Header.Factory nodeHeaderFactory, IEnumerable<DataFactory> relationshipDataFactory, Header.Factory relationshipHeaderFactory, IdType idType, Configuration config, Collector badCollector, Monitor monitor, Groups groups )
		 {
			  AssertSaneConfiguration( config );

			  this._nodeDataFactory = nodeDataFactory;
			  this._nodeHeaderFactory = nodeHeaderFactory;
			  this._relationshipDataFactory = relationshipDataFactory;
			  this._relationshipHeaderFactory = relationshipHeaderFactory;
			  this._idType = idType;
			  this._config = config;
			  this._badCollector = badCollector;
			  this._monitor = monitor;
			  this._groups = groups;

			  VerifyHeaders();
			  WarnAboutDuplicateSourceFiles();
		 }

		 /// <summary>
		 /// Verifies so that all headers in input files looks sane:
		 /// <ul>
		 /// <li>node/relationship headers can be parsed correctly</li>
		 /// <li>relationship headers uses ID spaces previously defined in node headers</li>
		 /// </ul>
		 /// </summary>
		 private void VerifyHeaders()
		 {
			  try
			  {
					// parse all node headers and remember all ID spaces
					foreach ( DataFactory dataFactory in _nodeDataFactory )
					{
						 using ( CharSeeker dataStream = charSeeker( new MultiReadable( dataFactory.Create( _config ).stream() ), _config, true ) )
						 {
							  // Parsing and constructing this header will create this group,
							  // so no need to do something with the result of it right now
							  _nodeHeaderFactory.create( dataStream, _config, _idType, _groups );
						 }
					}

					// parse all relationship headers and verify all ID spaces
					foreach ( DataFactory dataFactory in _relationshipDataFactory )
					{
						 using ( CharSeeker dataStream = charSeeker( new MultiReadable( dataFactory.Create( _config ).stream() ), _config, true ) )
						 {
							  // Merely parsing and constructing the header here will as a side-effect verify that the
							  // id groups already exists (relationship header isn't allowed to create groups)
							  _relationshipHeaderFactory.create( dataStream, _config, _idType, _groups );
						 }
					}
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private void WarnAboutDuplicateSourceFiles()
		 {
			  try
			  {
					ISet<string> seenSourceFiles = new HashSet<string>();
					WarnAboutDuplicateSourceFiles( seenSourceFiles, _nodeDataFactory );
					WarnAboutDuplicateSourceFiles( seenSourceFiles, _relationshipDataFactory );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void warnAboutDuplicateSourceFiles(java.util.Set<String> seenSourceFiles, Iterable<DataFactory> dataFactories) throws java.io.IOException
		 private void WarnAboutDuplicateSourceFiles( ISet<string> seenSourceFiles, IEnumerable<DataFactory> dataFactories )
		 {
			  foreach ( DataFactory dataFactory in dataFactories )
			  {
					RawIterator<CharReadable, IOException> stream = dataFactory.Create( _config ).stream();
					while ( stream.HasNext() )
					{
						 using ( CharReadable source = stream.Next() )
						 {
							  WarnAboutDuplicateSourceFiles( seenSourceFiles, source );
						 }
					}
			  }
		 }

		 private void WarnAboutDuplicateSourceFiles( ISet<string> seenSourceFiles, CharReadable source )
		 {
			  string sourceDescription = source.SourceDescription();
			  if ( !seenSourceFiles.Add( sourceDescription ) )
			  {
					_monitor.duplicateSourceFile( sourceDescription );
			  }
		 }

		 private static void AssertSaneConfiguration( Configuration config )
		 {
			  IDictionary<char, string> delimiters = new Dictionary<char, string>();
			  delimiters[config.delimiter()] = "delimiter";
			  CheckUniqueCharacter( delimiters, config.arrayDelimiter(), "array delimiter" );
			  CheckUniqueCharacter( delimiters, config.quotationCharacter(), "quotation character" );
		 }

		 private static void CheckUniqueCharacter( IDictionary<char, string> characters, char character, string characterDescription )
		 {
			  string conflict = characters[character] = characterDescription;
			  if ( !string.ReferenceEquals( conflict, null ) )
			  {
					throw new System.ArgumentException( "Character '" + character + "' specified by " + characterDescription + " is the same as specified by " + conflict );
			  }
		 }

		 public override InputIterable Nodes()
		 {
			  return () => Stream(_nodeDataFactory, _nodeHeaderFactory);
		 }

		 public override InputIterable Relationships()
		 {
			  return () => Stream(_relationshipDataFactory, _relationshipHeaderFactory);
		 }

		 private InputIterator Stream( IEnumerable<DataFactory> data, Header.Factory headerFactory )
		 {
			  return new CsvGroupInputIterator( data.GetEnumerator(), headerFactory, _idType, _config, _badCollector, _groups );
		 }

		 public override IdMapper IdMapper( NumberArrayFactory numberArrayFactory )
		 {
			  return _idType.idMapper( numberArrayFactory, _groups );
		 }

		 public override Collector BadCollector()
		 {
			  return _badCollector;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.unsafe.impl.batchimport.input.Input_Estimates calculateEstimates(System.Func<org.neo4j.values.storable.Value[], int> valueSizeCalculator) throws java.io.IOException
		 public override Neo4Net.@unsafe.Impl.Batchimport.input.Input_Estimates CalculateEstimates( System.Func<Value[], int> valueSizeCalculator )
		 {
			  long[] nodeSample = Sample( _nodeDataFactory, _nodeHeaderFactory, valueSizeCalculator, node => node.labels().length );
			  long[] relationshipSample = Sample( _relationshipDataFactory, _relationshipHeaderFactory, valueSizeCalculator, entity => 0 );
			  return knownEstimates( nodeSample[0], relationshipSample[0], nodeSample[1], relationshipSample[1], nodeSample[2], relationshipSample[2], nodeSample[3] );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long[] sample(Iterable<DataFactory> dataFactories, Header.Factory headerFactory, System.Func<org.neo4j.values.storable.Value[], int> valueSizeCalculator, System.Func<org.neo4j.unsafe.impl.batchimport.input.InputEntity, int> additionalCalculator) throws java.io.IOException
		 private long[] Sample( IEnumerable<DataFactory> dataFactories, Header.Factory headerFactory, System.Func<Value[], int> valueSizeCalculator, System.Func<InputEntity, int> additionalCalculator )
		 {
			  long[] estimates = new long[4]; // [entity count, property count, property size, labels (for nodes only)]
			  using ( CsvInputChunkProxy chunk = new CsvInputChunkProxy() )
			  {
					// One group of input files
					int groupId = 0;
					foreach ( DataFactory dataFactory in dataFactories ) // one input group
					{
						 groupId++;
						 Header header = null;
						 Data data = dataFactory.Create( _config );
						 RawIterator<CharReadable, IOException> sources = data.Stream();
						 while ( sources.HasNext() )
						 {
							  using ( CharReadable source = sources.Next() )
							  {
									if ( header == null )
									{
										 // Extract the header from the first file in this group
										 header = extractHeader( source, headerFactory, _idType, _config, _groups );
									}
									using ( CsvInputIterator iterator = new CsvInputIterator( source, data.Decorator(), header, _config, _idType, EMPTY, extractors(_config), groupId ), InputEntity entity = new InputEntity() )
									{
										 int entities = 0;
										 int properties = 0;
										 int propertySize = 0;
										 int additional = 0;
										 while ( iterator.Position() < _estimateSampleSize && iterator.Next(chunk) )
										 {
											  for ( ; chunk.Next( entity ); entities++ )
											  {
													properties += entity.PropertyCount();
													propertySize += calculatePropertySize( entity, valueSizeCalculator );
													additional += additionalCalculator( entity );
											  }
										 }
										 if ( entities > 0 )
										 {
											  long entityCountInSource = ( long )( ( ( double ) source.Length() / iterator.Position() ) * entities );
											  estimates[0] += entityCountInSource;
											  estimates[1] += ( long )( ( ( double ) properties / entities ) * entityCountInSource );
											  estimates[2] += ( long )( ( ( double ) propertySize / entities ) * entityCountInSource );
											  estimates[3] += ( long )( ( ( double ) additional / entities ) * entityCountInSource );
										 }
									}
							  }
						 }
					}
			  }
			  return estimates;
		 }

		 public interface Monitor
		 {
			  /// <summary>
			  /// Reports that a given source file has been specified more than one time. </summary>
			  /// <param name="sourceFile"> source file that is a duplicate. </param>
			  void DuplicateSourceFile( string sourceFile );
		 }

		 public static readonly Monitor NoMonitor = source =>
		 {
		 };

		 public class PrintingMonitor : Monitor
		 {
			  internal readonly PrintStream Out;

			  public PrintingMonitor( PrintStream @out )
			  {
					this.Out = @out;
			  }

			  public override void DuplicateSourceFile( string sourceFile )
			  {
					Out.println( string.Format( "WARN: source file {0} has been specified multiple times, this may result in unwanted duplicates", sourceFile ) );
			  }
		 }
	}

}