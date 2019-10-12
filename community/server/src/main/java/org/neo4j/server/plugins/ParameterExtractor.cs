using System;

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
namespace Org.Neo4j.Server.plugins
{
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using BadInputException = Org.Neo4j.Server.rest.repr.BadInputException;

	internal class ParameterExtractor : DataExtractor
	{
		 internal readonly string Name;
		 internal readonly Type Type;
		 internal readonly bool Optional;
		 internal readonly string Description;
		 internal readonly TypeCaster Caster;

		 internal ParameterExtractor( TypeCaster caster, Type type, Parameter param, Description description )
		 {
			  this.Caster = caster;
			  this.Type = type;
			  this.Name = param.name();
			  this.Optional = param.optional();
			  this.Description = description == null ? "" : description.value();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Object extract(org.neo4j.kernel.internal.GraphDatabaseAPI graphDb, Object source, ParameterList parameters) throws org.neo4j.server.rest.repr.BadInputException
		 internal override object Extract( GraphDatabaseAPI graphDb, object source, ParameterList parameters )
		 {
			  object result = Caster.get( graphDb, parameters, Name );
			  if ( Optional || result != null )
			  {
					return result;
			  }
			  throw new System.ArgumentException( "Mandatory argument \"" + Name + "\" not supplied." );
		 }

		 internal override void Describe( ParameterDescriptionConsumer consumer )
		 {
			  consumer.DescribeParameter( Name, Type, Optional, Description );
		 }
	}

}