using System;

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
namespace Neo4Net.Server.plugins
{
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using BadInputException = Neo4Net.Server.rest.repr.BadInputException;

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
//ORIGINAL LINE: Object extract(org.Neo4Net.kernel.internal.GraphDatabaseAPI graphDb, Object source, ParameterList parameters) throws org.Neo4Net.server.rest.repr.BadInputException
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