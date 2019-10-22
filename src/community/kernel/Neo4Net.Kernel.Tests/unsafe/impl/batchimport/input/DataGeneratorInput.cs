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
namespace Neo4Net.@unsafe.Impl.Batchimport.input
{

	using Extractors = Neo4Net.Csv.Reader.Extractors;
	using NumberArrayFactory = Neo4Net.@unsafe.Impl.Batchimport.cache.NumberArrayFactory;
	using IdMapper = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.IdMapper;
	using Header = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Header;
	using Entry = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Header.Entry;
	using IdType = Neo4Net.@unsafe.Impl.Batchimport.input.csv.IdType;
	using Type = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Type;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
	/// <summary>
	/// <seealso cref="Input"/> which generates data on the fly. This input wants to know number of nodes and relationships
	/// and then a function for generating the nodes and another for generating the relationships.
	/// So typical usage would be:
	/// 
	/// <pre>
	/// {@code
	/// BatchImporter importer = ...
	/// Input input = new DataGeneratorInput( 10_000_000, 1_000_000_000,
	///      batch -> {
	///          InputNode[] nodes = new InputNode[batch.getSize()];
	///          for ( int i = 0; i < batch.getSize(); i++ ) {
	///              long id = batch.getStart() + i;
	///              nodes[i] = new InputNode( .... );
	///          }
	///          return nodes;
	///      },
	///      batch -> {
	///          InputRelationship[] relationships = new InputRelationship[batch.getSize()];
	///          ....
	///          return relationships;
	///      } );
	/// }
	/// </pre>
	/// </summary>
	public class DataGeneratorInput : Input
	{
		 private readonly long _nodes;
		 private readonly long _relationships;
		 private readonly IdType _idType;
		 private readonly Collector _badCollector;
		 private readonly long _seed;
		 private readonly Header _nodeHeader;
		 private readonly Header _relationshipHeader;
		 private readonly Distribution<string> _labels;
		 private readonly Distribution<string> _relationshipTypes;
		 private readonly float _factorBadNodeData;
		 private readonly float _factorBadRelationshipData;
		 private readonly long _startId;
		 private readonly Groups _groups = new Groups();

		 public DataGeneratorInput( long nodes, long relationships, IdType idType, Collector badCollector, long seed, long startId, Header nodeHeader, Header relationshipHeader, int labelCount, int relationshipTypeCount, float factorBadNodeData, float factorBadRelationshipData )
		 {
			  this._nodes = nodes;
			  this._relationships = relationships;
			  this._idType = idType;
			  this._badCollector = badCollector;
			  this._seed = seed;
			  this._startId = startId;
			  this._nodeHeader = nodeHeader;
			  this._relationshipHeader = relationshipHeader;
			  this._factorBadNodeData = factorBadNodeData;
			  this._factorBadRelationshipData = factorBadRelationshipData;
			  this._labels = new Distribution<string>( Tokens( "Label", labelCount ) );
			  this._relationshipTypes = new Distribution<string>( Tokens( "TYPE", relationshipTypeCount ) );
		 }

		 public override InputIterable Nodes()
		 {
			  return () => new RandomEntityDataGenerator(_nodes, _nodes, 10_000, _seed, _startId, _nodeHeader, _labels, _relationshipTypes, _factorBadNodeData, _factorBadRelationshipData);
		 }

		 public override InputIterable Relationships()
		 {
			  return () => new RandomEntityDataGenerator(_nodes, _relationships, 10_000, _seed, _startId, _relationshipHeader, _labels, _relationshipTypes, _factorBadNodeData, _factorBadRelationshipData);
		 }

		 public override IdMapper IdMapper( NumberArrayFactory numberArrayFactory )
		 {
			  return _idType.idMapper( numberArrayFactory, _groups );
		 }

		 public override Collector BadCollector()
		 {
			  return _badCollector;
		 }

		 public override Input_Estimates CalculateEstimates( System.Func<Value[], int> valueSizeCalculator )
		 {
			  int sampleSize = 100;
			  InputEntity[] nodeSample = Sample( Nodes(), sampleSize );
			  double labelsPerNodeEstimate = SampleLabels( nodeSample );
			  double[] nodePropertyEstimate = SampleProperties( nodeSample, valueSizeCalculator );
			  double[] relationshipPropertyEstimate = SampleProperties( Sample( Relationships(), sampleSize ), valueSizeCalculator );
			  return Inputs.KnownEstimates( _nodes, _relationships, ( long )( _nodes * nodePropertyEstimate[0] ), ( long )( _relationships * relationshipPropertyEstimate[0] ), ( long )( _nodes * nodePropertyEstimate[1] ), ( long )( _relationships * relationshipPropertyEstimate[1] ), ( long )( _nodes * labelsPerNodeEstimate ) );
		 }

		 private InputEntity[] Sample( InputIterable source, int size )
		 {
			  try
			  {
					  using ( InputIterator iterator = source.GetEnumerator(), InputChunk chunk = iterator.NewChunk() )
					  {
						InputEntity[] sample = new InputEntity[size];
						int cursor = 0;
						while ( cursor < size && iterator.Next( chunk ) )
						{
							 while ( cursor < size && chunk.Next( sample[cursor++] = new InputEntity() ) )
							 {
								  // just loop
							 }
						}
						return sample;
					  }
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private static double SampleLabels( InputEntity[] nodes )
		 {
			  int labels = 0;
			  foreach ( InputEntity node in nodes )
			  {
					labels += node.Labels().Length;
			  }
			  return ( double ) labels / nodes.Length;
		 }

		 private static double[] SampleProperties( InputEntity[] sample, System.Func<Value[], int> valueSizeCalculator )
		 {
			  int propertiesPerEntity = sample[0].PropertyCount();
			  long propertiesSize = 0;
			  foreach ( InputEntity IEntity in sample )
			  {
					propertiesSize += Inputs.CalculatePropertySize( IEntity, valueSizeCalculator );
			  }
			  double propertySizePerEntity = ( double ) propertiesSize / sample.Length;
			  return new double[] { propertiesPerEntity, propertySizePerEntity };
		 }

		 public static Header SillyNodeHeader( IdType idType, Extractors extractors )
		 {
			  return new Header( new Header.Entry( null, Type.ID, null, idType.extractor( extractors ) ), new Header.Entry( "name", Type.PROPERTY, null, extractors.String() ), new Header.Entry("age", Type.PROPERTY, null, extractors.Int_()), new Header.Entry("something", Type.PROPERTY, null, extractors.String()), new Header.Entry(null, Type.LABEL, null, extractors.StringArray()) );
		 }

		 public static Header BareboneNodeHeader( IdType idType, Extractors extractors )
		 {
			  return BareboneNodeHeader( null, idType, extractors );
		 }

		 public static Header BareboneNodeHeader( string idKey, IdType idType, Extractors extractors, params Header.Entry[] additionalEntries )
		 {
			  IList<Header.Entry> entries = new List<Header.Entry>();
			  entries.Add( new Header.Entry( idKey, Type.ID, null, idType.extractor( extractors ) ) );
			  entries.Add( new Header.Entry( null, Type.LABEL, null, extractors.StringArray() ) );
			  ( ( IList<Header.Entry> )entries ).AddRange( asList( additionalEntries ) );
			  return new Header( entries.ToArray() );
		 }

		 public static Header BareboneRelationshipHeader( IdType idType, Extractors extractors, params Header.Entry[] additionalEntries )
		 {
			  IList<Header.Entry> entries = new List<Header.Entry>();
			  entries.Add( new Header.Entry( null, Type.START_ID, null, idType.extractor( extractors ) ) );
			  entries.Add( new Header.Entry( null, Type.END_ID, null, idType.extractor( extractors ) ) );
			  entries.Add( new Header.Entry( null, Type.TYPE, null, extractors.String() ) );
			  ( ( IList<Header.Entry> )entries ).AddRange( asList( additionalEntries ) );
			  return new Header( entries.ToArray() );
		 }

		 private static string[] Tokens( string prefix, int count )
		 {
			  string[] result = new string[count];
			  for ( int i = 0; i < count; i++ )
			  {
					result[i] = prefix + ( i + 1 );
			  }
			  return result;
		 }
	}

}