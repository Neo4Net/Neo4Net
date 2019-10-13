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
namespace Neo4Net.Kernel.impl.proc
{

	using DefaultParameterValue = Neo4Net.@internal.Kernel.Api.procs.DefaultParameterValue;
	using Neo4jTypes = Neo4Net.@internal.Kernel.Api.procs.Neo4jTypes;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.DefaultParameterValue.ntList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.proc.ParseUtil.parseList;


	public class ListConverter : System.Func<string, DefaultParameterValue>
	{
		 private readonly Type _type;
		 private readonly Neo4jTypes.AnyType _neoType;

		 public ListConverter( Type type, Neo4jTypes.AnyType neoType )
		 {
			  this._type = type;
			  this._neoType = neoType;
		 }

		 public override DefaultParameterValue Apply( string s )
		 {
			  string value = s.Trim();
			  if ( value.Equals( "null", StringComparison.OrdinalIgnoreCase ) )
			  {
					return ntList( null, _neoType );
			  }
			  else
			  {
					return ntList( parseList( value, _type ), _neoType );
			  }
		 }
	}

}