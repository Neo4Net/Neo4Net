﻿using System;
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
namespace Org.Neo4j.Server.rest.domain
{

	using Org.Neo4j.Graphdb;
	using PathExpanderBuilder = Org.Neo4j.Graphdb.PathExpanderBuilder;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;

	public class RelationshipExpanderBuilder
	{
		 private RelationshipExpanderBuilder()
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static org.neo4j.graphdb.PathExpander describeRelationships(java.util.Map<String, Object> description)
		 public static PathExpander DescribeRelationships( IDictionary<string, object> description )
		 {
			  PathExpanderBuilder expander = PathExpanderBuilder.allTypesAndDirections();

			  object relationshipsDescription = description["relationships"];
			  if ( relationshipsDescription != null )
			  {
					ICollection<object> pairDescriptions;
					if ( relationshipsDescription is System.Collections.ICollection )
					{
						 pairDescriptions = ( ICollection<object> ) relationshipsDescription;
					}
					else
					{
						 pairDescriptions = Arrays.asList( relationshipsDescription );
					}

					foreach ( object pairDescription in pairDescriptions )
					{
						 System.Collections.IDictionary map = ( System.Collections.IDictionary ) pairDescription;
						 string name = ( string ) map["type"];
						 RelationshipType type = RelationshipType.withName( name );
						 string directionName = ( string ) map["direction"];
						 expander = ( string.ReferenceEquals( directionName, null ) ) ? expander.Add( type ) : expander.Add( type, StringToEnum( directionName, typeof( RelationshipDirection ), true ).@internal );
					}
			  }
			  return expander.Build();
		 }

		 // TODO Refactor - same method exists in TraversalDescriptionBuilder
		 private static T StringToEnum<T>( string name, Type enumClass, bool fuzzyMatch ) where T : Enum<T>
		 {
				 enumClass = typeof( T );
			  if ( string.ReferenceEquals( name, null ) )
			  {
					return default( T );
			  }

			  // name = enumifyName( name );
			  foreach ( T candidate in enumClass.EnumConstants )
			  {
					if ( candidate.name().Equals(name) )
					{
						 return candidate;
					}
			  }
			  if ( fuzzyMatch )
			  {
					foreach ( T candidate in enumClass.EnumConstants )
					{
						 if ( candidate.name().StartsWith(name) )
						 {
							  return candidate;
						 }
					}
			  }
			  throw new Exception( "Unrecognized " + enumClass.Name + " '" + name + "'" );
		 }
	}

}