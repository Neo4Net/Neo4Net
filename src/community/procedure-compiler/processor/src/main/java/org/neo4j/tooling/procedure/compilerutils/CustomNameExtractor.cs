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
namespace Neo4Net.Tooling.procedure.compilerutils
{

	using Procedure = Neo4Net.Procedure.Procedure;
	using UserFunction = Neo4Net.Procedure.UserFunction;
	using UserAggregationFunction = Neo4Net.Procedure.UserAggregationFunction;

	public class CustomNameExtractor
	{
		 private CustomNameExtractor()
		 {

		 }

		 /// <summary>
		 /// Extracts user-defined names, usually from a <seealso cref="Procedure"/>, <seealso cref="UserFunction"/>
		 /// or <seealso cref="UserAggregationFunction"/>.
		 /// 
		 /// As such, extracted strings are assumed to be non-null.
		 /// </summary>
		 public static Optional<string> GetName( System.Func<string> nameSupplier, System.Func<string> valueSupplier )
		 {
			  string name = nameSupplier().Trim();
			  if ( name.Length > 0 )
			  {
					return name;
			  }
			  string value = valueSupplier().Trim();
			  if ( value.Length > 0 )
			  {
					return value;
			  }
			  return null;
		 }
	}

}