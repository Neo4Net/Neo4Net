﻿using System.Diagnostics;

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
namespace Org.Neo4j.Cypher.@internal.javacompat
{
	using QueryResult = Org.Neo4j.Cypher.result.QueryResult;
	using AnyValue = Org.Neo4j.Values.AnyValue;

	public class ResultRecord : Org.Neo4j.Cypher.result.QueryResult_Record
	{
		 private readonly AnyValue[] _fields;

		 //NOTE do not remove, used from generated code
		 public ResultRecord( int size )
		 {
			  this._fields = new AnyValue[size];
		 }

		 public ResultRecord( AnyValue[] fields )
		 {
			  this._fields = fields;
		 }

		 public virtual void Set( int i, AnyValue value )
		 {
			  Debug.Assert( value != null );
			  Debug.Assert( i >= 0 && i < _fields.Length );

			  _fields[i] = value;
		 }

		 public override AnyValue[] Fields()
		 {
			  return _fields;
		 }
	}

}