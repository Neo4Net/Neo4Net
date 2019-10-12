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
namespace Org.Neo4j.Server.rest.repr
{

	using IndexPopulationProgress = Org.Neo4j.Graphdb.index.IndexPopulationProgress;
	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;
	using Schema = Org.Neo4j.Graphdb.schema.Schema;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.single;

	public class IndexDefinitionRepresentation : MappingRepresentation
	{
		 private readonly IndexDefinition _indexDefinition;
		 private readonly IndexPopulationProgress _indexPopulationProgress;
		 private readonly Org.Neo4j.Graphdb.schema.Schema_IndexState _indexState;

		 public IndexDefinitionRepresentation( IndexDefinition indexDefinition ) : this( indexDefinition, org.neo4j.graphdb.schema.Schema_IndexState.Online, IndexPopulationProgress.DONE )
		 {
			  // Online state will mean progress is ignored
		 }

		 public IndexDefinitionRepresentation( IndexDefinition indexDefinition, Org.Neo4j.Graphdb.schema.Schema_IndexState indexState, IndexPopulationProgress indexPopulationProgress ) : base( RepresentationType.IndexDefinition )
		 {
			  this._indexDefinition = indexDefinition;
			  this._indexPopulationProgress = indexPopulationProgress;
			  this._indexState = indexState;
		 }

		 protected internal override void Serialize( MappingSerializer serializer )
		 {
			  if ( _indexDefinition.NodeIndex )
			  {
					serializer.PutList( "labels", new ListRepresentation( RepresentationType.String, map( label => ValueRepresentation.String( label.name() ), _indexDefinition.Labels ) ) );
					if ( !_indexDefinition.MultiTokenIndex )
					{
						 serializer.PutString( "label", single( _indexDefinition.Labels ).name() );
					}
			  }
			  else
			  {
					serializer.PutList( "relationshipTypes", new ListRepresentation( RepresentationType.String, map( relType => ValueRepresentation.String( relType.name() ), _indexDefinition.RelationshipTypes ) ) );
					if ( !_indexDefinition.MultiTokenIndex )
					{
						 serializer.PutString( "relationshipType", single( _indexDefinition.RelationshipTypes ).name() );
					}
			  }

			  System.Func<string, Representation> converter = ValueRepresentation.@string;
			  IEnumerable<Representation> propertyKeyRepresentations = map( converter, _indexDefinition.PropertyKeys );
			  serializer.PutList( "property_keys", new ListRepresentation( RepresentationType.String, propertyKeyRepresentations ) );
			  // Only print state and progress if progress is a valid value and not yet online
			  if ( _indexState == Org.Neo4j.Graphdb.schema.Schema_IndexState.Populating )
			  {
					serializer.PutString( "state", _indexState.name() );
					serializer.PutString( "population_progress", string.Format( "{0,1:F0}%", _indexPopulationProgress.CompletedPercentage ) );
			  }
		 }
	}

}