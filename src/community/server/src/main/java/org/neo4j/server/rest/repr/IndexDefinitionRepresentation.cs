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
namespace Neo4Net.Server.rest.repr
{

	using IndexPopulationProgress = Neo4Net.GraphDb.Index.IndexPopulationProgress;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using Schema = Neo4Net.GraphDb.Schema.Schema;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.single;

	public class IndexDefinitionRepresentation : MappingRepresentation
	{
		 private readonly IndexDefinition _indexDefinition;
		 private readonly IndexPopulationProgress _indexPopulationProgress;
		 private readonly Neo4Net.GraphDb.Schema.Schema_IndexState _indexState;

		 public IndexDefinitionRepresentation( IndexDefinition indexDefinition ) : this( indexDefinition, Neo4Net.GraphDb.Schema.Schema_IndexState.Online, IndexPopulationProgress.DONE )
		 {
			  // Online state will mean progress is ignored
		 }

		 public IndexDefinitionRepresentation( IndexDefinition indexDefinition, Neo4Net.GraphDb.Schema.Schema_IndexState indexState, IndexPopulationProgress indexPopulationProgress ) : base( RepresentationType.IndexDefinition )
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
			  if ( _indexState == Neo4Net.GraphDb.Schema.Schema_IndexState.Populating )
			  {
					serializer.PutString( "state", _indexState.name() );
					serializer.PutString( "population_progress", string.Format( "{0,1:F0}%", _indexPopulationProgress.CompletedPercentage ) );
			  }
		 }
	}

}