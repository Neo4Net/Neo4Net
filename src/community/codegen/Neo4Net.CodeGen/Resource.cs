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
namespace Neo4Net.CodeGen
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.codegen.TypeReference.typeReference;

	public class Resource
	{
		 public static Resource WithResource( Type type, string name, Expression producer )
		 {
			  return WithResource( typeReference( type ), name, producer );
		 }

		 public static Resource WithResource( TypeReference type, string name, Expression producer )
		 {
			  return new Resource( type, name, producer );
		 }

		 private readonly TypeReference _type;
		 private readonly string _name;
		 private readonly Expression _producer;

		 private Resource( TypeReference type, string name, Expression producer )
		 {
			  this._type = type;
			  this._name = name;
			  this._producer = producer;
		 }

		 public virtual TypeReference Type()
		 {
			  return _type;
		 }

		 public virtual string Name()
		 {
			  return _name;
		 }

		 public virtual Expression Producer()
		 {
			  return _producer;
		 }
	}

}