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
namespace Org.Neo4j.Values.@virtual
{

	using Org.Neo4j.Values;
	using Org.Neo4j.Values;
	using InvalidValuesArgumentException = Org.Neo4j.Values.utils.InvalidValuesArgumentException;

	/// <summary>
	/// The ErrorValue allow delaying errors in value creation until runtime, which is useful
	/// if it turns out that the value is never used.
	/// </summary>
	public sealed class ErrorValue : VirtualValue
	{
		 private readonly InvalidValuesArgumentException _e;

		 internal ErrorValue( Exception e )
		 {
			  this._e = new InvalidValuesArgumentException( e.Message );
		 }

		 public override bool Equals( VirtualValue other )
		 {
			  throw _e;
		 }

		 public override VirtualValueGroup ValueGroup()
		 {
			  return VirtualValueGroup.Error;
		 }

		 public override int CompareTo( VirtualValue other, IComparer<AnyValue> comparator )
		 {
			  throw _e;
		 }

		 protected internal override int ComputeHash()
		 {
			  throw _e;
		 }

		 public override void WriteTo<E>( AnyValueWriter<E> writer ) where E : Exception
		 {
			  throw _e;
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  throw _e;
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "Error";
			 }
		 }
	}

}