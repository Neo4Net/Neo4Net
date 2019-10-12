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
namespace Org.Neo4j.Kernel.builtinprocs
{

	public class RelationshipPropertySchemaInfoResult
	{
		 /// <summary>
		 /// A relationship type
		 /// </summary>
		 public readonly string RelType;

		 /// <summary>
		 /// A property name that occurs on the given relationship type or null
		 /// </summary>
		 public readonly string PropertyName;

		 /// <summary>
		 /// A List containing all types of the given property on the given relationship type or null
		 /// </summary>
		 public readonly IList<string> PropertyTypes;

		 /// <summary>
		 /// Indicates whether the property is present on all similar relationships (= true) or not (= false)
		 /// </summary>
		 public readonly bool Mandatory;

		 public RelationshipPropertySchemaInfoResult( string relType, string propertyName, IList<string> cypherTypes, bool mandatory )
		 {
			  this.RelType = relType;
			  this.PropertyName = propertyName;
			  this.PropertyTypes = cypherTypes;
			  this.Mandatory = mandatory;
		 }
	}

}