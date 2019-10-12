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
namespace Org.Neo4j.Codegen
{
	public class LocalVariable : Expression
	{
		 private readonly string _name;
		 private readonly int _index;

		 internal LocalVariable( TypeReference type, string name, int index ) : base( type )
		 {
			  this._name = name;
			  this._index = index;
		 }

		 public virtual string Name()
		 {
			  return _name;
		 }

		 public virtual int Index()
		 {
			  return _index;
		 }

		 public override void Accept( ExpressionVisitor visitor )
		 {
			  visitor.Load( this );
		 }
	}

}