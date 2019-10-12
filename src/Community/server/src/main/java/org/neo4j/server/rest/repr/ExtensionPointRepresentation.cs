using System;
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
namespace Neo4Net.Server.rest.repr
{

	using ParameterDescriptionConsumer = Neo4Net.Server.plugins.ParameterDescriptionConsumer;

	public sealed class ExtensionPointRepresentation : ObjectRepresentation, ParameterDescriptionConsumer
	{
		 private readonly RepresentationType _extended;
		 private readonly string _name;
		 private readonly string _description;
		 private readonly IList<ParameterRepresentation> _parameters = new List<ParameterRepresentation>();

		 public ExtensionPointRepresentation( string name, Type extended, string description ) : base( RepresentationType.PluginDescription )
		 {
			  this._name = name;
			  this._description = description;
			  this._extended = RepresentationType.Extended( extended );
		 }

		 public override void DescribeParameter( string name, Type type, bool optional, string description )
		 {
			  _parameters.Add( new ParameterRepresentation( name, type, optional, description, false ) );
		 }

		 public override void DescribeListParameter( string name, Type type, bool optional, string description )
		 {
			  _parameters.Add( new ParameterRepresentation( name, type, optional, description, true ) );
		 }

		 public string Name
		 {
			 get
			 {
				  return _name;
			 }
		 }

		 public string ExtendedEntity
		 {
			 get
			 {
				  return _extended.valueName;
			 }
		 }

		 [Mapping("name")]
		 public ValueRepresentation MethodName()
		 {
			  return ValueRepresentation.String( _name );
		 }

		 [Mapping("description")]
		 public ValueRepresentation Description()
		 {
			  return ValueRepresentation.String( _description );
		 }

		 [Mapping("extends")]
		 public ValueRepresentation ExtendedEntity()
		 {
			  return ValueRepresentation.String( ExtendedEntity );
		 }

		 [Mapping("parameters")]
		 public ListRepresentation ParametersList()
		 {
			  return new ListRepresentation( RepresentationType.PluginParameter, _parameters );
		 }

		 private class ParameterRepresentation : MappingRepresentation
		 {
			  internal readonly string Name;
			  internal readonly RepresentationType ParamType;
			  internal readonly string Description;
			  internal readonly bool Optional;
			  internal readonly bool List;

			  internal ParameterRepresentation( string name, Type type, bool optional, string description, bool list ) : base( RepresentationType.PluginParameter )
			  {
					this.Name = name;
					this.Optional = optional;
					this.List = list;
					this.ParamType = RepresentationType.Extended( type );
					this.Description = description;
			  }

			  protected internal override void Serialize( MappingSerializer serializer )
			  {
					serializer.PutString( "name", Name );
					serializer.PutString( "type", List ? ParamType.listName : ParamType.valueName );
					serializer.PutBoolean( "optional", Optional );
					serializer.PutString( "description", Description );
			  }
		 }
	}

}