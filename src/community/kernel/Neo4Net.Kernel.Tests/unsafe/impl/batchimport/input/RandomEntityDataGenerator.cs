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

	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;
	using Neo4Net.@unsafe.Impl.Batchimport;
	using Neo4Net.@unsafe.Impl.Batchimport.input.csv;
	using Header = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Header;
	using Entry = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Header.Entry;
	using Type = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Type;
	using RandomValues = Neo4Net.Values.Storable.RandomValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.InputEntity.NO_LABELS;

	/// <summary>
	/// Data generator as <seealso cref="InputIterator"/>, parallelizable
	/// </summary>
	public class RandomEntityDataGenerator : GeneratingInputIterator<RandomValues>
	{
		 public RandomEntityDataGenerator( long nodeCount, long count, int batchSize, long seed, long startId, Header header, Distribution<string> labels, Distribution<string> relationshipTypes, float factorBadNodeData, float factorBadRelationshipData ) : base(count, batchSize, new RandomsStates(seed), (randoms, visitor, id) ->
		 {
		  {
			  foreach ( Entry entry in header.Entries() )
			  {
				  switch ( entry.type() )
				  {
					  case ID:
						  if ( factorBadNodeData > 0 && id > 0 )
						  {
							  if ( randoms.nextFloat() <= factorBadNodeData )
							  {
								  id = randoms.nextLong( id );
							  }
						  }
						  visitor.id( IdValue( entry, id ), entry.group() );
						  if ( entry.name() != null )
						  {
							  visitor.property( entry.name(), id );
						  }
						  break;
					  case PROPERTY:
						  visitor.property( entry.name(), RandomProperty(entry, randoms) );
						  break;
					  case LABEL:
						  visitor.labels( RandomLabels( randoms, labels ) );
						  break;
					  case START_ID:
				  case END_ID:
					  long nodeId = randoms.nextLong( nodeCount );
					  if ( factorBadRelationshipData > 0 && nodeId > 0 )
					  {
						  if ( randoms.nextFloat() <= factorBadRelationshipData )
						  {
							  if ( randoms.nextBoolean() )
							  {
								  break;
							  }
							  nodeId = randoms.nextLong();
						  }
					  }
					  if ( entry.type() == Type.START_ID )
					  {
						  visitor.startId( IdValue( entry, nodeId ), entry.group() );
					  }
					  else
					  {
						  visitor.endId( IdValue( entry, nodeId ), entry.group() );
					  }
					  break;
				  case TYPE:
					  visitor.type( RandomRelationshipType( randoms, relationshipTypes ) );
					  break;
				  default:
					  throw new System.ArgumentException( entry.ToString() );
				  }
			  }
		  }, startId);
		 }

		 private static object IdValue( Header.Entry entry, long id )
		 {
			  switch ( entry.Extractor().name() )
			  {
			  case "String":
				  return "" + id;
			  case "long":
				  return id;
			  default:
				  throw new System.ArgumentException( entry.Name() );
			  }
		 }

		 private static string RandomRelationshipType( RandomValues random, Distribution<string> relationshipTypes )
		 {
			  return relationshipTypes.Random( random );
		 }

		 private static object RandomProperty( Header.Entry entry, RandomValues random )
		 {
			  string type = entry.Extractor().name();
			  switch ( type )
			  {
			  case "String":
					return random.NextAlphaNumericTextValue( 5, 20 ).stringValue();
			  case "long":
					return random.Next( int.MaxValue );
			  case "int":
					return random.Next( 20 );
			  default:
					throw new System.ArgumentException( "" + entry );
			  }
		 }

		 private static string[] RandomLabels( RandomValues random, Distribution<string> labels )
		 {
			  if ( labels.Length() == 0 )
			  {
					return NO_LABELS;
			  }
			  int length = random.Next( min( 3, labels.Length() ) ) + 1;

			  string[] result = new string[length];
			  for ( int i = 0; i < result.Length; )
			  {
					string candidate = labels.Random( random );
					if ( !ArrayUtil.contains( result, i, candidate ) )
					{
						 result[i++] = candidate;
					}
			  }
			  return result;
		 }

		 /// <summary>
		 /// Test utility method for converting an <seealso cref="InputEntity"/> into another representation.
		 /// </summary>
		 /// <param name="entity"> <seealso cref="InputEntity"/> filled with data. </param>
		 /// <param name="deserialization"> <seealso cref="Deserialization"/>. </param>
		 /// <param name="header"> <seealso cref="Header"/> to deserialize from. </param>
		 /// <returns> data from <seealso cref="InputEntity"/> converted into something else. </returns>
		 public static T Convert<T>( InputEntity IEntity, Deserialization<T> deserialization, Header header )
		 {
			  deserialization.Clear();
			  foreach ( Header.Entry entry in header.Entries() )
			  {
					switch ( entry.Type() )
					{
					case ID:
						 deserialization.Handle( entry, IEntity.HasLongId ? IEntity.LongId : IEntity.ObjectId );
						 break;
					case PROPERTY:
						 deserialization.Handle( entry, Property( IEntity.PropertiesConflict, entry.Name() ) );
						 break;
					case LABEL:
						 deserialization.Handle( entry, IEntity.Labels() );
						 break;
					case TYPE:
						 deserialization.Handle( entry, IEntity.HasIntType ? IEntity.IntType : IEntity.StringType );
						 break;
					case START_ID:
						 deserialization.Handle( entry, IEntity.HasLongStartId ? IEntity.LongStartId : IEntity.ObjectStartId );
						 break;
					case END_ID:
						 deserialization.Handle( entry, IEntity.HasLongEndId ? IEntity.LongEndId : IEntity.ObjectEndId );
						 break;
					default: // ignore other types
				break;
					}
			  }
			  return deserialization.Materialize();
		 }

		 private static object Property( IList<object> properties, string key )
		 {
			  for ( int i = 0; i < properties.Count; i += 2 )
			  {
					if ( properties[i].Equals( key ) )
					{
						 return properties[i + 1];
					}
			  }
			  return null;
		 }
	}

}