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
namespace Neo4Net.@unsafe.Impl.Batchimport.input
{

	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using NumberArrayFactory = Neo4Net.@unsafe.Impl.Batchimport.cache.NumberArrayFactory;
	using IdMapper = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.IdMapper;
	using Value = Neo4Net.Values.Storable.Value;

	public class Inputs
	{
		 private Inputs()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Input input(final org.neo4j.unsafe.impl.batchimport.InputIterable nodes, final org.neo4j.unsafe.impl.batchimport.InputIterable relationships, final org.neo4j.unsafe.impl.batchimport.cache.idmapping.IdMapper idMapper, final Collector badCollector, org.neo4j.unsafe.impl.batchimport.input.Input_Estimates estimates)
		 public static Input Input( InputIterable nodes, InputIterable relationships, IdMapper idMapper, Collector badCollector, Input_Estimates estimates )
		 {
			  return new InputAnonymousInnerClass( nodes, relationships, idMapper, badCollector, estimates );
		 }

		 private class InputAnonymousInnerClass : Input
		 {
			 private InputIterable _nodes;
			 private InputIterable _relationships;
			 private IdMapper _idMapper;
			 private Neo4Net.@unsafe.Impl.Batchimport.input.Collector _badCollector;
			 private Input_Estimates _estimates;

			 public InputAnonymousInnerClass( InputIterable nodes, InputIterable relationships, IdMapper idMapper, Neo4Net.@unsafe.Impl.Batchimport.input.Collector badCollector, Input_Estimates estimates )
			 {
				 this._nodes = nodes;
				 this._relationships = relationships;
				 this._idMapper = idMapper;
				 this._badCollector = badCollector;
				 this._estimates = estimates;
			 }

			 public InputIterable relationships()
			 {
				  return _relationships;
			 }

			 public InputIterable nodes()
			 {
				  return _nodes;
			 }

			 public IdMapper idMapper( NumberArrayFactory numberArrayFactory )
			 {
				  return _idMapper;
			 }

			 public Collector badCollector()
			 {
				  return _badCollector;
			 }

			 public Input_Estimates calculateEstimates( System.Func<Value[], int> valueSizeCalculator )
			 {
				  return _estimates;
			 }
		 }

		 public static Input_Estimates KnownEstimates( long numberOfNodes, long numberOfRelationships, long numberOfNodeProperties, long numberOfRelationshipProperties, long nodePropertiesSize, long relationshipPropertiesSize, long numberOfNodeLabels )
		 {
			  return new Input_EstimatesAnonymousInnerClass( numberOfNodes, numberOfRelationships, numberOfNodeProperties, numberOfRelationshipProperties, nodePropertiesSize, relationshipPropertiesSize, numberOfNodeLabels );
		 }

		 private class Input_EstimatesAnonymousInnerClass : Input_Estimates
		 {
			 private long _numberOfNodes;
			 private long _numberOfRelationships;
			 private long _numberOfNodeProperties;
			 private long _numberOfRelationshipProperties;
			 private long _nodePropertiesSize;
			 private long _relationshipPropertiesSize;
			 private long _numberOfNodeLabels;

			 public Input_EstimatesAnonymousInnerClass( long numberOfNodes, long numberOfRelationships, long numberOfNodeProperties, long numberOfRelationshipProperties, long nodePropertiesSize, long relationshipPropertiesSize, long numberOfNodeLabels )
			 {
				 this._numberOfNodes = numberOfNodes;
				 this._numberOfRelationships = numberOfRelationships;
				 this._numberOfNodeProperties = numberOfNodeProperties;
				 this._numberOfRelationshipProperties = numberOfRelationshipProperties;
				 this._nodePropertiesSize = nodePropertiesSize;
				 this._relationshipPropertiesSize = relationshipPropertiesSize;
				 this._numberOfNodeLabels = numberOfNodeLabels;
			 }

			 public long numberOfNodes()
			 {
				  return _numberOfNodes;
			 }

			 public long numberOfRelationships()
			 {
				  return _numberOfRelationships;
			 }

			 public long numberOfNodeProperties()
			 {
				  return _numberOfNodeProperties;
			 }

			 public long sizeOfNodeProperties()
			 {
				  return _nodePropertiesSize;
			 }

			 public long numberOfNodeLabels()
			 {
				  return _numberOfNodeLabels;
			 }

			 public long numberOfRelationshipProperties()
			 {
				  return _numberOfRelationshipProperties;
			 }

			 public long sizeOfRelationshipProperties()
			 {
				  return _relationshipPropertiesSize;
			 }
		 }

		 public static int CalculatePropertySize( InputEntity entity, System.Func<Value[], int> valueSizeCalculator )
		 {
			  int size = 0;
			  int propertyCount = entity.PropertyCount();
			  if ( propertyCount > 0 )
			  {
					Value[] values = new Value[propertyCount];
					for ( int i = 0; i < propertyCount; i++ )
					{
						 values[i] = ValueUtils.asValue( entity.PropertyValue( i ) );
					}
					size += valueSizeCalculator( values );
			  }
			  return size;
		 }
	}

}